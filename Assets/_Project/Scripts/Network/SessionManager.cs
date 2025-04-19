using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Shared;
using _Project.Scripts.Shared.Sessions.Data;
using _Project.Scripts.Shared.Sessions.Events;
using _Project.Scripts.Shared.TeamSelection.Events;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Network
{
    public class SessionManager : MonoBehaviour
    {
        private Lobby _activeLobby;
        private string _relayJoinCode;
        private Coroutine _heartbeatCoroutine;
        public static SessionManager instance;

        public Lobby ActiveLobby {
            get => _activeLobby;
            private set {
                _activeLobby = value;
                Debug.Log($"Active Lobby: {_activeLobby?.Id ?? "None"}");
            }
        }

        [SerializeField] private NetworkObject teamSync;

        [Header("Events")]
        [SerializeField] private SessionCreateRequestedEvent createRequestedEvent;
        [SerializeField] private SessionJoinRequestedEvent joinRequestedEvent;
        [SerializeField] private SessionRefreshRequestedEvent refreshRequestedEvent;
        [SerializeField] private SessionListUpdatedEvent sessionListUpdatedEvent;
        [SerializeField] private SessionStartedEvent sessionStartedEvent;
        [SerializeField] private TeamsReadyEvent teamsReadyEvent;

        private const string PlayerNameKey = "playerName";
        private const string RelayJoinCodeKey = "RelayJoinCode";

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }

        private void OnEnable() {
            createRequestedEvent.Register(HandleCreateSession);
            joinRequestedEvent.Register(HandleJoinSession);
            refreshRequestedEvent.Register(HandleRefreshSessions);
            teamsReadyEvent.Register(OnTeamsReady);
        }

        private void OnDisable() {
            createRequestedEvent.Unregister(HandleCreateSession);
            joinRequestedEvent.Unregister(HandleJoinSession);
            refreshRequestedEvent.Unregister(HandleRefreshSessions);
            teamsReadyEvent.Unregister(OnTeamsReady);
            StopLobbyHeartbeat();
        }

        private async void HandleCreateSession(string sessionName) {
            await CreateLobbyAndStartHost(sessionName);
        }

        private async void HandleJoinSession(string lobbyId) {
            await JoinLobbyAndStartClient(lobbyId);
        }

        private async void HandleRefreshSessions() {
            await QueryAvailableLobbies();
        }

        private async Task CreateLobbyAndStartHost(string lobbyName) {
            try {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
                _relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                CreateLobbyOptions options = new CreateLobbyOptions {
                    IsPrivate = false,
                    Player = new Player(AuthenticationService.Instance.PlayerId, data:new Dictionary<string, PlayerDataObject> {
                        { PlayerNameKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId) }
                    }),
                    Data = new Dictionary<string, DataObject> {
                        { RelayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, _relayJoinCode) }
                    }
                };

                ActiveLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, 4, options);
                Debug.Log($"Lobby created! Lobby ID: {ActiveLobby.Id}, Relay Join Code: {_relayJoinCode}");

                StartLobbyHeartbeat();

                var unityTransport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

                NetworkManager.Singleton.StartHost();
                sessionStartedEvent?.Raise();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        private async Task JoinLobbyAndStartClient(string lobbyId) {
            try {
                var lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);
                ActiveLobby = lobby;

                string relayJoinCode = lobby.Data[RelayJoinCodeKey].Value;
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

                var unityTransport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                unityTransport.SetClientRelayData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

                NetworkManager.Singleton.StartClient();
                sessionStartedEvent?.Raise();

                Debug.Log($"Joined Lobby {lobby.Id} with Relay Join Code {relayJoinCode}");
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        private async Task QueryAvailableLobbies() {
            try {
                var result = await Lobbies.Instance.QueryLobbiesAsync();
                var sessionsData = new List<SessionData>();

                foreach (var lobby in result.Results) {
                    int joinedPlayers = lobby.Players.Count;
                    var sessionData = new SessionData(lobby.Id, lobby.Name, joinedPlayers, lobby.IsLocked, lobby.MaxPlayers);
                    sessionsData.Add(sessionData);
                }

                sessionListUpdatedEvent?.Raise(sessionsData);
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public async Task LeaveLobby() {
            StopLobbyHeartbeat();

            if (ActiveLobby != null) {
                try {
                    await Lobbies.Instance.RemovePlayerAsync(ActiveLobby.Id, AuthenticationService.Instance.PlayerId);
                }
                catch (Exception e) {
                    Debug.LogWarning($"Failed to leave lobby: {e}");
                }
                finally {
                    ActiveLobby = null;
                }
            }
        }

        private void StartLobbyHeartbeat() {
            if (_heartbeatCoroutine != null)
                StopCoroutine(_heartbeatCoroutine);

            _heartbeatCoroutine = StartCoroutine(LobbyHeartbeatCoroutine());
        }

        private void StopLobbyHeartbeat() {
            if (_heartbeatCoroutine != null) {
                StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
            }
        }

        private IEnumerator LobbyHeartbeatCoroutine() {
            var wait = new WaitForSeconds(15f);

            while (ActiveLobby != null) {
                yield return wait;

                try {
                    Lobbies.Instance.SendHeartbeatPingAsync(ActiveLobby.Id);
                    Debug.Log($"Heartbeat sent for Lobby: {ActiveLobby.Id}");
                }
                catch (Exception e) {
                    Debug.LogWarning($"Heartbeat failed: {e}");
                }
            }
        }

        private async void OnTeamsReady(List<List<PlayerNetworkData>> teams) {
            
            if (_activeLobby == null) 
                return;

            var lobbyData = _activeLobby.Data;
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new FixedString64BytesConverter());
            lobbyData["TeamsData"] = new DataObject(DataObject.VisibilityOptions.Member, JsonConvert.SerializeObject(teams, settings));

            await Lobbies.Instance.UpdateLobbyAsync(_activeLobby.Id, new UpdateLobbyOptions {
                Data = lobbyData
            });
            
            Debug.Log("Lobby Data Updated");
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);

        }
        
        private async Task UpdateLobby()
        {
            _activeLobby = await Lobbies.Instance.GetLobbyAsync(ActiveLobby.Id);
        }

        public async Task<Dictionary<string, DataObject>> GetLobbyData()
        {
            await UpdateLobby();
            return _activeLobby?.Data;
        }
        
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = false;  // Don't auto-spawn!
        }
        
        
    }
}
