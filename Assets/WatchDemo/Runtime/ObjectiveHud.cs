using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
 public sealed class ObjectiveHud:MonoBehaviour
 {
  WatchDemoController watch;Transform head,wrist;HarborRadio radio;CanvasGroup group;RectTransform root,line,strike,tick;
  Text task,label,floor,subtitle;ObjectiveLineIcon icon;AudioSource cueSound;string caption="";float captionAt;int lastFloor=-99,lastTarget=-99,typed=-1;Material material,fontMaterial;Texture2D backing;
  // Caption-only guidance (no recorded audio yet); shown in the subtitle row when the radio is silent.
  string note="";float noteUntil,built;bool nagging,wakePending;
  public const string WatchGateCaption="Storm building — the watch begins when you start work.";
  public const string WrenchDownCaption="Pipe holding. Set the wrench down — you won't need it tonight.";
  public static string NextJobCaption(string task,string floor)=>"Done. Next: "+task+" — "+floor+".";
  public static string TonightCaption(string task,string floor)=>"Tonight: "+task+" — "+floor+".";
  public static bool PromptWrenchDown(bool pipeRepaired,bool holdingWrench)=>pipeRepaired&&holdingWrench;
  public static int StationFloor(string station)=>station==WatchState.PipeStation?1:station==WatchState.GeneratorStation?2:station=="BED"?3:4;
  public static int FloorAt(float headY)=>headY<3.4f?1:headY<6.7f?2:headY<10?3:4;
  public static string FloorDirection(int target,int here,string gap="  ")=>(target>here?"UP"+gap:target<here?"DOWN"+gap:"")+"FLOOR "+target;
  // Tracker distance and height relative to the eyes when it is placed (metres); it stays world-fixed until it fades.
  const float PlaceDistance=1.6f,PlaceHeight=0;
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
   label=Text("Objective small caps",new Vector2(-5,53),new Vector2(510,25),14,new Color(.74f,.65f,.48f,.72f));label.text="O B J E C T I V E";
   icon=Rect(root,"Objective line icon",new Vector2(-240,10),new Vector2(40,42)).gameObject.AddComponent<ObjectiveLineIcon>();icon.color=new Color(.76f,.62f,.36f);icon.material=material;icon.raycastTarget=false;
   cueSound=new GameObject("Soft objective completion").AddComponent<AudioSource>();cueSound.transform.SetParent(transform,false);cueSound.playOnAwake=false;cueSound.volume=.08f;cueSound.clip=Resources.Load<AudioClip>("Hands/clunk");
   task=Text("Objective task",new Vector2(15,10),new Vector2(470,50),34,new Color(.93f,.91f,.83f));task.transform.localScale=new Vector3(.90f,1,1);
   floor=Text("Relative floor",new Vector2(10,-28),new Vector2(470,25),16,new Color(.76f,.7f,.56f));
   subtitle=Text("Harbor subtitle",new Vector2(0,-72),new Vector2(530,85),19,new Color(.79f,.79f,.72f,.82f));subtitle.fontStyle=FontStyle.Italic;
   strike=Rule("Completed objective strike",new Vector2(15,10),new Vector2(460,1));tick=Rule("Completion tick stem",new Vector2(240,13),new Vector2(12,2));tick.localRotation=Quaternion.Euler(0,0,45);var up=Rule("Completion tick sweep",new Vector2(253,18),new Vector2(24,2));up.SetParent(tick,false);up.anchoredPosition=new Vector2(11,4);up.localRotation=Quaternion.Euler(0,0,90);
   foreach(var h in FindObjectsByType<HandRig>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(h.Node==XRNode.LeftHand)wrist=h.transform.parent;
   built=Time.unscaledTime;Reveal(7,true);
  }
  public void Reveal()=>Reveal(7);
  // Placed once when it appears (or on an explicit recall), level and facing the keeper; never follows the head.
  void Reveal(float seconds,bool place=false){bool hidden=until<=Time.unscaledTime;until=Mathf.Max(until,Time.unscaledTime+seconds);if(hidden||place)Place();}
  void Place()
  {
   var forward=head.forward;forward.y=0;if(forward.sqrMagnitude<1e-4f){forward=head.up;forward.y=0;}if(forward.sqrMagnitude<1e-4f)forward=Vector3.forward;
   var facing=Quaternion.LookRotation(forward.normalized,Vector3.up);root.SetPositionAndRotation(head.position+facing*new Vector3(0,PlaceHeight,PlaceDistance),facing);
  }
  void ShowNote(string text,float seconds,bool place=false){note=text;noteUntil=Time.unscaledTime+seconds;Reveal(seconds,place);}
  string StationAt(int value)=>value>=0&&value<watch.State.Definition.tasks.Length?watch.State.Definition.tasks[value].stationId:"BED";
  string FloorCaption(int value)=>FloorDirection(StationFloor(StationAt(value)),FloorAt(head.position.y)," ");
  public void PreviewCompletion(){pending=full;nextIndex=index;completeUntil=Time.unscaledTime+1.2f;Reveal();}
  void Set(int value,string text){index=value;full=text;changed=Time.unscaledTime;typed=-1;completeUntil=0;Reveal();}
  string Title(int value)=>value== -2?"Watch complete":value<0?"Rest in the upstairs bunk":watch.State.Definition.tasks[value].taskText;
  void Update()
  {
   var s=watch.State;if(s==null)return;if(!radio)radio=FindAnyObjectByType<HarborRadio>();
   int current=s.ObjectiveIndex;
   bool nag=!s.Failed&&PromptWrenchDown(s.Repaired,watch.LocalWrenchHeld),nagStart=nag&&!nagging;nagging=nag;
   if(day!=s.Day){wakePending=day!=0;day=s.Day;Set(current,Title(current));}
   else if(current!=index&&completeUntil<=0)
   {
    if(index>=0&&index<s.Definition.tasks.Length&&s.IsDone(s.Definition.tasks[index].stationId))
    {
     pending=Title(current);nextIndex=current;completeUntil=Time.unscaledTime+1.2f;if(cueSound.clip)cueSound.Play();Reveal();
     // A non-final job is done: name the next one and its floor (the radio only speaks after the last job).
     if(current>=0){var next=NextJobCaption(Title(current),FloorCaption(current));ShowNote(nag?WrenchDownCaption+"\n"+next:next,9);nagStart=false;}
    }
    else Set(current,Title(current));
   }
   if(completeUntil>0&&Time.unscaledTime>=completeUntil)Set(nextIndex,pending??Title(current));
   if(nagStart)ShowNote(WrenchDownCaption,8);
   // Waking in the bunk: show tonight's first job and which way to go, so nobody walks back to the chalkboard.
   if(wakePending&&!watch.Sleeping&&!watch.ObjectivePresentationBlocked){wakePending=false;ShowNote(TonightCaption(Title(current),FloorCaption(current)),12,true);}
   if(Time.unscaledTime-built<.5f)Place(); // Let XR tracking settle before the first placement sticks.
   bool done=completeUntil>0;strike.gameObject.SetActive(done);tick.gameObject.SetActive(done);
   if(done){task.text=full;task.color=new Color(.9f,.75f,.4f);}else{int n=Mathf.Min(full.Length,Mathf.CeilToInt(full.Length*Mathf.Clamp01((Time.unscaledTime-changed)/.4f)));if(typed!=n){typed=n;task.text=full.Substring(0,n);}task.color=new Color(.93f,.91f,.83f);}
   line.localScale=new Vector3(Mathf.Clamp01((Time.unscaledTime-changed)/.4f),1,1);
   string station=StationAt(index);
   int target=StationFloor(station),here=FloorAt(head.position.y);
   if(lastFloor!=here||lastTarget!=target){lastFloor=here;lastTarget=target;floor.text=FloorDirection(target,here);}
   icon.Set(station);
   string words=radio?radio.Subtitle:"";
   if(words.Length>0){caption=words;captionAt=Time.unscaledTime;Reveal(.7f);}
   bool spoken=Time.unscaledTime-captionAt<.7f;
   subtitle.text=spoken?caption:Time.unscaledTime<noteUntil?note:s.WatchStartHeld?WatchGateCaption:"";
   var colour=subtitle.color;colour.a=spoken?.82f*Mathf.Clamp01(1-(Time.unscaledTime-captionAt)/.7f):.82f;subtitle.color=colour;
   if(radio&&cue!=radio.Serial){cue=radio.Serial;Reveal();}
   group.alpha=watch.ObjectivePresentationBlocked?0:Mathf.Clamp01((until-Time.unscaledTime)/.8f);
   var device=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);device.TryGetFeatureValue(CommonUsages.primaryButton,out bool x);device.TryGetFeatureValue(CommonUsages.secondaryButton,out bool y);bool pressed=x||y;if(pressed&&!buttons)Reveal(7,true);buttons=pressed;
   bool looking=wrist&&Vector3.Distance(head.position,wrist.position)<1.1f&&Vector3.Dot(head.forward,(wrist.position-head.position).normalized)>.94f;if(looking&&!glance)Reveal(7,true);glance=looking;
  }
  void OnDestroy(){if(material)Destroy(material);if(fontMaterial)Destroy(fontMaterial);if(backing)Destroy(backing);}
 }
}
