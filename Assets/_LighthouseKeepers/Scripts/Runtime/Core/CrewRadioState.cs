using System;
namespace LighthouseKeepers.Core
{
    // Two leased radios, not a second audio stream. Only host-approved holds enter snapshots.
    public sealed class CrewRadioState
    {
        readonly int[] actors=new int[2];readonly double[] seen=new double[2];double clock;readonly float timeout;
        public bool ReadOnly {get;set;}
        public CrewRadioState(float timeout){this.timeout=Math.Max(.2f,timeout);}
        public int Actor(int index)=>index>=0&&index<2?actors[index]:0;
        public bool Transmitting(int actor)=>actor>0&&(actors[0]==actor||actors[1]==actor);
        public bool Set(int index,int actor,bool active)
        {
            if(ReadOnly||index<0||index>1||actor<=0||actors[index]!=0&&actors[index]!=actor)return false;
            if(!active){actors[index]=0;return true;}actors[index]=actor;seen[index]=clock;return true;
        }
        public void Tick(float seconds){if(ReadOnly)return;if(!float.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException();clock+=seconds;for(int i=0;i<2;i++)if(actors[i]!=0&&clock-seen[i]>timeout)actors[i]=0;}
        public void Clear(){if(ReadOnly)return;actors[0]=actors[1]=0;clock=0;}
        public void Remove(int actor){if(ReadOnly)return;for(int i=0;i<2;i++)if(actors[i]==actor)actors[i]=0;}
        public bool Apply(int a,int b){if(!ReadOnly||a<0||b<0)return false;actors[0]=a;actors[1]=b;return true;}
    }
}
