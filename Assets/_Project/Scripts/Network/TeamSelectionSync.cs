using System;
using _Project.Scripts.Shared;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class TeamSelectionSync : NetworkBehaviour
    {
        private readonly NetworkList<PlayerNetworkData> _team1 = new NetworkList<PlayerNetworkData>();
        private readonly NetworkList<PlayerNetworkData> _team2 = new NetworkList<PlayerNetworkData>();

        public override void OnNetworkSpawn()
        {
            _team1.OnListChanged += OnTeamsChanged;
            _team2.OnListChanged += OnTeamsChanged;
        }

        public override void OnNetworkDespawn()
        {
            _team1.OnListChanged -= OnTeamsChanged;
            _team2.OnListChanged -= OnTeamsChanged;
        }
        

        private void OnTeamsChanged(NetworkListEvent<PlayerNetworkData> eventData)
        {
            Debug.Log("Team 1");
            foreach (var player in _team1)
            {
                Debug.Log(player.Name);
            }
            Debug.Log("Team 2");
            foreach (var player in _team2)
            {
                Debug.Log(player.Name);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChooseTeamServerRpc(PlayerNetworkData player, int teamIndex)
        {
            if (teamIndex == 1)
            {
                _team2.Remove(player);
                _team1.Add(player);
            }
            else
            {
                _team1.Remove(player);
                _team2.Add(player);
            }
        }
    }
}
