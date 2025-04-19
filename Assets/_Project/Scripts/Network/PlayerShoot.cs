using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class PlayerShoot : NetworkBehaviour
    {
        [SerializeField] private float shootRange = 10f;
        [SerializeField] private int damage = 10;
        [SerializeField] private Transform shootOrigin;
        [SerializeField] private GameObject tracerPrefab;

        void Update()
        {
            if (!IsOwner) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 direction = shootOrigin.forward;
                ShootServerRpc(direction);
            }
        }

        [ServerRpc]
        void ShootServerRpc(Vector3 direction)
        {
            Vector3 startPoint = shootOrigin.position;
            Vector3 endPoint = startPoint + direction * shootRange;

            if (Physics.Raycast(startPoint, direction, out RaycastHit hit, shootRange))
            {
                endPoint = hit.point;

                var player = hit.collider.GetComponent<NetworkPlayer>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }

            // Always show tracer (hit or miss)
            ShowTracerClientRpc(startPoint, endPoint);
        }

        [ClientRpc]
        void ShowTracerClientRpc(Vector3 start, Vector3 end)
        {
            if (tracerPrefab)
            {
                GameObject tracer = Instantiate(tracerPrefab);
                tracer.GetComponent<Tracer>().Show(start, end);
            }
        }
    }
}