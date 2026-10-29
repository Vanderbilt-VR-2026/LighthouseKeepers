using System;
using UnityEngine;
namespace LighthouseKeepers.Lobby
{
    public interface ICoopWatchView
    {
        Component Owner {get;}
        void SetNetworkMode(bool practice,bool replica);
        string CaptureState();
        bool ApplyState(string json);
        string DescribeState();
    }
    public interface ICoopWatchLink { bool Ready {get;} }
    // A gameplay seam; the transport implementation is supplied by LK.Net.
    public static class CoopWatchBridge
    {
        public static Func<ICoopWatchView,ICoopWatchLink> Attach;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>Attach=null;
    }
}
