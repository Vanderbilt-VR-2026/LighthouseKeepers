using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>One persistent host/client session. Only the local XR rig owns cameras and input.</summary>
[DefaultExecutionOrder(10000)]
public sealed partial class FusionLauncher : MonoBehaviour, ISessionTransport, INetworkRunnerCallbacks
{
    public const string Protocol = "LK-Coop-1";
    [SerializeField] string roomName = "LighthouseKeepersRoom";
    NetworkRunner runner;
    bool online, connecting, stopping, watchStarted;
    int generation;
    string pendingName = "Keeper";
    readonly Dictionary<PlayerRef, LobbyPlayer> crew = new();
    readonly List<LobbyPlayer> roster = new();
    readonly HashSet<PlayerRef> worldReady = new();
    public static FusionLauncher Instance { get; private set; }
    public bool IsOnline => online && runner && runner.IsRunning;
    public bool IsHost => IsOnline && runner.IsServer;
    public bool IsConnecting => connecting || stopping;
    public string LastError { get; private set; } = "";
    public string RoomName { get => roomName; set { if (!IsOnline && !IsConnecting) roomName = NormalizeRoom(value); } }
    public string LocalPlayerId => runner ? runner.LocalPlayer.RawEncoded.ToString() : "";
    public IReadOnlyList<LobbyPlayer> Players => roster;
    public event Action PlayersChanged;
    public event Action<bool> ConnectionChanged;
    public event Action GameStarting;
    public NetworkRunner SessionRunner => runner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterTransport()
    {
        Instance = null;
        LobbyTransportProvider.Create = () => GetOrCreate();
        SceneManager.sceneLoaded -= EnsureSessionControls;
        SceneManager.sceneLoaded += EnsureSessionControls;
    }
    static void EnsureSessionControls(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LK_Bootstrap") GetOrCreate();
    }
    public static FusionLauncher GetOrCreate()
    {
        if (Instance) return Instance;
        return new GameObject("Lighthouse multiplayer").AddComponent<FusionLauncher>();
    }
    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneLoaded;
        gameObject.AddComponent<MultiplayerPanel>();
    }
    void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= SceneLoaded;
        Instance = null;
        if (runner) { runner.RemoveCallbacks(this); _ = runner.Shutdown(); }
        ClearWorld();
    }
    static string NormalizeRoom(string value)
    {
        value = string.IsNullOrWhiteSpace(value) ? "LighthouseKeepersRoom" : value.Trim();
        return value.Length > 32 ? value.Substring(0, 32) : value;
    }
    static string NormalizeName(string value)
    {
        value = string.IsNullOrWhiteSpace(value) ? "Keeper" : value.Trim();
        return value.Length > 24 ? value.Substring(0, 24) : value;
    }
    public void StartHost() => HostGame(GrabAuthority.MaximumPlayers);
    public void JoinSession() => JoinGame(RoomName);
    public bool HostGame(int maxPlayers) => BeginConnect(GameMode.Host);
    public bool JoinGame(string address)
    {
        if (IsOnline || IsConnecting || string.IsNullOrWhiteSpace(address)) return false;
        RoomName = address;
        return BeginConnect(GameMode.Client);
    }
    bool BeginConnect(GameMode mode)
    {
        if (IsOnline || IsConnecting) return false;
        LastError = "";
        if (!PhotonAppSettings.TryGetGlobal(out var settings) || string.IsNullOrWhiteSpace(settings.AppSettings.AppIdFusion))
        {
            LastError = "Set a Photon Fusion App ID in PhotonAppSettings before connecting.";
            ConnectionChanged?.Invoke(false);
            return false;
        }
        connecting = true;
        watchStarted = SceneManager.GetSceneByName("LK_Bootstrap").isLoaded;
        worldReady.Clear();
        ResetReplication();
        ConnectionChanged?.Invoke(false);
        _ = Connect(mode, ++generation);
        return true;
    }
    async Task Connect(GameMode mode, int attempt)
    {
        var runnerObject = new GameObject("Fusion Network Runner");
        runnerObject.transform.SetParent(transform);
        var candidate = runnerObject.AddComponent<NetworkRunner>();
        runner = candidate;
        candidate.ProvideInput = false;
        candidate.AddCallbacks(this);
        try
        {
            var photon = PhotonAppSettings.Global.AppSettings.GetCopy();
            // Best-region selection can split a named crew across regions. Keep a shared default.
            if (string.IsNullOrWhiteSpace(photon.FixedRegion)) photon.FixedRegion = "us";
            photon.AppVersion = string.IsNullOrEmpty(photon.AppVersion) ? Protocol : photon.AppVersion + "-" + Protocol;
            // EnvironmentBootstrap owns additive content; Fusion must not replace the scene and its XR rig.
            var result = await candidate.StartGame(new StartGameArgs
            {
                GameMode = mode, SessionName = RoomName, PlayerCount = GrabAuthority.MaximumPlayers,
                EnableClientSessionCreation = false, ConnectionToken = Encoding.UTF8.GetBytes(Protocol),
                IsVisible = false, CustomPhotonAppSettings = photon
            });
            if (attempt != generation || runner != candidate) return;
            connecting = false;
            if (!result.Ok)
            {
                LastError = "Could not connect: " + result.ShutdownReason + ". Check the room name, region and connection.";
                stopping = true;
                await StopCandidate(candidate);
                stopping = false;
                ConnectionChanged?.Invoke(false);
                return;
            }
            online = true;
            RefreshPhysics();
            if (candidate.IsServer)
            {
                foreach (var player in candidate.ActivePlayers) AddPlayer(player);
                SetProfile(candidate.LocalPlayer, pendingName, false);
            }
            else KeeperMessages.RPC_Profile(candidate, PlayerRef.None, pendingName, false);
            ConnectionChanged?.Invoke(true);
        }
        catch (Exception error)
        {
            if (attempt != generation) return;
            connecting = false;
            LastError = "Connection failed: " + error.Message;
            stopping = true;
            try { await StopCandidate(candidate); }
            finally { stopping = false; ConnectionChanged?.Invoke(false); }
        }
    }
    async Task StopCandidate(NetworkRunner candidate)
    {
        if (candidate)
        {
            candidate.RemoveCallbacks(this);
            await candidate.Shutdown();
            if (candidate) Destroy(candidate.gameObject);
        }
        if (runner == candidate) runner = null;
    }
    public void LeaveGame()
    {
        if (stopping) return;
        ++generation;
        connecting = online = false;
        stopping = true;
        var old = runner;
        runner = null;
        crew.Clear(); roster.Clear(); worldReady.Clear();
        ResetReplication();
        LastError = "";
        ConnectionChanged?.Invoke(false); PlayersChanged?.Invoke();
        _ = FinishLeave(old);
    }
    async Task FinishLeave(NetworkRunner old)
    {
        try { await StopCandidate(old); }
        catch (Exception error) { Debug.LogWarning("Multiplayer shutdown: " + error.Message); }
        finally { stopping = false; ConnectionChanged?.Invoke(false); }
    }
    public void SetLocalName(string name)
    {
        pendingName = NormalizeName(name);
        if (IsOnline) SendProfile(roster.FirstOrDefault(p => p.Id == LocalPlayerId)?.IsReady == true);
    }
    public void SetLocalReady(bool ready) { if (IsOnline) SendProfile(ready); }
    void SendProfile(bool ready)
    {
        if (IsHost) SetProfile(runner.LocalPlayer, pendingName, ready);
        else KeeperMessages.RPC_Profile(runner, PlayerRef.None, pendingName, ready);
    }
    internal void SetProfile(PlayerRef source, string name, bool ready)
    {
        if (!runner || !runner.IsServer || !crew.TryGetValue(source, out var member)) return;
        member.DisplayName = NormalizeName(name); member.IsReady = ready;
        BroadcastRoster();
    }
    void AddPlayer(PlayerRef player)
    {
        if (!crew.ContainsKey(player)) crew.Add(player, new LobbyPlayer(player.RawEncoded.ToString(),
            player == runner.LocalPlayer ? pendingName : "Keeper", player == runner.LocalPlayer));
    }
    [Serializable] internal sealed class RosterMessage { public LobbyPlayer[] Players; public string Room; }
    void BroadcastRoster()
    {
        if (!runner || !runner.IsRunning || !runner.IsServer) return;
        var message = new RosterMessage { Players = crew.OrderBy(p => p.Key.RawEncoded).Select(p => p.Value).ToArray(), Room = RoomName };
        KeeperMessages.RPC_Roster(runner, JsonUtility.ToJson(message));
    }
    internal void ApplyRoster(string json)
    {
        if (json == null || json.Length > 2048) return;
        var message = JsonUtility.FromJson<RosterMessage>(json);
        if (message?.Players == null || message.Players.Length > GrabAuthority.MaximumPlayers) return;
        roster.Clear(); roster.AddRange(message.Players);
        roomName = NormalizeRoom(message.Room);
        PruneAvatars();
        PlayersChanged?.Invoke();
    }
    public bool KickPlayer(string id)
    {
        if (!IsHost || id == LocalPlayerId) return false;
        var match = crew.Keys.FirstOrDefault(p => p.RawEncoded.ToString() == id);
        if (match == PlayerRef.None) return false;
        runner.Disconnect(match);
        return true;
    }
    public bool BeginGame(string sceneName, bool requireAllReady = true)
    {
        if (!IsHost || roster.Count == 0 || (requireAllReady && roster.Any(p => !p.IsReady)) || !Application.CanStreamedLevelBeLoaded(sceneName)) return false;
        if (sceneName != "LK_Bootstrap") { LastError = "Crew play starts in LK_Bootstrap."; return false; }
        watchStarted = true;
        foreach (var player in runner.ActivePlayers)
            if (player != runner.LocalPlayer) KeeperMessages.RPC_BeginWatch(runner, player);
        EnterWatch();
        return true;
    }
    internal void EnterWatch()
    {
        if (SceneManager.GetSceneByName("LK_Bootstrap").isLoaded) return;
        GameStarting?.Invoke();
        SceneManager.LoadScene("LK_Bootstrap", LoadSceneMode.Single);
    }
    public void OnPlayerJoined(NetworkRunner source, PlayerRef player)
    {
        if (source != runner || !source.IsServer) return;
        AddPlayer(player);
        BroadcastRoster();
        if (watchStarted && player != source.LocalPlayer) KeeperMessages.RPC_BeginWatch(source, player);
    }
    public void OnPlayerLeft(NetworkRunner source, PlayerRef player)
    {
        if (source != runner) return;
        RemoveAvatar(player);
        if (!source.IsServer) return;
        crew.Remove(player); worldReady.Remove(player); acceptedPoses.Remove(player);
        foreach (var id in authority.ReleasePlayer(player.RawEncoded)) PublishObject(id, true);
        BroadcastRoster();
    }
    public void OnConnectRequest(NetworkRunner source, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        if (source == runner && source.IsServer && source.ActivePlayers.Count() < GrabAuthority.MaximumPlayers &&
            token != null && token.SequenceEqual(Encoding.UTF8.GetBytes(Protocol))) request.Accept();
        else request.Refuse();
    }
    public void OnShutdown(NetworkRunner source, ShutdownReason reason)
    {
        if (source != runner) return;
        online = connecting = false;
        LastError = "Session ended: " + reason + ". You can host or join again.";
        crew.Clear(); roster.Clear(); worldReady.Clear();
        ResetReplication();
        runner = null;
        if (source) Destroy(source.gameObject);
        ConnectionChanged?.Invoke(false); PlayersChanged?.Invoke();
    }
    public void OnDisconnectedFromServer(NetworkRunner source, NetDisconnectReason reason)
    {
        if (source == runner) LastError = "Disconnected: " + reason;
    }
    public void OnConnectFailed(NetworkRunner source, NetAddress address, NetConnectFailedReason reason)
    {
        if (source == runner) LastError = "Connection failed: " + reason;
    }
    public void OnHostMigration(NetworkRunner source, HostMigrationToken token) { if (source == runner) LeaveGame(); }
    public void OnConnectedToServer(NetworkRunner source) { }
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
