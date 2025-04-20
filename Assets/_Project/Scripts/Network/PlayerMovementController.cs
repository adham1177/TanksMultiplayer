using Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network {
    public class PlayerMovementController : NetworkBehaviour {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float turnSpeed = 100f;
        public bool enableClientPrediction = true;

        private Vector2 input;

        void Update() {
            if (!IsOwner) return;

            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (enableClientPrediction) {
                ApplyMovement(input);
            }

            SendInputToServerRpc(input);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                return;
            var virtualCam = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCam == null) 
                return;
            virtualCam.Follow = transform;
            virtualCam.LookAt = transform;
        }

        [ServerRpc]
        private void SendInputToServerRpc(Vector2 moveInput) {
            ApplyMovement(moveInput);
            BroadcastStateClientRpc(transform.position, transform.rotation);
        }

        private void ApplyMovement(Vector2 moveInput) {
            float move = moveInput.y * moveSpeed * Time.fixedDeltaTime;
            float turn = moveInput.x * turnSpeed * Time.fixedDeltaTime;

            transform.position += transform.forward * move;
            transform.Rotate(Vector3.up * turn);
        }

        [ClientRpc]
        private void BroadcastStateClientRpc(Vector3 newPosition, Quaternion newRotation) {
            if (IsOwner && enableClientPrediction) return; // Don't overwrite predicted movement

            transform.position = newPosition;
            transform.rotation = newRotation;
        }
    }
}