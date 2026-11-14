using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
 public static class ShipNavigation
 {
        // Eight-metre fairway outside the island even for the full hull at every sampled turn.
        public static Pose Route(float age,float approach,float window,float depart,float sea=-.5f)
        {
            Vector3 a,b,c,d;float t;
            if(age<approach){t=Mathf.Clamp01(age/approach);a=new(-200,0,155);b=new(-110,0,130);c=new(-42,0,85);d=new(-42,0,45);}
            else if(age<approach+window){t=(age-approach)/window;a=new(-42,0,45);b=new(-42,0,25);c=new(-42,0,-25);d=new(-42,0,-45);}
            else{t=Mathf.Clamp01((age-approach-window)/depart);a=new(-42,0,-45);b=new(-42,0,-105);c=new(-140,0,-170);d=new(-205,0,-180);}
            float u=1-t;var p=u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;var tangent=3*u*u*(b-a)+6*u*t*(c-b)+3*t*t*(d-c);
            p.y=sea;return new Pose(p,Quaternion.LookRotation(tangent.normalized,Vector3.up));
        }
        public static float Heading(float wheel) => -225+Mathf.Clamp01(wheel)*270;
        public static float WheelFor(Vector3 position) => Mathf.Clamp01((Mathf.Atan2(position.x,position.z)*Mathf.Rad2Deg+225)/270);
        public static bool Aimed(float wheel,Vector3 position,float tolerance) => Mathf.Abs(Mathf.DeltaAngle(Heading(wheel),Mathf.Atan2(position.x,position.z)*Mathf.Rad2Deg))<=tolerance;
 }
}
