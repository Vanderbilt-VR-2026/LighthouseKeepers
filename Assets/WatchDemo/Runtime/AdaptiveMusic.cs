using UnityEngine;
using UnityEngine.SceneManagement;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
 // Host chooses transitions; guests consume snapshot mode/time. No day rules changed.
 public sealed class AdaptiveMusic:MonoBehaviour
 {
  WatchDemoController watch;AudioPassSettings settings;AudioSource[] deck;AudioSource stings;
  AudioClip[] clips;AudioClip fixedClip,survivedClip,lostClip;
  int mode=-1,pending=-1,active,day,tasks;double since,nextChange,lastElapsed;float fadeAge=100,nextSting;int pendingFixed;bool failed,sleeping,lobby;
  public int Mode=>mode;public double Since=>since;
  public static int Choose(bool fault,float waterFraction,double darkRemaining,bool dark,double timeRemaining)
   =>!fault?0:waterFraction>.7f||dark&&darkRemaining<20||timeRemaining<60?2:fault?1:0;
  public static AdaptiveMusic Create(WatchDemoController w)
  {
   var s=AudioPassSettings.Current;if(!s||!s.adaptiveMusic)return null;
   var m=w.gameObject.AddComponent<AdaptiveMusic>();m.watch=w;m.Setup(s);return m;
  }
  void Setup(AudioPassSettings s)
  {
   settings=s;clips=new[]{Resources.Load<AudioClip>("AudioPass/calm"),Resources.Load<AudioClip>("AudioPass/tension"),Resources.Load<AudioClip>("AudioPass/crisis")};
   deck=new[]{Source("Adaptive music A"),Source("Adaptive music B")};foreach(var d in deck){d.loop=true;d.priority=180;}
   stings=Source("Short watch stings");stings.priority=50;
   fixedClip=Resources.Load<AudioClip>("AudioPass/sting_fixed");survivedClip=Resources.Load<AudioClip>("AudioPass/sting_survived");lostClip=Resources.Load<AudioClip>("AudioPass/sting_lost");
  }
  AudioSource Source(string name){var g=new GameObject(name);g.transform.SetParent(transform,false);var a=g.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=0;return a;}
  void Update()
  {
   if(lobby){if(!deck[0].isPlaying&&deck[0].clip)deck[0].Play();return;}
   var s=watch.State;if(s==null)return;
   if(day!=s.Day||s.Elapsed<lastElapsed-.01){day=s.Day;tasks=s.CompletedTasks;failed=sleeping=false;pendingFixed=0;pending=-1;mode=-1;}
   lastElapsed=s.Elapsed;
   if(!s.RemoteReplica)
   {
    float height=s.Tuning?s.Tuning.failWaterHeightM:.55f;
    int wanted=Choose(!s.Complete,Mathf.Max(0,watch.WaterHeight)/height,s.BeaconDarkRemaining,!s.BeaconShining&&s.BeaconDarkLimit>0,s.Duration-s.Elapsed);
    if(mode<0)Transition(wanted,s.Elapsed,0);
    if(wanted!=mode&&wanted!=pending)
    {
     pending=wanted;float bar=settings.barSeconds[Mathf.Clamp(mode,0,2)];
     nextChange=since+(System.Math.Floor((s.Elapsed-since)/bar)+1)*bar;
    }
    if(wanted==mode)pending=-1;
    if(pending>=0&&s.Elapsed>=nextChange){Transition(pending,nextChange,0);pending=-1;}
   }
   fadeAge+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(fadeAge/settings.crossfadeSeconds);
   deck[active].volume=settings.musicVolume*Mathf.Sin(t*Mathf.PI*.5f);deck[1-active].volume=settings.musicVolume*Mathf.Cos(t*Mathf.PI*.5f);
   if(t>=1)deck[1-active].Stop();
   if(s.CompletedTasks>tasks)pendingFixed+=s.CompletedTasks-tasks;tasks=s.CompletedTasks;
   if(pendingFixed>0&&Time.unscaledTime>=nextSting&&fixedClip){pendingFixed--;stings.PlayOneShot(fixedClip,settings.stingVolume);nextSting=Time.unscaledTime+fixedClip.length;}
   if(s.Failed&&!failed&&lostClip)stings.PlayOneShot(lostClip,settings.stingVolume);failed=s.Failed;
  }
  void Transition(int next,double at,float age)
  {
   if(!clips[next])return;mode=next;since=at;active=1-active;var a=deck[active];a.Stop();a.clip=clips[next];a.time=age%a.clip.length;a.volume=0;a.Play();fadeAge=age;
  }
  public void Capture(WatchSnapshot s)
  {
   s.musicMode=Mathf.Max(0,mode);s.musicSince=System.Math.Clamp(since,0,s.elapsed);
   if(s.sleeping&&!sleeping&&survivedClip)stings.PlayOneShot(survivedClip,settings.stingVolume);sleeping=s.sleeping;
  }
  public void Apply(WatchSnapshot s)
  {
   day=s.day;lastElapsed=s.elapsed;
   if(s.musicMode!=mode||System.Math.Abs(s.musicSince-since)>.01)Transition(s.musicMode,s.musicSince,(float)System.Math.Max(0,s.elapsed-s.musicSince));
   if(s.sleeping&&!sleeping&&survivedClip)stings.PlayOneShot(survivedClip,settings.stingVolume);sleeping=s.sleeping;
  }
  // Solo also gets the survived sting without requiring a network Capture call.
  public void Survived(){if(!sleeping&&survivedClip)stings.PlayOneShot(survivedClip,settings.stingVolume);sleeping=true;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void InstallLobby()
  {SceneManager.sceneLoaded-=Lobby;SceneManager.sceneLoaded+=Lobby;Lobby(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
  static void Lobby(Scene scene,LoadSceneMode unused)
  {
   var s=AudioPassSettings.Current;if(!s||!s.adaptiveMusic||scene.name!="LK_NetworkLobby")return;
   var m=new GameObject("Quiet lobby calm drone").AddComponent<AdaptiveMusic>();m.lobby=true;m.Setup(s);
   var a=m.deck[0];a.clip=m.clips[0];a.volume=s.lobbyVolume;a.Play();
  }
 }
}
