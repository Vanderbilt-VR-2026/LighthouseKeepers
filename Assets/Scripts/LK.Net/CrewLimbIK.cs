using System;
using System.Numerics;

namespace LighthouseKeepers.Net
{
    /// <summary>Allocation-free two-bone geometry shared by the cosmetic arm and leg poser.</summary>
    public static class CrewLimbIK
    {
        public readonly struct Solution
        {
            public readonly Vector3 Bend, End;
            public readonly float Stretch;
            public Solution(Vector3 bend,Vector3 end,float stretch){Bend=bend;End=end;Stretch=stretch;}
        }
        static bool Finite(Vector3 v)=>float.IsFinite(v.X)&&float.IsFinite(v.Y)&&float.IsFinite(v.Z);
        public static Solution Solve(Vector3 root,Vector3 target,Vector3 pole,float upperLength,float lowerLength,float maxStretch=1)
        {
            if(!Finite(root)||!Finite(target)||!Finite(pole)||!float.IsFinite(upperLength)||!float.IsFinite(lowerLength)||
               !float.IsFinite(maxStretch)||upperLength<=0||lowerLength<=0||maxStretch<1)
                throw new ArgumentException("Limb positions and positive lengths must be finite; stretch must be at least one.");
            var delta=target-root;float distance=delta.Length();
            var direction=distance>.00001f?delta/distance:-Vector3.UnitY;
            float stretch=Math.Min(maxStretch,Math.Max(1,distance/(upperLength+lowerLength)));
            float a=upperLength*stretch,b=lowerLength*stretch;
            float epsilon=Math.Min(a,b)*.0001f;
            float reach=Math.Max(Math.Abs(a-b)+epsilon,Math.Min(a+b-epsilon,distance));
            var bendDirection=pole-root;
            bendDirection-=direction*Vector3.Dot(bendDirection,direction);
            if(bendDirection.LengthSquared()<.000001f)
                bendDirection=Vector3.Cross(direction,Math.Abs(direction.Y)<.9f?Vector3.UnitY:Vector3.UnitX);
            bendDirection=Vector3.Normalize(bendDirection);
            float along=(a*a-b*b+reach*reach)/(2*reach);
            float height=(float)Math.Sqrt(Math.Max(0,a*a-along*along));
            return new Solution(root+direction*along+bendDirection*height,root+direction*reach,stretch);
        }
    }

    /// <summary>One planted foot and a bounded cosmetic step; it does not move the player.</summary>
    public sealed class CrewFootstep
    {
        public Vector3 Position {get;private set;}
        public bool Moving {get;private set;}
        Vector3 from,to;
        float elapsed,duration,height;
        public void Reset(Vector3 position){Position=from=to=position;Moving=false;elapsed=0;}
        public void Begin(Vector3 goal,float seconds,float lift)
        {
            if(!float.IsFinite(seconds)||!float.IsFinite(lift)||seconds<=0||lift<0)throw new ArgumentOutOfRangeException();
            if(Moving)return;
            from=Position;to=goal;duration=seconds;height=lift;elapsed=0;Moving=true;
        }
        public void Tick(float seconds)
        {
            if(!float.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
            if(!Moving)return;
            elapsed=Math.Min(duration,elapsed+seconds);float t=elapsed/duration;
            float smooth=t*t*(3-2*t);
            Position=Vector3.Lerp(from,to,smooth)+Vector3.UnitY*((float)Math.Sin(Math.PI*t)*height);
            if(t>=1){Position=to;Moving=false;}
        }
    }
}
