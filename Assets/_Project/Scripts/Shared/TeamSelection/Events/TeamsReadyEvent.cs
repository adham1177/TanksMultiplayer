using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Shared.TeamSelection.Events
{
    [CreateAssetMenu(menuName = "Events/Teams Ready Event")]
    public class TeamsReadyEvent : ScriptableObject
    {
        private Action<List<List<PlayerNetworkData>>> _listeners;

        public void Raise(List<List<PlayerNetworkData>> teams)
        {
            _listeners?.Invoke(teams);
        }

        public void Register(Action<List<List<PlayerNetworkData>>> listener)
        {
            _listeners += listener;
        }

        public void Unregister(Action<List<List<PlayerNetworkData>>> listener)
        {
            _listeners -= listener;
        }
    }
}
