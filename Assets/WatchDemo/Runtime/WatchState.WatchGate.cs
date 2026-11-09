using System;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        bool watchEngaged;
        double watchGateElapsed,stationGraceUntil;
        int seenStationMask,nightStartSeenMask;
        public bool SoloWatchGate=>Coop==null&&StartGateEnabled;
        bool StartGateEnabled=>!NetworkRehearsal&&Immersion&&Master&&Master.watchStartGate;
        public double WatchStartGraceSeconds=>Master?Master.watchStartGraceSeconds:0;
        public bool WatchStartHeld=>StartGateEnabled&&!watchEngaged&&watchGateElapsed<WatchStartGraceSeconds&&!Failed&&!Complete;
        public double WatchGateElapsed=>watchGateElapsed;
        public double StationGraceRemaining=>Master&&Master.firstStationGrace?Math.Max(0,stationGraceUntil-Elapsed):0;
        public void EngageWatch(){if(!resetting&&!ReadOnly)watchEngaged=true;}
        bool Engaged(bool accepted){if(accepted)EngageWatch();return accepted;}
        public bool AdvanceWatchGate(double seconds)
        {
            if(ReadOnly)return !WatchStartHeld;
            if(!WatchStartHeld)return true;
            if(double.IsFinite(seconds)&&seconds>0)watchGateElapsed=Math.Min(WatchStartGraceSeconds,watchGateElapsed+seconds);
            return !WatchStartHeld;
        }
        // Only the host introduces a station. Already-taught jobs do not renew grace every night.
        public void OfferStationGrace(KeeperStep step)
        {
            if(ReadOnly||!Master||!Master.firstStationGrace)return;
            string station=ObjectiveDirector.Job(step);int bit=station==PipeStation?1:station==GeneratorStation?2:station==BeaconStation?4:station==RadioStation?8:0;
            if(bit==0||(seenStationMask&bit)!=0||IsPending(station)||IsDone(station))return;
            seenStationMask|=bit;
            double length=station==GeneratorStation?Master.firstGeneratorGraceSeconds:Master.firstStationGraceSeconds;
            stationGraceUntil=Math.Max(stationGraceUntil,Elapsed+length);
        }
        void ResetWatchGate(){watchEngaged=false;watchGateElapsed=0;stationGraceUntil=0;}
        bool ValidGraceSnapshot(WatchSnapshot s)=>s.watchGateElapsed<=WatchStartGraceSeconds+.01&&s.stationGraceUntil<=s.elapsed+(Master?Math.Max(Master.firstStationGraceSeconds,Master.firstGeneratorGraceSeconds):0)+.01;
    }
}
