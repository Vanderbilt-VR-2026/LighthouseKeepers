using System;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Day-owned rules. Input supplies jaw contact, wrist rotation and elapsed time only.
    public sealed class PipeRepairState
    {
        public readonly PipeRepairProfile Profile;
        public event Action Changed,QuarterTurn,Completed;
        readonly int[] order={0,1,2};
        public int BoltsDone{get;private set;}
        public int ActiveBolt=>BoltsDone<3?order[BoltsDone]:-1;
        public int TurnsDone{get;private set;}
        public float StrokeDegrees{get;private set;}
        public float SealElapsed{get;private set;}
        public float SeatElapsed{get;private set;}
        public bool NeedsReturn{get;private set;}
        public bool Complete=>BoltsDone==3;
        public bool NeedsSeal=>ActiveBolt==2&&SealElapsed<Profile.sealSeconds;
        public float InflowFraction=>1-BoltsDone/3f;
        bool touching;float direction,returned;
        public PipeRepairState(PipeRepairProfile profile){Profile=profile?profile:throw new ArgumentNullException(nameof(profile));}
        public void Reset(int seed,bool repaired=false)
        {
            for(int i=0;i<3;i++)order[i]=i;
            var random=new System.Random(seed);for(int i=2;i>0;i--){int j=random.Next(i+1);(order[i],order[j])=(order[j],order[i]);}
            BoltsDone=repaired?3:0;ResetBolt();Changed?.Invoke();
        }
        void ResetBolt(){TurnsDone=0;StrokeDegrees=SealElapsed=SeatElapsed=returned=direction=0;NeedsReturn=touching=false;}
        public void Observe(float seconds,float twistDegrees,bool contact,bool steady,bool canWork)
        {
            if(!float.IsFinite(seconds)||seconds<0||!float.IsFinite(twistDegrees))throw new ArgumentOutOfRangeException();
            if(Complete||!canWork)return;
            if(!contact)
            {
                if(touching){StrokeDegrees*=Profile.retainedPartialProgress;if(NeedsSeal)SealElapsed*=Profile.retainedPartialProgress;NeedsReturn=false;SeatElapsed=0;Changed?.Invoke();}
                touching=false;return;
            }
            touching=true;SeatElapsed+=seconds;if(SeatElapsed<Profile.seatSeconds)return;
            if(NeedsSeal){SealElapsed=steady?Mathf.Min(Profile.sealSeconds,SealElapsed+seconds):0;return;}
            float delta=Mathf.Clamp(twistDegrees,-Profile.maximumDegreesPerSecond*seconds,Profile.maximumDegreesPerSecond*seconds);
            if(Mathf.Abs(delta)<.01f)return;
            if(direction==0)direction=Mathf.Sign(delta);
            if(NeedsReturn){returned=Mathf.Max(0,returned-delta*direction);if(returned>=Profile.returnDegrees){NeedsReturn=false;returned=0;Changed?.Invoke();}return;}
            StrokeDegrees=Mathf.Clamp(StrokeDegrees+delta*direction,0,Profile.quarterTurnDegrees);
            if(StrokeDegrees<Profile.quarterTurnDegrees)return;
            TurnsDone++;StrokeDegrees=0;QuarterTurn?.Invoke();
            if(TurnsDone>=Profile.quarterTurns[ActiveBolt])
            {BoltsDone++;ResetBolt();Changed?.Invoke();if(Complete)Completed?.Invoke();}
            else{NeedsReturn=true;returned=0;Changed?.Invoke();}
        }
    }
}
