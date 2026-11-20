using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Presentation.Environment;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    [DefaultExecutionOrder(200)]
    public sealed class EmergencyLighting : MonoBehaviour
    {
        [SerializeField] BeaconAndMainsLightingGroup bakedLighting;
        [SerializeField] Renderer[] interiorSurfaces;
        [SerializeField] Light[] interiorLights;
        [SerializeField] AudioSource alarm;
        [SerializeField,Min(2)] float cycleSeconds=2;
        [SerializeField,Min(1)] float recoverySeconds=1;
        WatchState state;
        Material[][] originals;
        readonly List<Material> materials=new();
        readonly List<Color> baseColours=new();readonly List<int> rooms=new();
        Color[] normalLightColours;float[] normalIntensities;
        float warm=1,outageStart,recoveryStart=-100;
        bool power=true,initialized;
        S4Tuning tuning;
        public float WarmBlend=>warm;
        public float CycleSeconds=>cycleSeconds;
        public static float PulseAt(float time,float cycle)=>.55f+.10f*Mathf.Sin(time*Mathf.PI*2/Mathf.Max(2,cycle));
        void Awake()
        {
            // Preserve the accepted bake/UVs. One set of shared runtime material instances
            // modulates baked illumination coherently; no per-renderer per-frame property blocks.
            bakedLighting.Powered=false;
            bakedLighting.ApplyNow();
            bakedLighting.enabled=false;
            originals=new Material[interiorSurfaces.Length][];
            var copies=new Dictionary<(Material,int),Material>();
            for(int i=0;i<interiorSurfaces.Length;i++)
            {
                var renderer=interiorSurfaces[i];if(!renderer)continue;
                int room=LanternRoomLighting.Room(renderer.bounds.center);var source=renderer.sharedMaterials;originals[i]=source;var assigned=(Material[])source.Clone();
                for(int j=0;j<source.Length;j++)
                {
                    var original=source[j];if(!original||!original.HasProperty("_BaseColor"))continue;
                    if(!copies.TryGetValue((original,room),out var copy))
                    {
                        copy=new Material(original){name=original.name+" / central emergency"};copies.Add((original,room),copy);materials.Add(copy);rooms.Add(room);
                        copy.EnableKeyword("_EMISSION");
                        baseColours.Add(original.GetColor("_BaseColor"));
                    }
                    assigned[j]=copy;
                }
                renderer.sharedMaterials=assigned;
            }
            normalLightColours=new Color[interiorLights.Length];normalIntensities=new float[interiorLights.Length];
            for(int i=0;i<interiorLights.Length;i++){normalLightColours[i]=interiorLights[i].color;normalIntensities[i]=interiorLights[i].intensity;}
        }
        public void Bind(WatchState value)
        {
            if(state!=null)state.Changed-=StateChanged;
            state=value;tuning=state.Tuning;state.Changed+=StateChanged;StateChanged();warm=power?1:0;initialized=true;Apply();
            if(tuning&&tuning.soundPass){alarm.spatialBlend=1;alarm.minDistance=3;alarm.maxDistance=18;}
            if(tuning&&tuning.blackout)
            {cycleSeconds=tuning.emergencyCycleSeconds;recoverySeconds=tuning.warmRecoverySeconds;Apply();}
        }
        void StateChanged()
        {
            bool next=state.HasPower(PowerCircuit.Lights)&&(!state.Immersion||!state.Failed);if(next==power&&initialized&&state.Elapsed>0)return;if(next&&!power)recoveryStart=Time.time;power=next;bakedLighting.Powered=power;bakedLighting.ApplyNow();
            if(!power){outageStart=Time.unscaledTime;alarm.volume=tuning&&tuning.soundPass?tuning.alarmVolume:.09f;}
            if(power)alarm.Stop();else if(!alarm.isPlaying)alarm.Play();
        }
        void LateUpdate()
        {
            if(!initialized)return;
            if(!power)alarm.volume=Mathf.Lerp(tuning&&tuning.soundPass?tuning.alarmVolume:.09f,tuning&&tuning.soundPass?tuning.alarmHumVolume:.02f,Mathf.SmoothStep(0,1,(Time.unscaledTime-outageStart)/(tuning&&tuning.soundPass?tuning.alarmFadeSeconds:10f)));
            warm=Mathf.MoveTowards(warm,power?1:0,Time.deltaTime/(power?recoverySeconds:.5f));Apply();
        }
        void Apply()
        {
            bool blackout=tuning&&tuning.blackout;
            float pulse=blackout?tuning.EmergencyPulse(Time.unscaledTime):PulseAt(Time.unscaledTime,cycleSeconds);
            var tint=blackout?tuning.emergencyTint:new Color(1,.82f,.68f,1);
            float atmosphere=state.Immersion&&state.Immersion.atmosphere?state.Immersion.interiorLightMultiplier:1;
            float sputter=state.FuelSputtering?.85f+.15f*Mathf.Cos((float)state.FuelRunoutRemaining*Mathf.PI*2/1.5f):1;
            float dread=state.Sprint5&&state.Sprint5.groundedHorror?DrownedMotion.LampDip(state.Elapsed,state.Day,2):1;
            atmosphere*=dread;
            // Illumination comes from the selected bake and carried lamps, not glowing wall materials.

            for(int i=0;i<materials.Count;i++)
            {
                var illumination=Color.white*((rooms[i]==0||power)?LanternRoomLighting.Filament(Time.time,rooms[i]):1)*(rooms[i]==0?1:sputter);
                var colour=baseColours[i]*illumination;colour.a=baseColours[i].a;materials[i].SetColor("_BaseColor",colour);
                if(materials[i].HasProperty("_EmissionColor"))
                {
                    materials[i].SetColor("_EmissionColor",Color.black);
                }
            }
            for(int i=0;i<interiorLights.Length;i++)
            {
                var light=interiorLights[i];light.color=Color.Lerp(blackout?tint:Color.Lerp(normalLightColours[i],new Color(1,.48f,.25f),.25f),normalLightColours[i],warm);
                float floorWarm=state.Director!=null&&power?Mathf.Clamp01((Time.time-recoveryStart-TowerWiringPresentation.RestoreDelay(light.transform.position.y))/recoverySeconds):warm;
                light.intensity=normalIntensities[i]*Mathf.Lerp(pulse,atmosphere,floorWarm)*sputter;
            }
        }
        void OnDestroy()
        {
            if(state!=null)state.Changed-=StateChanged;
            if(originals!=null)for(int i=0;i<interiorSurfaces.Length;i++)if(interiorSurfaces[i]&&originals[i]!=null)interiorSurfaces[i].sharedMaterials=originals[i];
            foreach(var m in materials)Destroy(m);
        }
    }
}
