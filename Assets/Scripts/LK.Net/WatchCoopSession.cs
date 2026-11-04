using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    // Shared Lab commands and snapshots use Will's existing authenticated Fusion channel.
    public sealed class WatchCoopSession : MonoBehaviour, ICoopWatchLink, ICoopWrenchClutch, LighthouseKeepers.Interaction.INetworkKeeperProximity
    {
        const int HelloKind=40,SnapshotKind=41,CommandKind=42,ResultKind=43;
        const string Protocol=LighthouseKeepers.Core.CrewNetworkVersion.Snapshot;
        [Serializable] sealed class Hello {public string protocol=Protocol,world,nonce;}
        [Serializable] sealed class Envelope {public string protocol=Protocol,world,nonce,state;public int sequence;}
        [Serializable] public sealed class Command
        {public string world,nonce;public int sequence,revision;public CoopAction action;public float value;public bool pressed;}
        [Serializable] sealed class Result {public string world,nonce;public int sequence,result;public CoopAction action;}
        ICoopWatchView watch;
        FusionLobbyTransport crew;
        FusionMessageChannel channel;
        CrewPoseStream poses;
        FusionGrabAuthority props;
        readonly Dictionary<int,string> nonces=new();
        readonly HashSet<int> acknowledged=new();
        readonly CoopBedVotes beds=new();
        readonly CoopCommandWindow commands=new();
        readonly Dictionary<int,float> lastPull=new(),rateStart=new();
        readonly Dictionary<int,int> rateCount=new();
        string world,nonce,notice="";
        int sequence,receivedSequence,commandSequence,resultSequence,revision,micActor,wrenchActor;
        float nextSend,nextLog,micSeen,nextStatus,wrenchSeen;
        bool micPressed,wrenchPressed,detached;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        TextMesh debugLine;
        void RefreshDebug()
        {
            if(LighthouseKeepers.Core.ImmersionTuning.Current){if(debugLine)debugLine.gameObject.SetActive(false);return;}
            if(!debugLine)
            {
                var rig=FindAnyObjectByType<XROrigin>();if(!rig||!rig.Camera)return;
                var go=new GameObject("Local crew debug readout");go.transform.SetParent(rig.Camera.transform,false);
                go.transform.localPosition=new Vector3(0,-.34f,.85f);
                debugLine=go.AddComponent<TextMesh>();debugLine.fontSize=64;debugLine.characterSize=.0028f;
                debugLine.anchor=TextAnchor.UpperCenter;debugLine.alignment=TextAlignment.Center;debugLine.color=Color.white;
            }
            debugLine.text=$"us | {crew.SessionName} | {crew.Players.Count}/2 keepers\n"+string.Join(" / ",crew.Players.Select(p=>p.DisplayName));
        }
#endif
        public bool NearKeeper(Vector3 point,float radius)
        {
            if(!channel||!channel.Runner||!poses)return false;
            foreach(var player in channel.Runner.ActivePlayers)
                if(poses.Sample(player.RawEncoded,out var pose))
                {var delta=pose.Head-point;if(Mathf.Abs(delta.y)<2.5f&&new Vector2(delta.x,delta.z).sqrMagnitude<radius*radius)return true;}
            return false;
        }
        public bool Ready
        {
            get
            {
                if(detached)return true;
                if(!channel || !channel.Connected)return false;
                if(!channel.IsHost)return receivedSequence>0;
                foreach(var player in channel.Runner.ActivePlayers)
                    if(player!=channel.Runner.LocalPlayer&&!acknowledged.Contains(player.RawEncoded))return false;
                return true;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()=>CoopWatchBridge.Attach=Attach;
        static ICoopWatchLink Attach(ICoopWatchView view)
        {
            var crew=FindAnyObjectByType<FusionLobbyTransport>();
            if(!crew || !crew.IsOnline)return null;
            bool shared=ShipWorld.CoopEnabled&&!crew.IsNetworkRehearsal;
            view.SetNetworkMode(!shared&&(crew.IsNetworkRehearsal||!crew.IsHost||crew.Players.Count>1),shared&&!crew.IsHost);
            if(!shared)return null;
            var link=view.Owner.gameObject.AddComponent<WatchCoopSession>();link.Initialize(view,crew);return link;
        }
        public void Initialize(ICoopWatchView controller,FusionLobbyTransport transport)
        {
            watch=controller;crew=transport;channel=crew.GetComponent<FusionMessageChannel>();
            poses=crew.GetComponent<CrewPoseStream>();props=crew.GetComponent<FusionGrabAuthority>();
            world=crew.IsHost?Guid.NewGuid().ToString("N"):null;nonce=Guid.NewGuid().ToString("N");
            channel.Received+=Receive;channel.ActorLeft+=Left;channel.SessionChanged+=Connection;
            PlaceKeeper(crew.IsHost);
            Debug.Log($"COOP_ROLE {(crew.IsHost?"HOST":"GUEST")} world={world}");
        }
        void PlaceKeeper(bool host)
        {
            var rig=FindAnyObjectByType<XROrigin>();if(!rig)return;
            var collider=rig.GetComponent<CharacterController>();bool enabled=collider&&collider.enabled;
            if(collider)collider.enabled=false;
            rig.transform.SetPositionAndRotation(new Vector3(-.9f,.04f,host?-9.15f:-7.65f),Quaternion.LookRotation(Vector3.left));
            var board=GameObject.Find("House wall briefing board");
            if(board){var direction=board.transform.position-rig.transform.position;direction.y=0;rig.transform.rotation=Quaternion.LookRotation(direction);}
            if(collider)collider.enabled=enabled;
            Physics.SyncTransforms();
#if UNITY_EDITOR
            var desktop=rig.GetComponent<LighthouseKeepers.Player.DesktopPreview>();if(desktop)desktop.MouseLookEnabled=true;
#endif
            Debug.Log($"COOP_SPAWN {(host?"HOST":"GUEST")} origin={rig.transform.position} head={rig.Camera.transform.position}");
        }
        static byte[] Encode(object value)=>Encoding.UTF8.GetBytes(JsonUtility.ToJson(value));
        public bool Send(CoopAction action,float value=0,bool pressed=false)
        {
            if(detached||!Ready||!channel.Connected)return false;
            channel.SendToHost(CommandKind,Encode(new Command{world=world,nonce=nonce,sequence=++commandSequence,
                revision=watch.Revision,action=action,value=value,pressed=pressed}));return true;
        }
        public bool RemoteHeld(Component prop)
        {
            if(detached||!channel||!channel.IsHost||!prop)return false;
            var binding=prop.GetComponent<FusionGrabBinding>();
            return binding&&binding.IsRemotelyOwned&&props.HasFreshLease(binding.Owner,binding.Id);
        }
        public bool RemoteMicrophone(Component prop,out bool trigger)
        {trigger=micPressed&&Time.unscaledTime-micSeen<=.5f;return RemoteHeld(prop)&&prop.GetComponent<FusionGrabBinding>().Owner==micActor;}
        public bool RemoteWrenchTrigger(Component prop)
        {return wrenchPressed&&Time.unscaledTime-wrenchSeen<=.5f&&RemoteHeld(prop)&&prop.GetComponent<FusionGrabBinding>().Owner==wrenchActor;}
        void Update()
        {
            if(detached||!channel||!channel.Connected)return;
            if(channel.IsHost&&revision!=watch.Revision)
            {revision=watch.Revision;beds.Clear();micActor=wrenchActor=0;micPressed=wrenchPressed=false;UpdateStatus();}
            if(Time.unscaledTime>=nextSend)
            {
                nextSend=Time.unscaledTime+(channel.IsHost?.1f:.5f);
                if(channel.IsHost)
                {
                    var state=watch.CaptureState();++sequence;
                    foreach(var peer in nonces)
                        channel.SampleToActor(peer.Key,SnapshotKind,Encode(new Envelope{world=world,nonce=peer.Value,sequence=sequence,state=state}));
                }
                else channel.SendToHost(HelloKind,Encode(new Hello{world=world,nonce=nonce}));
            }
            if(Time.unscaledTime>=nextStatus){nextStatus=Time.unscaledTime+.5f;if(channel.IsHost){UpdateStatus();TryBed();}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                RefreshDebug();
#endif
            }
            if(Time.unscaledTime>=nextLog)
            {nextLog=Time.unscaledTime+5;Debug.Log($"COOP_STATE role={(channel.IsHost?"HOST":"GUEST")} ready={Ready} region=us room={crew.SessionName} players={crew.Players.Count} sequence={(channel.IsHost?sequence:receivedSequence)} {watch.DescribeState()}");}
        }
        void Receive(int sender,int kind,byte[] data)
        {
            if(detached||!channel||!channel.Connected||data==null)return;
            try
            {
                if(kind==HelloKind&&channel.IsHost&&channel.IsActorConnected(sender))
                {
                    var h=JsonUtility.FromJson<Hello>(Encoding.UTF8.GetString(data));
                    if(h==null||h.protocol!=Protocol||string.IsNullOrEmpty(h.nonce)||h.nonce.Length!=32)return;
                    if(!nonces.TryGetValue(sender,out var old)||old!=h.nonce)
                    {nonces[sender]=h.nonce;acknowledged.Remove(sender);commands.Remove(sender);beds.Remove(sender);}
                    if(h.world==world && acknowledged.Add(sender))
                    { notice="";UpdateStatus(); }
                }
                else if(kind==SnapshotKind&&!channel.IsHost&&sender==0)
                {
                    var s=JsonUtility.FromJson<Envelope>(Encoding.UTF8.GetString(data));
                    if(s==null||s.protocol!=Protocol||s.nonce!=nonce||s.sequence<=receivedSequence||string.IsNullOrEmpty(s.world)||world!=null&&s.world!=world)return;
                    if(!watch.ApplyState(s.state))return;world=s.world;receivedSequence=s.sequence;
                }
                else if(kind==CommandKind&&channel.IsHost&&channel.IsActorConnected(sender))
                {
                    var c=JsonUtility.FromJson<Command>(Encoding.UTF8.GetString(data));
                    bool local=sender==channel.LocalActorId;
                    if(c==null||c.world!=world||
                        (!local&&(!nonces.TryGetValue(sender,out var n)||c.nonce!=n||!acknowledged.Contains(sender)))||
                        !AllowRate(sender)||!commands.Accept(sender,c.sequence,c.revision,watch.Revision,c.action,c.value))return;
                    int result=Apply(sender,c);
                    channel.SendToActor(sender,ResultKind,Encode(new Result{world=world,nonce=c.nonce,sequence=c.sequence,action=c.action,result=result}));
                }
                else if(kind==ResultKind&&sender==0)
                {
                    var r=JsonUtility.FromJson<Result>(Encoding.UTF8.GetString(data));
                    if(r==null||r.world!=world||r.nonce!=nonce||r.sequence<=resultSequence)return;
                    resultSequence=r.sequence;watch.CommandResult(r.action,r.result);
                }
            }
            catch(ArgumentException){Debug.LogWarning("COOP rejected malformed packet");}
        }
        bool AllowRate(int actor)
        {
            if(!rateStart.TryGetValue(actor,out var start)||Time.unscaledTime-start>=1){rateStart[actor]=Time.unscaledTime;rateCount[actor]=0;}
            return ++rateCount[actor]<=40;
        }
        int Apply(int actor,Command c)
        {
            if(!Ready)return -1;
            if(c.action==CoopAction.Bed)
            {
                if(!poses.Sample(actor,out var pose)||Vector3.Distance(pose.Head,watch.BedPosition)>=1.8f)return -1;
                if(!watch.CanSleep)return watch.ApplyCommand(CoopAction.Bed,0,false);
                beds.Add(actor,true);UpdateStatus();TryBed();return 1;
            }
            if(c.action==CoopAction.Retry||c.action==CoopAction.Replay)return watch.ApplyCommand(c.action,c.value,c.pressed);
            var target=watch.Target(c.action,c.value);if(!target)return -1;
            if(!poses.NearHand(actor,target.transform.position,c.action==CoopAction.Microphone?1.2f:.85f))return -1;
            if(c.action==CoopAction.Starter||c.action==CoopAction.Microphone||c.action==CoopAction.WrenchClutch)
            {
                var binding=target.GetComponent<FusionGrabBinding>();
                if(!binding||!props.HasFreshLease(actor,binding.Id))return -1;
            }
            if(c.action==CoopAction.Starter)
            {
                if(lastPull.TryGetValue(actor,out var time)&&Time.unscaledTime-time<.25f)return -1;
                lastPull[actor]=Time.unscaledTime;
            }
            if(c.action==CoopAction.Microphone)
            {micActor=actor;micSeen=Time.unscaledTime;micPressed=c.pressed;return 1;}
            if(c.action==CoopAction.WrenchClutch)
            {wrenchActor=actor;wrenchSeen=Time.unscaledTime;wrenchPressed=c.pressed;return 1;}
            return watch.ApplyCommand(c.action,c.value,c.pressed);
        }
        void TryBed()
        {
            if(!watch.CanSleep)return;
            if(!beds.AllReady(channel.Runner.ActivePlayers.Select(p=>p.RawEncoded)))return;
            watch.BeginSharedSleep();
        }
        void UpdateStatus()
        {
            watch.CrewStatus(beds.Count,crew.Players.Count,notice);
            if(watch is ICoopBedPresentation lamps&&channel&&channel.Runner)
            {
                int mask=0;foreach(var p in channel.Runner.ActivePlayers)if(beds.Contains(p.RawEncoded))mask|=p==channel.Runner.LocalPlayer?1:2;
                lamps.SetBedMask(mask);
            }
        }
        void Left(int actor)
        {
            nonces.Remove(actor);acknowledged.Remove(actor);commands.Remove(actor);beds.Remove(actor);lastPull.Remove(actor);rateStart.Remove(actor);rateCount.Remove(actor);
            if(micActor==actor){micActor=0;micPressed=false;}
            if(wrenchActor==actor){wrenchActor=0;wrenchPressed=false;}
            if(channel&&channel.IsHost){notice="Other keeper disconnected. You can keep playing.";UpdateStatus();TryBed();}
        }
        void Connection()
        {
            if(channel&&channel.Connected||detached)return;
            detached=true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(debugLine)Destroy(debugLine.gameObject);
#endif
            watch.ContinueAlone("Crew disconnected. Continuing this watch alone.");
        }
        void OnDestroy(){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(debugLine)Destroy(debugLine.gameObject);
#endif
            if(channel){channel.Received-=Receive;channel.ActorLeft-=Left;channel.SessionChanged-=Connection;}}
    }
}
