using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Shared.TeamSelection.Events
{
    [CreateAssetMenu(menuName = "Events/Teams Changed Event")]
    public class TeamsChangedEvent : ScriptableObject
    {
        private Action<List<PlayerNetworkData>, List<PlayerNetworkData>> _listeners;

        public void Raise(List<PlayerNetworkData> team1, List<PlayerNetworkData> team2)
        {
            _listeners?.Invoke(team1, team2);
        }

        public void Register(Action<List<PlayerNetworkData>, List<PlayerNetworkData>> listener)
        {
            _listeners += listener;
        }

        public void Unregister(Action<List<PlayerNetworkData>, List<PlayerNetworkData>> listener)
        {
            _listeners -= listener;
        }
    }
}
