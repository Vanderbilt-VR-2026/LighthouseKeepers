using System;

namespace LighthouseKeepers.Core.Progression
{
    [Serializable] public sealed class ShipFrame
    {
        public bool enabled,cleared;
        public double departureWait;
        public int seen, entered, passed, extended, closeWarned, wreck = -1;
        public double wreckAt;
        public float[] dark = Array.Empty<float>(), delay = Array.Empty<float>(), steady = Array.Empty<float>(), hold = Array.Empty<float>();
        public ShipFrame Copy() { var result = (ShipFrame)MemberwiseClone(); result.hold=(float[])hold.Clone(); result.dark = (float[])dark.Clone(); result.delay=(float[])delay.Clone(); result.steady=(float[])steady.Clone(); return result; }
        public bool Valid()
        {
            if (!double.IsFinite(departureWait)||departureWait<0) return false;
            if (hold==null || hold.Length!=dark?.Length || dark == null || delay==null || steady==null || delay.Length!=dark.Length || steady.Length!=dark.Length || dark.Length > 4 || !double.IsFinite(wreckAt) || wreckAt < 0) return false;
            int mask = (1 << dark.Length) - 1;
            if (seen < 0 || entered < 0 || passed < 0 || extended < 0 || closeWarned < 0 ||
                ((seen | entered | passed | extended | closeWarned) & ~mask) != 0 ||
                (entered & ~seen) != 0 || (passed & ~entered) != 0 || (extended & ~seen) != 0 ||
                (closeWarned & ~entered) != 0 || wreck < -1 || wreck >= dark.Length) return false;
            if (wreck >= 0 && ((entered & (1 << wreck)) == 0 || (passed & (1 << wreck)) != 0)) return false;
            if (wreck < 0 && wreckAt != 0) return false;
            foreach (float value in dark) if (!float.IsFinite(value) || value < 0 || value > 300) return false;
            foreach(float value in hold) if(!float.IsFinite(value)||value<0||value>180) return false;
            foreach (float value in delay) if (!float.IsFinite(value) || value<0) return false;
            foreach (float value in steady) if (!float.IsFinite(value) || value<0 || value>2) return false;
            return enabled || departureWait==0&&!cleared&&dark.Length == 0 && seen == 0 && entered == 0 && passed == 0 && extended == 0 && closeWarned == 0 && wreck == -1;
        }
    }

