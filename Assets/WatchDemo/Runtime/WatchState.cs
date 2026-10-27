using LighthouseKeepers.Core.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LighthouseKeepers.WatchDemo
{
    public enum PowerState { Off, On }
    public sealed class WatchState
    {
        public const string PipeStation = "STN_PIPE_LEAK", GeneratorStation = "STN_GENERATOR", RadioStation = "STN_RADIO_CONSOLE";
        // Historical editor captures refer to this; physical M4 repair uses PipeRepairProfile.
        public const double RepairSeconds = 3.5;
        public const int RequiredStarterPulls = 3;
        readonly DayDefinition[] days;
        readonly HashSet<string> faults = new HashSet<string>();
        int index;
        public event Action Changed;
        public RadioState Radio {get;}
        public PipeRepairState Pipe {get;}
        public double BeaconDarkElapsed {get;private set;}
        public double BeaconDarkRemaining=>Math.Max(0,Definition.beaconDarkLimitSeconds-BeaconDarkElapsed);
        public bool BeaconDarkFailed {get;private set;}
        public string FailureMessage=>BeaconDarkFailed?"WATCH LOST — BEACON DARK":"WATCH ENDED — TIME EXPIRED";
        int repairSeed=System.Environment.TickCount;
        public DayDefinition Definition => days[index];
        public int Day => Definition.day;
        public double Duration => Definition.dayLengthSeconds;
        public double Elapsed { get; private set; }
        public int StarterPulls { get; private set; }
        public bool Repaired => !faults.Contains(PipeStation);
        public PowerState Power { get; private set; }
        public bool IsDone(string station) => !faults.Contains(station);
        public int CompletedTasks { get { int n = 0; foreach (var task in Definition.tasks) if (IsDone(task.stationId)) n++; return n; } }
        public bool Complete => CompletedTasks == Definition.tasks.Length;
        public bool HasNextDay => index + 1 < days.Length;
        public bool Failed => BeaconDarkFailed || (Elapsed >= Duration && !Complete);
        public WatchState() : this(Resources.Load<DayCatalog>("WatchDays").days, 1) { }
        public WatchState(DayDefinition[] definitions, int firstDay)
        {
            if (definitions == null || definitions.Length == 0) throw new ArgumentException("No authored watch days");
            days = definitions;
            Pipe=new PipeRepairState(Resources.Load<PipeRepairProfile>("PipeRepair"));
            Pipe.Changed+=()=>Changed?.Invoke();Pipe.Completed+=()=>CompleteRepair();
            Radio=new RadioState(Resources.Load<RadioProgram>("StormRadio"));
            Radio.BroadcastCompleted+=()=>{if(faults.Remove(RadioStation))Changed?.Invoke();};
            index = Array.FindIndex(days, d => d.day == firstDay);
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(firstDay));
            Reset();
        }
        public void Tick(double seconds)
        {
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            bool wasFailed = Failed;
            if(wasFailed)return;
            if(Power==PowerState.Off && Definition.beaconDarkLimitSeconds>0)
            {BeaconDarkElapsed=Math.Min(Definition.beaconDarkLimitSeconds,BeaconDarkElapsed+seconds);BeaconDarkFailed=BeaconDarkElapsed>=Definition.beaconDarkLimitSeconds;}
            Elapsed = Math.Min(Duration, Elapsed + seconds);
            Radio.Tick(seconds,!Failed);
            if (wasFailed != Failed) Changed?.Invoke();
        }
        public bool CompleteRepair()
        {
            if (Failed || Repaired) return false;
            faults.Remove(PipeStation); Changed?.Invoke(); return true;
        }
        // Called once per full stroke by the constrained interaction, never per frame.
        public bool StarterPulled()
        {
            if (Failed || Power == PowerState.On) return false;
            StarterPulls++;
            if (StarterPulls >= RequiredStarterPulls) { faults.Remove(GeneratorStation); Power = PowerState.On; BeaconDarkElapsed=0; Radio.SetPower(true); }
            Changed?.Invoke(); return true;
        }
        public bool Sleep()
        {
            if (!Complete || !HasNextDay) return false;
            index++; Reset(); return true;
        }
        public void Retry() => Reset();
        public bool ReplayFromDayOne()
        {
            if (Failed || !Complete || HasNextDay) return false;
            int first = Array.FindIndex(days, day => day.day == 1);
            if (first < 0) return false;
            index = first; Reset(); return true;
        }
        void Reset()
        {
            Elapsed = 0; BeaconDarkElapsed=0; BeaconDarkFailed=false; StarterPulls = 0; faults.Clear();
            foreach (var fault in Definition.startingFaults) faults.Add(fault);
            Power = faults.Contains(GeneratorStation) ? PowerState.Off : PowerState.On;
            Radio.Reset(Power==PowerState.On);
            Pipe.Reset(unchecked(++repairSeed),Repaired);
            Changed?.Invoke();
        }
    }
}
