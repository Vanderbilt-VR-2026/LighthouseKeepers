using UnityEngine;using LighthouseKeepers.Core;using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class CrewCompanion {
  WatchEngineControls workEngine;WatchPipeRepair workPipe;WatchPowerPanel workPanel;WatchRadioConsole workRadio;WatchBeaconStation workBeacon;
  GameObject workWrench;Vector3 workHand;bool working,workingPipe,clearanceRequested,clearanceOffered,carryingFuel;
  float workFor;double lastWatchElapsed;KeeperStep workStep;
  public KeeperStep WorkingStep=>workStep;
  public bool Transmitting=>working&&workStep==KeeperStep.RadioTransmit&&clearanceRequested;
  void PrepareStationWork(){
   workEngine=FindAnyObjectByType<WatchEngineControls>();workPipe=FindAnyObjectByType<WatchPipeRepair>();workPanel=FindAnyObjectByType<WatchPowerPanel>();workRadio=FindAnyObjectByType<WatchRadioConsole>();workBeacon=FindAnyObjectByType<WatchBeaconStation>();
   var parts=watch.GetComponent<M2StationParts>();workWrench=new GameObject("Owen non-grabbable working spanner");workWrench.transform.SetParent(actor.transform,false);
   parts.Part("Spanner handle",workWrench.transform,new Vector3(0,-.12f,0),new Vector3(.035f,.30f,.018f),parts.Metal);
   parts.Part("Spanner jaw",workWrench.transform,new Vector3(0,.055f,0),new Vector3(.075f,.07f,.02f),parts.Metal);workWrench.SetActive(false);ClearStationMemory();
  }
  void ClearStationMemory(){StopStationWork();clearanceRequested=clearanceOffered=false;lastWatchElapsed=0;}
  void StopStationWork(){
   working=workingPipe=false;workFor=0;workStep=KeeperStep.None;
   if(watch&&watch.State!=null&&watch.State.PipeWorkerActor==WatchState.CompanionActor)watch.State.PipeWorkerActor=0;
   if(workEngine&&carryingFuel){workEngine.FuelCan.AssistantHeld=false;if(!workEngine.FuelCan.Grab.isSelected){workEngine.FuelCan.transform.position=workEngine.transform.TransformPoint(new Vector3(.98f,.28f,1.16f));workEngine.FuelCan.OnNetworkReleased();}carryingFuel=false;}
  }
  bool RequestClearance(){if(!clearanceOffered||clearanceRequested)return false;clearanceRequested=true;nextReport=Time.unscaledTime+tuning.reportCooldown;Report(KeeperStrings.Get("companion.clearanceSent"));return true;}
  bool AtControl(KeeperStep step,Transform target,Vector3 standing){
   if(workStep!=step){workStep=step;workFor=0;nextPath=0;}working=false;workingPipe=false;statusKey="companion.work";
   if(!MoveTo(standing)||Vector3.Distance(Position+Vector3.up*1.2f,target.position)>tuning.reach+.25f)return false;
   agent.ResetPath();working=true;workHand=target.position;workFor+=Time.deltaTime;return true;
  }
  // Choose a feasible circuit set; never try to restore every switch on a scarce-power night.
  public static int WorkCircuitMask(WatchState s,PowerCircuit priority){
   int mask=0;float remaining=s.Grid.Capacity;
   void Add(PowerCircuit circuit){int bit=1<<(int)circuit;if((mask&bit)!=0)return;
    float cost=circuit==PowerCircuit.Pump?s.PowerTuning.pumpLoad:circuit==PowerCircuit.Beacon?s.PowerTuning.beaconLoad:circuit==PowerCircuit.Radio?s.PowerTuning.radioLoad:s.PowerTuning.lightsLoad;
    if(cost<=remaining+.0001f){mask|=bit;remaining-=cost;}}
   Add(priority);if(!s.Repaired)Add(PowerCircuit.Pump);if(!s.Radio.Completed)Add(PowerCircuit.Radio);
   Add(PowerCircuit.Beacon);Add(PowerCircuit.Lights);Add(PowerCircuit.Pump);return mask;
  }
  bool WorkPanelPlan(PowerCircuit priority){
   var s=watch.State;int wanted=WorkCircuitMask(s,priority);int circuit=-1;bool selected=false;
   // Shed load first, then restore the needed branch, then reset once it is safe.
   for(int i=0;i<4;i++)if(s.Grid.Selected((PowerCircuit)i)&&(wanted&(1<<i))==0){circuit=i;break;}
   if(circuit<0)for(int i=0;i<4;i++)if(!s.Grid.Selected((PowerCircuit)i)&&(wanted&(1<<i))!=0){circuit=i;selected=true;break;}
   bool reset=circuit<0&&s.Grid.Tripped;if(circuit<0&&!reset)return false;
   for(int i=0;i<4;i++)if(workPanel.Control(i).GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().isSelected){StopStationWork();return false;}
   if(workPanel.ResetControl.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().isSelected){StopStationWork();return false;}
   var key=reset?workPanel.ResetControl:workPanel.Control(circuit);
   var step=reset?KeeperStep.ResetBreaker:(KeeperStep)((int)(selected?KeeperStep.PumpOn:KeeperStep.PumpOff)+circuit);
   if(AtControl(step,key.transform,key.transform.position+workPanel.transform.forward*.65f)&&workFor>=tuning.controlSeconds){
    if(reset)s.ResetBreaker();else s.SetCircuit(circuit,selected);workFor=0;
   }return true;
  }
  bool WorkStation(){
   var s=watch.State;working=workingPipe=false;
   // Night 1 is the player's hands-on rehearsal; the helper supports rather than skipping it.
   if(s.Day==1||!s.Campaign||s.Recovery.AnyDown){StopStationWork();return false;}
   if(Duty==CrewDuty.Machinist){
    if(!s.Engine.Settled||s.FuelJobPending){
     if(workEngine.FuelCan.Grab.isSelected||workEngine.GetComponentInParent<WatchGenerator>().Starter.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().isSelected){StopStationWork();return false;}
     bool fuel=s.Engine.Fuel<s.Master.guidedFuelTarget;
     if(fuel){
      if(!carryingFuel){workStep=KeeperStep.Fuel;var can=workEngine.FuelCan;if(!MoveTo(can.transform.position)||Vector3.Distance(Position+Vector3.up*.7f,can.transform.position)>tuning.reach)return true;carryingFuel=true;can.AssistantHeld=true;}
      if(AtControl(KeeperStep.Fuel,workEngine.Filler,workEngine.Filler.position+workEngine.transform.forward*.65f)){
       var can=workEngine.FuelCan;can.transform.rotation=workEngine.transform.rotation*Quaternion.Euler(0,0,115);can.transform.position=workEngine.Filler.position+Vector3.up*.12f-(can.Tip.position-can.transform.position);
       workHand=can.transform.TransformPoint(new Vector3(0,.30f,0));s.PourFuel(Time.deltaTime);
      }else {var can=workEngine.FuelCan;can.transform.position=Position+Vector3.up*.65f+actor.transform.right*.30f;}
     }else{
      if(carryingFuel){workEngine.FuelCan.AssistantHeld=false;workEngine.FuelCan.transform.position=workEngine.transform.TransformPoint(new Vector3(.98f,.28f,1.16f));workEngine.FuelCan.OnNetworkReleased();carryingFuel=false;}
      var starter=workEngine.GetComponentInParent<WatchGenerator>().Starter;
      if(AtControl(KeeperStep.Starter,starter.transform,starter.transform.position+workEngine.transform.forward*.65f)){
       workHand+=workEngine.transform.forward*(Mathf.Sin(workFor*5)*.12f);
       if(workFor>=tuning.controlSeconds&&s.StarterInWindow){s.StarterPulled();workFor=0;}
      }
     }return true;
    }
    if(s.Grid.Tripped&&WorkPanelPlan(s.ShipGuidanceDue||s.Radio.Completed?PowerCircuit.Beacon:!s.Repaired?PowerCircuit.Pump:PowerCircuit.Radio))return true;
    if(!s.Repaired&&!workPipe.wrench.isSelected){
     var bolt=workPipe.bolts[Mathf.Max(0,s.Pipe.ActiveBolt)];var normal=Vector3.ProjectOnPlane(bolt.up,Vector3.up).normalized;if(normal.sqrMagnitude<.1f)normal=Vector3.left;
     if(AtControl(KeeperStep.TightenBolt,bolt,workPipe.ActivePosition+normal*.75f)){
      s.PipeWorkerActor=WatchState.CompanionActor;workingPipe=true;workHand=workPipe.ActivePosition;
      s.Pipe.Observe(Time.deltaTime,(s.Pipe.NeedsReturn?-1:1)*tuning.pipeDegreesPerSecond*Time.deltaTime,true,true,true);
     }return true;
    }
    if(s.PipeWorkerActor==WatchState.CompanionActor)s.PipeWorkerActor=0;
    if(WorkPanelPlan(s.ShipGuidanceDue||s.Radio.Completed?PowerCircuit.Beacon:PowerCircuit.Radio))return true;
   }else if(s.Engine.Settled&&s.Repaired){
    if(s.HasRadioRelay&&learnedCode.Length==0){StopStationWork();return false;}
    if(!s.Radio.Completed){
     if(!s.HasPower(PowerCircuit.Radio)){
      if(WorkPanelPlan(PowerCircuit.Radio))return true;
     }
     var r=s.Radio;var step=r.Step==0?KeeperStep.RadioPower:r.Step==1?KeeperStep.RadioTune:r.Step==2?KeeperStep.RadioCode:KeeperStep.RadioTransmit;
     var target=r.Step==3?workRadio.microphone.transform:workRadio.controls[0].transform;
     var stand=r.Step==3?workRadio.transform.TransformPoint(new Vector3(.35f,0,.56f)):workRadio.transform.TransformPoint(new Vector3(-.4f,0,1));
     if(AtControl(step,target,stand)){
      // Console root is at floor height: use its working face for the arm.
      if(r.Step<3)workHand=workRadio.transform.TransformPoint(new Vector3(-.3f,1.3f,.4f));
      if(workFor>=tuning.controlSeconds){
       if(r.Step==0)r.ToggleMain();else if(r.Step==1)r.Tune(r.Program.targetMHz);else if(r.Step==2)r.PressCode(r.CodeSymbol(r.CodeProgress));
       else if(!clearanceOffered){clearanceOffered=true;Report(KeeperStrings.Get("companion.clearanceReady"));}
       workFor=0;
      }
     }return true;
    }
    if(s.ShipGuidanceDue){
     if(!s.HasPower(PowerCircuit.Beacon)){
      if(WorkPanelPlan(PowerCircuit.Beacon))return true;
     }
     var wheel=workBeacon.Wheel.transform;
     if(AtControl(KeeperStep.KeepLight,wheel,wheel.position+workBeacon.transform.forward*.65f)){
      s.IgniteBeacon(true);s.WipeBeacon(Time.deltaTime*.15f);
      s.AlignBeacon(Mathf.MoveTowards(s.Beacon.Alignment,s.ShipWheelTarget,tuning.beaconTurnsPerSecond*Time.deltaTime));
     }return true;
    }
   }
   StopStationWork();return false;
  }
 }
}
