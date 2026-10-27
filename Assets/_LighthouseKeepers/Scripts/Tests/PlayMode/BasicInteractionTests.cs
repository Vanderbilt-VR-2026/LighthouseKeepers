#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LighthouseKeepers.Puzzles;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.Tests
{
    public sealed class BasicInteractionTests
    {
        GameObject root, target;
        XRInteractionManager manager;
        XRSocketInteractor hand;
        XRSimpleInteractable interactable;

        [UnitySetUp] public IEnumerator SetUp()
        {
            root = new GameObject("Basic interaction test fixture");
            // Keep the fixture far from gameplay scenes left open by other tests.
            root.transform.position = Vector3.right * 1000;
            manager = root.AddComponent<XRInteractionManager>();
            target = new GameObject("Control"); target.transform.SetParent(root.transform, false);
            target.AddComponent<BoxCollider>().size = Vector3.one * .2f;
            interactable = target.AddComponent<XRSimpleInteractable>();
            interactable.interactionManager = manager;
            var go = new GameObject("Nearby physical hand"); go.transform.SetParent(root.transform, false);
            var collider = go.AddComponent<SphereCollider>(); collider.radius = .1f; collider.isTrigger = true;
            go.AddComponent<Rigidbody>().isKinematic = true;
            hand = go.AddComponent<XRSocketInteractor>(); hand.interactionManager = manager;
            hand.showInteractableHoverMeshes = false;
            yield return null;
        }
        [UnityTearDown] public IEnumerator TearDown() { Object.Destroy(root); yield return null; }
        IEnumerator Select()
        {
            if (!hand.IsSelecting((IXRSelectInteractable)interactable)) manager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)interactable);
            yield return null;
        }
        void Release()
        {
            if (hand.IsSelecting((IXRSelectInteractable)interactable)) manager.SelectExit((IXRSelectInteractor)hand, (IXRSelectInteractable)interactable);
        }
        IEnumerator Turn(float degrees, int frames = 1)
        {
            for (int i = 0; i < frames; i++) { hand.transform.Rotate(0, 0, degrees, Space.World); yield return null; }
        }
        static PuzzleSocket Bind(Object control, string field, GameObject root, System.Action action)
        {
            var go = new GameObject("Existing socket"); go.transform.SetParent(root.transform, false);
            var socket = go.AddComponent<PuzzleSocket>();
            // UnityEvent is private by design; retrieve its actual serialized backing instance.
            var eventField = typeof(PuzzleSocket).GetField("onInteraction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ((UnityEngine.Events.UnityEvent)eventField.GetValue(socket)).AddListener(() => action());
            var settings = new SerializedObject(control); settings.FindProperty(field).objectReferenceValue = socket; settings.ApplyModifiedPropertiesWithoutUndo();
            return socket;
        }

        [UnityTest] public IEnumerator WheelRockingCannotFarmProgressAndRegripDoesNotJump()
        {
            var wheel = target.AddComponent<ValveHandwheel>(); int completions = 0;
            Bind(wheel, "repairSocket", root, () => completions++);
            yield return Select();
            for (int i = 0; i < 8; i++) { yield return Turn(15); yield return Turn(-15); }
            Assert.That(wheel.NormalizedProgress, Is.LessThan(.001f));
            yield return Turn(15, 6);
            float partial = wheel.NormalizedProgress;
            Release(); hand.enabled = false; hand.transform.Rotate(0, 0, 100); yield return null;
            hand.enabled = true; yield return Select();
            Assert.That(wheel.NormalizedProgress, Is.EqualTo(partial).Within(.001f), "Regripping must establish a new wrist baseline.");
            yield return Turn(15, 32);
            Assert.IsTrue(wheel.IsClosed); Assert.AreEqual(1, completions);
            yield return Turn(-15, 4); yield return Turn(15, 4);
            Assert.AreEqual(1, completions, "A latched valve fires its socket only once.");
        }

        [UnityTest] public IEnumerator WristSwingAndDistantHandCannotTurnWheel()
        {
            var wheel = target.AddComponent<ValveHandwheel>(); yield return Select();
            hand.transform.Rotate(45, 0, 0, Space.World); yield return null;
            Assert.That(wheel.NormalizedProgress, Is.EqualTo(0).Within(.001f));
            hand.transform.position += Vector3.up * 2;
            yield return Turn(15, 4);
            Assert.That(wheel.NormalizedProgress, Is.EqualTo(0).Within(.001f));
        }

        [UnityTest] public IEnumerator LeverTogglesBothWaysWithoutThresholdChatter()
        {
            var lever = target.AddComponent<LeverSwitch>(); var states = new List<bool>();
            lever.OnStateChanged.AddListener(states.Add); yield return Select();
            yield return Turn(-15, 3); Assert.IsTrue(lever.IsOn);
            yield return Turn(9); Assert.IsTrue(lever.IsOn, "Movement around the on threshold should not chatter.");
            yield return Turn(9); Assert.IsFalse(lever.IsOn);
            CollectionAssert.AreEqual(new[] { true, false }, states);
            Release(); hand.enabled = false; yield return null;
            Assert.That(lever.NormalizedPosition, Is.EqualTo(.45f).Within(.01f), "Release retains local lever position.");
        }

        [UnityTest] public IEnumerator DialTurnsBackwardsAndRemainsAvailableAfterFullTurn()
        {
            var dial = target.AddComponent<RotaryDial>(); yield return Select();
            yield return Turn(15, 24); Assert.That(Mathf.DeltaAngle(0, dial.AngleDegrees), Is.EqualTo(0).Within(.01f));
            yield return Turn(-15, 2); Assert.That(dial.AngleDegrees, Is.EqualTo(330).Within(.01f));
            yield return Turn(15, 4); Assert.That(dial.AngleDegrees, Is.EqualTo(30).Within(.01f));
        }

        [UnityTest] public IEnumerator PushToTalkBalancesPressReleaseDropAndDisable()
        {
            var button = target.AddComponent<PushToTalkButton>(); var states = new List<bool>(); int presses = 0;
            button.OnTalkingChanged.AddListener(states.Add); Bind(button, "boundSocket", root, () => presses++);
            yield return Select();
            interactable.activated.Invoke(new ActivateEventArgs()); interactable.activated.Invoke(new ActivateEventArgs());
            Assert.IsTrue(button.IsTalking); Assert.AreEqual(1, presses);
            interactable.deactivated.Invoke(new DeactivateEventArgs()); Assert.IsFalse(button.IsTalking);
            interactable.activated.Invoke(new ActivateEventArgs()); Release(); hand.enabled = false;
            Assert.IsFalse(button.IsTalking, "Dropping without a deactivation event must release PTT.");
            hand.enabled = true; yield return Select();
            interactable.activated.Invoke(new ActivateEventArgs()); button.enabled = false;
            Assert.IsFalse(button.IsTalking);
            interactable.activated.Invoke(new ActivateEventArgs()); Assert.IsFalse(button.IsTalking, "Disabled components must unsubscribe input.");
            button.enabled = true; interactable.activated.Invoke(new ActivateEventArgs()); button.enabled = false;
            Assert.AreEqual(4, presses, "Re-enabling must not duplicate listeners.");
            CollectionAssert.AreEqual(new[] { true,false,true,false,true,false,true,false }, states);
        }
    }

    public sealed class BasicInteractionSceneTests
    {
        readonly List<Scene> opened = new();
        GameObject interactionRoot;
        int[] originalManagerIds;
        static int[] ManagerIds() => Object.FindObjectsByType<XRInteractionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Select(manager => manager.GetInstanceID()).OrderBy(id => id).ToArray();

        [UnitySetUp] public IEnumerator CreateOwnedInteractionManager()
        {
            originalManagerIds = ManagerIds();
            // Level-only loads contain interactables before any player rig. Supplying a manager
            // first prevents XRI from generating a default DontDestroyOnLoad manager that leaks
            // into later lobby/network fixtures. Never remove a manager owned by another fixture.
            interactionRoot = new GameObject("BasicInteractionSceneTests owned interaction manager");
            interactionRoot.AddComponent<XRInteractionManager>();
            yield return null;
        }

        [UnityTearDown] public IEnumerator UnloadTestScenes()
        {
            try
            {
                // Unregister and unload the fixture's interactables before destroying their manager.
                foreach (var scene in opened)
                    if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            }
            finally
            {
                opened.Clear();
                if (interactionRoot) Object.Destroy(interactionRoot);
            }
            yield return null;
            CollectionAssert.AreEqual(originalManagerIds, ManagerIds(), "Level-only interaction tests must leave the pre-test manager set unchanged, including DontDestroyOnLoad objects.");
        }
        static T[] Components<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
        static void AssertBindings<T>(Scene scene, string field, string station, string role, int expected) where T : Component
        {
            var controls = Components<T>(scene);
            Assert.AreEqual(expected, controls.Length, scene.name + " / " + typeof(T).Name);
            foreach (var control in controls)
            {
                var socket = new SerializedObject(control).FindProperty(field).objectReferenceValue as PuzzleSocket;
                Assert.IsNotNull(socket, control.name);
                Assert.AreEqual(scene, socket.gameObject.scene);
                Assert.AreEqual(station, socket.StationId); Assert.AreEqual(role, socket.Role);
                Assert.IsNotNull(socket.Anchor);
                Assert.AreEqual(station + " - " + role, socket.name, "Bind the existing exact socket, not a renamed or duplicate station.");
                Assert.AreEqual(0, control.gameObject.layer, "Quest near interaction queries Default, excluding static geometry layer 8.");
                Assert.IsTrue(control.gameObject.activeInHierarchy, "Authored control must be active and reachable, including its ancestors.");
                Assert.IsFalse(control.gameObject.isStatic);
                Assert.IsNotNull(control.GetComponent<Collider>());
                Assert.IsTrue(control.GetComponent<XRBaseInteractable>().enabled);
                Assert.IsTrue(control.GetComponent<Renderer>().enabled, "Moving source renderers must be visible outside rebuilt static batches.");
            }
        }

        [UnityTest] public IEnumerator AuthoredLegacyScenesPreserveSocketContractsAndReachableBindings()
        {
            string[] names = { "LK_Level01_Plumbing", "LK_Level02_Generator", "LK_Level03_Communications", "LK_Level04_Lantern" };
            int[] expectedSockets = { 5, 4, 5, 2 };
            var scenes = new List<Scene>();
            for (int i = 0; i < names.Length; i++)
            {
                string path = "Assets/_LighthouseKeepers/Scenes/Levels/" + names[i] + ".unity";
                var scene = SceneManager.GetSceneByPath(path);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    yield return EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
                    scene = SceneManager.GetSceneByPath(path); opened.Add(scene);
                }
                scenes.Add(scene);
                var sockets = Components<PuzzleSocket>(scene);
                Assert.AreEqual(expectedSockets[i], sockets.Length, "Preserve the sixteen existing sockets across four levels.");
                Assert.AreEqual(sockets.Length, sockets.Select(s => s.StationId + " / " + s.Role).Distinct().Count());
                foreach (var socket in sockets)
                {
                    Assert.IsNotNull(socket.Anchor);
                    Assert.AreEqual("Interaction Anchor", socket.Anchor.name);
                    Assert.IsTrue(socket.Anchor.IsChildOf(socket.transform));
                }
            }
            AssertBindings<ValveHandwheel>(scenes[0], "repairSocket", "Plumbing Repair", "Wrench", 3);
            foreach (var wheel in Components<ValveHandwheel>(scenes[0]))
            {
                var visuals = new SerializedObject(wheel).FindProperty("syncedVisuals");
                Assert.AreEqual(2, visuals.arraySize, "Both existing red wheel spokes must turn with their wheel.");
                for (int j = 0; j < visuals.arraySize; j++)
                {
                    var visual = visuals.GetArrayElementAtIndex(j).objectReferenceValue as Transform;
                    Assert.IsNotNull(visual); Assert.AreEqual("Red valve wheel spoke", visual.name);
                    Assert.IsTrue(visual.gameObject.activeInHierarchy);
                    Assert.IsFalse(visual.gameObject.isStatic); Assert.IsTrue(visual.GetComponent<Renderer>().enabled);
                }
            }
            AssertBindings<LeverSwitch>(scenes[1], "boundSocket", "Generator Service", "Starter", 1);
            AssertBindings<PushToTalkButton>(scenes[2], "boundSocket", "Radio Relay", "Push to talk", 1);
            AssertBindings<RotaryDial>(scenes[2], "boundSocket", "Radio Relay", "Frequency dial", 1);
            AssertBindings<LeverSwitch>(scenes[3], "boundSocket", "Breaker Coordination", "Beacon status", 1);
            AssertBindings<RotaryDial>(scenes[3], "boundSocket", "Lantern Alignment", "Bearing controls", 1);
        }
    }
}
#endif
