using System.Runtime.InteropServices;
using Fusion;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct KeeperPose : INetworkStruct
{
    public Vector3 Root, Head, LeftHand, RightHand;
    public Quaternion RootRotation, HeadRotation, LeftRotation, RightRotation;
}

[StructLayout(LayoutKind.Sequential)]
public struct PropPose : INetworkStruct
{
    public Vector3 Position, Velocity, AngularVelocity;
    public Quaternion Rotation;
    public int Owner, Epoch, Sequence;
}

/// <summary>Fusion's static RPCs work without baked NetworkObjects. The installed Assembly-CSharp weaver processes these.</summary>
public sealed class KeeperMessages : SimulationBehaviour
{
    static FusionLauncher Session(NetworkRunner runner) => FusionLauncher.Instance && FusionLauncher.Instance.SessionRunner == runner
        ? FusionLauncher.Instance : null;
    // Static RPCs do not enforce RpcSources/RpcTargets. Always verify the authenticated sender explicitly.
    static bool FromHost(NetworkRunner runner, RpcInfo info) => info.Source == PlayerRef.None && Session(runner) != null;

    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false)]
    public static void RPC_Profile(NetworkRunner runner, [RpcTarget] PlayerRef target, string name, bool ready, RpcInfo info = default)
    {
        if (runner.IsServer) Session(runner)?.SetProfile(info.Source, name, ready);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false, Channel = RpcChannel.ReliableLargeData)]
    public static void RPC_Roster(NetworkRunner runner, string json, RpcInfo info = default)
    {
        if (FromHost(runner, info)) Session(runner).ApplyRoster(json);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false)]
    public static void RPC_BeginWatch(NetworkRunner runner, [RpcTarget] PlayerRef target, RpcInfo info = default)
    {
        if (FromHost(runner, info)) Session(runner).EnterWatch();
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false)]
    public static void RPC_WorldReady(NetworkRunner runner, [RpcTarget] PlayerRef target, string layout, RpcInfo info = default)
    {
        if (runner.IsServer) Session(runner)?.AcceptWorld(info.Source, layout);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false)]
    public static void RPC_WorldAccepted(NetworkRunner runner, [RpcTarget] PlayerRef target, string layout, RpcInfo info = default)
    {
        if (FromHost(runner, info)) Session(runner).ConfirmWorld(layout);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false, Channel = RpcChannel.Unreliable)]
    public static void RPC_SubmitPose(NetworkRunner runner, [RpcTarget] PlayerRef target, KeeperPose pose, int sequence, RpcInfo info = default)
    {
        if (runner.IsServer) Session(runner)?.AcceptPose(info.Source, pose, sequence);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false, Channel = RpcChannel.Unreliable)]
    public static void RPC_PlayerPose(NetworkRunner runner, PlayerRef player, KeeperPose pose, int sequence, RpcInfo info = default)
    {
        if (FromHost(runner, info)) Session(runner).RenderPose(player, pose, sequence);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false)]
    public static void RPC_RequestGrab(NetworkRunner runner, [RpcTarget] PlayerRef target, int objectId, int epoch, RpcInfo info = default)
    {
        if (runner.IsServer) Session(runner)?.AcceptGrab(info.Source, objectId, epoch);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false)]
    public static void RPC_GrabDenied(NetworkRunner runner, [RpcTarget] PlayerRef target, int objectId, int epoch, RpcInfo info = default)
    {
        if (FromHost(runner, info)) Session(runner).RejectGrab(objectId, epoch);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false, Channel = RpcChannel.Unreliable)]
    public static void RPC_HeldPose(NetworkRunner runner, [RpcTarget] PlayerRef target, int objectId, PropPose pose, RpcInfo info = default)
    {
        if (runner.IsServer) Session(runner)?.AcceptHeldPose(info.Source, objectId, pose);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false)]
    public static void RPC_Release(NetworkRunner runner, [RpcTarget] PlayerRef target, int objectId, PropPose pose, RpcInfo info = default)
    {
        if (runner.IsServer) Session(runner)?.AcceptRelease(info.Source, objectId, pose);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false)]
    public static void RPC_Ownership(NetworkRunner runner, int objectId, PropPose pose, RpcInfo info = default)
    {
        if (FromHost(runner, info)) Session(runner).ApplyObject(objectId, pose);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false, Channel = RpcChannel.Unreliable)]
    public static void RPC_ObjectPose(NetworkRunner runner, int objectId, PropPose pose, RpcInfo info = default)
    {
        if (FromHost(runner, info)) Session(runner).ApplyObject(objectId, pose);
    }
    [Rpc(RpcSources.All, RpcTargets.All, TickAligned = false, Channel = RpcChannel.Unreliable)]
    public static void RPC_Environment(NetworkRunner runner, float height, bool paused, float speed, int day, RpcInfo info = default)
    {
        if (FromHost(runner, info)) Session(runner).ApplyEnvironment(height, paused, speed, day);
    }
}
