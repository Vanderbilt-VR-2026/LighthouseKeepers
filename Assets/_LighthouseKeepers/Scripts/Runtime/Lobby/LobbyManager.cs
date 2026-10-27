using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Lobby
{
    /// <summary>Lobby rules with local practice and optional network session flow.</summary>
    public sealed class LobbyManager : MonoBehaviour
    {
        [SerializeField] LobbyConfig config;
        [SerializeField, Tooltip("Preferred transport. Falls back to a transport on this object, then the registered Photon default.")]
        MonoBehaviour transportBehaviour;
        [SerializeField] string playerName = "";
        [SerializeField] UnityEvent onRosterChanged = new();
        [SerializeField] UnityEvent<bool> onConnectionChanged = new();
        [SerializeField] UnityEvent onGameStarting = new();
        INetworkTransport transport;
        bool starting;
        string lastError = "";
        ISessionLobbyTransport Session => transport as ISessionLobbyTransport;
        public static System.Func<INetworkTransport> CreateDefaultTransport { private get; set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetDefaultTransport() => CreateDefaultTransport = null;

        public INetworkTransport Transport => transport;
        public UnityEvent OnRosterChanged => onRosterChanged;
        public UnityEvent<bool> OnConnectionChanged => onConnectionChanged;
        public UnityEvent OnGameStarting => onGameStarting;
        public bool IsOnline => transport != null && transport.IsOnline;
        public bool IsHost => transport != null && transport.IsHost;
        public bool IsLocalPreview => transport is LocalLobbyTransport;
        public bool IsStarting => starting || Session?.IsStarting == true;
        public bool IsBusy => Session?.IsBusy == true;
        public string SessionName { get => Session?.SessionName ?? ""; set { if (Session != null) Session.SessionName = value; } }
        public int MaxPlayers => IsOnline && Session != null ? Session.MaxPlayers : config ? config.MaxPlayers : 4;
        public int MinPlayersToStart => config ? config.MinPlayersToStart : 1;
        public bool RequireAllReady => config == null || config.RequireAllReady;
        public string Title => config ? config.LobbyTitle : "Lighthouse Keepers";
        public string StartSceneName => config ? config.StartSceneName : ShipWorld.GameplaySceneName;
        public string LastError => !string.IsNullOrEmpty(lastError) ? lastError : Session?.LastError ?? "";
        public IReadOnlyList<LobbyPlayer> Players => transport != null ? transport.Players : System.Array.Empty<LobbyPlayer>();
        string LaunchBlockReason => (transport as ILobbyLaunchPolicy)?.StartBlockReason(StartSceneName) ?? "";
        public bool CanStart => !IsStarting && !IsBusy && IsOnline && IsHost && Players.Count >= MinPlayersToStart &&
            (!RequireAllReady || Players.All(p => p.IsReady)) && string.IsNullOrEmpty(LaunchBlockReason);
        public string StartHint
        {
            get
            {
                if (IsStarting) return "Entering the lighthouse...";
                if (IsBusy) return "Connecting to the crew...";
                if (!IsOnline) return IsLocalPreview ? "Take your post to open local practice." :
                    StartSceneName == ShipWorld.GameplaySceneName ? "Choose PLAY SOLO to begin offline, or host a crew for connection diagnostics." :
                    "Host a crew, or join the same room as your partner.";
                if (!string.IsNullOrEmpty(LaunchBlockReason)) return LaunchBlockReason;
                if (!IsHost) return IsLocalPreview ? "Guest preview only. Leave and open practice to play." : "Your host will start when the crew is ready.";
                if (Players.Count < MinPlayersToStart) return $"Waiting for {MinPlayersToStart - Players.Count} more keeper(s).";
                if (RequireAllReady && Players.Any(p => !p.IsReady)) return "Every keeper must be ready before entering.";
                return "Crew ready. Enter the lighthouse when you are ready.";
            }
        }
        public string PlayerName
        {
            get => string.IsNullOrWhiteSpace(playerName) ? (config ? config.DefaultPlayerName : "Keeper") : playerName.Trim();
            set { playerName = value; transport?.SetLocalName(PlayerName); }
        }

        void Awake() => ResolveTransport();
        void OnDestroy()
        {
            if (transport == null) return;
            transport.PlayersChanged -= HandleRosterChanged;
            transport.ConnectionChanged -= HandleConnectionChanged;
            if (Session != null)
            {
                Session.StatusChanged -= HandleStatusChanged;
                Session.GameStarting -= HandleGameStarting;
            }
        }
        public bool Host()
        {
            if (transport == null || IsOnline || IsBusy) return false;
            lastError = "";
            transport.SetLocalName(PlayerName);
            bool success = transport.HostGame(MaxPlayers);
            if (!success && string.IsNullOrEmpty(Session?.LastError)) lastError = "Could not open the session. Please try again.";
            return success;
        }
        public bool Join(string address)
        {
            if (transport == null || IsOnline || IsBusy) return false;
            lastError = "";
            if (!IsLocalPreview && string.IsNullOrWhiteSpace(address))
            {
                lastError = "Enter a room name before joining.";
                return false;
            }
            transport.SetLocalName(PlayerName);
            bool success = transport.JoinGame(address?.Trim());
            if (!success && string.IsNullOrEmpty(Session?.LastError)) lastError = "Could not join that session. Check the room name and try again.";
            return success;
        }
        public void Leave() { starting = false; lastError = ""; transport?.LeaveGame(); }
        public void ToggleReady() { var local = LocalPlayer(); if (local != null && !IsStarting && !IsBusy) transport.SetLocalReady(!local.IsReady); }
        public bool Kick(string playerId) => !IsStarting && !IsBusy && transport != null && transport.KickPlayer(playerId);
        public LobbyPlayer LocalPlayer() => transport == null ? null : Players.FirstOrDefault(p => p.Id == transport.LocalPlayerId);
        public bool StartGame()
        {
            if (!CanStart) return false;
            lastError = "";
            if (Session != null) return Session.StartGame(StartSceneName, MinPlayersToStart, RequireAllReady);
            if (!Application.CanStreamedLevelBeLoaded(StartSceneName))
            {
                lastError = $"Cannot open {StartSceneName}. Add it to the active build scene list.";
                return false;
            }
            lastError = "";
            starting = true;
            onGameStarting.Invoke();
            SceneManager.LoadScene(StartSceneName, LoadSceneMode.Single);
            return true;
        }
        void ResolveTransport()
        {
            if (transport != null) return;
            transport = transportBehaviour as INetworkTransport;
            if (transport == null)
                transport = GetComponents<MonoBehaviour>().OfType<INetworkTransport>().FirstOrDefault();
            transport ??= CreateDefaultTransport?.Invoke();
            if (transport == null)
            {
                lastError = "Photon crew transport is missing. Open the game lobby to connect.";
                return;
            }
            transport.PlayersChanged += HandleRosterChanged;
            transport.ConnectionChanged += HandleConnectionChanged;
            if (Session != null)
            {
                Session.StatusChanged += HandleStatusChanged;
                Session.GameStarting += HandleGameStarting;
            }
        }
        void HandleRosterChanged() => onRosterChanged.Invoke();
        void HandleConnectionChanged(bool online) { if (!online) starting = false; onConnectionChanged.Invoke(online); }
        void HandleStatusChanged() { if (Session?.IsStarting != true) starting = false; onRosterChanged.Invoke(); }
        void HandleGameStarting() { if (starting) return; starting = true; onGameStarting.Invoke(); }
    }
}
