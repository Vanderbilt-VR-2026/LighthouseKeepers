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
  [TestCase(76,"pipeOpening")][TestCase(77,"upperJobs")][TestCase(78,"wrenchDown")][TestCase(79,"startGrace")]
  [TestCase(80,"tonightPair")][TestCase(81,"tonightFull")][TestCase(82,"valvePuzzle")][TestCase(83,"valveReleased")]
  [TestCase(84,"soloClamp")][TestCase(85,"crewRadios")][TestCase(86,"pole")][TestCase(87,"breakerTripped")]
  [TestCase(88,"lowFuel")][TestCase(89,"engineStalled")][TestCase(90,"lensSoot")][TestCase(91,"wrongCode")]
  [TestCase(92,"broadcast")][TestCase(93,"closeCall")]
  public void SituationalTriggerUsesOncePerNightOrExplicitCooldown(int id,string signal)
  {
   var t=Type.GetType("LighthouseKeepers.WatchDemo.HarborCueRules, Assembly-CSharp",true);var rule=Activator.CreateInstance(t);var context=Activator.CreateInstance(t.GetNestedType("Context"));context.GetType().GetField(signal).SetValue(context,true);
   var output=new System.Collections.Generic.List<int>();Func<int,bool> queue=n=>{output.Add(n);return true;};var observe=t.GetMethod("Observe");var played=t.GetMethod("Played");
   observe.Invoke(rule,new object[]{context,0f,queue});CollectionAssert.AreEqual(new[]{id},output);played.Invoke(rule,new object[]{id,0f});output.Clear();observe.Invoke(rule,new object[]{context,1f,queue});Assert.IsEmpty(output);
   float cooldown=(float)t.GetMethod("Cooldown").Invoke(null,new object[]{id});
   observe.Invoke(rule,new object[]{context,cooldown>0?cooldown-.01f:10000f,queue});Assert.IsEmpty(output);
   if(cooldown>0){observe.Invoke(rule,new object[]{context,cooldown,queue});CollectionAssert.AreEqual(new[]{id},output);}
   output.Clear();t.GetMethod("Reset").Invoke(rule,null);observe.Invoke(rule,new object[]{context,0f,queue});CollectionAssert.AreEqual(new[]{id},output);
   for(int future=94;future<=101;future++)Assert.False((bool)t.GetMethod("Allows").Invoke(rule,new object[]{future,10000f}),"Future P-line accidentally wired");
  }
  [Test] public void NewCueDomainIsVersionedAndAllImportedPClipsAreAddressable()
  {
   Assert.AreEqual("LK-S5-SHIPS1",LighthouseKeepers.Core.CrewNetworkVersion.Matchmaking);
   var catalog=Resources.Load<ScriptableObject>("HarborRadioLines");foreach(var e in Read().entries){Assert.NotNull(catalog.GetType().GetMethod("Line").Invoke(catalog,new object[]{e.id}),e.file);Assert.AreEqual(e.caption,catalog.GetType().GetMethod("Caption").Invoke(catalog,new object[]{e.id}));}
  }
  [Test] public void ObjectiveTransitionsAllowTwoLinesThenRequireAQuietStretch()
  {
   var t=Type.GetType("LighthouseKeepers.WatchDemo.HarborCueRules, Assembly-CSharp",true);var can=t.GetMethod("CanSpeak");
   Assert.True((bool)t.GetMethod("Transition").Invoke(null,new object[]{4}),"Failure explanation should follow the transition spacing, while retaining the two-line burst limit");
   bool Allowed(int n,float quiet,bool transition)=>(bool)can.Invoke(null,new object[]{n,quiet,transition,20f,8f});
   Assert.True(Allowed(1,1.1f,true));Assert.False(Allowed(1,.5f,true));Assert.False(Allowed(1,1.1f,false));
   Assert.False(Allowed(2,19f,true));Assert.False(Allowed(2,19f,false));Assert.True(Allowed(2,20f,true));
  }
  [Test] public void SubtitleSdfRendersAboveItsDarkBackingAndKeepsAtlasReference()
  {
   var material=Resources.Load<Material>("HarborSubtitleSdf");Assert.NotNull(material);Assert.AreEqual("TextMeshPro/Mobile/Distance Field Overlay",material.shader.name);Assert.Greater(material.renderQueue,4000);Assert.NotNull(material.mainTexture);
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
