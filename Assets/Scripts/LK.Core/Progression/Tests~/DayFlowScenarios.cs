using System;
using System.Collections.Generic;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Presentation.Board;

static class DayFlowScenarios
{
    static int passed;
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Run(string name, Action scenario) { scenario(); passed++; Console.WriteLine("PASS " + name); }
    static DayObservation Water(float height = .5f, bool generator = true, bool failed = false, int tick = 1)
        => new DayObservation(tick, height, generator, failed);
    static DayFlow Begin(int day, int crew = 1)
    {
        var flow = new DayFlow { TryFireIncident = incident => true };
        flow.Begin(AssetPlans.Load(day), .15f, crew); return flow;
    }
    static DayFlow Fault()
    {
        var flow = Begin(2); flow.Advance(45, Water()); flow.Advance(.1f, Water(generator: false)); return flow;
    }
    static void Ready(DayFlow flow, int actor = 0)
    { flow.ReadFuel(actor, .25f); for (int i = 0; i < 3; i++) flow.PrimerStroke(actor); flow.SetChoke(actor, true); }
    static void RepairDay1(DayFlow flow)
    { flow.Advance(15, Water()); flow.WrenchGrabbed(0); flow.PlayerNearLeak(0); Check(flow.LeakRepairCompleted(0), "Repair rejected"); }
    static void Main()
    {
        Run("Day 1 solo route uses actual asset predicates and threshold", () =>
        {
            var flow = Begin(1); var completed = new List<int>(); flow.OnObjectiveCompleted += i => completed.Add(i);
            Check(!flow.BedGrip(0), "Early bed grip was banked");
            RepairDay1(flow); Check(!flow.Steps[3].Complete, "Water cleared while still high");
            flow.Advance(1, Water(.15001f)); Check(!flow.Steps[3].Complete, "Safe threshold rounded");
            flow.Advance(1, Water(.15f)); Check(flow.Steps[3].Complete, "Exact safe threshold rejected");
            Check(flow.Stage == DayStage.Playing, "Won without bed grip");
            Check(flow.BedGrip(0) && flow.Stage == DayStage.Results, "Solo did not reach results");
            Check(completed.Count == 5, "Expected one cue per objective");
        });
        Run("Incidents fire once at 15 and 45 seconds", () =>
        {
            foreach (int day in new[] { 1, 2 })
            {
                int fired = 0; var flow = Begin(day); flow.OnIncidentFired += incident => fired++;
                float due = day == 1 ? 15 : 45;
                flow.Advance(due - .1f, Water()); Check(fired == 0, "Incident fired early");
                flow.Advance(.2f, Water()); flow.Advance(10, Water()); Check(fired == 1, "Incident lost/duplicated");
            }
        });
        Run("Missing incident capability cannot count as fired", () =>
        {
            var flow = Begin(1); flow.TryFireIncident = incident => false;
            flow.Advance(16, Water()); Check(flow.IntegrationIssue.Length > 0, "Missing adapter hidden");
            Check(!flow.LeakRepairCompleted(0), "Repair counted before actual incident");
        });
        Run("Pump optional on Day 1 and contributes only statistics", () =>
        {
            var flow = Begin(1); for (int i = 0; i < 10; i++) flow.PumpStroke(0);
            RepairDay1(flow); flow.Advance(1, Water(.1f)); flow.BedGrip(0);
            Check(flow.Result.PumpStrokes == 10, "Stroke tally changed");
        });
        Run("Failed starter identifies primer and enforces 2-second cooldown", () =>
        {
            var flow = Fault(); flow.ReadFuel(0, .25f); flow.SetChoke(0, true);
            Check(flow.StarterPulled(0) == RestartResult.NeedsPrimer, "Missing primer reason");
            Check(flow.StarterPulled(0) == RestartResult.Cooldown, "Crank spam accepted");
            flow.Advance(2, Water(generator: false));
            Check(flow.StarterPulled(0) == RestartResult.NeedsPrimer, "Cooldown did not expire");
            Check(flow.GeneratorRestarts == 0, "Failed crank counted as restart");
        });
        Run("Empty fuel blocks until gauge reports refilled fuel", () =>
        {
            var flow = Fault(); flow.ReadFuel(0, 0); for (int i = 0; i < 3; i++) flow.PrimerStroke(0); flow.SetChoke(0, true);
            Check(flow.StarterPulled(0) == RestartResult.NoFuel, "Empty tank started");
            flow.ReadFuel(0, .25f); flow.Advance(2, Water(generator: false));
            Check(flow.StarterPulled(0) == RestartResult.Requested, "Refill did not recover");
        });
        Run("Wrong choke produces its own failure reason", () =>
        {
            var flow = Fault(); flow.ReadFuel(0, .25f); for (int i = 0; i < 3; i++) flow.PrimerStroke(0);
            Check(flow.StarterPulled(0) == RestartResult.NeedsChoke, "Wrong choke was accepted");
        });
        Run("Day 2 solo restart sends one command and waits for engine", () =>
        {
            var flow = Fault(); int commands = 0;
            flow.OnRequest += (kind, actor) => { if (kind == DayRequest.StartGenerator) commands++; };
            Ready(flow); Check(flow.StarterPulled(0) == RestartResult.Requested, "Valid restart rejected");
            Check(flow.StarterPulled(0) == RestartResult.AwaitingEngine && commands == 1, "Duplicate start command");
            Check(flow.GeneratorRestarts == 0, "Request counted as engine catch");
            flow.Advance(.1f, Water(generator: true)); Check(flow.GeneratorRestarts == 1, "Actual restart not counted");
            flow.Advance(1, Water(.1f)); Check(flow.BedGrip(0) && flow.Stage == DayStage.Results, "Solo Day 2 cannot finish");
        });
        Run("Two players may share primer and choke contributions", () =>
        {
            var flow = Begin(2, 2); flow.Advance(45, Water()); flow.Advance(.1f, Water(generator: false));
            flow.ReadFuel(0, .25f); for (int i = 0; i < 3; i++) flow.PrimerStroke(0); flow.SetChoke(1, true);
            Check(flow.StarterPulled(1) == RestartResult.Requested, "Co-op contributions rejected");
        });
        Run("An engine that does not catch can be cranked again after cooldown", () =>
        {
            var flow = Fault(); Ready(flow);
            Check(flow.StarterPulled(0) == RestartResult.Requested, "Initial command rejected");
            flow.Advance(2.1f, Water(generator: false));
            Check(flow.StarterPulled(0) == RestartResult.Requested, "Unacknowledged command permanently locked starter");
        });
        Run("Choke opens after ten seconds of actual engine operation", () =>
        {
            var flow = Fault(); Ready(flow); flow.StarterPulled(0); flow.Advance(.1f, Water());
            flow.SetChoke(0, false); Check(flow.ChokeClosed, "Choke opened during warm-up");
            flow.Advance(9.9f, Water()); Check(!flow.CanOpenChoke, "Warm-up timer shortened");
            flow.Advance(.2f, Water()); flow.SetChoke(0, false); Check(!flow.ChokeClosed, "Choke cannot open after warm-up");
        });
        Run("Day 3 solo is gated with no simulation commands", () =>
        {
            var flow = new DayFlow(); int commands = 0; flow.OnRequest += (kind, actor) => commands++;
            flow.Begin(AssetPlans.Load(3), .15f, 1); flow.Advance(100, Water());
            Check(flow.Stage == DayStage.WaitingForCrew && flow.CurrentObjectiveText == "Day 3 requires two keepers.", "Solo message/gate wrong");
            Check(commands == 0 && flow.ElapsedSeconds == 0, "Solo consumed simulation or time");
            flow.SetKeeperCount(2); Check(flow.Stage == DayStage.Playing && commands == 2, "Second keeper cannot begin");
        });
        Run("Day 3 rejects same actor, release, unsafe pressure and stale hold", () =>
        {
            var flow = Begin(3, 2); flow.Advance(15, Water());
            Check(!flow.CanRepairPipe(0), "No holder accepted");
            flow.ObserveIsolation(0, true, true); Check(!flow.CanRepairPipe(0), "Same actor accepted");
            Check(flow.CanRepairPipe(1), "Different keeper rejected");
            flow.ObserveIsolation(0, false, true); Check(!flow.CanRepairPipe(1), "Released valve accepted");
            flow.ObserveIsolation(0, true, false); Check(!flow.CanRepairPipe(1), "Unsafe pressure accepted");
            flow.ObserveIsolation(0, true, true); flow.Advance(.31f, Water()); Check(!flow.CanRepairPipe(1), "Stale hold accepted");
            flow.ObserveIsolation(0, true, true); flow.WrenchGrabbed(1);
            Check(flow.LeakRepairCompleted(1), "Fresh partner cannot repair");
            flow.Advance(1, Water(.1f)); Check(flow.BedGrip(1) && flow.Stage == DayStage.Results, "Co-op route cannot finish");
        });
        Run("Crew loss pauses Day 3 time and disallows repair", () =>
        {
            var flow = Begin(3, 2); flow.Advance(15, Water()); flow.ObserveIsolation(0, true, true);
            flow.SetKeeperCount(1); float before = flow.ElapsedSeconds;
            flow.Advance(30, Water()); Check(flow.ElapsedSeconds == before && !flow.CanRepairPipe(1), "Disconnect enabled solo progress");
            flow.SetKeeperCount(2); Check(!flow.CanRepairPipe(1), "Rejoin revived stale isolation hold");
        });
        Run("Repair permission is unavailable before the leak incident", () =>
        {
            var flow = Begin(1); Check(!flow.CanRepairPipe(0), "Pre-incident repair authorized");
            flow.Advance(15, Water()); Check(flow.CanRepairPipe(0), "Day 1 solo repair unauthorized");
        });
        Run("Failure wins over final bed and terminal results freeze", () =>
        {
            var flow = Begin(1); RepairDay1(flow); flow.Advance(1, Water(.1f));
            flow.Advance(.1f, Water(.95f, failed: true)); Check(!flow.BedGrip(0), "Bed beat failure");
            float elapsed = flow.Result.Seconds; flow.Advance(10, Water(2)); flow.PumpStroke(0);
            Check(flow.Stage == DayStage.Failed && flow.Result.Seconds == elapsed && flow.Result.PumpStrokes == 0, "Terminal metrics mutated");
        });
        Run("Retry is same-day clean state with no earlier-day punishment", () =>
        {
            var flow = Fault(); Ready(flow); flow.Advance(1, Water(.95f, failed: true));
            flow.Begin(AssetPlans.Load(2), .15f, 1);
            Check(flow.CurrentDay == 2 && flow.PrimerStrokes == 0 && flow.PumpStrokes == 0 && flow.ElapsedSeconds == 0, "Retry state leaked");
            Check(!flow.Steps[0].Complete && flow.Stage == DayStage.Playing, "Retry stage/objectives leaked");
        });
        Run("Results and live checkboxes contain all required board content", () =>
        {
            var flow = Begin(1); RepairDay1(flow);
            string live = string.Join("\n", DayBoardContent.Build(flow, "Day 1", "Briefing", flow.Stage, flow.ElapsedSeconds, flow.Result));
            Check(live.Contains("[x] Take the wrench") && live.Contains("[ ] Clear the water"), "Checkbox content wrong");
            flow.PumpStroke(0); flow.Advance(1, Water(.1f)); flow.BedGrip(0);
            string result = string.Join("\n", DayBoardContent.Build(flow, "Day 1", "", flow.Stage, flow.ElapsedSeconds, flow.Result));
            foreach (string field in new[] { "Day 1 results", "Time:", "Peak water:", "Pump strokes: 1", "Generator restarts: 0" })
                Check(result.Contains(field), "Missing result field " + field);
        });
        Run("New data-authored day needs no day-number code", () =>
        {
            var plan = AssetPlans.Load(1); plan.Day = 4;
            var flow = new DayFlow { TryFireIncident = incident => true }; flow.Begin(plan, .15f, 1);
            RepairDay1(flow); flow.Advance(1, Water(.1f)); flow.BedGrip(0);
            Check(flow.CurrentDay == 4 && flow.Stage == DayStage.Results, "Day 4 required a code branch");
        });
        Run("Invalid objectives, unknown incident IDs and NaN are rejected", () =>
        {
            var plan = AssetPlans.Load(1); plan.Objectives[0].incidentId = "missing";
            bool threw = false; try { new DayFlow().Begin(plan, .15f, 1); } catch (ArgumentException) { threw = true; }
            Check(threw, "Invalid incident ID accepted");
            var flow = Begin(1); threw = false;
            try { flow.Advance(float.NaN, Water()); } catch (ArgumentException) { threw = true; }
            Check(threw, "NaN time accepted");
        });
        Console.WriteLine("PASS " + passed + " scenarios; pure rules/content only, no Unity runtime or headset acceptance.");
    }
}
