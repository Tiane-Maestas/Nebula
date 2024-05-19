using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

namespace Nebula.Multiplayer
{
    public class MultiplayerManager : Singleton<MultiplayerManager>
    {
        private async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public void SignOut()
        {
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();
        }
    }
}
