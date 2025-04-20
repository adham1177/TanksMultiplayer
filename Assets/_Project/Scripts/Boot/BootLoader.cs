using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace _Project.Scripts.Boot
{
    public class BootLoader : MonoBehaviour
    {
        private async void Start()
        {
            
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;
            try
            {
                await UnityServices.InitializeAsync();
                AuthenticationService.Instance.SwitchProfile($"Player{UnityEngine.Random.Range(1, 1000)}");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
                SceneManager.LoadScene("HomeScene");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
