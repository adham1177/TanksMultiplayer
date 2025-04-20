using System;
using UnityEngine;

namespace _Project.Scripts.Shared.TeamSelection.Events
{
    [CreateAssetMenu(menuName = "Events/Team Selected Event")]
    public class TeamSelectedEvent : ScriptableObject
    {
        private Action<int> _listeners;

        public void Raise(int teamIndex)
        {
            _listeners?.Invoke(teamIndex);
        }

        public void Register(Action<int> listener)
        {
            _listeners += listener;
        }

        public void Unregister(Action<int> listener)
        {
            _listeners -= listener;
        }
    }
}
