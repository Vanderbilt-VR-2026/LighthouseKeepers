using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class StormSeascapeTests
    {
        [Test] public void OceanAndCoastMeshesStayWithinQuestVertexBudget()
        {
            var ocean=StormSeascapeGeometry.BuildOcean(false);var coast=StormSeascapeGeometry.BuildCoast(false);
            try
            {
                Assert.That(ocean.vertexCount,Is.LessThanOrEqualTo(StormSeascapeGeometry.OceanVertexBudget));
                Assert.That(ocean.vertexCount,Is.GreaterThan(5000),"Fewer vertices cannot carry the near-field waves");
                Assert.That(ocean.indexFormat,Is.EqualTo(UnityEngine.Rendering.IndexFormat.UInt16));
                Assert.That(coast.vertexCount,Is.LessThanOrEqualTo(StormSeascapeGeometry.CoastVertexBudget));
                Assert.That(ocean.bounds.extents.x,Is.GreaterThanOrEqualTo(StormSeascapeGeometry.OceanOuterRadius));
            }
            finally{Object.DestroyImmediate(ocean);Object.DestroyImmediate(coast);}
        }
        [Test] public void WaveAmplitudeVanishesAtTheIslandAndIsFullInOpenWater()
        {
            var c=StormSeascapeGeometry.IslandCentre;var r=StormSeascapeGeometry.IslandRadii;
            foreach(var dir in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down,new Vector2(.7f,.7f)})
            {
                var edge=c+Vector2.Scale(dir.normalized,r);
                Assert.That(StormSeascapeGeometry.WaveAttenuation(edge),Is.LessThan(.1f),"Waves must not swamp the shore at "+edge);
                Assert.That(StormSeascapeGeometry.WaveAttenuation(c+dir.normalized*45),Is.EqualTo(1).Within(.01f),"Open water near the island");
            }
            Assert.That(StormSeascapeGeometry.WaveAttenuation(Vector2.zero),Is.LessThan(.1f),"Under the tower");
            Assert.That(StormSeascapeGeometry.WaveAttenuation(new Vector2(0,300)),Is.EqualTo(StormSeascapeGeometry.CalmAmplitude).Within(.01f),"Horizon stays gentle");
        }
        [Test] public void CoastSitsInsideTheOceanAndFarClipOnOneSideOnly()
        {
            var coast=StormSeascapeGeometry.BuildCoast(false);
            try
            {
                foreach(var v in coast.vertices)
                {
                    var xz=new Vector2(v.x,v.z);
                    Assert.That(xz.magnitude,Is.InRange(150f,340f),"Silhouette distance, inside the 350 m far clip");
                    float az=Mathf.Atan2(v.x,v.z)*Mathf.Rad2Deg;
                    Assert.That(az,Is.InRange(StormSeascapeGeometry.CoastFrom-8,StormSeascapeGeometry.CoastTo+8),"Coast on one side; the rest open ocean");
                }
                Assert.That(StormSeascapeGeometry.CoastRadius(StormSeascapeGeometry.InletAzimuth),Is.GreaterThan(StormSeascapeGeometry.CoastRadius(StormSeascapeGeometry.InletAzimuth-25)+30),"Inlet recedes");
            }
            finally{Object.DestroyImmediate(coast);}
        }
        [Test] public void SeascapeShadersCompileWithoutErrors()
        {
            foreach(var path in new[]{"Assets/_LighthouseKeepers/Art/VFX/StormOceanWaves.shader","Assets/_LighthouseKeepers/Art/VFX/StormSky.shader","Assets/WatchDemo/Resources/DistantCoast.shader","Assets/WatchDemo/Resources/EliStormSky.shader"})
            {var s=AssetDatabase.LoadAssetAtPath<Shader>(path);Assert.That(s,Is.Not.Null,path);Assert.That(ShaderUtil.ShaderHasError(s),Is.False,path);}
            var ocean=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Environment/M_ShellStormOcean.mat");
            Assert.That(ocean.shader.name,Is.EqualTo("LighthouseKeepers/Storm Ocean Waves"));Assert.That(ocean.renderQueue,Is.LessThan(2500),"Opaque");
        }
    }
}
