using System;
using System.Collections.Generic;
using System.Text;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Net
{
    // One state stream on Will's existing Fusion runner. No guest mission writes in step 1.
    public sealed class WatchCoopSession : MonoBehaviour, ICoopWatchLink
    {
        const int HelloKind=40, SnapshotKind=41;
        [Serializable] sealed class Hello { public string protocol=Protocol,world; }
        const string Protocol="LK-Lab-Coop-Step1-v1";
        [Serializable] sealed class Envelope {public string protocol=Protocol,world,state;public int sequence;}
        ICoopWatchView watch;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()=>CoopWatchBridge.Attach=Attach;
        static ICoopWatchLink Attach(ICoopWatchView view)
        {
            var crew=FindAnyObjectByType<FusionLobbyTransport>();
            if(!crew || !crew.IsOnline)return null;
            bool shared=ShipWorld.CoopEnabled && !crew.IsNetworkRehearsal;
            view.SetNetworkMode(!shared && (crew.IsNetworkRehearsal || !crew.IsHost || crew.Players.Count>1),shared && !crew.IsHost);
            if(!shared)return null;
            var link=view.Owner.gameObject.AddComponent<WatchCoopSession>();link.Initialize(view,crew);return link;
        }
        FusionLobbyTransport crew;
        FusionMessageChannel channel;
        readonly Dictionary<int,float> acknowledged=new();
        string world;
        int sequence,receivedSequence;
        float nextSend,lastSnapshot,nextLog;
        public bool Ready
        {
            get
            {
                if(!channel || !channel.Connected)return false;
                if(!channel.IsHost)return receivedSequence>0 && Time.unscaledTime-lastSnapshot<4;
                foreach(var peer in crew.Players)
                {
                    if(peer.IsHost)continue;
                    if(!int.TryParse(peer.Id,out var playerId))return false;
                    bool found=false;
                    foreach(var player in channel.Runner.ActivePlayers)
                        if(player.PlayerId==playerId && acknowledged.TryGetValue(player.RawEncoded,out var time) && Time.unscaledTime-time<4)found=true;
                    if(!found)return false;
                }
                return true;
            }
        }
        public void Initialize(ICoopWatchView controller,FusionLobbyTransport transport)
        {
            watch=controller;crew=transport;channel=crew.GetComponent<FusionMessageChannel>();
            if(!channel){Debug.LogError("COOP missing existing message channel");return;}
            world=crew.IsHost?Guid.NewGuid().ToString("N"):null;
            channel.Received+=Receive;
            PlaceKeeper(crew.IsHost);
            if(!crew.IsHost)
                foreach(var interaction in FindObjectsByType<XRBaseInteractable>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                    if(interaction.gameObject.scene==gameObject.scene)interaction.enabled=false;
            Debug.Log($"COOP_ROLE {(crew.IsHost?"HOST":"GUEST")} readOnly={!crew.IsHost} world={world}");
        }
        void PlaceKeeper(bool host)
        {
            var rig=FindAnyObjectByType<XROrigin>();if(!rig)return;
            var collider=rig.GetComponent<CharacterController>();bool enabled=collider&&collider.enabled;
            if(collider)collider.enabled=false;
            // Accepted House spawn is (-.9,.04,-8.4), facing the west-wall board.
            // Offset along Z rather than toward the desk or the wall; preserve floor-relative tracking.
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
        void Update()
        {
            if(!channel || !channel.Connected || Time.unscaledTime<nextSend)return;
            nextSend=Time.unscaledTime+(channel.IsHost?.2f:1f);
            if(channel.IsHost)
            {
                var snapshot=new Envelope{world=world,sequence=++sequence,state=watch.CaptureState()};
                channel.Broadcast(SnapshotKind,Encoding.UTF8.GetBytes(JsonUtility.ToJson(snapshot)));
            }
            else channel.SendToHost(HelloKind,Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Hello{world=world})));
            if(Time.unscaledTime>=nextLog)
            {
                nextLog=Time.unscaledTime+5;
                Debug.Log($"COOP_STATE role={(channel.IsHost?"HOST":"GUEST")} ready={Ready} sequence={(channel.IsHost?sequence:receivedSequence)} {watch.DescribeState()}");
            }
        }
        void Receive(int sender,int kind,byte[] data)
        {
            if(!channel || !channel.Connected || data==null)return;
            try
            {
                if(kind==HelloKind && channel.IsHost && channel.IsActorConnected(sender))
                {
                    var hello=JsonUtility.FromJson<Hello>(Encoding.UTF8.GetString(data));
                    if(hello!=null && hello.protocol==Protocol && hello.world==world)acknowledged[sender]=Time.unscaledTime;
                }
                else if(kind==SnapshotKind && !channel.IsHost && sender==0)
                {
                    var s=JsonUtility.FromJson<Envelope>(Encoding.UTF8.GetString(data));
                    if(s==null || s.protocol!=Protocol || s.sequence<=receivedSequence || string.IsNullOrEmpty(s.world) || world!=null && s.world!=world)return;
                    if(!watch.ApplyState(s.state))return;
                    world=s.world;receivedSequence=s.sequence;lastSnapshot=Time.unscaledTime;
                }
            }
            catch(ArgumentException){Debug.LogWarning("COOP rejected malformed state packet");}
        }
        void OnDestroy(){if(channel)channel.Received-=Receive;}
    }
}
