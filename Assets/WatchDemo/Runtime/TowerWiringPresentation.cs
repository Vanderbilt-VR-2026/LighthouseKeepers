using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    // Presentation only: both peers read the accepted, host-owned PowerGrid snapshot.
    public sealed class TowerWiringPresentation : MonoBehaviour
    {
        WatchDemoController watch;TowerWiringAsset layout;Material material;
        readonly bool[] previous=new bool[5];readonly float[] since=new float[5];
        AudioSource hum;AudioClip humClip;ParticleSystem sparks;bool tripped;
        public static bool Create(WatchDemoController watch)
        {
            var data=Resources.Load<TowerWiringAsset>("TowerWiring/Layout");if(!data)return false;
            var p=watch.gameObject.AddComponent<TowerWiringPresentation>();p.watch=watch;p.layout=data;p.Build();return true;
        }
        void Build()
        {
            material=new Material(layout.material){name="Tower wiring / circuit state"};
            var root=new GameObject("Tower power wiring (baked routes)");root.transform.SetParent(transform,false);
            for(int i=0;i<layout.floors.Length;i++)
            {
                var go=new GameObject("Wiring floor "+i,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root.transform,false);go.isStatic=true;
                go.GetComponent<MeshFilter>().sharedMesh=layout.floors[i];var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
            }
            // One quiet nearby junction source, not one continuously playing voice per box.
            var h=new GameObject("Junction electrical hum");h.transform.SetParent(root.transform,false);hum=h.AddComponent<AudioSource>();
            hum.playOnAwake=false;hum.loop=true;hum.spatialBlend=1;hum.minDistance=.2f;hum.maxDistance=2.5f;hum.dopplerLevel=0;hum.priority=230;
            const int rate=22050;var samples=new float[rate];for(int i=0;i<rate;i++)samples[i]=.06f*Mathf.Sin(i*2*Mathf.PI*100/rate)+.02f*Mathf.Sin(i*2*Mathf.PI*200/rate);
            humClip=AudioClip.Create("Original synthesized 100 Hz junction hum",rate,1,rate,false);humClip.SetData(samples,0);hum.clip=humClip;KeeperAudioMix.Route(hum,"SFX_World");
            var s=new GameObject("Breaker arc sparks");s.transform.SetParent(root.transform,false);s.transform.position=layout.breaker;sparks=s.AddComponent<ParticleSystem>();sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=sparks.main;main.playOnAwake=false;main.loop=false;main.maxParticles=24;main.startLifetime=.22f;main.startSpeed=.7f;main.startSize=.012f;main.gravityModifier=.6f;main.startColor=new Color(1,.65f,.23f);
            var emission=sparks.emission;emission.enabled=false;var shape=sparks.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.02f;
            // Existing shared particle material; no second conduit material or realtime light.
            var render=sparks.GetComponent<ParticleSystemRenderer>();render.sharedMaterial=Resources.Load<Material>("TowerWiring/Sparks");
        }
        public static float RestoreDelay(float height)=>.18f+Mathf.Abs(Mathf.Clamp(Mathf.FloorToInt(height/3.3f)+1,1,4)-2)*.285f;
        public static bool Fed(WatchState state,int circuit)=>!state.Failed&&(circuit==4?state.Power==PowerState.On:state.HasPower((PowerCircuit)circuit));
        void LateUpdate()
        {
            var state=watch.State;Vector4 live=Vector4.zero,age=Vector4.zero;
            for(int i=0;i<5;i++){bool on=Fed(state,i);if(on&&!previous[i])since[i]=Time.time;previous[i]=on;if(i<4){live[i]=on?1:0;age[i]=Time.time-since[i];}}
            material.SetVector("_CircuitOn",live);material.SetVector("_CircuitAge",age);material.SetVector("_Feed",new Vector4(previous[4]?1:0,Time.time-since[4],layout.pulseSeconds,0));
            bool trip=state.Grid!=null&&state.Grid.Tripped;if(trip&&!tripped)sparks.Emit(18);tripped=trip; // panel already supplies the trip bang
            var camera=Camera.main;float nearest=float.MaxValue;Vector3 point=layout.breaker;
            if(camera)foreach(var p in layout.junctions){float d=(p-camera.transform.position).sqrMagnitude;if(d<nearest){nearest=d;point=p;}}
            hum.transform.position=point;bool audible=live.sqrMagnitude>0&&nearest<6.25f;hum.volume=audible?.12f:0;
            if(audible&&!hum.isPlaying)hum.Play();else if(!audible&&hum.isPlaying)hum.Stop();
        }
        void OnDestroy(){if(material)Destroy(material);if(humClip)Destroy(humClip);}
    }
}
