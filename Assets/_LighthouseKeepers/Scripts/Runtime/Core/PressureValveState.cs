using System;
namespace LighthouseKeepers.Core
{
    // Only the host advances holds. A clamp replaces the second keeper when alone.
    public sealed class PressureValveState
    {
        readonly float timeout; double clock,lastHeld;
        public bool ReadOnly {get;set;}
        public bool CrewRequired {get;private set;}
        public bool Clamped {get;private set;}
        public int Holder {get;private set;}
        public bool Reduced=>CrewRequired?Holder>0:Clamped;
        public event Action Changed;
        public PressureValveState(float timeoutSeconds){timeout=Math.Max(.2f,timeoutSeconds);}
        public void SetCrew(bool two){if(ReadOnly||CrewRequired==two)return;CrewRequired=two;Reset();}
        public void Reset(){if(ReadOnly)return;Holder=0;Clamped=false;clock=lastHeld=0;Changed?.Invoke();}
        public bool Hold(int actor,bool pressed)
        {
            if(ReadOnly||!CrewRequired||actor<=0)return false;
            if(!pressed){if(Holder!=actor)return false;Holder=0;Changed?.Invoke();return true;}
            if(Holder!=0&&Holder!=actor)return false;
            bool change=Holder!=actor;Holder=actor;lastHeld=clock;if(change)Changed?.Invoke();return true;
        }
        public bool Clamp(){if(ReadOnly||CrewRequired||Clamped)return false;Clamped=true;Changed?.Invoke();return true;}
        public void Tick(float seconds){if(ReadOnly)return;if(!float.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException();clock+=seconds;if(Holder>0&&clock-lastHeld>timeout){Holder=0;Changed?.Invoke();}}
        public bool Allows(int worker)=>CrewRequired?Holder>0&&worker>0&&worker!=Holder:Clamped;
        public static bool Valid(bool crew,bool clamped,int holder)=>holder>=0&&(!crew?holder==0:!clamped);
        public bool Apply(bool crew,bool clamped,int holder){if(!ReadOnly||!Valid(crew,clamped,holder))return false;CrewRequired=crew;Clamped=clamped;Holder=holder;Changed?.Invoke();return true;}
    }
}
