using System.Linq;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LighthouseKeepers.Net
{
    // One entry flow around the existing transport, offline launch, alphanumeric keypad and rejoin.
    public sealed class CrewLobbyBoard : MonoBehaviour
    {
        [SerializeField] CrewLobbyTransport transport;
        LobbyScreen screen;LobbyManager manager;
        enum Page {Main,Choice,Host,Join,Settings}
        Page page;GameObject[] pages=new GameObject[5];GameObject keypad;TMP_InputField codeInput;
        TMP_Text codeLabel,roster,joinStatus,hostStatus;Button start,join;float nextRefresh;
        static string S(string key)=>PlayerStrings.Get("lobby."+key);
        void Start()
        {
            screen=GetComponent<LobbyScreen>();manager=GetComponent<LobbyManager>();
            if(!screen||!screen.BoardCanvas||!transport){enabled=false;return;}
            var root=screen.BoardCanvas.transform;screen.ExternalFlow=true;
            foreach(Transform child in root)child.gameObject.SetActive(false);
            Label(root,"CrewFlowTitle",new Vector2(0,335),new Vector2(1150,85),48).text="LIGHTHOUSE KEEPERS";
            Label(root,"NetworkVersion",new Vector2(0,-443),new Vector2(1150,35),17).text="NETWORK VERSION: "+CrewNetworkVersion.Matchmaking;
            for(int i=0;i<pages.Length;i++)pages[i]=Rect("Flow"+(Page)i,root,Vector2.zero,new Vector2(1160,650)).gameObject;
            Key(pages[0].transform,"Play",new Vector2(0,40),new Vector2(600,110),()=>Show(Page.Choice));Text("Play",S("play"));
            Key(pages[0].transform,"Settings",new Vector2(-165,-130),new Vector2(300,65),()=>Show(Page.Settings));Text("Settings",S("settings"));
            Key(pages[0].transform,"Quit",new Vector2(165,-130),new Vector2(300,65),Application.Quit);Text("Quit",S("quit"));
            Key(pages[1].transform,"HostPlay",new Vector2(0,90),new Vector2(650,100),()=>{Show(Page.Host);manager.Host();Refresh();});Text("HostPlay",S("host"));
            Key(pages[1].transform,"JoinChoice",new Vector2(0,-65),new Vector2(650,100),()=>Show(Page.Join));Text("JoinChoice",S("join"));
            codeLabel=Label(pages[2].transform,"CrewCode",new Vector2(0,170),new Vector2(1100,80),56);
            roster=Label(pages[2].transform,"JoinedKeepers",new Vector2(0,35),new Vector2(1000,130),30);roster.textWrappingMode=TextWrappingModes.Normal;
            hostStatus=Label(pages[2].transform,"HostStatus",new Vector2(0,-90),new Vector2(1100,70),23);hostStatus.textWrappingMode=TextWrappingModes.Normal;
            Key(pages[2].transform,"StartCrew",new Vector2(0,-205),new Vector2(580,85),()=>transport.StartOrOffline());start=Find("StartCrew");Text("StartCrew",S("start"));
            codeInput=CreateInput(pages[3].transform);
            joinStatus=Label(pages[3].transform,"JoinStatus",new Vector2(0,240),new Vector2(1100,65),23);joinStatus.textWrappingMode=TextWrappingModes.Normal;
            BuildKeypad(pages[3].transform);
            Key(pages[3].transform,"JoinCrewCode",new Vector2(0,-245),new Vector2(580,75),()=>{manager.Join(codeInput.text);Refresh();});join=Find("JoinCrewCode");Text("JoinCrewCode",S("join"));
            Key(pages[4].transform,"LowerBoard",new Vector2(0,20),new Vector2(650,85),()=>screen.SetLowered(!screen.IsLowered));Text("LowerBoard",S("height"));
            Key(root,"FlowBack",new Vector2(0,-355),new Vector2(300,55),()=>{if(page==Page.Host||page==Page.Join)transport.LeaveGame();Show(page==Page.Main||page==Page.Choice||page==Page.Settings?Page.Main:Page.Choice);});Text("FlowBack",S("back"));
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            for(int day=2;day<=3;day++){int selected=day;Key(pages[2].transform,"DevStartDay"+day,new Vector2(day==2?-200:200,-295),new Vector2(375,45),()=>{LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=selected;});Text("DevStartDay"+day,"DEV: SELECT NIGHT "+day);}
#endif
            Show(Page.Main);
        }
        TMP_InputField CreateInput(Transform parent)
        {
            var r=Rect("CrewCodeInput",parent,new Vector2(0,145),new Vector2(600,70));r.gameObject.AddComponent<Image>().color=new Color(.11f,.19f,.19f);
            var t=Label(r,"CodeText",Vector2.zero,new Vector2(560,66),34);var hint=Label(r,"CodeHint",Vector2.zero,new Vector2(560,66),25);hint.text=S("codeHint");
            var input=r.gameObject.AddComponent<TMP_InputField>();input.textViewport=r;input.textComponent=t;input.placeholder=hint;input.characterLimit=6;input.contentType=TMP_InputField.ContentType.Alphanumeric;input.shouldHideSoftKeyboard=true;input.shouldHideMobileInput=true;
            input.onSelect.AddListener(_=>{if(keypad&&!transport.IsOnline)keypad.SetActive(true);});input.onValueChanged.AddListener(_=>Refresh());return input;
        }
        Button Find(string name)=>screen.BoardCanvas.GetComponentsInChildren<Button>(true).First(b=>b.name==name);
        void Text(string name,string value){var b=Find(name);var text=b.GetComponentInChildren<TMP_Text>();text.text=value;text.fontSize=name.StartsWith("Dev")?18:name=="Play"?40:30;if(name=="Play"||name=="StartCrew"){b.GetComponent<Image>().color=new Color(.82f,.62f,.32f);text.color=new Color(.06f,.07f,.07f);}}
        void Show(Page value){page=value;for(int i=0;i<pages.Length;i++)pages[i].SetActive(i==(int)page);Find("FlowBack").gameObject.SetActive(page!=Page.Main);if(keypad)keypad.SetActive(page==Page.Join&&!transport.IsOnline);Refresh();}
        void Update(){if(transport.IsOnline&&(page==Page.Main||page==Page.Choice))Show(transport.IsHost?Page.Host:Page.Join);if(Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.15f;Refresh();}}
        void Refresh()
        {
            if(!codeLabel)return;
            string names=string.Join("\n",transport.Players.Select(p=>p.DisplayName+(p.IsHost?" · HOST":" · GUEST")));
            codeLabel.text=transport.IsOnline?S("code")+"  "+transport.SessionName:transport.IsBusy?S("connecting"):S("offline");
            roster.text=names;hostStatus.text=transport.IsOnline?S("startHint"):transport.IsBusy?S("cancelHint"):S("offline");
            start.interactable=!transport.IsStarting;start.GetComponentInChildren<TMP_Text>().text=transport.IsStarting?S("entering"):transport.IsBusy?S("startOffline"):S("start");
            bool connected=transport.IsOnline;codeInput.gameObject.SetActive(!connected);if(keypad&&(connected||transport.IsBusy))keypad.SetActive(false);
            join.gameObject.SetActive(!connected);join.interactable=!transport.IsBusy&&CrewConnectionPolicy.TryCode(codeInput.text,out _);
            joinStatus.text=connected?names+"\n"+S("waitHost"):transport.IsBusy?S("connecting"):string.IsNullOrEmpty(transport.LastError)?S("enterCode"):transport.LastError;
        }
        void BuildKeypad(Transform root)
        {
            var panel=Rect("Join code keypad",root,new Vector2(0,-15),new Vector2(520,318));
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
