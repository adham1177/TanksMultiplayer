using System;
using _Project.Scripts.Shared.Sessions.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.SessionsPanel
{
    public class SessionView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sessionNameText;
        [SerializeField] private TextMeshProUGUI numberOfPlayersText;
        [SerializeField] private Button joinButton;
        private string _sessionId;
        public static event Action<string> TryJoinSession;

        private void Awake()
        {
            joinButton.onClick.AddListener(JoinButtonCallBack);
        }

        public void Init(SessionData sessionData)
        {
            _sessionId = sessionData.ID;
            sessionNameText.text = sessionData.Name;
            numberOfPlayersText.text = sessionData.NumberOfPlayers.ToString();
            joinButton.interactable = sessionData.CanJoin;
        }
        
        private void JoinButtonCallBack()
        {
            if (!string.IsNullOrEmpty(_sessionId))
            {
                TryJoinSession?.Invoke(_sessionId);
            }
        }
        
    }
    
}
