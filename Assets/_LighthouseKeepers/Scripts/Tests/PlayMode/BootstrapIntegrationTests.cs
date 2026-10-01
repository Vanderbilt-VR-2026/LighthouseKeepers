using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Core;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Puzzles;

namespace LighthouseKeepers.Tests
{
    public sealed class BootstrapIntegrationTests
    {
        [UnityTest]
        public IEnumerator BootstrapLoadsTeamEnvironmentWithOneFloodAuthority()
        {
            yield return SceneManager.LoadSceneAsync("LK_Bootstrap", LoadSceneMode.Single);
            float deadline = Time.realtimeSinceStartup + 60;
            EnvironmentBootstrap bootstrap;
            do
            {
                yield return null;
                bootstrap = Object.FindAnyObjectByType<EnvironmentBootstrap>();
            } while ((!bootstrap || !bootstrap.Ready) && Time.realtimeSinceStartup < deadline);
            Assert.IsNotNull(bootstrap);
            Assert.IsTrue(bootstrap.Ready, "Additive loading did not reach Ready.");
            foreach (var name in new[] {"LK_Exterior", "LK_Core", "LK_Level01_Plumbing", "LK_Level02_Generator", "LK_Level03_Communications", "LK_Level04_Lantern"})
                Assert.IsTrue(SceneManager.GetSceneByName(name).isLoaded, name);
            Assert.AreEqual(1, Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(1, Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(1, Object.FindObjectsByType<FloodController>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(16, Object.FindObjectsByType<PuzzleSocket>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(4, Object.FindObjectsByType<FloodThreshold>(FindObjectsSortMode.None).Length);
            Assert.IsFalse(SceneManager.GetSceneByName("LK_Recovery").isLoaded);
            // Structural/runtime smoke test only; no claim of headset navigation or feel.
        }
    }
}
