using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

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

        public void StartLanHost() 
        {
            NetworkManager.Singleton.Shutdown();
            ConfigureLanTransport();
            NetworkManager.Singleton.StartHost();
        }

        public enum TransportMode
        {
            Relay,
            LAN
        }
        private TransportMode _currentTransportMode = TransportMode.Relay;

        private void ConfigureLanTransport()
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(
                "0.0.0.0",
                7777
            );

            _currentTransportMode = TransportMode.LAN;
        }

        // Todo: Figure out how to go back to relay transport.
        // private void ConfigureRelayTransport(RelayServerData relayData)
        // {
        //     var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //     transport.SetRelayServerData(relayData);

        //     _currentTransportMode = TransportMode.Relay;
        // }
    }
}
