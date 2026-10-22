using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    /// <summary>Read-only scene feedback. Never changes the authoritative water, power or progression.</summary>
    public sealed class SnapshotWaterView : MonoBehaviour
    {
        FusionSimulationAdapter simulation;
        Transform surface;
        Renderer water;
        Renderer[] beacon;
        Light beaconLight;
        MonoBehaviour lighting;
        PropertyInfo powered;
        bool appliedPower,lastPower;
        public void Bind(FusionSimulationAdapter state)
        {
            simulation=state;
            var mesh=GameObject.Find("SURFACE_WaterF1");
            if(!mesh)throw new InvalidOperationException("Ship F1 is missing SURFACE_WaterF1 for authoritative water feedback.");
            surface=mesh.transform;water=mesh.GetComponent<Renderer>();
            var lantern=GameObject.Find("STN_BEACON_LANTERN");
            if(!lantern)throw new InvalidOperationException("Ship F4 is missing STN_BEACON_LANTERN.");
            beacon=lantern.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("Amber",StringComparison.Ordinal)).ToArray();
            beaconLight=lantern.GetComponentInChildren<Light>(true);
            lighting=FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).FirstOrDefault(c=>c && c.GetType().Name=="Round2LightingController");
            powered=lighting?lighting.GetType().GetProperty("Powered"):null;
        }
        void LateUpdate()
        {
            if(!simulation || !surface)return;
            var state=simulation.Current;var position=surface.position;position.y=simulation.PresentationWaterHeight;surface.position=position;
            if(water)water.enabled=state.WaterVolumeM3>0;
            bool lit=state.GeneratorRunning && state.PowerAvailable>.5f;
            if(!appliedPower || lastPower!=lit)
            {
                foreach(var renderer in beacon)if(renderer)renderer.enabled=lit;
                if(beaconLight)beaconLight.enabled=lit;
                if(lighting && powered!=null)powered.SetValue(lighting,lit);
                lastPower=lit;appliedPower=true;
            }
        }
    }
}
