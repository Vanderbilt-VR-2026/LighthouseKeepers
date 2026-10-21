using System;
using UnityEngine;

namespace LighthouseKeepers.StormWatch
{
    public enum WatchAct { Briefing, FirstWatch, FirstRefuge, PowerFailure, SecondRefuge, Storm, RadioRelay, Ascent, Won, Lost }
    public enum WatchAction { None, Brace, Pipe, Pump, Generator, Radio, BeaconA, BeaconB, BeaconC, CastLine, CatchLine, PullLine, RepairSupply, RescueSupply, Begin, Continue, Retry, CallSun, CallWave, CallStar }

    [Serializable]
    public sealed class WatchTuning
    {
        [Min(1)] public float pipeStepSeconds = 14;
        [Min(1)] public float generatorStepSeconds = 18;
        [Min(1)] public float beaconStepSeconds = 8;
        [Min(1)] public float blackoutDelay = 12;
        [Min(1)] public float unsupportedGrace = 7;
        [Min(1)] public float airSeconds = 6;
        [Min(1)] public float rescueSeconds = 22;
        [Min(1)] public float pullSeconds = 4;
        [Min(0)] public float firstLeakRise = .007f;
        [Min(0)] public float blackoutRise = .027f;
        [Min(0)] public float stormRise = .033f;
        [Min(0)] public float manualPumpDrain = .055f;
        [Min(0)] public float poweredDrain = .065f;
        [Min(0)] public float mistakeRise = .18f;
        [Min(0)] public float refugeRise = .018f;
        [Min(.1f)] public float stationReach = 2.4f;
        [Min(1)] public float lineReach = 8;

        public void Validate()
        {
            float[] positive = { pipeStepSeconds, generatorStepSeconds, beaconStepSeconds, blackoutDelay,
                unsupportedGrace, airSeconds, rescueSeconds, pullSeconds, stationReach, lineReach };
            float[] nonnegative = { firstLeakRise, blackoutRise, stormRise, manualPumpDrain, poweredDrain, mistakeRise, refugeRise };
            foreach (float value in positive)
                if (!float.IsFinite(value) || value <= 0) throw new ArgumentException("Watch durations and reach must be finite and positive.");
            foreach (float value in nonnegative)
                if (!float.IsFinite(value) || value < 0) throw new ArgumentException("Watch water rates must be finite and nonnegative.");
        }
    }

    [Serializable]
    public sealed class KeeperInput
    {
        public bool connected;
        public Vector3 feet, head, left, right, aim = Vector3.forward;
        public Quaternion headRotation = Quaternion.identity, leftRotation = Quaternion.identity, rightRotation = Quaternion.identity;
        public WatchAction action;
        public float effort;
        public WatchAction command;
        public int sequence;
    }

    [Serializable]
    public sealed class WatchState
    {
        public WatchAct act;
        public float elapsed, actTime, water = -.25f, pipeProgress, generatorProgress, beaconProgress, unsupported;
        public int pipeSteps, generatorSteps, beaconSteps, mistakes, repairsRemaining, rescueBonus;
        public bool power = true, supplyChosen, gateOne, gateTwo, gateThree, beacon;
        public int order0, order1, order2;
        public float air0, air1, rescue0, rescue1;
        public bool down0, down1;
        public int ropeOwner = -1, ropeTarget = -1;
        public bool ropeCaught;
        public float pull;
        public int teleport0, teleport1;
        public Vector3 destination0, destination1;
        public string message = "Two keepers. Three watches. Reach the beacon together.";
        public int messageRevision;
        public int day = 1;
        public int Day => day;
        public bool Terminal => act == WatchAct.Won || act == WatchAct.Lost;
        public bool Down(int player) => player == 0 ? down0 : down1;
        public float Air(int player) => player == 0 ? air0 : air1;
        public float Rescue(int player) => player == 0 ? rescue0 : rescue1;
        public int Code(int step) => step == 0 ? order0 : step == 1 ? order1 : order2;
    }

