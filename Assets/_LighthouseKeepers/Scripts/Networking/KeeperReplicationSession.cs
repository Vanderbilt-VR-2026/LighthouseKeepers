using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Replicates development-environment players and props on the existing crew runner.</summary>
[DefaultExecutionOrder(10000)]
public sealed partial class KeeperReplicationSession : MonoBehaviour, INetworkRunnerCallbacks
{
    public const string Protocol = "LK-Coop-1";
    FusionLobbyTransport transport;
    NetworkRunner runner;
    readonly Dictionary<PlayerRef, LobbyPlayer> crew = new();
    readonly HashSet<PlayerRef> worldReady = new();
    public static KeeperReplicationSession Instance { get; private set; }
    public bool IsOnline => transport && transport.IsOnline && runner && runner.IsRunning;
    public bool IsHost => IsOnline && runner.IsServer;
    public bool IsConnecting => transport && transport.IsBusy;
    public string LastError => transport ? transport.LastError : "";
    public string RoomName
    {
        get => transport ? transport.SessionName : "LighthouseKeepersRoom";
        set { ResolveTransport(); if (transport) transport.SessionName = value; }
    }
    public string LocalPlayerId => transport ? transport.LocalPlayerId : "";
    public IReadOnlyList<LobbyPlayer> Players => transport ? transport.Players : Array.Empty<LobbyPlayer>();
    public NetworkRunner SessionRunner => runner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterEnvironmentPreview()
    {
        Instance = null;
        SceneManager.sceneLoaded -= EnsureSessionControls;
        SceneManager.sceneLoaded += EnsureSessionControls;
    }
    static void EnsureSessionControls(Scene scene, LoadSceneMode mode)
    {
        // Storm Watch has its own authoritative gameplay link; do not install a second gameplay layer there.
        if (scene.name == "LK_Bootstrap") GetOrCreate();
    }
    public static KeeperReplicationSession GetOrCreate()
    {
        if (Instance) return Instance;
        return new GameObject("Environment player and prop replication").AddComponent<KeeperReplicationSession>();
    }
    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneLoaded;
        ResolveTransport();
        gameObject.AddComponent<MultiplayerPanel>();
    }
    void ResolveTransport()
    {
        if (transport) return;
        transport = FindAnyObjectByType<FusionLobbyTransport>();
        if (!transport) transport = new GameObject("Fusion crew session").AddComponent<FusionLobbyTransport>();
        transport.ConnectionChanged += ConnectionChanged;
        transport.StatusChanged += SynchronizeSession;
        transport.PlayersChanged += SyncCrew;
        SynchronizeSession();
    }
    void ConnectionChanged(bool online) => SynchronizeSession();
    void SynchronizeSession()
    {
        var current = transport && transport.IsOnline && transport.Runner && transport.Runner.IsRunning ? transport.Runner : null;
        if (runner == current) return;
        if (runner) runner.RemoveCallbacks(this);
        runner = current;
        crew.Clear(); worldReady.Clear();
        ResetReplication();
        if (runner)
        {
            runner.AddCallbacks(this);
            SyncCrew();
        }
    }
    void SyncCrew()
    {
        if (!runner || !runner.IsRunning) return;
        var active = runner.ActivePlayers.ToArray();
        foreach (var player in crew.Keys.Except(active).ToArray()) crew.Remove(player);
        foreach (var player in active)
        {
            string id = PlayerId(player);
            crew[player] = Players.FirstOrDefault(p => p.Id == id) ?? new LobbyPlayer(id, "Keeper", player == runner.LocalPlayer);
        }
        PruneAvatars();
    }
    static string PlayerId(PlayerRef player) => player.PlayerId.ToString(System.Globalization.CultureInfo.InvariantCulture);
    public bool HostGame(int capacity)
    {
        ResolveTransport();
        return transport.HostGame(Mathf.Clamp(capacity, 2, GrabAuthority.MaximumPlayers));
    }
    public bool JoinGame(string room) { ResolveTransport(); return transport.JoinGame(room); }
    public void SetLocalName(string name) { ResolveTransport(); transport.SetLocalName(name); }
    public void LeaveGame() { if (transport) transport.LeaveGame(); }
    void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= SceneLoaded;
        if (transport)
        {
            transport.ConnectionChanged -= ConnectionChanged;
            transport.StatusChanged -= SynchronizeSession;
            transport.PlayersChanged -= SyncCrew;
        }
        if (runner) runner.RemoveCallbacks(this);
        Instance = null;
        ClearWorld();
        // FusionLobbyTransport owns the runner's lifetime. Never shut down another component's session here.
    }
    public void OnPlayerJoined(NetworkRunner source, PlayerRef player) { if (source == runner) SyncCrew(); }
    public void OnPlayerLeft(NetworkRunner source, PlayerRef player)
    {
        if (source != runner) return;
        RemoveAvatar(player);
        crew.Remove(player); worldReady.Remove(player); acceptedPoses.Remove(player);
        if (source.IsServer)
            foreach (var id in authority.ReleasePlayer(player.RawEncoded)) PublishObject(id, true);
    }
    public void OnShutdown(NetworkRunner source, ShutdownReason reason) { if (source == runner) SynchronizeSession(); }
    public void OnDisconnectedFromServer(NetworkRunner source, NetDisconnectReason reason) { }
    // The transport alone accepts/refuses peers and owns scene changes, startup, shutdown and roster traffic.
    public void OnConnectRequest(NetworkRunner source, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectedToServer(NetworkRunner source) { }
    public void OnConnectFailed(NetworkRunner source, NetAddress address, NetConnectFailedReason reason) { }
    public void OnHostMigration(NetworkRunner source, HostMigrationToken token) { }
    public void OnInput(NetworkRunner source, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner source, PlayerRef player, NetworkInput input) { }
    public void OnObjectExitAOI(NetworkRunner source, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner source, NetworkObject obj, PlayerRef player) { }
    public void OnUserSimulationMessage(NetworkRunner source, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner source, List<SessionInfo> sessions) { }
    public void OnCustomAuthenticationResponse(NetworkRunner source, Dictionary<string, object> data) { }
    public void OnSceneLoadDone(NetworkRunner source) { }
    public void OnSceneLoadStart(NetworkRunner source) { }
    public void OnReliableDataReceived(NetworkRunner source, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner source, PlayerRef player, ReliableKey key, float progress) { }
}
