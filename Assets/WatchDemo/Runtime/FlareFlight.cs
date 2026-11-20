using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
using UnityEngine;
using UnityEngine.Rendering;
namespace LighthouseKeepers.WatchDemo {
    [DefaultExecutionOrder(2200)] public sealed class FlareFlight:MonoBehaviour {
        WatchDemoController watch;HorrorTuning tune;Material material;
        readonly LineRenderer[] core=new LineRenderer[2],halo=new LineRenderer[2],smoke=new LineRenderer[2];
        readonly double[] shots={-1,-1};readonly float[] priorAge=new float[2];readonly bool[] impactSeen=new bool[2];
        readonly Transform[] bursts=new Transform[2];readonly AudioSource[] sounds=new AudioSource[2];
        readonly ParticleSystem[] sparks=new ParticleSystem[2];Material sparkMaterial;
        Light flash;AudioClip impact,scream;Material burstMaterial;
        public static void Create(WatchDemoController w){var c=w.gameObject.AddComponent<FlareFlight>();c.watch=w;c.Build();}
        void Build(){
            tune=HorrorTuning.Current;material=new Material(Resources.Load<Shader>("FlareGlow"));burstMaterial=new Material(material);burstMaterial.SetColor("_BaseColor",new Color(1,.04f,.01f,.9f));
            sparkMaterial=new Material(material);sparkMaterial.SetFloat("_Radial",1);
            impact=Resources.Load<AudioClip>("S4Audio/bolt-clunk");scream=Resources.Load<AudioClip>("S5Audio/DrownedHorror/retreat");
            flash=new GameObject("Single temporary flare light").AddComponent<Light>();flash.transform.SetParent(transform,false);flash.type=LightType.Point;flash.shadows=LightShadows.None;flash.color=new Color(1,.08f,.015f);flash.range=tune.flareLightRange;flash.intensity=tune.flareLightIntensity;flash.enabled=false;
            for(int i=0;i<2;i++){
                core[i]=Line("Flare magnesium core "+i,.065f,new Color(1,.48f,.18f));halo[i]=Line("Flare red sparks "+i,.22f,new Color(1,.025f,.005f,.7f));smoke[i]=Line("Flare smoke trail "+i,.16f,new Color(.22f,.04f,.025f,.16f));
                var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Flare impact fire "+i;Destroy(g.GetComponent<Collider>());g.transform.SetParent(transform,false);g.GetComponent<Renderer>().sharedMaterial=burstMaterial;g.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;g.SetActive(false);bursts[i]=g.transform;
                var fx=new GameObject("Flare red magnesium sparks "+i);fx.transform.SetParent(transform,false);sparks[i]=fx.AddComponent<ParticleSystem>();sparks[i].Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=sparks[i].main;main.playOnAwake=false;main.loop=false;main.duration=1;main.startLifetime=new ParticleSystem.MinMaxCurve(.25f,.65f);main.startSpeed=new ParticleSystem.MinMaxCurve(1.5f,4);main.startSize=new ParticleSystem.MinMaxCurve(.035f,.09f);main.startColor=new Color(1,.12f,.015f);main.gravityModifier=1;main.maxParticles=24;main.simulationSpace=ParticleSystemSimulationSpace.World;var emission=sparks[i].emission;emission.enabled=false;var shape=sparks[i].shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.08f;var sr=fx.GetComponent<ParticleSystemRenderer>();sr.sharedMaterial=sparkMaterial;sr.shadowCastingMode=ShadowCastingMode.Off;sr.receiveShadows=false;
                sounds[i]=g.AddComponent<AudioSource>();sounds[i].playOnAwake=false;sounds[i].spatialBlend=1;sounds[i].minDistance=1;sounds[i].maxDistance=28;sounds[i].dopplerLevel=0;KeeperAudioMix.Route(sounds[i],"SFX_Critical");
            }
        }
        LineRenderer Line(string name,float width,Color colour){var g=new GameObject(name);g.transform.SetParent(transform,false);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=material;l.widthMultiplier=width;l.startColor=l.endColor=colour;l.positionCount=2;l.numCapVertices=4;l.shadowCastingMode=ShadowCastingMode.Off;l.receiveShadows=false;l.enabled=false;return l;}
        void LateUpdate(){
            flash.enabled=false;var s=watch.State;
            for(int i=0;i<2;i++){
                var shot=s.Flares?.Shot(i);if(shot==null||!shot.fired){core[i].enabled=halo[i].enabled=smoke[i].enabled=false;bursts[i].gameObject.SetActive(false);shots[i]=-1;continue;}
                float age=(float)(s.Flares.Clock-shot.at);if(shots[i]!=shot.at){shots[i]=shot.at;priorAge[i]=0;impactSeen[i]=false;}
                if(!s.ReadOnly&&!shot.stopped&&age>=0){Resolve(i,shot,Mathf.Min(priorAge[i],age),Mathf.Min(age,FlareState.FlightSeconds));shot=s.Flares.Shot(i);}
                priorAge[i]=age;bool alive=age>=0&&age<=(shot.stopped?shot.stopAge+FlareState.BurnSeconds:FlareState.FlightSeconds)&&!watch.Sleeping;
                core[i].enabled=halo[i].enabled=smoke[i].enabled=alive;if(!alive){bursts[i].gameObject.SetActive(false);continue;}
                var p=FlareState.Position(shot,age);var tail=shot.stopped?p+Vector3.up*.15f:FlareState.Position(shot,Mathf.Max(0,age-tune.flareTrailSeconds));
                core[i].SetPosition(0,Vector3.Lerp(tail,p,.8f));core[i].SetPosition(1,p);halo[i].SetPosition(0,tail);halo[i].SetPosition(1,p);smoke[i].SetPosition(0,tail+Vector3.up*.08f);smoke[i].SetPosition(1,p);
                float fade=shot.stopped?Mathf.Clamp01(1-(age-shot.stopAge)/FlareState.BurnSeconds):1;
                core[i].startColor=core[i].endColor=new Color(1,.48f,.18f,fade);halo[i].startColor=halo[i].endColor=new Color(1,.025f,.005f,fade*.7f);
                flash.enabled=true;flash.transform.position=p;flash.intensity=tune.flareLightIntensity*(shot.stopped?fade*.45f:1);
                if(shot.stopped){var burst=bursts[i];burst.gameObject.SetActive(true);burst.position=p;
                    float hitAge=age-shot.stopAge;burst.localScale=Vector3.one*(shot.repelled?Mathf.Lerp(.35f,.08f,Mathf.Clamp01(hitAge/tune.flareHitSeconds)):.16f*fade);
                    if(!impactSeen[i]){impactSeen[i]=true;if(hitAge<1){sparks[i].transform.position=p;sparks[i].Emit(shot.repelled?24:8);sounds[i].PlayOneShot(shot.repelled?scream:impact,shot.repelled?.75f:.4f);}}
                }
            }
            foreach(var t in FlareImpactTarget.ActiveTargets)if(t)t.FinishFrame();
        }
        public void Resolve(int slot,FlareShot shot,float from,float to){
            if(to<=from)return;float span=to-from,step=Mathf.Clamp(tune.flareSubstep,.001f,.05f);
            for(float t=from;t<to;t+=step){float end=Mathf.Min(to,t+step);var a=FlareState.Position(shot,t);var b=FlareState.Position(shot,end);
                bool hit=FlareCollision.Sweep(a,b,tune.flareSweepRadius,(t-from)/span,(end-from)/span,out var p,out var target);
                // Ocean plane also stops a miss; it must not disappear beneath invisible water geometry.
                if(!hit&&b.y<-.45f&&a.y>=-.45f){p=Vector3.Lerp(a,b,Mathf.InverseLerp(a.y,b.y,-.45f));hit=true;}
                if(hit||end>=FlareState.FlightSeconds){bool repelled=target&&target.Hit();watch.State.Flares.Stop(slot,hit?p:b,end,repelled);return;}
            }
        }
        void OnDestroy(){if(material)Destroy(material);if(burstMaterial)Destroy(burstMaterial);if(sparkMaterial)Destroy(sparkMaterial);}
    }
}
