using System;
using System.Collections.Generic;
using _Project.Scripts.Shared.Sessions.Data;
using _Project.Scripts.Shared.Sessions.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.SessionsPanel
{
    public class SessionsPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField sessionNameInputField;
        [SerializeField] private Button sessionCreateButton;
        [SerializeField] private SessionListView sessionListView;
        [SerializeField] private Button refreshButton;
        
        [Header("Events")]
        [SerializeField] private SessionCreateRequestedEvent createRequestedEvent;
        [SerializeField] private SessionJoinRequestedEvent joinRequestedEvent;
        [SerializeField] private SessionRefreshRequestedEvent refreshRequestedEvent;
        [SerializeField] private SessionListUpdatedEvent sessionListUpdatedEvent;

        private void Awake()
        {
            refreshButton.onClick.AddListener(RefreshButtonCallback);
            sessionCreateButton.onClick.AddListener(CreateSessionCallback);
        }

        private void OnEnable()
        {
            SessionView.TryJoinSession += TryJoinSession;
            sessionListUpdatedEvent.Register(UpdateSessionsList);
        }

        private void OnDisable()
        {
            SessionView.TryJoinSession -= TryJoinSession;
            sessionListUpdatedEvent.Unregister(UpdateSessionsList);
        }

        private void Start()
        {
            InvokeRepeating(nameof(RefreshButtonCallback),0f,2f);
        }

        private void TryJoinSession(string sessionName)
        {
            joinRequestedEvent?.Raise(sessionName);
        }

        private void CreateSessionCallback()
        {
            Debug.Log($"Creating session: {sessionNameInputField.text}");
            createRequestedEvent?.Raise(sessionNameInputField.text);
        }

        private void RefreshButtonCallback()
        {
            refreshRequestedEvent?.Raise();
        }

        private void UpdateSessionsList(List<SessionData> sessions)
        {
            Debug.Log("Listing sessions");
            sessionListView.Init(sessions);
        }
    }
}
