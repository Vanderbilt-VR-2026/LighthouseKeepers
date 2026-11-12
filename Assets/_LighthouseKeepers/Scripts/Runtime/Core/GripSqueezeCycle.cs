namespace LighthouseKeepers.Core
{
    // A sustained grip is one squeeze. Hysteresis prevents sensor noise from adding counts.
    public sealed class GripSqueezeCycle
    {
        public const float SqueezeThreshold=.8f, RefillThreshold=.25f;
        bool squeezed;
        public bool Sample(float pressure,bool selected)
        {
            if(!selected){squeezed=false;return false;}
            if(!float.IsFinite(pressure))return false;
            if(pressure<=RefillThreshold)squeezed=false;
            if(squeezed||pressure<SqueezeThreshold)return false;
            squeezed=true;return true;
        }
    }
}
