using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
public class ScareSequenceTests
{
    HorrorTuning tuning;ScareSequenceState state;
    [SetUp] public void Setup(){tuning=ScriptableObject.CreateInstance<HorrorTuning>();state=new ScareSequenceState(tuning);}
    [TearDown] public void Cleanup(){Object.DestroyImmediate(tuning);}
    void Seconds(float n,bool seen=false,bool corner=false,int outside=0,int down=0){while(n>0){float dt=Mathf.Min(n,.1f);state.Tick(dt,seen,corner,outside,down);n-=dt;}}
    void Chase(int humans=3){Assert.IsTrue(state.Begin(humans,Vector3.zero));Seconds(4,true);Seconds(1,true,true);Seconds(1,true);Seconds(tuning.nightmareTurnSeconds+tuning.telegraphSeconds+tuning.quietSeconds+1,true);Assert.AreEqual(ScareBeat.Escape,state.Beat);}
    [Test] public void WakeAndCornerWaitForGaze(){state.Begin(1,Vector3.zero);Seconds(5);Assert.AreEqual(ScareBeat.Wake,state.Beat);Seconds(1,true);Assert.AreEqual(ScareBeat.Follow,state.Beat);Seconds(1,false,true);Assert.AreEqual(ScareBeat.Corner,state.Beat);Seconds(5);Assert.AreEqual(ScareBeat.Corner,state.Beat);}
    [Test] public void BothEscapeThenRelief(){Chase();Seconds(1,outside:1);Assert.AreEqual(ScareBeat.Escape,state.Beat);Seconds(1,outside:3);Assert.AreEqual(ScareBeat.Relief,state.Beat);Seconds(9);Assert.IsTrue(state.Finished);Assert.IsTrue(state.Capture().Valid());}
    [Test] public void RescueDeadlineStartsOnlyAfterEscape(){Chase();state.Catch(2,Vector3.one);Seconds(20,down:2);Assert.AreEqual(ScareBeat.Escape,state.Beat);Seconds(.1f,outside:1,down:2);Assert.AreEqual(ScareBeat.Return,state.Beat);Seconds(tuning.nightmareRescueSeconds-2,down:2);Assert.IsFalse(state.Failed);Seconds(1,down:0);Assert.AreEqual(ScareBeat.Relief,state.Beat);}
    [Test] public void RescueTimeoutFails(){Chase();state.Catch(2,Vector3.one);Seconds(1,outside:1,down:2);Seconds(40,down:2);Assert.IsTrue(state.Failed);}
    [TestCase(1)][TestCase(2)]public void SoloCatchFailsInEitherRole(int role){Chase(role);Assert.IsTrue(state.Catch(role,Vector3.one));Assert.IsTrue(state.Failed);Assert.IsTrue(state.Capture().Valid());}
    [Test] public void BothCaughtFailAndRetryUsesWakeCheckpoint(){Chase();state.Catch(1,Vector3.left);state.Catch(2,Vector3.right);Assert.IsTrue(state.Failed);Assert.IsTrue(state.Retry(Vector3.up));Assert.AreEqual(ScareBeat.Wake,state.Beat);Assert.AreEqual(2,state.Attempt);Assert.AreEqual(0,state.Caught);Assert.IsTrue(state.CanSkip);}
    [Test] public void UnescapedPartnerPreventsEarlyRevivePhase(){Chase();state.Catch(2,Vector3.one);Seconds(1);Assert.IsFalse(state.CanRescue);}
    [Test] public void EscapedPlayerCannotBeCaught(){Chase();Seconds(.1f,outside:1);Assert.IsFalse(state.Catch(1,Vector3.zero));}
    [Test] public void CrewDisconnectRestartsSafeCheckpoint(){Chase();state.Catch(2,Vector3.one);Assert.IsTrue(state.CrewChanged(1,Vector3.up));Assert.AreEqual(ScareBeat.Wake,state.Beat);Assert.AreEqual(0,state.Caught);Assert.AreEqual(1,state.Participants);}
    [Test] public void ReplicasCannotAdvanceAndFramesAreIsolated(){Chase();var snapshot=state.Capture();var guest=new ScareSequenceState(tuning){ReadOnly=true};Assert.IsTrue(guest.Apply(snapshot));snapshot.body=Vector3.one*999;Assert.AreNotEqual(snapshot.body,guest.Body);guest.Tick(1,true,true,3,0);Assert.AreEqual(ScareBeat.Escape,guest.Beat);Assert.IsFalse(guest.Catch(1,Vector3.zero));Assert.IsFalse(guest.Retry(Vector3.zero));}
    [Test] public void InvalidFrameRejectedBeforeMutation(){Chase();var guest=new ScareSequenceState(tuning){ReadOnly=true};var f=state.Capture();f.age=float.NaN;Assert.IsFalse(guest.Apply(f));f=state.Capture();f.escaped=f.caught=1;Assert.IsFalse(guest.Apply(f));f=state.Capture();f.body.x=float.PositiveInfinity;Assert.IsFalse(guest.Apply(f));f=state.Capture();f.beat=ScareBeat.Return;Assert.IsFalse(guest.Apply(f));Assert.AreEqual(ScareBeat.Idle,guest.Beat);}
    [Test] public void FirstPlayCannotSkipAndUnknownDutyCannotCatch(){Chase();Assert.IsFalse(state.Skip());Assert.IsFalse(state.Catch(3,Vector3.zero));}
    [Test] public void EscapeHasFiniteDeadline(){Chase();Seconds(56);Assert.IsTrue(state.Failed);}
}
