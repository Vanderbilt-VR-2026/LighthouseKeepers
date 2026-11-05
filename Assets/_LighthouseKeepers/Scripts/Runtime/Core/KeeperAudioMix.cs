using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
namespace LighthouseKeepers.Core
{
 public sealed class KeeperAudioMix:MonoBehaviour
 {
  public static KeeperAudioMix Instance{get;private set;}
  static readonly string[] buses={"VoiceChat","Radio","SFX_Critical","SFX_World","Ambience","Music"};
  readonly Dictionary<string,AudioMixerGroup> groups=new();readonly HashSet<int> routed=new();readonly List<AudioSignalMeter> meters=new();
  readonly Dictionary<string,float> gains=new(),last=new();readonly Dictionary<string,string> exposed=new();
  AudioMixer mixer;AudioPassSettings settings;float scanUntil,nextScan,musicDuck,worldDuck,radioDuck,localPeak,saveAt=-1;
  public bool VoiceActive{get;private set;}public bool RadioActive{get;private set;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetStatic()=>Instance=null;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot()
  {
   var s=AudioPassSettings.Current;if(!s||!s.audioMix||Instance)return;
   var g=new GameObject("Keeper audio mix");DontDestroyOnLoad(g);var m=g.AddComponent<KeeperAudioMix>();Instance=m;m.settings=s;m.mixer=Resources.Load<AudioMixer>("KeeperAudio");
   if(!m.mixer){Destroy(g);return;}
   foreach(var n in new[]{"Music","Effects","Radio","VoiceChat"})m.gains[n]=Gain(n);
   foreach(var b in buses){m.exposed[b]=b+"Volume";var found=m.mixer.FindMatchingGroups(b);if(found.Length>0)m.groups[b]=found[0];}
   var config=AudioSettings.GetConfiguration();config.numRealVoices=24;config.numVirtualVoices=64;AudioSettings.Reset(config);
   m.scanUntil=Time.unscaledTime+2;SceneManager.sceneLoaded+=m.SceneLoaded;
  }
  void SceneLoaded(Scene s,LoadSceneMode mode){routed.Clear();scanUntil=Time.unscaledTime+2;nextScan=0;}
  public static float Gain(string name)=>PlayerPrefs.GetFloat("LK.Audio."+name,1);
  public static void SetGain(string name,float gain){PlayerPrefs.SetFloat("LK.Audio."+name,Mathf.Clamp01(gain));if(Instance){Instance.gains[name]=Mathf.Clamp01(gain);Instance.saveAt=Time.unscaledTime+.5f;}}
  public static void LocalVoice(float peak){if(Instance)Instance.localPeak=peak;}
  public static void Route(AudioSource source,string bus)
  {if(Instance)Instance.Assign(source,bus);}
  void Assign(AudioSource source,string bus)
  {
   if(!source||!groups.TryGetValue(bus,out var group))return;
   if(SceneManager.GetActiveScene().name=="LK_NetworkLobby"&&source.clip&&source.clip.name.StartsWith("Placeholder_"))source.Stop();
   source.outputAudioMixerGroup=group;source.priority=bus=="VoiceChat"?0:bus=="Radio"?10:bus=="SFX_Critical"?40:bus=="SFX_World"?90:bus=="Ambience"?140:200;
   if(!routed.Add(source.GetInstanceID()))return;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
   if(bus=="VoiceChat")Debug.Log("AUDIO_VOICE_ROUTE name="+source.name+" group=VoiceChat priority="+source.priority);
#endif
   if(bus=="VoiceChat"&&!source.GetComponent<SpeechLeveler>())source.gameObject.AddComponent<SpeechLeveler>();
   if(bus=="VoiceChat"||bus=="Radio"){var meter=source.GetComponent<AudioSignalMeter>();if(!meter)meter=source.gameObject.AddComponent<AudioSignalMeter>();meter.Source=source;meter.Voice=bus=="VoiceChat";if(!meters.Contains(meter))meters.Add(meter);}
  }
  static string Classify(AudioSource a)
  {
   if(a.GetComponent<SpeechLeveler>())return "VoiceChat"; // Photon renames its Speaker after linking.
   string n=a.name.ToLowerInvariant();if(n.Contains("remote keeper voice"))return "VoiceChat";
   foreach(var p in a.GetComponentsInParent<MonoBehaviour>(true))if(p&&(p.GetType().Name=="HarborRadio"||p.GetType().Name=="PAIntercom"))return "Radio";
   if(n.Contains("adaptive music"))return "Music";
   if(n.Contains("ambience")||n.Contains("rain")||n.Contains("gust")||n.Contains("ocean")||n.Contains("weather")||n.Contains("window")||n.Contains("heartbeat"))return "Ambience";
   if(n.Contains("pipe")||n.Contains("engine")||n.Contains("generator")||n.Contains("beacon")||n.Contains("radio")||n.Contains("alarm")||n.Contains("breaker")||n.Contains("stings"))return "SFX_Critical";
   return "SFX_World";
  }
  void Update()
  {
   if(Time.unscaledTime<=scanUntil&&Time.unscaledTime>=nextScan)
   {
    nextScan=Time.unscaledTime+.4f;
    foreach(var a in FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(!routed.Contains(a.GetInstanceID()))Assign(a,Classify(a));
    foreach(var listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
    {
     if(!listener.isActiveAndEnabled||listener.GetComponent<AudioCeiling>())continue;
     if(listener.GetComponent<AudioSource>())
     {
      // Keep the tracked camera and its SFX untouched. Only the audio listener
      // moves to an identical local pose, with exactly one active listener.
      var endpoint=new GameObject("Master audio listener endpoint");endpoint.transform.SetParent(listener.transform,false);
      listener.enabled=false;Destroy(listener);endpoint.AddComponent<AudioListener>();endpoint.AddComponent<AudioCeiling>();
     }
     else listener.gameObject.AddComponent<AudioCeiling>();
    }

   }
   VoiceActive=localPeak>.012f;RadioActive=false;
   for(int i=meters.Count-1;i>=0;i--){var m=meters[i];if(!m){meters.RemoveAt(i);continue;}if(!m.Active)continue;if(m.Voice)VoiceActive=true;else RadioActive=true;}
   bool duck=VoiceActive||RadioActive;
   musicDuck=Fade(musicDuck,RadioActive?-9:duck?-7:0,duck,RadioActive?9:7);worldDuck=Fade(worldDuck,RadioActive?-5:duck?-3:0,duck,RadioActive?5:3);radioDuck=Fade(radioDuck,VoiceActive?-3:0,VoiceActive,3);
   Set("VoiceChat",0,gains["VoiceChat"]);Set("Radio",1.7f+radioDuck,gains["Radio"]);Set("Music",-2+musicDuck,gains["Music"]);Set("Ambience",-6.5f+worldDuck,gains["Effects"]);Set("SFX_Critical",-3,gains["Effects"]);Set("SFX_World",-6,gains["Effects"]);
   if(saveAt>=0&&Time.unscaledTime>=saveAt){PlayerPrefs.Save();saveAt=-1;}
  }
  static float Fade(float v,float target,bool attack,float range)=>Mathf.MoveTowards(v,target,Time.unscaledDeltaTime*range/(attack?.05f:.6f));
  void Set(string bus,float db,float gain){float value=db+(gain<=0?-80:20*Mathf.Log10(gain));if(last.TryGetValue(bus,out var old)&&Mathf.Abs(old-value)<.01f)return;last[bus]=value;mixer.SetFloat(exposed[bus],value);}
  void OnDestroy(){SceneManager.sceneLoaded-=SceneLoaded;if(Instance==this)Instance=null;}
 }
 public sealed class AudioSignalMeter:MonoBehaviour
 {
  public AudioSource Source;public bool Voice;volatile float rms;
  public bool Active=>Source&&Source.isPlaying&&Source.volume>.005f&&rms>.012f;
  void OnAudioFilterRead(float[] data,int channels){double sum=0;foreach(float x in data)sum+=x*x;rms=(float)System.Math.Sqrt(sum/System.Math.Max(1,data.Length));}
 }
 public sealed class SpeechLeveler:MonoBehaviour
 {
  float gain=1;int rate=48000;
  void Awake()=>rate=AudioSettings.outputSampleRate;
  void OnAudioFilterRead(float[] data,int channels)
  {
   double sum=0;float peak=0;foreach(float x in data){sum+=x*x;peak=Mathf.Max(peak,Mathf.Abs(x));}float rms=(float)System.Math.Sqrt(sum/System.Math.Max(1,data.Length));
   float target=rms<.002f?1:Mathf.Clamp(.09f/rms,.5f,6);target=Mathf.Min(target,.8f/Mathf.Max(.0001f,peak));float seconds=(float)data.Length/channels/rate;float next=gain+(target-gain)*(1-Mathf.Exp(-seconds/(target<gain?.05f:.6f)));float step=(next-gain)/System.Math.Max(1,data.Length);
   for(int i=0;i<data.Length;i++){gain+=step;data[i]*=gain;}
  }
 }
 public sealed class AudioCeiling:MonoBehaviour
 {
  long ticks,blocks;
  public double MeanCpuMs=>blocks==0?0:1000.0*ticks/System.Diagnostics.Stopwatch.Frequency/blocks;
  const float Ceiling=.8413951f; // -1.5 dBFS with margin for inter-sample peaks.
  public static float InterSamplePeak(float a,float b,float c,float d)
  {float peak=Mathf.Max(Mathf.Abs(b),Mathf.Abs(c));for(int i=1;i<4;i++){float t=i*.25f;float x=.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);peak=Mathf.Max(peak,Mathf.Abs(x));}return peak;}
  public static float Limit(float[] data,int channels)
  {
   float peak=0;for(int i=channels;i<data.Length-2*channels;i++)peak=Mathf.Max(peak,InterSamplePeak(data[i-channels],data[i],data[i+channels],data[i+2*channels]));
   foreach(float x in data)peak=Mathf.Max(peak,Mathf.Abs(x));float gain=peak>Ceiling?Ceiling/peak:1;for(int i=0;i<data.Length;i++)data[i]*=gain;return peak*gain;
  }

  void OnAudioFilterRead(float[] data,int channels)
  {
   long start=System.Diagnostics.Stopwatch.GetTimestamp();Limit(data,channels);ticks+=System.Diagnostics.Stopwatch.GetTimestamp()-start;blocks++;
#if UNITY_EDITOR
   AudioAudit.Write(data,channels);
#endif
  }

 }
#if UNITY_EDITOR
 public static class AudioAudit
 {
  static float[] samples;static int count,rate,channels;public static bool Active=>samples!=null;public static bool Full;
  public static void Start(int seconds){rate=AudioSettings.outputSampleRate;channels=2;samples=new float[rate*channels*seconds];count=0;Full=false;}
  public static void Write(float[] data,int c){if(samples==null||Full)return;channels=c;int n=System.Math.Min(data.Length,samples.Length-count);System.Array.Copy(data,0,samples,count,n);count+=n;if(count>=samples.Length)Full=true;}
  public static void Save(string path)
  {
   Full=true;if(samples==null)return;System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));using(var w=new System.IO.BinaryWriter(System.IO.File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)channels);w.Write(rate);w.Write(rate*channels*2);w.Write((short)(channels*2));w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);for(int i=0;i<count;i++)w.Write((short)(Mathf.Clamp(samples[i],-1,1)*32767));}samples=null;
  }
 }
#endif
}
