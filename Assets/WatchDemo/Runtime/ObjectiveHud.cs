using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
 public sealed class ObjectiveHud:MonoBehaviour
 {
  WatchDemoController watch;Transform head,wrist;HarborRadio radio;CanvasGroup group;RectTransform root,line,strike,tick,danger;Text dangerText;
  Text task,label,floor,subtitle,partner;ObjectiveLineIcon icon;AudioSource cueSound;string caption="";float captionAt;int lastFloor=-99,lastTarget=-99,typed=-1;Material material,fontMaterial;Texture2D backing;Vector3 velocity;Quaternion follow;
  int index=int.MinValue,day,cue;float changed,until,completeUntil;bool buttons,glance;string full="",pending;int nextIndex;
  public static ObjectiveHud Create(WatchDemoController w,Transform head)
  {var s=AudioPassSettings.Current;if(!s||!s.objectiveHud)return null;var h=w.gameObject.AddComponent<ObjectiveHud>();h.watch=w;h.head=head;h.Build();return h;}
  static RectTransform Rect(Transform parent,string name,Vector2 p,Vector2 size)
  {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchoredPosition=p;r.sizeDelta=size;return r;}
  Text Text(string name,Vector2 p,Vector2 size,int pixels,Color c)
  {var t=Rect(root,name,p,size).gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=pixels;t.color=c;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;t.material=fontMaterial;return t;}
  RectTransform Rule(string name,Vector2 p,Vector2 size)
  {var r=Rect(root,name,p,size);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.74f,.58f,.3f,.8f);image.material=material;image.raycastTarget=false;return r;}
  void Build()
  {
   var go=new GameObject("Transient objective tracker",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup));root=go.GetComponent<RectTransform>();root.SetParent(transform,false);root.localScale=Vector3.one*.001f;root.sizeDelta=new Vector2(600,210);
   var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=head.GetComponent<Camera>();canvas.sortingOrder=1000;group=go.GetComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
   material=new Material(Resources.Load<Shader>("ObjectiveOverlay"));fontMaterial=new Material(material);fontMaterial.SetFloat("_AlphaOnly",1);
   backing=new Texture2D(64,32,TextureFormat.RGBA32,false);var data=new Color[64*32];var random=new System.Random(41);
   for(int y=0;y<32;y++)for(int x=0;x<64;x++){float edge=Mathf.Sin((y+.5f)/32*Mathf.PI)*Mathf.Sin((x+.5f)/64*Mathf.PI);data[y*64+x]=new Color(.018f,.024f,.029f,.30f*edge*(.96f+(float)random.NextDouble()*.08f));}
   backing.SetPixels(data);backing.Apply();var bg=Rect(root,"Subtle gradient backing",Vector2.zero,root.sizeDelta).gameObject.AddComponent<RawImage>();bg.texture=backing;bg.material=material;bg.raycastTarget=false;
   line=Rule("Brass accent",new Vector2(-265,76),new Vector2(480,1.4f));line.pivot=new Vector2(0,.5f);
   label=Text(LighthouseKeepers.Core.PlayerStrings.Get("legacy.ObjectiveHud.1"),new Vector2(-5,53),new Vector2(510,25),14,new Color(.74f,.65f,.48f,.72f));label.text=LighthouseKeepers.Core.PlayerStrings.Get("legacy.ObjectiveHud.2");
   icon=Rect(root,"Objective line icon",new Vector2(-240,10),new Vector2(40,42)).gameObject.AddComponent<ObjectiveLineIcon>();icon.color=new Color(.76f,.62f,.36f);icon.material=material;icon.raycastTarget=false;
   cueSound=new GameObject("Soft objective completion").AddComponent<AudioSource>();cueSound.transform.SetParent(transform,false);cueSound.playOnAwake=false;cueSound.volume=.08f;cueSound.clip=Resources.Load<AudioClip>("Hands/clunk");
   task=Text("Objective task",new Vector2(15,10),new Vector2(510,50),28,new Color(.93f,.91f,.83f));task.transform.localScale=new Vector3(.90f,1,1);
   floor=Text("Relative floor",new Vector2(10,-28),new Vector2(470,25),16,new Color(.76f,.7f,.56f));
   partner=Text("Partner job",new Vector2(10,-48),new Vector2(470,20),14,new Color(.76f,.7f,.56f));
   subtitle=Text("Harbor subtitle",new Vector2(0,-72),new Vector2(530,85),19,new Color(.79f,.79f,.72f,.82f));subtitle.fontStyle=FontStyle.Italic;
   strike=Rule("Completed objective strike",new Vector2(15,10),new Vector2(460,1));tick=Rule("Completion tick stem",new Vector2(240,13),new Vector2(12,2));tick.localRotation=Quaternion.Euler(0,0,45);var up=Rule("Completion tick sweep",new Vector2(253,18),new Vector2(24,2));up.SetParent(tick,false);up.anchoredPosition=new Vector2(11,4);up.localRotation=Quaternion.Euler(0,0,90);
   foreach(var h in FindObjectsByType<HandRig>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(h.Node==XRNode.LeftHand)wrist=h.transform.parent;
   danger=Rule("Danger fill",new Vector2(-220,-110),new Vector2(440,6));danger.pivot=new Vector2(0,.5f);dangerText=Text("Danger cause",new Vector2(0,-126),new Vector2(440,20),14,new Color(.95f,.72f,.37f));
   follow=Quaternion.LookRotation(head.forward,Vector3.up);root.SetPositionAndRotation(head.position+follow*new Vector3(-.28f,.34f,1.6f),follow);Reveal();
  }
  public void Reveal()=>until=Time.unscaledTime+7;
  public void PreviewCompletion(){pending=full;nextIndex=index;completeUntil=Time.unscaledTime+1.2f;Reveal();}
  void Set(int value,string text){index=value;full=text;changed=Time.unscaledTime;typed=-1;completeUntil=0;Reveal();}
  string Title(int value)=>watch.State.Director!=null?watch.State.Director.Instruction((KeeperStep)value):value== -2?KeeperStrings.Get("hud.complete"):value<0?KeeperStrings.Get("hud.rest"):watch.State.Definition.tasks[value].taskText;
  void Update()
  {
   var s=watch.State;if(s==null)return;if(!radio)radio=FindAnyObjectByType<HarborRadio>();
   int current=s.Director!=null?(int)watch.CurrentInstruction:s.ObjectiveIndex;
   partner.text=s.Director!=null&&watch.PartnerInstruction!=KeeperStep.None?string.Format(KeeperStrings.Get("partner"),KeeperStrings.Get("job."+ObjectiveDirector.Job(watch.PartnerInstruction))):"";
   if(day!=s.Day){day=s.Day;Set(current,Title(current));}
   else if(current!=index&&completeUntil<=0)
   {
    if(s.Director!=null&&ObjectiveDirector.Job((KeeperStep)index)==ObjectiveDirector.Job((KeeperStep)current)||s.Director==null&&index>=0&&index<s.Definition.tasks.Length&&s.IsDone(s.Definition.tasks[index].stationId)){pending=Title(current);nextIndex=current;completeUntil=Time.unscaledTime+1.2f;if(cueSound.clip)cueSound.Play();Reveal();}
    else Set(current,Title(current));
   }
   if(s.Director!=null&&current==index&&Title(current)!=full&&completeUntil<=0)Set(current,Title(current));
   if(completeUntil>0&&Time.unscaledTime>=completeUntil)Set(nextIndex,pending??Title(current));
   bool done=completeUntil>0;strike.gameObject.SetActive(done);tick.gameObject.SetActive(done);
   if(done){task.text=full;task.color=new Color(.9f,.75f,.4f);}else{int n=Mathf.Min(full.Length,Mathf.CeilToInt(full.Length*Mathf.Clamp01((Time.unscaledTime-changed)/.4f)));if(typed!=n){typed=n;task.text=full.Substring(0,n);}task.color=new Color(.93f,.91f,.83f);}
   line.localScale=new Vector3(Mathf.Clamp01((Time.unscaledTime-changed)/.4f),1,1);
   string station=s.Director!=null?ObjectiveDirector.Job((KeeperStep)index):index>=0&&index<s.Definition.tasks.Length?s.Definition.tasks[index].stationId:"BED";
   int target=s.Director!=null?ObjectiveDirector.Floor((KeeperStep)index):station==WatchState.PipeStation?1:station==WatchState.GeneratorStation?2:station=="BED"?3:4;
   int here=head.position.y<3.4f?1:head.position.y<6.7f?2:head.position.y<10?3:4;
   if(lastFloor!=here||lastTarget!=target){lastFloor=here;lastTarget=target;floor.text=(target>here?LighthouseKeepers.Core.PlayerStrings.Get("legacy.ObjectiveHud.3"):target<here?LighthouseKeepers.Core.PlayerStrings.Get("legacy.ObjectiveHud.4"):"")+LighthouseKeepers.Core.PlayerStrings.Get("legacy.ObjectiveHud.5")+target;}
   icon.Set(station);
   string words=radio?radio.Subtitle:"";
   if(words.Length>0){caption=words;captionAt=Time.unscaledTime;until=Mathf.Max(until,Time.unscaledTime+.7f);}
   subtitle.text=Time.unscaledTime-captionAt<.7f?caption:"";
   var colour=subtitle.color;colour.a=.82f*Mathf.Clamp01(1-(Time.unscaledTime-captionAt)/.7f);subtitle.color=colour;
   if(radio&&cue!=radio.Serial){cue=radio.Serial;Reveal();}
   bool unsafeNow=s.PressureEnabled&&!s.Complete&&s.Danger>=.5f;
   danger.gameObject.SetActive(unsafeNow);dangerText.gameObject.SetActive(unsafeNow);if(unsafeNow){danger.localScale=new Vector3(s.Danger,1,1);dangerText.text=KeeperStrings.Get(s.DangerKey);until=Mathf.Max(until,Time.unscaledTime+.5f);}
   group.alpha=watch.ObjectivePresentationBlocked?0:Mathf.Clamp01((until-Time.unscaledTime)/.8f);
   var device=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);device.TryGetFeatureValue(CommonUsages.primaryButton,out bool x);device.TryGetFeatureValue(CommonUsages.secondaryButton,out bool y);bool pressed=x||y;if(pressed&&!buttons){Reveal();if(s.PressureEnabled&&radio)radio.RequestReplay();}buttons=pressed;
   bool looking=wrist&&Vector3.Distance(head.position,wrist.position)<1.1f&&Vector3.Dot(head.forward,(wrist.position-head.position).normalized)>.94f;if(looking&&!glance)Reveal();glance=looking;
   follow=Quaternion.Slerp(follow,Quaternion.LookRotation(head.forward,Vector3.up),1-Mathf.Exp(-Time.unscaledDeltaTime/ .45f));var desired=head.position+follow*new Vector3(-.28f,.34f,1.6f);root.position=Vector3.SmoothDamp(root.position,desired,ref velocity,.20f,20,Time.unscaledDeltaTime);root.rotation=follow;
  }
  void OnDestroy(){if(material)Destroy(material);if(fontMaterial)Destroy(fontMaterial);if(backing)Destroy(backing);}
 }
}
