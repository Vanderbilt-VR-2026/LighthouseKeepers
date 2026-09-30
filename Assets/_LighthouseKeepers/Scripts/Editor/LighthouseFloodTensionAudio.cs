using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Audio;
namespace LighthouseKeepers.Editor {
/// <summary>One-time, idempotent wiring: adds FloodDangerMonitor (not present anywhere yet)
/// and FloodTensionAudio to the existing persistent systems object in LK_Bootstrap.
/// Safe to run repeatedly; does not touch any other authored content.</summary>
public static class LighthouseFloodTensionAudio {
 [MenuItem("Lighthouse Keepers/Integration/Add flood tension audio")]
 public static void Add(){
  LighthouseAssets.Create();
  var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));
  var flood=Object.FindAnyObjectByType<FloodController>();
  if(!flood){Debug.LogError("LK_Bootstrap has no FloodController; run scene generation first.");return;}
  var systems=flood.gameObject;
  var danger=systems.GetComponent<FloodDangerMonitor>();if(!danger)danger=systems.AddComponent<FloodDangerMonitor>();
  var music=Object.FindAnyObjectByType<AmbienceMusic>();
  if(!Object.FindAnyObjectByType<FloodTensionAudio>()){
   var mixer=LighthouseAudioAssets.Create();var oceanGroup=mixer.FindMatchingGroups("").First(g=>g.name=="Ocean");
   var go=new GameObject("Flood tension audio");go.transform.SetParent(systems.transform,false);
   var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=false;source.spatialBlend=0;source.outputAudioMixerGroup=oceanGroup;
   var tension=go.AddComponent<FloodTensionAudio>();
   LighthouseScenes.Set(tension,"danger",danger);
   if(music)LighthouseScenes.Set(tension,"music",music);
   LighthouseScenes.SetArray(tension,"waveCrashClips",new Object[]{LighthouseAssets.Clip("WaveCrash")});
  }
  EditorSceneManager.SaveScene(scene);
  Debug.Log("LK_TENSION_AUDIO_WIRED: FloodDangerMonitor + FloodTensionAudio present on Lighthouse persistent systems.");
 }
}}
