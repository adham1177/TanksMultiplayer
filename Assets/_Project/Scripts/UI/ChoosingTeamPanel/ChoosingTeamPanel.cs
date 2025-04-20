using System.Collections.Generic;
using _Project.Scripts.Shared;
using _Project.Scripts.Shared.Sessions.Events;
using _Project.Scripts.Shared.TeamSelection.Events;
using Unity.Services.Authentication;
using UnityEngine;

namespace _Project.Scripts.UI.ChoosingTeamPanel
{
    public class ChoosingTeamPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TeamView team1View;
        [SerializeField] private TeamView team2View;
        
        [Header("Events")]
        [SerializeField] private TeamSelectedEvent teamSelectedEvent;
        [SerializeField] private TeamsChangedEvent teamsChangedEvent;
        


        private void OnEnable()
        {
            team1View.TeamSelected += TeamSelected;
            team2View.TeamSelected += TeamSelected;
            teamsChangedEvent.Register(OnTeamsChanged);
            teamSelectedEvent?.Raise(-1);
        }
        private void OnDisable()
        {
            team1View.TeamSelected -= TeamSelected;
            team2View.TeamSelected -= TeamSelected;
            teamsChangedEvent.Unregister(OnTeamsChanged);
        }

        private void TeamSelected(int index)
        {
            teamSelectedEvent?.Raise(index);
        }
        
        private void OnTeamsChanged(List<PlayerNetworkData> team1, List<PlayerNetworkData> team2)
        {
            team1View.Init(team1,AuthenticationService.Instance.PlayerId);
            team2View.Init(team2,AuthenticationService.Instance.PlayerId);
        }

    }
}
