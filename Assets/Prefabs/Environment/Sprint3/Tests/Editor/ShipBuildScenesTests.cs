using System.Linq;
using LighthouseKeepers.Environment.Sprint3.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
namespace LighthouseKeepers.Environment.Sprint3.Tests
{
    public sealed class ShipBuildScenesTests
    {
        [Test] public void ActualBuildListMatchesShipManifestAndContainsNoRetiredScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            Assert.That(scenes.All(s => s.enabled), Is.True);
            ShipBuildScenes.Validate(scenes.Select(s => s.path).ToArray());
            CollectionAssert.AreEqual(ShipSceneCatalog.Paths, scenes.Select(s => s.path).ToArray());
        }
        [TestCase("LK_StormWatch")]
        [TestCase("LK_Lab_Environment")]
        [TestCase("LK_DevGym")]
        [TestCase("LK_Level01_Plumbing")]
        public void RetiredSceneCannotReplaceAShipScene(string name)
        {
            var paths = ShipSceneCatalog.Paths.ToArray();
            paths[3] = "Assets/Scenes/" + name + ".unity";
            Assert.Throws<BuildFailedException>(() => ShipBuildScenes.Validate(paths));
        }
    }
}
