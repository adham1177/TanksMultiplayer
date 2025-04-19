using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class KillFeedEntryPrefab : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void Init(string message)
        {
            text.text = message;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
