using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using _Project.Scripts.Shared;
using _Project.Scripts.Shared.Gameplay.Events;
using Cinemachine;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        private readonly Dictionary<ulong, NetworkPlayer> _players = new();
        [SerializeField] private List<Transform> team1Positions;
        [SerializeField] private List<Transform> team2Positions;
        List<NetworkPlayer> _team1Players = new();
        List<NetworkPlayer> _team2Players = new();

        [SerializeField] private MatchEndEvent matchEndEvent;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            Instance = this;
            SpawnAllPlayersFromLobby();
            NetworkPlayer.PlayerKilled += OnPlayerKilled;
        }
        

        private void OnDisable()
        {
            if (IsServer)
                NetworkPlayer.PlayerKilled -= OnPlayerKilled;
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
            var teamList = teamId == 0 ? _team1Players : _team2Players;
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
                teamList.Add(networkPlayer);
            }
        }


        private Transform GetPlayerInitialTransform(int teamIndex, int playerIndex)
        {
            return teamIndex == 0 ? team1Positions[playerIndex] : team2Positions[playerIndex];
        }
        
        private void OnPlayerKilled(int teamIndex)
        {
            Debug.Log("Checking For End Game");
            var team = teamIndex == 1 ? _team1Players : _team2Players;
            if (IsAllTeamDead(team))
            {
                var teamSuffix = teamIndex == 1 ? "2" : "1";
                var teamName = $"Team{teamSuffix}";
                NotifyGameOverClientRpc(teamName);
            }
        }

        private bool IsAllTeamDead(List<NetworkPlayer> team)
        {
            foreach (var player in team)
            {
                Debug.Log(player.isDead.Value);
                if (!player.isDead.Value)
                {
                    return false;
                }
            }

            return true;
        }

        [ClientRpc]
        private void NotifyGameOverClientRpc(string winnerTeamName)
        {
            matchEndEvent?.Raise(winnerTeamName);
        }
        
        
    }
}
