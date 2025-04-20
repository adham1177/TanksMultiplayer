using System;
using _Project.Scripts.Shared;
using _Project.Scripts.Shared.Gameplay.Events;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;

namespace _Project.Scripts.Network
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private GameObject playerUI;
        [SerializeField] private ReviveUI reviveUI;
        
        [Header("PlayerComponents")]
        [SerializeField] public PlayerMovementController playerMovementController;
        [SerializeField] public PlayerShoot playerShoot;
        [SerializeField] public PlayerReviveArea playerReviveArea;

        [Header("NetworkVariables")]
        public NetworkVariable<int> teamId = new();
        public NetworkVariable<FixedString64Bytes> playerName = new("");
        public NetworkVariable<float> currentHealth = new(100);
        public NetworkVariable<bool> isDead = new(false);
        private readonly NetworkVariable<ulong> _lastKillerId = new(ulong.MaxValue);

        [Header("Events")] 
        [SerializeField] private PlayerKilledEvent playerKilledEvent;

        public static event Action<int> PlayerKilled; 

        private const float MaxHealth = 100;
        

        public override void OnNetworkSpawn()
        {
            Debug.Log($"OnNetworkSpawn");
            Debug.Log($"isClient: {NetworkManager.Singleton.IsClient}");
            if (IsClient)
            {
                teamId.OnValueChanged += OnTeamChanged;
                playerName.OnValueChanged += UpdatePlayerNameUI;
                currentHealth.OnValueChanged += UpdateHealthUI;
                isDead.OnValueChanged += OnDeathStateChanged;
                
            }
        }

        public void ServerInitialize(PlayerNetworkData data)
        {
            Debug.Log($"isClient: {NetworkManager.Singleton.IsClient}");
            Debug.Log($"isHost: {NetworkManager.Singleton.IsHost}");
            Debug.Log($"isServer: {NetworkManager.Singleton.IsServer}");
            if (!NetworkManager.Singleton.IsServer) 
                return;
            
            Debug.Log("Initializing Prefab");

            teamId.Value = data.TeamId;
            playerName.Value = data.Name;
            currentHealth.Value = MaxHealth;
        }

        private void OnTeamChanged(int oldTeam, int newTeam)
        {
            Debug.Log($"OnTeamChanged: {oldTeam}, {newTeam}");
            playerRenderer.material.color = GetTeamColor(newTeam);
        }

        private void UpdatePlayerNameUI(FixedString64Bytes oldName, FixedString64Bytes newName)
        {
            Debug.Log($"UpdatePlayerNameUI: {oldName}, {newName}");
            playerNameText.text = newName.ToString();
        }

        private void UpdateHealthUI(float oldHealth, float newHealth)
        {
            Debug.Log($"UpdateHealthUI: {oldHealth}, {newHealth}");
            healthSlider.maxValue = MaxHealth;
            healthSlider.value = newHealth;
        }

        private Color GetTeamColor(int teamIndex)
        {
            return teamIndex switch
            {
                1 => Color.blue,
                2 => Color.red,
                _ => Color.gray
            };
        }

        public void TakeDamage(float damage, ulong attackerId)
        {
            if (!IsServer) return;

            currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0);
            if (currentHealth.Value <= 0 && !isDead.Value)
            {
                _lastKillerId.Value = attackerId;
                isDead.Value = true;
                RaisePlayerKilledEvent();
                PlayerKilled?.Invoke(teamId.Value);
            }
        }
        
        private void OnDeathStateChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                Debug.Log($"Player {playerName.Value} died.");
                playerMovementController.enabled = false;
                playerShoot.enabled = false;
                playerReviveArea.EnableReviveArea();
                playerUI.SetActive(false);
                reviveUI.Show();
            }
            else
            {
                playerMovementController.enabled = true;
                playerShoot.enabled = true;
                playerReviveArea.DisableReviveArea();
                playerUI.gameObject.SetActive(true);
                reviveUI.Hide();
            }
        }

        private void RaisePlayerKilledEvent()
        {
            if (_lastKillerId.Value == ulong.MaxValue)
                return;
            
            var victimName = playerName.Value;
            var killerName = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(_lastKillerId.Value).GetComponent<NetworkPlayer>().playerName.Value;
            
            Debug.Log($"{killerName.ToString()} killed {victimName.ToString()}");
            NotifyPlayerKilledEventClientRpc(killerName, victimName);
        }

        [ClientRpc]
        private void NotifyPlayerKilledEventClientRpc(FixedString64Bytes killerName, FixedString64Bytes victimName)
        {
            playerKilledEvent?.Raise(killerName.ToString(), victimName.ToString());
        }
        
    }
}
