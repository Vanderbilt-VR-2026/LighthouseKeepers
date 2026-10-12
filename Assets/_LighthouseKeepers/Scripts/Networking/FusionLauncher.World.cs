using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Fusion;
using LighthouseKeepers.Core;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Progression;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public sealed partial class FusionLauncher
{
    readonly GrabAuthority authority = new();
    readonly Dictionary<int, NetworkGrabbable> props = new();
    readonly Dictionary<PlayerRef, RemoteKeeper> avatars = new();
    internal readonly Dictionary<PlayerRef, AcceptedPose> acceptedPoses = new();
    internal struct AcceptedPose { public KeeperPose Pose; public int Sequence; public double Time; }
    XROrigin localRig;
    Transform leftHand, rightHand;
    FloodController flood;
    DayDirector days;
    bool floodWasEnabled, worldBound, worldConfirmed;
    string layoutHash;
    float nextScan, nextPose, nextSnapshot, nextAnnounce;
    int poseSequence, stateSequence;
    public bool CanInteract => IsOnline && worldBound && worldConfirmed;
    internal int LocalOwner => runner ? runner.LocalPlayer.RawEncoded : 0;

    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single) ClearWorld();
        nextScan = 0;
    }
    void Update()
    {
        if (Time.unscaledTime < nextScan || worldBound) return;
        nextScan = Time.unscaledTime + .5f;
        var bootstrap = FindAnyObjectByType<EnvironmentBootstrap>();
        if (bootstrap && !bootstrap.Ready) return;
        if (!SceneManager.GetSceneByName("LK_Bootstrap").isLoaded) return;
        BindWorld();
    }
    void BindWorld()
    {
        localRig = FindAnyObjectByType<XROrigin>();
        if (!localRig || !localRig.Camera) return;
        leftHand = localRig.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Left Controller");
        rightHand = localRig.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Right Controller");
        var grabs = FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(g => g.gameObject.scene.name.StartsWith("LK_", StringComparison.Ordinal) && g.GetComponent<Rigidbody>())
            .OrderBy(g => StablePath(g.transform), StringComparer.Ordinal).ToArray();
        var paths = grabs.Select(g => StablePath(g.transform)).ToArray();
        using (var hash = SHA256.Create())
            layoutHash = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(Protocol + "\n" + string.Join("\n", paths))));
        for (int i = 0; i < grabs.Length; i++)
        {
            var binding = grabs[i].gameObject.AddComponent<NetworkGrabbable>();
            binding.Configure(this, i + 1, grabs[i]);
            props.Add(i + 1, binding);
            authority.Register(i + 1);
        }
        flood = FindAnyObjectByType<FloodController>();
        floodWasEnabled = flood && flood.enabled;
        days = FindAnyObjectByType<DayDirector>();
        worldBound = true;
        nextAnnounce = 0;
        RefreshPhysics();
    }
    static string StablePath(Transform item)
    {
        var path = item.name + "[" + item.GetSiblingIndex() + "]";
        for (var parent = item.parent; parent; parent = parent.parent)
            path = parent.name + "[" + parent.GetSiblingIndex() + "]/" + path;
        return item.gameObject.scene.name + ":" + path;
    }
    void ClearWorld()
    {
        foreach (var binding in props.Values) if (binding) { binding.Unbind(); Destroy(binding); }
        props.Clear(); authority.Clear();
        foreach (var avatar in avatars.Values) if (avatar) Destroy(avatar.gameObject);
        avatars.Clear(); acceptedPoses.Clear(); worldReady.Clear();
        if (flood) flood.enabled = floodWasEnabled;
        localRig = null; leftHand = rightHand = null; flood = null; days = null;
        worldBound = worldConfirmed = false; layoutHash = null;
        poseSequence = stateSequence = 0; nextScan = nextAnnounce = 0;
    }
    void ResetReplication()
    {
        foreach (var avatar in avatars.Values) if (avatar) Destroy(avatar.gameObject);
        avatars.Clear(); acceptedPoses.Clear(); authority.Clear();
        foreach (var pair in props)
        {
            authority.Register(pair.Key);
            if (pair.Value) pair.Value.ResetSession();
        }
        worldConfirmed = false; poseSequence = stateSequence = 0; nextAnnounce = 0;
        RefreshPhysics();
    }
    void RefreshPhysics()
    {
        foreach (var binding in props.Values) if (binding) binding.RefreshPhysics();
        if (flood) flood.enabled = IsOnline && !IsHost ? false : floodWasEnabled;
    }
    void LateUpdate()
    {
        if (!IsOnline || !worldBound || !localRig || !localRig.Camera) return;
        if (!worldConfirmed && Time.unscaledTime >= nextAnnounce)
        {
            nextAnnounce = Time.unscaledTime + 1;
            if (IsHost) AcceptWorld(runner.LocalPlayer, layoutHash);
            else KeeperMessages.RPC_WorldReady(runner, PlayerRef.None, layoutHash);
        }
        if (Time.unscaledTime >= nextPose)
        {
            nextPose = Time.unscaledTime + .05f;
            var head = localRig.Camera.transform;
            var left = leftHand ? leftHand : head;
            var right = rightHand ? rightHand : head;
            var pose = new KeeperPose
            {
                Root = localRig.transform.position, RootRotation = localRig.transform.rotation,
                Head = head.position, HeadRotation = head.rotation,
                LeftHand = left.position, LeftRotation = left.rotation,
                RightHand = right.position, RightRotation = right.rotation
            };
            if (IsHost) AcceptPose(runner.LocalPlayer, pose, ++poseSequence);
            else KeeperMessages.RPC_SubmitPose(runner, PlayerRef.None, pose, ++poseSequence);
            foreach (var binding in props.Values)
            {
                if (!binding || !binding.IsLocallyHeld) continue;
                var held = binding.CaptureHeld();
                if (IsHost) AcceptHeldPose(runner.LocalPlayer, binding.Id, held);
                else KeeperMessages.RPC_HeldPose(runner, PlayerRef.None, binding.Id, held);
            }
        }
        if (!IsHost || Time.unscaledTime < nextSnapshot) return;
        nextSnapshot = Time.unscaledTime + 1f / 15;
        foreach (var id in authority.Expire(Time.unscaledTimeAsDouble)) PublishObject(id, true);
        foreach (var pair in props) if (pair.Value && pair.Value.gameObject.activeInHierarchy) PublishObject(pair.Key, false);
        if (flood) KeeperMessages.RPC_Environment(runner, flood.Height, flood.Paused, flood.RiseSpeed, days && days.Current ? days.Current.Day : 1);
    }
    internal void AcceptWorld(PlayerRef player, string hash)
    {
        if (!runner || !runner.IsServer || !worldBound || !crew.ContainsKey(player)) return;
        if (hash != layoutHash)
        {
            Debug.LogWarning("Refused incompatible Lighthouse world for player " + player);
            if (player != runner.LocalPlayer) runner.Disconnect(player);
            return;
        }
        worldReady.Add(player);
        if (player == runner.LocalPlayer) ConfirmWorld(hash);
        else KeeperMessages.RPC_WorldAccepted(runner, player, layoutHash);
        foreach (var id in props.Keys) PublishObject(id, true);
    }
    internal void ConfirmWorld(string hash)
    {
        if (hash != layoutHash || !worldBound) return;
        worldConfirmed = true;
        RefreshPhysics();
    }
    internal void AcceptPose(PlayerRef player, KeeperPose pose, int sequence)
    {
        if (!runner || !runner.IsServer || !crew.ContainsKey(player) || !Valid(pose)) return;
        if (acceptedPoses.TryGetValue(player, out var old) && sequence <= old.Sequence) return;
        acceptedPoses[player] = new AcceptedPose { Pose = pose, Sequence = sequence, Time = Time.unscaledTimeAsDouble };
        KeeperMessages.RPC_PlayerPose(runner, player, pose, sequence);
    }
    internal void RenderPose(PlayerRef player, KeeperPose pose, int sequence)
    {
        if (!worldBound || !runner || player == runner.LocalPlayer || !Valid(pose) || !roster.Any(p => p.Id == player.RawEncoded.ToString())) return;
        if (!avatars.TryGetValue(player, out var avatar) || !avatar)
        {
            avatar = new GameObject("Remote keeper " + player.RawEncoded).AddComponent<RemoteKeeper>();
            avatar.transform.SetParent(transform);
            avatar.Build(player.RawEncoded);
            avatars[player] = avatar;
        }
        avatar.SetPose(pose, sequence);
    }
    void RemoveAvatar(PlayerRef player)
    {
        if (avatars.TryGetValue(player, out var avatar) && avatar) Destroy(avatar.gameObject);
        avatars.Remove(player);
    }
    void PruneAvatars()
    {
        foreach (var player in avatars.Keys.ToArray())
            if (!roster.Any(p => p.Id == player.RawEncoded.ToString())) RemoveAvatar(player);
    }
    internal void RequestGrab(NetworkGrabbable binding)
    {
        if (!CanInteract) { binding.DenyGrab(); return; }
        if (IsHost) AcceptGrab(runner.LocalPlayer, binding.Id, binding.State.Epoch);
        else KeeperMessages.RPC_RequestGrab(runner, PlayerRef.None, binding.Id, binding.State.Epoch);
    }
    internal void AcceptGrab(PlayerRef player, int id, int epoch)
    {
        if (!runner || !runner.IsServer || !props.TryGetValue(id, out var binding) || !binding) return;
        bool near = worldReady.Contains(player) && acceptedPoses.TryGetValue(player, out var pose) &&
            Time.unscaledTimeAsDouble - pose.Time < GrabAuthority.LeaseTimeout && NearHands(pose.Pose, binding.AuthoritativePosition);
        if (!near || !authority.Acquire(id, player.RawEncoded, epoch, Time.unscaledTimeAsDouble))
        {
            if (player == runner.LocalPlayer) binding.DenyGrab();
            else KeeperMessages.RPC_GrabDenied(runner, player, id, epoch);
            return;
        }
        binding.ResetOwnerSequence();
        PublishObject(id, true);
    }
    internal void RejectGrab(int id, int epoch)
    {
        if (props.TryGetValue(id, out var binding) && binding && binding.State.Epoch <= epoch) binding.DenyGrab();
    }
    internal void AcceptHeldPose(PlayerRef player, int id, PropPose pose)
    {
        if (!runner || !runner.IsServer || !props.TryGetValue(id, out var binding) || !binding || !Valid(pose)) return;
        if (!worldReady.Contains(player) || !acceptedPoses.TryGetValue(player, out var rig) ||
            Time.unscaledTimeAsDouble - rig.Time >= GrabAuthority.LeaseTimeout ||
            !NearHands(rig.Pose, pose.Position) || pose.Sequence <= binding.LastOwnerSequence) return;
        if (!authority.Touch(id, player.RawEncoded, pose.Epoch, Time.unscaledTimeAsDouble)) return;
        binding.LastOwnerSequence = pose.Sequence;
        if (player != runner.LocalPlayer) binding.ApplyHeldOnHost(pose);
    }
    internal void ReleaseGrab(NetworkGrabbable binding, PropPose pose)
    {
        if (!IsOnline) return;
        if (IsHost) AcceptRelease(runner.LocalPlayer, binding.Id, pose);
        else KeeperMessages.RPC_Release(runner, PlayerRef.None, binding.Id, pose);
    }
    internal void AcceptRelease(PlayerRef player, int id, PropPose pose)
    {
        if (!runner || !runner.IsServer || !props.TryGetValue(id, out var binding) || !binding) return;
        if (!authority.Release(id, player.RawEncoded, pose.Epoch)) return;
        if (Valid(pose) && acceptedPoses.TryGetValue(player, out var rig) && NearHands(rig.Pose, pose.Position))
            binding.ApplyReleaseOnHost(pose);
        PublishObject(id, true);
    }
    void PublishObject(int id, bool reliable)
    {
        if (!runner || !runner.IsServer || !props.TryGetValue(id, out var binding) || !binding) return;
        var lease = authority.Find(id);
        var state = binding.CaptureState();
        state.Owner = lease.Owner; state.Epoch = lease.Epoch; state.Sequence = ++stateSequence;
        if (reliable) KeeperMessages.RPC_Ownership(runner, id, state);
        else KeeperMessages.RPC_ObjectPose(runner, id, state);
    }
    internal void ApplyObject(int id, PropPose state)
    {
        if (!worldBound || !Valid(state) || !props.TryGetValue(id, out var binding) || !binding) return;
        binding.ApplyState(state);
    }
    internal void ApplyEnvironment(float height, bool paused, float speed, int day)
    {
        if (IsHost || !worldBound || !flood || !Finite(height) || !Finite(speed)) return;
        flood.enabled = false;
        if (days && (!days.Current || days.Current.Day != day)) days.ApplyDay(Mathf.Clamp(day, 1, 5));
        flood.RiseSpeed = Mathf.Clamp(speed, 0, 10); flood.Paused = paused; flood.SetHeight(height);
    }
    static bool NearHands(KeeperPose pose, Vector3 position) =>
        (position - pose.Head).sqrMagnitude <= 36 &&
        Mathf.Min((position - pose.LeftHand).sqrMagnitude, (position - pose.RightHand).sqrMagnitude) <= 25;
    internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    static bool Valid(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z) && value.sqrMagnitude < 1000000;
    static bool Valid(Quaternion value)
    {
        if (!Finite(value.x) || !Finite(value.y) || !Finite(value.z) || !Finite(value.w)) return false;
        float length = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;
        return length > .8f && length < 1.2f;
    }
    static bool Valid(KeeperPose pose) => Valid(pose.Root) && Valid(pose.Head) && Valid(pose.LeftHand) && Valid(pose.RightHand) &&
        Valid(pose.RootRotation) && Valid(pose.HeadRotation) && Valid(pose.LeftRotation) && Valid(pose.RightRotation) &&
        (pose.Head - pose.Root).sqrMagnitude < 100 && (pose.LeftHand - pose.Head).sqrMagnitude < 36 && (pose.RightHand - pose.Head).sqrMagnitude < 36;
    static bool Valid(PropPose pose) => Valid(pose.Position) && Valid(pose.Rotation) && Valid(pose.Velocity) && Valid(pose.AngularVelocity);
}
