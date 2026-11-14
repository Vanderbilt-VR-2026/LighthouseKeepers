using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class SteamerTests
    {
        [Test] public void HorizonAndCloseViewsUseTheSameHullWithAttachedLights()
        {
            var view=Object.Instantiate(Resources.Load<SteamerView>("Sprint5Ships/CoastalSteamer"));
            try{
                view.Initialize("Mary Ellen");var lods=view.GetComponent<LODGroup>().GetLODs();
                Assert.AreEqual(2,lods.Length);Assert.AreSame(lods[0].renderers[0],lods[1].renderers[0]);
                Assert.AreSame(view.hull,lods[1].renderers[0].GetComponent<MeshFilter>().sharedMesh);
                Assert.IsNull(view.transform.Find("Distant navigation lights"));
                Assert.Less(lods[1].screenRelativeTransitionHeight,.01f,"No silhouette cull within the 350m camera range");
            }finally{Object.DestroyImmediate(view.gameObject);}
        }
        [Test] public void BakedVesselFitsMobileGeometryAndMaterialBudget()
        {
            var prefab=Resources.Load<SteamerView>("Sprint5Ships/CoastalSteamer");Assert.NotNull(prefab);
            var name=SteamerGeometry.BuildName("Mary Ellen");try{
                int triangles=prefab.hull.triangles.Length/3+name.triangles.Length/3+prefab.beam.triangles.Length/3+prefab.particles.triangles.Length/3;
                Assert.That(triangles,Is.InRange(8000,SteamerGeometry.TriangleBudget));Assert.Less(prefab.distant.triangles.Length/3,300);
                Assert.That(prefab.hull.bounds.size.z,Is.GreaterThan(50));Assert.That(prefab.hull.bounds.size.y,Is.GreaterThan(14));
                Assert.AreEqual(72,SteamerGeometry.SmokeCount);Assert.LessOrEqual(SteamerGeometry.SmokeCount,150);
                Assert.False(ShaderUtil.ShaderHasError(prefab.hullMaterial.shader));Assert.False(ShaderUtil.ShaderHasError(prefab.effectsMaterial.shader));
                Assert.AreEqual(0,prefab.GetComponentsInChildren<Light>(true).Length);Assert.AreEqual(0,prefab.GetComponentsInChildren<Collider>(true).Length);
            }finally{Object.DestroyImmediate(name);}
        }
        [Test] public void WholeHullClearsIslandAndDistantCoastThroughoutPassage()
        {
            var mesh=SteamerGeometry.BuildHull();try{var vertices=mesh.vertices;float clear=float.MaxValue,far=0,east=float.MinValue;for(int i=0;i<=240;i++){
                var pose=SteamerGeometry.Route(i*180/240f,75,55,50);
                foreach(var vertex in vertices){var p=pose.position+pose.rotation*(vertex*1.25f);var planar=new Vector2(p.x,p.z);var island=(planar-StormSeascapeGeometry.IslandCentre)/StormSeascapeGeometry.IslandRadii;
                    clear=Mathf.Min(clear,island.magnitude);far=Mathf.Max(far,planar.magnitude);east=Mathf.Max(east,p.x);}
            }Assert.Greater(clear,1.8f,"Vessel must not intersect rocks/rails");Assert.Less(far,335,"Inside350m far clip");Assert.Less(east,-20,"Western fairway stays away from eastern coast");}finally{Object.DestroyImmediate(mesh);}
        }
        [Test] public void RouteIsContinuousAtApproachWindowAndDeparture()
        {
            foreach(float t in new[]{75f,130f}){
                var a=SteamerGeometry.Route(t-.001f,75,55,50);var b=SteamerGeometry.Route(t+.001f,75,55,50);
                Assert.Less(Vector3.Distance(a.position,b.position),.02f);Assert.Less(Quaternion.Angle(a.rotation,b.rotation),.1f);
            }
        }
        [Test] public void PaintedNamesHaveFiniteGeometryAndDoNotNeedAnotherMaterial()
        {
            foreach(string name in new[]{"Mary Ellen","North Star","Meridian"}){var m=SteamerGeometry.BuildName(name);try{Assert.Greater(m.triangles.Length,300);Assert.AreEqual(1,m.subMeshCount);foreach(var v in m.vertices)Assert.True(float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z));}finally{Object.DestroyImmediate(m);}}
        }
    }
}
