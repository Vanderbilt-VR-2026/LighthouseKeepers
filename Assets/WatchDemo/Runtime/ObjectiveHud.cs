using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;
using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
 public sealed class ObjectiveHud:MonoBehaviour
 {
  WatchDemoController watch;Transform head,wrist;HarborRadio radio;CanvasGroup group;RectTransform root,line,strike,tick,danger;TMP_Text dangerText;
  TMP_Text task,label,floor,subtitle,partner,speakerTag;RawImage subtitleBacking;ObjectiveLineIcon icon;AudioSource cueSound;string caption="";float captionAt;int lastFloor=-99,lastTarget=-99,typed=-1;Material material,fontMaterial;Texture2D backing,subtitleTexture;float built;bool wakePending,nagging;string note="";float noteUntil;
  int index=int.MinValue,day,cue;float changed,until,completeUntil;bool buttons,glance;string full="",pending;int nextIndex;

  public static string WatchGateCaption=>string.Format(PlayerStrings.Get("integrated.watchGate"),Mathf.CeilToInt(MasterPassTuning.Current?MasterPassTuning.Current.watchStartGraceSeconds:45));
  public static string WrenchDownCaption=>PlayerStrings.Get("integrated.wrenchDown");
  public static string NextJobCaption(string task,string floor)=>string.Format(PlayerStrings.Get("integrated.next"),task,floor);
  public static string TonightCaption(string task,string floor)=>string.Format(PlayerStrings.Get("integrated.tonight"),task,floor);
  public static bool PromptWrenchDown(bool repaired,bool held)=>repaired&&held;
  public static int StationFloor(string station)=>station==WatchState.PipeStation?1:station==WatchState.GeneratorStation?2:station=="BED"?3:4;
  public static int FloorAt(float y)=>y<3.4f?1:y<6.7f?2:y<10?3:4;
  public static string FloorDirection(int target,int here,string gap=" ")=>string.Format(PlayerStrings.Get(target>here?"integrated.floorUp":target<here?"integrated.floorDown":"integrated.floorHere"),target);
  string FloorCaption(int value)=>FloorDirection(watch.State.Director!=null?ObjectiveDirector.Floor((KeeperStep)value):StationFloor(value>=0?watch.State.Definition.tasks[value].stationId:"BED"),FloorAt(head.position.y));
  void Place(){var f=head.forward;f.y=0;if(f.sqrMagnitude<.0001f)f=Vector3.forward;var q=Quaternion.LookRotation(f.normalized,Vector3.up);root.SetPositionAndRotation(head.position+q*new Vector3(0,0,1.6f),q);}
  void Reveal(float seconds,bool place=false){bool hidden=until<=Time.unscaledTime;until=Mathf.Max(until,Time.unscaledTime+seconds);if(hidden||place)Place();}
  void ShowNote(string text,float seconds,bool place=false){note=text;noteUntil=Time.unscaledTime+seconds;Reveal(seconds,place);}
  public static ObjectiveHud Create(WatchDemoController w,Transform head)
  {var s=AudioPassSettings.Current;if(!s||!s.objectiveHud)return null;var h=w.gameObject.AddComponent<ObjectiveHud>();h.watch=w;h.head=head;h.Build();return h;}
  static RectTransform Rect(Transform parent,string name,Vector2 p,Vector2 size)
  {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchoredPosition=p;r.sizeDelta=size;return r;}
  TMP_Text Text(string name,Vector2 p,Vector2 size,int pixels,Color c)
  {var t=Rect(root,name,p,size).gameObject.AddComponent<TextMeshProUGUI>();t.font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");t.fontSharedMaterial=fontMaterial;t.fontSize=pixels;t.color=c;t.alignment=TextAlignmentOptions.MidlineLeft;t.raycastTarget=false;t.enableAutoSizing=false;t.textWrappingMode=TextWrappingModes.Normal;t.extraPadding=true;return t;}
  RectTransform Rule(string name,Vector2 p,Vector2 size)
  {var r=Rect(root,name,p,size);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.74f,.58f,.3f,.8f);image.material=material;image.raycastTarget=false;return r;}
  void Build()
  {
   var go=new GameObject("Transient objective tracker",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup));root=go.GetComponent<RectTransform>();root.SetParent(transform,false);root.localScale=Vector3.one*.001f;root.sizeDelta=new Vector2(600,210);
   var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=head.GetComponent<Camera>();canvas.sortingOrder=1000;group=go.GetComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
   material=new Material(Resources.Load<Shader>("ObjectiveOverlay"));fontMaterial=new Material(Resources.Load<Material>("HarborSubtitleSdf"));
   backing=new Texture2D(64,32,TextureFormat.RGBA32,true);var data=new Color[64*32];var random=new System.Random(41);
   for(int y=0;y<32;y++)for(int x=0;x<64;x++){float edge=Mathf.Sin((y+.5f)/32*Mathf.PI)*Mathf.Sin((x+.5f)/64*Mathf.PI);data[y*64+x]=new Color(.018f,.024f,.029f,.30f*edge);}
   backing.SetPixels(data);backing.Apply();var bg=Rect(root,"Subtle gradient backing",Vector2.zero,root.sizeDelta).gameObject.AddComponent<RawImage>();bg.texture=backing;bg.material=material;bg.raycastTarget=false;
   line=Rule("Brass accent",new Vector2(-265,76),new Vector2(480,1.4f));line.pivot=new Vector2(0,.5f);
   label=Text(LighthouseKeepers.Core.PlayerStrings.Get("legacy.ObjectiveHud.1"),new Vector2(-5,53),new Vector2(510,25),14,new Color(.74f,.65f,.48f,.72f));label.text=LighthouseKeepers.Core.PlayerStrings.Get("legacy.ObjectiveHud.2");
   icon=Rect(root,"Objective line icon",new Vector2(-240,10),new Vector2(40,42)).gameObject.AddComponent<ObjectiveLineIcon>();icon.color=new Color(.76f,.62f,.36f);icon.material=material;icon.raycastTarget=false;
   cueSound=new GameObject("Soft objective completion").AddComponent<AudioSource>();cueSound.transform.SetParent(transform,false);cueSound.playOnAwake=false;cueSound.volume=.08f;cueSound.clip=Resources.Load<AudioClip>("Hands/clunk");
   task=Text("Objective task",new Vector2(15,10),new Vector2(510,50),28,new Color(.93f,.91f,.83f));task.transform.localScale=new Vector3(.90f,1,1);
   floor=Text("Relative floor",new Vector2(10,-28),new Vector2(470,25),16,new Color(.76f,.7f,.56f));
   partner=Text("Partner job",new Vector2(10,-48),new Vector2(470,20),14,new Color(.76f,.7f,.56f));
   subtitleBacking=Rect(root,"Subtitle soft backing",new Vector2(0,-126),new Vector2(850,145)).gameObject.AddComponent<RawImage>();subtitleBacking.color=new Color(.012f,.018f,.023f,.8f);subtitleBacking.material=material;subtitleBacking.raycastTarget=false;
   subtitleTexture=new Texture2D(64,32,TextureFormat.RGBA32,true);var shade=new Color[64*32];
   for(int y=0;y<32;y++)for(int x=0;x<64;x++){float edge=Mathf.Min(Mathf.Min(x,63-x)/5f,Mathf.Min(y,31-y)/4f);shade[y*64+x]=new Color(1,1,1,Mathf.SmoothStep(0,1,Mathf.Clamp01(edge)));}
   subtitleTexture.SetPixels(shade);subtitleTexture.Apply();subtitleBacking.texture=subtitleTexture;
   speakerTag=Text("Subtitle speaker",new Vector2(0,-78),new Vector2(800,25),16,new Color(.84f,.73f,.51f));
   subtitle=Text("Harbor subtitle",new Vector2(0,-137),new Vector2(800,84),24,Color.white);subtitle.overflowMode=TextOverflowModes.Page;subtitle.margin=new Vector4(0,0,0,0);subtitle.fontSharedMaterial=fontMaterial;speakerTag.fontSharedMaterial=fontMaterial;
   strike=Rule("Completed objective strike",new Vector2(15,10),new Vector2(460,1));tick=Rule("Completion tick stem",new Vector2(240,13),new Vector2(12,2));tick.localRotation=Quaternion.Euler(0,0,45);var up=Rule("Completion tick sweep",new Vector2(253,18),new Vector2(24,2));up.SetParent(tick,false);up.anchoredPosition=new Vector2(11,4);up.localRotation=Quaternion.Euler(0,0,90);
   foreach(var h in FindObjectsByType<HandRig>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(h.Node==XRNode.LeftHand)wrist=h.transform.parent;
   danger=Rule("Danger fill",new Vector2(-220,-216),new Vector2(440,6));danger.pivot=new Vector2(0,.5f);dangerText=Text("Danger cause",new Vector2(0,-238),new Vector2(440,20),14,new Color(.95f,.72f,.37f));
   built=Time.unscaledTime;Reveal(7,true);
  }
  public void Reveal()=>Reveal(7,true);
  public void PreviewCompletion(){pending=full;nextIndex=index;completeUntil=Time.unscaledTime+1.2f;Reveal();}
  void Set(int value,string text){index=value;full=text;changed=Time.unscaledTime;typed=-1;completeUntil=0;Reveal();}
  string Title(int value)=>watch.State.Director!=null?watch.State.Director.Instruction((KeeperStep)value):value== -2?KeeperStrings.Get("hud.complete"):value<0?KeeperStrings.Get("hud.rest"):watch.State.Definition.tasks[value].taskText;
  void Update()
  {
   var s=watch.State;if(s==null)return;if(!radio)radio=FindAnyObjectByType<HarborRadio>();
   int current=s.Director!=null?(int)watch.CurrentInstruction:s.ObjectiveIndex;
   partner.text=s.Director!=null&&watch.PartnerInstruction!=KeeperStep.None?string.Format(KeeperStrings.Get("partner"),KeeperStrings.Get("job."+ObjectiveDirector.Job(watch.PartnerInstruction))):"";
   if(day!=s.Day){wakePending=day!=0;day=s.Day;Set(current,Title(current));}
   else if(current!=index&&completeUntil<=0)
   {
    if(s.Director!=null&&ObjectiveDirector.Job((KeeperStep)index)==ObjectiveDirector.Job((KeeperStep)current)||s.Director==null&&index>=0&&index<s.Definition.tasks.Length&&s.IsDone(s.Definition.tasks[index].stationId)){pending=Title(current);nextIndex=current;completeUntil=Time.unscaledTime+1.2f;if(cueSound.clip)cueSound.Play();Reveal();}
    else Set(current,Title(current));
   }
   if(s.Director!=null&&current==index&&Title(current)!=full&&completeUntil<=0)Set(current,Title(current));
   if(completeUntil>0&&Time.unscaledTime>=completeUntil)Set(nextIndex,pending??Title(current));
   bool nag=!s.Failed&&PromptWrenchDown(s.Repaired,watch.LocalWrenchHeld);
   if(nag&&!nagging&&!radio)ShowNote(WrenchDownCaption,7);nagging=nag;
   if(wakePending&&!watch.Sleeping&&!watch.ObjectivePresentationBlocked){wakePending=false;if(!radio||HarborRadio.TonightCue(s)==0)ShowNote(TonightCaption(Title(current),FloorCaption(current)),10,true);else Reveal(10,true);}
   if(Time.unscaledTime-built<.5f)Place();
   bool done=completeUntil>0;strike.gameObject.SetActive(done);tick.gameObject.SetActive(done);
   if(done){task.text=full;task.color=new Color(.9f,.75f,.4f);}else{int n=Mathf.Min(full.Length,Mathf.CeilToInt(full.Length*Mathf.Clamp01((Time.unscaledTime-changed)/.4f)));if(typed!=n){typed=n;task.text=full.Substring(0,n);}task.color=new Color(.93f,.91f,.83f);}
   line.localScale=new Vector3(Mathf.Clamp01((Time.unscaledTime-changed)/.4f),1,1);
   string station=s.Director!=null?ObjectiveDirector.Job((KeeperStep)index):index>=0&&index<s.Definition.tasks.Length?s.Definition.tasks[index].stationId:"BED";
   int target=s.Director!=null?ObjectiveDirector.Floor((KeeperStep)index):station==WatchState.PipeStation?1:station==WatchState.GeneratorStation?2:station=="BED"?3:4;
   int here=FloorAt(head.position.y);
   if(lastFloor!=here||lastTarget!=target){lastFloor=here;lastTarget=target;floor.text=FloorDirection(target,here);}
   icon.Set(station);
   bool showVoice=HarborSubtitleSettings.Enabled&&radio&&radio.SubtitleAlpha>0;
   string words=showVoice?radio.Subtitle:"";
   string display=showVoice?words:Time.unscaledTime<noteUntil?note:s.WatchStartHeld&&!radio?WatchGateCaption:"";
   int size=HarborSubtitleSettings.Large?34:28;
   if(subtitle.text!=display||subtitle.fontSize!=size){subtitle.text=display;subtitle.fontSize=size;subtitle.rectTransform.sizeDelta=new Vector2(800,size*2.05f);subtitle.maxVisibleCharacters=int.MaxValue;subtitle.ForceMeshUpdate();}
   int visible=showVoice?radio.SubtitleCharacters:display.Length;
   int page=1;
   // Paginate long recordings into two readable lines rather than shrinking or clipping text.
   var info=subtitle.textInfo;
   for(int i=0;i<info.characterCount&&info.characterInfo[i].index<visible;i++)page=info.characterInfo[i].pageNumber+1;
   subtitle.pageToDisplay=page;subtitle.maxVisibleCharacters=visible;
   float alpha=showVoice?radio.SubtitleAlpha:1;
   bool urgent=showVoice&&radio.DangerSubtitle;
   subtitle.color=urgent?new Color(1,.78f,.42f,alpha):new Color(.98f,.98f,.94f,alpha);
   speakerTag.text=showVoice?KeeperStrings.Get(urgent?"subtitle.dangerSpeaker":"subtitle.speaker"):"";speakerTag.alpha=alpha;
   subtitleBacking.gameObject.SetActive(display.Length>0);subtitleBacking.color=new Color(.012f,.018f,.023f,.8f*alpha);
   if(showVoice)until=Mathf.Max(until,Time.unscaledTime+.9f);
   if(radio&&cue!=radio.Serial){cue=radio.Serial;Reveal();}
   bool unsafeNow=s.PressureEnabled&&!s.Complete&&s.Danger>=.5f;
   danger.gameObject.SetActive(unsafeNow);dangerText.gameObject.SetActive(unsafeNow);if(unsafeNow){danger.localScale=new Vector3(s.Danger,1,1);dangerText.text=KeeperStrings.Get(s.DangerKey);until=Mathf.Max(until,Time.unscaledTime+.5f);}
   group.alpha=watch.ObjectivePresentationBlocked?0:Mathf.Clamp01((until-Time.unscaledTime)/.8f);
   var device=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);device.TryGetFeatureValue(CommonUsages.primaryButton,out bool x);device.TryGetFeatureValue(CommonUsages.secondaryButton,out bool y);bool pressed=x||y;if(pressed&&!buttons){Reveal();if(s.PressureEnabled&&radio)radio.RequestReplay();}buttons=pressed;
   bool looking=wrist&&Vector3.Distance(head.position,wrist.position)<1.1f&&Vector3.Dot(head.forward,(wrist.position-head.position).normalized)>.94f;if(looking&&!glance)Reveal();glance=looking;
   // World-fixed until recall or the next objective; never trails head turns.
  }
  void OnDestroy(){if(material)Destroy(material);if(fontMaterial)Destroy(fontMaterial);if(backing)Destroy(backing);if(subtitleTexture)Destroy(subtitleTexture);}
 }
}
