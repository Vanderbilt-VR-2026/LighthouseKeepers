using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Lobby
{
    /// <summary>Lobby rules and local scene flow. A real transport must also implement shared game launch.</summary>
    public sealed class LobbyManager : MonoBehaviour
    {
        [SerializeField] LobbyConfig config;
        [SerializeField, Tooltip("Preferred transport. Falls back to a transport on this object, then local practice.")]
        MonoBehaviour transportBehaviour;
        [SerializeField] string playerName = "";
        [SerializeField] UnityEvent onRosterChanged = new();
        [SerializeField] UnityEvent<bool> onConnectionChanged = new();
        [SerializeField] UnityEvent onGameStarting = new();
        INetworkTransport transport;
        bool starting;

        public INetworkTransport Transport => transport;
        public UnityEvent OnRosterChanged => onRosterChanged;
        public UnityEvent<bool> OnConnectionChanged => onConnectionChanged;
        public UnityEvent OnGameStarting => onGameStarting;
        public bool IsOnline => transport != null && transport.IsOnline;
        public bool IsConnecting => (transport as ISessionTransport)?.IsConnecting == true;
        public bool IsHost => transport != null && transport.IsHost;
        public bool IsLocalPreview => transport is LocalLobbyTransport;
        public bool IsStarting => starting;
        public int MaxPlayers => transport is ISessionTransport ? 4 : config ? config.MaxPlayers : 4;
        public int MinPlayersToStart => Mathf.Min(config ? config.MinPlayersToStart : 1, MaxPlayers);
        public bool RequireAllReady => config == null || config.RequireAllReady;
        public string Title => config ? config.LobbyTitle : "Lighthouse Keepers";
        public string StartSceneName => config ? config.StartSceneName : "LK_Bootstrap";
        string lastError = "";
        public string LastError { get => string.IsNullOrEmpty(lastError) ? (transport as ISessionTransport)?.LastError ?? "" : lastError; private set => lastError = value; }
        public IReadOnlyList<LobbyPlayer> Players => transport != null ? transport.Players : System.Array.Empty<LobbyPlayer>();
        public bool CanStart => !starting && IsOnline && IsHost && Players.Count >= MinPlayersToStart &&
            (!RequireAllReady || Players.All(p => p.IsReady));
        public string StartHint
        {
            get
            {
                if (starting) return "Entering the lighthouse...";
                if (IsConnecting) return "Connecting to the crew...";
                if (!IsOnline) return "Open local practice or connect to a crew to begin.";
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
            if (transport is ISessionTransport session) session.GameStarting -= HandleGameStarting;
        }
        public bool Host()
        {
            if (transport == null || IsOnline || IsConnecting) return false;
            LastError = "";
            transport.SetLocalName(PlayerName);
            bool success = transport.HostGame(MaxPlayers);
            if (!success) LastError = (transport as ISessionTransport)?.LastError ?? "Could not open the session. Please try again.";
            return success;
        }
        public bool Join(string address)
        {
            if (transport == null || IsOnline || IsConnecting) return false;
            LastError = "";
            if (!IsLocalPreview && string.IsNullOrWhiteSpace(address))
            {
                LastError = "Enter the crew's room name before joining.";
                return false;
            }
            transport.SetLocalName(PlayerName);
            bool success = transport.JoinGame(address?.Trim());
            if (!success) LastError = (transport as ISessionTransport)?.LastError ?? "Could not join that session. Check the address and try again.";
            return success;
        }
        public void Leave() { starting = false; LastError = ""; transport?.LeaveGame(); }
        public void ToggleReady() { var local = LocalPlayer(); if (local != null && !starting) transport.SetLocalReady(!local.IsReady); }
        public bool Kick(string playerId) => !starting && transport != null && transport.KickPlayer(playerId);
        public LobbyPlayer LocalPlayer() => transport == null ? null : Players.FirstOrDefault(p => p.Id == transport.LocalPlayerId);
        public bool StartGame()
        {
            if (!CanStart) return false;
            if (!Application.CanStreamedLevelBeLoaded(StartSceneName))
            {
                LastError = $"Cannot open {StartSceneName}. Add it to the active build scene list.";
                return false;
            }
            LastError = "";
            if (transport is ISessionTransport session) return session.BeginGame(StartSceneName, RequireAllReady);
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
            if (Application.isPlaying) transport ??= LobbyTransportProvider.Create?.Invoke();
            transport ??= gameObject.AddComponent<LocalLobbyTransport>();
            transport.PlayersChanged += HandleRosterChanged;
            transport.ConnectionChanged += HandleConnectionChanged;
            if (transport is ISessionTransport session) session.GameStarting += HandleGameStarting;
        }
        void HandleGameStarting() { starting = true; onGameStarting.Invoke(); }
        void HandleRosterChanged() => onRosterChanged.Invoke();
        void HandleConnectionChanged(bool online) { if (!online && !IsConnecting) starting = false; onConnectionChanged.Invoke(online); }
    }
}
