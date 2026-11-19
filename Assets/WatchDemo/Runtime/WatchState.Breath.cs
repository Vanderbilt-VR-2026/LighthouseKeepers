using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class WatchState {
    readonly float[] breathLease=new float[2];int breathReplica;
    public int BreathMask=>ReadOnly?breathReplica:(breathLease[0]>0?1:0)|(breathLease[1]>0?2:0);
    public bool BreathHeld(int duty)=>duty>=1&&duty<=2&&(BreathMask&duty)!=0;
    public bool SetBreath(int duty,bool held){if(ReadOnly||duty<1||duty>2||Recovery.Downed(duty))return false;breathLease[duty-1]=held?HorrorTuning.Current.breathInputLease:0;return true;}
    public void TickBreath(float dt){if(ReadOnly||!float.IsFinite(dt)||dt<0)return;for(int i=0;i<2;i++)breathLease[i]=Mathf.Max(0,breathLease[i]-dt);}
 }
}
