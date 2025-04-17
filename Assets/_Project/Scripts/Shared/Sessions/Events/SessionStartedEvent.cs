using System;
using UnityEngine;

namespace _Project.Scripts.Shared.Sessions.Events
{
    [CreateAssetMenu(menuName = "Events/Session StartedEvent")]
    public class SessionStartedEvent : ScriptableObject
    {
        private Action _listeners;

        public void Raise() => _listeners?.Invoke();
        public void Register(Action listener) => _listeners += listener;
        public void Unregister(Action listener) => _listeners -= listener;
    }
}
