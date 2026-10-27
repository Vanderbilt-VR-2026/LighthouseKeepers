using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LighthouseKeepers.Lobby;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Tests
{
    // Deterministic board test transport. It does not claim Photon or headset acceptance.
    public sealed class ControllerEntryTestTransport : MonoBehaviour, ISessionLobbyTransport
    {
        public bool IsOnline { get; private set; }
        public bool IsHost { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsStarting { get; private set; }
        public int MaxPlayers => 2;
        public string LastError => "";
        public string SessionName { get; set; } = "LighthouseKeepersRoom";
        public string LocalPlayerId => "local";
        public string EnteredRoom, EnteredName;
        readonly List<LobbyPlayer> players = new();
        public IReadOnlyList<LobbyPlayer> Players => players;
        public event Action PlayersChanged;
        public event Action<bool> ConnectionChanged;
        public event Action StatusChanged;
        public event Action GameStarting;
        public bool HostGame(int capacity) => Connect(true, SessionName);
        public bool JoinGame(string room) => Connect(false, room);
        bool Connect(bool host, string room)
        {
            EnteredRoom = SessionName = room; IsHost = host; IsOnline = true;
            players.Add(new LobbyPlayer("local", EnteredName, host));
            ConnectionChanged?.Invoke(true); PlayersChanged?.Invoke(); return true;
        }
        public void LeaveGame()
        {
            IsOnline = IsHost = IsBusy = IsStarting = false; players.Clear();
            ConnectionChanged?.Invoke(false); PlayersChanged?.Invoke();
        }
        public void SetLocalName(string name) => EnteredName = name;
        public void SetLocalReady(bool ready) { players[0].IsReady = ready; PlayersChanged?.Invoke(); }
        public bool KickPlayer(string id) => false;
        public bool StartGame(string scene, int minimum, bool allReady)
        { IsStarting = true; GameStarting?.Invoke(); return true; }
        public void AddReadyGuest()
        { var guest = new LobbyPlayer("guest", "Guest", false); guest.IsReady = true; players.Add(guest); PlayersChanged?.Invoke(); }
        public void SetBusy(bool value) { IsBusy = value; StatusChanged?.Invoke(); }
    }

    public sealed class LobbyControllerEntryTests
    {
        GameObject root, createdEventSystem;
        LobbyConfig config;
        LobbyManager manager;
        LobbyScreen screen;
        ControllerEntryTestTransport transport;
        Button Button(string name) => root.GetComponentsInChildren<Button>(true).Single(button => button.name == name);
        TMP_Text Text(string name) => root.GetComponentsInChildren<TMP_Text>(true).Single(text => text.name == name);
        TMP_InputField Room => root.GetComponentsInChildren<TMP_InputField>(true).Single(field => field.name == "HostAddress");
        void Click(string name)
        {
            var button = Button(name);
            Assert.IsTrue(button.gameObject.activeInHierarchy, name + " must be visible.");
            Assert.IsTrue(button.IsInteractable(), name + " must be enabled.");
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        void Code(string value)
        { Click("RoomKeyCLEAR"); foreach (char digit in value) Click("RoomKey" + digit); }
        [UnitySetUp] public IEnumerator Setup()
        {
            bool hadEventSystem = Object.FindAnyObjectByType<EventSystem>();
            root = new GameObject("Controller room entry tests"); root.SetActive(false);
            transport = root.AddComponent<ControllerEntryTestTransport>();
            manager = root.AddComponent<LobbyManager>();
            config = ScriptableObject.CreateInstance<LobbyConfig>();
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(LobbyConfig).GetField("maxPlayers", flags).SetValue(config, 2);
            typeof(LobbyConfig).GetField("minPlayersToStart", flags).SetValue(config, 2);
            typeof(LobbyManager).GetField("config", flags).SetValue(manager, config);
            screen = root.AddComponent<LobbyScreen>();
            typeof(LobbyScreen).GetField("manager", flags).SetValue(screen, manager);
            root.SetActive(true);
            if (!hadEventSystem) createdEventSystem = EventSystem.current.gameObject;
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Object.Destroy(root); Object.Destroy(config);
            if (createdEventSystem) Object.Destroy(createdEventSystem);
            yield return null;
        }
        [UnityTest] public IEnumerator ControllerPadPreservesLeadingZeroesAndRecoversFromEmptyBackspace()
        {
            Click("CrewOptions");
            Click("RoomKey0"); Click("RoomKey0"); Click("RoomKey7");
            Assert.AreEqual("007", Room.text, "First digit replaces the default named room.");
            Click("RoomKeyBACK"); Assert.AreEqual("00", Room.text);
            Click("RoomKeyCLEAR"); Click("RoomKeyBACK"); Assert.AreEqual("", Room.text);
            Code("0123"); Assert.AreEqual("ROOM: 0123", Text("CurrentRoom").text);
            for (int i = Room.text.Length; i < 70; i++) Click("RoomKey1");
            Assert.AreEqual(64, Room.text.Length, "Room limit matches the existing transport contract.");
            yield return null;
        }
        [UnityTest] public IEnumerator HostAndGuestUseDisplayedCodeWithoutTypingAName()
        {
            Click("CrewOptions"); Code("0012"); Click("OpenPractice");
            Assert.AreEqual("0012", transport.EnteredRoom); Assert.AreEqual("Keeper", transport.EnteredName);
            Assert.AreEqual("ROOM: 0012", Text("CurrentRoom").text);
            Click("Leave"); Click("CrewOptions"); Code("0012"); Click("JoinCrew");
            Assert.AreEqual("0012", transport.EnteredRoom); Assert.IsFalse(manager.IsHost);
            Assert.IsFalse(Button("Start").IsInteractable());
            StringAssert.Contains(manager.StartHint, Text("StartHint").text);
            StringAssert.Contains("SIGNAL READY", Text("StartHint").text);
            Click("Ready"); StringAssert.Contains("host", Text("StartHint").text);
            yield return null;
        }
        [UnityTest] public IEnumerator BoardExplainsDisabledStartUntilTwoKeepersAreReady()
        {
            Click("OpenPractice");
            Assert.IsFalse(Button("Start").IsInteractable());
            StringAssert.Contains(manager.StartHint, Text("StartHint").text);
            Click("Ready"); Assert.IsFalse(Button("Start").IsInteractable());
            StringAssert.Contains(manager.StartHint, Text("StartHint").text);
            transport.AddReadyGuest(); Assert.IsTrue(Button("Start").IsInteractable());
            Click("Start"); Assert.IsFalse(Button("Ready").IsInteractable());
            Assert.IsFalse(Button("Start").IsInteractable());
            StringAssert.Contains("load", Text("StartHint").text);
            yield return null;
        }
        [UnityTest] public IEnumerator EmptyJoinShowsErrorAndAllowsControllerOnlyRecovery()
        {
            Click("CrewOptions"); Click("RoomKeyCLEAR"); Click("JoinCrew");
            Assert.IsFalse(manager.IsOnline); Assert.IsNull(transport.EnteredRoom);
            Assert.IsTrue(Text("Error").gameObject.activeInHierarchy);
            StringAssert.Contains(manager.LastError, Text("Error").text);
            Click("CrewOptions"); Code("77"); Click("JoinCrew");
            Assert.IsTrue(manager.IsOnline); Assert.AreEqual("77", transport.EnteredRoom);
            Assert.IsFalse(Text("Error").gameObject.activeInHierarchy);
            yield return null;
        }
        void AssertVisibleTextFits()
        {
            Canvas.ForceUpdateCanvases();
            foreach (var text in root.GetComponentsInChildren<TMP_Text>())
            {
                text.ForceMeshUpdate();
                Assert.IsFalse(text.isTextOverflowing, text.name + " overflows: " + text.text);
            }
        }
        void CaptureIfRequested(Camera camera, string name)
        {
            if (System.Environment.GetEnvironmentVariable("LK_CAPTURE_SPRINT3") != "1") return;
            const string directory = "Docs/Verification/Sprint3";
            System.IO.Directory.CreateDirectory(directory);
            var render = new RenderTexture(1280, 1000, 24);
            var pixels = new Texture2D(1280, 1000, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = render; camera.Render(); RenderTexture.active = render;
                pixels.ReadPixels(new Rect(0, 0, 1280, 1000), 0, 0); pixels.Apply();
                System.IO.File.WriteAllBytes(directory + "/" + name + ".png", pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null; RenderTexture.active = previous;
                render.Release(); Object.Destroy(render); Object.Destroy(pixels);
            }
        }
        [UnityTest] public IEnumerator WorldSpaceKeypadReceivesTrackedPointerAndFitsBoard()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Canvas mesh depth and tracked-pointer rendering QA require a graphics device. Run this test again with graphics enabled.");
            var cameraObject = new GameObject("Lobby verification camera");
            var camera = cameraObject.AddComponent<Camera>();
            var canvas = screen.BoardCanvas;
            var previousCamera = canvas.worldCamera;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.025f, .045f, .05f);
            camera.fieldOfView = 60; camera.nearClipPlane = .05f; camera.aspect = 1.28f;
            camera.transform.SetPositionAndRotation(canvas.transform.position - canvas.transform.forward * 2, canvas.transform.rotation);
            canvas.worldCamera = camera;
            try
            {
                yield return null;
                Assert.AreEqual(RenderMode.WorldSpace, canvas.renderMode);
                AssertVisibleTextFits(); CaptureIfRequested(camera, "Entry");
                // Options stay closed here: scrolling input text may be clipped while editing,
                // but the separate room caption must display the full transport-compatible value.
                foreach (string longRoom in new[] { new string('0', 64), new string('W', 64) })
                {
                    Room.text = longRoom;
                    Assert.AreEqual("ROOM: " + longRoom, Text("CurrentRoom").text);
                    AssertVisibleTextFits();
                }
                var captionBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, Text("CurrentRoom").rectTransform);
                var statusBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, Text("WatchStatus").rectTransform);
                var optionsBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, Button("CrewOptions").GetComponent<RectTransform>());
                Assert.Less(captionBounds.max.y, statusBounds.min.y, "Room caption must stay below watch status.");
                Assert.Greater(captionBounds.min.y, optionsBounds.max.y, "Room caption must stay above Crew Options.");
                Click("CrewOptions"); Code("0123");
                yield return null;
                AssertVisibleTextFits(); CaptureIfRequested(camera, "Keypad");
                var panel = root.GetComponentsInChildren<RectTransform>(true).Single(rect => rect.name == "CrewOptionsPanel");
                var corners = new Vector3[4];
                foreach (var button in root.GetComponentsInChildren<Button>().Where(control => control.name.StartsWith("RoomKey")))
                {
                    button.GetComponent<RectTransform>().GetWorldCorners(corners);
                    foreach (var corner in corners)
                    {
                        Vector3 local = panel.InverseTransformPoint(corner);
                        Assert.That(local.x, Is.InRange(panel.rect.xMin, panel.rect.xMax), button.name + " leaves panel horizontally.");
                        Assert.That(local.y, Is.InRange(panel.rect.yMin, panel.rect.yMax), button.name + " leaves panel vertically.");
                    }
                    Vector3 center = button.transform.position;
                    var pointer = new UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData(EventSystem.current)
                    {
                        button = PointerEventData.InputButton.Left,
                        position = camera.WorldToScreenPoint(center), layerMask = ~0,
                        rayPoints = new List<Vector3> { camera.transform.position, center + (center - camera.transform.position).normalized * .05f }
                    };
                    var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
                    Assert.IsNotEmpty(hits, button.name + " needs a tracked pointer hit.");
                    Assert.AreSame(button.gameObject, hits[0].gameObject, button.name + " must be the first UI hit.");
                    var trackedHits = new List<RaycastResult>();
                    canvas.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>().Raycast(pointer, trackedHits);
                    Assert.IsNotEmpty(trackedHits, button.name + " needs an XR raycaster hit independently of desktop input.");
                    Assert.AreSame(button.gameObject, trackedHits[0].gameObject, button.name + " must be the first XR UI hit.");
                    pointer.pointerCurrentRaycast = hits[0];
                    ExecuteEvents.Execute(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
                }
                Assert.IsEmpty(Room.text, "Tracked pointer clicks must execute the final CLEAR key.");
                Code("0123"); Click("OpenPractice"); AssertVisibleTextFits();
                Click("Ready"); AssertVisibleTextFits();
                transport.AddReadyGuest(); AssertVisibleTextFits();
            }
            finally { canvas.worldCamera = previousCamera; Object.Destroy(cameraObject); }
        }

        [UnityTest] public IEnumerator BusyConnectionLocksRoomControlsAndExplainsCancellation()
        {
            Click("CrewOptions"); Code("45"); transport.SetBusy(true);
            Assert.IsFalse(Button("RoomKey1").IsInteractable());
            Assert.IsFalse(Button("JoinCrew").IsInteractable()); Assert.IsFalse(Room.interactable);
            StringAssert.Contains(manager.StartHint, Text("StartHint").text);
            StringAssert.Contains("CANCEL CONNECTION", Text("StartHint").text);
            Click("Leave"); Click("CrewOptions");
            Assert.IsTrue(Button("RoomKey1").IsInteractable()); Assert.AreEqual("45", Room.text);
            yield return null;
        }
    }
}
