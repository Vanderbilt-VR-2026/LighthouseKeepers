using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.WatchDemo
{
    [DefaultExecutionOrder(200)]
    public sealed class EmergencyLighting : MonoBehaviour
    {
        [SerializeField] BeaconAndMainsLightingGroup bakedLighting;
        [SerializeField] Renderer[] interiorSurfaces;
        [SerializeField] Light[] interiorLights;
        [SerializeField] AudioSource alarm;
        [SerializeField,Min(1.5f)] float cycleSeconds=1.5f;
        [SerializeField,Min(1)] float recoverySeconds=1;
        WatchState state;
        Material[][] originals;
        readonly List<Material> materials=new();
        readonly List<Color> baseColours=new(), emissions=new();
        Color[] normalLightColours;float[] normalIntensities;
        float warm=1;
        bool power=true,initialized;
        public float WarmBlend=>warm;
        public float CycleSeconds=>cycleSeconds;
        public static float PulseAt(float time,float cycle)=>.52f+.20f*Mathf.Sin(time*Mathf.PI*2/Mathf.Max(1.5f,cycle));
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
            state=value;state.Changed+=StateChanged;StateChanged();warm=power?1:0;initialized=true;Apply();
        }
        void StateChanged()
        {
            bool next=state.Power==PowerState.On;if(next==power&&initialized)return;power=next;
            if(power)alarm.Stop();else if(!alarm.isPlaying)alarm.Play();
        }
        void LateUpdate()
        {
            if(!initialized)return;
            if(power&&warm>=1)return;
            warm=Mathf.MoveTowards(warm,power?1:0,Time.deltaTime/(power?recoverySeconds:.5f));Apply();
        }
        void Apply()
        {
            float pulse=PulseAt(Time.unscaledTime,cycleSeconds);
            var illumination=Color.Lerp(new Color(pulse,.035f,.02f,1),Color.white,Mathf.SmoothStep(0,1,warm));
            for(int i=0;i<materials.Count;i++)
            {
                var colour=baseColours[i]*illumination;colour.a=baseColours[i].a;materials[i].SetColor("_BaseColor",colour);
                if(materials[i].HasProperty("_EmissionColor"))
                {
                    var e=emissions[i];float strength=Mathf.Max(e.r,Mathf.Max(e.g,e.b));
                    materials[i].SetColor("_EmissionColor",Color.Lerp(new Color(Mathf.Max(strength*pulse,.035f+.05f*pulse),strength*.015f+.0015f,strength*.008f+.001f),e,warm));
                }
            }
            for(int i=0;i<interiorLights.Length;i++)
            {
                var light=interiorLights[i];light.color=Color.Lerp(new Color(1,.035f,.015f),normalLightColours[i],warm);
                light.intensity=normalIntensities[i]*Mathf.Lerp(.45f+pulse,1,warm);
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
