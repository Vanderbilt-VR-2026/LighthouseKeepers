using System.Linq;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class CrewDutyTests
 {
  static FusionLobbyState Crew(){var s=new FusionLobbyState();s.Reset(2);s.Add("1",true);s.Add("2",false);return s;}
  static LobbyPacket Pick(int revision,CrewDuty duty)=>new(){revision=revision,requestedDuty=duty};
  [Test] public void HostAloneCanChooseEitherAndGuestReceivesVacantDuty(){var s=new FusionLobbyState();s.Reset(2);s.Add("1",true);Assert.True(s.ApplyRequest("1",Pick(1,CrewDuty.Machinist)));s.Add("2",false);Assert.AreEqual(CrewDuty.Machinist,s.Players[0].Duty);Assert.AreEqual(CrewDuty.Lightkeeper,s.Players[1].Duty);}
  [Test] public void OccupiedDutiesOnlySwapAfterBothKeepersAgree(){var s=Crew();s.ApplyRequest("1",Pick(1,CrewDuty.Machinist));Assert.AreEqual(CrewDuty.Lightkeeper,s.Players[0].Duty);Assert.AreEqual(CrewDuty.Machinist,s.Players[0].RequestedDuty);s.ApplyRequest("2",Pick(1,CrewDuty.Lightkeeper));Assert.AreEqual(CrewDuty.Machinist,s.Players[0].Duty);Assert.AreEqual(CrewDuty.Lightkeeper,s.Players[1].Duty);Assert.True(s.Players.All(p=>p.RequestedDuty==CrewDuty.None));}
  [Test] public void SelectingCurrentDutyCancelsPendingSwap(){var s=Crew();s.ApplyRequest("1",Pick(1,CrewDuty.Machinist));s.ApplyRequest("1",Pick(2,CrewDuty.Lightkeeper));s.ApplyRequest("2",Pick(1,CrewDuty.Lightkeeper));Assert.AreEqual(CrewDuty.Lightkeeper,s.Players[0].Duty);Assert.AreEqual(CrewDuty.Machinist,s.Players[1].Duty);}
  [Test] public void StartFreezesDutiesAndRejoinRestoresVacancy(){var s=Crew();s.ApplyRequest("1",Pick(1,CrewDuty.Machinist));s.Begin(1,false);Assert.True(s.Players.All(p=>p.RequestedDuty==CrewDuty.None));Assert.False(s.ApplyRequest("2",Pick(1,CrewDuty.Lightkeeper),true));s.Remove("2");s.AddToRunningWatch("3");Assert.AreEqual(CrewDuty.Machinist,s.Players[1].Duty);var g=new FusionLobbyState();Assert.True(g.ApplySnapshot(s.Snapshot()));Assert.AreEqual(s.Players[1].Duty,g.Players[1].Duty);}
  [Test] public void SpoofedAndStaleDutyRequestsCannotMovePartner(){var s=Crew();Assert.False(s.ApplyRequest("9",Pick(1,CrewDuty.Machinist)));Assert.False(s.ApplyRequest("1",Pick(1,(CrewDuty)99)));Assert.True(s.ApplyRequest("1",Pick(2,CrewDuty.Machinist)));Assert.False(s.ApplyRequest("1",Pick(1,CrewDuty.Lightkeeper)));Assert.AreEqual(CrewDuty.Machinist,s.Players[1].Duty);}
  [Test] public void DuplicateDutiesRejectSnapshotWithoutMutation(){var s=Crew();var g=new FusionLobbyState();g.ApplySnapshot(s.Snapshot());var bad=s.Snapshot();bad.revision++;bad.players[1].Duty=bad.players[0].Duty;Assert.False(g.ApplySnapshot(bad));Assert.AreEqual(CrewDuty.Machinist,g.Players[1].Duty);}
  [Test] public void OldLobbyWireVersionRejected(){var p=Pick(1,CrewDuty.Lightkeeper);p.version=1;Assert.False(LobbyProtocol.TryDecode(LobbyProtocol.Encode(p),out _));}
  [Test] public void HostDirectorRoutesEngineToMachinistWithoutLockingTools(){
   var t=System.Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);var c=UnityEngine.Resources.Load("WatchDaysS4M2");
   var state=System.Activator.CreateInstance(t,c.GetType().GetField("days").GetValue(c),2,UnityEngine.Resources.Load<LighthouseKeepers.Core.S4Tuning>("S4Tuning"),UnityEngine.Resources.Load<LighthouseKeepers.Core.S4M2Tuning>("S4M2Tuning"),UnityEngine.Resources.Load<LighthouseKeepers.Core.ImmersionTuning>("ImmersionTuning"));
   t.GetMethod("Tick").Invoke(state,new object[]{76d});var d=t.GetProperty("Director").GetValue(state);var dt=d.GetType();
   dt.GetField("DutyForActor").SetValue(d,new System.Func<int,CrewDuty>(actor=>actor==1?CrewDuty.Lightkeeper:CrewDuty.Machinist));
   dt.GetMethod("Tick").Invoke(d,new object[]{80f,1,UnityEngine.Vector3.zero,2,UnityEngine.Vector3.zero,true});
   Assert.AreEqual("Fuel",dt.GetProperty("Second").GetValue(d).ToString());
   Assert.AreNotEqual("Fuel",dt.GetProperty("First").GetValue(d).ToString());
   // A role is guidance, never a repair permission: the same host state still accepts work.
   Assert.True((bool)t.GetMethod("CompleteRepair").Invoke(state,null));
  }
 }
}
