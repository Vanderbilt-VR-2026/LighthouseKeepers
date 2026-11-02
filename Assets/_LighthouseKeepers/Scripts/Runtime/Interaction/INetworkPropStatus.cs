namespace LighthouseKeepers.Interaction
{
    public interface INetworkPropStatus
    {
        int Owner {get;}
        bool CanSimulateReleased {get;}
        float PoseInterval {get;}
    }
    public interface INetworkPropRelease { void OnNetworkReleased(); }
}
