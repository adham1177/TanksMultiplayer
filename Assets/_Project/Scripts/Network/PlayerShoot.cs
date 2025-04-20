using System;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class PlayerShoot : NetworkBehaviour
    {
        [SerializeField] private float shootRange = 10f;
        [SerializeField] private int damage = 10;
        [SerializeField] private Transform shootOrigin;
        [SerializeField] private Tracer tracerPrefab;
        private NetworkPlayer _player; 

        private void Update()
        {
            if (!IsOwner) return;

            if (!Input.GetKeyDown(KeyCode.Space)) 
                return;
            
            var direction = shootOrigin.forward;
            ShootServerRpc(direction);
        }

        private void Awake()
        {
            _player = GetComponent<NetworkPlayer>();
        }

        [ServerRpc]
        private void ShootServerRpc(Vector3 direction)
        {
            var startPoint = shootOrigin.position;
            var endPoint = startPoint + direction * shootRange;

            if (Physics.Raycast(startPoint, direction, out RaycastHit hit, shootRange))
            {
                endPoint = hit.point;

                var otherPlayer = hit.collider.GetComponent<NetworkPlayer>();
                if (otherPlayer != null && otherPlayer.teamId.Value != _player.teamId.Value)
                {
                    otherPlayer.TakeDamage(damage, OwnerClientId);
                }
            }
            
            ShowTracerClientRpc(startPoint, endPoint);
        }

        [ClientRpc]
        private void ShowTracerClientRpc(Vector3 start, Vector3 end)
        {
            if (!tracerPrefab) 
                return;
            var tracer = Instantiate(tracerPrefab);
            tracer.Show(start, end);
        }
    }
}