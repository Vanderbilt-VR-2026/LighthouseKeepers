using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
 // Head-based audio zones only. No flood or avatar visuals are accessed.
 public sealed class RainSoundscape:MonoBehaviour
 {
  WatchDemoController watch;
  AudioPassSettings settings;Transform head;AudioSource[] beds,pool;AudioSource wind;AudioLowPassFilter windFilter;AudioClip[] impacts;Vector3 blend;
  readonly Vector3[] windows=new Vector3[16];float nextImpact,nextGust,gustStart=-100,gustLength=6;int cursor;
  public static Vector3 Weights(Vector3 p)
  {
   float house=BoxWeight(p,new Vector3(0,2.1f,-7.3f),new Vector3(7.9f,4.2f,6));
   float radius=p.y>=9.7f?3.05f:4.4f;
   float tower=Mathf.Clamp01((radius-new Vector2(p.x,p.z).magnitude)/(p.y>=9.7f?.35f:.6f))*Mathf.Clamp01((14.8f-p.y)/.6f);
   float inside=Mathf.Max(house,tower);float roof=Mathf.Clamp01((p.y-9.7f)/.7f)*Mathf.Clamp01((3.3f-new Vector2(p.x,p.z).magnitude)/.6f)*inside;
   if(Mathf.Abs(p.x)<1&&Mathf.Abs(p.z+4.4f)<.65f&&p.y<3.3f)inside=Mathf.Min(inside,.65f);
   return new Vector3(1-inside,inside-roof,roof);
  }
  static float BoxWeight(Vector3 p,Vector3 c,Vector3 size)
  {var d=size*.5f-(p-c);var e=size*.5f+(p-c);return Mathf.Clamp01(Mathf.Min(d.x,d.y,d.z,e.x,e.y,e.z)/.6f);}
  public static void Create(WatchDemoController watch,Transform head)
  {
   var s=AudioPassSettings.Current;if(!s||!s.layeredRain)return;
   var rain=watch.gameObject.AddComponent<RainSoundscape>();rain.watch=watch;rain.settings=s;rain.head=head;rain.Setup();
  }
  AudioSource Source(string name,bool spatial)
  {var g=new GameObject(name);g.transform.SetParent(transform,false);var a=g.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=spatial?1:0;a.dopplerLevel=0;a.rolloffMode=AudioRolloffMode.Linear;a.minDistance=1;a.maxDistance=8;a.priority=130;return a;}
  void Setup()
  {
   var legacy=FindAnyObjectByType<LighthouseKeepers.Presentation.Environment.ShellWeatherPresentation>();if(legacy)legacy.ExternalStormAudio=true;
   beds=new AudioSource[3];string[] names={"exterior","interior","roof"};
   for(int i=0;i<3;i++){beds[i]=Source("Rain bed "+names[i],false);beds[i].clip=Resources.Load<AudioClip>("AudioPass/rain_"+names[i]);beds[i].loop=true;beds[i].volume=0;beds[i].gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=i==1?6500:i==2?9000:16000;if(beds[i].clip)beds[i].Play();}
   impacts=new AudioClip[4];names=new[]{"window","metal","puddle","drip"};for(int i=0;i<4;i++)impacts[i]=Resources.Load<AudioClip>("AudioPass/impact_"+names[i]);
   pool=new AudioSource[4];for(int i=0;i<pool.Length;i++)pool[i]=Source("Rain impact "+i,true);
   wind=Source("Storm gust through gaps",true);wind.clip=Resources.Load<AudioClip>("AudioPass/wind");wind.loop=true;windFilter=wind.gameObject.AddComponent<AudioLowPassFilter>();if(wind.clip)wind.Play();nextGust=Time.time+15;
   int k=0;for(int floor=0;floor<4;floor++)for(int side=0;side<4;side++){float a=side*Mathf.PI*.5f;windows[k++]=new Vector3(Mathf.Sin(a)*3.6f,1.8f+floor*3.2f,Mathf.Cos(a)*3.6f);}
   Zone("House rain zone",new Vector3(0,2.1f,-7.3f),new Vector3(7.9f,4.2f,6));Zone("Tower interior rain zone",new Vector3(0,7,0),new Vector3(8.8f,14,8.8f));Zone("Roof beacon rain zone",new Vector3(0,12,0),new Vector3(6.6f,4.6f,6.6f));Zone("Open doorway partial rain zone",new Vector3(0,1.6f,-4.4f),new Vector3(2,3.2f,1.3f));
  }
  void Zone(string name,Vector3 c,Vector3 size){var g=new GameObject(name);g.layer=2;g.transform.SetParent(transform,false);g.transform.position=c;var b=g.AddComponent<BoxCollider>();b.isTrigger=true;b.size=size;}
  void Update()
  {
   var wanted=Weights(head.position);blend=Vector3.MoveTowards(blend,wanted,Time.deltaTime*Mathf.Sqrt(2)/settings.zoneFadeSeconds);float total=blend.x+blend.y+blend.z;if(total>0)blend/=total;
   float storm=watch.State.Storm!=null?Mathf.Lerp(.75f,1.2f,(watch.State.Storm.Stage-1)/3f):1;
   if(LighthouseKeepers.Core.VisualPassSettings.Current&&LighthouseKeepers.Core.VisualPassSettings.Current.water)storm*=Mathf.Lerp(.65f,1.1f,Mathf.Clamp01((watch.State.Day-1)*.5f));
   beds[0].volume=settings.rainGains.x*blend.x*storm;beds[1].volume=settings.rainGains.y*blend.y*storm;beds[2].volume=settings.rainGains.z*blend.z*storm;
   if(Time.time>=nextGust){gustStart=Time.time;gustLength=Random.Range(5,9);nextGust=Time.time+Random.Range(15,40);}
   float gust=Mathf.Sin(Mathf.Clamp01((Time.time-gustStart)/gustLength)*Mathf.PI);wind.volume=(.025f+.10f*gust)*(.35f+.65f*(blend.x+blend.z))*storm;wind.transform.position=head.position+new Vector3(2,1,0);windFilter.cutoffFrequency=Mathf.Lerp(1200,7000,blend.x+blend.z);
   if(Time.time<nextImpact)return;nextImpact=Time.time+Random.Range(.2f,.5f)*(blend.x>.5f?1:2);
   var a=pool[cursor++%pool.Length];if(a.isPlaying)return;
   int kind=head.position.y>9.7f?1:blend.x>.5f?2:blend.z>.5f?1:Random.value<.7f?0:3;Vector3 position=head.position;
   if(kind==0){float distance=float.MaxValue;foreach(var w in windows){float d=(w-head.position).sqrMagnitude;if(d<distance){distance=d;position=w;}}if(head.position.z< -4)position=new Vector3(head.position.x<0?-3.6f:3.6f,2,-8.6f);}
   else{var offset=Random.insideUnitCircle.normalized*Random.Range(1,3);position+=new Vector3(offset.x,kind==1?1.2f:kind==2?-1.4f:1.5f,offset.y);}
   a.transform.position=position;a.clip=impacts[kind];a.pitch=Random.Range(.85f,1.15f);a.volume=Random.Range(.08f,.16f);if(a.clip)a.Play();
  }
  void OnDestroy(){var legacy=FindAnyObjectByType<LighthouseKeepers.Presentation.Environment.ShellWeatherPresentation>();if(legacy)legacy.ExternalStormAudio=false;}
 }
}
