
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Nebula;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Nebula.Multiplayer
{
    // https://www.youtube.com/watch?v=-KDlEBfCBiU @ 23:47
    public class LobbyManager : Singleton<LobbyManager>
    {
        private Lobby _hostLobby = null;
        private NebulaTimer _heartBeat = new NebulaTimer();
        private async void LobbyHeartbeat()
        {
            if (_hostLobby == null) return;
            await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
        }
        private void OnDestroy() { _heartBeat.Cancel(); }

        public async void CreateLobby(System.Action<Lobby> createLobbyCallback, string lobbyName, int maxPlayers, CreateLobbyOptions createLobbyOptions = null)
        {
            try
            {
                Debug.Log("Creating lobby...");
                Lobby createdLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

                // Heartbeat every 15 seconds. (30 second timeout w/ max of 5 pings per 30 seconds. https://docs.unity.com/ugs/manual/lobby/manual/rate-limits)
                _hostLobby = createdLobby;
                _heartBeat.SetInterval(LobbyHeartbeat, 15.0f);

                Debug.Log("Lobby Created as: " + createdLobby.Name + "(" + createdLobby.MaxPlayers + ")");
                createLobbyCallback(createdLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                createLobbyCallback(null);
            }
        }

        public async void ListLobbies(System.Action<List<Lobby>> listLobbiesCallback, QueryLobbiesOptions queryLobbiesOptions = null)
        {
            try
            {
                Debug.Log("Querying for lobbies...");
                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
                listLobbiesCallback(queryResponse.Results);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                listLobbiesCallback(null);
            }
        }

        public async void JoinLobby(System.Action<Lobby> joinLobbyCallback, Lobby lobby)
        {
            try
            {
                Debug.LogFormat("Joining lobby {0}...", lobby.Name);
                Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
                joinLobbyCallback(joinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                joinLobbyCallback(null);
            }
        }

        public async void JoinLobbyByCode(System.Action<Lobby> joinLobbyCallback, string lobbyCode)
        {
            try
            {
                Debug.LogFormat("Joining private lobby {0}...", lobbyCode);
                Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
                joinLobbyCallback(joinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                joinLobbyCallback(null);
            }
        }

        public async void QuickJoinLobby(System.Action<Lobby> joinLobbyCallback, QuickJoinLobbyOptions quickJoinLobbyOptions = null)
        {
            try
            {
                Debug.LogFormat("Looking to quick join lobby...");
                Lobby joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
                joinLobbyCallback(joinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                joinLobbyCallback(null);
            }
        }
    }
}