namespace LighthouseKeepers.Interaction
{
    public interface INetworkPropStatus
    {
        int Owner {get;}
        bool CanSimulateReleased {get;}
    }
    public interface INetworkPropRelease { void OnNetworkReleased(); }
}
