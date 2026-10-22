using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusion;
using LighthouseKeepers.Core;
using LighthouseKeepers.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Net
{
    /// <summary>Extends William's epoch/lease arbitration to the new watch stations on the same runner.</summary>
    [RequireComponent(typeof(FusionMessageChannel),typeof(CrewPoseStream))]
    public sealed class FusionGrabAuthority : MonoBehaviour
    {
        const int RequestKind = 20, StateKind = 21, DeniedKind = 22;
        readonly GrabAuthority leases = new();
        readonly Dictionary<int, FusionGrabBinding> props = new();
        readonly Dictionary<int, NetworkObject> authorityObjects = new();
        readonly Dictionary<int, int> ownerSequences = new();
        readonly Dictionary<int, double> requestTimes = new();
        readonly Dictionary<int, int> requestCounts = new();
        FusionMessageChannel channel;
        CrewPoseStream poses;
        float nextScan, nextPublish;
        int stateSequence;
        public CoopStationGate Gate { get; } = new();
        public bool IsHost => channel.IsHost;
        public int LocalActor => channel.LocalActorId;
        [Serializable] public sealed class Packet
        {
            public int id, epoch, owner, sequence;
            public string path;
            public int action; // 0 acquire, 1 held pose/heartbeat, 2 release, 3 authoritative state
            public Vector3 position;
            public Quaternion rotation;
        }
        void Awake()
        {
            channel = GetComponent<FusionMessageChannel>(); poses = GetComponent<CrewPoseStream>();
            channel.Received += Receive; channel.SessionChanged += Clear; channel.ActorLeft += Left;
            SceneManager.sceneLoaded += SceneLoaded;
        }
        void SceneLoaded(Scene scene,LoadSceneMode mode) { Clear(); nextScan = 0; }
        void Update()
        {
            if (!channel.Connected) return;
            if (Time.unscaledTime >= nextScan)
            {
                nextScan = Time.unscaledTime + 1;
                var bootstrap = FindAnyObjectByType<EnvironmentBootstrap>();
                if (bootstrap && !bootstrap.Ready) return;
                if (props.Count == 0) BindProps();
            }
            if (!IsHost || Time.unscaledTime < nextPublish) return;
            nextPublish = Time.unscaledTime + 1f/15;
            foreach (var id in leases.Expire(Time.unscaledTimeAsDouble)) Publish(id);
            foreach (var id in props.Keys) Publish(id);
        }
        void BindProps()
        {
            var candidates = FindObjectsByType<XRBaseInteractable>(FindObjectsSortMode.None)
                .Where(Eligible).OrderBy(g => Path(g.transform),StringComparer.Ordinal).ToArray();
            for (int i=0;i<candidates.Length;i++)
            {
                int id = i+1;
                var binding = candidates[i].gameObject.AddComponent<FusionGrabBinding>();
                binding.Bind(this,id,Path(candidates[i].transform),candidates[i]); props.Add(id,binding); leases.Register(id);
                if (IsHost && NetworkPrefabRegistry.PropAuthority)
                {
                    var obj = channel.Runner.Spawn(NetworkPrefabRegistry.PropGuid,inputAuthority: channel.Runner.LocalPlayer);
                    if (obj) authorityObjects.Add(id,obj);
                }
            }
        }
        static bool Eligible(XRBaseInteractable value)
        {
            if(value is XRGrabInteractable && value.name.IndexOf("wrench",StringComparison.OrdinalIgnoreCase)>=0)return true;
            // Only E's three requested interactions. Do not take ownership of lifelines, UI, or unrelated puzzles.
            for (var t=value.transform;t;t=t.parent)
                if (t.name=="STN_MANUAL_PUMP" || t.name=="STN_TOOL_TABLE" || t.name=="GRIP_StarterHandle" ||
                    t.name=="STN_PIPE_LEAK" || t.name.StartsWith("Watch wrench",StringComparison.Ordinal) ||
                    t.name=="PUMP / grip and stroke" || t.name=="GENERATOR / grip and turn" || t.name=="BRACE / hold coupling") return true;
            return false;
        }
        static string Path(Transform item)
        {
            string path=item.name;
            for (var parent=item.parent;parent;parent=parent.parent) path=parent.name+"/"+path;
            return item.gameObject.scene.name+":"+path;
        }
        public void Request(FusionGrabBinding binding,int action,int sequence)
        {
            if (!channel.Connected) { binding.Reject(); return; }
            var packet=new Packet { id=binding.Id,path=binding.Path,epoch=binding.Epoch,sequence=sequence,action=action,
                position=binding.transform.position,rotation=binding.transform.rotation };
            channel.SendToHost(RequestKind,Encode(packet));
        }
        static byte[] Encode(Packet packet) => Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet));
        void Receive(int sender,int kind,byte[] data)
        {
            if (kind!=RequestKind && kind!=StateKind && kind!=DeniedKind) return;
            Packet p;
            try { p=JsonUtility.FromJson<Packet>(Encoding.UTF8.GetString(data)); } catch (ArgumentException) { return; }
            if (p==null || !props.TryGetValue(p.id,out var binding) || !binding || p.path!=binding.Path) return;
            if (kind==RequestKind)
            {
                if (!IsHost || !channel.IsActorConnected(sender) || !AllowRequest(sender)) return;
                if (p.action==0)
                {
                    bool accepted=poses.NearHand(sender,binding.AuthoritativePosition,.8f) && leases.Acquire(p.id,sender,p.epoch,Time.unscaledTimeAsDouble);
                    if (!accepted) { channel.SendToActor(sender,DeniedKind,Encode(p)); return; }
                    ownerSequences[p.id]=0; Publish(p.id); return;
                }
                if (p.action==2)
                {
                    if (leases.Release(p.id,sender,p.epoch))
                    { Gate.Observe(sender,p.id,false,Time.unscaledTimeAsDouble); Publish(p.id); }
                    return;
                }
                if (p.action!=1 || !ValidPose(p) || !poses.NearHand(sender,p.position,1.2f) ||
                    p.sequence<=0 || ownerSequences.TryGetValue(p.id,out var previous) && p.sequence<=previous) return;
                if (!leases.Touch(p.id,sender,p.epoch,Time.unscaledTimeAsDouble)) return;
                ownerSequences[p.id]=p.sequence;
                if (sender!=LocalActor) binding.ApplyHeld(p.position,p.rotation);
                Gate.Observe(sender,p.id,true,Time.unscaledTimeAsDouble);
            }
            else if (sender==0 && kind==DeniedKind) binding.Reject(p.epoch);
            else if (sender==0 && kind==StateKind && ValidPose(p)) binding.ApplyState(p);
        }
        bool AllowRequest(int actor)
        {
            double now=Time.unscaledTimeAsDouble;
            if (!requestTimes.TryGetValue(actor,out var start) || now-start>=1)
            { requestTimes[actor]=now; requestCounts[actor]=0; }
            return ++requestCounts[actor]<=100;
        }
        static bool ValidPose(Packet p)
        {
            var v=p.position;var q=p.rotation;
            float norm=q.x*q.x+q.y*q.y+q.z*q.z+q.w*q.w;
            return KeeperPoseFrame.Finite(v.x)&&KeeperPoseFrame.Finite(v.y)&&KeeperPoseFrame.Finite(v.z)&&v.sqrMagnitude<1000000 &&
                KeeperPoseFrame.Finite(norm)&&norm>.8f&&norm<1.2f;
        }
        void Publish(int id)
        {
            if (!props.TryGetValue(id,out var binding) || !binding) return;
            var lease=leases.Find(id);
            if (authorityObjects.TryGetValue(id,out var obj) && obj && obj.IsValid)
            {
                var owner=channel.TryPlayer(lease.Owner,out var peer)?peer:channel.Runner.LocalPlayer;
                if (obj.InputAuthority!=owner) obj.AssignInputAuthority(owner);
            }
            channel.Broadcast(StateKind,Encode(new Packet { id=id,path=binding.Path,epoch=lease.Epoch,owner=lease.Owner,sequence=++stateSequence,action=3,
                position=binding.transform.position,rotation=binding.transform.rotation }));
        }
        public bool HasFreshWrenchLease(int actor)
        {
            foreach(var pair in props)
                if(pair.Value && pair.Value.name.IndexOf("wrench",StringComparison.OrdinalIgnoreCase)>=0 && HasFreshLease(actor,pair.Key))return true;
            return false;
        }
        public bool HasFreshLease(int actor,int station)
        {
            var lease=leases.Find(station);
            return IsHost && lease!=null && lease.Owner==actor && channel.IsActorConnected(actor) &&
                Time.unscaledTimeAsDouble-lease.LastUpdate<=CoopStationGate.HoldTimeoutSeconds;
        }
        public bool TryStationId(string anchor,out int id)
        {
            foreach(var pair in props)
                if(pair.Value) for(var at=pair.Value.transform;at;at=at.parent)
                    if(at.name==anchor){id=pair.Key;return true;}
            id=0;return false;
        }
        void Left(int actor)
        {
            Gate.RemoveActor(actor);requestTimes.Remove(actor);requestCounts.Remove(actor);
            if (IsHost) foreach (var id in leases.ReleasePlayer(actor)) Publish(id);
        }
        void Clear()
        {
            foreach (var binding in props.Values) if (binding) { binding.Unbind(); Destroy(binding); }
            foreach (var obj in authorityObjects.Values) if (obj && obj.IsValid && IsHost) channel.Runner.Despawn(obj);
            props.Clear(); authorityObjects.Clear(); leases.Clear();ownerSequences.Clear();requestTimes.Clear();requestCounts.Clear();Gate.Clear();stateSequence=0;
        }
        void OnDestroy()
        { SceneManager.sceneLoaded-=SceneLoaded; channel.Received-=Receive;channel.SessionChanged-=Clear;channel.ActorLeft-=Left;Clear(); }
    }
}
