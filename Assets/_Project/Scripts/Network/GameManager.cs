using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using _Project.Scripts.Shared;
using Cinemachine;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        private readonly Dictionary<ulong, PlayerNetworkData> _players = new();
        [SerializeField] private List<Transform> team1Positions;
        [SerializeField] private List<Transform> team2Positions;

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
                SpawnTeamPlayers(team, index);
            }
        }

        private void SpawnTeamPlayers(List<PlayerNetworkData> team, int teamId)
        {
            for (var index = 0; index < team.Count; index++)
            {
                var initialTransform = GetPlayerInitialTransform(teamId, index);
                var player = team[index];
                var prefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
                prefab.transform.position = initialTransform.position;
                prefab.transform.rotation = initialTransform.rotation;

                var playerPrefab = Instantiate(prefab);
                playerPrefab.GetComponent<NetworkObject>().SpawnAsPlayerObject(player.ClientId);

                var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(player.ClientId);
                var networkPlayer = playerNetworkObject.GetComponent<NetworkPlayer>();
                networkPlayer.ServerInitialize(player);

                var movementController = playerNetworkObject.GetComponent<PlayerMovementController>();
                movementController.InitializePosition(initialTransform.position, initialTransform.rotation);

                _players[player.ClientId] = player;
            }
        }


        private Transform GetPlayerInitialTransform(int teamIndex, int playerIndex)
        {
            return teamIndex == 0 ? team1Positions[playerIndex] : team2Positions[playerIndex];
        }
    }
}
