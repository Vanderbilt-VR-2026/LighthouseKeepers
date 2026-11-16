using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class FlareFlight : MonoBehaviour
    {
        WatchDemoController watch; readonly LineRenderer[] trails=new LineRenderer[2], halos=new LineRenderer[2];
        readonly float[] priorAge=new float[2]; readonly double[] shots={-1,-1}; Material material;
        public static void Create(WatchDemoController w){var c=w.gameObject.AddComponent<FlareFlight>();c.watch=w;c.Build();}
        void Build(){material=new Material(Resources.Load<Shader>("FlareGlow"));for(int i=0;i<2;i++){trails[i]=Line("Flare hot core "+i,.022f,new Color(1,.48f,.15f));halos[i]=Line("Flare ember halo "+i,.13f,new Color(.7f,.08f,.015f));}}
        LineRenderer Line(string name,float width,Color colour){var g=new GameObject(name);g.transform.SetParent(transform,false);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=material;l.widthMultiplier=width;l.startColor=l.endColor=colour;l.positionCount=2;l.numCapVertices=4;l.shadowCastingMode=ShadowCastingMode.Off;l.receiveShadows=false;l.enabled=false;return l;}
        void Update()
        {
            var state=watch.State;
            for(int i=0;i<2;i++){
                var shot=state.Flares?.Shot(i);
                if(shot==null||!shot.fired){trails[i].enabled=halos[i].enabled=false;shots[i]=-1;continue;}
                float age=(float)(state.Elapsed-shot.at);
                if(shots[i]!=shot.at){shots[i]=shot.at;priorAge[i]=0;}
                if(!state.ReadOnly&&!shot.stopped&&age>=0){Resolve(i,shot,Mathf.Min(priorAge[i],age),Mathf.Min(age,FlareState.FlightSeconds));shot=state.Flares.Shot(i);}
                priorAge[i]=age;
                bool alive=age>=0&&age<=(shot.stopped?shot.stopAge+FlareState.BurnSeconds:FlareState.FlightSeconds)&&!watch.Sleeping;
                trails[i].enabled=halos[i].enabled=alive;if(!alive)continue;
                var p=FlareState.Position(shot,age);var tail=shot.stopped?p+Vector3.up*.06f:FlareState.Position(shot,Mathf.Max(0,age-.035f));
                trails[i].SetPosition(0,tail);trails[i].SetPosition(1,p);halos[i].SetPosition(0,tail);halos[i].SetPosition(1,p);
                float fade=shot.stopped?Mathf.Clamp01(1-(age-shot.stopAge)/FlareState.BurnSeconds):1;
                trails[i].startColor=trails[i].endColor=new Color(1,.48f,.15f,fade);halos[i].startColor=halos[i].endColor=new Color(.7f,.08f,.015f,fade*.65f);
            }
        }
        void Resolve(int slot,FlareShot shot,float from,float to)
        {
            if(to<=from)return;
            // Substep curved flight to prevent tunnelling and preserve walls at low frame rates.
            for(float t=from;t<to;t+=.04f){float end=Mathf.Min(to,t+.04f);var a=FlareState.Position(shot,t);var b=FlareState.Position(shot,end);var delta=b-a;bool blocked=false;float nearest=delta.magnitude;
                foreach(var hit in Physics.SphereCastAll(a,.025f,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore)){
                    if(hit.collider.GetComponentInParent<WatchFlareGun>()||hit.collider.GetComponentInParent<HandRig>()||hit.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())continue;
                    if(hit.distance<nearest){nearest=hit.distance;blocked=true;}
                }
                if(blocked)b=a+delta.normalized*nearest;
                var d=watch.State.Drowned;
                bool struck=d!=null&&d.Anchored&&d.Phase==DrownedPhase.Manifest&&FlareState.NearSegment(a,b,new Vector3(d.X,d.Y+DrownedForms.Height(DrownedForms.Select(watch.State.Day,d.Fault))*.58f,d.Z),.60f);
                if(struck){bool repelled=watch.State.RepelWithFlare();watch.State.Flares.Stop(slot,b,end,repelled);return;}
                if(blocked||end>=FlareState.FlightSeconds){watch.State.Flares.Stop(slot,b,end,false);return;}
            }
        }
        void OnDestroy(){if(material)Destroy(material);}
    }
}
