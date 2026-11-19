using System;
using UnityEngine;

namespace LighthouseKeepers.Core.Progression
{
    public enum ScareBeat { Idle, Wake, Follow, Corner, Turn, Warning, Quiet, Escape, Return, Relief, Complete, Failed }

    // A separate, bounded host clock: a nightmare must continue while the watch is paused.
    [Serializable] public sealed class ScareSequenceFrame
    {
        public int scene, attempt, participants, caught, escaped;
        public ScareBeat beat;
        public float clock, age;
        public Vector3 body, catchOne, catchTwo;
        public float yaw,doorOne,doorTwo;
        public bool replay, reposition;
        static bool Point(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.sqrMagnitude<1000000;
        public bool Valid()=>scene>=0&&scene<=1&&attempt>=0&&attempt<10000&&
            (int)beat>=0&&(int)beat<=(int)ScareBeat.Failed&&
            float.IsFinite(clock)&&clock>=0&&clock<=86400&&float.IsFinite(age)&&age>=0&&age<=clock+.01f&&
            Point(body)&&Point(catchOne)&&Point(catchTwo)&&float.IsFinite(yaw)&&Mathf.Abs(yaw)<=360&&
            float.IsFinite(doorOne)&&doorOne>=0&&doorOne<=1&&float.IsFinite(doorTwo)&&doorTwo>=0&&doorTwo<=1&&
            participants>=0&&participants<=3&&caught>=0&&caught<=3&&escaped>=0&&escaped<=3&&
            (caught&~participants)==0&&(escaped&~participants)==0&&(caught&escaped)==0&&
            (scene==0?beat==ScareBeat.Idle&&participants==0&&attempt==0:beat!=ScareBeat.Idle&&participants>0&&attempt>0)&&
            (beat!=ScareBeat.Return||escaped!=0)&&
            (beat!=ScareBeat.Complete||caught==0);
        public ScareSequenceFrame Copy()=>(ScareSequenceFrame)MemberwiseClone();
    }

    public sealed class ScareSequenceState
    {
        readonly HorrorTuning tuning;
        ScareSequenceFrame frame=new();
        public bool ReadOnly {get;set;}
        public ScareBeat Beat=>frame.beat;
        public bool Active=>Beat>ScareBeat.Idle&&Beat<ScareBeat.Complete;
        public bool Paused=>Active||Failed;
        public bool Failed=>Beat==ScareBeat.Failed;
        public bool Finished=>Beat==ScareBeat.Complete;
        public bool Dark=>Beat>=ScareBeat.Wake&&Beat<=ScareBeat.Escape;
        public bool CanRescue=>Beat==ScareBeat.Return;
        public float Age=>frame.age;
        public float Clock=>frame.clock;
        public int Participants=>frame.participants;
        public int Caught=>frame.caught;
        public int Escaped=>frame.escaped;
        public int Attempt=>frame.attempt;
        public Vector3 Body=>frame.body;
        public Vector3 CatchPoint(int duty)=>duty==1?frame.catchOne:frame.catchTwo;
        public float Yaw=>frame.yaw;
        public float Door(int index)=>index==0?frame.doorOne:frame.doorTwo;
        public bool PullDoor(float packed){if(ReadOnly||Beat!=ScareBeat.Escape||!float.IsFinite(packed))return false;int index=Mathf.FloorToInt(packed/2);float angle=packed-index*2;if(index<0||index>1||angle<0||angle>1)return false;float before=Door(index);float next=Mathf.Clamp(angle,before,before+.18f);if(index==0)frame.doorOne=next;else frame.doorTwo=next;return true;}
        public bool Reposition=>frame.reposition;
        public bool CanSkip=>Active&&frame.replay;
        public ScareSequenceState(HorrorTuning tuning){this.tuning=tuning;}
        void Stage(ScareBeat beat){frame.beat=beat;frame.age=0;}
        public bool Begin(int humans,Vector3 start,bool replay=false)
        {
            if(ReadOnly||frame.scene!=0||humans<1||humans>3||!Finite(start))return false;
            frame=new(){scene=1,attempt=1,participants=humans,body=start,replay=replay};Stage(ScareBeat.Wake);return true;
        }
        static bool Finite(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.sqrMagnitude<1000000;
        public bool Place(Vector3 point,float yaw)
        {
            if(ReadOnly||!Active||!Finite(point)||!float.IsFinite(yaw))return false;
            frame.body=point;frame.yaw=Mathf.DeltaAngle(0,yaw);return true;
        }
        public void Tick(float dt,bool seen,bool cornerReached,int outsideMask,int downMask)
        {
            if(ReadOnly||!Active||!float.IsFinite(dt)||dt<=0||dt>1)return;
            frame.clock=Mathf.Min(86400,frame.clock+dt);frame.age=Mathf.Min(86400,frame.age+dt);
            switch(Beat){
                case ScareBeat.Wake:if(Age>=tuning.nightmareWakeSeconds&&seen)Stage(ScareBeat.Follow);break;
                case ScareBeat.Follow:if(cornerReached)Stage(ScareBeat.Corner);break;
                case ScareBeat.Corner:if(seen)Stage(ScareBeat.Turn);break;
                case ScareBeat.Turn:if(Age>=tuning.nightmareTurnSeconds)Stage(ScareBeat.Warning);break;
                case ScareBeat.Warning:if(Age>=tuning.telegraphSeconds)Stage(ScareBeat.Quiet);break;
                case ScareBeat.Quiet:if(Age>=tuning.quietSeconds)Stage(ScareBeat.Escape);break;
                case ScareBeat.Escape:
                    frame.escaped|=outsideMask&frame.participants&~frame.caught;
                    if((frame.escaped|frame.caught)==frame.participants&&frame.escaped!=0)Stage(ScareBeat.Return);
                    else if(Age>=tuning.nightmareEscapeSeconds)Stage(ScareBeat.Failed);
                    break;
                case ScareBeat.Return:
                    frame.caught&=downMask;
                    if(frame.caught==0&&(outsideMask&frame.participants)==0)Stage(ScareBeat.Relief);
                    else if(frame.caught!=0&&Age>=tuning.nightmareRescueSeconds)Stage(ScareBeat.Failed);
                    break;
                case ScareBeat.Relief:if(Age>=tuning.nightmareReliefSeconds)Stage(ScareBeat.Complete);break;
            }
        }
        public bool Catch(int duty,Vector3 point)
        {
            if(ReadOnly||Beat!=ScareBeat.Escape||(duty!=1&&duty!=2)||(frame.participants&duty)==0||((frame.caught|frame.escaped)&duty)!=0||!Finite(point))return false;
            frame.caught|=duty;if(duty==1)frame.catchOne=point;else frame.catchTwo=point;
            if(frame.caught==frame.participants)Stage(ScareBeat.Failed);
            return true;
        }
        // Changing crew composition restarts the safe wake checkpoint, never fabricates an escape.
        public bool CrewChanged(int humans,Vector3 start)
        {
            if(ReadOnly||!Paused||humans<1||humans>3||humans==frame.participants||!Finite(start))return false;
            Restart(humans,start);return true;
        }
        void Restart(int humans,Vector3 start)
        {int attempt=Mathf.Min(9999,frame.attempt+1);frame=new(){scene=1,attempt=attempt,participants=humans,body=start,replay=true};Stage(ScareBeat.Wake);}
        public bool Retry(Vector3 start){if(ReadOnly||!Failed||!Finite(start))return false;Restart(frame.participants,start);frame.reposition=true;return true;}
        public bool Skip(){if(ReadOnly||!CanSkip)return false;frame.caught=0;Stage(ScareBeat.Complete);return true;}
        public ScareSequenceFrame Capture()=>frame.Copy();
        public bool Apply(ScareSequenceFrame value){if(!ReadOnly||value==null||!value.Valid())return false;frame=value.Copy();return true;}
    }
}
