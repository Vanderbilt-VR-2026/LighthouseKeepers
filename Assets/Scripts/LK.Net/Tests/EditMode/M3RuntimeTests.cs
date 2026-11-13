using System;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class M3RuntimeTests
 {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  static void Set(object o,string n,object v)=>o.GetType().GetProperty(n).SetValue(o,v);
  sealed class Crew:ICoopWatchLink
  {public bool Ready=>true;public bool Send(CoopAction a,float v=0,bool p=false)=>true;public bool RemoteHeld(Component p)=>false;public bool RemoteMicrophone(Component p,out bool t){t=false;return false;}}
  object New(int day=3,bool crew=false,bool demo=false)
  {
   var cat=Resources.Load("M3Campaign");Assert.NotNull(cat);var defs=cat.GetType().GetField("nights").GetValue(cat);
   if(demo){var d=cat.GetType().GetField("demo").GetValue(cat);var a=Array.CreateInstance(d.GetType(),1);a.SetValue(d,0);defs=a;}
   var state=Activator.CreateInstance(T("WatchState"),defs,day,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));
   if(crew){Set(state,"Coop",new Crew());Call(state,"SetKeeperCount",2);}return state;
  }
  void Tick(object s,double t)=>Call(s,"Tick",t);
  void Start(object s)
  {
   for(int i=0;i<100;i++)Call(s,"PourFuel",.1f);
   for(int i=0;i<3;i++){Call(s,"PrimeEngine");Tick(s,.4);}
   Call(s,"SetChoke",true);for(int i=0;i<3;i++){Call(s,"StarterPulled");Tick(s,.4);}Call(s,"SetChoke",false);
  }
  [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)]
  public void AuthoredNightsStartWithOnlyThePipeAndRetryRestoresIt(int day)
  {
   var s=New(day);Assert.True(Get<bool>(s,"Campaign"));Assert.False(Get<bool>(s,"Day3Balance"));Assert.False(Get<bool>(s,"Repaired"));Assert.True(Get<bool>(s,"BeaconShining"));
   Assert.AreEqual(1,Get<int>(s,"CampaignFaultMask"));Tick(s,30);Assert.False((bool)Call(s,"Sleep"));
   Call(s,"CompleteRepair");Tick(s,50);Call(s,"Retry");Assert.Zero(Get<double>(s,"Elapsed"));Assert.AreEqual(1,Get<int>(s,"CampaignFaultMask"));Assert.False(Get<bool>(s,"Repaired"));
  }
  [TestCase(1,false)][TestCase(2,false)][TestCase(3,true)][TestCase(4,true)][TestCase(5,true)]
  public void ValveUsesCurrentRosterAndAuthoredNightWithoutRestoringSoloClamp(int day,bool required)
  {
   var s=New(day);var pressure=Get<object>(s,"Pressure");
   Assert.True(Get<bool>(s,"PipeReady"));Assert.False(Get<bool>(pressure,"CrewRequired"));
   Call(s,"SetKeeperCount",2);Assert.AreEqual(required,Get<bool>(pressure,"CrewRequired"));
   Call(s,"Retry");Assert.AreEqual(required,Get<bool>(pressure,"CrewRequired"));
   Call(s,"SetKeeperCount",1);Assert.True(Get<bool>(s,"PipeReady"));Assert.False(Get<bool>(pressure,"CrewRequired"));
   var guest=New(day);Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",Call(s,"Capture",.1f)));
   Call(guest,"SetKeeperCount",2);Assert.False(Get<bool>(Get<object>(guest,"Pressure"),"CrewRequired"));
  }
  [Test] public void PipeCompletionWaitsForOpeningThenEngineThenUpperJobsWithBreathers()
  {
   var s=New();Tick(s,20);Call(s,"CompleteRepair");Tick(s,24);Assert.AreEqual("On",Get<object>(s,"Power").ToString());Tick(s,1);
   Assert.AreEqual("Off",Get<object>(s,"Power").ToString());Assert.True((bool)Call(s,"IsPending","STN_RADIO_CONSOLE"));Start(s);
   Assert.AreEqual("On",Get<object>(s,"Power").ToString());Assert.True((bool)Call(s,"IsPending","STN_BEACON_CONTROL"));
   Tick(s,16);Assert.False((bool)Call(s,"IsPending","STN_BEACON_CONTROL"));Tick(s,12);Assert.True((bool)Call(s,"IsPending","STN_RADIO_CONSOLE"),"Sprint5 requires at least fifteen seconds between faults");Tick(s,3);Assert.False((bool)Call(s,"IsPending","STN_RADIO_CONSOLE"));
   Assert.False(Get<bool>(s,"Complete"));Assert.False(Get<bool>(s,"Failed"));
  }
  [Test] public void CrewSnapshotAndRetryCarryScheduleAndGuestsCannotFireEvents()
  {
   var h=New(3,true);Tick(h,80);var g=New();Call(g,"SetRemoteReplica",true);
   Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.AreEqual(Get<int>(h,"CampaignFaultMask"),Get<int>(g,"CampaignFaultMask"));Assert.AreEqual(510,Get<double>(g,"Duration"));
   Tick(g,100);Call(g,"CompleteRepair");Assert.AreEqual(80,Get<double>(g,"Elapsed"));Assert.False(Get<bool>(g,"Repaired"));
   foreach(var field in new[]{"nightFaultMask","nightGraceUntil","campaignEnabled"})
   {var snap=Call(h,"Capture",.1f);var f=snap.GetType().GetField(field);f.SetValue(snap,field=="nightFaultMask"?(object)int.MaxValue:field=="campaignEnabled"?false:(object)999d);Assert.False((bool)Call(g,"ApplyRemote",snap));}
   Call(h,"Retry");Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",0f)));Assert.AreEqual(1,Get<int>(g,"CampaignFaultMask"));
  }
  [Test] public void DemoConsumesItsOwnDataAndNightOneCannotLose()
  {
   var demo=New(3,false,true);Assert.AreEqual(360,Get<double>(demo,"Duration"));Tick(demo,75);Assert.AreEqual("Off",Get<object>(demo,"Power").ToString());
   var one=New(1);Call(one,"EngageWatch");Tick(one,400);Call(one,"ObserveWater",10d,1f);Assert.False(Get<bool>(one,"Failed"));
  }
  [Test] public void NightTwoActuallyConsumesFuelSputtersThenCutsEveryCircuitAndReplicates()
  {
   var s=New(2);Tick(s,72.5);Assert.True(Get<bool>(s,"FuelSputtering"));Assert.AreEqual("On",Get<object>(s,"Power").ToString());
   var engine=Get<object>(s,"Engine");Assert.Greater(Get<float>(engine,"Fuel"),0);
   Tick(s,3);Assert.Zero(Get<float>(engine,"Fuel"));Assert.True(Get<bool>(s,"NightTwoOutage"));Assert.False(Get<bool>(s,"BeaconShining"));
   foreach(PowerCircuit circuit in Enum.GetValues(typeof(PowerCircuit)))Assert.False((bool)Call(s,"HasPower",circuit));
   Assert.False((bool)Call(s,"IsPending","STN_GENERATOR"));
   Assert.AreEqual("Fuel",Call(Get<object>(s,"Director"),"StepForStation","STN_GENERATOR",false).ToString());
   var g=New(2);Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(s,"Capture",.1f)));Assert.True(Get<bool>(g,"NightTwoOutage"));
   Start(s);Assert.AreEqual("On",Get<object>(s,"Power").ToString());Assert.True((bool)Call(s,"IsDone","STN_GENERATOR"));
  }
  [Test] public void LowFuelEventDoesNotRestartEngineAndCannotCompleteBeforeRefilling()
  {
   var s=New(4);Tick(s,75);Call(s,"CompleteRepair");Start(s);Tick(s,95);
   Assert.True(Get<bool>(s,"FuelJobPending"));Assert.AreEqual("On",Get<object>(s,"Power").ToString());Assert.False((bool)Call(s,"IsDone","STN_GENERATOR"));
   var director=Get<object>(s,"Director");Assert.AreEqual("Fuel",Call(director,"StepForStation","STN_GENERATOR",false).ToString());
   for(int i=0;i<100;i++)Call(s,"PourFuel",.1f);Tick(s,.3);Assert.False(Get<bool>(s,"FuelJobPending"));Assert.True((bool)Call(s,"IsDone","STN_GENERATOR"));
  }
  [Test] public void RepairReliefExtendsPhysicalFuelBudgetWithoutFakingPowerAndReplicatesConsumption()
  {
   var h=New(2);Tick(h,70);Call(h,"CompleteRepair");Tick(h,15.5);
   var engine=Get<object>(h,"Engine");Assert.True(Get<bool>(engine,"Running"));Assert.Greater(Get<float>(engine,"Fuel"),0);Assert.False(Get<bool>(h,"NightTwoOutage"));Assert.True(Get<bool>(h,"FuelSputtering"));
   var g=New(2);Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.02f)));Assert.True(Get<bool>(g,"FuelSputtering"));Assert.AreEqual(Get<float>(engine,"Consumption"),Get<float>(Get<object>(g,"Engine"),"Consumption"));
   Tick(h,4);Assert.Zero(Get<float>(engine,"Fuel"));Assert.False(Get<bool>(engine,"Running"));Assert.True(Get<bool>(h,"NightTwoOutage"));
   foreach(PowerCircuit circuit in Enum.GetValues(typeof(PowerCircuit)))Assert.False((bool)Call(h,"HasPower",circuit));
  }
  [Test] public void TensionSnapshotRejectsMismatchedStageAndGuestCannotAdvanceItsPacing()
  {
   var h=New(3);Tick(h,80);var g=New(3);Call(g,"SetRemoteReplica",true);var snap=Call(h,"Capture",.1f);Assert.True((bool)Call(g,"ApplyRemote",snap));
   var host=Get<object>(h,"Tension");var guest=Get<object>(g,"Tension");Assert.NotNull(host);Assert.AreEqual(Get<float>(host,"Value"),Get<float>(guest,"Value"));
   Call(guest,"Fixed",81d);Assert.AreEqual(Get<double>(host,"ReliefUntil"),Get<double>(guest,"ReliefUntil"));
   snap.GetType().GetField("stormStage").SetValue(snap,Get<int>(host,"StormStage")==4?3:4);Assert.False((bool)Call(g,"ApplyRemote",snap));
   snap=Call(h,"Capture",.1f);snap.GetType().GetField("engineConsumption").SetValue(snap,float.NaN);Assert.False((bool)Call(g,"ApplyRemote",snap));
   Call(h,"Retry");Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",0f)));Assert.AreEqual(Get<int>(Get<object>(h,"Tension"),"StormStage"),Get<int>(Get<object>(g,"Tension"),"StormStage"));
  }
  [Test] public void HostSamplesFearByFloorAndGuestCannotSubmitItsOwnFear()
  {
   var h=New(3,true);Tick(h,80);Call(h,"ObserveWater",.1d,.5f);Call(h,"ObserveKeepers",8f,1,new Vector3(0,1.6f,0),2,new Vector3(0,8.2f,0));
   var fear=Get<object>(h,"Fear");float low=(float)Call(fear,"ForActor",1),high=(float)Call(fear,"ForActor",2);Assert.Greater(low,high);Assert.Greater(high,0);
   var g=New(3);Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.5f)));Call(g,"ObserveKeepers",100f,1,Vector3.zero,2,Vector3.zero);Assert.AreEqual(low,Call(Get<object>(g,"Fear"),"ForActor",1));
   var bad=Call(h,"Capture",.5f);var f=bad.GetType().GetField("fear").GetValue(bad);f.GetType().GetField("first").SetValue(f,float.NaN);Assert.False((bool)Call(g,"ApplyRemote",bad));
   Call(h,"Retry");Assert.Zero((float)Call(Get<object>(h,"Fear"),"ForActor",1));
  }
  [Test] public void WorldFilterNeverAttachesToSharedCameraOrMultipleSourceObjects()
  {
   var go=new GameObject("Filter endpoint test");try{var a=go.AddComponent<AudioSource>();Assert.True(FearWorldFilter.CanAttach(a));var listener=go.AddComponent<AudioListener>();Assert.False(FearWorldFilter.CanAttach(a));UnityEngine.Object.DestroyImmediate(listener);go.AddComponent<AudioSource>();Assert.False(FearWorldFilter.CanAttach(a));}finally{UnityEngine.Object.DestroyImmediate(go);}
  }
  [Test] public void FearComfortControlsStayInsideTheLogbookPage()
  {
   var panel=new GameObject("Settings bounds test",typeof(RectTransform));try{
    T("WatchAudioSettings").GetMethod("Attach").Invoke(null,new object[]{panel,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null,(Action)(()=>{})});
    var page=panel.transform.Find("Sound settings page");Assert.NotNull(page);
    float half=((RectTransform)panel.transform).sizeDelta.y/2;
    foreach(Transform child in page)if(child.name=="HeartbeatHaptics"||child.name=="CloseAudioSettings"||child.name=="ObjectiveHeight"){
     var r=(RectTransform)child;Assert.LessOrEqual(Mathf.Abs(r.anchoredPosition.y)+r.sizeDelta.y/2,half,child.name);
    }
   }finally{UnityEngine.Object.DestroyImmediate(panel);}
  }

 }
}
