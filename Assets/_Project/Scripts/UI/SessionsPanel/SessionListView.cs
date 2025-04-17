using System.Collections.Generic;
using _Project.Scripts.Shared.Sessions.Data;
using UnityEngine;

namespace _Project.Scripts.UI.SessionsPanel
{
    public class SessionListView : MonoBehaviour
    {
        [SerializeField] private SessionView sessionViewPrefab;
        [SerializeField] private Transform contentParent;

        [SerializeField] private List<SessionView> sessionsViews;

        public void Init(List<SessionData> sessions)
        {
            // Instantiate new views if needed
            for (var i = sessionsViews.Count; i < sessions.Count; i++)
            {
                var newView = Instantiate(sessionViewPrefab, contentParent);
                sessionsViews.Add(newView);
            }

            // Initialize or update the active session views
            for (var i = 0; i < sessions.Count; i++)
            {
                sessionsViews[i].gameObject.SetActive(true);
                sessionsViews[i].Init(sessions[i]);
            }

            // Deactivate extra views if any
            for (var i = sessions.Count; i < sessionsViews.Count; i++)
            {
                sessionsViews[i].gameObject.SetActive(false);
            }
        }
    }
}
