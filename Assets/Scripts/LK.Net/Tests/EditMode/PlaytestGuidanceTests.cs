using System;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
 // Solo playtest guidance: one job order, hints after the repair, a solo-only watch start gate and night-3 cue order.
 public sealed class PlaytestGuidanceTests
 {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static object Static(string type,string n,params object[] a)=>T(type).GetMethod(n).Invoke(null,a);
  static string Const(string type,string n)=>(string)(T(type).GetProperty(n)?.GetValue(null)??T(type).GetField(n).GetValue(null));
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  S4Tuning tuning;ImmersionTuning immersion;
  [SetUp]public void Setup(){tuning=UnityEngine.Object.Instantiate(Resources.Load<S4Tuning>("S4Tuning"));immersion=UnityEngine.Object.Instantiate(Resources.Load<ImmersionTuning>("ImmersionTuning"));}
  [TearDown]public void Teardown(){UnityEngine.Object.DestroyImmediate(tuning);UnityEngine.Object.DestroyImmediate(immersion);}
  sealed class Crew:ICoopWatchLink
  {public bool Ready=>true;public bool Send(CoopAction a,float v=0,bool p=false)=>true;public bool RemoteHeld(Component p)=>false;public bool RemoteMicrophone(Component p,out bool t){t=false;return false;}}
  object New(int day,bool crew=false)
  {
   var cat=Resources.Load("WatchDaysS4M2");Assert.NotNull(cat);var days=cat.GetType().GetField("days").GetValue(cat);
   var s=Activator.CreateInstance(T("WatchState"),days,day,tuning,Resources.Load<S4M2Tuning>("S4M2Tuning"),immersion);
   if(crew)s.GetType().GetProperty("Coop").SetValue(s,new Crew());return s;
  }
  static void StartEngine(object s)
  {
   for(int i=0;i<100;i++)Call(s,"PourFuel",.1f);
   for(int i=0;i<3;i++){Assert.True((bool)Call(s,"PrimeEngine"));Call(s,"Tick",.4);}
   Assert.True((bool)Call(s,"SetChoke",true));
   for(int i=0;i<3;i++){Assert.True((bool)Call(s,"StarterPulled"));Call(s,"Tick",.4);}
   Assert.True((bool)Call(s,"SetChoke",false));
  }
  static string Station(object s)
  {
   int index=Get<int>(s,"ObjectiveIndex");if(Get<bool>(s,"Complete")||index<0)return "DONE";
   var definition=Get<object>(s,"Definition");var tasks=(Array)definition.GetType().GetField("tasks").GetValue(definition);
   var task=tasks.GetValue(index);return (string)task.GetType().GetField("stationId").GetValue(task);
  }
  // The radio's idle hint must name the job the tracker shows.
  static int AssertRadioMatchesTracker(object s,int expectedCue)
  {
   var director=Get<object>(s,"Director");Call(director,"Tick",(float)Get<double>(s,"Elapsed"),1,Vector3.zero,0,Vector3.zero,true);
   int cue=(int)Static("HarborRadio","JobCue",s);string station=(string)Static("ObjectiveDirector","Job",Get<object>(director,"First"));
   int fromTracker=station=="STN_PIPE_LEAK"?7:station=="STN_GENERATOR"?8:station=="STN_RADIO_CONSOLE"?11:station=="STN_BEACON_CONTROL"?(cue==9?9:10):12;
   Assert.AreEqual(fromTracker,cue,"Radio hint job differs from tracker job "+station);
   Assert.AreEqual(expectedCue,cue,"Unexpected job for "+station);return cue;
  }
  [Test]public void RadioHintJobFollowsTrackerObjectiveOrderEveryNight()
  {
   var one=New(1);AssertRadioMatchesTracker(one,7);Call(one,"CompleteRepair");AssertRadioMatchesTracker(one,12);
   var two=New(2);Call(two,"Tick",76d);AssertRadioMatchesTracker(two,8);StartEngine(two);AssertRadioMatchesTracker(two,7);Call(two,"CompleteRepair");AssertRadioMatchesTracker(two,12);
   var three=New(3);Assert.True(Get<bool>(three,"Day3Balance"));AssertRadioMatchesTracker(three,7);
   Call(three,"CompleteRepair");AssertRadioMatchesTracker(three,8);
   StartEngine(three);Assert.AreEqual(3,Get<int>(three,"Day3Stage"));Assert.AreEqual("STN_BEACON_CONTROL",Station(three));
   AssertRadioMatchesTracker(three,10);
   // Night 4 is unstaged: once the generator runs the tracker sends the keeper to the pipe while the beacon has
   // more than 20 s of dark budget; the old radio order jumped to the beacon here.
   var four=New(4);Assert.False(Get<bool>(four,"Day3Balance"));AssertRadioMatchesTracker(four,8);StartEngine(four);
   Assert.Greater(Get<double>(four,"BeaconDarkRemaining"),20);AssertRadioMatchesTracker(four,7);
  }
  [Test]public void CarryingTheWrenchMutesHintsOnlyUntilThePipeIsRepaired()
  {
   var s=New(3);Assert.False(Get<bool>(s,"Repaired"));
   Assert.True((bool)Static("HarborRadio","WrenchSuppressesHints",true,Get<bool>(s,"Repaired")),"Keeper carrying the wrench to the pipe gets a hint");
   Assert.False((bool)Static("ObjectiveHud","PromptWrenchDown",Get<bool>(s,"Repaired"),true));
   Call(s,"CompleteRepair");Assert.True(Get<bool>(s,"Repaired"));
   Assert.False((bool)Static("HarborRadio","WrenchSuppressesHints",true,Get<bool>(s,"Repaired")),"Kept wrench silences later hints");
   Assert.False((bool)Static("HarborRadio","WrenchSuppressesHints",false,false));
   Assert.True((bool)Static("ObjectiveHud","PromptWrenchDown",Get<bool>(s,"Repaired"),true));Assert.False((bool)Static("ObjectiveHud","PromptWrenchDown",true,false));
   StringAssert.Contains("Set the wrench down",Const("ObjectiveHud","WrenchDownCaption"));
  }
  [Test]public void SoloWatchClockAndFloodWaitForFirstWorkThenRun()
  {
   var s=New(1);Assert.True(Get<bool>(s,"SoloWatchGate"));Assert.True(Get<bool>(s,"WatchStartHeld"));
   // Same loop as WatchDemoController: the clock (and flood) advance only when the gate allows it.
   for(int i=0;i<200;i++)if((bool)Call(s,"AdvanceWatchGate",.1))Call(s,"Tick",.1);
   Assert.Zero(Get<double>(s,"Elapsed"));Assert.True(Get<bool>(s,"WatchStartHeld"));
   Call(s,"EngageWatch"); // Night 1: first wrench grab.
   Assert.False(Get<bool>(s,"WatchStartHeld"));
   for(int i=0;i<10;i++)if((bool)Call(s,"AdvanceWatchGate",.1))Call(s,"Tick",.1);
   Assert.AreEqual(1,Get<double>(s,"Elapsed"),1e-6);
   // Any station use counts on later nights; the gate re-arms each night.
   Call(s,"CompleteRepair");Assert.True((bool)Call(s,"Sleep"));Assert.AreEqual(2,Get<int>(s,"Day"));Assert.True(Get<bool>(s,"WatchStartHeld"));
   Call(s,"Tick",76d);Assert.True((bool)Call(s,"PrimeEngine"));Assert.False(Get<bool>(s,"WatchStartHeld"));
  }
  [Test]public void SoloWatchStartsAfterCappedGraceWithoutEngagement()
  {
   var s=New(2);double grace=MasterPassTuning.Current.watchStartGraceSeconds;Assert.AreEqual(grace,Get<double>(s,"WatchStartGraceSeconds"));Assert.That(grace,Is.InRange(1,120));
   double held=0;while(held<grace-.5){Assert.False((bool)Call(s,"AdvanceWatchGate",.25));held+=.25;}
   Assert.Zero(Get<double>(s,"Elapsed"));
   bool released=false;for(int i=0;i<4&&!released;i++)released=(bool)Call(s,"AdvanceWatchGate",.25);
   Assert.True(released,"Grace did not release the watch");Assert.False(Get<bool>(s,"WatchStartHeld"));
   Assert.True((bool)Call(s,"AdvanceWatchGate",1d));
   Call(s,"Retry");Assert.True(Get<bool>(s,"WatchStartHeld"),"Retry should re-arm the start gate");
  }
  [Test]public void WatchStartGateIsHostOwnedAndReplicated()
  {
   var crew=New(1,true);Assert.False(Get<bool>(crew,"SoloWatchGate"));Assert.True(Get<bool>(crew,"WatchStartHeld"));Assert.False((bool)Call(crew,"AdvanceWatchGate",.1));
   var guest=New(1);Call(guest,"SetRemoteReplica",true);Assert.True(Get<bool>(guest,"WatchStartHeld"));Assert.False((bool)Call(guest,"AdvanceWatchGate",.1));Assert.Zero(Get<double>(guest,"WatchGateElapsed"));
   var practice=New(1);Call(practice,"SetNetworkRehearsal",true);Assert.False(Get<bool>(practice,"WatchStartHeld"));
   immersion.enabledForWatch=false;var off=New(1);Assert.False(Get<bool>(off,"WatchStartHeld"));Assert.True((bool)Call(off,"AdvanceWatchGate",.1));
   immersion.enabledForWatch=true;
   // The gate never alters the clock itself: direct ticks (and night lengths) are unchanged.
   var solo=New(1);Call(solo,"Tick",5d);Assert.AreEqual(5,Get<double>(solo,"Elapsed"),1e-6);Assert.AreEqual(Get<double>(crew,"Duration"),Get<double>(solo,"Duration"));
   StringAssert.Contains("Watch starts",Const("ObjectiveHud","WatchGateCaption"));
  }
  [Test]public void NightThreeOpeningCueMatchesStagedPipeAndLanternLineWaitsForUpperJobs()
  {
   var catalog=Resources.Load<ScriptableObject>("HarborRadioLines");Func<int,string> caption=id=>(string)Call(catalog,"Caption",id);
   var s=New(3);Assert.True(Get<bool>(s,"Day3Balance"));Assert.AreEqual("STN_PIPE_LEAK",Station(s));
   int opening=(int)Static("HarborRadio","OpeningCue",3,Get<bool>(s,"Day3Balance"));string text=caption(opening).ToLowerInvariant();
   Assert.IsNotEmpty(text);foreach(var word in new[]{"lantern","beacon","storm warning","light it"})StringAssert.DoesNotContain(word,text,"Night 3 opens by sending the keeper away from the staged pipe");
   int upper=(int)T("HarborRadio").GetField("UpperJobsCue").GetValue(null);Assert.AreNotEqual(upper,opening);StringAssert.Contains("lantern",caption(upper).ToLowerInvariant());
   Assert.True((bool)Call(s,"IsPending","STN_BEACON_CONTROL"));Call(s,"CompleteRepair");Assert.True((bool)Call(s,"IsPending","STN_BEACON_CONTROL"));
   StartEngine(s);Assert.AreEqual(3,Get<int>(s,"Day3Stage"));Assert.False((bool)Call(s,"IsPending","STN_BEACON_CONTROL"));Assert.False((bool)Call(s,"IsPending","STN_RADIO_CONSOLE"));
   // Other nights keep their recorded openings; an unstaged Night 3 keeps cue 14.
   Assert.AreEqual(1,(int)Static("HarborRadio","OpeningCue",1,false));Assert.AreEqual(13,(int)Static("HarborRadio","OpeningCue",2,false));Assert.AreEqual(upper,(int)Static("HarborRadio","OpeningCue",3,false));
  }
  [Test]public void WrenchTeachingNamesGripOnlyAndWakingNamesTheWayDown()
  {
   var catalog=Resources.Load<ScriptableObject>("HarborRadioLines");StringAssert.DoesNotContain("trigger",((string)Call(catalog,"Caption",3)).ToLowerInvariant());
   var plate=Const("ImmersionWorld","WrenchBenchText");StringAssert.Contains("GRIP",plate);StringAssert.DoesNotContain("TRIGGER",plate);StringAssert.Contains("BRING TO BOLT",plate);
   // The bunk is on floor 3; each night's first job is below it.
   var bunk=(Vector3)T("BunkPlacement").GetProperty("FramePosition").GetValue(null);int here=(int)Static("ObjectiveHud","FloorAt",bunk.y+1.6f);Assert.AreEqual(3,here);
   foreach(var (day,station,floor) in new[]{(2,"STN_GENERATOR",2),(3,"STN_PIPE_LEAK",1)})
   {
    var s=New(day);if(day==2)Call(s,"Tick",76d);Assert.AreEqual(station,Station(s));Assert.AreEqual(floor,(int)Static("ObjectiveHud","StationFloor",station));
    string direction=(string)Static("ObjectiveHud","FloorDirection",floor,here," ");Assert.AreEqual("DOWN FLOOR "+floor,direction);
    StringAssert.Contains(direction,(string)Static("ObjectiveHud","TonightCaption","job",direction));
   }
   StringAssert.Contains("UP FLOOR 2",(string)Static("ObjectiveHud","NextJobCaption","Restart the generator",(string)Static("ObjectiveHud","FloorDirection",2,1," ")));
  }

  [Test]public void HostGraceRoundTripsAndGuestCannotEngageOrRenewIt()
  {
   var host=New(3,true);var guest=New(3);Call(guest,"SetRemoteReplica",true);
   Call(host,"AdvanceWatchGate",12d);Call(host,"OfferStationGrace",Enum.Parse(T("KeeperStep"),"BoltA"));
   var snap=Call(host,"Capture",.1f);Assert.True((bool)Call(guest,"ApplyRemote",snap));Assert.AreEqual(12,Get<double>(guest,"WatchGateElapsed"));Assert.Greater(Get<double>(guest,"StationGraceRemaining"),0);
   Call(guest,"EngageWatch");Call(guest,"AdvanceWatchGate",99d);Assert.True(Get<bool>(guest,"WatchStartHeld"));Assert.AreEqual(12,Get<double>(guest,"WatchGateElapsed"));
   Call(host,"EngageWatch");Call(host,"Tick",10d);double grace=Get<double>(host,"StationGraceRemaining");Call(host,"OfferStationGrace",Enum.Parse(T("KeeperStep"),"BoltB"));Assert.AreEqual(grace,Get<double>(host,"StationGraceRemaining"),"Same job must not renew grace for another bolt");
   Assert.True((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.1f)));Assert.False(Get<bool>(guest,"WatchStartHeld"));Assert.AreEqual(grace,Get<double>(guest,"StationGraceRemaining"));
   foreach(var pair in new[]{("watchGateElapsed",(object)double.NaN),("stationGraceUntil",(object)9999d),("seenStationMask",(object)32)}){var invalid=Call(host,"Capture",.1f);invalid.GetType().GetField(pair.Item1).SetValue(invalid,pair.Item2);Assert.False((bool)Call(guest,"ApplyRemote",invalid));}
  }
 }
}
