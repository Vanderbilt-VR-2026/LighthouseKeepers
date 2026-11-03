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
        readonly List<Color> baseColours=new(), emissions=new();
        Color[] normalLightColours;float[] normalIntensities;
        float warm=1,outageStart;
        bool power=true,initialized;
        S4Tuning tuning;
        readonly List<Light> guides=new();
        public float WarmBlend=>warm;
        public float CycleSeconds=>cycleSeconds;
        public static float PulseAt(float time,float cycle)=>.55f+.10f*Mathf.Sin(time*Mathf.PI*2/Mathf.Max(2,cycle));
        void Awake()
        {
            // Preserve the accepted bake/UVs. One set of shared runtime material instances
            // modulates baked illumination coherently; no per-renderer per-frame property blocks.
            bakedLighting.Powered=true;
            bakedLighting.ApplyNow();
            bakedLighting.enabled=false;
            originals=new Material[interiorSurfaces.Length][];
            var copies=new Dictionary<Material,Material>();
            for(int i=0;i<interiorSurfaces.Length;i++)
            {
                var renderer=interiorSurfaces[i];if(!renderer)continue;
                var source=renderer.sharedMaterials;originals[i]=source;var assigned=(Material[])source.Clone();
                for(int j=0;j<source.Length;j++)
                {
                    var original=source[j];if(!original||!original.HasProperty("_BaseColor"))continue;
                    if(!copies.TryGetValue(original,out var copy))
                    {
                        copy=new Material(original){name=original.name+" / central emergency"};copies.Add(original,copy);materials.Add(copy);
                        copy.EnableKeyword("_EMISSION");
                        baseColours.Add(original.GetColor("_BaseColor"));emissions.Add(original.HasProperty("_EmissionColor")?original.GetColor("_EmissionColor"):Color.black);
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
            {cycleSeconds=tuning.emergencyCycleSeconds;recoverySeconds=tuning.warmRecoverySeconds;CreateGuides();Apply();}
        }
        void StateChanged()
        {
            bool next=state.HasPower(PowerCircuit.Lights);if(next==power&&initialized&&state.Elapsed>0)return;power=next;
            if(!power){outageStart=Time.unscaledTime;alarm.volume=tuning&&tuning.soundPass?tuning.alarmVolume:.09f;}
            if(power)alarm.Stop();else if(!alarm.isPlaying)alarm.Play();
        }
        void LateUpdate()
        {
            if(!initialized)return;
            if(power&&warm>=1)return;
            if(!power)alarm.volume=Mathf.Lerp(tuning&&tuning.soundPass?tuning.alarmVolume:.09f,tuning&&tuning.soundPass?tuning.alarmHumVolume:.02f,Mathf.SmoothStep(0,1,(Time.unscaledTime-outageStart)/(tuning&&tuning.soundPass?tuning.alarmFadeSeconds:10f)));
            warm=Mathf.MoveTowards(warm,power?1:0,Time.deltaTime/(power?recoverySeconds:.5f));Apply();
        }
        void Apply()
        {
            bool blackout=tuning&&tuning.blackout;
            float pulse=blackout?tuning.EmergencyPulse(Time.unscaledTime):PulseAt(Time.unscaledTime,cycleSeconds);
            var tint=blackout?tuning.emergencyTint:new Color(1,.82f,.68f,1);
            var illumination=Color.Lerp(tint*pulse,Color.white,Mathf.SmoothStep(0,1,warm));
            for(int i=0;i<materials.Count;i++)
            {
                var colour=baseColours[i]*illumination;colour.a=baseColours[i].a;materials[i].SetColor("_BaseColor",colour);
                if(materials[i].HasProperty("_EmissionColor"))
                {
                    var e=emissions[i];float strength=Mathf.Max(e.r,Mathf.Max(e.g,e.b));
                    var emergency=blackout?tint*strength*pulse+baseColours[i]*.003f:new Color(1,.78f,.58f)*strength*pulse+baseColours[i]*(.025f+.025f*pulse);
                    materials[i].SetColor("_EmissionColor",Color.Lerp(emergency,e,warm));
                }
            }
            for(int i=0;i<interiorLights.Length;i++)
            {
                var light=interiorLights[i];light.color=Color.Lerp(blackout?tint:Color.Lerp(normalLightColours[i],new Color(1,.48f,.25f),.25f),normalLightColours[i],warm);
                light.intensity=normalIntensities[i]*Mathf.Lerp(pulse,1,warm);
            }
            foreach(var guide in guides)guide.intensity=tuning.guideLampIntensity*(1-warm)*(blackout?pulse/Mathf.Max(.01f,tuning.emergencyMaximum):1);
        }
        void CreateGuides()
        {
            if(guides.Count>0)return;
            // Frozen Sprint 2 side-stair walk lines from lighthouse-layout.json. Small,
            // shadowless lights sit beside the route; no geometry or colliders on the walkway.
            for(int floor=0;floor<3;floor++)
            {
                float radius=floor==2?1.7f:3f;
                AddGuide(Polar(320,radius,floor*3.3f+.35f),"stairs F"+(floor+1));
                AddGuide(Polar(floor==2?140:220,radius,(floor+1)*3.3f+.35f),"landing F"+(floor+2));
            }
            var generator=GameObject.Find("STN_GENERATOR");
            if(generator)AddGuide(generator.transform.TransformPoint(new Vector3(.6f,1.35f,.8f)),"generator");
        }
        static Vector3 Polar(float angle,float radius,float y)=>new Vector3(Mathf.Sin(angle*Mathf.Deg2Rad)*radius,y,Mathf.Cos(angle*Mathf.Deg2Rad)*radius);
        void AddGuide(Vector3 position,string label)
        {
            var go=new GameObject("S4 red guide light "+label);go.transform.SetParent(transform,false);
            go.transform.position=position;
            var light=go.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.06f,.025f);
            light.range=tuning.guideLampRangeM;light.shadows=LightShadows.None;guides.Add(light);
        }
        void OnDestroy()
        {
            if(state!=null)state.Changed-=StateChanged;
            if(originals!=null)for(int i=0;i<interiorSurfaces.Length;i++)if(interiorSurfaces[i]&&originals[i]!=null)interiorSurfaces[i].sharedMaterials=originals[i];
            foreach(var m in materials)Destroy(m);
        }
    }
}