    public static class WatchStations
    {
        public static readonly Vector3 Pipe = new(0, 1.1f, -2.7f), Brace = new(-.9f, 1.1f, -2.7f),
            Pump = new(-2.8f, 1.1f, -1.4f), Generator = new(1.7f, 4.35f, -2.85f),
            Radio = new(.65f, 7.55f, -3.1f), Beacon = new(0, 10.8f, 3.05f);
        public static Vector3 For(WatchAction action) => action switch
        {
            WatchAction.Brace => Brace, WatchAction.Pipe => Pipe, WatchAction.Pump => Pump,
            WatchAction.Generator => Generator, WatchAction.Radio => Radio,
            WatchAction.BeaconA or WatchAction.BeaconB or WatchAction.BeaconC => Beacon,
            _ => Vector3.zero
        };
    }

    /// <summary>Single authority for both keepers. Inputs are sampled at a bounded fixed step.</summary>
    public sealed class StormWatchRules
    {
        public WatchState State { get; private set; } = new();
        public readonly WatchTuning Tuning;
        public bool SoloPractice { get; }
        public bool SoloRadioRead { get; private set; }
        readonly int[] commands = new int[2];
        int attempt;
        float faultCooldown;
        public StormWatchRules(WatchTuning tuning = null, bool soloPractice = false)
        { SoloPractice = soloPractice; Tuning = tuning ?? new(); Tuning.Validate(); Reset(); }
        public void Reset(bool discardCommands = true)
        {
            State = new WatchState();
            SoloRadioRead = false;
            if (SoloPractice)
            {
                State.gateOne = State.gateTwo = State.gateThree = true;
                State.message = "Solo practice. All stairs open. Choose a watch on any instruction board.";
            }
            if (discardCommands) commands[0] = commands[1] = -1;
            faultCooldown = 0;
            int[] code = { 0, 1, 2 }; var random = new System.Random(++attempt * 7919);
            for (int i = 2; i > 0; i--) { int j = random.Next(i + 1); (code[i], code[j]) = (code[j], code[i]); }
            State.order0 = code[0]; State.order1 = code[1]; State.order2 = code[2];
        }
        public bool Near(KeeperInput input, Vector3 point) => Finite(input.head) && Vector3.Distance(input.head, point + Vector3.up * .35f) < Tuning.stationReach;
        public bool SelectSoloWatch(WatchAct act)
        {
            if (!SoloPractice || (act != WatchAct.FirstWatch && act != WatchAct.PowerFailure &&
                act != WatchAct.Storm && act != WatchAct.RadioRelay)) return false;
            Reset();
            State.power = act == WatchAct.FirstWatch || act == WatchAct.RadioRelay;
            if (act == WatchAct.RadioRelay) State.pipeSteps = State.generatorSteps = 3;
            Enter(act, "SOLO PRACTICE. Work stations yourself; no partner required. All stairs are open.");
            return true;
        }
        public static bool Finite(Vector3 v) => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
        bool Working(KeeperInput p, int id, WatchAction action) => p.connected && !State.Down(id) && p.action == action && Near(p, WatchStations.For(action));
        float Effort(KeeperInput p) => float.IsFinite(p.effort) ? Mathf.Clamp01(p.effort) : 0;
        public void Tick(float seconds, KeeperInput first, KeeperInput second)
        {
            if (!float.IsFinite(seconds) || seconds <= 0 || seconds > .25f) return;
            var s = State;
            // A lost partner pauses the shared simulation. Reconnection/retry is handled by the session.
            if (!first.connected || (!SoloPractice && !second.connected)) return;
            Command(0, first, second);
            if (!SoloPractice) Command(1, second, first);
            s = State;
            if (s.act == WatchAct.Briefing || s.Terminal) return;
            s.elapsed += seconds; s.actTime += seconds; faultCooldown = Mathf.Max(0, faultCooldown - seconds);
            if (s.act == WatchAct.PowerFailure && s.power && s.generatorSteps == 0 && s.actTime >= Tuning.blackoutDelay)
            { s.power = false; Tell("BLACKOUT. One keeper pump below; one restore the generator above."); }
            bool pumping = Working(first, 0, WatchAction.Pump) && Effort(first) > .05f || Working(second, 1, WatchAction.Pump) && Effort(second) > .05f;
            bool repairAct = s.act == WatchAct.FirstWatch || s.act == WatchAct.PowerFailure || s.act == WatchAct.Storm;
            float rise;
            if (s.act == WatchAct.FirstRefuge) rise = s.actTime < 15 ? -Tuning.poweredDrain : Tuning.refugeRise;
            else if (s.act == WatchAct.SecondRefuge) rise = s.actTime < 10 ? -Tuning.poweredDrain : Tuning.refugeRise * 1.5f;
            else if (s.act == WatchAct.RadioRelay || s.act == WatchAct.Ascent) rise = Tuning.stormRise * 1.6f;
            else if (s.act == WatchAct.FirstWatch) rise = s.pipeSteps < 3 ? Tuning.firstLeakRise : -Tuning.poweredDrain;
            else if (!s.power) rise = s.act == WatchAct.Storm ? Tuning.stormRise : Tuning.blackoutRise;
            else rise = s.pipeSteps < 3 ? Tuning.firstLeakRise : -Tuning.poweredDrain;
            if (pumping && !s.power) rise -= Tuning.manualPumpDrain;
            s.water = Mathf.Clamp(s.water + rise * seconds, -.25f, 10.5f);
            if (repairAct)
            {
                bool brace0 = Working(first, 0, WatchAction.Brace), brace1 = Working(second, 1, WatchAction.Brace);
                float pipe = Working(first, 0, WatchAction.Pipe) && (SoloPractice || brace1) ? Effort(first) : Working(second, 1, WatchAction.Pipe) && brace0 ? Effort(second) : 0;
                if (s.pipeSteps < 3 && pipe > 0)
                {
                    s.pipeProgress += seconds * pipe / (Tuning.pipeStepSeconds * (s.repairsRemaining > 0 ? .6f : 1));
                    if (s.pipeProgress >= 1)
                    { s.pipeProgress = 0; s.pipeSteps++; if (s.repairsRemaining > 0) s.repairsRemaining--; Tell(s.pipeSteps == 3 ? "Pipe sealed. Watch the water recover." : "Seal stage locked. Completed stages are safe."); }
                }
                if (!s.power && s.generatorSteps < 3)
                {
                    float gen = Working(first, 0, WatchAction.Generator) ? Effort(first) : Working(second, 1, WatchAction.Generator) ? Effort(second) : 0;
                    bool separatePump = SoloPractice || Working(first, 0, WatchAction.Generator) && Working(second, 1, WatchAction.Pump) && Effort(second) > .05f ||
                        Working(second, 1, WatchAction.Generator) && Working(first, 0, WatchAction.Pump) && Effort(first) > .05f;
                    if (separatePump) s.unsupported = Mathf.Max(0, s.unsupported - seconds * 2);
                    else if (gen > 0) s.unsupported += seconds;
                    if (gen > 0 && s.unsupported < Tuning.unsupportedGrace)
                    {
                        s.generatorProgress += seconds * gen / (Tuning.generatorStepSeconds * (s.repairsRemaining > 0 ? .6f : 1));
                        if (s.generatorProgress >= 1)
                        {
                            s.generatorProgress = 0; s.generatorSteps++; if (s.repairsRemaining > 0) s.repairsRemaining--;
                            if (s.generatorSteps == 3) { s.power = true; s.unsupported = 0; Tell("POWER RESTORED. Both keepers can finish the pipe below."); }
                            else Tell("Generator step locked. Keep pumping!");
                        }
                    }
                    else if (gen > 0 && faultCooldown <= 0) Fault("Support lost: unfinished generator step reset. Return to the manual pump.", false);
                }
                if (s.pipeSteps == 3 && s.power && (s.act == WatchAct.FirstWatch || s.generatorSteps == 3))
                {
                    if (s.act == WatchAct.FirstWatch) { s.gateOne = true; Enter(WatchAct.FirstRefuge, "FIRST WATCH COMPLETE. Climb to the generator landing; choose repair help or rescue protection."); }
                    else if (s.act == WatchAct.PowerFailure) { s.gateTwo = true; Enter(WatchAct.SecondRefuge, "POWER FAILURE SURVIVED. Climb to the radio refuge and choose supplies."); }
                    else { s.gateThree = true; Enter(WatchAct.RadioRelay, "All routes open. Radio keeper reads the code; lantern keeper turns the matching switches."); }
                }
            }
            if (s.act == WatchAct.RadioRelay)
            {
                if (SoloPractice && Working(first, 0, WatchAction.Radio) && !SoloRadioRead)
                {
                    SoloRadioRead = true;
                    Tell("Radio code noted. Climb to the lantern; its solo board shows your code.");
                }
                for (int id = 0; id < 2; id++)
                {
                    var p = id == 0 ? first : second; var other = id == 0 ? second : first;
                    int switchId = (int)p.action - (int)WatchAction.BeaconA;
                    if (switchId < 0 || switchId > 2 || !Working(p, id, p.action) || Effort(p) <= 0) continue;
                    bool alreadyLocked=false;for(int locked=0;locked<s.beaconSteps;locked++)if(s.Code(locked)==switchId)alreadyLocked=true;
                    if(alreadyLocked || !(SoloPractice ? SoloRadioRead : Working(other, 1-id, WatchAction.Radio))) continue;
                    if (switchId != s.Code(s.beaconSteps)) { if (faultCooldown <= 0) Fault("Wrong beacon switch. Unfinished step reset; ask the radio keeper for the next symbol.", true); continue; }
                    s.beaconProgress += seconds * Effort(p) / Tuning.beaconStepSeconds;
                    if (s.beaconProgress >= 1)
                    {
                        s.beaconProgress = 0; s.beaconSteps++;
                        if (s.beaconSteps == 3) { s.beacon = true; Enter(WatchAct.Ascent, "BEACON RESTORED. Radio keeper climb now. Both keepers must reach the top alive."); break; }
                        Tell("Beacon switch locked. Radio keeper, call the next symbol.");
                    }
                }
            }
            Survival(0, first, seconds);
            if (!SoloPractice) Survival(1, second, seconds);
            else if (s.down0) Lose("Out of air. Restart solo practice or choose another watch on the board.");
            if (s.down0 && s.down1) Lose("Both keepers need rescue. Restart together.");
            if (s.ropeOwner >= 0)
            {
                int owner = s.ropeOwner, target = s.ropeTarget;
                var a = owner == 0 ? first : second; var b = target == 0 ? first : second;
                if (s.Down(owner) || !s.Down(target) || Vector3.Distance(a.head, b.head) > Tuning.lineReach + 1) ClearRope();
                else if (s.ropeCaught && a.action == WatchAction.PullLine && a.head.y > s.water + .2f)
                {
                    s.pull += seconds * Effort(a) / Tuning.pullSeconds;
                    if (s.pull >= 1)
                    {
                        Vector3 destination = a.feet + Vector3.up * .03f;
                        if (target == 0) { s.down0 = false; s.air0 = 0; s.rescue0 = 0; s.destination0 = destination; s.teleport0++; }
                        else { s.down1 = false; s.air1 = 0; s.rescue1 = 0; s.destination1 = destination; s.teleport1++; }
                        ClearRope(); Tell("Keeper recovered. Catch your breath and regroup.");
                    }
                }
            }
            if (s.act == WatchAct.Ascent && !s.down0 && !s.down1 && Top(first) && (SoloPractice || Top(second)))
                Enter(WatchAct.Won, SoloPractice ? "SOLO PRACTICE COMPLETE. Beacon lit. Return to the lobby or try another watch." : "BOTH KEEPERS SAFE. Beacon lit. Three watches survived together.");
        }
        bool Top(KeeperInput p) => p.feet.y >= 9.45f && p.head.y > State.water && new Vector2(p.head.x, p.head.z).magnitude < 6.6f;
        void Survival(int id, KeeperInput p, float dt)
        {
            var s = State;
            float air = s.Air(id), rescue = s.Rescue(id); bool down = s.Down(id);
            if (!down)
            {
                air = p.head.y <= s.water ? air + dt : Mathf.Max(0, air - dt * 2);
                if (air >= Tuning.airSeconds) { down = true; rescue = Tuning.rescueSeconds + s.rescueBonus; Tell("KEEPER OVERBOARD. Throw the yellow lifeline; catch the ring, then pull."); }
            }
            else { rescue -= dt; if (rescue <= 0) Lose("The rescue window closed. Restart together."); }
            if (id == 0) { s.air0 = air; s.rescue0 = rescue; s.down0 = down; }
            else { s.air1 = air; s.rescue1 = rescue; s.down1 = down; }
        }
        void Command(int id, KeeperInput p, KeeperInput other)
        {
            if (p.sequence <= commands[id]) return;
            commands[id] = p.sequence; var s = State;
            if (p.command == WatchAction.Retry && s.Terminal)
            { Reset(false); State.destination0 = new(-8, .1f, 0); State.destination1 = new(-8, .1f, 1); State.teleport0 = s.teleport0 + 1; State.teleport1 = s.teleport1 + 1; return; }
            if (p.command == WatchAction.Begin && s.act == WatchAct.Briefing) { Enter(WatchAct.FirstWatch, "DAY 1 / FIRST WATCH. One keeper braces the coupling; the other grips the wrench and tightens the coupling in three stages."); return; }
            bool refuge = s.act == WatchAct.FirstRefuge || s.act == WatchAct.SecondRefuge;
            float floor = s.act == WatchAct.FirstRefuge ? 3.2f : 6.4f;
            if (refuge && !s.Down(id) && Mathf.Abs(p.feet.y - floor) < .4f)
            {
                if (!s.supplyChosen && (p.command == WatchAction.RepairSupply || p.command == WatchAction.RescueSupply))
                { s.supplyChosen = true; if (p.command == WatchAction.RepairSupply) s.repairsRemaining += 3; else s.rescueBonus += 10; Tell("Supply packed. Water is approaching; regroup and open the next watch when ready."); }
                if (p.command == WatchAction.Continue && s.supplyChosen && (SoloPractice || !s.Down(1-id) && Mathf.Abs(other.feet.y-floor) < .4f))
                {
                    s.pipeSteps = s.generatorSteps = 0; s.pipeProgress = s.generatorProgress = s.unsupported = 0;
                    if (s.act == WatchAct.FirstRefuge) Enter(WatchAct.PowerFailure, "DAY 2 / POWER FAILURE. A fresh leak below. The generator is struggling...");
                    else { s.power = false; Enter(WatchAct.Storm, "DAY 3 / THE STORM. Pump below, restore power above, then seal the final leak. The radio has the beacon code."); }
                }
            }
            if (s.act == WatchAct.RadioRelay && !s.Down(id) && Near(p, WatchStations.Radio) &&
                p.command >= WatchAction.CallSun && p.command <= WatchAction.CallStar)
                Tell("RADIO CALL: " + ((int)p.command-(int)WatchAction.CallSun == 0 ? "SUN" : (int)p.command-(int)WatchAction.CallSun == 1 ? "WAVE" : "STAR") + ". Lantern keeper, turn that switch.");
            if (p.command == WatchAction.CastLine && !s.Down(id) && s.Down(1-id) && p.head.y > s.water + .2f)
            {
                Vector3 delta = other.head-p.head;
                if (delta.magnitude <= Tuning.lineReach && Vector3.Dot(p.aim.normalized,delta.normalized) > .65f)
                { s.ropeOwner = id; s.ropeTarget = 1-id; s.ropeCaught = false; s.pull = 0; Tell("Lifeline attached. Stranded keeper: grab the yellow ring. Rescuer: grip and pull the handle."); }
            }
            if (p.command == WatchAction.CatchLine && s.ropeTarget == id) s.ropeCaught = true;
        }
        void Fault(string message, bool beacon)
        {
            State.water = Mathf.Min(10.5f, State.water + Tuning.mistakeRise); State.mistakes++; faultCooldown = 2;
            if (beacon) State.beaconProgress = 0; else State.generatorProgress = 0;
            Tell(message);
        }
        void ClearRope() { State.ropeOwner = State.ropeTarget = -1; State.ropeCaught = false; State.pull = 0; }
        void Enter(WatchAct act, string message) { State.act = act; if(act<=WatchAct.FirstRefuge)State.day=1;else if(act<=WatchAct.SecondRefuge)State.day=2;else if(act!=WatchAct.Won&&act!=WatchAct.Lost)State.day=3; State.actTime = 0; State.supplyChosen = false; Tell(message); }
        void Tell(string text) { State.message = text; State.messageRevision++; }
        void Lose(string message) { if (!State.Terminal) Enter(WatchAct.Lost, message); }
    }
}
