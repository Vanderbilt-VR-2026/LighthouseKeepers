using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
 public sealed class PAIntercom:MonoBehaviour
 {
  WatchDemoController watch;Transform head;readonly float[] roomGains=new float[6];HarborRadio program;AudioSource[] voice,noise;Component[] buttons;RadioLineCatalog catalog;Mesh horn;
  Material hornMaterial;AudioClip click;bool closing;AudioClip clip;float age,nextReplay;bool active;int line,variant;
  public Component Button(int i)=>i>=0&&i<buttons.Length?buttons[i]:null;
  public static PAIntercom Create(WatchDemoController w,Transform head,HarborRadio radio,RadioLineCatalog catalog,AudioClip staticClip,AudioClip click)
  {var go=new GameObject("Lighthouse brass PA system");go.transform.SetParent(w.transform,false);var p=go.AddComponent<PAIntercom>();p.watch=w;p.head=head;p.program=radio;p.catalog=catalog;p.Build(staticClip,click);return p;}
  void Build(AudioClip staticClip,AudioClip click)
  {
   this.click=click;var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();hornMaterial=new Material(parts.Brass);hornMaterial.SetFloat("_Cull",0);voice=new AudioSource[6];noise=new AudioSource[6];buttons=new Component[6];horn=Horn();
   var positions=new[]{new Vector3(-3.8f,2.35f,-6.4f),new Vector3(-3.8f,2.35f,-.7f),new Vector3(-3.8f,5.55f,-.7f),new Vector3(-3.8f,8.75f,-.7f),new Vector3(-2.9f,11.95f,-.6f),new Vector3(2.4f,2.35f,-10.315f)};
   for(int i=0;i<6;i++)
   {
    var root=new GameObject("Brass PA speaker "+i).transform;root.SetParent(transform,false);root.SetPositionAndRotation(positions[i],Quaternion.LookRotation(i==5?Vector3.back:Vector3.right));
    if(i<5)
    {
     var origin=new Vector3(0,positions[i].y,i==0?positions[i].z:0);
     float nearest=100;foreach(var hit in Physics.RaycastAll(origin,Vector3.left,5,~0,QueryTriggerInteraction.Ignore))if(hit.collider.name.StartsWith("UCX_")&&hit.collider.name.Contains("Wall")&&hit.distance<nearest){nearest=hit.distance;root.SetPositionAndRotation(hit.point+hit.normal*.012f,Quaternion.LookRotation(hit.normal));}
    }
    parts.Part("Riveted PA wall mount",root,Vector3.zero,new Vector3(.3f,.38f,.035f),parts.Metal);
    var cone=new GameObject("Brass horn bell",typeof(MeshFilter),typeof(MeshRenderer));cone.transform.SetParent(root,false);cone.transform.localPosition=new Vector3(0,0,.04f);cone.GetComponent<MeshFilter>().sharedMesh=horn;cone.GetComponent<MeshRenderer>().sharedMaterial=hornMaterial;
    parts.Part("Dark horn throat",root,new Vector3(0,0,.07f),new Vector3(.10f,.10f,.015f),parts.Dark);
    parts.Part("PA replay conduit",root,new Vector3(0,-.48f,.01f),new Vector3(.012f,.85f,.014f),parts.Brass);
    var plate=parts.Part("Replay button plate",root,new Vector3(0,-.92f,.02f),new Vector3(.18f,.22f,.025f),parts.Dark);
    var cap=parts.Part("PRESS TO REPLAY",root,new Vector3(0,-.92f,.045f),new Vector3(.075f,.075f,.035f),parts.Brass,PrimitiveType.Cylinder);cap.transform.localRotation=Quaternion.Euler(90,0,0);
    var collider=cap.AddComponent<BoxCollider>();var button=cap.AddComponent<XRSimpleInteractable>();button.colliders.Clear();button.colliders.Add(collider);buttons[i]=button;int index=i;
    var rest=cap.transform.localPosition;button.selectEntered.AddListener(e=>{cap.transform.localPosition=rest-Vector3.forward*.008f;var hand=e.interactorObject.transform.GetComponentInParent<HandRig>();if(hand)hand.Pulse(.15f,.05f);Press(index);});button.selectExited.AddListener(e=>cap.transform.localPosition=rest);
    parts.Text("Replay stencil","REPLAY LAST",root,new Vector3(0,-1.015f,.04f),.0027f);
    voice[i]=Source(root,"Harbor radio PA voice "+i,.95f);voice[i].gameObject.AddComponent<AudioHighPassFilter>().cutoffFrequency=350;voice[i].gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=3500;voice[i].gameObject.AddComponent<RadioPresence>();
    var reverb=voice[i].gameObject.AddComponent<AudioReverbFilter>();reverb.reverbPreset=AudioReverbPreset.Room;reverb.decayTime=.35f;reverb.room=-1800;reverb.reflectionsLevel=-2200;reverb.reverbLevel=-2200;
    noise[i]=Source(root,"Harbor radio PA hiss "+i,0);noise[i].clip=staticClip;noise[i].loop=true;
    KeeperAudioMix.Route(voice[i],"Radio");KeeperAudioMix.Route(noise[i],"Radio");
   }
  }
  AudioSource Source(Transform parent,string name,float volume)
  {var go=new GameObject(name);go.transform.SetParent(parent,false);var a=go.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=1;a.dopplerLevel=0;a.minDistance=1.5f;a.maxDistance=12;a.rolloffMode=AudioRolloffMode.Linear;a.volume=volume;a.priority=10;return a;}
  void Press(int index){if(watch.State.RemoteReplica)watch.State.Coop?.Send(CoopAction.PAReplay,index);else Replay();}
  public bool Replay(){if(Time.unscaledTime<nextReplay||program.Cue==0)return false;nextReplay=Time.unscaledTime+3;if(watch.State.PressureEnabled)return program.StatusUpdate();program.ReplayLast();return true;}
  public void Play(int id,AudioClip value,float offset,int serial)
  {
   clip=value;variant=serial;line=id;age=offset;active=true;closing=false;double start=AudioSettings.dspTime+System.Math.Max(0,.3-offset);
   for(int i=0;i<voice.Length;i++){voice[i].Stop();noise[i].Stop();voice[i].clip=clip;if(clip&&offset<clip.length+.3f){voice[i].time=Mathf.Clamp(offset-.3f,0,clip.length-.01f);voice[i].PlayScheduled(start);}noise[i].Play();if(offset<.1f&&click)noise[i].PlayOneShot(click,.25f);}
  }
  public void Stop(){active=false;foreach(var s in voice)s.Stop();foreach(var s in noise)s.Stop();}
  void Update()
  {
   if(!active||!clip)return;age+=Time.deltaTime;
   float end=.3f+clip.length;if(age>=end&&!closing){closing=true;if(click)foreach(var s in noise)s.PlayOneShot(click,.12f);}
   float hiss=age<.3f?Mathf.Lerp(.11f,.02f,age/.3f):age<end?.008f:Mathf.Lerp(.045f,0,Mathf.Clamp01((age-end)/.4f));
   float dips=1;var pauses=catalog.Pauses(line);float speech=age-.3f;
   if(Mathf.Abs(speech-pauses.x)<.035f||(variant%2==0&&Mathf.Abs(speech-pauses.y)<.035f)){dips=.6f;hiss+=.018f;}
   int room=head.position.z< -10.3f||new Vector2(head.position.x,head.position.z).magnitude>13?5:head.position.z< -4.3f?0:head.position.y<3.4f?1:head.position.y<6.7f?2:head.position.y<10?3:4;
   for(int i=0;i<voice.Length;i++){float transition=Mathf.Abs(head.position.y/3.3f-Mathf.Round(head.position.y/3.3f));bool stairBlend=head.position.z> -4.3f&&transition<.18f;float target=i==room?1:stairBlend&&i>0&&i<5&&Mathf.Abs(i-room)==1?.45f:.08f;roomGains[i]=Mathf.MoveTowards(roomGains[i],target,Time.deltaTime*3);voice[i].volume=.95f*dips*roomGains[i];}foreach(var s in noise)s.volume=hiss;
   if(age>end+.4f)Stop();
  }
  static Mesh Horn()
  {
   const int n=16;var v=new Vector3[n*2];var tris=new int[n*6];for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;v[i]=new Vector3(Mathf.Cos(a)*.055f,Mathf.Sin(a)*.055f,0);v[i+n]=new Vector3(Mathf.Cos(a)*.23f,Mathf.Sin(a)*.23f,.25f);int j=(i+1)%n;int t=i*6;tris[t]=i;tris[t+1]=j;tris[t+2]=i+n;tris[t+3]=j;tris[t+4]=j+n;tris[t+5]=i+n;}var m=new Mesh{name="Low-poly brass PA horn"};m.vertices=v;m.triangles=tris;m.RecalculateNormals();return m;
  }
  void OnDestroy(){if(horn)Destroy(horn);if(hornMaterial)Destroy(hornMaterial);}
 }
 // Gentle +3 dB presence EQ and soft peak compression, on PA speech only.
 public sealed class RadioPresence:MonoBehaviour
 {
  double b0,b1,b2,a1,a2;readonly double[] z1=new double[8],z2=new double[8];
  void Awake(){double a=System.Math.Pow(10,3d/40),w=2*System.Math.PI*2500/AudioSettings.outputSampleRate,alpha=System.Math.Sin(w)/(2*.8);double d=1+alpha/a;b0=(1+alpha*a)/d;b1=-2*System.Math.Cos(w)/d;b2=(1-alpha*a)/d;a1=b1;a2=(1-alpha/a)/d;}
  void OnAudioFilterRead(float[] data,int channels)
  {for(int i=0;i<data.Length;i++){int c=i%channels;double x=data[i],y=b0*x+z1[c];z1[c]=b1*x-a1*y+z2[c];z2[c]=b2*x-a2*y;double level=System.Math.Abs(y);if(level>.55)y=System.Math.Sign(y)*(.55+(level-.55)*.5);data[i]=(float)(System.Math.Tanh(y*1.08)/1.08);}}
 }
}
