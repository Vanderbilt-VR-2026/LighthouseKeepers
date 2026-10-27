using LighthouseKeepers.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LighthouseKeepers.Net
{
    /// <summary>Adds recovery and voice information to the existing physical board without replacing its design.</summary>
    public sealed class CrewLobbyBoard : MonoBehaviour
    {
        [SerializeField] CrewLobbyTransport transport;
        LobbyScreen screen;
        TMP_Text codeLabel, launchHint;
        LobbyManager manager;
        Button reconnect, solo, optionsSolo;
        TMP_InputField codeInput;
        float nextRefresh;
        GameObject keypad;
        void Start()
        {
            screen = GetComponent<LobbyScreen>();
            manager = GetComponent<LobbyManager>();
            if (!screen || !screen.BoardCanvas || !transport) { enabled = false; return; }
            var root = screen.BoardCanvas.transform;
            // Readiness is already shown on the ledger and ready button. Use the top-right
            // status area for an always-visible route into the game without a crew.
            foreach (string name in new[] { "WatchStatus", "WatchLamp" })
            {
                var status = root.Find(name);
                if (status) status.gameObject.SetActive(false);
            }
            solo = SoloButton(root, "PlaySolo", new Vector2(310, 175), new Vector2(510, 82));
            var options = root.Find("CrewOptionsPanel");
            if (options) optionsSolo = SoloButton(options, "PlaySoloOptions", new Vector2(0, -65), new Vector2(500, 70));
            codeLabel = Label(root, "CrewCode", new Vector2(-80, 422), new Vector2(550, 36), 24);
            launchHint = Label(root, "LaunchStatus", new Vector2(0, -267), new Vector2(1150, 82), 22);
            launchHint.textWrappingMode = TextWrappingModes.Normal;
            var face = Rect("Reconnect", root, new Vector2(310, -176), new Vector2(510, 82));
            var image = face.gameObject.AddComponent<Image>(); image.color = new Color(.12f, .20f, .20f);
            reconnect = face.gameObject.AddComponent<Button>(); reconnect.targetGraphic = image;
            var label = Label(face, "Label", Vector2.zero, face.sizeDelta, 26); label.text = "RECONNECT";
            face.gameObject.AddComponent<LobbyButtonFeedback>().Initialize(reconnect, label.rectTransform);
            reconnect.onClick.AddListener(() => { screen.PlaySelectionSound(); transport.Reconnect(); });
            foreach (var field in root.GetComponentsInChildren<TMP_InputField>(true))
            {
                if (field.name == "KeeperName")
                {
                    field.shouldHideSoftKeyboard = true;
                    if (field.placeholder is TMP_Text nameHint) nameHint.text = "Keeper name (optional)";
                }
                if (field.name == "HostAddress")
                {
                    codeInput = field; field.characterLimit = 6;
                    field.shouldHideSoftKeyboard = true; field.shouldHideMobileInput = true;
                    field.onSelect.AddListener(_ => { if (keypad) keypad.SetActive(true); });
                    field.onValueChanged.AddListener(_ => Refresh());
                    field.contentType = TMP_InputField.ContentType.Alphanumeric;
                    field.text = transport.SessionName;
                    if (field.placeholder is TMP_Text hint) hint.text = "6-character join code";
                }
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var debugFace = Rect("DebugSoloGates", root, new Vector2(-280, -450), new Vector2(490, 45));
            var debugImage = debugFace.gameObject.AddComponent<Image>(); debugImage.color = new Color(.12f, .20f, .20f);
            var debugButton = debugFace.gameObject.AddComponent<Button>(); debugButton.targetGraphic = debugImage;
            var debugLabel = Label(debugFace, "Label", Vector2.zero, debugFace.sizeDelta, 15);
            void UpdateDebugLabel() => debugLabel.text = "Debug: allow solo co-op gates: " + (DebugSoloOverride.Enabled ? "ON" : "OFF");
            UpdateDebugLabel();
            debugButton.onClick.AddListener(() => { DebugSoloOverride.Enabled = !DebugSoloOverride.Enabled; UpdateDebugLabel(); });
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // A one-shot launch choice, next to the existing debug control; no world selector panels.
            Key(root, "DevStartDay2", new Vector2(120, -450), new Vector2(255, 45), () =>
            {
                LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay = 2;
                transport.PlaySolo();
            });
            root.Find("DevStartDay2").GetComponentInChildren<TMP_Text>().text = "DEV: PLAY DAY 2";
            Key(root,"DevStartDay3",new Vector2(405,-450),new Vector2(255,45),()=>
            {LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=3;transport.PlaySolo();});
            root.Find("DevStartDay3").GetComponentInChildren<TMP_Text>().text="DEV: PLAY DAY 3";
#endif
            BuildKeypad(root);
            Refresh();
        }
        void Update() { if (Time.unscaledTime >= nextRefresh) { nextRefresh = Time.unscaledTime + .2f; Refresh(); } }
        void Refresh()
        {
            if (!codeLabel || !transport) return;
            if (launchHint && manager)
                launchHint.text = manager.StartHint + "\n" + (!transport.IsOnline && !transport.IsBusy
                    ? "PLAY SOLO works offline. HOST CREW creates a code; guest enters it in CREW OPTIONS."
                    : transport.Players.Count > 1 ? "Crew connection and ready signals work; gameplay launch is held for shared state."
                    : "SIGNAL READY, then BEGIN WATCH to debug alone. PLAY SOLO remains available.");
            codeLabel.text = string.IsNullOrEmpty(transport.SessionName) ? "HOST TO GET A JOIN CODE" : "JOIN CODE  " + transport.SessionName;
            if (keypad && (transport.IsOnline || transport.IsBusy)) keypad.SetActive(false);
            reconnect.interactable = transport.CanReconnect;
            solo.interactable = !transport.LaunchingSolo && !transport.IsStarting;
            solo.GetComponentInChildren<TMP_Text>().text = transport.LaunchingSolo ? "ENTERING SOLO..." : "PLAY SOLO";
            if (optionsSolo) optionsSolo.interactable = solo.interactable;
            reconnect.gameObject.SetActive(!keypad || !keypad.activeSelf);
            if (codeInput && (transport.IsOnline || transport.IsBusy)) codeInput.SetTextWithoutNotify(transport.SessionName);
        }
        Button SoloButton(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var face = Rect(name, parent, position, size);
            var image = face.gameObject.AddComponent<Image>(); image.color = new Color(.82f, .62f, .32f);
            var button = face.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var label = Label(face, "Label", Vector2.zero, size, 26); label.text = "PLAY SOLO";
            label.color = new Color(.06f, .07f, .07f);
            face.gameObject.AddComponent<LobbyButtonFeedback>().Initialize(button, label.rectTransform);
            button.onClick.AddListener(() => { screen.PlaySelectionSound(); transport.PlaySolo(); });
            return button;
        }
        void BuildKeypad(Transform root)
        {
            var panel=Rect("Join code keypad",root,new Vector2(310,-36),new Vector2(520,318));
            keypad=panel.gameObject;panel.gameObject.AddComponent<Image>().color=new Color(.055f,.10f,.11f);
            for(int i=0;i<CrewConnectionPolicy.Alphabet.Length;i++)
            {
                string letter=CrewConnectionPolicy.Alphabet[i].ToString();
                Key(panel,letter,new Vector2((i%8-3.5f)*64,115-(i/8)*58),new Vector2(58,52),()=>
                { if(codeInput.text.Length<6)codeInput.text += letter; });
            }
            Key(panel,"BACKSPACE",new Vector2(-128,-126),new Vector2(246,52),()=>
            { if(codeInput.text.Length>0)codeInput.text = codeInput.text.Substring(0,codeInput.text.Length-1); });
            Key(panel,"CLOSE KEYPAD",new Vector2(128,-126),new Vector2(246,52),()=>keypad.SetActive(false));
            keypad.SetActive(false);
        }
        void Key(Transform panel,string value,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action)
        {
            var face=Rect(value,panel,position,size);var image=face.gameObject.AddComponent<Image>();image.color=new Color(.12f,.20f,.20f);
            var button=face.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var label=Label(face,"Label",Vector2.zero,size,value.Length==1?26:20);label.text=value;
            face.gameObject.AddComponent<LobbyButtonFeedback>().Initialize(button,label.rectTransform);
            button.onClick.AddListener(action);button.onClick.AddListener(screen.PlaySelectionSound);
        }
        static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static TMP_Text Label(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
        {
            var text = Rect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset; text.fontSize = fontSize;
            text.color = new Color(.93f, .88f, .74f); text.richText = false; text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }
    }
}
