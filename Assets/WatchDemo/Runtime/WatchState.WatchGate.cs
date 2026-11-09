namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        // Solo-only start gate: a night's clock and flood wait until the keeper starts work
        // (first wrench grab or station use) or ImmersionTuning.soloWatchStartGraceSeconds pass.
        // Crew sessions, rehearsal and replicas are never gated. Tick() itself stays ungated.
        bool watchEngaged;
        double watchGateElapsed;
        public bool SoloWatchGate=>!ReadOnly&&Coop==null&&Immersion&&Immersion.soloWatchGate;
        public double WatchStartGraceSeconds=>Immersion?Immersion.soloWatchStartGraceSeconds:0;
        public bool WatchStartHeld=>SoloWatchGate&&!watchEngaged&&watchGateElapsed<WatchStartGraceSeconds&&!Failed&&!Complete;
        public double WatchGateElapsed=>watchGateElapsed;
        public void EngageWatch(){if(!resetting&&!ReadOnly)watchEngaged=true;}
        bool Engaged(bool accepted){if(accepted)EngageWatch();return accepted;}
        /// <summary>Advance the start gate; true when the watch clock and flood may advance this frame.</summary>
        public bool AdvanceWatchGate(double seconds)
        {
            if(!WatchStartHeld)return true;
            if(double.IsFinite(seconds)&&seconds>0)watchGateElapsed+=seconds;
            return !WatchStartHeld;
        }
        void ResetWatchGate(){watchEngaged=false;watchGateElapsed=0;}
    }
}
