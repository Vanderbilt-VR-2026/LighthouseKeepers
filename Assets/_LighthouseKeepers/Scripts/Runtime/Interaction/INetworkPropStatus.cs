namespace LighthouseKeepers.Interaction
{
    public interface INetworkPropStatus
    {
        int Owner {get;}
        bool CanSimulateReleased {get;}
        float PoseInterval {get;}
    }
    // Optional policy for tools that rest kinematically until their first pickup.
    public interface INetworkReleasedBody { bool ReleasedBodyIsKinematic { get; } }
    public interface INetworkKeeperProximity { bool NearKeeper(UnityEngine.Vector3 point, float radius); }
    public interface INetworkPropRelease { void OnNetworkReleased(); }
}
