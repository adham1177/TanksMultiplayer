using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class PlayerMovementController : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 720f;

        private Vector3 _inputDirection;
        private Quaternion _inputRotation;
        private NetworkPlayer _player;

        // Server authoritative position/rotation
        private NetworkVariable<Vector3> serverPosition = new(writePerm: NetworkVariableWritePermission.Server);
        private NetworkVariable<Quaternion> serverRotation = new(writePerm: NetworkVariableWritePermission.Server);

        private void Awake()
        {
            _player = GetComponent<NetworkPlayer>();
        }

        private void Update()
        {
            if (IsOwner)
            {
                HandleInput();
                PredictMovement();
                SubmitMoveServerRpc(_inputDirection, _inputRotation);
            }
            else
            {
                InterpolateToServerState();
            }
        }

        private void HandleInput()
        {
            _inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            _inputRotation = transform.rotation;

            if (_inputDirection != Vector3.zero)
            {
                _inputRotation = Quaternion.LookRotation(_inputDirection);
            }
        }

        private void PredictMovement()
        {
            // Local prediction — immediate feedback
            transform.position += _inputDirection * (moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                _inputRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

        private void InterpolateToServerState()
        {
            // Smoothly interpolate to server state (for non-owners)
            transform.position = Vector3.Lerp(
                transform.position,
                serverPosition.Value,
                Time.deltaTime * 15f
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                serverRotation.Value,
                Time.deltaTime * 15f
            );
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitMoveServerRpc(Vector3 direction, Quaternion lookRotation, ServerRpcParams rpcParams = default)
        {
            // Validate ownership to prevent cheating
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
                return;

            // Move and rotate the player on the server
            transform.position += direction * (moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            // Update authoritative server state
            serverPosition.Value = transform.position;
            serverRotation.Value = transform.rotation;
            
        }
    }
}
