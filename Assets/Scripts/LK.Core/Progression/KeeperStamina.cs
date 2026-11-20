using UnityEngine;
namespace LighthouseKeepers.Core.Progression {
    // Local locomotion budget; does not depend on empty hands or arm tracking velocity.
    public sealed class KeeperStamina {
        readonly HorrorTuning tuning; bool exhausted, warned;
        public float Remaining {get;private set;}=1;
        public bool Running {get;private set;}
        public bool Warning {get;private set;}
        public KeeperStamina(HorrorTuning t){tuning=t;}
        public float Tick(float dt,bool held,bool moving,bool allowed){
            Warning=false;if(!float.IsFinite(dt)||dt<=0)return Running?tuning.sprintMultiplier:1;
            Running=allowed&&held&&moving&&!exhausted&&Remaining>0;
            if(Running){Remaining=Mathf.Max(0,Remaining-dt/Mathf.Max(.1f,tuning.sprintSeconds));
                if(Remaining<=tuning.sprintWarning&&!warned){Warning=true;warned=true;}if(Remaining<=0){exhausted=true;Running=false;}}
            else if(!held||!allowed){Remaining=Mathf.Min(1,Remaining+dt/Mathf.Max(.1f,tuning.sprintCooldown));exhausted=false;if(Remaining>tuning.sprintWarning)warned=false;}
            return Running?tuning.sprintMultiplier:1;
        }
    }
}
