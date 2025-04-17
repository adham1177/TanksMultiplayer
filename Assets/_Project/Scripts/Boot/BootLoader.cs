using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Boot
{
    public class BootLoader : MonoBehaviour
    {
        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync(); // Initialize Unity Gaming Services SDKs.
                AuthenticationService.Instance.SwitchProfile("aa");
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Anonymously authenticate the player
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
