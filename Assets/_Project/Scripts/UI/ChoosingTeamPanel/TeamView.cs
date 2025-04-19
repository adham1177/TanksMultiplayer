using System;
using System.Collections.Generic;
using _Project.Scripts.Shared;
using Unity.Collections;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.ChoosingTeamPanel
{
    public class TeamView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private PlayerTeamView playerTeamViewPrefab;
        [SerializeField] private List<PlayerTeamView> playerTeamViews;
        [SerializeField] private Button chooseTeamButton;
        [SerializeField] private Button startGameButton;
        
        [Header("Data")]
        [SerializeField] private int index;
        
        public event Action<int> TeamSelected;

        private void Awake()
        {
            chooseTeamButton.onClick.AddListener(ChooseTeamCallBack);
        }
        

        public void Init(List<PlayerNetworkData> players, string localPlayerId)
        {
            for (var i = playerTeamViews.Count; i < players.Count; i++)
            {
                var newView = Instantiate(playerTeamViewPrefab, contentParent);
                playerTeamViews.Add(newView);
            }
            
            for (var i = 0; i < players.Count; i++)
            {
                playerTeamViews[i].gameObject.SetActive(true);
                playerTeamViews[i].Init(players[i].Name.ToString(), localPlayerId == players[i].PlayerId);
            }
            
            for (var i = players.Count; i < playerTeamViews.Count; i++)
            {
                playerTeamViews[i].gameObject.SetActive(false);
            }

            chooseTeamButton.gameObject.SetActive(players.Count < 2);
            
        }
        
        private void ChooseTeamCallBack()
        {
            TeamSelected?.Invoke(index);
        }
    }
}
