using System;

namespace LighthouseKeepers.Gameplay.Interaction
{
    // Time/threshold rules are independent of tracking and simulation. Reset on release/tracking loss.
    public sealed class StrokeCycle
    {
        bool sawBottom, sawTop;
        double lastBottom, lastAccepted = double.NegativeInfinity;
        public bool TopReached { get; private set; }
        public void Reset() { sawBottom = sawTop = false; TopReached = false; }
        public bool Advance(float travel, double now, float minimumSeconds)
        {
            TopReached = false;
            if (float.IsNaN(travel) || float.IsInfinity(travel) || double.IsNaN(now) || double.IsInfinity(now) || minimumSeconds <= 0 || float.IsNaN(minimumSeconds) || float.IsInfinity(minimumSeconds)) { Reset(); return false; }
            if (travel <= .1f)
            {
                bool accepted = sawBottom && sawTop && now - lastBottom >= minimumSeconds && now - lastAccepted >= minimumSeconds;
                if (accepted) lastAccepted = now;
                sawBottom = true; sawTop = false; lastBottom = now;
                return accepted;
            }
            if (sawBottom && travel >= .9f && !sawTop) { sawTop = true; TopReached = true; }
            return false;
        }
    }
}
