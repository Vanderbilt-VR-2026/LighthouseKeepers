using UnityEngine;using UnityEngine.AI;using TMPro;
using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Net;using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo {
 public enum CompanionCommand { Work,Follow,Wait,Help }
 // Bounded, host-only helper. No general task solver, remote repairs, tool theft or cloud service.
 public sealed class CrewCompanion:MonoBehaviour {
  WatchDemoController watch;Transform head;CrewCompanionTuning tuning;HunterNavigation navigation;NavMeshAgent agent;
  GameObject actor;CrewAvatarModel model;TextMeshPro nameTag;Transform nameRoot;
  readonly RaycastHit[] floorHits=new RaycastHit[24];
  WatchPressureValve valve;CompanionCommand command;float nextPath,readFor,still;Vector3 lastPosition,goal;bool hasGoal,holding,rescuing;CompanionCommand rescueReturn;Vector3 rescueHand;int night;string learnedCode="",statusKey="companion.follow";
  public bool Active=>actor&&actor.activeSelf;
  public string LearnedCode=>learnedCode;
  public string Status=>KeeperStrings.Get(statusKey);
  bool wantsCode;float reportUntil,radioPoseUntil,nextReport;string reportLine="";GameObject radioProp;
  public bool ReportActive=>Time.unscaledTime<reportUntil;public string ReportLine=>reportLine;
  public void RequestReport(){if(!Active||Time.unscaledTime<nextReport)return;nextReport=Time.unscaledTime+tuning.reportCooldown;if(watch.State.HasRadioRelay&&learnedCode.Length==0){wantsCode=true;SetCommand(CompanionCommand.Work);Report(KeeperStrings.Get("companion.fetchReply"));}else Report(learnedCode.Length>0?KeeperStrings.Get("companion.codeReply")+"\n"+learnedCode:KeeperStrings.Get("companion.readyReply"));}
  void Report(string text){reportLine=text;reportUntil=Time.unscaledTime+tuning.reportSeconds;radioPoseUntil=Time.unscaledTime+tuning.radioGestureSeconds;var n=FindAnyObjectByType<HarborRadio>();if(n)n.RecordSilentLine(text,KeeperStrings.Get("companion.name"));watch.GetComponent<ObjectiveHud>()?.RevealFor(tuning.reportSeconds);}
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
   nameRoot=new GameObject("Owen cloth name patch").transform;nameRoot.SetParent(actor.transform,false);
   nameTag=new GameObject("Keeper name").AddComponent<TextMeshPro>();nameTag.transform.SetParent(nameRoot,false);nameTag.rectTransform.sizeDelta=new Vector2(.13f,.045f);nameTag.fontSize=.55f;nameTag.alignment=TextAlignmentOptions.Center;nameTag.color=new Color(.8f,.74f,.56f);nameTag.text=KeeperStrings.Get("companion.name");
   var parts=watch.GetComponent<M2StationParts>();radioProp=new GameObject("Owen working radio");radioProp.transform.SetParent(actor.transform,false);
   parts.Part("Owen radio case",radioProp.transform,Vector3.zero,new Vector3(.065f,.16f,.05f),parts.Dark);
   parts.Part("Owen radio aerial",radioProp.transform,new Vector3(-.02f,.12f,0),new Vector3(.008f,.12f,.008f),parts.Dark);
   valve=FindAnyObjectByType<WatchPressureValve>();command=CompanionCommand.Work;readFor=still=0;hasGoal=holding=false;lastPosition=start;night=watch.State.Day;watch.State.SetScriptedPartner(true);return true;
  }
  void Release(){holding=false;if(watch&&watch.State!=null&&!watch.State.ReadOnly)watch.State.Pressure?.Hold(WatchState.CompanionActor,false);if(agent&&agent.isOnNavMesh)agent.ResetPath();}
  void Retire(){Release();if(watch&&watch.State!=null&&!watch.State.ReadOnly)watch.State.SetScriptedPartner(false);DestroyVisual();}
  void DestroyVisual(){if(actor){actor.SetActive(false);Destroy(actor);}navigation?.Dispose();navigation=null;actor=null;agent=null;learnedCode="";hasGoal=false;}
  bool FloorPoint(Vector3 near,out Vector3 floor){
   floor=near;int count=Physics.RaycastNonAlloc(near+Vector3.up*.15f,Vector3.down,floorHits,3,~0,QueryTriggerInteraction.Ignore);float best=float.NegativeInfinity;
   if(count==floorHits.Length)return false;
   for(int i=0;i<count;i++){var h=floorHits[i];if(h.normal.y<.55f||h.collider.attachedRigidbody||h.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>()||!navigation.SampleFloor(h.point,out _))continue;if(h.point.y>best){best=h.point.y;floor=h.point;}}
   return best>float.NegativeInfinity;
  }
  bool Destination(Vector3 near,out Vector3 sampled){
   sampled=default;if(!FloorPoint(near,out var floor))return false;
   if(navigation.SampleFloor(floor,out sampled))return true;
   for(int ring=1;ring<=3;ring++)for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var p=floor+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(ring*.25f);if(navigation.SampleFloor(p,out sampled))return true;}
   return false;
  }
  bool MoveTo(Vector3 near){
   if(!agent.isOnNavMesh)return false;
   if(Time.time>=nextPath){
    nextPath=Time.time+tuning.repathSeconds;
    if(!Destination(near,out var p)){RouteBlocked();return false;}
    goal=p;hasGoal=true;
    if(Vector3.Distance(Position,p)<=tuning.arriveDistance+.1f){agent.ResetPath();return true;}
    if(!navigation.Route(Position,p,out _)||!agent.SetDestination(p)){RouteBlocked();return false;}
   }
   return hasGoal&&!agent.pathPending&&Vector3.Distance(Position,goal)<=tuning.arriveDistance+.1f;
  }
  void RouteBlocked(){
   // Closed lesson gates, a player or a dropped tool are temporary obstacles, not a WAIT order.
   Release();hasGoal=false;still=0;nextPath=Time.time+Mathf.Max(.5f,tuning.repathSeconds);statusKey="companion.blocked";
  }
  void Update(){
   if(!watch||watch.State==null)return;var s=watch.State;
   if(s.ReadOnly||s.HumanKeeperCount>1||!tuning.enabled){if(Active)Retire();return;}
   if(!Active){if(Time.time<nextPath)return;nextPath=Time.time+3;if(!Spawn())return;}
   if(night!=s.Day){night=s.Day;learnedCode="";wantsCode=false;reportUntil=0;SetCommand(CompanionCommand.Work);}
   if(watch.Sleeping||s.Failed){Release();statusKey="companion.wait";return;}
   if(s.Recovery.LessonActive){
    if(!lessonMode){lessonMode=true;lessonReturn=command;Release();hasGoal=false;nextPath=0;}statusKey="companion.follow";
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
    if(holding)Release();;readFor=0;statusKey="companion.rescueGoing";
    var rescue=watch.GetComponent<CrewRecovery>();
    if(rescue.HandPoint((int)watch.CurrentDuty,out var hand)){
     var toward=Vector3.ProjectOnPlane(Position-hand,Vector3.up).normalized;if(toward.sqrMagnitude<.1f)toward=Vector3.right;
     MoveTo(hand+toward*.65f);
     if(Vector3.Distance(Position+Vector3.up,hand)<tuning.reach){agent.ResetPath();rescueHand=Vector3.MoveTowards(Position+Vector3.up*1.15f,hand,tuning.reach);if(watch.CompanionRescue(rescueHand))statusKey="companion.rescuing";}
    }
    CheckRoute();return;
   }
   if(command==CompanionCommand.Wait){Release();if(statusKey!="companion.blocked")statusKey="companion.wait";return;}
   bool support=command==CompanionCommand.Work||command==CompanionCommand.Help;
   if(support&&s.Pressure?.CrewRequired==true&&!s.Repaired&&valve&&(watch.CurrentInstruction==KeeperStep.TightenBolt||Vector3.Distance(head.position,valve.transform.position)<3)){
    statusKey="companion.valveGoing";var target=valve.transform.position+valve.transform.forward*.62f+valve.transform.right*.42f;
    if(MoveTo(target)&&Vector3.Distance(Position+Vector3.up, valve.transform.position)<tuning.reach){
     agent.ResetPath();holding=watch.PressureCommand(WatchState.CompanionActor,true);statusKey=holding?"companion.valve":"companion.wait";
    }
   }else{
    if(holding)Release();
    if(support&&s.HasRadioRelay&&(s.Repaired||wantsCode)&&learnedCode.Length==0&&CodeSource.Active){
     statusKey="companion.codeGoing";var paper=CodeSource.Active.transform;var target=paper.position-paper.forward*.85f;
     if(MoveTo(target)&&Vector3.Distance(Position+Vector3.up,paper.position)<tuning.reach){agent.ResetPath();readFor+=Time.deltaTime;if(readFor>=tuning.readSeconds){learnedCode=CodeSource.CodeText(s.Radio);readFor=0;hasGoal=false;nextPath=0;if(wantsCode){wantsCode=false;Report(KeeperStrings.Get("companion.codeReply")+"\n"+learnedCode);}}}
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
   if(!navigation.SegmentClear(lastPosition,Position)||still>tuning.stuckSeconds){agent.Warp(lastPosition);RouteBlocked();}
   lastPosition=Position;
  }
  void LateUpdate(){
   if(!Active||!model||!model.FullBody)return;
   var rotation=actor.transform.rotation;var forward=rotation*Vector3.forward;var right=rotation*Vector3.right;
   if(!agent.hasPath){var look=Vector3.ProjectOnPlane((holding&&valve?valve.transform.position:head.position)-Position,Vector3.up);if(look.sqrMagnitude>.01f){rotation=Quaternion.RotateTowards(rotation,Quaternion.LookRotation(look),120*Time.deltaTime);actor.transform.rotation=rotation;forward=rotation*Vector3.forward;right=rotation*Vector3.right;}}
   float swing=Mathf.Sin(Time.time*5)*Mathf.Clamp01(agent.velocity.magnitude)*.10f;
   var left=Position+Vector3.up*.81f-right*.28f+forward*(.06f+swing);var r=Position+Vector3.up*.81f+right*.28f+forward*(.06f-swing);
   if(Time.unscaledTime<radioPoseUntil)r=Position+Vector3.up*1.57f+right*.21f+forward*.16f;
   if(rescuing&&statusKey=="companion.rescuing")r=rescueHand;
   else if(holding&&valve)r=valve.transform.position+valve.transform.forward*.09f;
   var recovery=watch.State.Recovery;bool down=recovery.Downed((int)Duty);float amount=down&&DrownedAppearance.Sightings?1-recovery.Slot((int)Duty).progress:0;
   possession=Mathf.MoveTowards(possession,amount,Time.deltaTime*.65f);float maximum=PossessionLesson.SafeLift(Position+Vector3.up*1.8f,HorrorTuning.Current.possessionLift,HorrorTuning.Current.ceilingClearance,ceilingHits);
   lift=Mathf.Min(maximum,Mathf.MoveTowards(lift,amount*maximum,Time.deltaTime*(down?HorrorTuning.Current.possessionRiseSpeed:HorrorTuning.Current.possessionRiseSpeed*1.8f)));model.FullBody.Possession=possession;model.FullBody.VisualLift=lift;
   if(down){left=Position+Vector3.up*1.05f-right*.22f+forward*.55f;r=Position+Vector3.up*(.85f+.11f*Mathf.Sin(Time.time*1.7f)*possession)+right*(.25f+.06f*Mathf.Sin(Time.time*1.1f)*possession);}nameRoot.gameObject.SetActive(!down);
   model.FullBody.ApplyPose(new KeeperPoseFrame{Actor=WatchState.CompanionActor,Sequence=1,Head=Position+Vector3.up*model.FullBody.ReferenceEyeHeight,HeadRotation=rotation,Left=left,Right=r,LeftRotation=rotation,RightRotation=rotation,LeftGrip=.5f,RightGrip=holding||rescuing||Time.unscaledTime<radioPoseUntil?1:.5f},Time.deltaTime,Position.y);
   nameRoot.SetPositionAndRotation(Position+Vector3.up*(1.36f+lift)-right*.14f+forward*.19f,rotation*Quaternion.Euler(0,180,0));
   radioProp.SetActive(!down);radioProp.transform.SetPositionAndRotation(Time.unscaledTime<radioPoseUntil?r:Position+Vector3.up*.9f+right*.24f,rotation);

  }
  // The companion shares the controller lifetime. Scene teardown must not publish changes
  // to already-destroyed station listeners; normal human handoff uses Retire instead.
  void OnDestroy()=>DestroyVisual();
 }
}
