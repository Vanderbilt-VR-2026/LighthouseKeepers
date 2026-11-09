using UnityEngine;
using UnityEngine.SceneManagement;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.WatchDemo
{
    // Runtime adaptation of the authored exterior (the lab scene is not re-saved): swaps the flat ocean
    // plane for a radial wave grid, adds one combined distant-coast mesh, and feeds the ocean/sky shaders
    // with storm intensity (watch day), lightning (existing flash lights) and the beacon beam.
    // Presentation only; reads gameplay state, never writes it.
    public sealed class StormSeascape : MonoBehaviour
    {
        public const string OceanName="Original storm ocean presentation";
        [SerializeField, Tooltip("Seconds to ease storm intensity to a new watch's level.")] float stormEaseSeconds=20;
        [SerializeField, Tooltip("Lightning envelope release (1/s). Attack is near-instant.")] float lightningRelease=2.5f;
        static readonly int Waves=Shader.PropertyToID("_LK_OceanWaves"),Storm=Shader.PropertyToID("_LK_StormIntensity"),Lightning=Shader.PropertyToID("_LK_Lightning"),
            LightningDir=Shader.PropertyToID("_LK_LightningDir"),BeamPos=Shader.PropertyToID("_LK_BeamPos"),BeamDir=Shader.PropertyToID("_LK_BeamDir");
        WatchDemoController watch;WatchBeacon beacon;Light gentleFlash,shellFlash;
        float storm,flash,nextSearch,searchUntil;Vector3 strikeDir=new Vector3(.5f,.55f,.65f).normalized;bool rising;
        Mesh ocean,coast;Material coastMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
        static void Loaded(Scene scene,LoadSceneMode mode)
        {
            if(!scene.isLoaded)return;
            foreach(var root in scene.GetRootGameObjects())
            {
                var water=root.name==OceanName?root.transform:root.transform.Find(OceanName);
                if(!water||water.GetComponentInParent<StormSeascape>())continue;
                (water.parent?water.parent.gameObject:water.gameObject).AddComponent<StormSeascape>().Build(water);return;
            }
        }
        // Public for Editor capture/inspection; idempotent per ocean object.
        public void Build(Transform water)
        {
            var filter=water.GetComponent<MeshFilter>();var renderer=water.GetComponent<MeshRenderer>();if(!filter||!renderer)return;
            ocean=StormSeascapeGeometry.BuildOcean();filter.sharedMesh=ocean;
            water.localScale=Vector3.one;water.rotation=Quaternion.identity;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            var mat=renderer.sharedMaterial;if(mat&&mat.HasProperty("_RingRatio"))mat.SetFloat("_RingRatio",StormSeascapeGeometry.RingRatio);
            var shader=Resources.Load<Shader>("DistantCoast");
            if(shader)
            {
                coast=StormSeascapeGeometry.BuildCoast();coastMaterial=new Material(shader){name="Distant coast (runtime)"};
                var go=new GameObject("Distant coast and inlet");go.transform.SetParent(transform,false);go.transform.position=new Vector3(0,water.position.y,0);
                go.AddComponent<MeshFilter>().sharedMesh=coast;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=coastMaterial;
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;r.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }
            else Debug.LogWarning("StormSeascape: DistantCoast shader missing from Resources; coast skipped.");
            Shader.SetGlobalFloat(Waves,1);searchUntil=Time.unscaledTime+90;
        }
        void Update()
        {
            // Lookups for presentation sources created after scene load (the watch builds its flash light later):
            // 1 Hz for the first 90 s, then every 10 s, and none once everything is found.
            if((!watch||!beacon||!gentleFlash)&&Time.unscaledTime>=nextSearch)
            {
                nextSearch=Time.unscaledTime+(Time.unscaledTime<searchUntil?1:10);
                if(!watch)watch=FindAnyObjectByType<WatchDemoController>();
                if(!beacon)beacon=FindAnyObjectByType<WatchBeacon>();
                if(!gentleFlash||!shellFlash)foreach(var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                {if(l.name=="Gentle lightning through windows")gentleFlash=l;else if(l.name=="Distant lightning flash")shellFlash=l;}
            }
            // Storm worsens each watch: day 1 -> 0, day 3 -> 1.
            float target=watch&&watch.State!=null?Mathf.Clamp01((watch.State.Day-1)/2f):0;
            storm=Mathf.MoveTowards(storm,target,Time.deltaTime/Mathf.Max(.1f,stormEaseSeconds));
            float raw=0;
            if(gentleFlash&&gentleFlash.isActiveAndEnabled)raw=Mathf.Max(raw,gentleFlash.intensity/.35f);
            if(shellFlash&&shellFlash.isActiveAndEnabled)raw=Mathf.Max(raw,shellFlash.intensity/1.2f);
            raw=Mathf.Clamp01(raw);
            // New strike: pick a fresh region of sky so flashes do not always bloom in the same place.
            if(raw>.15f&&!rising){rising=true;float az=Random.Range(0f,360f)*Mathf.Deg2Rad,el=Random.Range(14f,40f)*Mathf.Deg2Rad;strikeDir=new Vector3(Mathf.Sin(az)*Mathf.Cos(el),Mathf.Sin(el),Mathf.Cos(az)*Mathf.Cos(el));}
            else if(raw<.05f)rising=false;
            // Fast attack, smooth release: flash lights' hard 70 ms steps never strobe the whole sky.
            flash=raw>flash?Mathf.MoveTowards(flash,raw,Time.deltaTime*12):Mathf.MoveTowards(flash,raw,Time.deltaTime*lightningRelease);
            Shader.SetGlobalFloat(Storm,storm);Shader.SetGlobalFloat(Lightning,flash);Shader.SetGlobalVector(LightningDir,strikeDir);
            if(beacon&&beacon.beam&&beacon.beam.isActiveAndEnabled&&beacon.beam.intensity>0)
            {
                var t=beacon.beam.transform;var p=t.position;
                Shader.SetGlobalVector(BeamPos,new Vector4(p.x,p.y,p.z,Mathf.Clamp01(beacon.beam.intensity/6)));
                var f=t.forward;Shader.SetGlobalVector(BeamDir,new Vector4(f.x,f.y,f.z,Mathf.Cos(beacon.beam.spotAngle*.5f*Mathf.Deg2Rad)));
            }
            else Shader.SetGlobalVector(BeamPos,Vector4.zero);
        }
        void OnDestroy()
        {
            Shader.SetGlobalFloat(Waves,0);Shader.SetGlobalFloat(Lightning,0);Shader.SetGlobalVector(BeamPos,Vector4.zero);
            Release(ocean);Release(coast);Release(coastMaterial);
        }
        static void Release(Object o){if(!o)return;if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
    }
}
