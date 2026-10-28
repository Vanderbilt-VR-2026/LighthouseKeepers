using System;
using System.Collections.Generic;
using System.Collections;
using LighthouseKeepers.StormWatch;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Net
{
    /// <summary>Six-character crew codes and recovery around William's unchanged Fusion transport.</summary>
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class CrewLobbyTransport : MonoBehaviour, ISessionLobbyTransport, ILobbyLaunchPolicy
    {
        [SerializeField] FusionLobbyTransport fusion;
        static string rememberedCode, rememberedName;
        static bool rememberedHost;
        string localError = "", desiredName = "Keeper", desiredCode = "";
        bool timedOut, connecting, launchingSolo;
        float connectStarted;
        public FusionLobbyTransport Fusion => fusion;
        public bool IsOnline => fusion && fusion.IsOnline;
        public bool IsHost => fusion && fusion.IsHost;
        public bool IsBusy => launchingSolo || fusion && fusion.IsBusy;
        public bool IsStarting => launchingSolo || fusion && fusion.IsStarting;
        public bool LaunchingSolo => launchingSolo;
        public string LocalPlayerId => fusion ? fusion.LocalPlayerId : "";
        public IReadOnlyList<LobbyPlayer> Players => fusion ? fusion.Players : Array.Empty<LobbyPlayer>();
        public int MaxPlayers => CrewConnectionPolicy.Capacity;
        public string LastError => string.IsNullOrEmpty(localError) ? CrewConnectionPolicy.Explain(fusion ? fusion.LastError : "Crew transport is missing.") : localError;
        public string SessionName { get => IsOnline || IsBusy ? fusion.SessionName : desiredCode; set => desiredCode = value ?? ""; }
        public bool CanReconnect => !IsOnline && !IsBusy && !string.IsNullOrEmpty(rememberedCode);
        public event Action PlayersChanged;
        public event Action<bool> ConnectionChanged;
        public event Action StatusChanged;
        public event Action GameStarting;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetMemory() { rememberedCode = rememberedName = null; rememberedHost = false; }
        void Awake()
        {
            if (!fusion) fusion = GetComponent<FusionLobbyTransport>();
            if (!fusion) { localError = "Crew transport is missing. Reopen the networking lobby."; return; }
            desiredCode = rememberedCode ?? "";
            desiredName = rememberedName ?? "Keeper";
            fusion.PlayersChanged += Roster;
            fusion.ConnectionChanged += Connection;
            fusion.StatusChanged += Status;
            fusion.GameStarting += Starting;
        }
        public bool HostGame(int maxPlayers) => Connect(true, CrewConnectionPolicy.NewCode());
        public bool JoinGame(string address)
        {
            if (!CrewConnectionPolicy.TryCode(address, out var code)) return Fail("Enter the six-character join code shown on your host's board.");
            return Connect(false, code);
        }
        public bool Reconnect()
        {
            if (!CanReconnect) return false;
            desiredName = rememberedName ?? desiredName;
            return Connect(rememberedHost, rememberedCode);
        }
        public bool PlaySolo()
        {
            if (launchingSolo || fusion && fusion.IsStarting) return false;
            if (!Application.CanStreamedLevelBeLoaded(ShipWorld.GameplaySceneName))
                return Fail("The solo lighthouse is missing from this build. Reinstall the current APK.");
            launchingSolo = true;
            connecting = false;
            localError = "";
            Status();
            StartCoroutine(LaunchSolo());
            return true;
        }
        IEnumerator LaunchSolo()
        {
            // Hosting is optional. If it was already attempted, finish cancellation before loading locally.
            var sessions = FindObjectsByType<FusionLobbyTransport>(FindObjectsSortMode.None);
            foreach (var session in sessions) session.LeaveGame();
            float deadline = Time.realtimeSinceStartup + 30;
            while (true)
            {
                bool closing = false;
                foreach (var session in sessions)
                    if (session && (session.IsBusy || session.IsOnline || session.Runner)) closing = true;
                if (!closing) break;
                if (Time.realtimeSinceStartup >= deadline)
                {
                    launchingSolo = false;
                    Fail("Could not close the crew connection. Leave the crew, then choose Play Solo again.");
                    yield break;
                }
                yield return null;
            }
            SoloWatchSession.Request();
            AsyncOperation load = null;
            try { load = SceneManager.LoadSceneAsync(ShipWorld.GameplaySceneName); }
            catch (Exception error) { localError = "Could not open solo practice: " + error.Message; }
            if (load == null)
            {
                SoloWatchSession.Cancel();
                launchingSolo = false;
                Status();
                yield break;
            }
            yield return load;
            // The offline watch owns no room, runner, poses or grab leases.
            foreach (var session in sessions) if (session) Destroy(session.gameObject);
            Destroy(gameObject);
        }
        bool Connect(bool host, string code)
        {
            if (!fusion || IsOnline || IsBusy) return false;
            localError = ""; timedOut = false;
            rememberedCode = desiredCode = code; rememberedHost = host; rememberedName = desiredName;
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return Fail("No internet connection. Connect to Wi-Fi, then press Reconnect.");
            connecting = true; connectStarted = Time.realtimeSinceStartup;
            fusion.SessionName = code;
            fusion.SetLocalName(desiredName);
            bool accepted = host ? fusion.HostGame(CrewConnectionPolicy.Capacity) : fusion.JoinGame(code);
            Status();
            if (!accepted) connecting = false;
            return accepted;
        }
        void Update()
        {
            if (!connecting || !IsBusy || timedOut || Time.realtimeSinceStartup - connectStarted < 30) return;
            timedOut = true;
            localError = "Connection timed out. Check Wi-Fi and Photon settings, then press Reconnect.";
            fusion.LeaveGame(); Status();
        }
        public void LeaveGame() { connecting = false; localError = ""; if (fusion) fusion.LeaveGame(); }
        public void SetLocalReady(bool ready) { if (fusion) fusion.SetLocalReady(ready); }
        public void SetLocalName(string value)
        {
            desiredName = LobbyProtocol.DisplayName(value);
            if (IsOnline) rememberedName = desiredName;
            if (fusion) fusion.SetLocalName(desiredName);
        }
        public bool KickPlayer(string playerId) => fusion && fusion.KickPlayer(playerId);
        public bool StartNetworkRehearsal()
        {
            if(!IsHost || IsBusy || IsStarting)return false;
            foreach(var player in Players)if(!player.IsReady)return Fail("Every keeper must signal ready before network practice.");
            localError="";
            return fusion.StartNetworkRehearsal();
        }
        public string StartBlockReason(string sceneName) => CrewSessionPolicy.GameplayLaunchBlockReason(ShipWorld.GameplaySceneName, Players.Count);
        public bool StartGame(string sceneName, int minimumPlayers, bool requireAllReady)
        {
            string blocked = StartBlockReason(sceneName);
            if (!string.IsNullOrEmpty(blocked)) return Fail(blocked);
            if (!IsHost || Players.Count < 1 || Players.Count > CrewConnectionPolicy.Capacity) return Fail("Host a crew before starting.");
            foreach (var player in Players) if (!player.IsReady) return Fail("Every keeper must signal ready before starting.");
            localError = "";
            return fusion.StartGame(ShipWorld.GameplaySceneName, 1, true);
        }
        bool Fail(string message) { localError = message; Status(); return false; }
        void Roster()
        {
            if (localError == CrewSessionPolicy.SharedGameStatePending && string.IsNullOrEmpty(StartBlockReason(ShipWorld.GameplaySceneName)))
                localError = "";
            PlayersChanged?.Invoke();
        }
        void Connection(bool online) { connecting = false; if (online) localError = ""; ConnectionChanged?.Invoke(online); }
        void Status() => StatusChanged?.Invoke();
        void Starting() => GameStarting?.Invoke();
        void OnDestroy()
        {
            if (!fusion) return;
            fusion.PlayersChanged -= Roster; fusion.ConnectionChanged -= Connection;
            fusion.StatusChanged -= Status; fusion.GameStarting -= Starting;
        }
    }
}
