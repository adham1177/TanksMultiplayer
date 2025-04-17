using System;
using UnityEngine;

namespace _Project.Scripts.Shared.Sessions.Events
{
    [CreateAssetMenu(menuName = "Events/Session Create Requested Event")]
    public class SessionCreateRequestedEvent : ScriptableObject
    {
        private Action<string> _listeners;

        public void Raise(string sessionName) => _listeners?.Invoke(sessionName);

        public void Register(Action<string> listener) => _listeners += listener;
        public void Unregister(Action<string> listener) => _listeners -= listener;
    }
}
