using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Shared.Sessions.Data;
using _Project.Scripts.Shared.Sessions.Events;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class SessionManager : MonoBehaviour
    {
        private Lobby _activeLobby;
        private string _relayJoinCode;

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

        private const string PlayerNameKey = "playerName";
        private const string RelayJoinCodeKey = "RelayJoinCode";

        private void OnEnable() {
            createRequestedEvent.Register(HandleCreateSession);
            joinRequestedEvent.Register(HandleJoinSession);
            refreshRequestedEvent.Register(HandleRefreshSessions);
        }

        private void OnDisable() {
            createRequestedEvent.Unregister(HandleCreateSession);
            joinRequestedEvent.Unregister(HandleJoinSession);
            refreshRequestedEvent.Unregister(HandleRefreshSessions);
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
    }
}
