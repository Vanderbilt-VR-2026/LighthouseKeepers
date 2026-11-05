using Photon.Voice.Unity;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net
{
 // Separate behaviour preserves FusionVoiceClient's own Update lifecycle.
 public sealed class CrewAudioActivity:MonoBehaviour
 {
  public Recorder Recorder;
  void Update(){var meter=Recorder?Recorder.LevelMeter:null;KeeperAudioMix.LocalVoice(Recorder&&Recorder.TransmitEnabled&&meter!=null?meter.CurrentPeakAmp:0);}
  void OnDisable()=>KeeperAudioMix.LocalVoice(0);
 }
}
