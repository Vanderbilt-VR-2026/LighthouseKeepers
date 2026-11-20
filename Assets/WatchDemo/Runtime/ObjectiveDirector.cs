using System;
using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
    public enum KeeperStep { None, GrabWrench, BoltA, BoltB, BoltC, Fuel, Prime, CloseChoke, Starter, OpenChoke, PumpOff, BeaconOff, RadioOff, LightsOff, PumpOn, BeaconOn, RadioOn, LightsOn, ResetBreaker, RadioPower, RadioTune, RadioCode, RadioTransmit, BeaconLight, BeaconAlign, BeaconWipe, Sleep, Complete, Lost, CheckFuel, HoldValve, ClampValve, TightenBolt, KeepLight, ReadCode, AwaitRescue, ReviveKeeper, GatherF2, WatchPartner, RescueF2, AwaitRescueF2, ChallengeCall, JudgeCall, HelpPartner, ReadyShip, GrabWalkie, GrabLantern, LanternWick, HoldStarterAssist }
    // Sole instruction policy. The host assigns jobs; presentation never invents a next action.
    public sealed class ObjectiveDirector
    {
        readonly WatchState state;
        static readonly PowerCircuit[] shedOrder={PowerCircuit.Pump,PowerCircuit.Radio,PowerCircuit.Lights,PowerCircuit.Beacon};
        int shipGuideActor,guideShip=-1;
        int actorA,actorB,jobA=-1,jobB=-1;float assignmentUntil;
        public Func<string,Vector3> StationPosition;
        public Func<int,CrewDuty> DutyForActor;
        public Func<KeeperStep> CompanionStep;
        CrewDuty Duty(int actor)=>DutyForActor?.Invoke(actor)??CrewDuty.None;
        public KeeperStep First {get;private set;}
        public KeeperStep Second {get;private set;}
        public int FirstActor=>actorA;
        public int SecondActor=>actorB;
        public ObjectiveDirector(WatchState owner){state=owner;}
        public KeeperStep ForActor(int actor)=>actorB!=0&&actor==actorB?Second:First;
        public KeeperStep Partner(int actor)=>actorB==0?KeeperStep.None:actor==actorB?First:Second;
        public static bool Valid(int step)=>step>=0&&step<=(int)KeeperStep.HoldStarterAssist;
        public void Apply(int first,int second,int a,int b)
        {if(!state.ReadOnly)return;First=(KeeperStep)first;Second=(KeeperStep)second;actorA=a;actorB=b;}
        public void Reset(){First=Second=KeeperStep.None;actorA=actorB=shipGuideActor=0;guideShip=-1;jobA=jobB=-1;assignmentUntil=0;}
        public void Tick(float now,int firstActor,Vector3 firstHead,int secondActor,Vector3 secondHead,bool wrenchHeld)
        {
            // The helper is shown as a partner, but is not a second human assignment candidate.
            // Restore that input before comparing rosters so movement keeps the normal assignment hold.
            if(!state.ReadOnly&&state.ScriptedPartner&&actorB==WatchState.CompanionActor)actorB=0;
            TickCore(now,firstActor,firstHead,secondActor,secondHead,wrenchHeld);
            // Starting the engine needs two different human keepers. A ship or
            // pipe assignment can also resolve to Starter when power is out;
            // don't leave the other keeper holding a valve or another starter.
            if(!state.ReadOnly&&!state.ScriptedPartner&&actorB!=0&&actorA!=actorB&&state.RequiresStartAssist){
                bool first=First==KeeperStep.Starter,second=Second==KeeperStep.Starter;
                if(first&&second){
                    bool firstWorks=Duty(actorA)==CrewDuty.Machinist||Duty(actorB)!=CrewDuty.Machinist;
                    First=firstWorks?KeeperStep.Starter:KeeperStep.HoldStarterAssist;
                    Second=firstWorks?KeeperStep.HoldStarterAssist:KeeperStep.Starter;
                }else if(first)Second=KeeperStep.HoldStarterAssist;
                else if(second)First=KeeperStep.HoldStarterAssist;
            }
            if(!state.ReadOnly&&state.ScriptedPartner){
                actorB=WatchState.CompanionActor;
                Second=state.Recovery.LessonActive?(state.Recovery.Lesson==4?KeeperStep.AwaitRescueF2:KeeperStep.GatherF2):state.Recovery.AnyDown&&!state.Failed?KeeperStep.ReviveKeeper:state.Failed||state.Complete?KeeperStep.None:state.Repaired?state.Radio.Step==2?KeeperStep.ReadCode:KeeperStep.None:state.RequiresCrewValve?KeeperStep.HoldValve:KeeperStep.None;
                if(!state.Recovery.LessonActive&&!state.Recovery.AnyDown&&!state.Failed&&!state.Complete){
                    var work=CompanionStep?.Invoke()??KeeperStep.None;
                    if(work!=KeeperStep.None){Second=work;
                        if(work==KeeperStep.TightenBolt)First=state.RequiresCrewValve?KeeperStep.HoldValve:KeeperStep.HelpPartner;
                        else if(work==KeeperStep.HoldStarterAssist)First=KeeperStep.Starter;
                        else if(work==KeeperStep.Starter&&state.RequiresStartAssist)First=KeeperStep.HoldStarterAssist;
                        else if(work==KeeperStep.RadioTransmit)First=KeeperStep.ReadyShip;
                        else if(ObjectiveDirector.Job(First)==ObjectiveDirector.Job(work)||work==KeeperStep.Fuel||work==KeeperStep.Starter)First=KeeperStep.HelpPartner;
                    }
                }
            }
        }
        void TickCore(float now,int firstActor,Vector3 firstHead,int secondActor,Vector3 secondHead,bool wrenchHeld)
        {
            if(state.ReadOnly)return;
            bool roster=actorA!=firstActor||actorB!=secondActor;actorA=firstActor;actorB=secondActor;
            if(state.Failed){First=Second=KeeperStep.Lost;return;}
            if(state.Recovery.LessonActive){
                var step=state.Recovery.Lesson==1?KeeperStep.GatherF2:state.Recovery.Lesson==4?KeeperStep.RescueF2:KeeperStep.WatchPartner;
                First=state.Recovery.Downed((int)Duty(actorA))?KeeperStep.AwaitRescueF2:step;Second=actorB==0?KeeperStep.None:state.Recovery.Downed((int)Duty(actorB))?KeeperStep.AwaitRescueF2:step;return;
            }
            if(state.Recovery.AnyDown){First=state.Recovery.Downed((int)Duty(actorA))?KeeperStep.AwaitRescue:KeeperStep.ReviveKeeper;Second=actorB==0?KeeperStep.None:state.Recovery.Downed((int)Duty(actorB))?KeeperStep.AwaitRescue:KeeperStep.ReviveKeeper;return;}
            if(state.Complete){First=state.HasNextDay?KeeperStep.Sleep:KeeperStep.Complete;Second=actorB!=0?First:KeeperStep.None;return;}
            if(state.Scenario?.radioClearance==true){OrderedCampaign(wrenchHeld);return;}
            if(state.ShipGuidanceDue){
                var step=BeaconStep();if(step==KeeperStep.None)step=KeeperStep.KeepLight;
                // Keep the wheel assignment while its keeper walks upstairs. Re-picking by current
                // height every frame swaps both jobs mid-stair and can leave the wheel unattended.
                if(guideShip!=state.AimingShip||roster||shipGuideActor!=actorA&&shipGuideActor!=actorB){guideShip=state.AimingShip;shipGuideActor=actorB==0||Duty(actorA)==CrewDuty.Lightkeeper?actorA:Duty(actorB)==CrewDuty.Lightkeeper?actorB:firstHead.y>=secondHead.y?actorA:actorB;}
                bool firstNear=shipGuideActor==actorA;
                var other=state.Authenticity.NeedsDecision?AuthStep:OtherShipJob(wrenchHeld);
                First=firstNear?step:other;Second=actorB==0?KeeperStep.None:firstNear?other:step;return;
            }
            if(actorB!=0&&state.Pressure?.CrewRequired==true&&!state.Repaired&&(state.Power==PowerState.On&&(!state.Campaign||state.Engine==null||state.Engine.Settled)||state.IsPending(WatchState.GeneratorStation)))
            {
                int holder=state.Pressure.Holder;
                if(holder!=actorA&&holder!=actorB||state.Campaign&&holder==state.PipeWorkerActor){int worker=state.PipeWorkerActor;var valve=StationPosition!=null?StationPosition("STN_PRESSURE_VALVE"):new Vector3(2.7f,1,2.5f);holder=worker==actorA?actorB:worker==actorB?actorA:Duty(actorA)==CrewDuty.Lightkeeper?actorA:Duty(actorB)==CrewDuty.Lightkeeper?actorB:Vector3.Distance(firstHead,valve)<=Vector3.Distance(secondHead,valve)?actorA:actorB;}
                First=holder==actorA?KeeperStep.HoldValve:PipeStep(wrenchHeld);Second=holder==actorB?KeeperStep.HoldValve:PipeStep(wrenchHeld);return;
            }
            if(state.Authenticity.NeedsDecision&&state.Repaired&&state.Power==PowerState.On){
                bool firstOperates=actorB==0||Duty(actorA)==CrewDuty.Lightkeeper;
                First=firstOperates?AuthStep:KeeperStep.ReadCode;Second=actorB==0?KeeperStep.None:firstOperates?KeeperStep.ReadCode:AuthStep;return;
            }
            // Once the engine and pipe are safe, two keepers have different halves of this task.
            // A ship window still takes priority above; no clue answer is put in the operator HUD.
            if(actorB!=0&&state.RadioRelay&&state.Repaired&&state.Power==PowerState.On&&state.Engine?.Settled!=false&&StepForStation(WatchState.RadioStation,true)==KeeperStep.RadioCode)
            {
                bool firstOperates=Duty(actorA)==CrewDuty.Lightkeeper||Duty(actorB)!=CrewDuty.Lightkeeper;
                First=firstOperates?KeeperStep.RadioCode:KeeperStep.ReadCode;Second=firstOperates?KeeperStep.ReadCode:KeeperStep.RadioCode;return;
            }
            if(roster||now>=assignmentUntil||!Available(jobA)||actorB!=0&&!Available(jobB))
            {
                float best=float.PositiveInfinity;int a=-1,b=-1;
                for(int i=0;i<state.Definition.tasks.Length;i++)if(Available(i))
                {
                    if(actorB==0){float cost=Cost(i,firstHead,actorA);if(cost<best){best=cost;a=i;}continue;}
                    for(int j=0;j<state.Definition.tasks.Length;j++)if(Available(j))
                    {float cost=Cost(i,firstHead,actorA)+Cost(j,secondHead,actorB)+(i==j?10000:0);if(cost<best){best=cost;a=i;b=j;}}
                }
                jobA=a;jobB=b;assignmentUntil=now+state.Master.assignmentSeconds;
            }
            First=jobA<0&&state.GeneratorTutorial&&!state.NightTwoOutage?KeeperStep.CheckFuel:Step(jobA,wrenchHeld);Second=actorB!=0?Step(jobB,wrenchHeld):KeeperStep.None;
            if(First==KeeperStep.None&&state.Ships!=null&&!state.Ships.Finished)First=KeeperStep.KeepLight;
            if(actorB!=0&&Second==KeeperStep.None&&state.Ships!=null&&!state.Ships.Finished)Second=KeeperStep.KeepLight;
        }
        // Preparation has one stable dependency chain. Walking past another floor must
        // not replace the task. A cleared ship takes priority once the engine is healthy.
        void OrderedCampaign(bool held)
        {
            Second=KeeperStep.None;
            if(state.Power==PowerState.Off||state.Engine?.Settled==false||state.FuelJobPending){First=EngineStep();if(actorB!=0){Second=First;if(First==KeeperStep.Starter&&state.RequiresStartAssist){bool firstWorks=Duty(actorA)==CrewDuty.Machinist||Duty(actorB)!=CrewDuty.Machinist;First=firstWorks?KeeperStep.Starter:KeeperStep.HoldStarterAssist;Second=firstWorks?KeeperStep.HoldStarterAssist:KeeperStep.Starter;}}return;}
            if(state.ShipGuidanceDue){
                var beam=BeaconStep();if(beam==KeeperStep.None)beam=KeeperStep.KeepLight;
                bool first=actorB==0||Duty(actorA)==CrewDuty.Lightkeeper||Duty(actorB)!=CrewDuty.Lightkeeper;
                First=first?beam:OtherShipJob(held);Second=actorB==0?KeeperStep.None:first?OtherShipJob(held):beam;return;
            }
            if(!state.Repaired){
                First=PipeStep(held);
                if(actorB!=0&&state.Pressure?.CrewRequired==true){
                    int holder=state.Pressure.Holder,worker=state.PipeWorkerActor;
                    if(holder!=actorA&&holder!=actorB||holder==worker)
                        holder=worker==actorA?actorB:worker==actorB?actorA:Duty(actorA)==CrewDuty.Lightkeeper?actorA:actorB;
                    First=holder==actorA?KeeperStep.HoldValve:PipeStep(held);
                    Second=holder==actorB?KeeperStep.HoldValve:PipeStep(held);
                }else if(actorB!=0)Second=First;
                return;
            }
            if(!state.HasPower(PowerCircuit.Pump)){First=BreakerStep(PowerCircuit.Pump);if(actorB!=0)Second=First;return;}
            if(state.Tutorial&&state.IsPending(WatchState.GeneratorStation)){First=KeeperStep.CheckFuel;if(actorB!=0)Second=First;return;}
            if(state.Tutorial&&(!state.IsDone(WatchState.BeaconStation)||state.IsPending(WatchState.BeaconStation))){First=BeaconStep();if(First==KeeperStep.None)First=KeeperStep.BeaconLight;if(actorB!=0)Second=First;return;}
            if(state.Tutorial&&state.IsPending(WatchState.RadioStation)){First=KeeperStep.RadioPower;if(actorB!=0)Second=First;return;}
            if(state.Authenticity.NeedsDecision){
                bool first=actorB==0||Duty(actorA)==CrewDuty.Lightkeeper||Duty(actorB)!=CrewDuty.Lightkeeper;
                First=first?AuthStep:KeeperStep.ReadCode;Second=actorB==0?KeeperStep.None:first?KeeperStep.ReadCode:AuthStep;return;
            }
            if(!state.Radio.Completed&&!state.IsPending(WatchState.RadioStation)){
                var step=StepForStation(WatchState.RadioStation,held);
                bool first=actorB==0||Duty(actorA)==CrewDuty.Lightkeeper||Duty(actorB)!=CrewDuty.Lightkeeper;
                var partner=state.RadioRelay?KeeperStep.ReadCode:step;
                First=first?step:partner;Second=actorB==0?KeeperStep.None:first?partner:step;return;
            }
            First=BeaconStep();if(First==KeeperStep.None)First=KeeperStep.KeepLight;
            if(actorB!=0)Second=First;
        }
        KeeperStep AuthStep=>state.Authenticity.Stage==2?KeeperStep.JudgeCall:KeeperStep.ChallengeCall;
        KeeperStep OtherShipJob(bool held){for(int i=0;i<state.Definition.tasks.Length;i++)if(Available(i)&&state.Definition.tasks[i].stationId!=WatchState.BeaconStation){var step=Step(i,held);return state.RadioRelay&&step==KeeperStep.RadioCode?KeeperStep.ReadCode:step;}return KeeperStep.KeepLight;}
        bool Available(int i)=>i>=0&&i<state.Definition.tasks.Length&&!state.IsDone(state.Definition.tasks[i].stationId)&&!state.IsPending(state.Definition.tasks[i].stationId)&&(state.Power==PowerState.On||state.Definition.tasks[i].stationId==WatchState.PipeStation||state.Definition.tasks[i].stationId==WatchState.GeneratorStation);
        float Cost(int i,Vector3 head,int actor)
        {
            string id=state.Definition.tasks[i].stationId;float floor=id==WatchState.PipeStation?1:id==WatchState.GeneratorStation?2:4;
            var point=StationPosition!=null?StationPosition(id):new Vector3(0,(floor-1)*3.3f+1.6f,0);
            float cost=Vector3.Distance(head,point);
            var duty=Duty(actor);
            if(duty==CrewDuty.Machinist&&(id==WatchState.PipeStation||id==WatchState.GeneratorStation)||duty==CrewDuty.Lightkeeper&&(id==WatchState.RadioStation||id==WatchState.BeaconStation))cost-=18;
            if(duty==CrewDuty.Machinist&&id==WatchState.GeneratorStation)cost-=8;
            if(id==WatchState.GeneratorStation&&(state.Power==PowerState.Off||state.Campaign&&state.Engine!=null&&(!state.Engine.Settled||state.FuelJobPending)))cost-=45;
            if(id==WatchState.BeaconStation&&!state.BeaconShining)cost-=state.BeaconDarkRemaining<30?65:15;
            if(id==WatchState.PipeStation)cost-=12;
            return cost;
        }
        KeeperStep EngineStep()
        {
            var e=state.Engine;
            if(e==null)return KeeperStep.Starter;
            if(state.Campaign&&state.FuelJobPending)return KeeperStep.Fuel;
            if(e.Running)return e.ChokeClosed?KeeperStep.OpenChoke:KeeperStep.None;
            if(!e.EnoughFuel||state.Master.onboarding&&e.Fuel<state.Master.guidedFuelTarget)return KeeperStep.Fuel;
            if(state.TimedStarter)return KeeperStep.Starter;
            if(!e.Primed)return KeeperStep.Prime;
            if(!e.ChokeClosed)return KeeperStep.CloseChoke;
            return KeeperStep.Starter;
        }
        public KeeperStep StepForStation(string id,bool held)
        {
            if(state.Failed)return KeeperStep.Lost;
            if(state.IsDone(id)||state.IsPending(id))return KeeperStep.None;
            if(id==WatchState.PipeStation)return state.Pressure?.CrewRequired==true&&!state.ScriptedPartner?KeeperStep.HoldValve:PipeStep(held);
            if(id==WatchState.GeneratorStation)return EngineStep();
            if(id==WatchState.RadioStation&&state.Connected&&state.Connected.dependencies&&state.Scenario?.radioClearance!=true&&!state.BeaconShining)return BeaconStep();
            var circuit=id==WatchState.RadioStation?PowerCircuit.Radio:PowerCircuit.Beacon;
            if(state.Power==PowerState.Off||state.Engine?.ChokeClosed==true)return EngineStep();
            if(!state.HasPower(circuit))return BreakerStep(circuit);
            if(id==WatchState.RadioStation)
                return state.Radio.Completed?KeeperStep.None:state.Radio.Step==0?KeeperStep.RadioPower:state.Radio.Step==1?KeeperStep.RadioTune:state.Radio.Step==2?KeeperStep.RadioCode:KeeperStep.RadioTransmit;
            return BeaconStep();
        }
        KeeperStep BeaconStep()
        {
            if(state.Power==PowerState.Off||state.Engine?.ChokeClosed==true)return EngineStep();
            if(!state.HasPower(PowerCircuit.Beacon))return BreakerStep(PowerCircuit.Beacon);
            var beacon=state.Beacon;
            return beacon==null?KeeperStep.None:!beacon.Ignited?KeeperStep.BeaconLight:!state.BeamTrackingMode&&!beacon.Aligned?KeeperStep.BeaconAlign:KeeperStep.None;
        }
        KeeperStep PipeStep(bool held)
        {if(!held)return KeeperStep.GrabWrench;if(state.Pressure?.CrewRequired==true)return KeeperStep.TightenBolt;return state.Pipe.ActiveBolt>=0?(KeeperStep)((int)KeeperStep.BoltA+state.Pipe.ActiveBolt):KeeperStep.None;}
        KeeperStep Step(int job,bool held)=>Available(job)?StepForStation(state.Definition.tasks[job].stationId,held):KeeperStep.None;
        KeeperStep BreakerStep(PowerCircuit desired)
        {
            var g=state.Grid;if(g==null)return EngineStep();
            float required=g.Selected(desired)?0:desired==PowerCircuit.Radio?state.PowerTuning.radioLoad:desired==PowerCircuit.Pump?state.PowerTuning.pumpLoad:desired==PowerCircuit.Lights?state.PowerTuning.lightsLoad:state.PowerTuning.beaconLoad;
            if(g.Demand+required>g.Capacity+.001f)
            {
                foreach(var c in shedOrder)
                    if(c!=desired&&g.Selected(c))return (KeeperStep)((int)KeeperStep.PumpOff+(int)c);
            }
            if(g.Tripped)return KeeperStep.ResetBreaker;
            return (KeeperStep)((int)KeeperStep.PumpOn+(int)desired);
        }
        public static string Job(KeeperStep step)
        {
            if(step>=KeeperStep.GrabWalkie&&step<=KeeperStep.LanternWick)return "KIT";
            if(step>=KeeperStep.GatherF2&&step<=KeeperStep.AwaitRescueF2)return "LESSON";
            if(step==KeeperStep.AwaitRescue||step==KeeperStep.ReviveKeeper)return "RESCUE";
            if(step==KeeperStep.HoldValve||step==KeeperStep.ClampValve||step==KeeperStep.TightenBolt||step>=KeeperStep.GrabWrench&&step<=KeeperStep.BoltC)return WatchState.PipeStation;
            if(step==KeeperStep.HoldStarterAssist||step==KeeperStep.CheckFuel||step>=KeeperStep.Fuel&&step<=KeeperStep.OpenChoke)return WatchState.GeneratorStation;
            if(step>=KeeperStep.PumpOff&&step<=KeeperStep.ResetBreaker)return "BREAKER";
            if(step==KeeperStep.ChallengeCall||step==KeeperStep.JudgeCall||step==KeeperStep.ReadCode||step>=KeeperStep.RadioPower&&step<=KeeperStep.RadioTransmit)return WatchState.RadioStation;
            if(step==KeeperStep.KeepLight)return "SHIP";
            if(step>=KeeperStep.BeaconLight&&step<=KeeperStep.BeaconWipe)return WatchState.BeaconStation;
            return step==KeeperStep.Sleep?"BED":"";
        }
        public static int Floor(KeeperStep step){if(step==KeeperStep.HelpPartner||step==KeeperStep.ReadyShip)return -1;if(step>=KeeperStep.GatherF2&&step<=KeeperStep.AwaitRescueF2)return 2;if(step==KeeperStep.ReadCode||step>=KeeperStep.GrabWalkie&&step<=KeeperStep.LanternWick)return 0;string id=Job(step);return step==KeeperStep.AwaitRescue||step==KeeperStep.ReviveKeeper?1:id==WatchState.PipeStation?1:id==WatchState.GeneratorStation||id=="BREAKER"?2:id=="BED"?3:4;}
        public static string Location(KeeperStep step)
        {
            string key=step==KeeperStep.HelpPartner?"where.partner":step==KeeperStep.ReadyShip?"where.walkie":step>=KeeperStep.GrabWalkie&&step<=KeeperStep.LanternWick?"where.kit":step==KeeperStep.ReadCode?"where.telegram":step==KeeperStep.GrabWrench?"where.workbench":step==KeeperStep.Fuel?"where.fuel":"where."+Job(step);
            return KeeperStrings.Has(key)?KeeperStrings.Get(key):"";
        }
        public static string Button(KeeperStep step)=>step==KeeperStep.HelpPartner?"":step==KeeperStep.ReadyShip||step==KeeperStep.LanternWick?"TRIGGER":step==KeeperStep.GatherF2||step==KeeperStep.WatchPartner||step==KeeperStep.AwaitRescueF2||step==KeeperStep.AwaitRescue||step==KeeperStep.None||step==KeeperStep.Lost||step==KeeperStep.Complete||step==KeeperStep.CheckFuel||step==KeeperStep.ReadCode||step==KeeperStep.KeepLight?"":step==KeeperStep.RadioTransmit?"TRIGGER":"GRIP";
        public static int Hint(KeeperStep step){string id=Job(step);return step==KeeperStep.AwaitRescue||step==KeeperStep.ReviveKeeper?1:id==WatchState.PipeStation?7:id==WatchState.GeneratorStation?8:id=="BREAKER"?9:id==WatchState.BeaconStation?10:id==WatchState.RadioStation?11:id=="BED"?12:0;}
        public string StationInstruction(string station) {
            if(First!=KeeperStep.ReadCode&&Job(First)==station)return Instruction(First);
            if(Second!=KeeperStep.ReadCode&&Job(Second)==station)return Instruction(Second);
            return "";
        }
        public string Instruction(KeeperStep step)
        {
            string text=KeeperStrings.Get("step."+step);
            if(step==KeeperStep.Prime)text=string.Format(text,Mathf.Max(0,state.PowerTuning.primerSqueezes-state.Engine.Primer));
            if(step==KeeperStep.Starter&&state.TimedStarter)text=KeeperStrings.Get("starter.timed.objective");
            if(step==KeeperStep.Starter)text=string.Format(text,Mathf.Max(0,state.PowerTuning.starterPulls-(state.Engine?.Pulls??state.StarterPulls)));
            if(step==KeeperStep.RadioTune)text=string.Format(text,state.Radio.TargetMHz.ToString("F1"));
            if(step==KeeperStep.RadioCode)text=KeeperStrings.Get(state.RadioRelay?"step.RadioCodeRelay":"step.RadioCodeLocal");
            return text;
        }
    }
}
