using System;
namespace LighthouseKeepers.Core
{
    // No queue: a late/stale scare must not sound after the event, nor can repeated frames stack hits.
    public sealed class MusicAccentGate
    {
        double next=double.NegativeInfinity;
        public bool Try(double now,double age,float spacing,bool playing)
        {
            if(!double.IsFinite(now)||!double.IsFinite(age)||!float.IsFinite(spacing)||age<0||age>1.25||playing||now<next)return false;
            next=now+Math.Max(5,spacing);return true;
        }
    }
}
