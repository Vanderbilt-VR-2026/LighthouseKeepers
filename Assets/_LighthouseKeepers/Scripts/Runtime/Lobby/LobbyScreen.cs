using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LighthouseKeepers.Lobby {
/// <summary>Drop-in lobby screen: add to any scene (e.g. Bootstrap) and it
/// builds its own overlay Canvas, finds or provisions a LobbyManager, and
/// wires Host/Join/Ready/Start/Leave. Screen-space overlay renders in both
/// desktop previews and Quest builds. Scale by editing rows here or by
/// replacing this view; rules stay in LobbyManager.</summary>
public sealed class LobbyScreen:MonoBehaviour {
 [SerializeField] LobbyManager manager;
 Text titleText,statusText,rosterText;
 InputField nameField,addressField;
 Button hostButton,joinButton,readyButton,startButton,leaveButton;
 UnityAction rosterHandler;
 UnityAction<bool> connectionHandler;
 void Awake(){if(!manager){manager=FindAnyObjectByType<LobbyManager>();}
  if(!manager){var go=new GameObject("Lobby");manager=go.AddComponent<LobbyManager>();}
  rosterHandler=Refresh;connectionHandler=_=>Refresh();
  BuildUi();Bind();}
 void OnEnable(){if(manager){manager.OnRosterChanged.AddListener(rosterHandler);manager.OnConnectionChanged.AddListener(connectionHandler);}Refresh();}
 void OnDisable(){if(manager){manager.OnRosterChanged.RemoveListener(rosterHandler);manager.OnConnectionChanged.RemoveListener(connectionHandler);}}
 public void Bind(){if(manager==null)return;hostButton.onClick.RemoveAllListeners();joinButton.onClick.RemoveAllListeners();
  readyButton.onClick.RemoveAllListeners();startButton.onClick.RemoveAllListeners();leaveButton.onClick.RemoveAllListeners();
  hostButton.onClick.AddListener(()=>{manager.PlayerName=nameField.text;manager.Host();});
  joinButton.onClick.AddListener(()=>{manager.PlayerName=nameField.text;manager.Join(addressField.text);});
  readyButton.onClick.AddListener(()=>manager.ToggleReady());
  startButton.onClick.AddListener(()=>manager.StartGame());
  leaveButton.onClick.AddListener(()=>manager.Leave());}
 public void Refresh(){if(manager==null||titleText==null)return;
  titleText.text="Lighthouse Keepers — Lobby";
  var players=manager.Players;
  statusText.text=manager.IsOnline?($"{(manager.IsHost?"Hosting":"Joined")}  {players.Count}/{manager.MaxPlayers} keepers"):"Offline — host or join a session.";
  var rows=new System.Text.StringBuilder();
  var localId=manager.Transport!=null?manager.Transport.LocalPlayerId:"";
  foreach(var p in players)rows.AppendLine($"{(p.Id==localId?"> ":"  ")}{p}");
  if(players.Count==0)rows.AppendLine("(empty)");
  rosterText.text=rows.ToString();
  hostButton.interactable=!manager.IsOnline;
  joinButton.interactable=!manager.IsOnline;
  readyButton.interactable=manager.IsOnline;
  leaveButton.interactable=manager.IsOnline;
  startButton.interactable=manager.CanStart;}
 void BuildUi(){var font=Resources.GetBuiltinResource<Font>("Arial.ttf");
  var canvasGo=new GameObject("LobbyCanvas");canvasGo.transform.SetParent(transform,false);
  var canvas=canvasGo.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
  canvasGo.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
  canvasGo.AddComponent<GraphicRaycaster>();
  var bg=NewUi("Background",canvasGo.transform,font);var bgImg=bg.gameObject.AddComponent<Image>();
  bgImg.color=new Color(0.02f,0.05f,0.08f,0.92f);Stretch(bg);
  var panel=NewUi("Panel",canvasGo.transform,font);var panelImg=panel.gameObject.AddComponent<Image>();
  panelImg.color=new Color(0.08f,0.14f,0.19f);StretchCentered(panel,600,720);
  titleText=AddLabel(panel,"Title",font,28,-48);
  statusText=AddLabel(panel,"Status",font,18,-104);
  nameField=AddInput(panel,"Name",font,-164,"Keeper name");addressField=AddInput(panel,"Address",font,-214,"Host address");
  rosterText=AddLabel(panel,"Roster",font,18,-400);rosterText.rectTransform.sizeDelta=new Vector2(520,300);
  rosterText.alignment=TextAnchor.UpperLeft;
  hostButton=AddButton(panel,"Host",font,-224);joinButton=AddButton(panel,"Join",font,-112);
  readyButton=AddButton(panel,"Ready",font,0);startButton=AddButton(panel,"Start",font,112);leaveButton=AddButton(panel,"Leave",font,224);}
 static RectTransform NewUi(string name,Transform parent,Font font){var go=new GameObject(name);go.transform.SetParent(parent,false);return go.AddComponent<RectTransform>();}
 static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
 static void StretchCentered(RectTransform r,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0.5f,0.5f);r.sizeDelta=new Vector2(w,h);}
 Text AddLabel(Transform parent,string name,Font font,int size,float y){var r=NewUi(name,parent,font);var t=r.gameObject.AddComponent<Text>();
  t.font=font;t.fontSize=size;t.color=Color.white;t.alignment=TextAnchor.UpperCenter;
  r.anchorMin=r.anchorMax=new Vector2(0.5f,1f);r.anchoredPosition=new Vector2(0,y);r.sizeDelta=new Vector2(480,44);return t;}
 InputField AddInput(Transform parent,string name,Font font,float y,string placeholder){var r=NewUi(name,parent,font);
  r.anchorMin=r.anchorMax=new Vector2(0.5f,1f);r.anchoredPosition=new Vector2(0,y);r.sizeDelta=new Vector2(480,40);
  var img=r.gameObject.AddComponent<Image>();img.color=new Color(0.16f,0.24f,0.30f);
  var f=r.gameObject.AddComponent<InputField>();var tr=NewUi("Text",r,font);Stretch(tr);
  var t=tr.gameObject.AddComponent<Text>();t.font=font;t.fontSize=18;t.color=Color.white;f.textComponent=t;
  var pr=NewUi("Placeholder",r,font);Stretch(pr);var pt=pr.gameObject.AddComponent<Text>();
  pt.font=font;pt.fontSize=18;pt.color=new Color(1,1,1,0.5f);pt.text=placeholder;f.placeholder=pt;return f;}
 Button AddButton(Transform parent,string label,Font font,float x){var r=NewUi(label+"Button",parent,font);
  r.anchorMin=r.anchorMax=new Vector2(0.5f,0f);r.anchoredPosition=new Vector2(x,46);r.sizeDelta=new Vector2(104,44);
  var img=r.gameObject.AddComponent<Image>();img.color=new Color(0.12f,0.35f,0.45f);
  var b=r.gameObject.AddComponent<Button>();var tr=NewUi("Text",r,font);Stretch(tr);
  var t=tr.gameObject.AddComponent<Text>();t.font=font;t.fontSize=20;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;t.text=label;return b;}
}}
