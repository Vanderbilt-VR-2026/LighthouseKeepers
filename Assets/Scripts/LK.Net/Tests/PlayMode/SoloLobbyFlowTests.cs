#if UNITY_EDITOR
using System.Collections;
using LighthouseKeepers.StormWatch;
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
        [UnityTest] public IEnumerator SoloButtonLoadsPlayableWatchWithoutHostingAndReturnsToLobby()
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
            StormWatchDirector director;
            do { yield return null; director = Object.FindAnyObjectByType<StormWatchDirector>(); }
            while ((!director || !director.Ready) && Time.realtimeSinceStartup < deadline);
            Assert.IsNotNull(director); Assert.IsTrue(director.Ready);
            Assert.IsTrue(director.SoloPractice); Assert.IsTrue(director.CrewReady);
            Assert.AreEqual(0, Object.FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None).Length);
            yield return new WaitForSeconds(.2f);
            Assert.AreEqual(WatchAct.FirstWatch, director.State.act);
            Assert.Greater(director.State.elapsed, 0);
            Assert.IsFalse(GameObject.Find("Route 3 / mechanical stair gate"));
            GameObject.Find("SOLO DAY 2").GetComponent<Button>().onClick.Invoke();
            Assert.IsFalse(director.State.power);
            GameObject.Find("SOLO RELAY").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSeconds(.15f);
            Assert.AreEqual(WatchAct.RadioRelay, director.State.act);
            var call = GameObject.Find("CALL SUN").GetComponent<RectTransform>();
            var selector = call.parent.Find("SOLO RELAY").GetComponent<RectTransform>();
            Assert.Greater(call.rect.min.y + call.anchoredPosition.y,
                selector.rect.max.y + selector.anchoredPosition.y, "Radio call and solo selector must not overlap.");
            director.Leave();
            deadline = Time.realtimeSinceStartup + 15;
            while (!GameObject.Find("PlaySolo") && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsNotNull(GameObject.Find("PlaySolo"));
            Assert.IsFalse(SoloWatchSession.ConsumeRequest(), "A later multiplayer run must not inherit solo mode.");
        }
    }
}
#endif
