using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
 public static class HarborSubtitleSettings
 {
  public static bool Enabled {get=>PlayerPrefs.GetInt("LK.Subtitles.Enabled",1)!=0;set{PlayerPrefs.SetInt("LK.Subtitles.Enabled",value?1:0);PlayerPrefs.Save();}}
  public static bool Large {get=>PlayerPrefs.GetInt("LK.Subtitles.Large",0)!=0;set{PlayerPrefs.SetInt("LK.Subtitles.Large",value?1:0);PlayerPrefs.Save();}}
 }
}
