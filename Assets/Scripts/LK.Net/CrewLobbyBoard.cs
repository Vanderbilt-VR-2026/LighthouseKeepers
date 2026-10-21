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
        TMP_Text codeLabel;
        Button reconnect;
        TMP_InputField codeInput;
        float nextRefresh;
        GameObject keypad;
        void Start()
        {
            screen = GetComponent<LobbyScreen>();
            if (!screen || !screen.BoardCanvas || !transport) { enabled = false; return; }
            var root = screen.BoardCanvas.transform;
            codeLabel = Label(root, "CrewCode", new Vector2(-80, 422), new Vector2(550, 36), 24);
            var voice = Label(root, "VoiceNotice", new Vector2(0, -270), new Vector2(1150, 35), 23);
            voice.text = CrewConnectionPolicy.VoiceNotice;
            var face = Rect("Reconnect", root, new Vector2(310, -176), new Vector2(510, 82));
            var image = face.gameObject.AddComponent<Image>(); image.color = new Color(.12f, .20f, .20f);
            reconnect = face.gameObject.AddComponent<Button>(); reconnect.targetGraphic = image;
            var label = Label(face, "Label", Vector2.zero, face.sizeDelta, 26); label.text = "RECONNECT";
            face.gameObject.AddComponent<LobbyButtonFeedback>().Initialize(reconnect, label.rectTransform);
            reconnect.onClick.AddListener(() => { screen.PlaySelectionSound(); transport.Reconnect(); });
            foreach (var field in root.GetComponentsInChildren<TMP_InputField>(true))
                if (field.name == "HostAddress")
                {
                    codeInput = field; field.characterLimit = 6;
                    field.shouldHideSoftKeyboard = true; field.shouldHideMobileInput = true;
                    field.onSelect.AddListener(_ => { if (keypad) keypad.SetActive(true); });
                    field.contentType = TMP_InputField.ContentType.Alphanumeric;
                    field.text = transport.SessionName;
                    if (field.placeholder is TMP_Text hint) hint.text = "6-character join code";
                }
            BuildKeypad(root);
            Refresh();
        }
        void Update() { if (Time.unscaledTime >= nextRefresh) { nextRefresh = Time.unscaledTime + .2f; Refresh(); } }
        void Refresh()
        {
            if (!codeLabel || !transport) return;
            codeLabel.text = string.IsNullOrEmpty(transport.SessionName) ? "HOST TO GET A JOIN CODE" : "JOIN CODE  " + transport.SessionName;
            if (keypad && (transport.IsOnline || transport.IsBusy)) keypad.SetActive(false);
            reconnect.interactable = transport.CanReconnect;
            reconnect.gameObject.SetActive(!keypad || !keypad.activeSelf);
            if (codeInput && (transport.IsOnline || transport.IsBusy)) codeInput.SetTextWithoutNotify(transport.SessionName);
        }
        void BuildKeypad(Transform root)
        {
            var panel=Rect("Join code keypad",root,new Vector2(310,-30),new Vector2(520,330));
            keypad=panel.gameObject;panel.gameObject.AddComponent<Image>().color=new Color(.055f,.10f,.11f);
            for(int i=0;i<CrewConnectionPolicy.Alphabet.Length;i++)
            {
                string letter=CrewConnectionPolicy.Alphabet[i].ToString();
                Key(panel,letter,new Vector2((i%8-3.5f)*64,115-(i/8)*58),new Vector2(58,52),()=>
                { if(codeInput.text.Length<6)codeInput.SetTextWithoutNotify(codeInput.text+letter); });
            }
            Key(panel,"BACKSPACE",new Vector2(-128,-126),new Vector2(246,52),()=>
            { if(codeInput.text.Length>0)codeInput.SetTextWithoutNotify(codeInput.text.Substring(0,codeInput.text.Length-1)); });
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
