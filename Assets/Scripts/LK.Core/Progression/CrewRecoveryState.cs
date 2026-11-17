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
        public int lesson,lessonVictim;public float lessonAge;
        public RecoverySlot lightkeeper=new(),machinist=new();
        public bool Valid() => humans>=1&&humans<=3&&lesson>=0&&lesson<=6&&lessonVictim>=0&&lessonVictim<=2&&float.IsFinite(lessonAge)&&lessonAge>=0&&(lesson<4||lessonVictim>0)&&lightkeeper!=null&&machinist!=null&&
            lightkeeper.Valid()&&machinist.Valid()&&((humans&1)!=0||lesson==4&&lessonVictim==1||!lightkeeper.down)&&
            ((humans&2)!=0||lesson==4&&lessonVictim==2||!machinist.down)&&(!failed||lightkeeper.down||machinist.down)&&
            (failed||!(lightkeeper.down&&machinist.down))&&
            (failed||(!lightkeeper.down||lightkeeper.remaining>0)&&(!machinist.down||machinist.remaining>0));
        public RecoveryFrame Copy()=>new(){humans=humans,failed=failed,lesson=lesson,lessonVictim=lessonVictim,lessonAge=lessonAge,lightkeeper=lightkeeper.Copy(),machinist=machinist.Copy()};
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
        public int Lesson=>frame.lesson;
        public float LessonAge=>frame.lessonAge;
        public int LessonVictim=>frame.lessonVictim;
        public bool LessonActive=>Lesson>=1&&Lesson<=5;
        public bool RoomSealed=>Lesson>=2&&Lesson<=5;
        void Stage(int n){frame.lesson=n;frame.lessonAge=0;}
        public void AdvanceLesson(float seconds,int night,double watchSeconds,bool together,int victim,bool enabled)
        {
            if(ReadOnly||frame.failed||!enabled||night!=2||!float.IsFinite(seconds)||seconds<0||Lesson==6)return;
            frame.lessonAge+=seconds;
            if(Lesson==0&&watchSeconds>=tuning.lessonStartSeconds)Stage(1);
            else if(Lesson==1&&together&&LessonAge>=tuning.lessonCallSeconds)Stage(2);
            else if(Lesson==2&&LessonAge>=tuning.lessonSealSeconds)Stage(3);
            else if(Lesson==3&&LessonAge>=tuning.lessonThreatSeconds&&victim>=1&&victim<=2){
                frame.lessonVictim=victim;Stage(4);var slot=Slot(victim);slot.down=true;slot.progress=slot.grace=0;slot.remaining=tuning.reviveSeconds;slot.serial++;lease=0;
            }
            else if(Lesson==4&&!Downed(LessonVictim))Stage(5);
            else if(Lesson==5&&LessonAge>=tuning.lessonReliefSeconds)Stage(6);
        }
        public CrewRecoveryState(HorrorTuning tuning){this.tuning=tuning;}
        public RecoverySlot Slot(int duty)=>duty==1?frame.lightkeeper:duty==2?frame.machinist:null;
        public bool Downed(int duty)=>Slot(duty)?.down==true;
        public void SetHumans(int mask)
        {
            if(ReadOnly||frame.failed||mask<1||mask>3||mask==frame.humans)return;
            // A disconnected keeper is no longer a rescue target. The remaining keeper is helped
            // by the bot; a rejoining keeper starts upright. Never move an absent body or fail on it.
            if(LessonActive){frame.lightkeeper=new();frame.machinist=new();frame.lessonVictim=0;Stage(1);}
            frame.humans=mask;
            if((mask&1)==0&&!(Lesson==4&&LessonVictim==1))frame.lightkeeper=new();
            if((mask&2)==0&&!(Lesson==4&&LessonVictim==2))frame.machinist=new();
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
                if(Lesson!=4)s.remaining=Mathf.Max(0,s.remaining-seconds);
                if(s.remaining==0)frame.failed=true;
            }
            lease=Mathf.Max(0,lease-seconds);
        }
        public RecoveryFrame Capture()=>frame.Copy();
        public bool Apply(RecoveryFrame value){if(!ReadOnly||value==null||!value.Valid())return false;frame=value.Copy();lease=0;target=0;return true;}
    }
}
