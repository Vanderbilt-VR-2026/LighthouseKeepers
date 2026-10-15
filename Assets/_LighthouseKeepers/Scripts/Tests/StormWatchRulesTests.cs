using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.StormWatch;
namespace LighthouseKeepers.Tests
{
    public sealed class StormWatchRulesTests
    {
        StormWatchRules rules;
        KeeperInput a,b;
        WatchState S=>rules.State;
        [SetUp] public void Setup()
        {
            rules=new StormWatchRules(new WatchTuning{pipeStepSeconds=.3f,generatorStepSeconds=.4f,beaconStepSeconds=.3f,
                blackoutDelay=.3f,unsupportedGrace=.5f,airSeconds=1,rescueSeconds=2,pullSeconds=.3f});
            a=new KeeperInput{connected=true,head=new(0,1.65f,-2.2f),feet=Vector3.zero};
            b=new KeeperInput{connected=true,head=new(-.9f,1.65f,-2.2f),feet=Vector3.zero};
        }
        void Command(KeeperInput p,WatchAction c){p.command=c;p.sequence++;Tick(.05f);p.command=WatchAction.None;}
        void Tick(float seconds){for(int i=0;i<Mathf.CeilToInt(seconds/.05f);i++)rules.Tick(.05f,a,b);}
        void Work(KeeperInput p,WatchAction action){p.action=action;p.effort=1;p.head=WatchStations.For(action)+Vector3.up*.5f;}
        void Seal(){Work(a,WatchAction.Pipe);Work(b,WatchAction.Brace);Tick(1);a.action=b.action=WatchAction.None;}
        void Next(){a.feet=b.feet=new(3,S.act==WatchAct.FirstRefuge?3.2f:6.4f,0);a.head=b.head=a.feet+Vector3.up*1.65f;Command(a,WatchAction.RepairSupply);Command(a,WatchAction.Continue);}
        void Restore(){Work(a,WatchAction.Generator);Work(b,WatchAction.Pump);Tick(1.5f);a.action=b.action=WatchAction.None;}
        void ToRadio(){Command(a,WatchAction.Begin);Seal();Next();Tick(.4f);Restore();Seal();Next();Restore();Seal();Assert.AreEqual(WatchAct.RadioRelay,S.act);}
        [Test] public void NeedsTwoConnectedKeepersAndDistinctJobs()
        {
            b.connected=false;Command(a,WatchAction.Begin);Assert.AreEqual(WatchAct.Briefing,S.act);
            b.connected=true;Command(a,WatchAction.Begin);Work(a,WatchAction.Pipe);Tick(1);
            Assert.AreEqual(0,S.pipeSteps);Work(b,WatchAction.Brace);Tick(.4f);Assert.AreEqual(1,S.pipeSteps);
        }
        [Test] public void FirstRepairOpensRouteAndVisiblyDrainsWater()
        {
            Command(a,WatchAction.Begin);S.water=.7f;Seal();Assert.AreEqual(WatchAct.FirstRefuge,S.act);Assert.IsTrue(S.gateOne);
            float before=S.water;Tick(3);Assert.Less(S.water,before);Assert.IsFalse(S.gateTwo);
        }
        [Test] public void SuppliesAreOneSharedChoiceAndRequireRegrouping()
        {
            Command(a,WatchAction.Begin);Seal();a.feet=new(3,3.2f,0);Command(a,WatchAction.RepairSupply);Command(b,WatchAction.RescueSupply);
            Assert.AreEqual(3,S.repairsRemaining);Assert.AreEqual(0,S.rescueBonus);
            Command(a,WatchAction.Continue);Assert.AreEqual(WatchAct.FirstRefuge,S.act);
            b.feet=a.feet;Command(a,WatchAction.Continue);Assert.AreEqual(WatchAct.PowerFailure,S.act);
        }
        [Test] public void BlackoutMakesPumpEffectiveAndRestorationRequiresCompletedGenerator()
        {
            Command(a,WatchAction.Begin);Seal();Next();Tick(.4f);Assert.IsFalse(S.power);
            S.water=.5f;float before=S.water;Work(b,WatchAction.Pump);Tick(.2f);Assert.Less(S.water,before);
            Restore();Assert.IsTrue(S.power);Assert.AreEqual(3,S.generatorSteps);Assert.AreEqual(WatchAct.PowerFailure,S.act);
            Seal();Assert.IsTrue(S.gateTwo);Assert.AreEqual(WatchAct.SecondRefuge,S.act);
        }
        [Test] public void AbandoningPumpCostsOnlyUnfinishedStepAndReleaseAvoidsSetback()
        {
            Command(a,WatchAction.Begin);Seal();Next();Tick(.4f);S.repairsRemaining=0;
            Work(a,WatchAction.Generator);Work(b,WatchAction.Pump);Tick(.45f);Assert.AreEqual(1,S.generatorSteps);
            rules.Tuning.generatorStepSeconds=2;
            b.action=WatchAction.None;Tick(.15f);Assert.Greater(S.generatorProgress,0);
            a.action=WatchAction.None;Tick(.8f);Assert.AreEqual(0,S.mistakes);
            a.action=WatchAction.Generator;Tick(.5f);Assert.AreEqual(1,S.generatorSteps);Assert.AreEqual(0,S.generatorProgress);Assert.AreEqual(1,S.mistakes);
            Tick(.3f);Assert.AreEqual(1,S.mistakes,"One noisy held input cannot repeat the surge every frame.");
        }
        [Test] public void FarAwayActionsCannotOperateStations()
        {
            Command(a,WatchAction.Begin);Work(a,WatchAction.Pipe);Work(b,WatchAction.Brace);a.head=Vector3.up*10;Tick(2);Assert.AreEqual(0,S.pipeSteps);
        }
        [Test] public void BeaconRequiresRadioPartnerAndBothSurvivorsAtTop()
        {
            ToRadio();int next=S.Code(0);Work(a,(WatchAction)((int)WatchAction.BeaconA+next));b.action=WatchAction.None;Tick(.4f);Assert.AreEqual(0,S.beaconSteps);
            Work(b,WatchAction.Radio);
            for(int step=0;step<3;step++){Work(a,(WatchAction)((int)WatchAction.BeaconA+S.Code(step)));Tick(.4f);}
            Assert.AreEqual(WatchAct.Ascent,S.act);Assert.IsTrue(S.beacon);
            a.feet=new(3,9.6f,0);a.head=a.feet+Vector3.up*1.65f;Tick(.1f);Assert.AreEqual(WatchAct.Ascent,S.act);
            b.feet=a.feet;b.head=a.head;Tick(.1f);Assert.AreEqual(WatchAct.Won,S.act);
        }
        [Test] public void WrongBeaconSwitchResetsCurrentStepPreservingCompletedSwitches()
        {
            ToRadio();Work(b,WatchAction.Radio);Work(a,(WatchAction)((int)WatchAction.BeaconA+S.Code(0)));Tick(.4f);
            Assert.AreEqual(1,S.beaconSteps);Work(a,(WatchAction)((int)WatchAction.BeaconA+S.Code(2)));Tick(.1f);
            Assert.AreEqual(1,S.beaconSteps);Assert.AreEqual(0,S.beaconProgress);Assert.AreEqual(1,S.mistakes);
        }
        [Test] public void LifelineNeedsAimCatchAndPullThenRestoresPartner()
        {
            Command(a,WatchAction.Begin);S.water=.5f;b.head=new(1,.4f,0);a.head=new(0,1.65f,0);a.feet=Vector3.zero;
            Tick(1.1f);Assert.IsTrue(S.down1);a.aim=Vector3.back;Command(a,WatchAction.CastLine);Assert.AreEqual(-1,S.ropeOwner);
            a.aim=(b.head-a.head).normalized;Command(a,WatchAction.CastLine);Assert.AreEqual(0,S.ropeOwner);
            a.action=WatchAction.PullLine;a.effort=1;Tick(.4f);Assert.IsTrue(S.down1);
            Command(b,WatchAction.CatchLine);Tick(.4f);Assert.IsFalse(S.down1);Assert.AreEqual(1,S.teleport1);Assert.Less(S.air1,.5f);
        }
        [Test] public void MissedRescueFailsAndRetryClearsAllSharedState()
        {
            Command(a,WatchAction.Begin);S.water=.5f;b.head=new(1,.4f,0);Tick(3.2f);Assert.AreEqual(WatchAct.Lost,S.act);
            Command(a,WatchAction.Retry);Assert.AreEqual(WatchAct.Briefing,S.act);Assert.IsFalse(S.down1);Assert.IsFalse(S.gateOne);Assert.AreEqual(0,S.elapsed);
        }
        [Test] public void RetryDoesNotReplayRetainedRemoteCommands()
        {
            b.command=WatchAction.Begin;b.sequence=1;Tick(.05f);
            Assert.AreEqual(WatchAct.FirstWatch,S.act);
            S.act=WatchAct.Lost;Command(a,WatchAction.Retry);Tick(.1f);
            Assert.AreEqual(WatchAct.Briefing,S.act,"A retained remote Begin belongs to the previous run.");
            b.sequence=2;Tick(.05f);Assert.AreEqual(WatchAct.FirstWatch,S.act);
            S.act=WatchAct.Lost;b.command=WatchAction.Retry;b.sequence=3;Tick(.05f);
            Assert.AreEqual(WatchAct.Briefing,S.act);
            S.act=WatchAct.Lost;Tick(.05f);
            Assert.AreEqual(WatchAct.Lost,S.act,"The same retained Retry cannot restart a later loss.");
        }
        [Test] public void InvalidTuningFailsBeforeTheWatchBegins()
        {
            Assert.Throws<System.ArgumentException>(()=>new StormWatchRules(new WatchTuning {pipeStepSeconds=0}));
            Assert.Throws<System.ArgumentException>(()=>new StormWatchRules(new WatchTuning {manualPumpDrain=float.NaN}));
        }
        [Test] public void DisconnectPausesFloodAndElapsedTime()
        {
            Command(a,WatchAction.Begin);b.connected=false;float water=S.water,elapsed=S.elapsed;Tick(20);Assert.AreEqual(water,S.water);Assert.AreEqual(elapsed,S.elapsed);
        }
    }
}
