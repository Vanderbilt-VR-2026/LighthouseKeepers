using System.Collections.Generic;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Lobby;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace LighthouseKeepers.WatchDemo
{
    // Two shadowless spotlights replace the two carried flashlights. Only the host
    // applies exposure; head poses already travel through the existing crew stream.
    [DefaultExecutionOrder(700)]
    public sealed class KeeperHeadlamps : MonoBehaviour
    {
        WatchDemoController watch;
        Transform head;
        Light local, partner;
        Texture2D cookie;
        CrewCompanion companion;
        readonly RaycastHit[] hits = new RaycastHit[24];
        readonly int[] lastActor = new int[2];

        public IEnumerable<Light> Lights { get { if (local) yield return local; if (partner) yield return partner; } }
        public static KeeperHeadlamps Create(WatchDemoController w, Transform trackedHead)
        {
            var c = w.gameObject.AddComponent<KeeperHeadlamps>();
            c.watch = w; c.head = trackedHead; c.Build(); return c;
        }
        void Build()
        {
            cookie = new Texture2D(64,64,TextureFormat.RGBA32,false) { name="Helmet lamp reflector", wrapMode=TextureWrapMode.Clamp, filterMode=FilterMode.Bilinear };
            var pixels = new Color[64*64];
            for (int y=0;y<64;y++) for (int x=0;x<64;x++) {
                float u=(x+.5f)/32-1, v=(y+.5f)/32-1;
                float edge=1-Mathf.SmoothStep(.45f,1,Mathf.Sqrt(u*u+v*v));
                pixels[y*64+x]=new Color(edge,edge,edge,edge);
            }
            cookie.SetPixels(pixels); cookie.Apply(false,true);
            local=Lamp("Your helmet beam",head); local.transform.localPosition=new Vector3(0,.10f,.04f);
            partner=Lamp("Other keeper helmet beam",transform); partner.enabled=false;
        }
        Light Lamp(string label,Transform parent)
        {
            var l=new GameObject(label).AddComponent<Light>(); l.transform.SetParent(parent,false);
            l.type=LightType.Spot; l.shadows=LightShadows.None; l.color=new Color(1,.85f,.64f);
            l.cookie=cookie; l.enabled=false; return l;
        }
        public static bool InBeam(Vector3 from,Vector3 forward,Vector3 target,float range,float innerAngle)
        {
            var delta=target-from;
            return range>0 && delta.sqrMagnitude>.0001f && delta.sqrMagnitude<range*range &&
                forward.sqrMagnitude>.5f && Vector3.Dot(forward.normalized,delta.normalized)>Mathf.Cos(innerAngle*.5f*Mathf.Deg2Rad);
        }
        bool Clear(Vector3 from,Vector3 target)
        {
            var delta=target-from; int count=Physics.RaycastNonAlloc(from,delta.normalized,hits,delta.magnitude,~0,QueryTriggerInteraction.Ignore);
            if(count==hits.Length)return false;
            for(int i=0;i<count;i++) if(!hits[i].collider.GetComponentInParent<HandRig>() &&
                !hits[i].collider.GetComponentInParent<XROrigin>()) return false;
            return true;
        }
        void Observe(int slot,int actor,Vector3 eye,Vector3 forward,bool active)
        {
            var s=watch.State;
            if(lastActor[slot]!=actor){s.RepelWithLantern(slot,Time.deltaTime,false,false);lastActor[slot]=actor;}
            var drowned=s.Drowned; bool present=drowned?.Phase==DrownedPhase.Manifest && drowned.Anchored;
            var origin=eye+Vector3.up*.10f+forward.normalized*.04f;
            var target=present?new Vector3(drowned.X,drowned.Y+1.2f,drowned.Z):Vector3.zero;
            bool aim=active&&present&&InBeam(origin,forward,target,s.Sprint5.lanternHighRange,HorrorTuning.Current.lanternHighBeamInner)&&Clear(origin,target);
            s.RepelWithLantern(slot,Time.deltaTime,active,aim);
        }
        void LateUpdate()
        {
            if(!watch||watch.State==null||!head||!local||!partner)return;
            var s=watch.State; var tune=s.Sprint5; if(!tune){local.enabled=partner.enabled=false;return;}
            var horror=HorrorTuning.Current;
            foreach(var l in Lights){l.range=tune.lanternHighRange;l.intensity=tune.lanternHighIntensity;l.spotAngle=horror.lanternHighBeamAngle;l.innerSpotAngle=horror.lanternHighBeamInner;}
            bool active=!watch.Sleeping&&!s.Failed;
            local.enabled=active;
            watch.DrownedViewers(out int a,out var p,out var f,out int b,out var q,out var g);
            var locations=s.Coop as ICoopKeeperLocations;int localActor=locations?.LocalActor??0;
            bool other=false;Vector3 eye=default, forward=default;
            if(localActor!=0&&a!=0&&a!=localActor){other=true;eye=p;forward=f;}
            else if(localActor!=0&&b!=0&&b!=localActor){other=true;eye=q;forward=g;}
            if(!other){companion??=watch.GetComponent<CrewCompanion>();if(companion)other=companion.HeadlampPose(out eye,out forward);}
            if(other&&forward.sqrMagnitude>.5f){partner.transform.SetPositionAndRotation(eye+Vector3.up*.10f+forward.normalized*.04f,Quaternion.LookRotation(forward,Vector3.up));partner.enabled=active;}
            else partner.enabled=false;
            if(s.ReadOnly||s.Lanterns==null)return;
            // In solo, slot zero uses the actual tracked head even before a network pose exists.
            if(a==0){p=head.position;f=head.forward;}
            Observe(0,a,p,f,active);
            if(b!=0)Observe(1,b,q,g,active);
            else if(other&&s.ScriptedPartner)Observe(1,WatchState.CompanionActor,eye,forward,active);
            else Observe(1,0,Vector3.zero,Vector3.forward,false);
        }
        void OnDestroy(){if(local)Destroy(local.gameObject);if(partner)Destroy(partner.gameObject);if(cookie)Destroy(cookie);}
    }
}
