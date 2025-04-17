using System;
using _Project.Scripts.Shared.Sessions.Events;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class UiManager : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private GameObject sessionsPanel;
        [SerializeField] private GameObject teamSelectionPanel;
        
        [Header("Events")]
        [SerializeField] private SessionStartedEvent sessionStartedEvent;


        private void OnEnable()
        {
            sessionStartedEvent.Register(OnSessionStarted);
        }

        private void OnDisable()
        {
            sessionStartedEvent.Unregister(OnSessionStarted);
        }

        private void OnSessionStarted()
        {
            sessionsPanel.gameObject.SetActive(false);
            teamSelectionPanel.gameObject.SetActive(true);
        }
    }
}
