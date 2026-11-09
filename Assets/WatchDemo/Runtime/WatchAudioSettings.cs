using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
 public static class WatchAudioSettings
 {
  static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size)
  {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchoredPosition=position;r.sizeDelta=size;return r;}
  static Text Text(Transform parent,string name,string caption,Vector2 pos,Vector2 size,Font font,Material ink)
  {var t=Rect(parent,name,pos,size).gameObject.AddComponent<Text>();t.font=font;t.material=ink;t.text=caption;t.fontSize=25;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.15f,.095f,.055f);t.raycastTarget=false;return t;}
  static Button Button(Transform parent,string name,string caption,float y,Font font,Material ink)
  {var r=Rect(parent,name,new Vector2(0,y),new Vector2(490,46));var image=r.gameObject.AddComponent<Image>();image.color=new Color(.5f,.35f,.2f,.15f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;Text(r,"Caption",caption,Vector2.zero,r.sizeDelta,font,ink);return b;}
  public static void Attach(GameObject panel,Font font,Material ink,Action refresh)
  {
   var s=AudioPassSettings.Current;if(!s||!s.audioMix)return;
   var rect=(RectTransform)panel.transform;bool comfort=panel.transform.Find("TurnComfortMode")!=null;rect.sizeDelta=new Vector2(600,comfort?800:680);
   var open=Button(rect,"AudioSettingsButton",KeeperStrings.Get("menu.sound"),comfort?-350:-290,font,ink);
   var page=Rect(rect,"Sound settings page",Vector2.zero,rect.sizeDelta);Text(page,"Sound settings title",KeeperStrings.Get("menu.sound"),new Vector2(0,230),new Vector2(520,60),font,ink);
   string[] keys={"Music","Effects","Radio","VoiceChat"},labels={KeeperStrings.Get("menu.music"),KeeperStrings.Get("menu.effects"),KeeperStrings.Get("menu.radio"),KeeperStrings.Get("menu.voice")};
   for(int i=0;i<4;i++)
   {
    string key=keys[i];float y=130-i*90;
    Text(page,key+" label",labels[i],new Vector2(-175,y),new Vector2(180,50),font,ink);
    var r=Rect(page,"AudioGain"+key,new Vector2(35,y),new Vector2(240,34));var track=r.gameObject.AddComponent<Image>();track.color=new Color(.25f,.17f,.10f,.20f);
    var area=Rect(r,"Handle area",Vector2.zero,new Vector2(216,34));var handle=Rect(area,"Handle",Vector2.zero,new Vector2(24,42));var h=handle.gameObject.AddComponent<Image>();h.color=new Color(.27f,.17f,.08f);
    var slider=r.gameObject.AddComponent<Slider>();slider.targetGraphic=track;slider.handleRect=handle;slider.direction=Slider.Direction.LeftToRight;slider.minValue=0;slider.maxValue=1;slider.SetValueWithoutNotify(KeeperAudioMix.Gain(key));
    var value=Text(page,key+" value",Mathf.RoundToInt(slider.value*100)+"%",new Vector2(220,y),new Vector2(90,50),font,ink);
    slider.onValueChanged.AddListener(v=>{KeeperAudioMix.SetGain(key,v);value.text=Mathf.RoundToInt(v*100)+"%";});
   }
   var captions=Button(page,"SubtitleToggle","",-230,font,ink);var size=Button(page,"SubtitleSize","",-285,font,ink);
   void Labels(){captions.GetComponentInChildren<Text>().text=KeeperStrings.Get(HarborSubtitleSettings.Enabled?"menu.subtitlesOn":"menu.subtitlesOff");size.GetComponentInChildren<Text>().text=KeeperStrings.Get(HarborSubtitleSettings.Large?"menu.subtitleLarge":"menu.subtitleNormal");}
   captions.onClick.AddListener(()=>{HarborSubtitleSettings.Enabled=!HarborSubtitleSettings.Enabled;Labels();});size.onClick.AddListener(()=>{HarborSubtitleSettings.Large=!HarborSubtitleSettings.Large;Labels();});Labels();
   page.sizeDelta=new Vector2(600,800);
   var states=new Dictionary<GameObject,bool>();var back=Button(page,"CloseAudioSettings",KeeperStrings.Get("menu.back"),-350,font,ink);
   open.onClick.AddListener(()=>{states.Clear();foreach(Transform t in rect)if(t!=page){states[t.gameObject]=t.gameObject.activeSelf;t.gameObject.SetActive(false);}page.gameObject.SetActive(true);});
   back.onClick.AddListener(()=>{foreach(var pair in states)if(pair.Key)pair.Key.SetActive(pair.Value);page.gameObject.SetActive(false);refresh();});page.gameObject.SetActive(false);
  }
 }
}
