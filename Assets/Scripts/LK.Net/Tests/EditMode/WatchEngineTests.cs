using LighthouseKeepers.Core;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class WatchEngineTests
 {
  S4M2Tuning tuning;WatchEngineState engine;
  [SetUp]public void Setup(){tuning=ScriptableObject.CreateInstance<S4M2Tuning>();engine=new WatchEngineState(tuning);engine.Reset(.8f,.0025f,false);}
  [TearDown]public void Cleanup(){Object.DestroyImmediate(tuning);}
  void Prime(){for(int i=0;i<3;i++){Assert.IsTrue(engine.Prime());engine.Tick(.4f);}}
  void Pull(int count){for(int i=0;i<count;i++){Assert.IsTrue(engine.Pull());engine.Tick(.6f);}}
  [Test]public void WrongOrderCoughsThenThreePrimersClosedChokeAndThreePullsRequireOpening()
  {Assert.IsTrue(engine.Pull());Assert.IsFalse(engine.Running);Assert.AreEqual(0,engine.Pulls);engine.Tick(1);Prime();engine.SetChoke(true);Pull(3);Assert.IsTrue(engine.Running);Assert.IsFalse(engine.Settled);engine.SetChoke(false);Assert.IsTrue(engine.Settled);engine.Tick(10);Assert.IsTrue(engine.Running);}
  [Test]public void ClosedChokeDeadlineStallsAndExcessPullsFloodForEightSeconds()
  {Prime();engine.SetChoke(true);Pull(3);engine.Tick(5);Assert.IsFalse(engine.Running);Assert.AreEqual(0,engine.Primer);Prime();Pull(5);Assert.IsFalse(engine.Running);Assert.Greater(engine.FloodRemaining,7);Assert.IsFalse(engine.Pull());Assert.IsFalse(engine.Prime());engine.Tick(8);Assert.AreEqual(0,engine.FloodRemaining);Assert.IsTrue(engine.Prime());}
  [Test]public void LowFuelCannotStartPourIsBoundedAndRunningEngineCanRunDry()
  {engine.Reset(.04f,.1f,false);Prime();engine.SetChoke(true);Pull(3);Assert.IsFalse(engine.Running);for(int i=0;i<20;i++)engine.Pour(.1f);Assert.GreaterOrEqual(engine.Fuel,tuning.minimumFuel);Pull(3);engine.SetChoke(false);engine.Tick(10);Assert.IsFalse(engine.Running);Assert.AreEqual(0,engine.Fuel);}
  [Test]public void ReadOnlyGuestCannotChangeFuelOrEngineAndDayOneNeverDrains()
  {engine.Reset(1,0,true);engine.Tick(1000);Assert.IsTrue(engine.Running);Assert.AreEqual(1,engine.Fuel);engine.ReadOnly=true;Assert.IsFalse(engine.SetChoke(true));Assert.IsFalse(engine.Prime());Assert.IsFalse(engine.Pull());engine.Pour(1);engine.Tick(10);Assert.IsTrue(engine.Running);}
 }
}
