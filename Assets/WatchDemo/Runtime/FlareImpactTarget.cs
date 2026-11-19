using System;
using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo {
    // Explicit hurt volume, independent of the visual mesh. Host collision owns the outcome.
    public sealed class FlareImpactTarget:MonoBehaviour {
        public static readonly List<FlareImpactTarget> ActiveTargets=new();
        public bool AcceptsHit=true;public Func<bool> OnHit;
        public float Radius=.65f;public Vector3 PreviousCentre {get;private set;}
        public Vector3 Centre=>transform.position;
        public void ResetMotion()=>PreviousCentre=Centre;
        public void FinishFrame()=>PreviousCentre=Centre;
        void OnEnable(){ResetMotion();ActiveTargets.Add(this);}
        void OnDisable()=>ActiveTargets.Remove(this);
        public bool Hit()=>AcceptsHit&&(OnHit?.Invoke()??true);
    }
    public static class FlareCollision {
        static readonly RaycastHit[] hits=new RaycastHit[96];
        public static bool Ignored(Collider c)=>c.GetComponentInParent<WatchFlareGun>()||c.GetComponentInParent<HandRig>()||c.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>()||c.GetComponentInParent<LighthouseKeepers.Net.CrewAvatarView>();
        // Relative-motion sweep catches targets crossing the shot between render frames. Walls win ties.
        public static bool MovingSphere(Vector3 a,Vector3 b,Vector3 oldCentre,Vector3 centre,float radius,out float fraction){
            var p=a-oldCentre;var d=(b-a)-(centre-oldCentre);float c=p.sqrMagnitude-radius*radius;
            fraction=0;if(c<=0)return true;float aa=d.sqrMagnitude;if(aa<.000001f)return false;
            float bb=Vector3.Dot(p,d),disc=bb*bb-aa*c;if(disc<0)return false;
            fraction=(-bb-Mathf.Sqrt(disc))/aa;return fraction>=0&&fraction<=1;
        }
        public static bool Sweep(Vector3 a,Vector3 b,float radius,float startFraction,float endFraction,out Vector3 point,out FlareImpactTarget target){
            point=b;target=null;var delta=b-a;float distance=delta.magnitude;if(distance<.000001f)return false;
            float closest=1;bool blocked=false;int count=Physics.SphereCastNonAlloc(a,radius,delta/distance,hits,distance,~0,QueryTriggerInteraction.Ignore);
            if(count==hits.Length){point=a;return true;} // Fail closed in unusually dense scenery.
            for(int i=0;i<count;i++){var h=hits[i];if(Ignored(h.collider))continue;if(h.distance/distance<=closest){closest=h.distance/distance;blocked=true;}}
            foreach(var t in FlareImpactTarget.ActiveTargets){if(!t||!t.AcceptsHit)continue;
                var old=Vector3.Lerp(t.PreviousCentre,t.Centre,startFraction);var now=Vector3.Lerp(t.PreviousCentre,t.Centre,endFraction);
                if(MovingSphere(a,b,old,now,radius+t.Radius,out var f)&&f<closest){closest=f;blocked=true;target=t;}}
            point=a+delta*closest;return blocked;
        }
    }
}
