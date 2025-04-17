using System;
using System.Collections.Generic;
using _Project.Scripts.Shared.Sessions.Data;
using UnityEngine;

namespace _Project.Scripts.Shared.Sessions.Events
{
    [CreateAssetMenu(menuName = "Events/Session List Updated Event")]
    public class SessionListUpdatedEvent : ScriptableObject
    {
        private Action<List<SessionData>> _listeners;

        public void Raise(List<SessionData> sessionList)
        {
            _listeners?.Invoke(sessionList);
        }

        public void Register(Action<List<SessionData>> listener)
        {
            _listeners += listener;
        }

        public void Unregister(Action<List<SessionData>> listener)
        {
            _listeners -= listener;
        }
    }
}
