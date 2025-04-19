using UnityEngine;

namespace _Project.Scripts.Network
{
    public class Tracer : MonoBehaviour
    {
        private LineRenderer _line;
        private const float Duration = 0.1f;

        void Awake()
        {
            _line = GetComponent<LineRenderer>();
        }

        public void Show(Vector3 start, Vector3 end)
        {
            _line.SetPosition(0, start);
            _line.SetPosition(1, end);
            StartCoroutine(DestroyAfterDelay());
        }

        private System.Collections.IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(Duration);
            Destroy(gameObject);
        }
    }
}