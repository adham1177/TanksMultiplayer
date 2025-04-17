using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.UI.ChoosingTeamPanel
{
    public class PlayerTeamView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;

        public void Init(string playerName, bool isLocalPlayer)
        {
            playerNameText.text = playerName;
            playerNameText.color = isLocalPlayer ? Color.green : Color.white;
        }
    }
}
