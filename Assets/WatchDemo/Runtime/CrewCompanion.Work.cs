using UnityEngine;using LighthouseKeepers.Core;using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class CrewCompanion {
  static readonly PowerCircuit[] workCircuits={PowerCircuit.Pump,PowerCircuit.Radio,PowerCircuit.Beacon,PowerCircuit.Lights};
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
    if(!s.Repaired&&!workPipe.wrench.isSelected){
     var bolt=workPipe.bolts[Mathf.Max(0,s.Pipe.ActiveBolt)];var normal=Vector3.ProjectOnPlane(bolt.up,Vector3.up).normalized;if(normal.sqrMagnitude<.1f)normal=Vector3.left;
     if(AtControl(KeeperStep.TightenBolt,bolt,workPipe.ActivePosition+normal*.75f)){
      s.PipeWorkerActor=WatchState.CompanionActor;workingPipe=true;workHand=workPipe.ActivePosition;
      s.Pipe.Observe(Time.deltaTime,(s.Pipe.NeedsReturn?-1:1)*tuning.pipeDegreesPerSecond*Time.deltaTime,true,true,true);
     }return true;
    }
    if(s.PipeWorkerActor==WatchState.CompanionActor)s.PipeWorkerActor=0;
    foreach(var circuit in workCircuits)if(!s.HasPower(circuit)){
     var control=workPanel.Control((int)circuit);var step=(KeeperStep)((int)KeeperStep.PumpOn+(int)circuit);
     if(AtControl(step,control.transform,control.transform.position+workPanel.transform.forward*.65f)&&workFor>=tuning.controlSeconds){s.SetCircuit((int)circuit,true);if(s.Grid.Tripped)s.ResetBreaker();workFor=0;}return true;
    }
   }else if(s.Engine.Settled&&s.Repaired){
    if(s.HasRadioRelay&&learnedCode.Length==0){StopStationWork();return false;}
    if(!s.Radio.Completed){
     if(!s.HasPower(PowerCircuit.Radio)){
      var key=workPanel.Control((int)PowerCircuit.Radio);if(AtControl(KeeperStep.RadioOn,key.transform,key.transform.position+workPanel.transform.forward*.65f)&&workFor>=tuning.controlSeconds){s.SetCircuit(2,true);if(s.Grid.Tripped)s.ResetBreaker();workFor=0;}return true;
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
      var key=workPanel.Control(1);if(AtControl(KeeperStep.BeaconOn,key.transform,key.transform.position+workPanel.transform.forward*.65f)&&workFor>=tuning.controlSeconds){s.SetCircuit(1,true);if(s.Grid.Tripped)s.ResetBreaker();workFor=0;}return true;
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
