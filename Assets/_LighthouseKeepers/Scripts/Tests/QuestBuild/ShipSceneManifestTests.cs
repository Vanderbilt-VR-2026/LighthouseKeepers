using System.Collections.Generic;
using LighthouseKeepers.Editor.QuestBuild;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;

namespace LighthouseKeepers.Tests
{
    public class ShipSceneManifestTests
    {
        const string Lobby = "Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity";
        const string Lab = "Assets/Scenes/Lab/LK_Lab_Environment.unity";

        [Test]
        public void ShipManifestContainsExactlyLobbyThenExistingLab()
        {
            var scenes = ShipSceneManifest.Scenes;
            CollectionAssert.AreEqual(new[] { Lobby, Lab }, scenes);
            Assert.DoesNotThrow(() => ShipSceneManifest.Validate(Options(scenes)));
            // A caller cannot change the policy for subsequent builds by editing its copy.
            scenes[1] = Lobby;
            CollectionAssert.AreEqual(new[] { Lobby, Lab }, ShipSceneManifest.Scenes);
        }

        [Test]
        public void LobbyOnlyOptionsAreRejectedEvenWhenGlobalSceneSettingsAreCorrect()
        {
            var saved = EditorBuildSettings.scenes;
            try
            {
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Lobby, true), new EditorBuildSettingsScene(Lab, true) };
                // Regression: filtering /Lab/ from the actual options used to bypass the global guard.
                Assert.Throws<BuildFailedException>(() => ShipSceneManifest.Validate(Options(new[] { Lobby })));
            }
            finally { EditorBuildSettings.scenes = saved; }
        }

        static IEnumerable<TestCaseData> InvalidManifests()
        {
            yield return new TestCaseData(new object[] { new[] { Lab, Lobby } }).SetName("WrongSceneOrderIsRejected");
            yield return new TestCaseData(new object[] { new[] { "Assets/Scenes/EnvironmentLobby.unity", Lab } }).SetName("WrongEntryIsRejected");
            yield return new TestCaseData(new object[] { new[] { Lobby, "Assets/_LighthouseKeepers/Scenes/Development/LK_StormWatch.unity" } }).SetName("LegacyWorldReplacementIsRejected");
            yield return new TestCaseData(new object[] { new[] { Lobby, Lab, "Assets/_LighthouseKeepers/Scenes/Bootstrap/LK_Bootstrap.unity" } }).SetName("ExtraWorldIsRejected");
            yield return new TestCaseData(new object[] { new[] { Lobby, Lobby } }).SetName("DuplicateLobbyIsRejected");
            yield return new TestCaseData(new object[] { new string[0] }).SetName("EmptyManifestIsRejected");
            yield return new TestCaseData(new object[] { null }).SetName("NullManifestIsRejected");
        }

        [TestCaseSource(nameof(InvalidManifests))]
        public void InvalidActualBuildOptionsFail(string[] scenes) =>
            Assert.Throws<BuildFailedException>(() => ShipSceneManifest.Validate(Options(scenes)));

        static BuildPlayerOptions Options(string[] scenes) => new BuildPlayerOptions
        {
            scenes = scenes, target = BuildTarget.Android, locationPathName = "Builds/test.apk"
        };
    }
}
