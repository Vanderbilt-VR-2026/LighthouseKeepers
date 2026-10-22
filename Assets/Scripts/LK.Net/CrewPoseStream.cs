using System.Collections.Generic;
using Fusion;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Networking;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace LighthouseKeepers.Net
{
    [RequireComponent(typeof(FusionMessageChannel))]
    public sealed class CrewPoseStream : MonoBehaviour
    {
        const int SubmitKind = 10, StateKind = 11;
        readonly Dictionary<int, KeeperPoseBuffer> poses = new();
        readonly Dictionary<int, NetworkObject> players = new();
        FusionMessageChannel channel;
        FusionLobbyTransport lobby;
        XROrigin rig;
        Transform left, right;
        float nextSend, nextScan;
        int serial;
        byte leftPose, rightPose;
        float torsoYaw;
        public static CrewPoseStream Active { get; private set; }
        public FusionMessageChannel Channel => channel;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Active = null;
        void Awake()
        {
            channel = GetComponent<FusionMessageChannel>(); lobby = GetComponent<FusionLobbyTransport>();
            channel.Received += Receive; channel.SessionChanged += Clear; channel.ActorLeft += Remove;
        }
        public void SetHandPose(bool isLeft, byte sharedHandPose)
        { if (sharedHandPose <= 4) { if (isLeft) leftPose = sharedHandPose; else rightPose = sharedHandPose; } }
        public string NameFor(int actor)
        {
            if (channel.TryPlayer(actor, out var peer))
                foreach (var player in lobby.Players) if (player.Id == peer.PlayerId.ToString(System.Globalization.CultureInfo.InvariantCulture)) return player.DisplayName;
            return "Keeper";
        }
        public bool Sample(int actor, out KeeperPoseFrame pose)
        { pose = default; return poses.TryGetValue(actor, out var buffer) && buffer.Sample(Time.unscaledTimeAsDouble, out pose); }
        public bool NearHand(int actor, Vector3 point, float metres)
        {
            if (!Sample(actor, out var p)) return false;
            return Mathf.Min((p.Left-point).sqrMagnitude,(p.Right-point).sqrMagnitude) <= metres*metres;
        }
        void Update()
        {
            if (!channel.Connected) return;
            Active = this;
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + .5f;
            if (!rig)
            {
                rig = FindAnyObjectByType<XROrigin>();
                if (rig) foreach (var t in rig.GetComponentsInChildren<Transform>(true))
                { if (t.name == "Left Controller") left = t; if (t.name == "Right Controller") right = t; }
            }
            if (channel.IsHost && NetworkPrefabRegistry.Player)
                foreach (var peer in channel.Runner.ActivePlayers)
                    if (!players.TryGetValue(peer.RawEncoded,out var existing) || !existing || !existing.IsValid)
                    {
                        var player = channel.Runner.Spawn(NetworkPrefabRegistry.PlayerGuid, inputAuthority: peer);
                        if (player) { players[peer.RawEncoded] = player; channel.Runner.SetPlayerObject(peer, player); }
                    }
        }
        void LateUpdate()
        {
            if (!channel.Connected || !rig || !rig.Camera || Time.unscaledTime < nextSend) return;
            nextSend = Time.unscaledTime + .05f;
            var head = rig.Camera.transform;
            var l = left ? left : head; var r = right ? right : head;
            var frame = new KeeperPoseFrame { Actor = channel.LocalActorId, Sequence = ++serial,
                Head = head.position, HeadRotation = head.rotation, Left = l.position, LeftRotation = l.rotation,
                Right = r.position, RightRotation = r.rotation, LeftPose = leftPose, RightPose = rightPose };
            var ld = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand); var rd = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            ld.TryGetFeatureValue(CommonUsages.grip,out frame.LeftGrip); ld.TryGetFeatureValue(CommonUsages.trigger,out frame.LeftTrigger);
            rd.TryGetFeatureValue(CommonUsages.grip,out frame.RightGrip); rd.TryGetFeatureValue(CommonUsages.trigger,out frame.RightTrigger);
            torsoYaw=Mathf.LerpAngle(torsoYaw,head.eulerAngles.y,1-Mathf.Exp(-6*Time.unscaledDeltaTime));
            frame.TorsoYaw=Mathf.Repeat(torsoYaw,360);
            frame.LeftPose=PoseFor(left,frame.LeftGrip,frame.LeftTrigger);
            frame.RightPose=PoseFor(right,frame.RightGrip,frame.RightTrigger);
            channel.SendToHost(SubmitKind,frame.Encode());
        }
        static byte PoseFor(Transform controller,float grip,float trigger)
        {
            var hand=controller?controller.GetComponentInChildren<HandRig>():null;
            if(hand && hand.Direct && hand.Direct.interactablesSelected.Count>0)
            {
                var target=hand.Direct.interactablesSelected[0].transform.GetComponent<IHandPoseTarget>();
                return (byte)(target?.RequiredPose??HandPose.WrapGrip);
            }
            return (byte)(grip>.5f?HandPose.Fist:trigger>.5f?HandPose.Pinch:HandPose.Open);
        }
        void Receive(int sender, int kind, byte[] data)
        {
            if (kind != SubmitKind && kind != StateKind) return;
            if (!KeeperPoseFrame.TryDecode(data,out var pose)) return;
            if (kind == SubmitKind)
            {
                if (!channel.IsHost || !channel.IsActorConnected(sender)) return;
                pose.Actor = sender; // Replace the untrusted payload's identity.
                if (Accept(pose)) channel.Broadcast(StateKind,pose.Encode());
            }
            else if (sender == 0 && !channel.IsHost && channel.IsActorConnected(pose.Actor)) Accept(pose);
        }
        bool Accept(KeeperPoseFrame pose)
        {
            if (!poses.TryGetValue(pose.Actor, out var buffer)) poses[pose.Actor] = buffer = new KeeperPoseBuffer();
            return buffer.Add(pose,Time.unscaledTimeAsDouble);
        }
        void Remove(int actor)
        {
            poses.Remove(actor);
            if (players.TryGetValue(actor,out var obj) && obj && channel.IsHost) channel.Runner.Despawn(obj);
            players.Remove(actor);
        }
        void Clear() { poses.Clear(); players.Clear(); serial = 0; if (Active == this) Active = null; }
        void OnDestroy()
        { channel.Received -= Receive; channel.SessionChanged -= Clear; channel.ActorLeft -= Remove; Clear(); }
    }
}
