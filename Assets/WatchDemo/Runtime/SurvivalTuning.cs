using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    [CreateAssetMenu(menuName="Lighthouse Keepers/Endless survival")]
    public sealed class SurvivalTuning:ScriptableObject
    {
        [Min(.1f)] public float soloFlood=1,crewFlood=1.1f;
        [Min(0)] public float floodIncreasePerDay=.08f;
        [Range(.1f,1)] public float firstDayFloodCushion=.4f,secondDayFloodCushion=.9f;
        public bool firstDayPractice=true;
        [Range(.05f,.4f)] public float practiceWaterCap=.22f;
        [Min(0)] public float earlyGraceBonusSeconds=20;
        [Min(1)] public float firstBeamDegrees=14,minimumBeamDegrees=4;
        [Min(0)] public float aimDifficultyPerDay=.18f;
        [Min(30)] public float watchSeconds=420;
        [Min(0)] public float startGraceSeconds=45,firstStationGraceSeconds=45;
        [Min(1)] public float approachSeconds=45,passageSeconds=30;
        [Range(0,1)] public float initialFuel=.05f;
        [Min(0)] public float repairedSeepPerSecond=.0006f;
        public static SurvivalTuning Current=>Resources.Load<SurvivalTuning>("SurvivalTuning");
        public float Flood(int day,bool crew)=>(crew?crewFlood:soloFlood)*(1+Mathf.Max(0,day-1)*floodIncreasePerDay)*(day==1?firstDayFloodCushion:day==2?secondDayFloodCushion:1);
        public float Beam(int day)=>minimumBeamDegrees+(firstBeamDegrees-minimumBeamDegrees)/(1+Mathf.Max(0,day-1)*aimDifficultyPerDay);
        // One transient definition per active day; the five authored campaign assets are never changed.
        public DayDefinition Day(int number)
        {
            var seed=Resources.Load<FiveNightCatalog>("M3Campaign").nights[0];
            var d=Instantiate(seed);d.hideFlags=HideFlags.DontSave;
            d.scenario=JsonUtility.FromJson<NightScenario>(JsonUtility.ToJson(seed.scenario));
            d.day=number;d.name="Survival day "+number;d.initialFuelFraction=initialFuel;d.fuelDrainPerSecond=0;
            d.beaconInitiallyLit=false;d.beaconInitialAlignment=.3f;d.beaconDarkLimitSeconds=0;
            var s=d.scenario;s.id="survival";s.nameKey="survival.title";s.noFail=false;s.demo=false;
            s.soloSeconds=s.crewSeconds=watchSeconds;s.soloFloodMultiplier=Flood(number,false);s.crewFloodMultiplier=Flood(number,true);
            s.startGraceSeconds=startGraceSeconds+(number<=2?earlyGraceBonusSeconds:0);s.firstStationGraceSeconds=firstStationGraceSeconds+(number<=2?earlyGraceBonusSeconds:0);
            s.openingSeconds=0;s.reliefSeconds=15;s.stormBaseStage=Mathf.Min(4,1+(number-1)/2);s.peakStormAfterFraction=1;
            s.arrivalFuelFraction=initialFuel;s.radioClearance=true;s.tonightCue=0;
            s.faults=new[]{Fault("engine",WatchState.GeneratorStation),Fault("leak",WatchState.PipeStation),Fault("lamp",WatchState.BeaconStation),Fault("clearance",WatchState.RadioStation)};
            foreach(var station in s.stations){
                station.crewVariant=station.stationId==WatchState.PipeStation?NightCrewVariant.HoldValve:station.stationId==WatchState.RadioStation?NightCrewVariant.RelayRadioCode:NightCrewVariant.Local;
            }
            s.ships=new[]{new NightShipEvent{id="convoy",nameKey="m3.ship.northernStar",sightedAtSeconds=0,approachSeconds=approachSeconds,windowSeconds=passageSeconds,darkGraceSeconds=30,safePreview=true,aimingTutorial=true}};
            s.cues=System.Array.Empty<NightRadioCue>();return d;
        }
        static NightFault Fault(string id,string station)=>new(){id=id,stationId=station,gate=NightEventGate.Clock,atSeconds=0};
    }
}
