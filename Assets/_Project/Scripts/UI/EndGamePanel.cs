using System;
using _Project.Scripts.Shared.Gameplay.Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class EndGamePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI teamName;
        [SerializeField] private Button restartButton;
        [SerializeField] private GameObject panel;
        [SerializeField] private MatchEndEvent endGameEvent;

        private void Awake()
        {
            restartButton.onClick.AddListener(RestartButtonCallback);
        }

        private void OnEnable()
        {
            endGameEvent.Register(OnEndGame);
        }

        private void OnDisable()
        {
            endGameEvent.Unregister(OnEndGame);
        }

        public void Init(string winnerTeam)
        {
            teamName.text = winnerTeam;
        }
        
        private void RestartButtonCallback()
        {
            SceneManager.LoadScene("HomeScene");
        }

        private void OnEndGame(string winnerTeamName)
        {
            Init(winnerTeam: winnerTeamName);
            panel.SetActive(true);
        }
        
        
    }
}
