using System;
namespace LighthouseKeepers.Core.Progression
{
    public struct FearInput
    {
        public bool dark, threatPresent, threatVisible, hidden, holdingBreath;
        public float threatDistance, water, shipDanger, isolation;
    }
    [Serializable] public sealed class FearFrame
    {
        public bool enabled, firstHoldingBreath, secondHoldingBreath;
        public int firstActor, secondActor;
        public float first, second;
        public bool Valid() => firstActor >= 0 && secondActor >= 0 && (secondActor == 0 || secondActor != firstActor) &&
            Unit(first) && Unit(second) && (secondActor != 0 || second == 0 && !secondHoldingBreath) &&
            (enabled || firstActor == 0 && secondActor == 0 && first == 0 && second == 0 && !firstHoldingBreath && !secondHoldingBreath);
        static bool Unit(float x) => float.IsFinite(x) && x >= 0 && x <= 1;
        public FearFrame Copy() => (FearFrame)MemberwiseClone();
    }
    // Host combines environmental evidence. A client never supplies its own fear value.
    public sealed class KeeperFearState
    {
        FearFrame frame = new FearFrame { enabled = true };
        public bool ReadOnly { get; set; }
        public float Peak => Math.Max(frame.first,frame.second);
        public float ForActor(int actor) => actor == frame.firstActor ? frame.first : actor == frame.secondActor ? frame.second : 0;
        public bool HoldingBreath(int actor) => actor == frame.firstActor ? frame.firstHoldingBreath : actor == frame.secondActor && frame.secondHoldingBreath;
        public static float Target(FearInput input)
        {
            float Unit(float v) => float.IsFinite(v) ? Math.Clamp(v,0,1) : 0;
            float threat = input.threatPresent && float.IsFinite(input.threatDistance) ?
                Math.Clamp(1-input.threatDistance/12,0,1)*(input.threatVisible?1:.65f) : 0;
            float environment = Math.Max(Unit(input.water)*.8f,Math.Max(Unit(input.shipDanger)*.75f,input.dark?.28f:0));
            return Math.Clamp(Math.Max(environment,threat)+Unit(input.isolation)*(input.dark?.16f:.04f),0,1);
        }
        public void Observe(float seconds,int firstActor,FearInput first,int secondActor,FearInput second)
        {
            if(ReadOnly || !float.IsFinite(seconds) || seconds < 0 || firstActor < 0 || secondActor < 0 || secondActor != 0 && secondActor == firstActor)return;
            float oldFirst=ForActor(firstActor),oldSecond=secondActor==0?0:ForActor(secondActor);
            frame.firstActor=firstActor;frame.secondActor=secondActor;
            frame.first=Follow(oldFirst,Target(first),seconds);
            frame.second=secondActor==0?0:Follow(oldSecond,Target(second),seconds);
            frame.firstHoldingBreath=first.hidden&&first.holdingBreath;
            frame.secondHoldingBreath=secondActor!=0&&second.hidden&&second.holdingBreath;
        }
        static float Follow(float current,float target,float dt) => current+(target-current)*(1-(float)Math.Exp(-dt/(target>current?1.2f:6f)));
        public static float BeatsPerMinute(float fear) => 62+78*Math.Clamp(fear,0,1);
        public static float HapticAmplitude(float fear) => .03f+.15f*Math.Clamp(fear,0,1);
        public static float WorldCutoff(float fear) => 22000-16000*Math.Clamp((fear-.6f)/.4f,0,1);
        public FearFrame Capture()=>frame.Copy();
        public bool Apply(FearFrame source)
        {if(!ReadOnly||source==null||!source.enabled||!source.Valid())return false;frame=source.Copy();return true;}
    }
}
