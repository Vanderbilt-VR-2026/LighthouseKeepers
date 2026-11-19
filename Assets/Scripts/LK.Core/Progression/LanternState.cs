using System;
namespace LighthouseKeepers.Core.Progression
{
    public enum LanternWick { Off, Low, High }
    [Serializable] public sealed class LanternFrame
    {
        public bool enabled;
        public int wick0,wick1,refillSlot=-1;
        public float oil0,oil1,canOil;
        public LanternFrame Copy()=>(LanternFrame)MemberwiseClone();
        public bool Valid()=>wick0>=0&&wick0<=2&&wick1>=0&&wick1<=2&&Fraction(oil0)&&Fraction(oil1)&&Fraction(canOil)&&refillSlot>=-1&&refillSlot<=1&&(enabled||wick0==0&&wick1==0&&oil0==0&&oil1==0&&canOil==0&&refillSlot==-1);
        static bool Fraction(float v)=>float.IsFinite(v)&&v>=0&&v<=1;
    }
    // One host clock burns both lamps; replicas never infer fuel or spend the shared oil can.
    public sealed class LanternState
    {
        public bool ReadOnly {get;set;}
        public bool UnlimitedOil {get;}
        public bool FocusOnly {get;}
        readonly float highSeconds,lowSeconds,pourSeconds,stunSeconds,stunCost;
        LanternFrame frame=new(){enabled=true,oil0=1,oil1=1,canOil=1};
        readonly float[] exposure=new float[2];
        public LanternState(float high=90,float low=180,float pour=4,float stun=2,float cost=.15f,bool unlimited=false,bool focusOnly=false)
        {if(!float.IsFinite(high)||!float.IsFinite(low)||!float.IsFinite(pour)||!float.IsFinite(stun)||!float.IsFinite(cost)||high<=0||low<=0||pour<=0||stun<=0||cost<=0||cost>1)throw new ArgumentOutOfRangeException();highSeconds=high;lowSeconds=low;pourSeconds=pour;stunSeconds=stun;stunCost=cost;UnlimitedOil=unlimited;FocusOnly=focusOnly;if(unlimited)frame.canOil=0;}
        static bool Slot(int slot)=>slot==0||slot==1;
        public float Oil(int slot)=>slot==0?frame.oil0:slot==1?frame.oil1:0;
        public LanternWick Wick(int slot)=>(LanternWick)(slot==0?frame.wick0:slot==1?frame.wick1:0);
        public float CanOil=>frame.canOil;
        public bool Lit(int slot)=>Oil(slot)>0&&Wick(slot)!=LanternWick.Off;
        void Oil(int slot,float value){if(slot==0)frame.oil0=Math.Clamp(value,0,1);else frame.oil1=Math.Clamp(value,0,1);}
        public bool Cycle(int slot)
        {if(ReadOnly||!Slot(slot))return false;int next=FocusOnly?(Wick(slot)==LanternWick.Off?2:0):((int)Wick(slot)+1)%3;if(slot==0)frame.wick0=next;else frame.wick1=next;exposure[slot]=0;return true;}
        public bool Extinguish(int slot){if(ReadOnly||!Slot(slot)||Wick(slot)==LanternWick.Off)return false;if(slot==0)frame.wick0=0;else frame.wick1=0;exposure[slot]=0;return true;}
        public void Tick(float dt)
        {if(ReadOnly||UnlimitedOil||!float.IsFinite(dt)||dt<=0)return;for(int i=0;i<2;i++)if(Lit(i))Oil(i,Oil(i)-dt/(Wick(i)==LanternWick.High?highSeconds:lowSeconds));}
        public bool Pour(int slot,float dt)
        {
            if(ReadOnly||UnlimitedOil||!Slot(slot)||!float.IsFinite(dt)||dt<=0||frame.canOil<=0||Oil(slot)>=1||frame.refillSlot!=-1&&frame.refillSlot!=slot)return false;
            frame.refillSlot=slot;float amount=Math.Min(1-Oil(slot),Math.Min(frame.canOil,dt/pourSeconds));Oil(slot,Oil(slot)+amount);frame.canOil=Math.Max(0,frame.canOil-amount);
            // The one small can services one lantern once. No infinite top-ups or sharing partial charges.
            if(Oil(slot)>=.99999f)frame.canOil=0;return amount>0;
        }
        public bool Expose(int slot,float dt,bool held,bool clearAim,bool entityPresent)
        {
            if(ReadOnly||!Slot(slot))return false;
            if(!float.IsFinite(dt)||dt<=0||!held||!clearAim||!entityPresent||Wick(slot)!=LanternWick.High||Oil(slot)<stunCost){exposure[slot]=0;return false;}
            exposure[slot]+=dt;if(exposure[slot]+.00001f<stunSeconds)return false;exposure[slot]=0;if(!UnlimitedOil)Oil(slot,Oil(slot)-stunCost);return true;
        }
        public LanternFrame Capture()=>frame.Copy();
        public bool Apply(LanternFrame value){if(!ReadOnly||value==null||!value.enabled||!value.Valid())return false;frame=value.Copy();Array.Clear(exposure,0,2);return true;}
    }
}
