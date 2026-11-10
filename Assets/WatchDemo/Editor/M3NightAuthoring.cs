using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo.Editor
{
    public static class M3NightAuthoring
    {
        const string Folder="Assets/Data/M3Days",Catalog="Assets/WatchDemo/Resources/M3Campaign.asset";
        static NightStationRule Station(string id,NightCrewVariant variant,string site,params string[] steps)=>new NightStationRule{stationId=id,crewVariant=variant,informationSite=site,activeSteps=steps};
        static NightFault Fault(string id,string station,float time,NightEventGate gate=NightEventGate.Clock,string after=null,bool repeat=false)=>new NightFault{id=id,stationId=station,atSeconds=time,gate=gate,afterStationId=after,graceSeconds=60,repeat=repeat};
        static NightShipEvent Ship(string id,string name,float at,float lead=75,float window=45,bool preview=false,bool large=false,float extension=0)=>new NightShipEvent{id=id,nameKey=name,sightedAtSeconds=at,approachSeconds=lead,windowSeconds=window,safePreview=preview,largeFinale=large,broadcastExtensionSeconds=extension};
        [MenuItem("Lighthouse Keepers/M3/Create missing night data")]
        public static void Seed()
        {
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Data","M3Days");
            var catalog=AssetDatabase.LoadAssetAtPath<FiveNightCatalog>(Catalog);
            if(!catalog){catalog=ScriptableObject.CreateInstance<FiveNightCatalog>();AssetDatabase.CreateAsset(catalog,Catalog);}
            var days=new List<DayDefinition>();
            for(int n=1;n<=5;n++)days.Add(Create(n,false));
            catalog.nights=days.ToArray();catalog.demo=Create(3,true);EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        }
        static DayDefinition Create(int n,bool demo)
        {
            string path=Folder+"/"+(demo?"DemoNight":$"Night{n:00}")+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<DayDefinition>(path);if(existing)return existing;
            var source=AssetDatabase.LoadAssetAtPath<DayDefinition>($"Assets/Data/S4M2Days/Day{Math.Min(n,3):00}.asset");
            var d=UnityEngine.Object.Instantiate(source);d.name=demo?"DemoNight":$"Night{n:00}";d.day=n;
            var p=d.scenario=new NightScenario{configured=true,id=demo?"demo":$"night-{n}",nameKey=demo?"m3.demo":$"m3.night{n}",demo=demo,noFail=n==1&&!demo,stormBaseStage=n<=2?1:n==3?2:3,peakStormAfterFraction=n==5?.5f:1,finaleFaultPileup=n==5,openingCue=n==1?1:n==2?13:n==3?76:n==4?94:95,tonightCue=n==2?80:n>=3?81:0};
            // Inherit accepted watch lengths; later nights initially use the existing all-jobs
            // solo envelope. These are explicitly NOT approved M3 timings until A4 measurements.
            p.soloSeconds=demo?360:n<=2?315:n==3?515:n==4?540:760;p.crewSeconds=demo?360:n<=2?315:n==3?510:n==4?510:730;
            if(n<3){p.soloFloodMultiplier=p.crewFloodMultiplier=1;}
            var stations=new List<NightStationRule>{Station(WatchState.PipeStation,n>=3&&!demo?NightCrewVariant.HoldValve:NightCrewVariant.Local,"F1","clamp","tighten")};
            if(n>=2||demo)stations.Add(Station(WatchState.GeneratorStation,n==2||n==5?NightCrewVariant.RelayGenerator:NightCrewVariant.Local,"House","fuel","prime","choke-close","starter","choke-open","breakers"));
            if(n>=3&&!demo)stations.Add(Station(WatchState.RadioStation,NightCrewVariant.RelayRadioCode,"House","power","tune","code","transmit"));
            if(n>=3||demo)stations.Add(Station(WatchState.BeaconStation,n>=4?NightCrewVariant.RelayBeaconBearing:NightCrewVariant.Local,"F4 radio",n>=4?new[]{"ignite","align","wipe"}:new[]{"ignite","align"}));
            if(n>=4){d.fuelDrainPerSecond=.0015f;p.refillTargetFraction=.65f;var generator=stations.Find(s=>s.stationId==WatchState.GeneratorStation);generator.activeSteps=generator.activeSteps.Concat(new[]{"refill"}).ToArray();}
            p.stations=stations.ToArray();
            var faults=new List<NightFault>{Fault("leak",WatchState.PipeStation,0)};
            if(n>=2||demo)faults.Add(Fault("outage",WatchState.GeneratorStation,75,n>=3?NightEventGate.ClockOrJobComplete:NightEventGate.Clock,WatchState.PipeStation));
            if(n>=3||demo)faults.Add(Fault("beacon",WatchState.BeaconStation,0,NightEventGate.PowerRestored));
            if(n>=3&&!demo)faults.Add(Fault("broadcast",WatchState.RadioStation,0,NightEventGate.PowerRestored));
            if(n>=4){var low=Fault("fuel-low",WatchState.GeneratorStation,170,repeat:true);low.kind=NightFaultKind.LowFuel;low.fuelFraction=.18f;faults.Add(low);}
            if(n==5){faults.Add(Fault("final-outage",WatchState.GeneratorStation,225,repeat:true));faults.Add(Fault("final-rupture",WatchState.PipeStation,225,repeat:true));}
            p.faults=faults.ToArray();
            p.ships=demo?new[]{Ship("demo-passage","m3.ship.maryEllen",210,75,55)}:
                n==1?new[]{Ship("preview","m3.ship.maryEllen",180,75,45,true)}:
                n==2?new[]{Ship("blackout-passage","m3.ship.maryEllen",175,75,55)}:
                n==3?new[]{Ship("first-passage","m3.ship.northStar",90),Ship("warned-passage","m3.ship.maryEllen",210,60,45,extension:15)}:
                n==4?new[]{Ship("maintenance-first","m3.ship.northStar",120,60),Ship("maintenance-second","m3.ship.maryEllen",230,60)}:
                new[]{Ship("finale-first","m3.ship.northStar",75,60),Ship("finale-second","m3.ship.maryEllen",185,60),Ship("finale-large","m3.ship.meridian",300,75,70,large:true)};
            p.cues=new[]{new NightRadioCue{eventId="ship-sighted",cue=96},new NightRadioCue{eventId="ship-close",cue=97},new NightRadioCue{eventId="ship-passed",cue=98},new NightRadioCue{eventId="ship-wrecked",cue=99},new NightRadioCue{eventId="rescue",cue=100}};
            if(demo){d.tasks=d.tasks.Where(t=>t.stationId!=WatchState.RadioStation).ToArray();d.startingFaults=d.startingFaults.Where(t=>t!=WatchState.RadioStation).ToArray();}
            d.dayLengthSeconds=p.soloSeconds;
            var errors=p.Validate().ToArray();if(errors.Length>0)throw new InvalidOperationException(path+": "+string.Join("; ",errors));
            AssetDatabase.CreateAsset(d,path);return d;
        }
    }
}
