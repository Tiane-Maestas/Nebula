using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Nebula;

namespace Nebula.Multiplayer
{
    public class RelayManager : Singleton<RelayManager>
    {
        public void LeaveRelay()
        {
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        private void OnDestroy()
        {
            LeaveRelay();
        }

        public async void CreateRelay(System.Action<string> joinCodeCallback)
        {
            try
            {
                LeaveRelay();

                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1); // 2 Max Players. (For Now)

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();

                Debug.Log("Relay Join Code: " + joinCode);
                joinCodeCallback(joinCode);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
                joinCodeCallback(null);
            }
        }

        public async void JoinRelay(string joinCode)
        {
            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("[RelayManager] Cannot join Relay with null or empty join code.");
                return;
            }

            try
            {
                LeaveRelay();

                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }
}
