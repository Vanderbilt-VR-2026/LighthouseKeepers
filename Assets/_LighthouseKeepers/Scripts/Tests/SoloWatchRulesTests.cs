using LighthouseKeepers.StormWatch;
using NUnit.Framework;
using UnityEngine;

namespace LighthouseKeepers.Tests
{
    public sealed class SoloWatchRulesTests
    {
        StormWatchRules rules;
        KeeperInput keeper, absent;
        [SetUp] public void Setup()
        {
            rules = new StormWatchRules(new WatchTuning { pipeStepSeconds = .2f,
                generatorStepSeconds = .2f, beaconStepSeconds = .2f }, true);
            keeper = new KeeperInput { connected = true, head = Vector3.up * 1.6f };
            absent = new KeeperInput();
        }
        void Tick(int count = 1) { for (int i = 0; i < count; i++) rules.Tick(.05f, keeper, absent); }
        void Work(WatchAction action)
        {
            keeper.action = action; keeper.effort = 1;
            keeper.head = WatchStations.For(action) + Vector3.up * .5f;
        }
        void Command(WatchAction action)
        {
            keeper.command = action; keeper.sequence++; Tick(); keeper.command = WatchAction.None;
        }
        [Test] public void SoloStartsAndRepairsWithoutCreatingASecondKeeper()
        {
            Command(WatchAction.Begin);
            Assert.AreEqual(WatchAct.FirstWatch, rules.State.act);
            Work(WatchAction.Pipe); Tick(20);
            Assert.AreEqual(WatchAct.FirstRefuge, rules.State.act);
            Assert.IsFalse(absent.connected);
            Assert.IsFalse(rules.State.down1);
            Assert.IsTrue(rules.State.gateOne && rules.State.gateTwo && rules.State.gateThree);
        }
        [Test] public void SoloRefugeContinuesWithOnlyLocalKeeperPresent()
        {
            Command(WatchAction.Begin); Work(WatchAction.Pipe); Tick(20);
            keeper.action = WatchAction.None; keeper.feet = new Vector3(0, 3.2f, 0);
            Command(WatchAction.RepairSupply); Command(WatchAction.Continue);
            Assert.AreEqual(WatchAct.PowerFailure, rules.State.act);
        }
        [Test] public void SoloGeneratorCanRecoverWithoutSimultaneousPumpSupport()
        {
            Assert.IsTrue(rules.SelectSoloWatch(WatchAct.PowerFailure));
            Work(WatchAction.Generator); Tick(20);
            Assert.AreEqual(3, rules.State.generatorSteps);
            Assert.IsTrue(rules.State.power);
            Assert.AreEqual(0, rules.State.mistakes);
        }
        [Test] public void SoloStillRequiresNearStationAndRealPumpEffort()
        {
            rules.SelectSoloWatch(WatchAct.PowerFailure); rules.State.water = .5f;
            Work(WatchAction.Pump); keeper.effort = 0; Tick(4);
            float before = rules.State.water;
            keeper.effort = 1; Tick(4); Assert.Less(rules.State.water, before);
            Work(WatchAction.Generator); keeper.head = Vector3.up * 20; Tick(20);
            Assert.AreEqual(0, rules.State.generatorSteps);
        }
        [Test] public void SoloRelayNeedsRadioVisitThenAllowsOneKeeperToFinish()
        {
            rules.SelectSoloWatch(WatchAct.RadioRelay);
            Work((WatchAction)((int)WatchAction.BeaconA + rules.State.Code(0))); Tick(10);
            Assert.AreEqual(0, rules.State.beaconSteps);
            Work(WatchAction.Radio); Tick(); Assert.IsTrue(rules.SoloRadioRead);
            for (int i = 0; i < 3; i++)
            {
                Work((WatchAction)((int)WatchAction.BeaconA + rules.State.Code(i)));
                Tick(5);
            }
            keeper.feet = new Vector3(0, 9.6f, 0); keeper.head = new Vector3(0, 11.2f, 0); Tick();
            Assert.AreEqual(WatchAct.Won, rules.State.act);
        }
        [Test] public void SelectingAnotherSoloTestClearsFailureAndRecordedCode()
        {
            rules.SelectSoloWatch(WatchAct.RadioRelay); Work(WatchAction.Radio); Tick();
            rules.State.act = WatchAct.Lost; rules.State.down0 = true;
            Assert.IsTrue(rules.SelectSoloWatch(WatchAct.Storm));
            Assert.IsFalse(rules.State.down0); Assert.IsFalse(rules.SoloRadioRead);
            Assert.AreEqual(0, rules.State.pipeSteps); Assert.IsFalse(rules.State.power);
        }
        [Test] public void NormalCrewCannotUseSoloSelectorsOrRepairAlone()
        {
            var crew = new StormWatchRules();
            Assert.IsFalse(crew.SelectSoloWatch(WatchAct.Storm));
            keeper.command = WatchAction.Begin; keeper.sequence++; crew.Tick(.05f, keeper, absent);
            Assert.AreEqual(WatchAct.Briefing, crew.State.act);
            Assert.IsFalse(crew.State.gateThree);
        }
        [Test] public void SoloLaunchRequestIsConsumedOnceAndCanBeCancelled()
        {
            SoloWatchSession.Request(); Assert.IsTrue(SoloWatchSession.ConsumeRequest());
            Assert.IsFalse(SoloWatchSession.ConsumeRequest());
            SoloWatchSession.Request(); SoloWatchSession.Cancel();
            Assert.IsFalse(SoloWatchSession.ConsumeRequest());
        }
    }
}
