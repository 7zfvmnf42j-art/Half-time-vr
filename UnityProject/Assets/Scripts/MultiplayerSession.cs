using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Initial online-session layer. Hosts or joins a direct connection using Unity Transport.
/// A relay/private-room-code service and replicated match players are later stages.
/// </summary>
public class MultiplayerSession : MonoBehaviour
{
    public static MultiplayerSession Instance { get; private set; }
    NetworkManager manager;
    UnityTransport transport;

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

    public bool StartHosting(ushort port = 7777)
    {
        if (manager == null || manager.IsListening) return false;
        transport.SetConnectionData("0.0.0.0", port, "127.0.0.1");
        return manager.StartHost();
    }

    public bool JoinHost(string address, ushort port = 7777)
    {
        if (manager == null || manager.IsListening) return false;
        if (string.IsNullOrWhiteSpace(address)) address = "127.0.0.1";
        transport.SetConnectionData(address.Trim(), port);
        return manager.StartClient();
    }

    public void Disconnect()
    {
        if (manager != null && manager.IsListening)
            manager.Shutdown();
    }

    public bool IsConnected
    {
        get { return manager != null && manager.IsListening; }
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log("HALFTIME player connected: " + clientId);
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("HALFTIME player disconnected: " + clientId);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (manager != null)
        {
            manager.OnClientConnectedCallback -= OnClientConnected;
            manager.OnClientDisconnectCallback -= OnClientDisconnected;
            if (manager.IsListening) manager.Shutdown();
        }
    }
}
