using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.WatchDemo
{
    // Only renders the host clock/outcomes; no client can save or wreck a ship locally.
    public sealed class ShipFleetPresentation : MonoBehaviour
    {
        WatchDemoController watch; SteamerView[] ships; AudioSource[] voices,engines; int[] hornBeat; bool[] windowCue; ImmersionAtmosphere atmosphere;
        AudioClip horn,thanks,crash,engine; int day,seen,passed,wreck=-1;float sea=-.45f,shownClock,wreckAge;
        public static ShipFleetPresentation Create(WatchDemoController owner)
        {
            if(!owner.State.Campaign||!owner.State.Sprint5||!owner.State.Sprint5.shipVisuals)return null;
            if(owner.TryGetComponent<ShipFleetPresentation>(out var existing))return existing;
            var p=owner.gameObject.AddComponent<ShipFleetPresentation>();p.watch=owner;
            p.horn=Resources.Load<AudioClip>("Sprint5Ships/foghorn");p.thanks=Resources.Load<AudioClip>("Sprint5Ships/thanks");p.crash=Resources.Load<AudioClip>("Sprint5Ships/wreck");p.engine=Resources.Load<AudioClip>("Sprint5Ships/engine");p.atmosphere=owner.GetComponent<ImmersionAtmosphere>();p.ResetFleet();return p;
        }
        void ResetFleet()
        {
            if(ships!=null)foreach(var ship in ships)if(ship)Destroy(ship.gameObject);
            day=watch.State.Day;seen=passed=0;wreck=-1;wreckAge=0;shownClock=(float)watch.State.Elapsed;
            var events=watch.State.Scenario.ships;ships=new SteamerView[events.Length];voices=new AudioSource[events.Length];engines=new AudioSource[events.Length];hornBeat=new int[events.Length];windowCue=new bool[events.Length];
            var ocean=GameObject.Find(StormSeascape.OceanName);if(ocean)sea=ocean.transform.position.y;
            for(int i=0;i<ships.Length;i++){
                ships[i]=Spawn(events[i]);var go=new GameObject("Spatial ship horn and crash");go.transform.SetParent(ships[i].transform,false);go.transform.localPosition=Vector3.up*5;
                var a=go.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=1;a.minDistance=25;a.maxDistance=320;a.rolloffMode=AudioRolloffMode.Linear;a.dopplerLevel=0;a.volume=.35f;a.priority=45;KeeperAudioMix.Route(a,"SFX_Critical");voices[i]=a;hornBeat[i]=-1;
                var engineObject=new GameObject("Nearby steamer engine and hull creak");engineObject.transform.SetParent(ships[i].transform,false);
                var e=engineObject.AddComponent<AudioSource>();e.playOnAwake=false;e.loop=true;e.clip=engine;e.spatialBlend=1;e.minDistance=12;e.maxDistance=110;e.rolloffMode=AudioRolloffMode.Linear;e.dopplerLevel=0;e.volume=.16f;e.priority=115;KeeperAudioMix.Route(e,"SFX_World");engines[i]=e;
            }
        }
        public static SteamerView Spawn(NightShipEvent passage)
        {
            var prefab=Resources.Load<SteamerView>("Sprint5Ships/CoastalSteamer");if(!prefab)throw new System.InvalidOperationException("Bake ship assets using Lighthouse Keepers/Ships/Bake coastal steamer");
            var view=Instantiate(prefab);view.Initialize(PlayerStrings.Get(passage.nameKey));if(passage.largeFinale)view.transform.localScale=Vector3.one*1.25f;return view;
        }
        void Update()
        {
            if(watch.State.Day!=day||watch.State.Elapsed<shownClock-.75f)ResetFleet();
            var events=watch.State.Scenario.ships;var model=watch.State.Ships;float elapsed=(float)watch.State.Elapsed;
            shownClock=Mathf.Abs(elapsed-shownClock)>.75f?elapsed:Mathf.Lerp(shownClock,elapsed,1-Mathf.Exp(-Time.deltaTime/.12f));
            for(int i=0;i<ships.Length;i++){
                float approach=model==null?events[i].approachSeconds:(float)model.Approach(i);
                bool wrecked=model!=null&&model.WreckIndex==i;
                float age=(wrecked?(float)model.WreckAt:shownClock)-events[i].sightedAtSeconds;
                bool visible=age>=0&&age<=approach+events[i].windowSeconds+55;
                ships[i].gameObject.SetActive(visible);if(!visible)continue;
                if(!wrecked&&!engines[i].isPlaying&&engine)engines[i].Play();
                if(wrecked)engines[i].Stop();
                // A bounded approach call every 24 simulation seconds, spatially carried by the vessel.
                int call=(int)(age/24);if(!wrecked&&age<approach+events[i].windowSeconds&&call!=hornBeat[i]){hornBeat[i]=call;if(age%24<3)Play(i,horn);}
                if(!windowCue[i]&&age>=approach){windowCue[i]=true;if(age-approach<3&&atmosphere)atmosphere.ShipPassageLightning();}
                ships[i].Sample(age,approach,events[i].windowSeconds,55,((watch.State.Storm?.Stage??1)-1)/3f,sea);
                if(wrecked){if(wreck!=i){wreck=i;wreckAge=0;Play(i,crash);}wreckAge+=Time.deltaTime;ships[i].Wreck(wreckAge);}
                int bit=1<<i;
                if(model!=null&&(model.SeenMask&bit)!=0&&(seen&bit)==0){seen|=bit;}
                if(model!=null&&(model.PassedMask&bit)!=0&&(passed&bit)==0){passed|=bit;Play(i,thanks);}
            }
        }
        void Play(int i,AudioClip clip){if(clip){voices[i].Stop();voices[i].clip=clip;voices[i].Play();}}
        void OnDestroy(){if(ships!=null)foreach(var ship in ships)if(ship)Destroy(ship.gameObject);}
    }
}
