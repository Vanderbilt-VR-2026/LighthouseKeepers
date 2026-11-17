using System;
using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    [Serializable] public sealed class RecoverySlot
    {
        public bool down;
        public float remaining, progress, grace;
        public int serial;
        public RecoverySlot Copy() => (RecoverySlot)MemberwiseClone();
        public bool Valid() => float.IsFinite(remaining)&&remaining>=0&&remaining<=120&&
            float.IsFinite(progress)&&progress>=0&&progress<=1&&float.IsFinite(grace)&&grace>=0&&grace<=60&&
            serial>=0&&(!down||grace==0)&&(down||remaining==0);
    }
    [Serializable] public sealed class RecoveryFrame
    {
        // Stable duty slots (Lightkeeper=1, Machinist=2), independent of transport actor IDs.
        public int humans=1;
        public bool failed;
        public RecoverySlot lightkeeper=new(),machinist=new();
        public bool Valid() => humans>=1&&humans<=3&&lightkeeper!=null&&machinist!=null&&
            lightkeeper.Valid()&&machinist.Valid()&&((humans&1)!=0||!lightkeeper.down)&&
            ((humans&2)!=0||!machinist.down)&&(!failed||lightkeeper.down||machinist.down)&&
            (failed||!(lightkeeper.down&&machinist.down))&&
            (failed||(!lightkeeper.down||lightkeeper.remaining>0)&&(!machinist.down||machinist.remaining>0));
        public RecoveryFrame Copy()=>new(){humans=humans,failed=failed,lightkeeper=lightkeeper.Copy(),machinist=machinist.Copy()};
    }
    // All deadlines/progress live on the host. Inputs renew a short lease; stale grip cannot rescue.
    public sealed class CrewRecoveryState
    {
        readonly HorrorTuning tuning;
        RecoveryFrame frame=new();
        float lease; int target;
        public bool ReadOnly {get;set;}
        public bool Failed=>frame.failed;
        public bool AnyDown=>frame.lightkeeper.down||frame.machinist.down;
        public int Humans=>frame.humans;
        public CrewRecoveryState(HorrorTuning tuning){this.tuning=tuning;}
        public RecoverySlot Slot(int duty)=>duty==1?frame.lightkeeper:duty==2?frame.machinist:null;
        public bool Downed(int duty)=>Slot(duty)?.down==true;
        public void SetHumans(int mask)
        {
            if(ReadOnly||frame.failed||mask<1||mask>3||mask==frame.humans)return;
            // A disconnected keeper is no longer a rescue target. The remaining keeper is helped
            // by the bot; a rejoining keeper starts upright. Never move an absent body or fail on it.
            frame.humans=mask;
            if((mask&1)==0)frame.lightkeeper=new();
            if((mask&2)==0)frame.machinist=new();
            lease=0;target=0;
        }
        public bool Down(int duty)
        {
            var s=Slot(duty);
            if(ReadOnly||!tuning||frame.failed||s==null||(frame.humans&duty)==0||s.down||s.grace>0)return false;
            s.down=true;s.remaining=Mathf.Clamp(tuning.reviveSeconds,1,120);s.progress=0;s.serial++;lease=0;
            frame.failed=frame.lightkeeper.down&&frame.machinist.down;return true;
        }
        public bool Hold(int rescuer,int victim,bool held)
        {
            if(ReadOnly||frame.failed||rescuer<1||rescuer>2||rescuer==victim||Downed(rescuer)||!Downed(victim))return false;
            target=victim;lease=held?Mathf.Clamp(tuning.reviveGripLease,.1f,1):0;return true;
        }
        public void Tick(float seconds)
        {
            if(ReadOnly||frame.failed||!float.IsFinite(seconds)||seconds<=0)return;
            for(int duty=1;duty<=2;duty++){
                var s=Slot(duty);s.grace=Mathf.Max(0,s.grace-seconds);
                if(!s.down)continue;
                float hold=duty==target?Mathf.Min(seconds,lease,s.remaining):0;
                s.progress=Mathf.Min(1,s.progress+hold/Mathf.Clamp(tuning.reviveHoldSeconds,.5f,10));
                if(s.progress>=1){s.down=false;s.remaining=0;s.grace=Mathf.Clamp(tuning.reviveGraceSeconds,0,60);continue;}
                s.remaining=Mathf.Max(0,s.remaining-seconds);
                if(s.remaining==0)frame.failed=true;
            }
            lease=Mathf.Max(0,lease-seconds);
        }
        public RecoveryFrame Capture()=>frame.Copy();
        public bool Apply(RecoveryFrame value){if(!ReadOnly||value==null||!value.Valid())return false;frame=value.Copy();lease=0;target=0;return true;}
    }
}
