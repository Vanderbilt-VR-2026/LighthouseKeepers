using System;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    public enum KeeperStep { None, GrabWrench, BoltA, BoltB, BoltC, Fuel, Prime, CloseChoke, Starter, OpenChoke, PumpOff, BeaconOff, RadioOff, LightsOff, PumpOn, BeaconOn, RadioOn, LightsOn, ResetBreaker, RadioPower, RadioTune, RadioCode, RadioTransmit, BeaconLight, BeaconAlign, BeaconWipe, Sleep, Complete, Lost, CheckFuel, HoldValve, ClampValve, TightenBolt }
    // Sole instruction policy. The host assigns jobs; presentation never invents a next action.
    public sealed class ObjectiveDirector
    {
        readonly WatchState state;
        static readonly PowerCircuit[] shedOrder={PowerCircuit.Pump,PowerCircuit.Radio,PowerCircuit.Lights,PowerCircuit.Beacon};
        int actorA,actorB,jobA=-1,jobB=-1;float assignmentUntil;
        public Func<string,Vector3> StationPosition;
        public KeeperStep First {get;private set;}
        public KeeperStep Second {get;private set;}
        public int FirstActor=>actorA;
        public int SecondActor=>actorB;
        public ObjectiveDirector(WatchState owner){state=owner;}
        public KeeperStep ForActor(int actor)=>actorB!=0&&actor==actorB?Second:First;
        public KeeperStep Partner(int actor)=>actorB==0?KeeperStep.None:actor==actorB?First:Second;
        public static bool Valid(int step)=>step>=0&&step<=(int)KeeperStep.TightenBolt;
        public void Apply(int first,int second,int a,int b)
        {if(!state.ReadOnly)return;First=(KeeperStep)first;Second=(KeeperStep)second;actorA=a;actorB=b;}
        public void Reset(){First=Second=KeeperStep.None;actorA=actorB=0;jobA=jobB=-1;assignmentUntil=0;}
        public void Tick(float now,int firstActor,Vector3 firstHead,int secondActor,Vector3 secondHead,bool wrenchHeld)
        {
            if(state.ReadOnly)return;
            bool roster=actorA!=firstActor||actorB!=secondActor;actorA=firstActor;actorB=secondActor;
            if(state.Failed){First=Second=KeeperStep.Lost;return;}
            if(state.Complete){First=state.HasNextDay?KeeperStep.Sleep:KeeperStep.Complete;Second=actorB!=0?First:KeeperStep.None;return;}
            if(actorB!=0&&state.Pressure?.CrewRequired==true&&!state.Repaired&&(state.Power==PowerState.On||state.IsPending(WatchState.GeneratorStation)))
            {
                int holder=state.Pressure.Holder;
                if(holder!=actorA&&holder!=actorB){int worker=state.PipeWorkerActor;var valve=StationPosition!=null?StationPosition("STN_PRESSURE_VALVE"):new Vector3(2.7f,1,2.5f);holder=worker==actorA?actorB:worker==actorB?actorA:Vector3.Distance(firstHead,valve)<=Vector3.Distance(secondHead,valve)?actorA:actorB;}
                First=holder==actorA?KeeperStep.HoldValve:PipeStep(wrenchHeld);Second=holder==actorB?KeeperStep.HoldValve:PipeStep(wrenchHeld);return;
            }
            if(roster||now>=assignmentUntil||!Available(jobA)||actorB!=0&&!Available(jobB))
            {
                float best=float.PositiveInfinity;int a=-1,b=-1;
                for(int i=0;i<state.Definition.tasks.Length;i++)if(Available(i))
                {
                    if(actorB==0){float cost=Cost(i,firstHead);if(cost<best){best=cost;a=i;}continue;}
                    for(int j=0;j<state.Definition.tasks.Length;j++)if(Available(j))
                    {float cost=Cost(i,firstHead)+Cost(j,secondHead)+(i==j?10000:0);if(cost<best){best=cost;a=i;b=j;}}
                }
                jobA=a;jobB=b;assignmentUntil=now+state.Master.assignmentSeconds;
            }
            First=jobA<0&&state.GeneratorTutorial&&!state.NightTwoOutage?KeeperStep.CheckFuel:Step(jobA,wrenchHeld);Second=actorB!=0?Step(jobB,wrenchHeld):KeeperStep.None;
        }
        bool Available(int i)=>i>=0&&i<state.Definition.tasks.Length&&!state.IsDone(state.Definition.tasks[i].stationId)&&!state.IsPending(state.Definition.tasks[i].stationId)&&(state.Power==PowerState.On||state.Definition.tasks[i].stationId==WatchState.PipeStation||state.Definition.tasks[i].stationId==WatchState.GeneratorStation);
        float Cost(int i,Vector3 head)
        {
            string id=state.Definition.tasks[i].stationId;float floor=id==WatchState.PipeStation?1:id==WatchState.GeneratorStation?2:4;
            var point=StationPosition!=null?StationPosition(id):new Vector3(0,(floor-1)*3.3f+1.6f,0);
            float cost=Vector3.Distance(head,point);
            if(id==WatchState.GeneratorStation&&state.Power==PowerState.Off)cost-=45;
            if(id==WatchState.BeaconStation&&!state.BeaconShining)cost-=state.BeaconDarkRemaining<30?65:15;
            if(id==WatchState.PipeStation)cost-=12;
            return cost;
        }
        KeeperStep EngineStep()
        {
            var e=state.Engine;
            if(e==null)return KeeperStep.Starter;
            if(e.Running)return e.ChokeClosed?KeeperStep.OpenChoke:KeeperStep.None;
            if(!e.EnoughFuel||state.Master.onboarding&&e.Fuel<state.Master.guidedFuelTarget)return KeeperStep.Fuel;
            if(!e.Primed)return KeeperStep.Prime;
            if(!e.ChokeClosed)return KeeperStep.CloseChoke;
            return KeeperStep.Starter;
        }
        public KeeperStep StepForStation(string id,bool held)
        {
            if(state.Failed)return KeeperStep.Lost;
            if(state.IsDone(id)||state.IsPending(id))return KeeperStep.None;
            if(id==WatchState.PipeStation)return state.Pressure?.CrewRequired==true?KeeperStep.HoldValve:PipeStep(held);
            if(id==WatchState.GeneratorStation)return EngineStep();
            if(id==WatchState.RadioStation&&state.Connected&&state.Connected.dependencies&&!state.BeaconShining)return BeaconStep();
            var circuit=id==WatchState.RadioStation?PowerCircuit.Radio:PowerCircuit.Beacon;
            if(state.Power==PowerState.Off)return EngineStep();
            if(!state.HasPower(circuit))return BreakerStep(circuit);
            if(id==WatchState.RadioStation)
                return state.Radio.Completed?KeeperStep.None:state.Radio.Step==0?KeeperStep.RadioPower:state.Radio.Step==1?KeeperStep.RadioTune:state.Radio.Step==2?KeeperStep.RadioCode:KeeperStep.RadioTransmit;
            return BeaconStep();
        }
        KeeperStep BeaconStep()
        {
            if(state.Power==PowerState.Off)return EngineStep();
            if(!state.HasPower(PowerCircuit.Beacon))return BreakerStep(PowerCircuit.Beacon);
            var beacon=state.Beacon;
            return beacon==null?KeeperStep.None:!beacon.Ignited?KeeperStep.BeaconLight:!beacon.Aligned?KeeperStep.BeaconAlign:beacon.Soot>.5f?KeeperStep.BeaconWipe:KeeperStep.None;
        }
        KeeperStep PipeStep(bool held)
        {if(!held)return KeeperStep.GrabWrench;if(state.Pressure?.CrewRequired==true)return KeeperStep.TightenBolt;return state.Pipe.ActiveBolt>=0?(KeeperStep)((int)KeeperStep.BoltA+state.Pipe.ActiveBolt):KeeperStep.None;}
        KeeperStep Step(int job,bool held)=>Available(job)?StepForStation(state.Definition.tasks[job].stationId,held):KeeperStep.None;
        KeeperStep BreakerStep(PowerCircuit desired)
        {
            var g=state.Grid;if(g==null)return EngineStep();
            float required=g.Selected(desired)?0:desired==PowerCircuit.Radio?state.PowerTuning.radioLoad:state.PowerTuning.beaconLoad;
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
            if(step==KeeperStep.HoldValve||step==KeeperStep.ClampValve||step==KeeperStep.TightenBolt||step>=KeeperStep.GrabWrench&&step<=KeeperStep.BoltC)return WatchState.PipeStation;
            if(step==KeeperStep.CheckFuel||step>=KeeperStep.Fuel&&step<=KeeperStep.OpenChoke)return WatchState.GeneratorStation;
            if(step>=KeeperStep.PumpOff&&step<=KeeperStep.ResetBreaker)return "BREAKER";
            if(step>=KeeperStep.RadioPower&&step<=KeeperStep.RadioTransmit)return WatchState.RadioStation;
            if(step>=KeeperStep.BeaconLight&&step<=KeeperStep.BeaconWipe)return WatchState.BeaconStation;
            return step==KeeperStep.Sleep?"BED":"";
        }
        public static int Floor(KeeperStep step){string id=Job(step);return id==WatchState.PipeStation?1:id==WatchState.GeneratorStation||id=="BREAKER"?2:id=="BED"?3:4;}
        public static string Location(KeeperStep step)
        {
            string key=step==KeeperStep.GrabWrench?"where.workbench":step==KeeperStep.Fuel?"where.fuel":"where."+Job(step);
            return KeeperStrings.Has(key)?KeeperStrings.Get(key):"";
        }
        public static string Button(KeeperStep step)=>step==KeeperStep.None||step==KeeperStep.Lost||step==KeeperStep.Complete||step==KeeperStep.CheckFuel?"":step==KeeperStep.RadioTransmit?"TRIGGER":"GRIP";
        public static int Hint(KeeperStep step){string id=Job(step);return id==WatchState.PipeStation?7:id==WatchState.GeneratorStation?8:id=="BREAKER"?9:id==WatchState.BeaconStation?10:id==WatchState.RadioStation?11:id=="BED"?12:0;}
        public string StationInstruction(string station) {
            if(Job(First)==station)return Instruction(First);
            if(Job(Second)==station)return Instruction(Second);
            return "";
        }
        public string Instruction(KeeperStep step)
        {
            string text=KeeperStrings.Get("step."+step);
            if(step==KeeperStep.Prime)text=string.Format(text,Mathf.Max(0,state.PowerTuning.primerSqueezes-state.Engine.Primer));
            if(step==KeeperStep.Starter)text=string.Format(text,Mathf.Max(0,state.PowerTuning.starterPulls-(state.Engine?.Pulls??state.StarterPulls)));
            if(step==KeeperStep.RadioTune)text=string.Format(text,state.Radio.Program.targetMHz.ToString("F1"));
            if(step==KeeperStep.RadioCode)text=string.Format(text,KeeperStrings.Get("colour."+state.Radio.Program.callCode[Mathf.Min(state.Radio.CodeProgress,state.Radio.Program.callCode.Length-1)]));
            return text;
        }
    }
}
