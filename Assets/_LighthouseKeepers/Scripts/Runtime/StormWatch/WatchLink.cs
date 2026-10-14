using UnityEngine;
namespace LighthouseKeepers.StormWatch
{
    public abstract class WatchLink : MonoBehaviour
    {
        public abstract bool Connected { get; }
        public abstract bool IsHost { get; }
        public abstract int LocalSlot { get; }
        public abstract bool PartnerConnected { get; }
        public abstract string Status { get; }
        public abstract KeeperInput RemoteInput { get; }
        public abstract WatchState ReceivedState { get; }
        public abstract void Connect(bool host, string room);
        public abstract void SendInput(KeeperInput input);
        public abstract void Publish(WatchState state);
        public abstract void Leave();
    }
}
