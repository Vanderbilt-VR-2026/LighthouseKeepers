using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class VoicePassTests
 {
  [Serializable] public class Entry {public int id,number;public string file,caption;public float duration;public float[] wordStarts;public int[] characterEnds;public bool longSingleTake;}
  [Serializable] public class Table {public Entry[] entries;}
  static Table Read()=>JsonUtility.FromJson<Table>(File.ReadAllText("Assets/WatchDemo/Resources/HarborVoiceTiming.json"));
  [Test] public void All94MappedClipsExistWithExactScriptCaptionsAndTimedSingleCuts()
  {
   var table=Read();Assert.AreEqual(94,table.entries.Length);int longCount=0;
   var catalog=Resources.Load<ScriptableObject>("HarborRadioLines");var caption=catalog.GetType().GetMethod("Caption");
   var strings=JsonUtility.FromJson<StringTable>(File.ReadAllText("Assets/Data/Strings/Resources/KeeperStrings.json"));
   foreach(var e in table.entries)
   {
    var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(e.file);Assert.NotNull(clip,e.file);Assert.AreEqual(1,clip.channels,e.file);Assert.AreEqual(44100,clip.frequency,e.file);Assert.AreEqual(e.duration,clip.length,.035,e.file);
    string text=e.id<=18?(string)caption.Invoke(catalog,new object[]{e.id}):Array.Find(strings.entries,x=>x.key=="radio."+e.id)?.text;Assert.AreEqual(e.caption,text,"Caption "+e.number);
    Assert.AreEqual(e.wordStarts.Length,e.characterEnds.Length);Assert.AreEqual(e.caption.Length,e.characterEnds[e.characterEnds.Length-1]);
    for(int i=0;i<e.wordStarts.Length;i++){Assert.That(e.wordStarts[i],Is.InRange(0,e.duration));if(i>0){Assert.GreaterOrEqual(e.wordStarts[i],e.wordStarts[i-1]);Assert.Greater(e.characterEnds[i],e.characterEnds[i-1]);}}
    if(clip.length>=10){longCount++;Assert.True(e.longSingleTake,"Unreported long recording "+e.number);Assert.AreEqual(1,e.number,"Only the complete arrival has a documented duration exception");TestContext.WriteLine("NEEDS CHECK: script 1 is a complete 11.79-second single take; retained without speeding up.");}
    else Assert.False(e.longSingleTake);
   }
   Assert.AreEqual(1,longCount);
  }
  [Serializable] public class StringEntry {public string key,text;}
  [Serializable] public class StringTable {public StringEntry[] entries;}
  [Test] public void WordRevealNeverRewindsAndFinishesAtAudioEnd()
  {
   var type=Type.GetType("LighthouseKeepers.WatchDemo.HarborVoiceTiming, Assembly-CSharp",true);var get=type.GetMethod("Get");
   foreach(var e in Read().entries){var entry=get.Invoke(null,new object[]{e.id});var method=entry.GetType().GetMethod("VisibleCharacters");int previous=0;for(float age=0;age<e.duration;age+=.05f){int n=(int)method.Invoke(entry,new object[]{age});Assert.GreaterOrEqual(n,previous);previous=n;}Assert.AreEqual(e.caption.Length,method.Invoke(entry,new object[]{e.duration}));}
  }
 }
}
