using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class CrewLobbyBoardTests
    {
        GameObject root, createdEvents;
        LobbyScreen screen;
        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        TMP_InputField Code => root.GetComponentsInChildren<TMP_InputField>(true).Single(input => input.name == "HostAddress");
        Button Button(string name) => root.GetComponentsInChildren<Button>(true).Single(button => button.name == name);
        [UnitySetUp] public IEnumerator Setup()
        {
            bool hadEvents = Object.FindAnyObjectByType<EventSystem>();
            root = new GameObject("Alphanumeric crew board fixture"); root.SetActive(false);
            var fusion = root.AddComponent<FusionLobbyTransport>();
            var crew = root.AddComponent<CrewLobbyTransport>();
            typeof(CrewLobbyTransport).GetField("fusion", Private).SetValue(crew, fusion);
            var manager = root.AddComponent<LobbyManager>();
            typeof(LobbyManager).GetField("transportBehaviour", Private).SetValue(manager, crew);
            screen = root.AddComponent<LobbyScreen>();
            typeof(LobbyScreen).GetField("manager", Private).SetValue(screen, manager);
            var board = root.AddComponent<CrewLobbyBoard>();
            typeof(CrewLobbyBoard).GetField("transport", Private).SetValue(board, crew);
            root.SetActive(true); yield return null;
            if (!hadEvents) createdEvents = EventSystem.current.gameObject;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        { Object.Destroy(root); if (createdEvents) Object.Destroy(createdEvents); yield return null; }
        void Click(Button button, PointerEventData data = null)
        {
            Assert.IsTrue(button.IsActive() && button.IsInteractable(), button.name);
            ExecuteEvents.Execute(button.gameObject, data ?? new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        void OpenPad()
        {
            Click(Button("CrewOptions")); Code.text = "";
            ExecuteEvents.Execute(Code.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.selectHandler);
        }
        TrackedDeviceEventData TrackedPointer(Camera camera, Button button)
        {
            Vector3 center = button.transform.position;
            var pointer = new TrackedDeviceEventData(EventSystem.current) { button = PointerEventData.InputButton.Left,
                position = camera.WorldToScreenPoint(center), layerMask = ~0,
                rayPoints = new List<Vector3> { camera.transform.position, center + (center - camera.transform.position).normalized * .05f } };
            var hits = new List<RaycastResult>();
            screen.BoardCanvas.GetComponent<TrackedDeviceGraphicRaycaster>().Raycast(pointer, hits);
            Assert.IsNotEmpty(hits, button.name); Assert.AreSame(button.gameObject, hits[0].gameObject, button.name + " must receive the first XR UI hit.");
            pointer.pointerCurrentRaycast = hits[0]; return pointer;
        }
        void AssertTextFits()
        {
            Canvas.ForceUpdateCanvases();
            foreach (var text in root.GetComponentsInChildren<TMP_Text>())
            { text.ForceMeshUpdate(); Assert.IsFalse(text.isTextOverflowing, text.name + ": " + text.text); }
        }
        [UnityTest] public IEnumerator ExistingAlphanumericPadSupportsSixCharactersAndBackspaceWithoutKeyboard()
        {
            OpenPad();
            foreach (char character in "ABC234") Click(Button(character.ToString()));
            Assert.AreEqual("ABC234", Code.text); Assert.IsTrue(CrewConnectionPolicy.TryCode(Code.text, out _));
            Click(Button("Z")); Assert.AreEqual("ABC234", Code.text);
            Click(Button("BACKSPACE")); Click(Button("Z")); Assert.AreEqual("ABC23Z", Code.text);
            Assert.AreEqual(0, root.GetComponentsInChildren<Button>(true).Count(button => button.name.StartsWith("RoomKey")), "The numeric-only duplicate keypad must be absent.");
            yield return null;
        }
        [UnityTest] public IEnumerator AlphanumericPadReceivesFirstTrackedHitAndDoesNotCoverSolo()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Run rendered tracked-pointer QA with graphics enabled.");
            var cameraObject = new GameObject("Crew board verification camera"); var camera = cameraObject.AddComponent<Camera>();
            var canvas = screen.BoardCanvas; var previous = canvas.worldCamera;
            camera.fieldOfView = 60; camera.nearClipPlane = .05f; camera.aspect = 1.28f;
            camera.transform.SetPositionAndRotation(canvas.transform.position - canvas.transform.forward * 2, canvas.transform.rotation);
            canvas.worldCamera = camera;
            try
            {
                OpenPad(); yield return null; AssertTextFits();
                var pad = root.GetComponentsInChildren<RectTransform>(true).Single(rect => rect.name == "Join code keypad");
                var padBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, pad);
                var soloBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, Button("PlaySolo").GetComponent<RectTransform>());
                Assert.Less(padBounds.max.y, soloBounds.min.y, "The keypad must leave PLAY SOLO fully reachable.");
                foreach (var button in pad.GetComponentsInChildren<Button>())
                {
                    var corners = new Vector3[4]; button.GetComponent<RectTransform>().GetWorldCorners(corners);
                    foreach (var corner in corners)
                    { var local = pad.InverseTransformPoint(corner); Assert.IsTrue(pad.rect.Contains(local), button.name + " leaves keypad bounds."); }
                    if (button.name == "CLOSE KEYPAD") continue;
                    TrackedPointer(camera, button);
                }
                TrackedPointer(camera, Button("PlaySolo"));
                foreach (char character in "ABC234")
                { var button = Button(character.ToString()); Click(button, TrackedPointer(camera, button)); }
                Assert.AreEqual("ABC234", Code.text); AssertTextFits();
                Click(Button("CLOSE KEYPAD")); AssertTextFits();
            }
            finally { canvas.worldCamera = previous; Object.Destroy(cameraObject); }
        }
    }
}
