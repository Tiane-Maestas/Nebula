
using Unity.Services.Authentication;
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
    // https://www.youtube.com/watch?v=-KDlEBfCBiU
    public class LobbyManager : Singleton<LobbyManager>
    {
        public Lobby Lobby { get; private set; } = null;
        public float PollPeriod = 1.5f; // Update Lobby every 1.5 seconds. (Minimum is 1 time per second)
        
        private float _pollTimer = 0f;
        private float _heartbeatTimer = 0f;
        private bool _isPolling = false;
        private bool _isHeartbeating = false;

        private void Update()
        {
            if (this.Lobby == null) return;

            _pollTimer += Time.deltaTime;
            if (_pollTimer >= PollPeriod)
            {
                _pollTimer = 0f;
                LobbyUpdatePoll();
            }

            // Heartbeat every 15 seconds. (30 second timeout w/ max of 5 pings per 30 seconds. https://docs.unity.com/ugs/manual/lobby/manual/rate-limits)
            if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.PlayerId == this.Lobby.HostId)
            {
                _heartbeatTimer += Time.deltaTime;
                if (_heartbeatTimer >= 15.0f)
                {
                    _heartbeatTimer = 0f;
                    LobbyHeartbeat();
                }
            }
        }

        private async void LobbyUpdatePoll()
        {
            if (this.Lobby == null || _isPolling) return;
            _isPolling = true;
            try
            {
                this.Lobby = await LobbyService.Instance.GetLobbyAsync(this.Lobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
            finally
            {
                _isPolling = false;
            }
        }

        private async void LobbyHeartbeat()
        {
            if (this.Lobby == null || !AuthenticationService.Instance.IsSignedIn || AuthenticationService.Instance.PlayerId != this.Lobby.HostId || _isHeartbeating) return;
            _isHeartbeating = true;
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(this.Lobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
            finally
            {
                _isHeartbeating = false;
            }
        }

        public async void Clear()
        {
            if (this.Lobby == null)
                return;

            string lobbyId = this.Lobby.Id;
            bool isHost = AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.PlayerId == this.Lobby.HostId;
            this.Lobby = null;
            _pollTimer = 0f;
            _heartbeatTimer = 0f;

            try
            {
                if (isHost)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                }
                else
                {
                    await LobbyService.Instance.RemovePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private void OnDestroy() { this.Clear(); }

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

        public async void UpdateLobby(UpdateLobbyOptions updateLobbyOptions)
        {
            try
            {
                Debug.LogFormat("Updating lobby...");
                this.Lobby = await Lobbies.Instance.UpdateLobbyAsync(this.Lobby.Id, updateLobbyOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        public async void UpdatePlayer(UpdatePlayerOptions updatePlayerOptions)
        {
            try
            {
                Debug.LogFormat("Updating player...");
                this.Lobby = await Lobbies.Instance.UpdatePlayerAsync(this.Lobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        public async void KickPlayer(string playerId)
        {
            try
            {
                Debug.LogFormat("Kicking player: " + playerId);
                await LobbyService.Instance.RemovePlayerAsync(this.Lobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }
}