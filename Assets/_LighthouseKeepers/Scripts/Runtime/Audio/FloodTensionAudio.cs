using UnityEngine;
using LighthouseKeepers.Flood;
namespace LighthouseKeepers.Audio {
/// <summary>Reads FloodDangerMonitor.Stage only (not storm intensity). Swells the existing
/// ambience bed and plays escalating wave crashes as flood danger rises toward submersion.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class FloodTensionAudio:MonoBehaviour {
 [SerializeField] FloodDangerMonitor danger;
 [SerializeField] AmbienceMusic music;
 [SerializeField,Range(-80,0)] float musicVolumeSafeDb=-20, musicVolumeDangerDb=-4;
 [SerializeField,Min(0.1f)] float fadeSeconds=4;
 [SerializeField] AudioClip[] waveCrashClips;
 [SerializeField] Vector2 crashSpacingSafe=new(22,40), crashSpacingDanger=new(4,9);
 [SerializeField] Vector2 crashVolume=new(0.25f,0.9f), crashPitch=new(0.85f,1.15f);
 AudioSource crash; float target,current,nextCrash;
 public float Intensity=>current;
 void Awake(){crash=GetComponent<AudioSource>();}
 void OnEnable(){
  if(!danger)danger=FindAnyObjectByType<FloodDangerMonitor>();
  if(!music)music=FindAnyObjectByType<AmbienceMusic>();
  if(danger){danger.StageChanged+=OnStage;SetTarget(danger.Stage);}
  ScheduleCrash();
 }
 void OnDisable(){if(danger)danger.StageChanged-=OnStage;}
 void OnStage(FloodDangerStage previous,FloodDangerStage next)=>SetTarget(next);
 void SetTarget(FloodDangerStage stage)=>target=(int)stage/4f;
 void Update(){
  current=Mathf.Lerp(current,target,1-Mathf.Exp(-Time.deltaTime/fadeSeconds));
  if(music)music.SetVolume(Mathf.Lerp(musicVolumeSafeDb,musicVolumeDangerDb,current));
  if(Time.time>=nextCrash)PlayCrash();
 }
 void ScheduleCrash(){var spacing=Vector2.Lerp(crashSpacingSafe,crashSpacingDanger,current);nextCrash=Time.time+Random.Range(spacing.x,spacing.y);}
 void PlayCrash(){
  if(waveCrashClips!=null&&waveCrashClips.Length>0){crash.pitch=Random.Range(crashPitch.x,crashPitch.y);crash.PlayOneShot(waveCrashClips[Random.Range(0,waveCrashClips.Length)],Mathf.Lerp(crashVolume.x,crashVolume.y,current));}
  ScheduleCrash();
 }
}}
