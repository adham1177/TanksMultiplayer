using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Network
{
    public class ReviveUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        public void ResetProgress()
        {
            fillImage.fillAmount = 0f;
        }
        public void SetProgress(float progress)
        {
            Debug.Log($"Updating progress {progress}");
            fillImage.fillAmount = Mathf.Clamp01(progress);
        }

        public void Show()
        {
            ResetProgress();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            ResetProgress();
            gameObject.SetActive(false);
        }
    }
}
