using LighthouseKeepers.Lobby;
using UnityEngine;
using UnityEngine.XR;
namespace LighthouseKeepers.WatchDemo
{
    // Crew-only adapter. Solo station, clock, flood and sleep implementations remain in their original paths.
    public sealed partial class WatchDemoController
    {
        WatchPressureValve pressureValve;
        public bool PressureCommand(int actor,bool held)=>state.Pressure!=null&&!state.ReadOnly&&!sleeping&&!state.Failed&&!state.Repaired&&(state.Pressure.CrewRequired?state.Pressure.Hold(actor,held):held&&state.Pressure.Clamp());
        public void PressureWorker(int actor){if(!state.ReadOnly)state.PipeWorkerActor=actor;}
        public void PressureActorLeft(int actor){state.Pressure?.Hold(actor,false);}
        public int Revision {get;private set;}
        public bool CanSleep=>state!=null&&!sleeping&&!state.Failed&&state.Complete&&state.HasNextDay;
        public Vector3 BedPosition=>bed.position+Vector3.up*1.3f;
        int crewBeds,crewKeepers;
        string crewNotice="";
        float currentFade,replicaFadeTarget;
        public Component Target(CoopAction action,float value)
        {
            if(action==CoopAction.HandheldRadio)return value==(int)value&&value>=0&&value<2&&crewRadios[(int)value]?crewRadios[(int)value].Grab:null;
            if(action==CoopAction.PressureValve)return pressureValve?pressureValve.Grip:null;
            if(action==CoopAction.PAReplay)return value==(int)value&&immersionWorld&&immersionWorld.Narrator?immersionWorld.Narrator.ReplayButton((int)value):null;
            if(action==CoopAction.GridCircuit)return powerPanel?powerPanel.Control((int)value):null;
            if(action==CoopAction.GridReset)return powerPanel?powerPanel.ResetControl:null;
            if(action==CoopAction.BeaconIgnition)return beaconStation?beaconStation.Ignition:null;
            if(action==CoopAction.BeaconAlign)return beaconStation?beaconStation.Wheel:null;
            if(action==CoopAction.EnginePrime)return engineControls?engineControls.Primer:null;
            if(action==CoopAction.EngineChoke)return engineControls?engineControls.Choke:null;
            if(action==CoopAction.Starter)return generator.Starter;
            if(action==CoopAction.Microphone)return radio.microphone;
            if(action==CoopAction.WrenchClutch)return wrench;
            foreach(var control in radio.controls)
                if(action==CoopAction.RadioPower&&control.mainPower||action==CoopAction.RadioTune&&control.frequency||
                    action==CoopAction.RadioCode&&value>=0&&value<=3&&value==(int)value&&control.codeColour==(int)value)return control;
            return null;
        }
        public int ApplyCommand(CoopAction action,float value,bool pressed)
        {
            if(state.ReadOnly||sleeping)return -1;
            switch(action)
            {
                case CoopAction.StatusRequest:
                case CoopAction.PAReplay:return immersionWorld&&immersionWorld.Narrator&&immersionWorld.Narrator.RequestReplay()?1:-1;
                case CoopAction.Bed:
                    if(state.Immersion&&!state.Failed&&immersionWorld&&immersionWorld.Narrator){immersionWorld.Narrator.EarlyBed();return 1;}
                    return -1;
                case CoopAction.GridCircuit:
                    if(!float.IsFinite(value)||value!=(int)value||value<0||value>3)return -1;
                    return state.SetCircuit((int)value,pressed)?1:-1;
                case CoopAction.GridReset:return state.ResetBreaker()?1:-1;
                case CoopAction.BeaconIgnition:return state.IgniteBeacon(pressed)?1:-1;
                case CoopAction.BeaconAlign:return state.AlignBeacon(value)?1:-1;
                case CoopAction.EnginePrime:return state.PrimeEngine()?1:-1;
                case CoopAction.EngineChoke:return state.SetChoke(pressed)?1:-1;
                case CoopAction.Starter:return generator.ApplyPull()?1:-1;
                case CoopAction.RadioPower:return !state.Failed&&radio.ToggleMain()?1:-1;
                case CoopAction.RadioTune:
                    if(state.Failed||!float.IsFinite(value)||value<0||value>1)return -1;
                    radio.Tune(value);return 1;
                case CoopAction.RadioCode:
                    if(state.Failed||value<0||value>3||value!=(int)value)return -1;
                    int result=state.Radio.PressCode((int)value);radio.CodeResult(result);return result;
                case CoopAction.Retry:return RetryDay()?1:-1;
                case CoopAction.Replay:return ReplayFromDayOne()?1:-1;
                default:return -1;
            }
        }
        public void CommandResult(CoopAction action,int result)
        {if(state.RemoteReplica&&action==CoopAction.RadioCode)radio.CodeResult(result);}
        public void BeginSharedSleep(){if(CanSleep)StartCoroutine(Sleep());}
        public void CrewStatus(int beds,int keepers,string notice)
        {
            if(crewBeds==beds&&crewKeepers==keepers&&crewNotice==notice)return;
            crewBeds=beds;crewKeepers=keepers;crewNotice=notice;state.Pressure?.SetCrew(keepers>1);RefreshBoard();
        }
        public void ContinueAlone(string notice)
        {
            coop=null;state.Coop=null;state.SetRemoteReplica(false);state.SetNetworkRehearsal(false);
            state.Radio.SetMicrophone(false,false);sleeping=false;SetFade(0);
            CrewStatus(0,1,notice);shownSecond=-1;
        }
        void UpdateReplicaInput()
        {
            flood.Paused=true;SetFade(Mathf.MoveTowards(currentFade,replicaFadeTarget,Time.deltaTime*1.4f));
            if(sleeping)return;
            var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            left.TryGetFeatureValue(CommonUsages.gripButton,out bool lg);right.TryGetFeatureValue(CommonUsages.gripButton,out bool rg);
            left.TryGetFeatureValue(CommonUsages.triggerButton,out bool lt);right.TryGetFeatureValue(CommonUsages.triggerButton,out bool rt);
            bool grip=lg||rg,trigger=lt||rt,near=Vector3.Distance(head.position,BedPosition)<1.8f;
            bedPrompt.text=near?(CanSleep?SleepPrompt+LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoCoop.1"):LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoCoop.2")):"";
            if(near){var direction=bedPrompt.transform.position-head.position;direction.y=0;if(direction.sqrMagnitude>.01f)bedPrompt.transform.rotation=Quaternion.LookRotation(direction);}
            if(!state.Immersion&&(!menu||!menu.IsOpen)&&state.Failed&&trigger&&!previousTrigger)RetryDay();
            else if((!menu||!menu.IsOpen)&&near&&grip&&!previousGrip)TrySleep();
            previousGrip=grip;previousTrigger=trigger;
        }
    }
}
