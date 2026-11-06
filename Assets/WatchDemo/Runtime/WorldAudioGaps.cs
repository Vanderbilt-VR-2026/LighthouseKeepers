using System.Collections.Generic;using UnityEngine;using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo {
 public sealed class WorldAudioGaps:MonoBehaviour {
  WatchDemoController watch;Transform door;readonly List<AudioSource> pool=new();readonly List<(PoleSlide pole,bool latched)> poles=new();AudioSource seep;AudioClip creak,click,chalk;bool doorMoved;int day,done;int cursor;
  public static void Create(WatchDemoController w){if(!VisualPassSettings.Current||!VisualPassSettings.Current.audioGaps)return;var p=w.gameObject.AddComponent<WorldAudioGaps>();p.watch=w;}
  void Start(){door=GameObject.Find("Tutorial threshold gate")?.transform;creak=Resources.Load<AudioClip>("ImmersionAudio/creak");click=Resources.Load<AudioClip>("S4Audio/breaker");chalk=Resources.Load<AudioClip>("VisualAudio/chalk");for(int n=0;n<4;n++){var s=new GameObject("Spatial world cue "+n).AddComponent<AudioSource>();s.transform.SetParent(transform,false);s.playOnAwake=false;s.spatialBlend=1;s.minDistance=.7f;s.maxDistance=8;s.rolloffMode=AudioRolloffMode.Logarithmic;s.volume=.10f;KeeperAudioMix.Route(s,"SFX_World");pool.Add(s);}foreach(var p in FindObjectsByType<PoleSlide>(FindObjectsSortMode.None))poles.Add((p,p.Latched));seep=new GameObject("Water seeping through doorway").AddComponent<AudioSource>();seep.transform.SetParent(transform,false);seep.transform.position=new Vector3(0,.03f,-5.1f);seep.playOnAwake=false;seep.loop=true;seep.spatialBlend=1;seep.minDistance=.5f;seep.maxDistance=7;seep.volume=0;seep.clip=Resources.Load<AudioClip>("VisualAudio/trickle");KeeperAudioMix.Route(seep,"Ambience");if(seep.clip)seep.Play();day=watch.State.Day;done=watch.State.CompletedTasks;}
  void Cue(Vector3 pos,AudioClip clip,float volume){if(!clip)return;var s=pool[cursor++%pool.Count];s.transform.position=pos;s.Stop();s.clip=clip;s.volume=volume;s.Play();}
  public void BunkChanged(){if(pool.Count>0)Cue(watch.BedPosition,creak,.12f);}
  void Update(){if(door){bool moved=Quaternion.Angle(door.localRotation,Quaternion.identity)>4;if(moved!=doorMoved){Cue(door.position+Vector3.up,creak,.13f);doorMoved=moved;}}
   var s=watch.State;if(s.Day!=day||s.CompletedTasks!=done){Cue(new Vector3(-3.4f,1.5f,-8.4f),chalk,.10f);day=s.Day;done=s.CompletedTasks;}
   for(int i=0;i<poles.Count;i++){var p=poles[i].pole;if(p&&poles[i].latched!=p.Latched){Cue(p.transform.position+Vector3.up*.8f,click,.09f);poles[i]=(p,p.Latched);}}
   float target=s.HouseFlood!=null&&s.HouseFlood.DoorFront>.01f&&!s.Repaired?.075f:0;seep.volume=Mathf.MoveTowards(seep.volume,target,Time.deltaTime*.06f);
  }
 }
}
