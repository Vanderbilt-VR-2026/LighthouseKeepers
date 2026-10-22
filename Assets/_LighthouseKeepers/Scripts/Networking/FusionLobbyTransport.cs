using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using LighthouseKeepers.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Networking
{
    [DefaultExecutionOrder(-600)]
    [DisallowMultipleComponent]
    public sealed class FusionLobbyTransport : MonoBehaviour, ISessionLobbyTransport, INetworkRunnerCallbacks
    {
        public static Func<string> ResolveGameplayScene { private get; set; }
        public static Func<string,string> ResolveSessionRoom { private get; set; }
        public static string GameplaySceneName => ResolveGameplayScene?.Invoke() ?? throw new InvalidOperationException("Canonical ship routing is not registered.");
        public const string LobbySceneName = "LK_NetworkLobby";
        static string pendingLobbyNotice;
        public NetworkRunner Runner => runner;
        public bool IsInGame => enteredGame;

        [SerializeField] string sessionName = "LighthouseKeepersRoom";
        [SerializeField] string lobbySceneName = "LK_NetworkLobby";
        readonly FusionLobbyState state = new();
        NetworkRunner runner;
        CancellationTokenSource startup;
        bool online, busy, leaving, localReady, enteredGame;
        string pendingName = "Keeper";
        string initialScenePath;
        string gameSceneName;
        int requestSequence, transferSequence;

        public bool IsOnline => online;
        public bool IsHost => online && runner && runner.IsServer;
        public bool IsBusy => busy;
        public bool IsStarting => state.Starting;
        public int MaxPlayers => state.Capacity;
        public string LocalPlayerId => runner && runner.IsRunning ? Id(runner.LocalPlayer) : "";
        public IReadOnlyList<LobbyPlayer> Players => state.Players;
        public string LastError { get; private set; } = "";
        public string SessionName { get => sessionName; set { if (!online && !busy) sessionName = value?.Trim(); } }
        public event Action PlayersChanged;
        public event Action<bool> ConnectionChanged;
        public event Action StatusChanged;
        public event Action GameStarting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterDefaultTransport() => LobbyManager.CreateDefaultTransport = () =>
            FindAnyObjectByType<FusionLobbyTransport>() ?? new GameObject("Fusion crew session").AddComponent<FusionLobbyTransport>();

        void Awake()
        {
            // The network preview authors this as its own root, separate from the
            // board and XR rig, so only the session survives the scene transition.
            initialScenePath = gameObject.scene.path;
            if (gameObject.scene.name == lobbySceneName && !string.IsNullOrEmpty(pendingLobbyNotice))
            {
                LastError = pendingLobbyNotice;
                pendingLobbyNotice = null;
            }
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
        void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!online || (scene.name != "LK_Bootstrap" && scene.name != GameplaySceneName)) return;
            // Scene arrival is authoritative even if the separate reliable lobby
            // snapshot has not arrived yet. Bind gameplay before its Start runs.
            enteredGame = true;
            gameSceneName = scene.name;
            // William's original scene includes Fusion's debug launcher and its
            // dormant runner template. Keep those authored objects for isolated
            // prototype work, but prevent a second session alongside the crew.
            foreach (var prototype in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<FusionBootstrap>(true)))
            {
                prototype.enabled = false;
                var gui = prototype.GetComponent<FusionBootstrapDebugGUI>();
                if (gui) gui.enabled = false;
                if (prototype.RunnerPrefab && prototype.RunnerPrefab != runner && !prototype.RunnerPrefab.IsRunning)
                    prototype.RunnerPrefab.gameObject.SetActive(false);
            }
        }
        public bool HostGame(int maxPlayers) => BeginConnection(GameMode.Host, sessionName, maxPlayers);
        public bool JoinGame(string address) => BeginConnection(GameMode.Client, address, 4);

        bool BeginConnection(GameMode mode, string room, int capacity)
        {
            if (online || busy || runner) return false;
            if (!LobbyProtocol.ValidRoom(room)) return Fail("Enter a room name (1–64 characters).");
            sessionName = room.Trim();
            state.Reset(capacity); localReady = false; requestSequence = transferSequence = 0;
            LastError = ""; gameSceneName = null; leaving = enteredGame = false; busy = true;
            startup = new CancellationTokenSource();
            StatusChanged?.Invoke();
            _ = Connect(mode, startup.Token);
            return true; // Accepted; completion is reported through events.
        }
        async Task Connect(GameMode mode, CancellationToken cancellation)
        {
            var runnerObject = new GameObject("Fusion Network Runner");
            var current = runnerObject.AddComponent<NetworkRunner>();
            runner = current;
            current.ProvideInput = false; // Storm Watch sends bounded input on its own reliable-data channel.
            current.AddCallbacks(this);
            var sceneManager = runnerObject.AddComponent<NetworkSceneManagerDefault>();
            try
            {
                int index = SceneUtility.GetBuildIndexByScenePath(initialScenePath ?? "");
                if (index < 0) index = SceneUtility.GetBuildIndexByScenePath(
                    "Assets/_LighthouseKeepers/Scenes/Development/" + lobbySceneName + ".unity");
                if (index < 0) throw new InvalidOperationException("Add the networking lobby to Build Settings.");
                var scenes = new NetworkSceneInfo();
                scenes.AddSceneRef(SceneRef.FromIndex(index), LoadSceneMode.Single);
                var result = await current.StartGame(new StartGameArgs
                {
                    GameMode = mode, SessionName = ResolveSessionRoom?.Invoke(sessionName) ?? sessionName, PlayerCount = state.Capacity,
                    Scene = scenes, SceneManager = sceneManager, IsVisible = false,
                    EnableClientSessionCreation = false, StartGameCancellationToken = cancellation
                });
                if (cancellation.IsCancellationRequested || !this) return;
                if (!result.Ok) { Fail("Could not connect: " + result.ShutdownReason + ". Check the room name and Photon settings."); return; }
                online = true;
                if (current.IsServer)
                {
                    foreach (var player in current.ActivePlayers) state.Add(Id(player), player == current.LocalPlayer);
                }
                SendLocalState();
                ConnectionChanged?.Invoke(true);
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { if (this && !leaving) Fail("Could not connect: " + e.Message); }
            finally
            {
                if (!online || cancellation.IsCancellationRequested)
                {
                    await ReleaseRunner(current);
                    if (this) ClearConnection();
                }
                if (this)
                {
                    busy = false;
                    startup?.Dispose(); startup = null;
                    StatusChanged?.Invoke();
                }
            }
        }
        [ContextMenu("Leave crew session")]
        public void LeaveGame()
        {
            if (leaving || (!online && !busy && !runner)) return;
            leaving = true; LastError = ""; startup?.Cancel();
            if (busy) { StatusChanged?.Invoke(); return; }
            _ = Disconnect();
        }
        async Task Disconnect()
        {
            busy = true; StatusChanged?.Invoke();
            await ReleaseRunner(runner);
            if (!this) return;
            ClearConnection(); busy = leaving = false; StatusChanged?.Invoke();
            ReturnToLobby();
        }
        async Task ReleaseRunner(NetworkRunner current)
        {
            if (!current) return;
            current.RemoveCallbacks(this);
            try { await current.Shutdown(); }
            catch (Exception e) { Debug.LogWarning("Fusion shutdown: " + e.Message); }
            finally
            {
                if (current) Destroy(current.gameObject);
                if (runner == current) runner = null;
            }
        }
        void ClearConnection()
        {
            bool wasOnline = online;
            online = false; state.Reset(4); localReady = false;
            if (wasOnline) ConnectionChanged?.Invoke(false);
            PlayersChanged?.Invoke();
        }
        void ReturnToLobby()
        {
            if (!enteredGame || !Application.CanStreamedLevelBeLoaded(lobbySceneName)) return;
            pendingLobbyNotice = LastError;
            Destroy(gameObject);
            SceneManager.LoadScene(lobbySceneName);
        }
        public void SetLocalReady(bool ready)
        {
            if (!online || busy || state.Starting) return;
            localReady = ready; SendLocalState();
        }
        public void SetLocalName(string displayName)
        {
            pendingName = LobbyProtocol.DisplayName(displayName);
            if (online && !busy && !state.Starting) SendLocalState();
        }
        void SendLocalState()
        {
            if (!runner || !runner.IsRunning) return;
            var request = new LobbyPacket { revision = ++requestSequence, name = pendingName, ready = localReady };
            if (runner.IsServer)
            {
                if (state.ApplyRequest(LocalPlayerId, request)) PublishRoster();
            }
            else runner.SendReliableDataToServer(NextKey(), LobbyProtocol.Encode(request));
        }
        static string Id(PlayerRef player) => player.PlayerId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ReliableKey NextKey() => ReliableKey.FromInts(LobbyProtocol.Channel, 1, 0, ++transferSequence);
        void PublishRoster()
        {
            if (!runner || !runner.IsRunning || !runner.IsServer) return;
            var bytes = LobbyProtocol.Encode(state.Snapshot());
            foreach (var player in runner.ActivePlayers)
                if (player != runner.LocalPlayer) runner.SendReliableDataToPlayer(player, NextKey(), bytes);
            PlayersChanged?.Invoke();
        }
        public bool KickPlayer(string playerId)
        {
            if (!IsHost || busy || state.Starting || playerId == LocalPlayerId) return false;
            foreach (var player in runner.ActivePlayers)
            {
                if (Id(player) != playerId) continue;
                runner.Disconnect(player);
                if (state.Remove(playerId)) PublishRoster();
                return true;
            }
            return false;
        }
        public bool StartGame(string sceneName, int minimumPlayers, bool requireAllReady)
        {
            if (!IsHost || busy || state.Starting) return false;
            if (sceneName == GameplaySceneName)
            {
                if (state.Capacity != 2 || state.Players.Count != 2 || runner.ActivePlayers.Count() != 2)
                    return Fail("A crew needs exactly two connected keepers.");
                minimumPlayers = 2;
                requireAllReady = true;
            }
            int index = -1;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                if (System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) == sceneName) { index = i; break; }
            if (index < 0) return Fail("Add " + sceneName + " to Build Settings before starting.");
            if (!state.Begin(minimumPlayers, requireAllReady)) return false;
            gameSceneName = sceneName;
            LastError = "";
            if (runner.SessionInfo.IsValid) runner.SessionInfo.IsOpen = false; // No unready late joins during launch.
            PublishRoster(); GameStarting?.Invoke(); StatusChanged?.Invoke();
            _ = LoadGame(SceneRef.FromIndex(index));
            return true;
        }
        async Task LoadGame(SceneRef scene)
        {
            try { await runner.LoadScene(scene, LoadSceneMode.Single); }
            catch (Exception e)
            {
                if (!this || !IsHost) return;
                state.CancelStart(); if (runner.SessionInfo.IsValid) runner.SessionInfo.IsOpen = true;
                Fail("Could not load the lighthouse: " + e.Message); PublishRoster();
            }
        }
        bool Fail(string error) { LastError = error; StatusChanged?.Invoke(); return false; }
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            startup?.Cancel();
            if (runner) _ = ReleaseRunner(runner);
        }
        public void OnPlayerJoined(NetworkRunner source, PlayerRef player)
        {
            if (source != runner || !source.IsServer) return;
            if (state.Starting) { source.Disconnect(player); return; }
            state.Add(Id(player), player == source.LocalPlayer);
            PublishRoster();
        }
        public void OnPlayerLeft(NetworkRunner source, PlayerRef player)
        {
            if (source != runner) return;
            if (source.IsServer && state.Remove(Id(player))) PublishRoster();
            if ((enteredGame || state.Starting) && (gameSceneName == GameplaySceneName || CrewSessionPolicy.RequiresWholeCrew(gameSceneName)) && !busy && !leaving)
            {
                enteredGame = true; // Approval is already committed, even before the load callback.
                Fail("Your partner left the watch. Reconnect together to start a new run.");
                _ = Disconnect();
            }
        }
        public void OnReliableDataReceived(NetworkRunner source, PlayerRef sender, ReliableKey key, ReadOnlySpan<byte> data)
        {
            key.GetInts(out int channel, out int version, out _, out _);
            if (source != runner || channel != LobbyProtocol.Channel || version != 1 ||
                !LobbyProtocol.TryDecode(data, out var packet)) return;
            if (source.IsServer)
            {
                if (state.ApplyRequest(Id(sender), packet)) PublishRoster();
            }
            // Host/Client mode routes client-bound reliable streams through the server.
            else if (state.ApplySnapshot(packet))
            {
                if (state.Starting) GameStarting?.Invoke();
                PlayersChanged?.Invoke(); StatusChanged?.Invoke();
            }
        }
        public void OnShutdown(NetworkRunner source, ShutdownReason reason)
        {
            if (source != runner || leaving || busy || !this) return;
            Fail("Crew session ended: " + reason + ". Host a new room to reconnect.");
            _ = Disconnect();
        }
        public void OnDisconnectedFromServer(NetworkRunner source, NetDisconnectReason reason)
        {
            if (source != runner || leaving || busy || !this) return;
            Fail("Disconnected from crew: " + reason + ". Join again to reconnect.");
            _ = Disconnect();
        }
        public void OnConnectRequest(NetworkRunner source, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            if (state.Starting || state.Players.Count >= state.Capacity) request.Refuse(); else request.Accept();
        }
        public void OnSceneLoadStart(NetworkRunner source)
        {
            // StartGame can complete before the initial lobby finishes loading.
            // Only a host-approved launch is a gameplay transition.
            if (source == runner && state.Starting) { enteredGame = true; GameStarting?.Invoke(); }
        }
        public void OnSceneLoadDone(NetworkRunner source)
        {
            if (source == runner && (SceneManager.GetSceneByName("LK_Bootstrap").isLoaded ||
                SceneManager.GetSceneByName(GameplaySceneName).isLoaded))
            {
                enteredGame = true;
                gameSceneName = SceneManager.GetSceneByName(GameplaySceneName).isLoaded ? GameplaySceneName : "LK_Bootstrap";
            }
        }
        public void OnConnectedToServer(NetworkRunner source) { }
        public void OnConnectFailed(NetworkRunner source, NetAddress address, NetConnectFailedReason reason) { }
        public void OnInput(NetworkRunner source, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner source, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner source, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner source, NetworkObject obj, PlayerRef player) { }
        public void OnUserSimulationMessage(NetworkRunner source, SimulationMessagePtr message) { }
        public void OnReliableDataProgress(NetworkRunner source, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSessionListUpdated(NetworkRunner source, List<SessionInfo> sessions) { }
        public void OnCustomAuthenticationResponse(NetworkRunner source, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner source, HostMigrationToken token)
        {
            Fail("The host left. Host migration is not supported yet; reconnect in a new room.");
            _ = Disconnect();
        }
    }
}
