using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LighthouseKeepers.Lobby
{
    /// <summary>Desktop briefing screen; becomes a stationary, ray-interactable world panel in XR.</summary>
    public sealed class LobbyScreen : MonoBehaviour
    {
        [SerializeField] LobbyManager manager;
        [SerializeField, Min(1)] float boardDistance = 2.2f;
        Canvas canvas;
        RectTransform canvasRect;
        Text titleText, statusText, hintText, modeText;
        readonly Text[] rosterRows = new Text[8];
        InputField nameField, addressField;
        Button hostButton, joinButton, readyButton, startButton, leaveButton;
        GameObject addressGroup;
        bool worldMode, placed;
        static readonly Color Ink = new(0.035f, 0.075f, 0.105f);
        static readonly Color Paper = new(0.94f, 0.92f, 0.84f);
        static readonly Color Muted = new(0.60f, 0.70f, 0.73f);
        static readonly Color Amber = new(0.94f, 0.66f, 0.27f);
        Font font;

        void Awake()
        {
            if (!manager) manager = FindAnyObjectByType<LobbyManager>();
            if (!manager) manager = gameObject.AddComponent<LobbyManager>();
            BuildUi();
            Bind();
            EnsureEventSystem();
        }
        void Start()
        {
            // All scene managers have now completed Awake, regardless of object order.
            Refresh();
            foreach (var text in GetComponentsInChildren<Text>(true)) text.SetAllDirty();
        }
        void OnEnable()
        {
            if (manager)
            {
                manager.OnRosterChanged.AddListener(Refresh);
                manager.OnConnectionChanged.AddListener(ConnectionChanged);
                manager.OnGameStarting.AddListener(Refresh);
            }
            Refresh();
        }
        void OnDisable()
        {
            if (!manager) return;
            manager.OnRosterChanged.RemoveListener(Refresh);
            manager.OnConnectionChanged.RemoveListener(ConnectionChanged);
            manager.OnGameStarting.RemoveListener(Refresh);
        }
        void ConnectionChanged(bool _) => Refresh();
        void LateUpdate()
        {
            bool xr = XRSettings.isDeviceActive;
            if (xr != worldMode || (xr && !placed)) ConfigureCanvas(xr);
        }
        public void Bind()
        {
            hostButton.onClick.RemoveAllListeners();
            joinButton.onClick.RemoveAllListeners();
            readyButton.onClick.RemoveAllListeners();
            startButton.onClick.RemoveAllListeners();
            leaveButton.onClick.RemoveAllListeners();
            hostButton.onClick.AddListener(() => { manager.PlayerName = nameField.text; manager.Host(); Refresh(); });
            joinButton.onClick.AddListener(() => { manager.PlayerName = nameField.text; manager.Join(manager.IsLocalPreview ? "local" : addressField.text); Refresh(); });
            readyButton.onClick.AddListener(() => { manager.ToggleReady(); Refresh(); });
            startButton.onClick.AddListener(() => { manager.StartGame(); Refresh(); });
            leaveButton.onClick.AddListener(() => { manager.Leave(); Refresh(); });
        }
        public void Refresh()
        {
            if (!manager || !titleText) return;
            titleText.text = manager.Title.ToUpperInvariant();
            bool local = manager.IsLocalPreview;
            modeText.text = local ? "LOCAL PRACTICE  /  NO NETWORK CONNECTION" : "CREW SESSION";
            statusText.text = manager.IsOnline
                ? $"{(local ? (manager.IsHost ? "Practice crew" : "Simulated guest crew") : (manager.IsHost ? "Hosting crew" : "Joined crew"))}  /  {manager.Players.Count} of {manager.MaxPlayers}"
                : "Your watch starts here.";
            hintText.text = string.IsNullOrEmpty(manager.LastError) ? manager.StartHint : manager.LastError;
            hintText.color = string.IsNullOrEmpty(manager.LastError) ? Muted : Amber;
            for (int i = 0; i < rosterRows.Length; i++)
            {
                var row = rosterRows[i];
                row.transform.parent.gameObject.SetActive(i < manager.MaxPlayers);
                if (i >= manager.Players.Count)
                {
                    row.text = $"{i + 1:00}    —    Empty crew slot";
                    row.color = Muted;
                    continue;
                }
                var player = manager.Players[i];
                string you = player.Id == manager.Transport.LocalPlayerId ? " (you)" : "";
                string role = player.IsHost ? "HOST" : "CREW";
                row.text = $"{i + 1:00}    {player.DisplayName}{you}   /   {role}   /   {(player.IsReady ? "READY" : "NOT READY")}";
                row.color = player.IsReady ? new Color(0.62f, 0.86f, 0.73f) : Paper;
            }
            SetButtonLabel(hostButton, local ? "OPEN PRACTICE" : "HOST CREW");
            SetButtonLabel(joinButton, local ? "PREVIEW GUEST" : "JOIN CREW");
            SetButtonLabel(readyButton, manager.LocalPlayer()?.IsReady == true ? "UNREADY" : "I'M READY");
            hostButton.interactable = joinButton.interactable = !manager.IsOnline;
            readyButton.interactable = manager.LocalPlayer() != null && !manager.IsStarting;
            startButton.interactable = manager.CanStart;
            startButton.GetComponentInChildren<Text>().color = manager.CanStart ? Ink : Muted;
            leaveButton.interactable = manager.IsOnline && !manager.IsStarting;
            nameField.interactable = addressField.interactable = !manager.IsOnline;
            addressGroup.SetActive(!local);
        }
        void BuildUi()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            // Populate the shared dynamic atlas before building labels at several sizes.
            // This prevents early label meshes retaining UVs from an intermediate atlas.
            const string glyphs = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 /.,:;!?'-()_—";
            foreach (int size in new[] { 16, 18, 20, 21, 22, 38 })
                font.RequestCharactersInTexture(glyphs, size, FontStyle.Normal);
            canvasRect = Rect("LobbyCanvas", transform, Vector2.zero, new Vector2(1100, 740));
            canvas = canvasRect.gameObject.AddComponent<Canvas>();
            canvas.sortingOrder = 100;
            var scaler = canvasRect.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 860);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasRect.gameObject.AddComponent<GraphicRaycaster>();
            canvasRect.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            var panel = Rect("BriefingBoard", canvasRect, Vector2.zero, new Vector2(1100, 740));
            panel.gameObject.AddComponent<Image>().color = Ink;
            var rule = Rect("BrassRule", panel, new Vector2(0, 345), new Vector2(1020, 3));
            rule.gameObject.AddComponent<Image>().color = Amber;
            modeText = Label(panel, "Mode", new Vector2(0, 309), new Vector2(1000, 28), 16, Amber);
            titleText = Label(panel, "Title", new Vector2(0, 261), new Vector2(1000, 58), 38, Paper);
            statusText = Label(panel, "Status", new Vector2(0, 210), new Vector2(1000, 34), 22, Muted);
            Label(panel, "Briefing", new Vector2(-336, 136), new Vector2(320, 40), 21, Amber).text = "PREPARE FOR YOUR WATCH";
            var briefing = Label(panel, "Instructions", new Vector2(-336, 61), new Vector2(320, 108), 20, Paper);
            briefing.alignment = TextAnchor.UpperLeft;
            briefing.text = "Keep the light burning.\nWatch the water.\nFind higher ground as it rises.";
            nameField = Input(panel, "KeeperName", new Vector2(-336, -38), "Keeper name");
            nameField.text = manager.PlayerName;
            nameField.characterLimit = 24;
            addressField = Input(panel, "HostAddress", new Vector2(-336, -96), "Host address");
            addressField.characterLimit = 128;
            addressGroup = addressField.gameObject;
            hostButton = Button(panel, "OpenPractice", "OPEN PRACTICE", new Vector2(-336, -163), new Vector2(320, 52), false);
            joinButton = Button(panel, "JoinCrew", "PREVIEW GUEST", new Vector2(-336, -228), new Vector2(320, 52), false);
            Label(panel, "CrewHeading", new Vector2(194, 142), new Vector2(596, 36), 21, Amber).text = "CREW MANIFEST";
            for (int i = 0; i < rosterRows.Length; i++)
            {
                var row = Rect("CrewSlot" + i, panel, new Vector2(194, 92 - i * 43), new Vector2(596, 37));
                row.gameObject.AddComponent<Image>().color = new Color(0.085f, 0.14f, 0.17f);
                rosterRows[i] = Label(row, "Player", Vector2.zero, new Vector2(570, 35), 18, Paper);
                rosterRows[i].alignment = TextAnchor.MiddleLeft;
            }
            hintText = Label(panel, "StartHint", new Vector2(194, -255), new Vector2(596, 40), 18, Muted);
            readyButton = Button(panel, "Ready", "I'M READY", new Vector2(24, -310), new Vector2(200, 56), false);
            startButton = Button(panel, "Start", "ENTER LIGHTHOUSE", new Vector2(337, -310), new Vector2(330, 56), true);
            leaveButton = Button(panel, "Leave", "LEAVE CREW", new Vector2(-336, -310), new Vector2(320, 56), false);
            ConfigureCanvas(XRSettings.isDeviceActive);
        }
        void ConfigureCanvas(bool xr)
        {
            worldMode = xr;
            placed = false;
            canvas.renderMode = xr ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
            if (!xr) { canvasRect.localScale = Vector3.one; return; }
            var camera = Camera.main;
            if (!camera) return;
            canvas.worldCamera = camera;
            canvasRect.sizeDelta = new Vector2(1100, 740);
            canvasRect.localScale = Vector3.one * 0.0018f;
            var forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.1f) forward = Vector3.forward;
            canvasRect.position = camera.transform.position + forward * boardDistance;
            canvasRect.rotation = Quaternion.LookRotation(forward, Vector3.up);
            placed = true; // Stationary board: never follows the player's head.
        }
        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>()) return;
            new GameObject("Lobby EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
        }
        static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
        Text Label(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var text = Rect(name, parent, position, size).gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false; // Player names are always literal text.
            text.raycastTarget = false;
            return text;
        }
        InputField Input(Transform parent, string name, Vector2 position, string placeholder)
        {
            var rect = Rect(name, parent, position, new Vector2(320, 48));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.10f, 0.18f, 0.22f);
            var input = rect.gameObject.AddComponent<InputField>();
            input.targetGraphic = image;
            input.textComponent = Label(rect, "Text", Vector2.zero, new Vector2(292, 44), 20, Paper);
            var hint = Label(rect, "Placeholder", Vector2.zero, new Vector2(292, 44), 20, Muted);
            hint.text = placeholder;
            input.placeholder = hint;
            return input;
        }
        Button Button(Transform parent, string name, string label, Vector2 position, Vector2 size, bool primary)
        {
            var rect = Rect(name, parent, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = primary ? Amber : new Color(0.16f, 0.27f, 0.31f);
            colors.highlightedColor = primary ? new Color(1, 0.78f, 0.40f) : new Color(0.23f, 0.38f, 0.43f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.45f, 0.53f, 0.53f);
            colors.disabledColor = new Color(0.12f, 0.17f, 0.19f);
            button.colors = colors;
            var text = Label(rect, "Label", Vector2.zero, size, 20, primary ? Ink : Paper);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            return button;
        }
        static void SetButtonLabel(Button button, string text) => button.GetComponentInChildren<Text>().text = text;
    }
}
