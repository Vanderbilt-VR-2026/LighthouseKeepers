using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class ShellGeometryTests
    {
        [Test] public void StairGoingRiserAndWidthMeetApprovedBands()
        {
            foreach(var s in LighthouseBuilder.Layout.stairs)
            {double going=s.walkLineRadius*s.arcDegrees*Math.PI/180/s.goings;Assert.That(going,Is.InRange(.28,.34),s.id);Assert.That(s.rise/s.risers,Is.LessThanOrEqualTo(.19f));Assert.That(s.clearWidth,Is.GreaterThanOrEqualTo(1f));Assert.That(s.departureAzimuth,Is.EqualTo(320));}
        }
        [Test] public void LandingsFollowWallWithFullRevealAndClearWidth()
        {
            var d=LighthouseBuilder.Layout;
            foreach(var l in d.landings)
            {var f=d.floors.Single(f=>f.id==l.floor);Assert.That(f.interiorRadius-l.outerRadius,Is.GreaterThanOrEqualTo(.07999f));Assert.That(l.outerRadius-l.innerRadius,Is.GreaterThanOrEqualTo(1.29999f));Assert.That((l.arcEndDeg-l.arcStartDeg)*Mathf.Deg2Rad*l.innerRadius,Is.GreaterThanOrEqualTo(1.29999f));}
        }
        [Test] public void ArtScaleAndOrientationTest()=>LighthouseBuilder.ValidateScale();
        [Test] public void ContractRetainsFifteenStationsAndSixteenLegacyAliases()
        {
            var d=LighthouseBuilder.Layout;Assert.That(d.stations.Length,Is.EqualTo(15));Assert.That(d.stations.Sum(s=>s.legacySockets.Length),Is.EqualTo(16));Assert.That(d.stations.Select(s=>s.id).Distinct().Count(),Is.EqualTo(15));
            foreach(var s in d.stations){Assert.That(s.interactiveAnchors.Select(a=>a.name).Distinct().Count(),Is.EqualTo(s.interactiveAnchors.Length));}
        }
        [Test] public void ShellScenesContainNoPlayerOrGameplayComponents()
        {
            foreach(var path in Directory.GetFiles("Assets/Scenes/Shell","*.unity"))
            {
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                foreach(var root in scene.GetRootGameObjects())
                {Assert.That(root.GetComponentsInChildren<Camera>(true),Is.Empty,path);Assert.That(root.GetComponentsInChildren<AudioListener>(true),Is.Empty,path);Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true),Is.Empty,path);}
            }
        }
        [Test] public void RenderersBelongToAtMostOneLodGroup()
        {
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
            var renderers=UnityEngine.Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .SelectMany(g=>g.GetLODs().SelectMany(l=>l.renderers)).Where(r=>r).ToArray();
            Assert.That(renderers.Distinct().Count(),Is.EqualTo(renderers.Length));
        }
        [Test] public void GeometryClearanceAgainstImportedColliders()
        {
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);foreach(var c in UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Include,FindObjectsSortMode.None))c.enabled=false;Physics.SyncTransforms();
            var nav=NavigationClearanceValidator.Check();var work=WorkPositionValidator.Check();Assert.That(nav.failures,Is.Empty,string.Join("\n",nav.failures.Take(12)));Assert.That(work.failures,Is.Empty,string.Join("\n",work.failures.Take(12)));
        }
    }
}
