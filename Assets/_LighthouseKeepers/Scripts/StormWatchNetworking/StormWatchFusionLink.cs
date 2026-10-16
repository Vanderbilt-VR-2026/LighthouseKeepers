using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusion;
using Fusion.Sockets;
using LighthouseKeepers.Networking;
using LighthouseKeepers.StormWatch;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.StormWatchNetworking
{
    /// <summary>Gameplay messages on the crew lobby's existing runner. The lobby owns its lifetime.</summary>
    public sealed class StormWatchFusionLink : WatchLink, INetworkRunnerCallbacks
    {
        const int Channel = 0x53574B50;
        FusionLobbyTransport session;
        NetworkRunner runner;
        KeeperInput remote = new();
        WatchState received;
        bool receivedInput;
        float lastInput;
        int transfer, inputSerial, snapshot, acceptedInput = -1, acceptedSnapshot = -1;

        public override bool ReturnsToLobby => true;
        public override bool Connected => session && session.IsOnline && !session.IsBusy && runner && runner.IsRunning;
        public override bool IsHost => Connected && runner.IsServer;
        public override int LocalSlot => runner && !runner.IsServer ? 1 : 0;
        public override bool PartnerConnected => Connected && runner.ActivePlayers.Count() == 2;
        public override string Status => !session ? "Return to the crew lobby to start a watch." :
            !Connected ? session.LastError : PartnerConnected ? "Crew connected. Preparing the shared watch…" : "Waiting for your partner…";
        public override KeeperInput RemoteInput
        {
            get
            {
                remote.connected = PartnerConnected && receivedInput && Time.unscaledTime - lastInput <= .6f;
                if (!remote.connected) { remote.action = WatchAction.None; remote.effort = 0; }
                return remote;
            }
        }
        public override WatchState ReceivedState => received;
        [Serializable] public sealed class Packet { public int version = 1, serial; public KeeperInput input; public WatchState state; }

        void Start()
        {
            if (session) return;
            var crew = FindObjectsByType<FusionLobbyTransport>(FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.IsOnline && candidate.Runner &&
                    (candidate.IsStarting || candidate.IsInGame));
            if (crew) BindSession(crew);
        }

        public void BindSession(FusionLobbyTransport crew)
        {
            if (runner) runner.RemoveCallbacks(this);
            session = crew;
            runner = crew ? crew.Runner : null;
            remote = new(); received = null; receivedInput = false;
            transfer = inputSerial = snapshot = 0; acceptedInput = acceptedSnapshot = -1;
            if (runner) runner.AddCallbacks(this);
        }

        public override void SendInput(KeeperInput input)
        {
            if (!Connected || !PartnerConnected) return;
            Send(new Packet { input = input, serial = ++inputSerial }, 0);
        }
        public override void Publish(WatchState state)
        {
            if (IsHost && PartnerConnected) Send(new Packet { state = state, serial = ++snapshot }, 1);
        }
        void Send(Packet packet, int kind)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet));
            var key = ReliableKey.FromInts(Channel, 1, kind, ++transfer);
            if (IsHost)
            {
                foreach (var player in runner.ActivePlayers)
                    if (player != runner.LocalPlayer) runner.SendReliableDataToPlayer(player, key, bytes);
            }
            else runner.SendReliableDataToServer(key, bytes);
        }
        public override void Leave()
        {
            if (session && (session.IsOnline || session.IsBusy)) session.LeaveGame();
            else if (Application.CanStreamedLevelBeLoaded(FusionLobbyTransport.LobbySceneName))
                SceneManager.LoadScene(FusionLobbyTransport.LobbySceneName);
        }
        public void OnReliableDataReceived(NetworkRunner source, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
        {
            key.GetInts(out int channel, out int version, out int kind, out _);
            if (!Connected || source != runner ||
                channel != Channel || version != 1 || data.Length > 8192 || data.Length == 0) return;
            // In Host/Client mode, client-bound reliable streams originate from
            // the server endpoint, which need not be a roster PlayerRef. The host
            // still binds incoming keeper input to an actual remote player.
            if (source.IsServer && (player == runner.LocalPlayer || !runner.ActivePlayers.Contains(player))) return;
            try
            {
                var packet = JsonUtility.FromJson<Packet>(Encoding.UTF8.GetString(data));
                if (packet == null || packet.version != 1) return;
                if (kind == 0 && packet.serial > acceptedInput && packet.input != null &&
                    StormWatchRules.Finite(packet.input.head) && StormWatchRules.Finite(packet.input.feet) &&
                    StormWatchRules.Finite(packet.input.left) && StormWatchRules.Finite(packet.input.right) && StormWatchRules.Finite(packet.input.aim))
                {
                    if (packet.input.sequence < remote.sequence) return;
                    // A later pose must not erase a command before the authority samples it.
                    if (packet.input.sequence == remote.sequence && packet.input.command == WatchAction.None)
                        packet.input.command = remote.command;
                    remote = packet.input; receivedInput = true; lastInput = Time.unscaledTime;
                    acceptedInput = packet.serial;
                }
                else if (!IsHost && kind == 1 && packet.state != null && packet.serial > acceptedSnapshot)
                { acceptedSnapshot = packet.serial; received = packet.state; }
            }
            catch (ArgumentException) { /* Ignore an incompatible packet without changing authoritative state. */ }
        }
        void OnDestroy() { if (runner) runner.RemoveCallbacks(this); }
        public void OnPlayerJoined(NetworkRunner r, PlayerRef p) { }
        public void OnPlayerLeft(NetworkRunner r, PlayerRef p) { }
        public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnShutdown(NetworkRunner r, ShutdownReason reason) { }
        public void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason reason) { }
        public void OnConnectedToServer(NetworkRunner r) { }
        public void OnConnectFailed(NetworkRunner r, NetAddress address, NetConnectFailedReason reason) { }
        public void OnSceneLoadStart(NetworkRunner r) { }
        public void OnSceneLoadDone(NetworkRunner r) { }
        public void OnInput(NetworkRunner r, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
        public void OnObjectExitAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
        public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr message) { }
        public void OnReliableDataProgress(NetworkRunner r, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> sessions) { }
        public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner r, HostMigrationToken token) { }
    }
}
