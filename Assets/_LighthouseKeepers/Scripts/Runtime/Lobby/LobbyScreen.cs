using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LighthouseKeepers.Lobby
{
    /// <summary>A room-anchored watch board. Desktop and XR use the same physical canvas.</summary>
    public sealed class LobbyScreen : MonoBehaviour
    {
        [SerializeField] LobbyManager manager;
        [SerializeField] Texture paperTexture;
        [SerializeField] AudioClip selectSound;
        [SerializeField] Transform boardMount;
        Canvas canvas;
        TMP_Text statusText, hintText, modeText, crewCount, readyCaption;
        readonly TMP_Text[] rosterRows = new TMP_Text[8];
        TMP_InputField nameField, addressField;
        Button hostButton, joinButton, readyButton, startButton, leaveButton, heightButton;
        GameObject options, roster, addressGroup;
        Image watchLamp;
        AudioSource audioSource;
        Vector3 mountPosition;
        bool lowered;
        static readonly Color Slate = new(0.055f, 0.10f, 0.11f);
        static readonly Color Paper = new(0.85f, 0.80f, 0.65f);
        static readonly Color Ink = new(0.14f, 0.17f, 0.16f);
        static readonly Color Chalk = new(0.93f, 0.88f, 0.74f);
        static readonly Color Muted = new(0.61f, 0.69f, 0.66f);
        static readonly Color Brass = new(0.72f, 0.49f, 0.22f);
        static readonly Color Ready = new(0.47f, 0.77f, 0.58f);
        public Canvas BoardCanvas => canvas;
        public bool IsLowered => lowered;

        void Awake()
        {
            if (!manager) manager = FindAnyObjectByType<LobbyManager>();
            if (!manager) manager = gameObject.AddComponent<LobbyManager>();
            if (!boardMount) boardMount = transform;
            mountPosition = boardMount.localPosition;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 0.5f;
            audioSource.maxDistance = 5;
            BuildUi();
            Bind();
            if (!FindAnyObjectByType<EventSystem>())
                new GameObject("Lobby EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
        }
        void Start() { Refresh(); canvas.worldCamera = Camera.main; }
        void OnEnable()
        {
            if (canvas) canvas.gameObject.SetActive(true);
            if (!manager) return;
            manager.OnRosterChanged.AddListener(Refresh);
            manager.OnConnectionChanged.AddListener(ConnectionChanged);
            manager.OnGameStarting.AddListener(Refresh);
            Refresh();
        }
        void OnDisable()
        {
            if (canvas) canvas.gameObject.SetActive(false);
            if (!manager) return;
            manager.OnRosterChanged.RemoveListener(Refresh);
            manager.OnConnectionChanged.RemoveListener(ConnectionChanged);
            manager.OnGameStarting.RemoveListener(Refresh);
        }
        void ConnectionChanged(bool _) => Refresh();
        public void SetLowered(bool value)
        {
            lowered = value;
            // Move the authored frame and its UI together, never the camera or tracking origin.
            boardMount.localPosition = mountPosition + (lowered ? Vector3.down * 0.25f : Vector3.zero);
            SetButtonLabel(heightButton, lowered ? "RAISE BOARD" : "LOWER BOARD");
        }
        public void PlaySelectionSound()
        {
            if (selectSound) audioSource.PlayOneShot(selectSound, 0.25f);
        }
        void Bind()
        {
            hostButton.onClick.AddListener(() => { manager.PlayerName = nameField.text; manager.Host(); CloseOptions(); Refresh(); });
            joinButton.onClick.AddListener(() => { manager.PlayerName = nameField.text; manager.Join(manager.IsLocalPreview ? "local" : addressField.text); CloseOptions(); Refresh(); });
            readyButton.onClick.AddListener(() => { manager.ToggleReady(); Refresh(); });
            startButton.onClick.AddListener(() => { manager.StartGame(); Refresh(); });
            leaveButton.onClick.AddListener(() => { manager.Leave(); CloseOptions(); Refresh(); });
        }
        void CloseOptions() { options.SetActive(false); roster.SetActive(true); }
        public void Refresh()
        {
            if (!manager || !modeText) return;
            bool local = manager.IsLocalPreview;
            modeText.text = local ? "LOCAL PRACTICE" : "CREW SESSION";
            statusText.text = manager.IsOnline ? (manager.IsHost ? "Your name is in the ledger." : "Guest preview · simulated crew") : "A storm is coming. Take your post.";
            crewCount.text = $"{manager.Players.Count:00} / {manager.MaxPlayers:00} KEEPERS";
            for (int i = 0; i < rosterRows.Length; i++)
            {
                var row = rosterRows[i];
                row.gameObject.SetActive(i < manager.MaxPlayers);
                float spacing = manager.MaxPlayers > 4 ? 40 : 67;
                row.rectTransform.anchoredPosition = new Vector2(0, 84 - i * spacing);
                row.fontSize = manager.MaxPlayers > 4 ? 21 : 25;
                if (i >= manager.Players.Count) { row.text = $"{i + 1:00}    —    Open berth"; row.color = new Color(.39f,.41f,.36f); continue; }
                var player = manager.Players[i];
                string name = player.DisplayName.Length > 18 ? player.DisplayName.Substring(0, 17) + "…" : player.DisplayName;
                row.text = $"{i + 1:00}    {name}     /     {(player.IsReady ? "READY" : "NOT READY")}";
                row.color = player.IsReady ? new Color(.12f,.35f,.22f) : Ink;
            }
            bool ready = manager.LocalPlayer()?.IsReady == true;
            readyCaption.text = ready ? "WATCH STATUS / READY" : "WATCH STATUS / STANDBY";
            watchLamp.color = ready ? Ready : Brass;
            hintText.text = !string.IsNullOrEmpty(manager.LastError) ? manager.LastError : !manager.IsOnline
                ? "Sign the ledger, then signal that you are ready."
                : manager.CanStart ? "All set. The lighthouse is yours to keep." : manager.StartHint;
            SetButtonLabel(readyButton, ready ? "STAND DOWN" : "SIGNAL READY");
            hostButton.transform.parent.gameObject.SetActive(!manager.IsOnline);
            leaveButton.transform.parent.gameObject.SetActive(manager.IsOnline);
            readyButton.interactable = manager.LocalPlayer() != null && !manager.IsStarting;
            startButton.interactable = manager.CanStart;
            startButton.GetComponentInChildren<TMP_Text>().color = manager.CanStart ? Ink : Muted;
            joinButton.interactable = !manager.IsOnline;
            leaveButton.interactable = !manager.IsStarting;
            nameField.interactable = addressField.interactable = !manager.IsOnline;
            addressGroup.SetActive(!local);
            SetButtonLabel(joinButton, local ? "SIMULATE A GUEST" : "JOIN CREW");
        }
        void BuildUi()
        {
            var root = Rect("WatchBoardCanvas", transform, Vector2.zero, new Vector2(1280, 960));
            root.localScale = Vector3.one * .002f;
            canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            root.gameObject.AddComponent<GraphicRaycaster>();
            root.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            var scaler = root.gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 2;
            // The slate backing and brass frame are real scene geometry behind this transparent canvas.
            Label(root, "Station", new(-575, 422), new(720, 28), 19, Brass, "N O R T H   A T L A N T I C     /     K E E P E R   S T A T I O N");
            modeText = Label(root, "Mode", new(310, 422), new(265, 28), 19, Muted, "LOCAL PRACTICE");
            LighthouseMark(root, new Vector2(-541, 326));
            var title = Label(root, "Title", new(-447, 351), new(1010, 90), 53, Chalk, "LIGHTHOUSE KEEPERS");
            title.characterSpacing = 3;
            Label(root, "Motto", new(-445, 280), new(1000, 44), 25, Muted, "Take your place. Keep the light.");
            Rule(root, new(0, 240), new(1150, 2), Brass);
            roster = Rect("CrewLedger", root, new(-291, -11), new(570, 432)).gameObject;
            var sheet = roster.AddComponent<Image>(); sheet.color = Paper; sheet.raycastTarget = false;
            var grain = Rect("Paper grain",roster.transform,Vector2.zero,new(570,432)).gameObject.AddComponent<RawImage>();
            grain.texture = paperTexture; grain.color = new Color(1,1,1,.14f); grain.raycastTarget = false;
            Label(roster.transform, "LedgerTitle", new(-251, 175), new(390, 40), 29, Ink, "THE CREW LEDGER");
            crewCount = Label(roster.transform, "CrewCount", new(-250, 132), new(490, 30), 19, Ink, "00 / 04 KEEPERS");
            Rule(roster.transform, new(0, 110), new(500, 2), new Color(.38f,.36f,.28f));
            for (int i = 0; i < 8; i++)
            {
                rosterRows[i] = Label(roster.transform, "CrewSlot" + i, new(0, 84-i*40), new(502, 40), 25, Ink, "");
                rosterRows[i].rectTransform.pivot = new(.5f,.5f);
            }
            Label(root, "Briefing", new(55, 175), new(490, 35), 22, Brass, "BEFORE THE STORM");
            BriefingLine(root, 111, "01", "Keep the beacon burning.", "The coast is counting on you.");
            BriefingLine(root, 19, "02", "Watch the waterline.", "As the water rises, move upstairs.");
            Label(root, "Controls", new(55, -98), new(510, 70), 23, Chalk, "Point either controller at a control.\nPress the trigger to select.");
            heightButton = MakeButton(root, "BoardHeight", "LOWER BOARD", new(310,-180), new(510,66), false);
            heightButton.onClick.AddListener(() => SetLowered(!lowered));
            var details = MakeButton(root, "CrewOptions", "CREW OPTIONS", new(-291,-270), new(570,60), false);
            details.onClick.AddListener(() => { bool show = !options.activeSelf; options.SetActive(show); roster.SetActive(!show); });
            statusText = Label(root, "Status", new(55,-270), new(510,55), 23, Muted, "");
            Rule(root, new(0,-315), new(1150,2), Brass);
            hostButton = MakeButton(root, "OpenPractice", "TAKE YOUR POST", new(-386,-372), new(378,82), false);
            leaveButton = MakeButton(root, "Leave", "LEAVE WATCH", new(-386,-372), new(378,82), false);
            readyButton = MakeButton(root, "Ready", "SIGNAL READY", new(0,-372), new(350,82), false);
            startButton = MakeButton(root, "Start", "BEGIN WATCH  →", new(386,-372), new(378,82), true);
            hintText = Label(root, "StartHint", new(-575,-434), new(1150,42), 21, Muted, "");
            watchLamp = Rect("WatchLamp", root, new(75,204), new(12,12)).gameObject.AddComponent<Image>();
            watchLamp.raycastTarget = false;
            readyCaption = Label(root, "WatchStatus", new(95,203), new(470,26), 17, Brass, "WATCH STATUS / STANDBY");
            BuildOptions(root);
        }
        void BuildOptions(Transform root)
        {
            options = Rect("CrewOptionsPanel", root, new(-291,-11), new(570,432)).gameObject;
            var backing = options.AddComponent<Image>(); backing.color = Slate;
            Label(options.transform, "OptionsTitle", new(-250,169), new(500,38), 27, Chalk, "CREW OPTIONS");
            nameField = Input(options.transform, "KeeperName", new(0,99), "Keeper name");
            nameField.text = manager.PlayerName; nameField.characterLimit = 24;
            addressField = Input(options.transform, "HostAddress", new(0,18), "Host address");
            addressField.characterLimit = 128; addressGroup = addressField.gameObject;
            Label(options.transform, "SimulationNote", new(-250,-58), new(500,94), 23, Muted,
                "Local practice works without typing.\nGuest simulation is for preview only;\nit does not connect another headset.");
            joinButton = MakeButton(options.transform, "JoinCrew", "SIMULATE A GUEST", new(0,-159), new(500,70), false);
            options.SetActive(false);
        }
        void BriefingLine(Transform root, float y, string number, string title, string detail)
        {
            Label(root, "Step"+number, new(55,y), new(65,35), 27, Brass, number);
            Label(root, "Task"+number, new(120,y), new(455,38), 26, Chalk, title);
            Label(root, "Detail"+number, new(120,y-38), new(455,36), 20, Muted, detail);
        }
        static void Rule(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var image = Rect("Rule",parent,position,size).gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false;
        }
        static void LighthouseMark(Transform parent, Vector2 position)
        {
            Rule(parent,position+new Vector2(0,-37),new(66,5),Brass);
            Rule(parent,position+new Vector2(0,-7),new(26,56),Chalk);
            Rule(parent,position+new Vector2(0,29),new(44,5),Brass);
            Rule(parent,position+new Vector2(0,44),new(24,20),Brass);
            Rule(parent,position+new Vector2(0,61),new(38,5),Chalk);
            Rule(parent,position+new Vector2(-38,44),new(20,2),Brass);
            Rule(parent,position+new Vector2(38,44),new(20,2),Brass);
        }
        static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false); rect.anchorMin = rect.anchorMax = new(.5f,.5f);
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static TMP_Text Label(Transform parent, string name, Vector2 position, Vector2 size, float fontSize, Color color, string value)
        {
            var text = Rect(name,parent,position,size).gameObject.AddComponent<TextMeshProUGUI>();
            text.rectTransform.pivot = new(0,.5f);
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize; text.color = color; text.alignment = TextAlignmentOptions.MidlineLeft;
            text.richText = false; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Truncate;
            text.text = value; return text;
        }
        TMP_InputField Input(Transform parent, string name, Vector2 position, string placeholder)
        {
            var rect = Rect(name,parent,position,new(500,62));
            var image = rect.gameObject.AddComponent<Image>(); image.color = new(.13f,.21f,.21f);
            var viewport = Rect("Viewport",rect,Vector2.zero,new(460,58));
            viewport.gameObject.AddComponent<RectMask2D>();
            var input = rect.gameObject.AddComponent<TMP_InputField>(); input.targetGraphic = image; input.textViewport = viewport;
            var text = Label(viewport,"Text",new(-228,0),new(456,58),26,Chalk,"");
            input.textComponent = text;
            input.placeholder = Label(viewport,"Placeholder",new(-228,0),new(456,58),26,Muted,placeholder);
            return input;
        }
        Button MakeButton(Transform parent, string name, string label, Vector2 position, Vector2 size, bool primary)
        {
            // Inset rim and raised face give depth without moving the hit target.
            var rim = Rect(name+"Rim",parent,position,size+new Vector2(6,6));
            var border = rim.gameObject.AddComponent<Image>(); border.color = primary ? Brass : new(.3f,.35f,.32f); border.raycastTarget = false;
            var rect = Rect(name,rim,Vector2.zero,size);
            rect.localPosition += Vector3.back * 2;
            var image = rect.gameObject.AddComponent<Image>();
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = primary ? new(.82f,.62f,.32f) : new(.12f,.20f,.20f);
            colors.highlightedColor = primary ? new(.96f,.77f,.46f) : new(.25f,.36f,.32f);
            colors.selectedColor = colors.normalColor;
            colors.pressedColor = new(.38f,.47f,.37f);
            colors.disabledColor = new(.075f,.12f,.12f);
            button.colors = colors;
            var text = Label(rect,"Label",new(-size.x*.5f,0),size,26,primary ? Ink : Chalk,label);
            text.alignment = TextAlignmentOptions.Center;
            rect.gameObject.AddComponent<LobbyButtonFeedback>().Initialize(button, text.rectTransform);
            button.onClick.AddListener(PlaySelectionSound);
            return button;
        }
        static void SetButtonLabel(Button button, string value) => button.GetComponentInChildren<TMP_Text>().text = value;
    }
}
