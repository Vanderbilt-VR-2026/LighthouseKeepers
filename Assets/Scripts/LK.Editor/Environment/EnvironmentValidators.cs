using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment
{
    [Serializable] public sealed class CheckReport { public string validator; public bool passed; public int samples; public List<string> failures=new List<string>(); }
    public static class EnvironmentValidation
    {
        public static void Inspect(){LuminanceValidator.CaptureViews();RunGeometry();}
        public static void RunGeometry()
        {
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
            // Validate the building, not the spawn rig's own CharacterController.
            foreach(var c in UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Include,FindObjectsSortMode.None))c.enabled=false;
            Physics.SyncTransforms();
            var nav=NavigationClearanceValidator.Check();var work=WorkPositionValidator.Check();
            Save(nav);Save(work);LighthouseBuilder.ValidateScale();
            if(!nav.passed||!work.passed)throw new Exception("Environment geometry validators failed; see artifacts/batch-a reports.");
            Debug.Log("ENVIRONMENT_GEOMETRY_PASS");
        }
        public static void Save(CheckReport r) {Directory.CreateDirectory("artifacts/batch-a");File.WriteAllText("artifacts/batch-a/"+r.validator+".json",JsonUtility.ToJson(r,true));Debug.Log(r.validator+" samples="+r.samples+" failures="+r.failures.Count);}
        public static bool Floor(Vector3 foot,out RaycastHit hit) => Physics.SphereCast(foot+Vector3.up*.25f,.005f,Vector3.down,out hit,.50f,~0,QueryTriggerInteraction.Ignore);
        public static Collider[] Body(Vector3 foot,float radius=.275f,float height=1.9f)
        {
            return Physics.OverlapCapsule(foot+Vector3.up*(radius+.06f),foot+Vector3.up*(height-radius),radius,~0,QueryTriggerInteraction.Ignore)
                .Where(c=>!c.name.Contains("_Ramp")&&!c.name.StartsWith("UCX_SM_Floor_")&&!c.name.StartsWith("UCX_SM_Gallery_Deck")).ToArray();
        }
        public static string Pos(Vector3 p)=>string.Format(System.Globalization.CultureInfo.InvariantCulture,"({0:F3}, {1:F3}, {2:F3})",p.x,p.y,p.z);
    }
    public static class NavigationClearanceValidator
    {
        public static CheckReport Check()
        {
            var r=new CheckReport{validator="NavigationClearanceValidator"};var layout=LighthouseBuilder.Layout;
            foreach(var route in layout.routes)
            for(int i=1;i<route.points.Length;i++)
            {
                Vector3 a=route.points[i-1],b=route.points[i];int steps=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(a,b)/.10f));
                Vector3 direction=b-a;direction.y=0;Vector3 side=Vector3.Cross(Vector3.up,direction.normalized);
                for(int j=0;j<=steps;j++)
                {
                    var p=Vector3.Lerp(a,b,(float)j/steps);r.samples++;
                    if(!EnvironmentValidation.Floor(p,out var ground)){r.failures.Add(route.id+" missing floor "+EnvironmentValidation.Pos(p));continue;}
                    var foot=new Vector3(p.x,ground.point.y,p.z);
                    foreach(var c in EnvironmentValidation.Body(foot))r.failures.Add(route.id+" capsule blocked by "+c.name+" at "+EnvironmentValidation.Pos(foot));
                    // Full authored width, not merely the narrower player capsule.
                    foreach(float sign in new[]{-1f,1f})
                    {
                        var edge=foot+side*(route.widthM/2-.015f)*sign;
                        if(!EnvironmentValidation.Floor(edge,out var support))r.failures.Add(route.id+" unsupported route edge "+EnvironmentValidation.Pos(edge));
                        var hits=Physics.OverlapCapsule(edge+Vector3.up*.10f,edge+Vector3.up*1.85f,.01f,~0,QueryTriggerInteraction.Ignore).Where(c=>!c.name.Contains("_Ramp")&&!c.name.StartsWith("UCX_SM_Floor_")&&!c.name.StartsWith("UCX_SM_Gallery_Deck"));
                        foreach(var c in hits)r.failures.Add(route.id+" clear-width obstruction "+c.name+" at "+EnvironmentValidation.Pos(edge));
                    }
                    if(Physics.Raycast(foot+Vector3.up*.10f,Vector3.up,out var ceiling,(route.kind=="stairs"?2.50f:1.95f),~0,QueryTriggerInteraction.Ignore))r.failures.Add(route.id+" headroom below required stair/door clearance: "+ceiling.collider.name+" at "+EnvironmentValidation.Pos(foot));
                }
            }
            foreach(var l in layout.landings)
            {
                var f=layout.floors.Single(f=>f.id==l.floor);
                if(f.interiorRadius-l.outerRadius<.07999f||l.outerRadius-l.innerRadius<1.29999f||(l.arcEndDeg-l.arcStartDeg)*Mathf.Deg2Rad*l.innerRadius<1.29999f)r.failures.Add(l.id+" analytic landing clearances");
                // Sample the entire curved floor patch, including its outer boundary.
                for(int a=0;a<=16;a++)for(int rad=0;rad<=8;rad++)
                {
                    var p=LighthouseBuilder.Polar(Mathf.Lerp(l.arcStartDeg,l.arcEndDeg,a/16f),Mathf.Lerp(l.innerRadius,l.outerRadius,rad/8f),l.floorY);r.samples++;
                    if(rad==8){var radialDirection=new Vector3(p.x,0,p.z).normalized;if(Physics.Raycast(p+Vector3.up*.5f,radialDirection,out var reveal,.079f,~0,QueryTriggerInteraction.Ignore))r.failures.Add(l.id+" wall reveal below 0.08 m: "+reveal.collider.name+" at "+EnvironmentValidation.Pos(p));}
                    if(!EnvironmentValidation.Floor(p,out _))r.failures.Add(l.id+" unsupported landing "+EnvironmentValidation.Pos(p));
                    if(Physics.Raycast(p+Vector3.up*.10f,Vector3.up,out var hit,2.5f,~0,QueryTriggerInteraction.Ignore))r.failures.Add(l.id+" landing headroom: "+hit.collider.name+" at "+EnvironmentValidation.Pos(p));
                }
            }
            r.failures=r.failures.Distinct().ToList();r.passed=r.failures.Count==0;return r;
        }
    }
    public static class WorkPositionValidator
    {
        public static CheckReport Check()
        {
            var r=new CheckReport{validator="WorkPositionValidator"};var layout=LighthouseBuilder.Layout;
            foreach(var w in layout.operatorPositions)
            {
                var centre=LighthouseBuilder.V(w.position);
                for(int angle=0;angle<36;angle++)for(int radial=0;radial<=4;radial++)
                {
                    var p=centre+LighthouseBuilder.Polar(angle*10,w.radius*radial/4,0);r.samples++;
                    if(!EnvironmentValidation.Floor(p,out _))r.failures.Add(w.id+" unsupported stance disc "+EnvironmentValidation.Pos(p));
                    var obs=Physics.OverlapCapsule(p+Vector3.up*.12f,p+Vector3.up*.90f,.02f,~0,QueryTriggerInteraction.Ignore).Where(c=>!c.name.StartsWith("UCX_SM_Floor_")&&!c.name.StartsWith("UCX_SM_Gallery_Deck"));
                    foreach(var c in obs)r.failures.Add(w.id+" stance obstruction "+c.name+" at "+EnvironmentValidation.Pos(p));
                }
                float front=w.id=="WP-F4-SHARED"?.42f:.80f;
                for(int i=1;i<=8;i++){var p=centre+LighthouseBuilder.Polar(w.facingAzimuth,front*i/8,0);r.samples++;if(!EnvironmentValidation.Floor(p,out _))r.failures.Add(w.id+" front clearance lacks floor "+EnvironmentValidation.Pos(p));}
                foreach(var c in EnvironmentValidation.Body(centre))r.failures.Add(w.id+" standing body obstruction "+c.name);
            }
            // Touch heights are validated from published semantic metadata, not passive depth-zero markers.
            var source=File.ReadAllText(LighthouseBuilder.LayoutPath);
            var typed=JsonUtility.FromJson<ReachLayout>(source);
            foreach(var s in typed.stations)
            {
                float floor=s.floor=="House"?0:layout.floors.Single(f=>f.id==s.floor).floorY;
                foreach(var a in s.interactiveAnchors.Where(a=>a.requiresTouch))
                {float h=s.anchor.position[1]+a.localPosition[1]-floor;r.samples++;if(h<.75f||h>1.45f)r.failures.Add(s.id+"/"+a.name+" touch height "+h);}
            }
            r.failures=r.failures.Distinct().ToList();r.passed=r.failures.Count==0;return r;
        }
        [Serializable] sealed class ReachLayout {public ReachStation[] stations;}
        [Serializable] sealed class ReachStation {public string id,floor;public StationBoxExporter.Pose anchor;public ReachAnchor[] interactiveAnchors;}
        [Serializable] sealed class ReachAnchor {public string name;public bool requiresTouch;public float[] localPosition;}
    }
}
