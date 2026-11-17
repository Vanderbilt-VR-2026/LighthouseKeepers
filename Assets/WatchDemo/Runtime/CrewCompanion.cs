using UnityEngine;using UnityEngine.AI;using TMPro;
using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Net;using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo {
 public enum CompanionCommand { Work,Follow,Wait,Help }
 // Bounded, host-only helper. No general task solver, remote repairs, tool theft or cloud service.
 public sealed class CrewCompanion:MonoBehaviour {
  WatchDemoController watch;Transform head;CrewCompanionTuning tuning;HunterNavigation navigation;NavMeshAgent agent;
  GameObject actor;CrewAvatarModel model;TextMeshPro slate;Transform slateRoot;Material slateMaterial;
  readonly RaycastHit[] floorHits=new RaycastHit[24];
  WatchPressureValve valve;CompanionCommand command;float nextPath,readFor,still;Vector3 lastPosition,goal;bool hasGoal,reading,holding,rescuing;CompanionCommand rescueReturn;Vector3 rescueHand;int night;string learnedCode="",statusKey="companion.follow",shownSlate;
  public bool Active=>actor&&actor.activeSelf;
  public string LearnedCode=>learnedCode;
  public string Status=>KeeperStrings.Get(statusKey);
  bool lessonMode;CompanionCommand lessonReturn;float possession,lift;readonly RaycastHit[] ceilingHits=new RaycastHit[24];
  public Vector3 OfferedHand=>model&&model.FullBody&&model.FullBody.Joint("LeftHand")?model.FullBody.Joint("LeftHand").position:Position+Vector3.up;
  public Vector3 Position=>actor?actor.transform.position:Vector3.zero;
  public CrewDuty Duty=>watch.CurrentDuty==CrewDuty.Lightkeeper?CrewDuty.Machinist:CrewDuty.Lightkeeper;
  public CompanionCommand Command=>command;
  public static void Create(WatchDemoController w,Transform head){var t=CrewCompanionTuning.Current;if(!t||!t.enabled)return;var c=w.gameObject.AddComponent<CrewCompanion>();c.watch=w;c.head=head;c.tuning=t;}
  public void SetCommand(CompanionCommand value){command=value;statusKey="companion.follow";Release();hasGoal=false;readFor=0;nextPath=0;still=0;}
  bool Spawn(){
   navigation=new HunterNavigation(HorrorTuning.Current,Resources.Load<NavMeshData>("HunterNavigation"));
   if(!navigation.SampleFloor(tuning.start,out var start)){navigation.Dispose();navigation=null;return false;}
   actor=new GameObject("Owen — scripted keeper");actor.transform.position=start;
   agent=actor.AddComponent<NavMeshAgent>();agent.height=1.8f;agent.radius=.2f;agent.speed=tuning.walkSpeed;agent.acceleration=6;agent.angularSpeed=160;agent.stoppingDistance=tuning.arriveDistance;agent.autoBraking=true;
   var prefab=Resources.Load<GameObject>("PF_CompanionKeeper");if(!prefab){Retire();return false;}
   model=Instantiate(prefab,actor.transform).GetComponent<CrewAvatarModel>();if(!model||!model.PrepareVisualOnly(out _)){Retire();return false;}
   slateRoot=new GameObject("Owen's slate").transform;slateRoot.SetParent(actor.transform,false);
   var plate=GameObject.CreatePrimitive(PrimitiveType.Cube);plate.name="Chalk slate";plate.transform.SetParent(slateRoot,false);plate.transform.localScale=new Vector3(.46f,.28f,.018f);plate.GetComponent<Collider>().enabled=false;Destroy(plate.GetComponent<Collider>());
   slateMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));slateMaterial.SetColor("_BaseColor",new Color(.025f,.035f,.03f));plate.GetComponent<Renderer>().sharedMaterial=slateMaterial;
   slate=new GameObject("Owen chalk").AddComponent<TextMeshPro>();slate.transform.SetParent(slateRoot,false);slate.transform.localPosition=new Vector3(0,0,-.012f);slate.rectTransform.sizeDelta=new Vector2(.43f,.25f);slate.fontSize=.20f;slate.alignment=TextAlignmentOptions.Center;slate.color=new Color(.84f,.85f,.75f);slate.text="";
   valve=FindAnyObjectByType<WatchPressureValve>();command=CompanionCommand.Work;readFor=still=0;hasGoal=holding=false;lastPosition=start;night=watch.State.Day;watch.State.SetScriptedPartner(true);return true;
  }
  void Release(){holding=false;if(watch&&watch.State!=null&&!watch.State.ReadOnly)watch.State.Pressure?.Hold(WatchState.CompanionActor,false);if(agent&&agent.isOnNavMesh)agent.ResetPath();}
  void Retire(){Release();if(watch&&watch.State!=null&&!watch.State.ReadOnly)watch.State.SetScriptedPartner(false);DestroyVisual();}
  void DestroyVisual(){if(actor){actor.SetActive(false);Destroy(actor);}navigation?.Dispose();navigation=null;if(slateMaterial)Destroy(slateMaterial);actor=null;agent=null;learnedCode="";shownSlate=null;hasGoal=false;}
  bool FloorPoint(Vector3 near,out Vector3 floor){
   floor=near;int count=Physics.RaycastNonAlloc(near+Vector3.up*.15f,Vector3.down,floorHits,3,~0,QueryTriggerInteraction.Ignore);float best=float.NegativeInfinity;
   if(count==floorHits.Length)return false;
   for(int i=0;i<count;i++){var h=floorHits[i];if(h.normal.y<.55f||h.collider.attachedRigidbody||h.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())continue;if(h.point.y>best){best=h.point.y;floor=h.point;}}
   return best>float.NegativeInfinity;
  }
  bool Destination(Vector3 near,out Vector3 sampled){
   sampled=default;if(!FloorPoint(near,out var floor))return false;
   if(navigation.SampleFloor(floor,out sampled))return true;
   for(int ring=1;ring<=3;ring++)for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var p=floor+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(ring*.25f);if(navigation.SampleFloor(p,out sampled))return true;}
   return false;
  }
  bool MoveTo(Vector3 near){
   if(Time.time>=nextPath){nextPath=Time.time+tuning.repathSeconds;if(!Destination(near,out var p)||!navigation.Route(Position,p,out _)||!agent.SetDestination(p)){SetCommand(CompanionCommand.Wait);statusKey="companion.blocked";return false;}goal=p;hasGoal=true;}
   if(!hasGoal||agent.pathPending)return false;
   return Vector3.Distance(Position,goal)<=tuning.arriveDistance+.1f;
  }
  void Update(){
   if(!watch||watch.State==null)return;var s=watch.State;
   if(s.ReadOnly||s.HumanKeeperCount>1||!tuning.enabled){if(Active)Retire();return;}
   if(!Active){if(Time.time<nextPath)return;nextPath=Time.time+3;if(!Spawn())return;}
   if(night!=s.Day){night=s.Day;learnedCode="";SetCommand(CompanionCommand.Work);}
   if(watch.Sleeping||s.Failed){Release();statusKey="companion.wait";return;}
   if(s.Recovery.LessonActive){
    if(!lessonMode){lessonMode=true;lessonReturn=command;Release();hasGoal=false;nextPath=0;}reading=false;statusKey="companion.follow";
    if(s.Recovery.Lesson==1){if(MoveTo(HorrorTuning.Current.lessonPartnerSpot)){statusKey="companion.wait";still=0;return;}}else if(agent.isOnNavMesh)agent.ResetPath();
    if(s.Recovery.Downed((int)Duty))statusKey="companion.possessed";
    CheckRoute();return;
   }
   if(lessonMode){lessonMode=false;SetCommand(lessonReturn);}
   bool rescueNeeded=s.Recovery.AnyDown;
   if(rescueNeeded&&!rescuing){rescueReturn=command;Release();hasGoal=false;nextPath=0;}
   else if(!rescueNeeded&&rescuing)SetCommand(rescueReturn);
   rescuing=rescueNeeded;
   if(rescuing){
    if(holding)Release();reading=false;readFor=0;statusKey="companion.rescueGoing";
    var rescue=watch.GetComponent<CrewRecovery>();
    if(rescue.HandPoint((int)watch.CurrentDuty,out var hand)){
     var toward=Vector3.ProjectOnPlane(Position-hand,Vector3.up).normalized;if(toward.sqrMagnitude<.1f)toward=Vector3.right;
     MoveTo(hand+toward*.65f);
     if(Vector3.Distance(Position+Vector3.up,hand)<tuning.reach){agent.ResetPath();rescueHand=Vector3.MoveTowards(Position+Vector3.up*1.15f,hand,tuning.reach);if(watch.CompanionRescue(rescueHand))statusKey="companion.rescuing";}
    }
    CheckRoute();return;
   }
   if(command==CompanionCommand.Wait){Release();if(statusKey!="companion.blocked")statusKey="companion.wait";return;}
   reading=false;bool support=command==CompanionCommand.Work||command==CompanionCommand.Help;
   if(support&&s.Pressure?.CrewRequired==true&&!s.Repaired&&valve&&(watch.CurrentInstruction==KeeperStep.TightenBolt||Vector3.Distance(head.position,valve.transform.position)<3)){
    statusKey="companion.valveGoing";var target=valve.transform.position+valve.transform.forward*.62f+valve.transform.right*.42f;
    if(MoveTo(target)&&Vector3.Distance(Position+Vector3.up, valve.transform.position)<tuning.reach){
     agent.ResetPath();holding=watch.PressureCommand(WatchState.CompanionActor,true);statusKey=holding?"companion.valve":"companion.wait";
    }
   }else{
    if(holding)Release();
    if(support&&s.HasRadioRelay&&s.Radio.Step==2&&learnedCode.Length==0&&CodeSource.Active){
     statusKey="companion.codeGoing";var paper=CodeSource.Active.transform;var target=paper.position-paper.forward*.85f;
     if(MoveTo(target)&&Vector3.Distance(Position+Vector3.up,paper.position)<tuning.reach){agent.ResetPath();reading=true;readFor+=Time.deltaTime;if(readFor>=tuning.readSeconds){learnedCode=CodeSource.CodeText(s.Radio);readFor=0;hasGoal=false;nextPath=0;}}
    }else{
     readFor=0;statusKey=learnedCode.Length>0?"companion.code":"companion.follow";
     var forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;var offset=-forward*tuning.followDistance+Vector3.Cross(Vector3.up,forward)*.55f;
     if(Vector3.Distance(Position+Vector3.up*1.6f,head.position)>tuning.followDistance+.3f)MoveTo(head.position+offset);else {agent.ResetPath();hasGoal=false;}
    }
   }
   CheckRoute();
  }
  void CheckRoute(){var moved=Position-lastPosition;
   if(agent.hasPath&&!agent.pathPending&&moved.sqrMagnitude<.000001f)still+=Time.deltaTime;else still=0;
   if(!navigation.SegmentClear(lastPosition,Position)||still>tuning.stuckSeconds){agent.Warp(lastPosition);SetCommand(CompanionCommand.Wait);statusKey="companion.blocked";}
   lastPosition=Position;
  }
  void LateUpdate(){
   if(!Active||!model||!model.FullBody)return;
   var rotation=actor.transform.rotation;var forward=rotation*Vector3.forward;var right=rotation*Vector3.right;
   if(!agent.hasPath){var look=Vector3.ProjectOnPlane((holding&&valve?valve.transform.position:head.position)-Position,Vector3.up);if(look.sqrMagnitude>.01f){rotation=Quaternion.RotateTowards(rotation,Quaternion.LookRotation(look),120*Time.deltaTime);actor.transform.rotation=rotation;forward=rotation*Vector3.forward;right=rotation*Vector3.right;}}
   float swing=Mathf.Sin(Time.time*5)*Mathf.Clamp01(agent.velocity.magnitude)*.10f;
   var left=Position+Vector3.up*1.05f-right*.22f+forward*(.32f+swing);var r=Position+Vector3.up*1.05f+right*.22f+forward*(.32f-swing);
   if(rescuing&&statusKey=="companion.rescuing")r=rescueHand;
   else if(holding&&valve)r=valve.transform.position+valve.transform.forward*.09f;
   var recovery=watch.State.Recovery;bool down=recovery.Downed((int)Duty);float amount=down&&DrownedAppearance.Sightings?1-recovery.Slot((int)Duty).progress:0;
   possession=Mathf.MoveTowards(possession,amount,Time.deltaTime*.65f);float maximum=PossessionLesson.SafeLift(Position+Vector3.up*1.8f,HorrorTuning.Current.possessionLift,HorrorTuning.Current.ceilingClearance,ceilingHits);
   lift=Mathf.Min(maximum,Mathf.MoveTowards(lift,amount*maximum,Time.deltaTime*(down?HorrorTuning.Current.possessionRiseSpeed:HorrorTuning.Current.possessionRiseSpeed*1.8f)));model.FullBody.Possession=possession;model.FullBody.VisualLift=lift;
   if(down){left=Position+Vector3.up*1.05f-right*.22f+forward*.55f;r=Position+Vector3.up*.85f+right*.25f;}slateRoot.gameObject.SetActive(!down);
   model.FullBody.ApplyPose(new KeeperPoseFrame{Actor=WatchState.CompanionActor,Sequence=1,Head=Position+Vector3.up*model.FullBody.ReferenceEyeHeight,HeadRotation=rotation,Left=left,Right=r,LeftRotation=rotation,RightRotation=rotation,LeftGrip=.5f,RightGrip=holding||rescuing?1:.5f},Time.deltaTime,Position.y);
   slateRoot.SetPositionAndRotation((holding?left:(left+r)*.5f)+forward*.04f,rotation*Quaternion.Euler(0,180,0));
   string content=learnedCode.Length>0&&!holding&&!reading&&!rescuing?learnedCode:Status;
   if(shownSlate!=content){shownSlate=content;slate.text=KeeperStrings.Get("companion.name")+"\n"+content;}
  }
  // The companion shares the controller lifetime. Scene teardown must not publish changes
  // to already-destroyed station listeners; normal human handoff uses Retire instead.
  void OnDestroy()=>DestroyVisual();
 }
}
