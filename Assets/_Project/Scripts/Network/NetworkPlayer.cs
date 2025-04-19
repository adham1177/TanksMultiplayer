using _Project.Scripts.Shared;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.Scripts.Network
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Renderer playerRenderer;

        public NetworkVariable<int> teamId = new();
        public NetworkVariable<FixedString64Bytes> playerName = new("");
        public NetworkVariable<float> currentHealth = new(100);

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

        public void TakeDamage(float damage)
        {
            if (!IsServer) return;

            currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0);
            if (currentHealth.Value == 0)
            {
                HandleDeath();
            }
        }

        private void HandleDeath()
        {
            Debug.Log($"Player {playerName.Value} died.");
            // later you can notify GameManager for kill feed and respawn/revive.
        }
    }
}
