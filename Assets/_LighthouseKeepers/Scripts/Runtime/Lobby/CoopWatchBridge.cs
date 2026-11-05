using System;
using UnityEngine;
namespace LighthouseKeepers.Lobby
{
    public enum CoopAction { Starter, RadioPower, RadioTune, RadioCode, Microphone, Bed, Retry, Replay, GridCircuit, GridReset, EnginePrime, EngineChoke, BeaconIgnition, BeaconAlign, WrenchClutch, PAReplay, StatusRequest }
    public interface ICoopWatchView
    {
        Component Owner {get;}
        int Revision {get;}
        bool CanSleep {get;}
        Vector3 BedPosition {get;}
        Component Target(CoopAction action, float value);
        int ApplyCommand(CoopAction action,float value,bool pressed);
        void CommandResult(CoopAction action,int result);
        void BeginSharedSleep();
        void CrewStatus(int beds,int keepers,string notice);
        void ContinueAlone(string notice);
        void SetNetworkMode(bool practice,bool replica);
        string CaptureState();
        bool ApplyState(string json);
        string DescribeState();
    }
    public interface ICoopKeeperLocations {
        int LocalActor {get;}
        void Locations(out int first,out Vector3 firstHead,out int second,out Vector3 secondHead);
    }
    public interface ICoopBedPresentation { void SetBedMask(int mask); }
    public interface ICoopWrenchClutch
    {
        bool RemoteWrenchTrigger(Component prop);
        bool RemoteWrenchKeeper(Component prop,out Vector3 head);
    }
    public interface ICoopWatchLink
    {
        bool Ready {get;}
        bool Send(CoopAction action,float value=0,bool pressed=false);
        bool RemoteHeld(Component prop);
        bool RemoteMicrophone(Component prop,out bool trigger);
    }
    // Gameplay has no dependency on Fusion. Only an online crew installs a link.
    public static class CoopWatchBridge
    {
        public static Func<ICoopWatchView,ICoopWatchLink> Attach;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>Attach=null;
    }
}
