using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class LobbySummary
{
    public string Id;
    public string Name;
    public int Players;
    public int Capacity;
}

/// <summary>Online rooms for HALFTIME VR using Unity Lobby and Relay.</summary>
public class MultiplayerSession : MonoBehaviour
{
    public static MultiplayerSession Instance { get; private set; }
    const int MaxPlayers = 8;
    NetworkManager manager;
    UnityTransport transport;
    Lobby currentLobby;
    bool servicesReady;
    bool busy;

    public event Action<string> StatusChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateNetworkManager();
    }

    void CreateNetworkManager()
    {
        var networkObject = new GameObject("HALFTIME Network Manager");
        DontDestroyOnLoad(networkObject);
        manager = networkObject.AddComponent<NetworkManager>();
        transport = networkObject.AddComponent<UnityTransport>();
        manager.NetworkConfig = new NetworkConfig
        {
            NetworkTransport = transport,
            TickRate = 30,
            LoadSceneTimeOut = 120
        };
        manager.OnClientConnectedCallback += OnClientConnected;
        manager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    async Task EnsureServicesReady()
    {
        if (!servicesReady)
        {
            await UnityServices.InitializeAsync();
            servicesReady = true;
        }
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRoom()
    {
        if (busy || (manager != null && manager.IsListening)) return;
        busy = true;
        try
        {
            Report("Connecting to online services...");
            await EnsureServicesReady();
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("HALFTIME Public Match", MaxPlayers, options);
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));
            if (!manager.StartHost()) throw new Exception("The game host could not start.");
            InvokeRepeating(nameof(SendHeartbeat), 15f, 15f);
            Report("PUBLIC ROOM CREATED — CODE: " + currentLobby.LobbyCode);
        }
        catch (Exception exception)
        {
            Report("Couldn't create public room: " + exception.Message);
            await CleanupLobby();
        }
        finally { busy = false; }
    }

    public async void RefreshPublicRooms(Action<List<LobbySummary>> completed)
    {
        try
        {
            await EnsureServicesReady();
            var options = new QueryLobbiesOptions
            {
                Count = 20,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            var rooms = new List<LobbySummary>();
            foreach (Lobby lobby in response.Results)
            {
                if (lobby == null || lobby.Players == null) continue;
                rooms.Add(new LobbySummary
                {
                    Id = lobby.Id,
                    Name = lobby.Name,
                    Players = lobby.Players.Count,
                    Capacity = lobby.MaxPlayers
                });
            }
            Report(rooms.Count == 0 ? "No public rooms found. Create one!" : "Found " + rooms.Count + " public room(s).");
            completed?.Invoke(rooms);
        }
        catch (Exception exception)
        {
            Report("Couldn't load public rooms: " + exception.Message);
            completed?.Invoke(new List<LobbySummary>());
        }
    }

    public async void JoinPublicRoom(string lobbyId)
    {
        if (busy || (manager != null && manager.IsListening)) return;
        if (string.IsNullOrWhiteSpace(lobbyId)) return;
        busy = true;
        try
        {
            await EnsureServicesReady();
            Report("Joining public room...");
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            if (currentLobby.Data == null ||
                !currentLobby.Data.TryGetValue("relayJoinCode", out DataObject relayData) ||
                string.IsNullOrWhiteSpace(relayData.Value))
                throw new Exception("This room has no active connection.");
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayData.Value);
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));
            if (!manager.StartClient()) throw new Exception("The game couldn't connect to the host.");
            Report("Connecting to " + currentLobby.Name + "...");
        }
        catch (Exception exception)
        {
            Report("Couldn't join public room: " + exception.Message);
            await CleanupLobby();
        }
        finally { busy = false; }
    }

    public async void JoinRoom(string roomCode)
    {
        if (busy || (manager != null && manager.IsListening)) return;
        if (string.IsNullOrWhiteSpace(roomCode)) { Report("Enter a room code first."); return; }
        busy = true;
        try
        {
            await EnsureServicesReady();
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode.Trim().ToUpperInvariant());
            if (currentLobby.Data == null ||
                !currentLobby.Data.TryGetValue("relayJoinCode", out DataObject relayData) ||
                string.IsNullOrWhiteSpace(relayData.Value))
                throw new Exception("That room doesn't have a valid connection yet.");
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayData.Value);
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));
            if (!manager.StartClient()) throw new Exception("The game couldn't connect to the host.");
            Report("Connecting to room " + currentLobby.LobbyCode + "...");
        }
        catch (Exception exception)
        {
            Report("Couldn't join room: " + exception.Message);
            await CleanupLobby();
        }
        finally { busy = false; }
    }

    public async void Disconnect()
    {
        CancelInvoke(nameof(SendHeartbeat));
        if (manager != null && manager.IsListening) manager.Shutdown();
        await CleanupLobby();
        Report("Disconnected from the match.");
    }

    async Task CleanupLobby()
    {
        if (currentLobby == null || !servicesReady || !AuthenticationService.Instance.IsSignedIn)
        {
            currentLobby = null;
            return;
        }
        try
        {
            if (currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
            else
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (Exception exception) { Debug.LogWarning("HALFTIME room cleanup: " + exception.Message); }
        currentLobby = null;
    }

    async void SendHeartbeat()
    {
        if (currentLobby == null) return;
        try { await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id); }
        catch (Exception exception) { Debug.LogWarning("HALFTIME room heartbeat: " + exception.Message); }
    }

    void Report(string message)
    {
        Debug.Log("[HALFTIME Online] " + message);
        StatusChanged?.Invoke(message);
    }

    void OnClientConnected(ulong clientId) { Report("Player connected. Players in session: " + manager.ConnectedClientsList.Count); }
    void OnClientDisconnected(ulong clientId) { Report("A player disconnected."); }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        CancelInvoke(nameof(SendHeartbeat));
        if (manager != null)
        {
            manager.OnClientConnectedCallback -= OnClientConnected;
            manager.OnClientDisconnectCallback -= OnClientDisconnected;
            if (manager.IsListening) manager.Shutdown();
        }
    }
}