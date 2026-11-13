using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.WatchDemo
{
    // Presentation reads authored ship timing and the already replicated watch clock.
    // Outcome state/ship loss gates are step2.2; until then runtime presentation is disabled.
    public sealed class ShipFleetPresentation:MonoBehaviour
    {
        WatchDemoController watch;SteamerView[] ships;int day;float sea=-.45f,shownClock;
        public static ShipFleetPresentation Create(WatchDemoController owner)
        {
            if(!owner.State.Campaign||!owner.State.Sprint5||!owner.State.Sprint5.shipVisuals)return null;
            var p=owner.gameObject.AddComponent<ShipFleetPresentation>();p.watch=owner;p.ResetFleet();return p;
        }
        void ResetFleet()
        {
            if(ships!=null)foreach(var ship in ships)if(ship)Destroy(ship.gameObject);
            day=watch.State.Day;shownClock=(float)watch.State.Elapsed;var events=watch.State.Scenario.ships;ships=new SteamerView[events.Length];
            var ocean=GameObject.Find(StormSeascape.OceanName);if(ocean)sea=ocean.transform.position.y;
            for(int i=0;i<ships.Length;i++)ships[i]=Spawn(events[i]);
        }
        public static SteamerView Spawn(NightShipEvent passage)
        {
            var prefab=Resources.Load<SteamerView>("Sprint5Ships/CoastalSteamer");if(!prefab)throw new System.InvalidOperationException("Bake ship assets using Lighthouse Keepers/Ships/Bake coastal steamer");
            var view=Instantiate(prefab);view.Initialize(PlayerStrings.Get(passage.nameKey));if(passage.largeFinale)view.transform.localScale=Vector3.one*1.25f;return view;
        }
        void Update()
        {
            if(watch.State.Day!=day)ResetFleet();var events=watch.State.Scenario.ships;float elapsed=(float)watch.State.Elapsed;
            shownClock=Mathf.Abs(elapsed-shownClock)>.75f?elapsed:Mathf.Lerp(shownClock,elapsed,1-Mathf.Exp(-Time.deltaTime/.12f));
            for(int i=0;i<ships.Length;i++){
                float age=shownClock-events[i].sightedAtSeconds;bool visible=age>=0&&age<=events[i].approachSeconds+events[i].windowSeconds+55;
                ships[i].gameObject.SetActive(visible);if(visible)ships[i].Sample(age,events[i].approachSeconds,events[i].windowSeconds,55,(watch.State.Storm.Stage-1)/3f,sea);
            }
        }
        void OnDestroy(){if(ships!=null)foreach(var ship in ships)if(ship)Destroy(ship.gameObject);}
    }
}
