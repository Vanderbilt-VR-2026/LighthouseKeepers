using System;
namespace LighthouseKeepers.WatchDemo
{
    // Host-only narration gates. They observe existing state; never change station rules.
    public sealed class HarborCueRules
    {
        readonly bool[] played=new bool[102];readonly float[] next=new float[102];
        public void Reset(){Array.Clear(played,0,played.Length);Array.Clear(next,0,next.Length);}
        public static bool Transition(int id)=>id==4||id==5||id==17||id>=37&&id<=42||id==77||id==78||id==92||id==93;
        public static bool CanSpeak(int burst,float silence,bool transition,float recovery,float spacing)=>silence>=(burst>=2?recovery:transition?1f:spacing);
        public static bool InstructionExpires(int id)=>id==2||id==3||id==4||id>=7&&id<=12||id>=33&&id<=42||id==77||id==78||id>=82&&id<=91;
        public static float Cooldown(int id)=>id==83?20:id==91?15:0;
        public bool Allows(int id,float now)=>id>=76&&id<=93&&(Cooldown(id)>0?now>=next[id]:!played[id]);
        public bool AllowsCampaign(int id)=>id>=94&&id<=100&&!played[id];
        public void Played(int id,float now){if(id<76||id>100)return;played[id]=true;next[id]=now+Cooldown(id);}
        public static bool Danger(int id)=>id==18||id>=27&&id<=32||id==83||id>=87&&id<=89;
        public static int Tonight(bool pipe,bool generator,bool beacon,bool radio,int count)=>pipe&&generator&&beacon&&radio&&count==4?81:pipe&&generator&&count==2?80:0;
        public struct Context
        {
            public bool pipeOpening,upperJobs,wrenchDown,startGrace,tonightPair,tonightFull,valvePuzzle,valveReleased,soloClamp,crewRadios,pole,breakerTripped,lowFuel,engineStalled,lensSoot,wrongCode,broadcast,closeCall;
        }
        public static bool Trigger(int id,Context c)
        {
            switch(id){case 76:return c.pipeOpening;case 77:return c.upperJobs;case 78:return c.wrenchDown;case 79:return c.startGrace;case 80:return c.tonightPair;case 81:return c.tonightFull;case 82:return c.valvePuzzle;case 83:return c.valveReleased;case 84:return c.soloClamp;case 85:return c.crewRadios;case 86:return c.pole;case 87:return c.breakerTripped;case 88:return c.lowFuel;case 89:return c.engineStalled;case 90:return c.lensSoot;case 91:return c.wrongCode;case 92:return c.broadcast;case 93:return c.closeCall;default:return false;}
        }
        public void Observe(Context context,float now,Func<int,bool> queue)
        {for(int id=76;id<=93;id++)if(Trigger(id,context)&&Allows(id,now))queue(id);}
    }
}
