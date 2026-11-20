using System;
namespace LighthouseKeepers.Core.Progression
{
    [Serializable] public sealed class RadioAuthenticityFrame
    {
        // kind: 0 none, 1 Navy, 2 impostor. stage: 0 idle, 1 call, 2 challenged, 3 correction, 4 resolved.
        public int kind,stage,serial,result;
        public bool realDone,fakeDone,introDone;
        public float age;
        public RadioAuthenticityFrame Copy()=>(RadioAuthenticityFrame)MemberwiseClone();
        public bool Valid()=>kind>=0&&kind<=2&&stage>=0&&stage<=4&&serial>=0&&result>=0&&result<=3&&float.IsFinite(age)&&age>=0&&age<=3600&&(stage==0||kind>0)&&(stage!=4||result>0)&&(result==0||stage>=3);
    }
    // Caller decisions never mutate the generator, beacon or broadcast. All choices are host-owned.
    public sealed class RadioAuthenticityState
    {
        readonly HorrorTuning tuning;RadioAuthenticityFrame frame=new();
        public bool ReadOnly{get;set;}
        public int Kind=>frame.kind;public int Stage=>frame.stage;public int Serial=>frame.serial;
        public int Result=>frame.result;public float Age=>frame.age;
        public bool RealDone=>frame.realDone;public bool FakeDone=>frame.fakeDone;
        public bool Visible=>Stage>0;
        public bool NeedsDecision=>Stage>=1&&Stage<=3;
        public RadioAuthenticityState(HorrorTuning tuning){this.tuning=tuning;}
        void Change(int stage){frame.stage=stage;frame.age=0;frame.serial++;}
        void Start(int kind){frame.kind=kind;frame.result=0;Change(1);}
        public void Tick(float dt,bool nightThree,bool enabled,bool radioReady,bool firstShipApproaching,bool firstShipGone,bool presentationBusy)
        {
            if(ReadOnly||!float.IsFinite(dt)||dt<0)return;
            frame.age=Math.Min(3600,frame.age+dt);
            if(!nightThree||!enabled){if(Visible)Clear();return;}
            // Every frame rechecks the false order's relevance, including queued/retry feedback.
            if(Kind==2&&firstShipGone){frame.fakeDone=true;Clear();return;}
            if(Stage==4&&Age>=tuning.authResultSeconds){Clear();return;}
            if(!radioReady){if(NeedsDecision)Clear();return;}
            if(Visible||presentationBusy)return;
            if(!RealDone)Start(1);
            else if(!FakeDone&&firstShipApproaching&&!firstShipGone)Start(2);
        }
        void Clear(){frame.kind=frame.stage=frame.result=0;frame.age=0;frame.serial++;}
        public bool Command(int command,bool nightThree,bool powered)
        {
            if(ReadOnly||!nightThree||!powered||!NeedsDecision||command<0||command>2)return false;
            if(command==0){frame.result=0;Change(2);return true;}
            if(Stage!=2)return false; // Never accept an unchallenged call.
            bool correct=Kind==1?command==1:command==2;
            if(!correct){frame.result=3;Change(3);return true;}
            frame.result=Kind; if(Kind==1)frame.realDone=true;else frame.fakeDone=true;Change(4);return true;
        }
        public int ReplySymbol(int index,int realSymbol)=>Kind==2&&index==1?(realSymbol+1)%4:realSymbol;
        public RadioAuthenticityFrame Capture()=>frame.Copy();
        public bool Apply(RadioAuthenticityFrame source){if(!ReadOnly||source==null||!source.Valid())return false;frame=source.Copy();return true;}
    }
}
