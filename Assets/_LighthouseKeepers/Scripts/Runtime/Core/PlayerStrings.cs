using System;using System.Collections.Generic;using UnityEngine;
namespace LighthouseKeepers.Core {
 public static class PlayerStrings {
  [Serializable] public class Entry {public string key,text;}
  [Serializable] public class Table {public Entry[] entries;}
  static Dictionary<string,string> values;
  static void Load(){if(values!=null)return;values=new();var data=Resources.Load<TextAsset>("KeeperStrings");if(data)foreach(var e in JsonUtility.FromJson<Table>(data.text).entries)values[e.key]=e.text;}
  public static bool Has(string key){Load();return values.ContainsKey(key);}
  public static string Get(string key){Load();return values.TryGetValue(key,out var value)?value:key;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset()=>values=null;
 }
}