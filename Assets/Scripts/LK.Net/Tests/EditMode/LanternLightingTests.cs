using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class LanternLightingTests
 {
  readonly List<object> watches=new();
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  object New(int day,KeeperGameMode mode){var c=Resources.Load("M3Campaign");var s=Activator.CreateInstance(T("WatchState"),c.GetType().GetField("nights").GetValue(c),day,Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"),mode);watches.Add(s);return s;}
  [TearDown]public void Cleanup(){foreach(var s in watches)Call(s,"DisposeSurvival");watches.Clear();}
  [TestCase(KeeperGameMode.Campaign)][TestCase(KeeperGameMode.Survival)]
  public void EveryNightUsesUnlimitedFuelAndReplicasKeepWicks(KeeperGameMode mode){
   for(int day=1;day<=5;day++){
    var host=New(day,mode);var lamps=Get<LanternState>(host,"Lanterns");Assert.NotNull(lamps);Assert.True(lamps.UnlimitedOil);Assert.True(lamps.FocusOnly);
    Call(host,"CycleLantern",0);lamps.Tick(3600);Assert.AreEqual(1,lamps.Oil(0));
    var guest=New(day,mode);Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.1f)));
    var remote=Get<LanternState>(guest,"Lanterns");Assert.True(remote.UnlimitedOil);Assert.AreEqual(LanternWick.High,remote.Wick(0));Assert.False(remote.Cycle(0));
   }
  }
  [Test]public void BackupBulbsStayOnUpperBulbsRequireMainsAndPatternNeverStrobes(){
   var t=T("LanternRoomLighting");var power=t.GetMethod("Powered");var pulse=t.GetMethod("Filament");
   for(int room=0;room<5;room++){
    Assert.AreEqual(room<=1,power.Invoke(null,new object[]{room,false}));Assert.True((bool)power.Invoke(null,new object[]{room,true}));
    float previous=(float)pulse.Invoke(null,new object[]{0f,room});
    for(int i=1;i<6000;i++){float v=(float)pulse.Invoke(null,new object[]{i*.01f,room});Assert.That(v,Is.InRange(room==0?.6f:room==1?.28f:.06f,1));Assert.Less(Mathf.Abs(v-previous),.15f,"Soft edges, no square flashes");previous=v;}
   }
  }
  [Test]public void BeamModesHaveDifferentReachAndWidthWithoutShadows(){
   var t=Sprint5Tuning.Current;var h=HorrorTuning.Current;Assert.Greater(t.lanternHighRange,t.lanternLowRange*2);
   Assert.Greater(t.lanternHighIntensity,t.lanternLowIntensity*4);Assert.Less(h.lanternHighBeamAngle,h.lanternBeamAngle*.5f);
   Assert.Less(h.lanternHighBeamInner,h.lanternHighBeamAngle);Assert.Less(h.lanternBeamInner,h.lanternBeamAngle);
   Assert.NotNull(Resources.Load<Shader>("RoomFilamentLit"));
  }
 }
}
