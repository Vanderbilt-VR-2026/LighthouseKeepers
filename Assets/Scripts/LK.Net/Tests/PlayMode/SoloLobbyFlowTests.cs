#if UNITY_EDITOR
using System.Collections;
using LighthouseKeepers.StormWatch;
using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using NetworkRunner = Fusion.NetworkRunner;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class SoloLobbyFlowTests
    {
        [UnityTearDown] public IEnumerator Cleanup()
        {
            var sessions = Object.FindObjectsByType<LighthouseKeepers.Networking.FusionLobbyTransport>(FindObjectsSortMode.None);
            foreach (var session in sessions) session.LeaveGame();
            float deadline = Time.realtimeSinceStartup + 30;
            foreach (var session in sessions)
            {
                while (session && session.IsBusy && Time.realtimeSinceStartup < deadline) yield return null;
                if (session) Object.Destroy(session.gameObject);
            }
            yield return null;
        }
        [UnityTest] public IEnumerator SoloButtonLoadsBootstrapWithoutHostingAndReturnsToLobby()
            => VerifySoloFlow(false);
        [UnityTest] public IEnumerator SoloButtonClosesAnExistingRunnerBeforeOpeningPractice()
            => VerifySoloFlow(true);
        IEnumerator VerifySoloFlow(bool hosting)
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            if (hosting)
            {
                // Hosting can reload the lobby while its original transport survives.
                // The button must close that old session too, not just its board's transport.
                var raw = new GameObject("Surviving host session fixture")
                    .AddComponent<LighthouseKeepers.Networking.FusionLobbyTransport>();
                var runner = new GameObject("Existing offline host fixture").AddComponent<NetworkRunner>();
                var start = runner.StartGame(new Fusion.StartGameArgs { GameMode = Fusion.GameMode.Single,
                    SceneManager = runner.gameObject.AddComponent<Fusion.NetworkSceneManagerDefault>() });
                while (!start.IsCompleted) yield return null;
                Assert.IsTrue(start.Result.Ok);
                // Exercise the real shutdown path without depending on Photon service availability.
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                typeof(LighthouseKeepers.Networking.FusionLobbyTransport).GetField("runner", flags).SetValue(raw, runner);
                typeof(LighthouseKeepers.Networking.FusionLobbyTransport).GetField("online", flags).SetValue(raw, true);
            }
            var button = GameObject.Find("PlaySolo").GetComponent<Button>();
            Assert.IsTrue(button.interactable);
            Assert.IsNotNull(GameObject.Find("WatchBoardCanvas").transform.Find("CrewOptionsPanel/PlaySoloOptions"));
            button.onClick.Invoke();
            float deadline = Time.realtimeSinceStartup + 90;
            EnvironmentBootstrap bootstrap;
            do { yield return null; bootstrap = Object.FindAnyObjectByType<EnvironmentBootstrap>(); }
            while ((!bootstrap || !bootstrap.Ready) && Time.realtimeSinceStartup < deadline);
            Assert.IsNotNull(bootstrap); Assert.IsTrue(bootstrap.Ready);
            Assert.IsTrue(SceneManager.GetSceneByName("LK_Bootstrap").isLoaded);
            Assert.IsFalse(SceneManager.GetSceneByName("LK_StormWatch").isLoaded);
            Assert.AreEqual(7, SceneManager.sceneCount);
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(0, Object.FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(1, Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None).Length);
            var hands = Object.FindObjectsByType<HandRig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(2, hands.Length);
            Assert.AreNotEqual(hands[0].Node, hands[1].Node);
            // Tracking-loss/recovery must not let legacy cleanup hide the new skin.
            var rig = Object.FindAnyObjectByType<XROrigin>();
            foreach (var modality in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true)) modality.enabled = false;
            foreach (var hand in hands) hand.transform.parent.gameObject.SetActive(true);
            yield return null;
            foreach (var hand in hands) hand.transform.parent.gameObject.SetActive(false);
            yield return new WaitForSeconds(1.2f); // Allow the actual periodic legacy cleanup to run.
            foreach (var hand in hands) hand.transform.parent.gameObject.SetActive(true);
            yield return null;
            foreach (var hand in hands)
            {
                var skins = hand.GetComponentsInChildren<SkinnedMeshRenderer>();
                Assert.AreEqual(1, skins.Length);
                Assert.IsTrue(skins[0].enabled, "Tracking recovery must preserve the Sprint 3 skin.");
            }
            Assert.AreEqual(0, Object.FindObjectsByType<TunnelingVignetteController>(FindObjectsSortMode.None).Length);
            Assert.IsNull(Object.FindAnyObjectByType<StormWatchBoard>());
            Assert.IsFalse(GameObject.Find("SOLO DAY 2"));
            Assert.IsFalse(SoloWatchSession.ConsumeRequest(), "Bootstrap must consume the one-shot launch request.");
            yield return SceneManager.LoadSceneAsync("LK_NetworkLobby");
            deadline = Time.realtimeSinceStartup + 15;
            while (!GameObject.Find("PlaySolo") && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsNotNull(GameObject.Find("PlaySolo"));
            Assert.IsFalse(SoloWatchSession.ConsumeRequest(), "A later multiplayer run must not inherit solo mode.");
        }
    }
}
#endif
