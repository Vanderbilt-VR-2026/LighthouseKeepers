#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LighthouseKeepers.Networking;
using LighthouseKeepers.StormWatch;
using NUnit.Framework;
using Unity.XR.CoreUtils;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using NetworkRunner = Fusion.NetworkRunner;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class WatchMenuFlowTests
    {
        MonoBehaviour watch, menu;
        GameObject placementWall;
        object State => watch.GetType().GetProperty("State").GetValue(watch);
        static T Value<T>(object target, string name) => (T)target.GetType().GetProperty(name).GetValue(target);
        static object Call(object target, string method, params object[] args) => target.GetType().GetMethod(method).Invoke(target, args);
        static MonoBehaviour Component(string type) => Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c => c.GetType().Name == type);
        static Button Button(string name) => GameObject.Find(name).GetComponent<Button>();
        IEnumerator LoadLab(int day)
        {
            WatchDayLaunch.RequestedDay = day;
            SoloWatchSession.Request();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            watch = Component("WatchDemoController"); menu = Component("WatchMenu");
            Assert.IsNotNull(State);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            SoloWatchSession.Cancel(); WatchDayLaunch.RequestedDay = 1;
            if (placementWall) Object.Destroy(placementWall);
            var sessions = Object.FindObjectsByType<FusionLobbyTransport>(FindObjectsSortMode.None);
            foreach (var session in sessions) session.LeaveGame();
            float deadline = Time.realtimeSinceStartup + 30;
            foreach (var session in sessions)
            {
                while (session && (session.IsBusy || session.IsOnline || session.Runner) && Time.realtimeSinceStartup < deadline) yield return null;
                if (session) Object.Destroy(session.gameObject);
            }
            yield return null;
        }
        void Open() => Call(menu, "Open");
        static void TrackedClick(Button button)
        {
            Assert.IsTrue(button.IsActive() && button.IsInteractable(), button.name);
            Canvas.ForceUpdateCanvases();
            var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.worldCamera;
            Vector3 centre = button.transform.position;
            var pointer = new TrackedDeviceEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left, layerMask = ~0,
                position = camera.WorldToScreenPoint(centre),
                rayPoints = new List<Vector3> { camera.transform.position, centre + (centre - camera.transform.position).normalized * .05f }
            };
            var hits = new List<RaycastResult>();
            canvas.GetComponent<TrackedDeviceGraphicRaycaster>().Raycast(pointer, hits);
            Assert.IsNotEmpty(hits, button.name + " must be reachable with a tracked UI ray.");
            Assert.AreSame(button.gameObject, hits[0].gameObject, button.name + " must receive the first tracked UI hit.");
            pointer.pointerCurrentRaycast = hits[0];
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        static void CompleteTasks(object state)
        {
            Call(state, "CompleteRepair");
            for (int i = 0; i < 3; i++) Call(state, "StarterPulled");
            var radio = Value<object>(state, "Radio");
            Call(radio, "ToggleMain"); Call(radio, "Tune", 156.8f); Call(state, "Tick", 1.1d);
            foreach (int colour in new[] { 0, 2, 1, 3 }) Call(radio, "PressCode", colour);
            Call(radio, "SetMicrophone", true, true); Call(state, "Tick", 3.1d);
        }

        [UnityTest] public IEnumerator TerminalReplayButtonResetsToDayOneWithoutMovingTrackedTransforms()
        {
            var catalog = Resources.Load("WatchDays");
            var days = (Array)catalog.GetType().GetField("days").GetValue(catalog);
            int lastDay = days.Cast<object>().Max(day => (int)day.GetType().GetField("day").GetValue(day));
            yield return LoadLab(lastDay);
            var rig = Object.FindAnyObjectByType<XROrigin>();
            // Snapshot the authored rig, head and both controller transforms, not a replacement test camera.
            var transforms = new[] { rig.transform, rig.Camera.transform }.Concat(rig.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Left Controller" || t.name == "Right Controller")).ToArray();
            Assert.GreaterOrEqual(transforms.Length, 4);
            var positions = transforms.Select(t => t.localPosition).ToArray();
            var rotations = transforms.Select(t => t.localRotation).ToArray();
            CompleteTasks(State);
            Assert.IsTrue(Value<bool>(State, "Complete")); Assert.IsFalse(Value<bool>(State, "HasNextDay"));
            Assert.IsTrue(Value<bool>(menu, "IsOpen"), "Terminal completion must reveal replay/return actions.");
            Assert.IsTrue(Button("ReplayWatchFromDayOne").interactable);
            Button("ReplayWatchFromDayOne").onClick.Invoke();
            Assert.AreEqual(1, Value<int>(State, "Day")); Assert.IsFalse(Value<bool>(State, "Failed"));
            Assert.AreEqual(0d, Value<double>(State, "Elapsed")); Assert.AreEqual(0, Value<int>(State, "StarterPulls"));
            Assert.IsFalse(Value<bool>(State, "Repaired"));
            Assert.AreEqual("On", Value<object>(State, "Power").ToString());
            Assert.AreEqual(0, Value<int>(Value<object>(State, "Pipe"), "BoltsDone"));
            Assert.IsFalse(Value<bool>(Value<object>(State, "Radio"), "Completed"));
            Assert.IsFalse(Value<bool>(Value<object>(State, "Radio"), "MainOn"));
            Assert.IsFalse(Value<bool>(menu, "IsOpen"));
            for (int i = 0; i < transforms.Length; i++)
            {
                Assert.AreEqual(positions[i], transforms[i].localPosition, transforms[i].name);
                Assert.Less(Quaternion.Angle(rotations[i], transforms[i].localRotation), .001f, transforms[i].name);
            }
            yield return null;
        }

        [UnityTest] public IEnumerator FailedRetryButtonKeepsDayAndClearsPartialWork()
        {
            yield return LoadLab(2);
            Call(State, "StarterPulled"); Call(State, "Tick", Value<double>(State, "Duration") + 1);
            Assert.IsTrue(Value<bool>(State, "Failed")); Assert.IsTrue(Value<bool>(menu, "IsOpen"));
            Assert.IsTrue(Button("RetryWatchDay").interactable);
            Button("RetryWatchDay").onClick.Invoke();
            Assert.AreEqual(2, Value<int>(State, "Day")); Assert.IsFalse(Value<bool>(State, "Failed"));
            Assert.AreEqual(0, Value<int>(State, "StarterPulls")); Assert.AreEqual(0d, Value<double>(State, "Elapsed"));
            Assert.AreEqual("Off", Value<object>(State, "Power").ToString());
            Assert.IsFalse(Value<bool>(menu, "IsOpen"));
            yield return null;
        }

        [UnityTest] public IEnumerator OfflineMenuReturnButtonLoadsLobbyWithXrUiAvailable()
        {
            yield return LoadLab(1); Open();
            var canvas = GameObject.Find("Watch action menu").GetComponent<Canvas>();
            Assert.IsNotNull(canvas.GetComponent<TrackedDeviceGraphicRaycaster>());
            Assert.IsNotNull(canvas.GetComponent<GraphicRaycaster>(), "Desktop mouse input needs the regular raycaster too.");
            Assert.IsNotNull(canvas.worldCamera);
            Assert.AreEqual(1, Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length);
            Assert.IsTrue(Object.FindAnyObjectByType<XRUIInputModule>().isActiveAndEnabled);
            Assert.IsEmpty(Object.FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None));
            double elapsed = Value<double>(State, "Elapsed");
            yield return new WaitForSeconds(.1f);
            Assert.Greater(Value<double>(State, "Elapsed"), elapsed, "Watch Menu must not pause the timed challenge.");
            Button("ReturnWatchToLobby").onClick.Invoke();
            yield return WaitForLobby();
            Assert.IsEmpty(Object.FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None));
            Assert.IsFalse(Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Any(c => c.GetType().Name == "WatchMenu"));
        }

        [UnityTest] public IEnumerator TrackedRaysReachReplayRetryAndReturnWithNearbyWall()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Run tracked menu pointer QA with graphics enabled.");
            var catalog = Resources.Load("WatchDays");
            var days = (Array)catalog.GetType().GetField("days").GetValue(catalog);
            yield return LoadLab(days.Cast<object>().Max(day => (int)day.GetType().GetField("day").GetValue(day)));
            var camera = Object.FindAnyObjectByType<XROrigin>().Camera;
            Vector3 headPosition = camera.transform.position;
            Quaternion headRotation = camera.transform.rotation;
            var forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            placementWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placementWall.name = "Menu placement wall fixture";
            placementWall.transform.SetPositionAndRotation(headPosition + forward * .6f, Quaternion.LookRotation(forward));
            placementWall.transform.localScale = new Vector3(2, 2, .1f);
            Physics.SyncTransforms();
            CompleteTasks(State);
            yield return null;
            var canvas = GameObject.Find("Watch action menu").GetComponent<Canvas>();
            float menuDistance = Vector3.Dot(canvas.transform.position - headPosition, forward);
            Assert.That(menuDistance, Is.InRange(.1f, .5f), "Menu should move in front of the nearby wall.");
            Assert.AreEqual(headPosition, camera.transform.position); Assert.Less(Quaternion.Angle(headRotation, camera.transform.rotation), .001f);
            TrackedClick(Button("ReplayWatchFromDayOne"));
            Assert.AreEqual(1, Value<int>(State, "Day"));
            Call(State, "Tick", Value<double>(State, "Duration") + 1);
            yield return null;
            TrackedClick(Button("RetryWatchDay"));
            Assert.IsFalse(Value<bool>(State, "Failed")); Assert.AreEqual(1, Value<int>(State, "Day"));
            Open(); yield return null;
            TrackedClick(Button("ReturnWatchToLobby"));
            yield return WaitForLobby();
        }

        [UnityTest] public IEnumerator MenuReturnClosesExistingCrewRunnerBeforeLobbyArrival()
        {
            yield return LoadLab(1);
            var session = new GameObject("Watch menu real transport fixture").AddComponent<FusionLobbyTransport>();
            var runner = new GameObject("Watch menu offline crew runner").AddComponent<NetworkRunner>();
            var start = runner.StartGame(new Fusion.StartGameArgs { GameMode = Fusion.GameMode.Single, SceneManager = runner.gameObject.AddComponent<Fusion.NetworkSceneManagerDefault>() });
            while (!start.IsCompleted) yield return null;
            Assert.IsTrue(start.Result.Ok);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(FusionLobbyTransport).GetField("runner", flags).SetValue(session, runner);
            typeof(FusionLobbyTransport).GetField("online", flags).SetValue(session, true);
            typeof(FusionLobbyTransport).GetField("enteredGame", flags).SetValue(session, true);
            Open(); Button("ReturnWatchToLobby").onClick.Invoke();
            yield return WaitForLobby();
            Assert.IsFalse(session, "The prior crew transport must be destroyed through its existing leave path.");
            Assert.IsFalse(runner, "The prior runner must be destroyed before returning control to the lobby.");
            Assert.IsEmpty(Object.FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None));
        }
        static IEnumerator WaitForLobby()
        {
            float deadline = Time.realtimeSinceStartup + 45;
            while ((!SceneManager.GetSceneByName("LK_NetworkLobby").isLoaded || !GameObject.Find("PlaySolo")) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(SceneManager.GetSceneByName("LK_NetworkLobby").isLoaded);
            Assert.IsNotNull(GameObject.Find("PlaySolo"));
            yield return null;
        }
    }
}
#endif
