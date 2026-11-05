using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class HudPaTests
 {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name).Invoke(o,args);
  static int Index(object o)=>(int)o.GetType().GetProperty("ObjectiveIndex").GetValue(o);
  [Test]public void HostChoosesSharedObjectiveAndRejectsAnOutOfDayIndex()
  {
   var host=Activator.CreateInstance(T("WatchState"));var guest=Activator.CreateInstance(T("WatchState"));Call(guest,"SetRemoteReplica",true);
   var state=Call(host,"Capture",.02f);Assert.True((bool)Call(guest,"ApplyRemote",state));Assert.AreEqual(Index(host),Index(guest));
   Call(host,"CompleteRepair");state=Call(host,"Capture",.02f);Assert.AreEqual(-1,Index(host));Assert.True((bool)Call(guest,"ApplyRemote",state));Assert.AreEqual(-1,Index(guest));
   state.GetType().GetField("objectiveIndex").SetValue(state,30);Assert.False((bool)Call(guest,"ApplyRemote",state));
  }
  [Test]public void ReplayCommandFitsExistingRateAndRevisionWindow()
  {
   var statusWindow=new CoopCommandWindow();Assert.True(statusWindow.Accept(2,1,1,1,CoopAction.StatusRequest,0));Assert.False(statusWindow.Accept(0,1,1,1,CoopAction.StatusRequest,0));Assert.False(statusWindow.Accept(2,1,1,1,CoopAction.StatusRequest,0));
   var window=new CoopCommandWindow();Assert.True(window.Accept(1,1,1,1,CoopAction.PAReplay,5));Assert.False(window.Accept(1,1,1,1,CoopAction.PAReplay,5));Assert.False(window.Accept(1,2,0,1,CoopAction.PAReplay,0));
  }
  [Test]public void AllSpokenLinesHaveCaptionsAndMusicNeverRestsDuringCrisis()
  {
   var catalog=Resources.Load<ScriptableObject>("HarborRadioLines");var clips=(AudioClip[])catalog.GetType().GetField("lines").GetValue(catalog);var text=(string[])catalog.GetType().GetField("subtitles").GetValue(catalog);Assert.AreEqual(18,text.Length);
   for(int i=0;i<clips.Length;i++)if(clips[i])Assert.IsNotEmpty(text[i],clips[i].name);
   var rest=T("AdaptiveMusic").GetMethod("StormRest");Assert.True((bool)rest.Invoke(null,new object[]{1,150d,1}));Assert.False((bool)rest.Invoke(null,new object[]{1,150d,2}));Assert.False((bool)rest.Invoke(null,new object[]{1,100d,1}));
  }
 }
}
