using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LighthouseKeepers.Tests
{
    public sealed class WalkthroughGeometryTests
    {
        const string Root = "Assets/_LighthouseKeepers/Scenes/";
        [Test]
        public void PipeAssemblyFacesRoomAtThreeQuarterScaleAndWheelsHaveBackFaces()
        {
            EditorSceneManager.OpenScene(Root+"Levels/LK_Level01_Plumbing.unity");
            var assembly=GameObject.Find("Compact player facing manifold");Assert.IsNotNull(assembly);
            Assert.That(assembly.transform.localScale.x,Is.EqualTo(.75f).Within(.001f));
            Assert.That(Vector3.Dot(assembly.transform.forward,Vector3.back),Is.GreaterThan(.99f));
            var wheels=assembly.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Shutoff valve handwheel").ToArray();
            Assert.AreEqual(3,wheels.Length);
            foreach(var wheel in wheels){Assert.That(wheel.sharedMesh.bounds.size.y,Is.GreaterThan(.03f));Assert.IsTrue(wheel.sharedMesh.normals.Any(n=>n.y<-.1f));Assert.IsTrue(wheel.sharedMesh.normals.Any(n=>n.y>.1f));}
        }
        [Test]
        public void FloorSlabsHaveThicknessWithoutChangingTheirTopElevation()
        {
            EditorSceneManager.OpenScene(Root+"Environment/LK_Core.unity");
            var floors=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name.Contains("annular floor")).ToArray();
            Assert.AreEqual(4,floors.Length);
            foreach(var floor in floors){Assert.That(floor.sharedMesh.bounds.size.y,Is.EqualTo(.18f).Within(.001f));float top=floor.sharedMesh.bounds.max.y;Assert.That(top/3.2f,Is.EqualTo(Mathf.Round(top/3.2f)).Within(.001f));}
        }
        [Test]
        public void EveryStairFlightHasSupportAndStandingHeadroom()
        {
            EditorSceneManager.OpenScene(Root + "Environment/LK_Core.unity");
            Physics.SyncTransforms();
            for(int floor=0;floor<3;floor++)for(int degrees=6;degrees<360;degrees+=6)
            {
                float a=degrees*Mathf.Deg2Rad;
                var p=new Vector3(Mathf.Cos(a)*1.65f,floor*3.2f+degrees/360f*3.2f+.06f,Mathf.Sin(a)*1.65f);
                Assert.IsTrue(Physics.Raycast(p+Vector3.up*.1f,Vector3.down,.25f,1<<8),"Stair support missing at "+p);
                var blockers=Physics.OverlapCapsule(p+Vector3.up*.3f,p+Vector3.up*1.775f,.275f,1<<8,QueryTriggerInteraction.Ignore);
                Assert.IsEmpty(blockers,$"Stair clearance blocked at {p}: {string.Join(", ",blockers.Select(c=>c.name))}");
            }
        }
        [Test]
        public void EveryUpperFloorHasSupportedStairExit()
        {
            EditorSceneManager.OpenScene(Root + "Environment/LK_Core.unity");
            Physics.SyncTransforms();
            for (int floor = 1; floor <= 3; floor++)
                for (float x = 1.5f; x <= 3.3f; x += .1f)
                {
                    var p = new Vector3(x, floor * 3.2f + .12f, .12f);
                    Assert.IsTrue(Physics.Raycast(p, Vector3.down, out var hit, .2f, 1 << 8), "Missing landing at " + p);
                    Assert.That(hit.point.y, Is.EqualTo(floor * 3.2f).Within(.055f));
                }
        }
        [Test]
        public void StairRampIsCollisionOnlyAndRetainsComfortableWidth()
        {
            EditorSceneManager.OpenScene(Root + "Environment/LK_Core.unity");
            var ramps = Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None)
                .Where(c => c.name == "Smooth spiral collision and stair bed").ToArray();
            Assert.AreEqual(3, ramps.Length);
            foreach (var ramp in ramps)
            {
                Assert.IsNull(ramp.GetComponent<MeshRenderer>(), "Ramp must not cover visible stair treads, including in combined batches.");
                var radii = ramp.sharedMesh.vertices.Select(v => new Vector2(v.x,v.z).magnitude).ToArray();
                Assert.That(radii.Max()-radii.Min(), Is.GreaterThanOrEqualTo(1.29f));
            }
        }
        [Test]
        public void CommunicationsHasClearStandingRouteAroundShaft()
        {
            EditorSceneManager.OpenScene(Root + "Environment/LK_Core.unity");
            EditorSceneManager.OpenScene(Root + "Levels/LK_Level03_Communications.unity", OpenSceneMode.Additive);
            Physics.SyncTransforms();
            for (int degrees = 0; degrees < 360; degrees += 5)
            {
                float a = degrees*Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Cos(a)*3.05f,6.4f,Mathf.Sin(a)*3.05f);
                var blockers = Physics.OverlapCapsule(p+Vector3.up*.3f,p+Vector3.up*1.625f,.275f,1<<8,QueryTriggerInteraction.Ignore);
                Assert.IsEmpty(blockers, $"Standing route blocked at {p}: {string.Join(", ",blockers.Select(c=>c.name))}");
            }
        }
    }
}
