using System;
using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    public enum NightFaultKind { StationFailure=0, LowFuel=1 }
    public enum NightEventGate { Clock=0, ClockOrJobComplete=1, PowerRestored=2, JobComplete=3 }
    public enum NightCrewVariant { Local=0, HoldValve=1, RelayGenerator=2, RelayRadioCode=3, RelayBeaconBearing=4 }
    [Serializable] public sealed class NightFault
    {
        public string id,stationId,afterStationId;
        public NightEventGate gate;
        public NightFaultKind kind;
        [Range(0,1)] public float fuelFraction=.1f;
        [Min(0)] public float atSeconds,graceSeconds;
        public bool repeat;
    }
    [Serializable] public sealed class NightStationRule
    {
        public string stationId;
        public string[] activeSteps=Array.Empty<string>();
        public NightCrewVariant crewVariant;
        public string informationSite;
        public bool soloLocalInformation=true;
    }
    [Serializable] public sealed class NightShipEvent
    {
        public string id,nameKey;
        [Min(0)] public float sightedAtSeconds;
        [Min(1)] public float approachSeconds=75,windowSeconds=55,darkGraceSeconds=30;
        [Min(0)] public float broadcastExtensionSeconds;
        public bool safePreview,largeFinale;
        public float WindowStart=>sightedAtSeconds+approachSeconds;
        public float LatestExit=>WindowStart+windowSeconds+broadcastExtensionSeconds;
    }
    [Serializable] public sealed class NightRadioCue
    {
        public string eventId;
        [Range(1,101)] public int cue;
    }
    // Optional authored contract. Legacy days remain unchanged when this is not configured.
    // No network transport, game clock or presentation state lives in this data layer.
    [Serializable] public sealed class NightScenario
    {
        public string id,nameKey;
        public bool configured,demo,noFail,finaleFaultPileup,timingApproved;
        [Range(1,4)] public int stormBaseStage=1;
        [Range(0,1)] public float peakStormAfterFraction=1;
        [Min(30)] public float soloSeconds=450,crewSeconds=330;
        [Range(.1f,2)] public float soloFloodMultiplier=.65f,crewFloodMultiplier=.85f;
        [Min(0)] public float openingSeconds=45,reliefSeconds=15,startGraceSeconds=45,firstStationGraceSeconds=60;
        [Min(10)] public float faultSpacingSeconds=12;
        [Range(0,1)] public float arrivalFuelFraction=.8f;
        [Range(0,1)] public float refillTargetFraction=.35f;
        public int openingCue,tonightCue;
        public NightFault[] faults=Array.Empty<NightFault>();
        public NightStationRule[] stations=Array.Empty<NightStationRule>();
        public NightShipEvent[] ships=Array.Empty<NightShipEvent>();
        public NightRadioCue[] cues=Array.Empty<NightRadioCue>();
        public IEnumerable<string> Validate()
        {
            bool Positive(float x)=>!float.IsNaN(x)&&!float.IsInfinity(x)&&x>0;
            bool Nonnegative(float x)=>x==0||Positive(x);
            if(string.IsNullOrWhiteSpace(id)||string.IsNullOrWhiteSpace(nameKey))yield return "Night needs stable id and name key.";
            if(!Positive(soloSeconds)||!Positive(crewSeconds)||soloSeconds<30||crewSeconds<30)yield return "Night duration is invalid.";
            if(!Positive(soloFloodMultiplier)||!Positive(crewFloodMultiplier))yield return "Flood modifiers must be positive.";
            if(stormBaseStage<1||stormBaseStage>4||peakStormAfterFraction<0||peakStormAfterFraction>1)yield return "Storm stage is invalid.";
            if(!Nonnegative(openingSeconds)||!Nonnegative(reliefSeconds)||!Nonnegative(startGraceSeconds)||!Nonnegative(firstStationGraceSeconds))yield return "Grace or pacing is negative.";
            if(openingCue<1||openingCue>101||tonightCue<0||tonightCue>101)yield return "Unknown opening/tonight cue.";
            var stationIds=new HashSet<string>();
            foreach(var s in stations??Array.Empty<NightStationRule>()){
                if(s==null||string.IsNullOrWhiteSpace(s.stationId)||!stationIds.Add(s.stationId)){yield return "Missing/duplicate station.";continue;}
                if(s.activeSteps==null||s.activeSteps.Length==0)yield return "Station needs active steps.";
                else foreach(var step in s.activeSteps)if(string.IsNullOrWhiteSpace(step))yield return "Empty step id.";
                if(s.crewVariant!=NightCrewVariant.Local&&string.IsNullOrWhiteSpace(s.informationSite))yield return "Crew role needs an information site.";
            }
            if(!Nonnegative(faultSpacingSeconds)||faultSpacingSeconds<10||!Nonnegative(arrivalFuelFraction)||arrivalFuelFraction>1||!Nonnegative(refillTargetFraction)||refillTargetFraction>1)yield return "Invalid fault spacing or arrival fuel.";
            if(faults!=null&&faults.Length>30)yield return "At most 30 fault events can be replicated.";
            var ids=new HashSet<string>();
            foreach(var f in faults??Array.Empty<NightFault>()){
                if(f==null||string.IsNullOrWhiteSpace(f.id)||!ids.Add(f.id)){yield return "Missing/duplicate fault event.";continue;}
                if(!Enum.IsDefined(typeof(NightEventGate),f.gate)||!Enum.IsDefined(typeof(NightFaultKind),f.kind)||!Nonnegative(f.fuelFraction)||f.fuelFraction>1)yield return "Invalid fault kind or fuel: "+f.id;
                if(!stationIds.Contains(f.stationId))yield return "Fault station is not active: "+f.id;
                if(float.IsNaN(f.atSeconds)||float.IsInfinity(f.atSeconds)||f.atSeconds<0||!Nonnegative(f.graceSeconds))yield return "Fault timing is invalid: "+f.id;
                if((f.gate==NightEventGate.JobComplete||f.gate==NightEventGate.ClockOrJobComplete)&&!stationIds.Contains(f.afterStationId))yield return "Fault dependency is not active: "+f.id;
            }
            ids.Clear();
            foreach(var ship in ships??Array.Empty<NightShipEvent>()){
                if(ship==null||string.IsNullOrWhiteSpace(ship.id)||!ids.Add(ship.id)){yield return "Missing/duplicate ship.";continue;}
                if(string.IsNullOrWhiteSpace(ship.nameKey)||!Positive(ship.approachSeconds)||!Positive(ship.windowSeconds)||!Positive(ship.darkGraceSeconds)||!Nonnegative(ship.sightedAtSeconds)||!Nonnegative(ship.broadcastExtensionSeconds))yield return "Invalid ship event: "+ship.id;
                if(ship.LatestExit>Math.Min(soloSeconds,crewSeconds))yield return "Ship extends beyond dawn: "+ship.id;
            }
            foreach(var cue in cues??Array.Empty<NightRadioCue>())if(cue==null||string.IsNullOrWhiteSpace(cue.eventId)||cue.cue<1||cue.cue>101)yield return "Invalid radio event.";
        }
    }
}
