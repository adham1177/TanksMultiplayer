using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class PlayerReviveArea : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private ReviveUI reviveUI;
        
        
        [Header("References")]
        [SerializeField] private  GameObject reviveArea;
        [SerializeField] private Collider reviveCollider;
        [SerializeField] private float reviveDuration = 5f;

        private Coroutine _reviveCoroutine;
        private NetworkPlayer _ownerPlayer;
        private NetworkPlayer _currentReviver;
        
        private NetworkVariable<float> _syncedReviveProgress = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            reviveCollider.enabled = false;
            reviveArea.SetActive(false);
            _ownerPlayer = GetComponent<NetworkPlayer>();
        }

        public override void OnNetworkSpawn()
        {
            _syncedReviveProgress.OnValueChanged += OnReviveProgressChanged;
        }

        public override void OnNetworkDespawn()
        {
            _syncedReviveProgress.OnValueChanged -= OnReviveProgressChanged;

           
        }

        public void EnableReviveArea()
        {
            reviveArea.SetActive(true);
            reviveCollider.enabled = true;
        }

        public void DisableReviveArea()
        {
            reviveArea.SetActive(false);
            reviveCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            
            Debug.Log("Reviving");
            var teammate = other.GetComponent<NetworkPlayer>();
            if (teammate == null || teammate.teamId.Value != _ownerPlayer.teamId.Value || teammate.isDead.Value)
                return;

            if (_reviveCoroutine != null) 
                return;
            
            _currentReviver = teammate;
            _reviveCoroutine = StartCoroutine(ReviveTimer());
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer || _reviveCoroutine == null) return;

            var teammate = other.GetComponent<NetworkPlayer>();
            
            if (teammate != _currentReviver) 
                return;
            
            StopCoroutine(_reviveCoroutine);
            _reviveCoroutine = null;
            _currentReviver = null;
            reviveUI.ResetProgress();
        }

        private IEnumerator ReviveTimer()
        {
            var timer = 0f;
            while (timer < reviveDuration)
            {
                if (_currentReviver == null || _currentReviver.isDead.Value)
                    yield break;
                
                var normalized = timer / reviveDuration;
                _syncedReviveProgress.Value = normalized;

                timer += Time.deltaTime;
                yield return null;
            }

            RevivePlayer();
        }

        private void RevivePlayer()
        {
            Debug.Log($"Player {_ownerPlayer.playerName.Value} revived!");
            _ownerPlayer.currentHealth.Value = 100f;
            _ownerPlayer.isDead.Value = false;
            _ownerPlayer.playerMovementController.enabled = true;
            _ownerPlayer.playerShoot.enabled = true;
            reviveCollider.enabled = false;
            _reviveCoroutine = null;
            _currentReviver = null;
        }
        
        
        void OnReviveProgressChanged(float previousValue, float newValue)
        {
            reviveUI.SetProgress(newValue);
        }
    }
}
