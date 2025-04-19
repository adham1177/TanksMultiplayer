using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Shared;
using _Project.Scripts.Shared.Sessions.Events;
using _Project.Scripts.Shared.TeamSelection.Events;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Network
{
    public class TeamSelectionSync : NetworkBehaviour
    {
        [Header("Events")] 
        [SerializeField] private TeamSelectedEvent teamSelectedEvent;
        [SerializeField] private TeamsChangedEvent teamsChangedEvent;
        [SerializeField] private TeamsReadyEvent teamsReadyEvent;
        
        
        private readonly List<List<PlayerNetworkData>> _teams = new(){ new List<PlayerNetworkData>(), new List<PlayerNetworkData>()};

        public override void OnNetworkSpawn()
        {
            teamSelectedEvent.Register(OnTeamSelected);
            if (!IsServer) 
                return;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        }
        

        public override void OnNetworkDespawn()
        {
            teamSelectedEvent.Unregister(OnTeamSelected);
            if (IsServer)
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        }
        
        private void OnClientConnectedCallback(ulong clientId)
        {
            OnTeamsChangedClientRpc(_teams[0].ToArray(), _teams[1].ToArray());
        }
        

        private void OnTeamsChanged(NetworkListEvent<PlayerNetworkData> eventData)
        {
            teamsChangedEvent?.Raise(_teams[0], _teams[1]);
        }
        
        
        private void OnTeamSelected(int index)
        {
            if (index == -1 && IsServer)
            {
                OnTeamsChangedClientRpc(_teams[0].ToArray(), _teams[1].ToArray());
                return;
            }
            var playerData = new PlayerNetworkData(AuthenticationService.Instance.PlayerId, NetworkManager.Singleton.LocalClientId, index, new FixedString64Bytes($"Player{NetworkManager.Singleton.LocalClientId}"));
            UpdateTeamsServerRpc(playerData, index);
        }


        [ServerRpc(RequireOwnership = false)]
        private void UpdateTeamsServerRpc(PlayerNetworkData player, int teamIndex)
        {
            teamIndex -= 1;
            if (_teams[teamIndex].Contains(player) || _teams[teamIndex].Count == 2)
            {
                return;
            }

            foreach (var team in _teams)
            {
                team.Remove(player);
            }
            
            _teams[teamIndex].Add(player);
            OnTeamsChangedClientRpc(_teams[0].ToArray(), _teams[1].ToArray());
            CheckForTeamsCompleteness();
        }

        [ClientRpc(RequireOwnership = false)]
        private void OnTeamsChangedClientRpc(PlayerNetworkData[] team1, PlayerNetworkData[] team2)
        {
            teamsChangedEvent?.Raise(team1.ToList(), team2.ToList());
        }

        private void CheckForTeamsCompleteness()
        {
            Debug.Log("Checking For Teams");
            if (_teams[0].Count == 1 && _teams[1].Count == 1)
            {
                teamsReadyEvent?.Raise(_teams);
            }
        }
    }
}
