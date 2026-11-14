using System;

namespace LighthouseKeepers.Core.Progression
{
    [Serializable] public sealed class ShipFrame
    {
        public bool enabled;
        public int seen, entered, passed, extended, closeWarned, wreck = -1;
        public double wreckAt;
        public float[] dark = Array.Empty<float>();
        public ShipFrame Copy() { var result = (ShipFrame)MemberwiseClone(); result.dark = (float[])dark.Clone(); return result; }
        public bool Valid()
        {
            if (dark == null || dark.Length > 4 || !double.IsFinite(wreckAt) || wreckAt < 0) return false;
            int mask = (1 << dark.Length) - 1;
            if (seen < 0 || entered < 0 || passed < 0 || extended < 0 || closeWarned < 0 ||
                ((seen | entered | passed | extended | closeWarned) & ~mask) != 0 ||
                (entered & ~seen) != 0 || (passed & ~entered) != 0 || (extended & ~seen) != 0 ||
                (closeWarned & ~entered) != 0 || wreck < -1 || wreck >= dark.Length) return false;
            if (wreck >= 0 && ((entered & (1 << wreck)) == 0 || (passed & (1 << wreck)) != 0)) return false;
            if (wreck < 0 && wreckAt != 0) return false;
            foreach (float value in dark) if (!float.IsFinite(value) || value < 0 || value > 300) return false;
            return enabled || dark.Length == 0 && seen == 0 && entered == 0 && passed == 0 && extended == 0 && closeWarned == 0 && wreck == -1;
        }
    }

    // Pure host simulation. Presentation consumes snapshots and never decides an outcome.
    public sealed class ShipPassageState
    {
        readonly NightShipEvent[] events;
        readonly double approachDanger;
        ShipFrame frame;
        public bool ReadOnly { get; set; }
        public ShipPassageState(NightShipEvent[] events, double approachDanger = 0)
        {
            this.events = events ?? Array.Empty<NightShipEvent>();
            this.approachDanger = double.IsFinite(approachDanger) ? Math.Clamp(approachDanger,0,30) : 0;
            if (this.events.Length > 4) throw new ArgumentException("At most four ships per night");
            frame = new ShipFrame { enabled = true, dark = new float[this.events.Length] };
        }
        public int Count => events.Length;
        public int SeenMask => frame.seen;
        public int PassedMask => frame.passed;
        public int CloseMask => frame.closeWarned;
        public int WreckIndex => frame.wreck;
        public double WreckAt => frame.wreckAt;
        public bool Lost => frame.wreck >= 0;
        public bool Finished => frame.passed == (1 << Count) - 1;
        public bool Passed(int i) => (frame.passed & (1 << i)) != 0;
        public double Approach(int i) => events[i].approachSeconds + ((frame.extended & (1 << i)) != 0 ? events[i].broadcastExtensionSeconds : 0);
        public double WindowStart(int i) => events[i].sightedAtSeconds + Approach(i);
        public double ThreatStart(int i) => Math.Max(events[i].sightedAtSeconds, WindowStart(i)-approachDanger);
        public bool Threatens(int i,double now) => !events[i].safePreview && !Passed(i) && now>=ThreatStart(i) && now<Exit(i);
        public double ThreatRemaining(double now){double result=double.PositiveInfinity;for(int i=0;i<Count;i++)if(Threatens(i,now))result=Math.Min(result,DarkRemaining(i));return result;}
        public double ThreatLimit(double now){double result=0;for(int i=0;i<Count;i++)if(Threatens(i,now))result=Math.Max(result,events[i].darkGraceSeconds);return result;}
        public double Exit(int i) => WindowStart(i) + events[i].windowSeconds;
        public bool InWindow(int i, double now) => !Passed(i) && now >= WindowStart(i) && now < Exit(i);
        public bool AnyWindow(double now) { for (int i = 0; i < Count; i++) if (InWindow(i, now)) return true; return false; }
        public float Danger
        {
            get { float danger = 0; for (int i = 0; i < Count; i++) if (!events[i].safePreview && !Passed(i)) danger = Math.Max(danger, frame.dark[i] / events[i].darkGraceSeconds); return Math.Clamp(danger, 0, 1); }
        }
        public double DarkRemaining(int i) => Math.Max(0, events[i].darkGraceSeconds - frame.dark[i]);
        public bool Tick(double from, double to, bool shining, bool broadcast)
        {
            if (ReadOnly || Lost) return false;
            if (!double.IsFinite(from) || !double.IsFinite(to) || from < 0 || to < from) throw new ArgumentOutOfRangeException();
            bool changed = false;
            for (int i = 0; i < Count; i++)
            {
                if (Passed(i)) continue;
                int bit = 1 << i; var ship = events[i];
                if (to >= ship.sightedAtSeconds && (frame.seen & bit) == 0) { frame.seen |= bit; changed = true; }
                // A warning sent before the rocks makes the captain slow the approach, buying time.
                if (broadcast && to >= ship.sightedAtSeconds && (frame.entered & bit) == 0 && (frame.extended & bit) == 0 && ship.broadcastExtensionSeconds > 0) { frame.extended |= bit; changed = true; }
                double start = ThreatStart(i), end = Exit(i);
                if (to < start) continue;
                if ((frame.entered & bit) == 0) { frame.entered |= bit; changed = true; }
                double exposed = Math.Max(0, Math.Min(to, end) - Math.Max(from, start));
                if (shining || ship.safePreview) frame.dark[i] = 0;
                else if (exposed > 0)
                {
                    if ((frame.closeWarned & bit) == 0) { frame.closeWarned |= bit; changed = true; }
                    double previous = frame.dark[i];
                    frame.dark[i] = (float)Math.Min(ship.darkGraceSeconds, previous + exposed);
                    if (frame.dark[i] >= ship.darkGraceSeconds)
                    {
                        frame.wreck = i; frame.wreckAt = Math.Max(from, start) + ship.darkGraceSeconds - previous;
                        return true;
                    }
                }
                if (to >= end) { frame.passed |= bit; frame.dark[i] = 0; changed = true; }
            }
            return changed;
        }
        public ShipFrame Capture() => frame.Copy();
        public bool ValidReplica(ShipFrame source, double now)
        {
            if (source == null || !source.enabled || !source.Valid() || source.dark.Length != Count || source.wreckAt > now + .01) return false;
            for (int i = 0; i < Count; i++)
            {
                int bit = 1 << i; var e = events[i];
                double window = e.WindowStart + ((source.extended & bit) != 0 ? e.broadcastExtensionSeconds : 0);
                double start = Math.Max(e.sightedAtSeconds,window-approachDanger);
                if (source.wreck == i && (source.dark[i] != e.darkGraceSeconds || source.wreckAt < start || source.wreckAt > window + e.windowSeconds)) return false;
                if ((source.passed & bit) != 0 && source.dark[i] != 0) return false;
                if (source.dark[i] > e.darkGraceSeconds + .001 || e.safePreview && (source.dark[i] != 0 || source.wreck == i) ||
                    (source.seen & bit) != 0 && now < e.sightedAtSeconds || (source.entered & bit) != 0 && now < start ||
                    (source.passed & bit) != 0 && now < window + e.windowSeconds) return false;
            }
            return true;
        }
        public bool Apply(ShipFrame source, double now)
        { if (!ReadOnly || !ValidReplica(source, now)) return false; frame = source.Copy(); return true; }
    }
}