    // Pure host simulation. Presentation consumes snapshots and never decides an outcome.
    public sealed class ShipPassageState
    {
        readonly NightShipEvent[] events;
        readonly double approachDanger;
        readonly bool clearanceRequired;
        readonly bool wheelTracking; readonly float steadySeconds;
        ShipFrame frame;
        public bool ReadOnly { get; set; }
        public ShipPassageState(NightShipEvent[] events, double approachDanger = 0, bool wheelTracking = false, float steadySeconds = .6f, bool clearanceRequired = false)
        {
            this.clearanceRequired=clearanceRequired;this.wheelTracking=wheelTracking;this.steadySeconds=Math.Clamp(steadySeconds,.1f,2);
            this.events = events ?? Array.Empty<NightShipEvent>();
            this.approachDanger = double.IsFinite(approachDanger) ? Math.Clamp(approachDanger,0,30) : 0;
            if (this.events.Length > 4) throw new ArgumentException("At most four ships per night");
            frame = new ShipFrame { enabled = true, cleared=!clearanceRequired, dark = new float[this.events.Length],delay=new float[this.events.Length],steady=new float[this.events.Length],hold=new float[this.events.Length] };
        }
        public int Count => events.Length;
        public bool ClearanceGranted=>frame.cleared;
        public double DepartureWait=>frame.departureWait;
        public double TutorialDelay {get{double value=0;for(int i=0;i<Count;i++)if(events[i].aimingTutorial)value+=frame.delay[i];return value;}}
        public int SeenMask => frame.seen;
        public int PassedMask => frame.passed;
        public int CloseMask => frame.closeWarned;
        public int WreckIndex => frame.wreck;
        public double WreckAt => frame.wreckAt;
        public bool Lost => frame.wreck >= 0;
        public bool Finished => frame.passed == (1 << Count) - 1;
        public bool Passed(int i) => (frame.passed & (1 << i)) != 0;
        double PriorDelay(int i){double sum=0;for(int n=0;n<i;n++)sum+=frame.delay[n]+frame.hold[n];return sum;}
        public double Approach(int i) => PriorDelay(i) + events[i].approachSeconds + ((frame.extended & (1 << i)) != 0 ? events[i].broadcastExtensionSeconds : 0);
        public double WindowStart(int i) => frame.departureWait + events[i].sightedAtSeconds + Approach(i) + frame.hold[i];
        public double ThreatStart(int i) => Math.Max(events[i].sightedAtSeconds, WindowStart(i)-approachDanger);
        public bool Threatens(int i,double now) => !events[i].safePreview && !Passed(i) && now>=ThreatStart(i) && now<Exit(i);
        public double ThreatRemaining(double now){double result=double.PositiveInfinity;for(int i=0;i<Count;i++)if(Threatens(i,now))result=Math.Min(result,DarkRemaining(i));return result;}
        public double ThreatLimit(double now){double result=0;for(int i=0;i<Count;i++)if(Threatens(i,now))result=Math.Max(result,events[i].darkGraceSeconds);return result;}
        public double Exit(int i) => WindowStart(i) + events[i].windowSeconds + frame.delay[i];
        public double VisualAge(int i,double now) => now-frame.departureWait-events[i].sightedAtSeconds-frame.delay[i]-frame.hold[i];
        // A two-keeper valve job cannot leave a third keeper at the wheel. The captain may
        // wait OFFSHORE, never after entering danger. This does not stop water or the night clock.
        public bool CanHoldForCrew(double now, float lead, float limit)
        {
            int i=TrackingShip(now);
            return i>=0 && !events[i].safePreview && !Lost && (frame.entered&(1<<i))==0 && frame.hold[i]<Math.Clamp(limit,0,180)
                && now>=WindowStart(i)-Math.Max(lead,approachDanger+1);
        }
        public bool HoldForCrew(double from,double to,float lead,float limit)
        {
            if(ReadOnly || !CanHoldForCrew(to,lead,limit) || to<from) return false;
            int i=TrackingShip(to);
            frame.hold[i]+= (float)Math.Min(Math.Clamp(limit,0,180)-frame.hold[i],
                Math.Max(0,to-Math.Max(from,WindowStart(i)-Math.Max(lead,approachDanger+1))));
            return true;
        }
        public float Steady(int i) => frame.steady[i]/steadySeconds;
        public int TrackingShip(double now){if(!frame.cleared)return -1;for(int i=0;i<Count;i++)if((!events[i].safePreview||events[i].aimingTutorial)&&!Passed(i)&&now>=events[i].sightedAtSeconds+frame.departureWait&&now<Exit(i))return i;return -1;}
        public bool InWindow(int i, double now) => !Passed(i) && now >= WindowStart(i) && now < Exit(i);
        public bool AnyWindow(double now) { for (int i = 0; i < Count; i++) if (InWindow(i, now)) return true; return false; }
        public float Danger
        {
            get { float danger = 0; for (int i = 0; i < Count; i++) if (!events[i].safePreview && !Passed(i)) danger = Math.Max(danger, frame.dark[i] / events[i].darkGraceSeconds); return Math.Clamp(danger, 0, 1); }
        }
        public double DarkRemaining(int i) => Math.Max(0, events[i].darkGraceSeconds - frame.dark[i]);
        public bool Tick(double from, double to, bool shining, bool broadcast) => TickGuided(from,to,(_,__)=>shining,broadcast);
        public bool TickGuided(double from,double to,Func<int,double,bool> illuminating,bool broadcast)
        {
            if (ReadOnly || Lost) return false;
            if (!double.IsFinite(from) || !double.IsFinite(to) || from < 0 || to < from) throw new ArgumentOutOfRangeException();
            if(wheelTracking && to-from>.100001){bool result=false;while(to-from>.100001&&!Lost){result|=TickGuided(from,from+.1,illuminating,broadcast);from+=.1;}if(to>from&&!Lost)result|=TickGuided(from,to,illuminating,broadcast);return result;}
            if (ReadOnly || Lost) return false;
            if (!double.IsFinite(from) || !double.IsFinite(to) || from < 0 || to < from) throw new ArgumentOutOfRangeException();
            bool changed = false;
            if(clearanceRequired&&!frame.cleared){
                if(broadcast){frame.cleared=true;changed=true;}
                else {
                    frame.departureWait=Math.Min(to,frame.departureWait+(to-from));
                    // Waiting offshore is visible, but never starts a danger window or dark countdown.
                    for(int i=0;i<Count;i++)if(to-frame.departureWait>=events[i].sightedAtSeconds&&(frame.seen&(1<<i))==0){frame.seen|=1<<i;changed=true;}
                    return changed;
                }
            }
            for (int i = 0; i < Count; i++)
            {
                if (Passed(i)) continue;
                int bit = 1 << i; var ship = events[i];
                if (to >= ship.sightedAtSeconds+frame.departureWait && (frame.seen & bit) == 0) { frame.seen |= bit; changed = true; }
                // A warning sent before the rocks makes the captain slow the approach, buying time.
                if (broadcast && to >= ship.sightedAtSeconds+frame.departureWait && (frame.entered & bit) == 0 && (frame.extended & bit) == 0 && ship.broadcastExtensionSeconds > 0) { frame.extended |= bit; changed = true; }
                double start = ThreatStart(i), end = Exit(i);
                if (to < start) continue;
                if ((frame.entered & bit) == 0) { frame.entered |= bit; changed = true; }
                bool shining=illuminating(i,to);
                double exposed = Math.Max(0, Math.Min(to, end) - Math.Max(from, start));
                if(wheelTracking&&(!ship.safePreview||ship.aimingTutorial)){
                    frame.steady[i]=shining?Math.Min(steadySeconds,frame.steady[i]+(float)exposed):0;
                    shining=shining&&frame.steady[i]>=steadySeconds;
                    if(!shining){frame.delay[i]+=(float)Math.Max(0,Math.Min(to,end)-Math.Max(from,WindowStart(i)));end=Exit(i);}
                }
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
            if (source == null || !source.enabled || !source.Valid() || source.dark.Length != Count || source.wreckAt > now + .01 || source.departureWait>now+.001 || !clearanceRequired&&(source.departureWait!=0||!source.cleared) || !source.cleared&&(source.entered!=0||source.passed!=0||source.wreck>=0||source.extended!=0)) return false;
            for (int i = 0; i < Count; i++)
            {
                int bit = 1 << i; var e = events[i];
                double prior=0;for(int n=0;n<i;n++)prior+=source.delay[n]+source.hold[n];
                double window = source.departureWait + e.WindowStart + prior + source.hold[i] + ((source.extended & bit) != 0 ? e.broadcastExtensionSeconds : 0);
                double start = Math.Max(e.sightedAtSeconds,window-approachDanger);
                if (source.wreck == i && (source.dark[i] != e.darkGraceSeconds || source.wreckAt < start || source.wreckAt > window + e.windowSeconds + source.delay[i])) return false;
                if ((source.passed & bit) != 0 && source.dark[i] != 0) return false;
                if (source.hold[i]>now+.001 || e.safePreview&&source.hold[i]!=0 || source.delay[i]>now+.001 || source.steady[i]>steadySeconds+.001 || (!wheelTracking || e.safePreview&&!e.aimingTutorial)&&(source.delay[i]!=0||source.steady[i]!=0)) return false;
                if (source.dark[i] > e.darkGraceSeconds + .001 || e.safePreview && (source.dark[i] != 0 || source.wreck == i) ||
                    (source.seen & bit) != 0 && now+.001 < e.sightedAtSeconds+source.departureWait || (source.entered & bit) != 0 && now < start ||
                    (source.passed & bit) != 0 && now+.001 < window + e.windowSeconds + source.delay[i]) return false;
            }
            return true;
        }
        public bool Apply(ShipFrame source, double now)
        { if (!ReadOnly || !ValidReplica(source, now)) return false; frame = source.Copy(); return true; }
    }
}
