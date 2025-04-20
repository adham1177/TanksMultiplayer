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

        private readonly NetworkVariable<Vector3> _serverPosition = new(writePerm: NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<Quaternion> _serverRotation = new(writePerm: NetworkVariableWritePermission.Server);

        private Vector3 _interpolationVelocity;
        private const float CorrectionThreshold = 0.5f;

        private void Awake()
        {
            _player = GetComponent<NetworkPlayer>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) 
                return;
            
            var virtualCam = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCam == null) 
                return;
            virtualCam.Follow = cameraFollowTarget;
            virtualCam.LookAt = cameraFollowTarget;
        }

        public void InitializePosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            _serverPosition.Value = position;
            _serverRotation.Value = rotation;
        }

        private void Update()
        {
            if (IsOwner)
            {
                HandleInput();
            }
            
            if (!IsOwner && IsClient)
            {
                InterpolateToServerState();
            }
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                PredictMovement();
                SubmitMoveServerRpc(_inputForward, _inputTurn);

                var positionError = Vector3.Distance(transform.position, _serverPosition.Value);
                var rotationError = Quaternion.Angle(transform.rotation, _serverRotation.Value);

                if (!(positionError > CorrectionThreshold) && !(rotationError > 1f)) 
                    return; 
                transform.position = Vector3.Lerp(transform.position, _serverPosition.Value, 0.2f);
                transform.rotation = Quaternion.Slerp(transform.rotation, _serverRotation.Value, 0.2f);
            }
            else if (IsServer)
            {
                _serverPosition.Value = transform.position;
                _serverRotation.Value = transform.rotation;
            }
        }

        private void HandleInput()
        {
            _inputForward = Input.GetAxis("Vertical");
            _inputTurn = Input.GetAxis("Horizontal");
        }

        private void PredictMovement()
        {
            var moveAmount = _inputForward * moveSpeed * Time.fixedDeltaTime;
            var turnAmount = _inputTurn * rotationSpeed * Time.fixedDeltaTime;

            transform.Rotate(0f, turnAmount, 0f);
            transform.Translate(Vector3.forward * moveAmount);
        }

        private void InterpolateToServerState()
        {
            transform.position = Vector3.SmoothDamp(transform.position, _serverPosition.Value, ref _interpolationVelocity, 0.05f);
            transform.rotation = Quaternion.Slerp(transform.rotation, _serverRotation.Value, Time.deltaTime * 10f);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitMoveServerRpc(float inputForward, float inputTurn, ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
                return;

            var moveAmount = inputForward * moveSpeed * Time.fixedDeltaTime;
            var turnAmount = inputTurn * rotationSpeed * Time.fixedDeltaTime;

            transform.Rotate(0f, turnAmount, 0f);
            transform.Translate(Vector3.forward * moveAmount);
            _serverPosition.Value = transform.position;
            _serverRotation.Value = transform.rotation;
        }
    }
}
