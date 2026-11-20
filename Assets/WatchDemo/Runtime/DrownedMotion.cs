using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    // Animates the apparition only; all timing/pose selection comes from the host receipt.
    public static class DrownedMotion
    {
        public static int Style(int night,int fault)=>(Mathf.Max(0,night-2)+Mathf.Max(0,fault))%3;
        public static float Life(int style)=>style==0?1.15f:style==2?1.8f:4.8f;
        public static void Pose(DrownedState d,double now,out Vector3 position,out Quaternion rotation)
        {
            float age=(float)System.Math.Max(0,now-d.RevealAt);position=new Vector3(d.X,d.Y,d.Z);rotation=Quaternion.Euler(0,d.Yaw,0);
            if(d.PoseKind==2){rotation*=Quaternion.Euler(90,0,0);return;}
            // Lunge advances only 30 cm. Clearance is checked against the actual animated face.
            if(d.PoseKind==0&&d.Phase==DrownedPhase.Manifest)position+=rotation*Vector3.forward*(.30f*Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.22f)));
        }
        public static Vector3 Centre(DrownedState d,double now,float height){Pose(d,now,out var p,out var q);return p+q*(Vector3.up*height*.58f);}
        public static float LampDip(double elapsed,int night,int floor)
        {
            if(night<2)return 1;
            float phase=(float)((elapsed+floor*3.71+night*2.1)%29);
            // One irregular-looking, broad voltage sag. No strobe or full-black interval.
            return phase<2.4f?1-.38f*Mathf.Pow(Mathf.Sin(phase/2.4f*Mathf.PI),2):1;
        }
    }
}
