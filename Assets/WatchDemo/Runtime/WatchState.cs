using LighthouseKeepers.Core.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Core;

namespace LighthouseKeepers.WatchDemo
{
    public enum PowerState { Off, On }
    public sealed partial class WatchState
    {
        public const string PipeStation = "STN_PIPE_LEAK", GeneratorStation = "STN_GENERATOR", RadioStation = "STN_RADIO_CONSOLE", BeaconStation="STN_BEACON_CONTROL";
        // Historical editor captures refer to this; physical M4 repair uses PipeRepairProfile.
        public const double RepairSeconds = 3.5;
        public const int RequiredStarterPulls = 3;
        readonly DayDefinition[] days;
        public S4Tuning Tuning {get;}
        public S4M2Tuning PowerTuning {get;}
        public ImmersionTuning Immersion {get;}
        public PowerGrid Grid {get;}
        public WatchEngineState Engine {get;}
        public BeaconStationState Beacon {get;}
        public HouseFloodState HouseFlood {get;}
        bool resetting;
        public bool BeaconShining=>Beacon!=null?Beacon.Shining(HasPower(PowerCircuit.Beacon)):HasPower(PowerCircuit.Beacon);
        public bool IgniteBeacon(bool lit)=>!ReadOnly&&!Failed&&!IsPending(BeaconStation)&&Beacon!=null&&Engaged(Beacon.Ignite(lit,HasPower(PowerCircuit.Beacon)));
        public bool AlignBeacon(float angle)=>!ReadOnly&&!Failed&&!IsPending(BeaconStation)&&Beacon!=null&&Engaged(Beacon.Align(angle));
        public void WipeBeacon(float metres){if(!ReadOnly&&!Failed&&!IsPending(BeaconStation)&&Beacon!=null){EngageWatch();Beacon.Wipe(metres);}}
        void BeaconChanged(){if(resetting)return;if(BeaconShining){BeaconDarkElapsed=0;if(!IsPending(BeaconStation))faults.Remove(BeaconStation);}Changed?.Invoke();}
        public bool PrimeEngine()=>!ReadOnly&&!Failed&&Engine!=null&&Engaged(Engine.Prime());
        public bool SetChoke(bool closed)=>!ReadOnly&&!Failed&&Engine!=null&&Engaged(Engine.SetChoke(closed));
        public void PourFuel(float seconds){if(!ReadOnly&&!Failed&&Engine!=null){if(seconds>0)EngageWatch();Engine.Pour(seconds);}}
        void EngineChanged()
        {
            if(resetting)return;
            bool wasPowered=Power==PowerState.On;
            Power=Engine.Running?PowerState.On:PowerState.Off;
            if(Engine.Settled&&!IsPending(GeneratorStation))faults.Remove(GeneratorStation);
            else if(!Engine.Running&&Array.Exists(Definition.tasks,t=>t.stationId==GeneratorStation))faults.Add(GeneratorStation);
            if(Day3Balance)
            {
                if(!Engine.Running&&day3Stage==1){StartDay3Outage();return;}
                if(wasPowered&&!Engine.Running)day3GraceUntil=Elapsed+Day3GraceSeconds;
                if(Engine.Settled&&day3Stage==2)ActivateDay3UpperJobs();
            }
            StarterPulls=Math.Min(RequiredStarterPulls,Engine.Pulls);
            if(Grid!=null)Grid.SetEngine(Engine.Running);else Radio.SetPower(Engine.Running);
            Changed?.Invoke();
        }
        public bool HasPower(PowerCircuit circuit)=>Grid!=null?Grid.Powered(circuit):Power==PowerState.On;
        public bool SetCircuit(int circuit,bool selected)=>!ReadOnly&&!Failed&&Grid!=null&&Engaged(Grid.SetCircuit((PowerCircuit)circuit,selected));
        public bool ResetBreaker()=>!ReadOnly&&!Failed&&Grid!=null&&Engaged(Grid.ResetBreaker());
        void GridChanged(){if(resetting)return;Radio.SetPower(RadioAvailable);BeaconChanged();}
        public double HighWaterElapsed {get;private set;}
        public bool HighWaterFailed {get;private set;}
        readonly HashSet<string> faults = new HashSet<string>();
        int index;
        public bool NetworkRehearsal { get; private set; }
        public bool RemoteReplica { get; private set; }
        LighthouseKeepers.Lobby.ICoopWatchLink coop;
        public LighthouseKeepers.Lobby.ICoopWatchLink Coop
        {get=>coop;set{coop=value;if(value!=null)balanceCrew=true;}}
        public bool ReadOnly => NetworkRehearsal || RemoteReplica;
        public void SetRemoteReplica(bool value)
        { RemoteReplica=value; Pipe.ReadOnly=Radio.ReadOnly=ReadOnly;if(Grid!=null)Grid.ReadOnly=ReadOnly;if(Engine!=null)Engine.ReadOnly=ReadOnly;if(Beacon!=null)Beacon.ReadOnly=ReadOnly; }
        public void SetNetworkRehearsal(bool value)
        {NetworkRehearsal=value;Pipe.ReadOnly=Radio.ReadOnly=ReadOnly;if(Grid!=null)Grid.ReadOnly=ReadOnly;if(Engine!=null)Engine.ReadOnly=ReadOnly;if(Beacon!=null)Beacon.ReadOnly=ReadOnly;}
        public event Action Changed;
        public RadioState Radio {get;}
        public PipeRepairState Pipe {get;}
        public double BeaconDarkElapsed {get;private set;}
        public double BeaconDarkLimit=>Definition.beaconDarkLimitSeconds*(Tuning&&Tuning.difficulty?Tuning.beaconLimitMultiplier:1);
        public double BeaconDarkRemaining=>Math.Max(0,BeaconDarkLimit-BeaconDarkElapsed)+BeaconGraceRemaining;
        public bool BeaconDarkFailed {get;private set;}
        public string FailureMessage=>HighWaterFailed?"WATCH LOST — LIGHTHOUSE FLOODED":BeaconDarkFailed?"WATCH LOST — BEACON DARK":"WATCH ENDED — TIME EXPIRED";
        int repairSeed=System.Environment.TickCount;
        public DayDefinition Definition => days[index];
        public int Day => Definition.day;
        public double Duration => Day3Balance ? (balanceCrew?Tuning.day3CrewSeconds:Tuning.day3SoloSeconds) : Definition.dayLengthSeconds*(Tuning&&Tuning.difficulty?Tuning.dayLengthMultiplier:1);
        public double Elapsed { get; private set; }
        public int StarterPulls { get; private set; }
        public bool Repaired => !faults.Contains(PipeStation);
        public PowerState Power { get; private set; }
        int replicaObjective;
        public int ObjectiveIndex
        {
            get
            {
                if(RemoteReplica)return replicaObjective;
                if(Complete)return HasNextDay?-1:-2;
                int first=-2,generator=-1,pipe=-1,beacon=-1;
                for(int i=0;i<Definition.tasks.Length;i++){var id=Definition.tasks[i].stationId;if(IsDone(id)||IsPending(id))continue;if(first<0)first=i;if(id==GeneratorStation)generator=i;if(id==PipeStation)pipe=i;if(id==BeaconStation)beacon=i;}
                if(Power==PowerState.Off&&generator>=0)return generator;
                if(beacon>=0&&!BeaconShining&&(Day3Balance||BeaconDarkRemaining<20))return beacon;
                return pipe>=0?pipe:first;
            }
        }
        public bool IsDone(string station) => !faults.Contains(station);
        public int CompletedTasks { get { int n = 0; foreach (var task in Definition.tasks) if (IsDone(task.stationId)) n++; return n; } }
        public bool Complete => CompletedTasks == Definition.tasks.Length;
        public bool HasNextDay => index + 1 < days.Length;
        public bool Failed => HighWaterFailed || BeaconDarkFailed || (Elapsed >= Duration && !Complete);
        public WatchState() : this(Resources.Load<DayCatalog>("WatchDays").days, 1) { }
        public WatchState(DayDefinition[] definitions, int firstDay):this(definitions,firstDay,null){}
        public WatchState(DayDefinition[] definitions, int firstDay,S4Tuning tuning):this(definitions,firstDay,tuning,null){}
        public WatchState(DayDefinition[] definitions, int firstDay,S4Tuning tuning,S4M2Tuning powerTuning)
            :this(definitions,firstDay,tuning,powerTuning,null){}
        public WatchState(DayDefinition[] definitions, int firstDay,S4Tuning tuning,S4M2Tuning powerTuning,ImmersionTuning immersion)
        {
            if (definitions == null || definitions.Length == 0) throw new ArgumentException("No authored watch days");
            days = definitions;
            Tuning=tuning&&tuning.enabledForWatch?tuning:null;
            if(Tuning&&Tuning.houseDoorwayFlow)HouseFlood=new HouseFloodState(Tuning);
            Immersion=immersion&&immersion.enabledForWatch?immersion:null;
            bool forgiving=Immersion&&Immersion.forgivingPipe;
            Pipe=new PipeRepairState(Resources.Load<PipeRepairProfile>(forgiving?"ImmersionPipeRepair":"PipeRepair"),forgiving);
            Pipe.PullTowardTightens=forgiving&&Immersion.leverPipe&&Immersion.pullTowardTightens;
            if(Tuning&&Tuning.difficulty)Pipe.RetainedPartialProgress=Tuning.slipRetainedFraction;
            Pipe.Changed+=()=>{EngageWatch();Changed?.Invoke();};Pipe.Completed+=()=>CompleteRepair();
            Radio=new RadioState(Resources.Load<RadioProgram>("StormRadio"));
            Radio.BroadcastCompleted+=()=>{if(faults.Remove(RadioStation))Changed?.Invoke();};
            PowerTuning=powerTuning&&powerTuning.enabledForWatch?powerTuning:null;
            if(PowerTuning&&PowerTuning.powerGrid){Grid=new PowerGrid(PowerTuning.pumpLoad,PowerTuning.beaconLoad,PowerTuning.radioLoad,PowerTuning.lightsLoad);Grid.Changed+=GridChanged;}
            if(PowerTuning&&PowerTuning.generatorSequence){Engine=new WatchEngineState(PowerTuning){IgnoreExtraPulls=Immersion&&Immersion.forgivingGenerator};Engine.Changed+=EngineChanged;}
            if(PowerTuning&&PowerTuning.beaconStation){Beacon=new BeaconStationState(PowerTuning);Beacon.Changed+=BeaconChanged;}
            index = Array.FindIndex(days, d => d.day == firstDay);
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(firstDay));
            Reset();
        }
        public void Tick(double seconds)
        {
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            bool wasFailed = Failed;
            if(wasFailed||ReadOnly)return;
            // Split only the scheduled Day 3 boundary; a long simulation tick must not skip the stage.
            if(Day3Balance&&day3Stage==1)
            {
                double until=Tuning.day3GeneratorFailureSeconds-Elapsed;
                if(until>0&&seconds>until){Tick(until);Tick(seconds-until);return;}
            }
            double darkSeconds=Day3Balance?Math.Max(0,seconds-BeaconGraceRemaining):seconds;
            Engine?.Tick((float)seconds);
            Beacon?.Tick((float)seconds,HasPower(PowerCircuit.Beacon));
            if(!BeaconShining && Definition.beaconDarkLimitSeconds>0)
            {BeaconDarkElapsed=Math.Min(BeaconDarkLimit,BeaconDarkElapsed+darkSeconds);BeaconDarkFailed=BeaconDarkElapsed>=BeaconDarkLimit;}
            Elapsed = Math.Min(Duration, Elapsed + seconds);
            if(Day3Balance&&day3Stage==1&&Elapsed>=Tuning.day3GeneratorFailureSeconds)StartDay3Outage();
            Radio.Tick(seconds,!Failed);
            if (wasFailed != Failed) Changed?.Invoke();
        }
        public void AdvanceHouseFlood(float seconds,float height)
        {if(!ReadOnly&&!Failed)HouseFlood?.Step(seconds,height,Repaired&&HasPower(PowerCircuit.Pump));}
        public void ObserveWater(double seconds,float height)
        {
            if(!double.IsFinite(seconds)||seconds<0||!float.IsFinite(height))throw new ArgumentOutOfRangeException();
            if(ReadOnly||Failed||!Tuning||!Tuning.highWaterLoss)return;
            HighWaterElapsed=height>=Tuning.failWaterHeightM?Math.Min(Tuning.failWaterSustainSeconds,HighWaterElapsed+seconds):0;
            if(HighWaterElapsed>=Tuning.failWaterSustainSeconds){HighWaterFailed=true;Changed?.Invoke();}
        }
        public bool CompleteRepair()
        {
            if (ReadOnly || Failed || Repaired) return false;
            EngageWatch();faults.Remove(PipeStation);if(Day3Balance&&day3Stage==1)StartDay3Outage();Changed?.Invoke(); return true;
        }
        // Called once per full stroke by the constrained interaction, never per frame.
        public bool StarterPulled()
        {
            if(ReadOnly||Failed)return false;
            EngageWatch();
            if(Engine!=null)return Engine.Pull();
            if (Power == PowerState.On) return false;
            StarterPulls++;
            if (StarterPulls >= RequiredStarterPulls) { faults.Remove(GeneratorStation); Power = PowerState.On; if(Grid!=null)Grid.SetEngine(true);else {BeaconDarkElapsed=0; Radio.SetPower(true);} }
            Changed?.Invoke(); return true;
        }
        public bool Sleep()
        {
            if (ReadOnly || Failed || !Complete || !HasNextDay) return false;
            index++; Reset(); return true;
        }
        public void Retry() {if(!ReadOnly)Reset();}
        public bool ReplayFromDayOne()
        {
            if (ReadOnly || Failed || !Complete || HasNextDay) return false;
            int first = Array.FindIndex(days, day => day.day == 1);
            if (first < 0) return false;
            index = first; Reset(); return true;
        }
        public WatchSnapshot Capture(float waterHeight)
        {
            int done=0;
            for(int i=0;i<Definition.tasks.Length;i++)if(IsDone(Definition.tasks[i].stationId))done|=1<<i;
            return new WatchSnapshot { houseWaterVolume=HouseFlood?.Volume??0,day3Stage=day3Stage,day3Crew=balanceCrew,day3GraceUntil=day3GraceUntil,objectiveIndex=ObjectiveIndex,day=Day,elapsed=Elapsed,water=waterHeight,power=(int)Power,doneMask=done,
                beaconStationEnabled=Beacon!=null,beaconIgnited=Beacon?.Ignited??false,beaconAlignment=Beacon?.Alignment??0,beaconTarget=Beacon?.Target??0,beaconSoot=Beacon?.Soot??0,
                engineEnabled=Engine!=null,fuel=Engine?.Fuel??0,primer=Engine?.Primer??0,enginePulls=Engine?.Pulls??0,engineClosedPulls=Engine?.ClosedPulls??0,chokeClosed=Engine?.ChokeClosed??false,engineSettled=Engine?.Settled??false,chokeRemaining=Engine?.OpenRemaining??0,engineFloodRemaining=Engine?.FloodRemaining??0,coughSerial=Engine?.CoughSerial??0,
                gridEnabled=Grid!=null,gridCapacity=Grid?.Capacity??0,gridMask=Grid?.SelectedMask??0,gridTripped=Grid?.Tripped??false,
                beaconDarkElapsed=BeaconDarkElapsed,beaconDarkFailed=BeaconDarkFailed,starterPulls=StarterPulls,
                highWaterElapsed=HighWaterElapsed,highWaterFailed=HighWaterFailed,
                bolts=Pipe.BoltsDone,activeBolt=Pipe.ActiveBolt,turns=Pipe.TurnsDone,stroke=Pipe.StrokeDegrees,
                seal=Pipe.SealElapsed,needsReturn=Pipe.NeedsReturn,
                order0=Pipe.Order(0),order1=Pipe.Order(1),order2=Pipe.Order(2),seat=Pipe.SeatElapsed,
                direction=Pipe.Direction,returned=Pipe.Returned,touching=Pipe.Touching,
                radioHeld=Radio.Held,radioTrigger=Radio.Trigger,
                radioMain=Radio.MainOn,radioLocked=Radio.Locked,radioDone=Radio.Completed,frequency=Radio.FrequencyMHz,
                lockElapsed=Radio.LockElapsed,transmitElapsed=Radio.TransmitElapsed,codeProgress=Radio.CodeProgress };
        }
        public bool ApplyRemote(WatchSnapshot snapshot)
        {
            if(!RemoteReplica || snapshot==null || !snapshot.Valid())return false;
            int next=Array.FindIndex(days,d=>d.day==snapshot.day);
            if(next<0 || snapshot.objectiveIndex>=days[next].tasks.Length || snapshot.elapsed>SnapshotDuration(days[next],snapshot.day3Crew) || (snapshot.doneMask>>days[next].tasks.Length)!=0)return false;
            if(!ValidDay3Snapshot(days[next],snapshot))return false;
            if(snapshot.gridEnabled!=(Grid!=null)||snapshot.engineEnabled!=(Engine!=null)||snapshot.beaconStationEnabled!=(Beacon!=null))return false;
            if(Grid!=null&&!Grid.ApplyReplica(snapshot.gridCapacity,snapshot.gridMask,snapshot.power==1,snapshot.gridTripped))return false;
            Beacon?.ApplyReplica(snapshot.beaconIgnited,snapshot.beaconAlignment,snapshot.beaconTarget,snapshot.beaconSoot,days[next].beaconSootPerSecond);
            Engine?.ApplyReplica(snapshot.fuel,snapshot.primer,snapshot.enginePulls,snapshot.chokeClosed,snapshot.power==1,snapshot.engineSettled,snapshot.chokeRemaining,snapshot.engineFloodRemaining,snapshot.coughSerial,(snapshot.day3Stage>0?Tuning.day3FuelDrainPerSecond:days[next].fuelDrainPerSecond),snapshot.engineClosedPulls);
            day3Stage=snapshot.day3Stage;balanceCrew=snapshot.day3Crew;day3GraceUntil=snapshot.day3GraceUntil;replicaObjective=snapshot.objectiveIndex;index=next;Elapsed=snapshot.elapsed;Power=(PowerState)snapshot.power;StarterPulls=snapshot.starterPulls;
            BeaconDarkElapsed=snapshot.beaconDarkElapsed;BeaconDarkFailed=snapshot.beaconDarkFailed;
            HighWaterElapsed=snapshot.highWaterElapsed;HighWaterFailed=snapshot.highWaterFailed;
            HouseFlood?.Apply(snapshot.houseWaterVolume);
            faults.Clear();for(int i=0;i<Definition.tasks.Length;i++)if((snapshot.doneMask&(1<<i))==0)faults.Add(Definition.tasks[i].stationId);
            Pipe.ApplyReplica(snapshot);Radio.ApplyReplica(snapshot);Changed?.Invoke();return true;
        }
        void Reset()
        {
            resetting=true;HouseFlood?.Reset();ResetWatchGate();
            Elapsed = 0; BeaconDarkElapsed=0; BeaconDarkFailed=false; StarterPulls = 0; faults.Clear();
            HighWaterElapsed=0;HighWaterFailed=false;
            foreach (var fault in Definition.startingFaults) faults.Add(fault);
            day3Stage=Day3Balance?1:0;day3GraceUntil=0;balanceCrew=Coop!=null;
            Power = !Day3Balance&&faults.Contains(GeneratorStation) ? PowerState.Off : PowerState.On;
            Beacon?.Reset(Day3Balance||Definition.beaconInitiallyLit,Day3Balance?Definition.beaconTargetAlignment:Definition.beaconInitialAlignment,Definition.beaconTargetAlignment,Definition.beaconInitialSoot,Definition.beaconSootPerSecond);
            Engine?.Reset(Day3Balance?Tuning.day3ArrivalFuel:Definition.initialFuelFraction,Day3Balance?Tuning.day3FuelDrainPerSecond:Definition.fuelDrainPerSecond,Power==PowerState.On);
            if(Grid!=null)Grid.Reset(Definition.powerCapacity,Definition.initialCircuitMask,Power==PowerState.On);
            Radio.Reset(RadioAvailable);
            Pipe.Reset(unchecked(++repairSeed),Repaired);
            resetting=false;
            Changed?.Invoke();
        }
    }
}
