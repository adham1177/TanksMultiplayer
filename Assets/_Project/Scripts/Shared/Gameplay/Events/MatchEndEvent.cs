using System;
using UnityEngine;

namespace _Project.Scripts.Shared.Gameplay.Events
{
    [CreateAssetMenu(menuName = "Events/Match End Event")]
    public class MatchEndEvent : ScriptableObject
    {
        private Action<string> _listeners;

        public void Raise(string teamName)
        {
            _listeners?.Invoke(teamName);
        }

        public void Register(Action<string> listener)
        {
            _listeners += listener;
        }

        public void Unregister(Action<string> listener)
        {
            _listeners-= listener;
        }
    }
}
