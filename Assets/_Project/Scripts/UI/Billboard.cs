using UnityEngine;

namespace _Project.Scripts.UI
{
    public class Billboard : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_mainCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
            }
        }
    }
}