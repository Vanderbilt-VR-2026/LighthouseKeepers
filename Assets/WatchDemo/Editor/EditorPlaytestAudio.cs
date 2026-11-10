#if UNITY_EDITOR
using UnityEditor;
namespace LighthouseKeepers.Editor
{
 // Editor output only: microphone/network tests and shipped player audio are unchanged.
 [InitializeOnLoad] public static class EditorPlaytestAudio
 {
  static EditorPlaytestAudio(){Mute();EditorApplication.playModeStateChanged+=_=>Mute();}
  static void Mute(){EditorUtility.audioMasterMute=true;}
 }
}
#endif
