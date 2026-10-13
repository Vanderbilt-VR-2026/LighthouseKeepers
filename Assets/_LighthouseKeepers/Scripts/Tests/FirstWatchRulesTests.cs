using NUnit.Framework;
using LighthouseKeepers.Progression;

namespace LighthouseKeepers.Tests
{
    public sealed class FirstWatchRulesTests
    {
        FirstWatchRules rules;
        [SetUp] public void Setup() { rules = new FirstWatchRules(); rules.Reset(42); rules.Begin(); }
        void Repair()
        {
            for (int i = 0; i < 3; i++) rules.Turn(rules.NextValve, FirstWatchRules.DegreesPerValve, true);
        }
        [Test] public void UnbracedAndWrongValveCannotCompleteRepair()
        {
            int valve = rules.NextValve;
            Assert.AreEqual(ValveResult.Ignored, rules.Turn(valve, 270, false));
            Assert.AreEqual(ValveResult.WrongValve, rules.Turn((valve + 1) % 3, 270, true));
            Assert.AreEqual(0, rules.SealedCount);
            Assert.AreEqual(0, rules.TurnDegrees);
            Assert.AreEqual(1, rules.Mistakes);
            Assert.AreEqual(ValveResult.Ignored, rules.Turn(valve, float.NaN, true));
        }
        [Test] public void PartialTurnsSurviveReleaseAndSealedValvesCannotBeFarmed()
        {
            int valve = rules.NextValve;
            rules.Turn(valve, 135, true);
            rules.Turn(valve, 135, false);
            Assert.AreEqual(135, rules.TurnDegrees);
            Assert.AreEqual(ValveResult.ValveSealed, rules.Turn(valve, 135, true));
            Assert.AreEqual(ValveResult.Ignored, rules.Turn(valve, 270, true));
            Assert.AreEqual(1, rules.SealedCount);
        }
        [Test] public void LandingCannotSkipRepairAndRecoveryRequiresStayingSafe()
        {
            rules.Tick(1, 2, true);
            Assert.AreEqual(WatchPhase.Repair, rules.Phase);
            Repair(); rules.Tick(1, 2, false);
            Assert.AreEqual(WatchPhase.Escape, rules.Phase);
            rules.Tick(1, 2, true);
            Assert.AreEqual(WatchPhase.Recovery, rules.Phase);
            rules.Tick(5, 2, true); rules.Tick(1, 2, false);
            Assert.AreEqual(WatchPhase.Escape, rules.Phase);
            rules.Tick(1, 2, true); rules.Tick(FirstWatchRules.RecoverySeconds, 2, true);
            Assert.AreEqual(WatchPhase.Complete, rules.Phase);
        }
        [Test] public void BriefSubmersionIsRecoverableButSustainedSubmersionFails()
        {
            rules.Tick(2, -.1f, false); rules.Tick(1, 1, false);
            Assert.AreEqual(0, rules.SubmergedSeconds);
            rules.Tick(FirstWatchRules.AirSeconds, -.1f, false);
            Assert.AreEqual(WatchPhase.Failed, rules.Phase);
            Assert.AreEqual(ValveResult.Ignored, rules.Turn(rules.NextValve, 270, true));
        }
        [Test] public void UnderwaterArrivalCannotWinAndResetClearsTheWholeAttempt()
        {
            Repair(); rules.Tick(6, -.1f, true);
            Assert.AreEqual(WatchPhase.Failed, rules.Phase);
            rules.Reset(43);
            Assert.AreEqual(WatchPhase.Briefing, rules.Phase);
            Assert.AreEqual(0, rules.SealedCount);
            Assert.AreEqual(0, rules.SubmergedSeconds);
            Assert.AreEqual(0, rules.Elapsed);
            CollectionAssert.AreEquivalent(new[] {0,1,2}, new[] {rules.ValveAt(0),rules.ValveAt(1),rules.ValveAt(2)});
        }
    }
}
