using System;
using UnityEngine;

namespace _Project.Scripts.Shared.Gameplay.Events
{
    [CreateAssetMenu(menuName = "Events/Player Killed Event")]
    public class PlayerKilledEvent : ScriptableObject
    {
        private Action<string, string> _listeners;

        public void Raise(string killerName, string victimName)
        {
            _listeners?.Invoke(killerName, victimName);
        }

        public void Register(Action<string, string> listener)
        {
            _listeners += listener;
        }

        public void Unregister(Action<string, string> listener)
        {
            _listeners -= listener;
        }
    }
}
