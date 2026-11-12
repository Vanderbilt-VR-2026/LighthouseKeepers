using UnityEngine;
namespace LighthouseKeepers.Lobby
{
    // The watch menu and diagnostics know only this optional crew service, never Photon.
    public interface ICrewVoice
    {
        string StatusText {get;}
        void ToggleMute();
    }
    public interface ICrewVoiceCapture {bool Muted {get;} bool MicrophoneAllowed {get;}}
    public static class CrewVoiceBridge
    {
        public static ICrewVoice Current {get;set;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>Current=null;
    }
}
