using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using _Project.Scripts.Shared;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        private readonly Dictionary<ulong, PlayerNetworkData> _players = new();

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            Instance = this;
            SpawnAllPlayersFromLobby();
        }

        private async void SpawnAllPlayersFromLobby()
        {
            var lobbyData = await SessionManager.instance.GetLobbyData();
            
            Debug.Log($"LobbyData: {JsonConvert.SerializeObject(lobbyData)}");

            if (!lobbyData.TryGetValue("TeamsData", out var teamsDataObject) || teamsDataObject == null)
            {
                Debug.LogError("No teams data found");
                throw new InvalidDataException();
            }
    
            Debug.Log($"TeamsData: {teamsDataObject.Value}");
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new FixedString64BytesConverter());
            var teams = JsonConvert.DeserializeObject<List<List<PlayerNetworkData>>>(teamsDataObject.Value, settings);

            for (var index = 0; index < teams.Count; index++)
            {
                var team = teams[index];
                SpawnTeamPlayers(team);
            }
        }

        private void SpawnTeamPlayers(List<PlayerNetworkData> team)
        {
            foreach (var player in team)
            {

                var playerPrefab = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);
                playerPrefab.GetComponent<NetworkObject>().SpawnAsPlayerObject(player.ClientId);
                
                var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(player.ClientId);
                var networkPlayer = playerNetworkObject.GetComponent<NetworkPlayer>();
                networkPlayer.ServerInitialize(player);
                
                _players[player.ClientId] = player;
            }
        }
    }
}
