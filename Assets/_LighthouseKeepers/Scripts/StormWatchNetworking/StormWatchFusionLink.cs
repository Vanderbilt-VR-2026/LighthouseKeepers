using System;
using System.Collections.Generic;
using System.Text;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using LighthouseKeepers.StormWatch;

namespace LighthouseKeepers.StormWatchNetworking
{
    /// <summary>Host-owned session using Fusion's reliable transport; no second gameplay authority on clients.</summary>
    public sealed class StormWatchFusionLink : WatchLink, INetworkRunnerCallbacks
    {
        const int Channel = 0x53574B50;
        NetworkRunner runner;
        bool busy;
        string status="Host or join the same two-keeper room.";
        KeeperInput remote = new();
        WatchState received;
        float lastInput;
        int transfer, snapshot, acceptedSnapshot=-1;
        public override bool Connected => runner && runner.IsRunning;
        public override bool IsHost => Connected && runner.IsServer;
        public override int LocalSlot => Connected && !runner.IsServer ? 1 : 0;
        public override bool PartnerConnected => Connected && runner.ActivePlayers.Count() == 2;
        public override string Status => status;
        public override KeeperInput RemoteInput
        {
            get { remote.connected=PartnerConnected; if(Time.unscaledTime-lastInput>.6f){remote.action=WatchAction.None;remote.effort=0;}return remote; }
        }
        public override WatchState ReceivedState => received;
        [Serializable] public sealed class Packet { public int version=1, serial; public KeeperInput input; public WatchState state; }
        public override async void Connect(bool host,string room)
        {
            if(busy||runner)return;
            room=room?.Trim();if(string.IsNullOrEmpty(room)||room.Length>40){status="Use a room name of 1–40 characters.";return;}
            busy=true;status=host?"Opening crew room…":"Joining crew room…";
            var go=new GameObject("Storm Watch Fusion runner");go.transform.SetParent(transform);runner=go.AddComponent<NetworkRunner>();runner.AddCallbacks(this);
            try
            {
                // Both peers already loaded the isolated entry; only session data is transported.
                var result=await runner.StartGame(new StartGameArgs {GameMode=host?GameMode.Host:GameMode.Client,SessionName="SW-"+room,
                    PlayerCount=2,IsVisible=false,EnableClientSessionCreation=false});
                if(!this)return;
                status=result.Ok?"Connected. Waiting for both keepers to begin.":"Connection failed: "+result.ShutdownReason;
                if(!result.Ok)await Shutdown();
            }
            catch(Exception e){if(this){status="Connection failed: "+e.Message;await Shutdown();}}
            finally{busy=false;}
        }
        public override void SendInput(KeeperInput input)
        {
            if(!Connected||!PartnerConnected)return;
            var bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Packet{input=input}));
            var key=ReliableKey.FromInts(Channel,1,0,++transfer);
            if(IsHost){foreach(var player in runner.ActivePlayers)if(player!=runner.LocalPlayer)runner.SendReliableDataToPlayer(player,key,bytes);}
            else runner.SendReliableDataToServer(key,bytes);
        }
        public override void Publish(WatchState state)
        {
            if(!IsHost||!PartnerConnected)return;
            var bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Packet{state=state,serial=++snapshot}));
            foreach(var player in runner.ActivePlayers)if(player!=runner.LocalPlayer)runner.SendReliableDataToPlayer(player,ReliableKey.FromInts(Channel,1,1,++transfer),bytes);
        }
        public override async void Leave(){if(busy)return;busy=true;await Shutdown();busy=false;status="Session left. Host or join a new room.";}
        async System.Threading.Tasks.Task Shutdown()
        {
            var current=runner;runner=null;remote=new();received=null;acceptedSnapshot=-1;
            if(!current)return;current.RemoveCallbacks(this);await current.Shutdown();if(current)Destroy(current.gameObject);
        }
        public void OnReliableDataReceived(NetworkRunner source,PlayerRef player,ReliableKey key,ReadOnlySpan<byte> data)
        {
            key.GetInts(out int channel,out int version,out int kind,out _);
            if(source!=runner||channel!=Channel||version!=1||data.Length>8192||data.Length==0)return;
            try
            {
                var packet=JsonUtility.FromJson<Packet>(Encoding.UTF8.GetString(data));if(packet==null||packet.version!=1)return;
                if(kind==0&&packet.input!=null&&StormWatchRules.Finite(packet.input.head)&&StormWatchRules.Finite(packet.input.feet)&&
                    StormWatchRules.Finite(packet.input.left)&&StormWatchRules.Finite(packet.input.right)&&StormWatchRules.Finite(packet.input.aim))
                {
                    // Retain a command until the sequence changes; multiple receives in one frame must not erase it.
                    if(packet.input.sequence==remote.sequence && packet.input.command==WatchAction.None)packet.input.command=remote.command;
                    if(packet.input.sequence<remote.sequence)return;
                    remote=packet.input;remote.connected=true;lastInput=Time.unscaledTime;
                }
                else if(!IsHost&&kind==1&&packet.state!=null&&packet.serial>acceptedSnapshot){acceptedSnapshot=packet.serial;received=packet.state;}
            }
            catch(ArgumentException){status="Ignored an incompatible session packet.";}
        }
        public void OnPlayerJoined(NetworkRunner r,PlayerRef p){if(r==runner)status=PartnerConnected?"Crew connected. Both keepers ready.":"Room open. Waiting for the other keeper.";}
        public void OnPlayerLeft(NetworkRunner r,PlayerRef p){if(r==runner){remote.connected=false;status="Partner disconnected. Watch paused; leave and reconnect together.";}}
        public void OnConnectRequest(NetworkRunner r,NetworkRunnerCallbackArgs.ConnectRequest request,byte[] token){if(r.ActivePlayers.Count()>=2)request.Refuse();else request.Accept();}
        public void OnShutdown(NetworkRunner r,ShutdownReason reason){if(r==runner){status="Session ended: "+reason;remote=new();received=null;acceptedSnapshot=-1;runner=null;Destroy(r.gameObject);}}
        public void OnDisconnectedFromServer(NetworkRunner r,NetDisconnectReason reason){if(r==runner)status="Disconnected: "+reason;}
        void OnDestroy(){if(runner){runner.RemoveCallbacks(this);_ = runner.Shutdown();}}
        public void OnConnectedToServer(NetworkRunner r){}
        public void OnConnectFailed(NetworkRunner r,NetAddress address,NetConnectFailedReason reason){}
        public void OnSceneLoadStart(NetworkRunner r){}
        public void OnSceneLoadDone(NetworkRunner r){}
        public void OnInput(NetworkRunner r,NetworkInput input){}
        public void OnInputMissing(NetworkRunner r,PlayerRef p,NetworkInput input){}
        public void OnObjectEnterAOI(NetworkRunner r,NetworkObject obj,PlayerRef p){}
        public void OnObjectExitAOI(NetworkRunner r,NetworkObject obj,PlayerRef p){}
        public void OnUserSimulationMessage(NetworkRunner r,SimulationMessagePtr message){}
        public void OnReliableDataProgress(NetworkRunner r,PlayerRef p,ReliableKey key,float progress){}
        public void OnSessionListUpdated(NetworkRunner r,List<SessionInfo> sessions){}
        public void OnCustomAuthenticationResponse(NetworkRunner r,Dictionary<string,object> data){}
        public void OnHostMigration(NetworkRunner r,HostMigrationToken token){status="Host left. Reconnect together to restart.";Leave();}
    }
    static class PlayerEnumerable { public static int Count(this IEnumerable<PlayerRef> players){int n=0;foreach(var p in players)n++;return n;} }
}
