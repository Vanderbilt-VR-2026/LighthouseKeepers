using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using LighthouseKeepers.Networking;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    /// <summary>Bounded messages on the existing runner; this component never creates or shuts down a runner.</summary>
    [DisallowMultipleComponent]
    public sealed class FusionMessageChannel : MonoBehaviour, INetworkRunnerCallbacks
    {
        const int Channel = 0x4C4B5333;
        // v18 carries the integrated host-owned grace state. Keep a bounded 4 KiB envelope.
        public const int MaxBytes = 4096;
        FusionLobbyTransport transport;
        NetworkRunner runner;
        ActorIdentity localActor;
        int transfer;
        float nextBind;
        public NetworkRunner Runner => runner;
        public bool Connected => runner && runner.IsRunning && transport && transport.IsOnline;
        public bool IsHost => Connected && runner.IsServer;
        public int LocalActorId => Connected ? runner.LocalPlayer.RawEncoded : 0;
        public event Action<int, int, byte[]> Received;
        public event Action SessionChanged;
        public event Action<int> ActorLeft;
        public static FusionMessageChannel Active { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Active = null;
        void Awake()
        {
            transport = GetComponent<FusionLobbyTransport>();
            if (!transport) { enabled = false; return; }
            transport.StatusChanged += Bind;
            transport.ConnectionChanged += Connection;
        }
        void Connection(bool online) => Bind();
        void Bind()
        {
            var current = transport && transport.IsOnline && transport.Runner && transport.Runner.IsRunning ? transport.Runner : null;
            if (current == runner) return;
            if (runner) runner.RemoveCallbacks(this);
            runner = current; transfer = 0; samples.Clear();awaiting.Clear();sentAt.Clear();
            if (localActor) localActor.Bind(0);
            if (runner) { runner.AddCallbacks(this); Active = this; CrewVoiceSession.Ensure(runner,this); }
            else if (Active == this) Active = null;
            SessionChanged?.Invoke();
        }
        void Update()
        {
            Bind();
            FlushSamples();
            if (!Connected || Time.unscaledTime < nextBind) return;
            nextBind = Time.unscaledTime + .5f;
            if (!localActor)
            {
                var rig = FindAnyObjectByType<XROrigin>();
                if (rig) localActor = rig.GetComponent<ActorIdentity>() ?? rig.gameObject.AddComponent<ActorIdentity>();
            }
            if (localActor) localActor.Bind(LocalActorId);
        }
        public bool IsActorConnected(int actor)
        {
            if (!Connected || actor <= 0) return false;
            foreach (var peer in runner.ActivePlayers) if (peer.RawEncoded == actor) return true;
            return false;
        }
        public bool TryPlayer(int actor, out PlayerRef player)
        {
            if (Connected) foreach (var peer in runner.ActivePlayers) if (peer.RawEncoded == actor) { player = peer; return true; }
            player = PlayerRef.None; return false;
        }
        public void SendToHost(int kind, byte[] payload)
        {
            if (!Connected || payload == null || payload.Length == 0 || payload.Length > MaxBytes) return;
            if (IsHost) Received?.Invoke(LocalActorId, kind, payload);
            else runner.SendReliableDataToServer(NextKey(kind), payload);
        }
        public void SendToActor(int actor, int kind, byte[] payload)
        {
            if (!IsHost || payload == null || payload.Length == 0 || payload.Length > MaxBytes || !TryPlayer(actor, out var peer)) return;
            if (peer == runner.LocalPlayer) Received?.Invoke(0, kind, payload);
            else runner.SendReliableDataToPlayer(peer, NextKey(kind), payload);
        }
        public void Broadcast(int kind, byte[] payload)
        {
            if (!IsHost || payload == null || payload.Length == 0 || payload.Length > MaxBytes) return;
            foreach (var peer in runner.ActivePlayers)
                if (peer != runner.LocalPlayer) runner.SendReliableDataToPlayer(peer, NextKey(kind), payload);
            Received?.Invoke(0, kind, payload);
        }
        public void SampleToHost(int kind,byte[] payload,int slot=0)
        {
            if(!Connected)return;
            if(IsHost){Received?.Invoke(LocalActorId,kind,payload);return;}
            QueueSample(0,kind,slot,payload);
        }
        public void SampleToActor(int actor,int kind,byte[] payload)
        {
            if(!IsHost||!TryPlayer(actor,out var peer))return;
            if(peer==runner.LocalPlayer)Received?.Invoke(0,kind,payload);
            else QueueSample(actor,kind,0,payload);
        }
        public void SampleBroadcast(int kind,byte[] payload,int slot=0)
        {
            if(!IsHost)return;
            foreach(var peer in runner.ActivePlayers)
                if(peer!=runner.LocalPlayer)QueueSample(peer.RawEncoded,kind,slot,payload);
            Received?.Invoke(0,kind,payload);
        }
        readonly Dictionary<int,LatestSampleBatch> samples=new();
        readonly Dictionary<int,int> awaiting=new();
        readonly Dictionary<int,float> sentAt=new();
        int sampleSequence;float nextSamples;
        void QueueSample(int target,int kind,int slot,byte[] payload)
        {
            if(payload==null||payload.Length==0||payload.Length>MaxBytes)return;
            if(!samples.TryGetValue(target,out var batch))samples[target]=batch=new LatestSampleBatch();
            batch.Put(kind,slot,payload);
        }
        void FlushSamples()
        {
            if(!Connected||Time.unscaledTime<nextSamples)return;nextSamples=Time.unscaledTime+.05f;
            foreach(var pair in samples)
            {
                int target=pair.Key;
                if(awaiting.ContainsKey(target)&&Time.unscaledTime-sentAt[target]<2)continue;
                if(pair.Value.Count==0)continue;
                var data=pair.Value.Pack(++sampleSequence);
                if(data.Length>MaxBytes){Debug.LogError("Crew sample batch exceeds bounded channel");continue;}
                if(IsHost&&!TryPlayer(target,out _))continue;
                awaiting[target]=sampleSequence;sentAt[target]=Time.unscaledTime;
                if(IsHost)SendToActor(target,30,data);else SendToHost(30,data);
            }
        }
        void ReceiveBatch(int sender,int kind,ReadOnlySpan<byte> data)
        {
            if(kind==31)
            {
                if(data.Length==4&&awaiting.TryGetValue(sender,out var seq)&&BitConverter.ToInt32(data.ToArray(),0)==seq)awaiting.Remove(sender);
                return;
            }
            if(!LatestSampleBatch.TryUnpack(data.ToArray(),out int sequence,out var batch))return;
            // Acknowledge before delivering; a subsequent batch replaces outdated samples.
            if(IsHost)SendToActor(sender,31,BitConverter.GetBytes(sequence));else SendToHost(31,BitConverter.GetBytes(sequence));
            foreach(var sample in batch)Received?.Invoke(sender,sample.Kind,sample.Data);
        }
        ReliableKey NextKey(int kind) => ReliableKey.FromInts(Channel, 1, kind, ++transfer);
        public void OnReliableDataReceived(NetworkRunner source, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
        {
            key.GetInts(out int channel, out int version, out int kind, out _);
            if (!Connected || source != runner || channel != Channel || version != 1 || data.Length == 0 || data.Length > MaxBytes) return;
            // Host/Client streams to clients originate at the server. Clients cannot route directly to another client.
            if (IsHost && (player == runner.LocalPlayer || !runner.ActivePlayers.Contains(player))) return;
            int sender=IsHost?player.RawEncoded:0;
            if(kind==30||kind==31){ReceiveBatch(sender,kind,data);return;}
            Received?.Invoke(sender,kind,data.ToArray());
        }
        void OnDestroy()
        {
            if (transport) { transport.StatusChanged -= Bind; transport.ConnectionChanged -= Connection; }
            if (runner) runner.RemoveCallbacks(this);
            if (localActor) localActor.Bind(0);
            if (Active == this) Active = null;
        }
        public void OnPlayerLeft(NetworkRunner r, PlayerRef p) { if (r != runner)return; samples.Remove(p.RawEncoded);awaiting.Remove(p.RawEncoded);sentAt.Remove(p.RawEncoded);ActorLeft?.Invoke(p.RawEncoded); }
        public void OnPlayerJoined(NetworkRunner r, PlayerRef p) { }
        public void OnShutdown(NetworkRunner r, ShutdownReason reason) { if (r == runner) Bind(); }
        public void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason reason) { }
        public void OnConnectedToServer(NetworkRunner r) { }
        public void OnConnectFailed(NetworkRunner r, NetAddress a, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnHostMigration(NetworkRunner r, HostMigrationToken token) { }
        public void OnInput(NetworkRunner r, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
        public void OnObjectExitAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
        public void OnSceneLoadStart(NetworkRunner r) { }
        public void OnSceneLoadDone(NetworkRunner r) { }
        public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> list) { }
        public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
        public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) { }
        public void OnReliableDataProgress(NetworkRunner r, PlayerRef p, ReliableKey key, float progress) { }
    }
}
