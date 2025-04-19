using Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class PlayerMovementController : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private Transform cameraFollowTarget;

        private float _inputForward;
        private float _inputTurn;
        private NetworkPlayer _player;

        private NetworkVariable<Vector3> serverPosition = new(writePerm: NetworkVariableWritePermission.Server);
        private NetworkVariable<Quaternion> serverRotation = new(writePerm: NetworkVariableWritePermission.Server);

        private Vector3 interpolationVelocity;
        private const float correctionThreshold = 0.5f;

        private void Awake()
        {
            _player = GetComponent<NetworkPlayer>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                var virtualCam = FindObjectOfType<CinemachineVirtualCamera>();
                if (virtualCam != null)
                {
                    virtualCam.Follow = cameraFollowTarget;
                    virtualCam.LookAt = cameraFollowTarget;
                }
            }
        }

        public void InitializePosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            serverPosition.Value = position;
            serverRotation.Value = rotation;
        }

        private void Update()
        {
            if (IsOwner)
            {
                HandleInput(); // Input is still best in Update
            }

            // Visual-only interpolation for non-owners
            if (!IsOwner && IsClient)
            {
                InterpolateToServerState();
            }
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                PredictMovement(); // Local prediction
                SubmitMoveServerRpc(_inputForward, _inputTurn); // Send input to server

                float positionError = Vector3.Distance(transform.position, serverPosition.Value);
                float rotationError = Quaternion.Angle(transform.rotation, serverRotation.Value);

                if (positionError > correctionThreshold || rotationError > 1f) // 2 degrees threshold
                {
                    transform.position = Vector3.Lerp(transform.position, serverPosition.Value, 0.1f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, serverRotation.Value, 0.1f);
                }
            }
            else if (IsServer) // Server moves authoritative version
            {
                // Movement already applied via RPC
                // But server also updates authoritative position/rotation
                serverPosition.Value = transform.position;
                serverRotation.Value = transform.rotation;
            }
        }

        private void HandleInput()
        {
            _inputForward = Input.GetAxis("Vertical");
            _inputTurn = Input.GetAxis("Horizontal");
        }

        private void PredictMovement()
        {
            float moveAmount = _inputForward * moveSpeed * Time.fixedDeltaTime;
            float turnAmount = _inputTurn * rotationSpeed * Time.fixedDeltaTime;

            transform.Rotate(0f, turnAmount, 0f);
            transform.Translate(Vector3.forward * moveAmount);
        }

        private void InterpolateToServerState()
        {
            transform.position = Vector3.SmoothDamp(transform.position, serverPosition.Value, ref interpolationVelocity, 0.05f);
            transform.rotation = Quaternion.Slerp(transform.rotation, serverRotation.Value, Time.deltaTime * 10f);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitMoveServerRpc(float inputForward, float inputTurn, ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
                return;

            float moveAmount = inputForward * moveSpeed * Time.fixedDeltaTime;
            float turnAmount = inputTurn * rotationSpeed * Time.fixedDeltaTime;

            transform.Rotate(0f, turnAmount, 0f);
            transform.Translate(Vector3.forward * moveAmount);

            // Update authoritative position
            serverPosition.Value = transform.position;
            serverRotation.Value = transform.rotation;
        }
    }
}
