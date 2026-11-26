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

/// <summary>
/// Internet room service for HALFTIME VR using Unity Lobby and Relay.
/// Lobby codes are used by players; Relay carries the actual game connection.
/// </summary>
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

            Report("Creating a private match...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var options = new CreateLobbyOptions
            {
                IsPrivate = true,
                Data = new Dictionary<string, DataObject>
                {
                    { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("HALFTIME Match", MaxPlayers, options);

            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));
            if (!manager.StartHost())
                throw new Exception("The game host could not start.");

            InvokeRepeating(nameof(SendHeartbeat), 15f, 15f);
            Report("ROOM CODE: " + currentLobby.LobbyCode + " — share this with your friends.");
        }
        catch (Exception exception)
        {
            Report("Couldn't create room: " + exception.Message);
            await CleanupLobby();
        }
        finally { busy = false; }
    }

    public async void JoinRoom(string roomCode)
    {
        if (busy || (manager != null && manager.IsListening)) return;
        if (string.IsNullOrWhiteSpace(roomCode))
        {
            Report("Enter your friend's room code first.");
            return;
        }

        busy = true;
        try
        {
            Report("Finding room...");
            await EnsureServicesReady();
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode.Trim().ToUpperInvariant());

            if (currentLobby.Data == null ||
                !currentLobby.Data.TryGetValue("relayJoinCode", out DataObject relayData) ||
                string.IsNullOrWhiteSpace(relayData.Value))
                throw new Exception("That room doesn't have a valid connection yet.");

            Report("Joining match...");
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayData.Value);
            transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            if (!manager.StartClient())
                throw new Exception("The game couldn't connect to the host.");

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
        if (manager != null && manager.IsListening)
            manager.Shutdown();
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
        catch (Exception exception)
        {
            Debug.LogWarning("HALFTIME room cleanup: " + exception.Message);
        }
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

    void OnClientConnected(ulong clientId)
    {
        Report("Player connected. Players in session: " + manager.ConnectedClientsList.Count);
    }

    void OnClientDisconnected(ulong clientId)
    {
        Report("A player disconnected.");
    }

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