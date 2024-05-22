
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
    // https://www.youtube.com/watch?v=-KDlEBfCBiU @ 40:00
    public class LobbyManager : Singleton<LobbyManager>
    {
        public Lobby Lobby { get; private set; } = null;
        public float PollPeriod = 5.0f; // Update Lobby every 5 seconds. (Minimum is 1 time per second)
        private NebulaTimer _updatePoll = new NebulaTimer();
        private NebulaTimer _heartBeat = new NebulaTimer();
        private async void LobbyUpdatePoll()
        {
            if (this.Lobby == null) return;
            this.Lobby = await LobbyService.Instance.GetLobbyAsync(this.Lobby.Id);
        }
        private async void LobbyHeartbeat()
        {
            if (this.Lobby == null) return;
            await LobbyService.Instance.SendHeartbeatPingAsync(this.Lobby.Id);
        }
        // Heartbeat every 15 seconds. (30 second timeout w/ max of 5 pings per 30 seconds. https://docs.unity.com/ugs/manual/lobby/manual/rate-limits)
        private void Start() { _updatePoll.SetInterval(LobbyUpdatePoll, PollPeriod); _heartBeat.SetInterval(LobbyHeartbeat, 15.0f); }
        private void OnDestroy() { _updatePoll.Cancel(); _heartBeat.Cancel(); } // Todo: Check to make sure these stop when switching scenes and going to main menu!

        public async void CreateLobby(System.Action<Lobby> createLobbyCallback, string lobbyName, int maxPlayers, CreateLobbyOptions createLobbyOptions = null)
        {
            try
            {
                Debug.Log("Creating lobby...");
                this.Lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

                Debug.Log("Lobby Created as: " + this.Lobby.Name + "(" + this.Lobby.MaxPlayers + ")");
                createLobbyCallback(this.Lobby);
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
                this.Lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
                joinLobbyCallback(this.Lobby);
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
                this.Lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
                joinLobbyCallback(this.Lobby);
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
                this.Lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
                joinLobbyCallback(this.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                joinLobbyCallback(null);
            }
        }

        public async void UpdateLobby(System.Action<Lobby> updateLobbyCallback, Lobby lobby, UpdateLobbyOptions updateLobbyOptions)
        {
            try
            {
                Debug.LogFormat("Updating lobby...");
                this.Lobby = await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, updateLobbyOptions);
                updateLobbyCallback(this.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                updateLobbyCallback(null);
            }
        }
    }
}