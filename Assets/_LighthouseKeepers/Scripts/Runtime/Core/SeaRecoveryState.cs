using System;
using UnityEngine;
namespace LighthouseKeepers.Core
{
    // Host-owned per-keeper recovery. No elapsed-time or flood reset for the other keeper.
    [Serializable] public sealed class SeaRecoveryState
    {
        [Serializable] public sealed class Slot {public int actor=-1,serial;public float remaining,cooldown;}
        public Slot first=new(),second=new();public bool lost;
        public const float RecoverySeconds=10;
        public static bool Outside(Vector3 head)=>head.y<-.75f || head.y<3&&
            new Vector2((head.x+.2f)/7.8f,(head.z+3.2f)/13.7f).sqrMagnitude>1;
        public void Tick(int a,Vector3 p,int b,Vector3 q,float dt)
        {if(lost||!float.IsFinite(dt)||dt<0)return;TickSlot(first,a,p,b>0,dt);if(b>0)TickSlot(second,b,q,true,dt);else {second.actor=-1;second.remaining=0;}}
        void TickSlot(Slot s,int actor,Vector3 p,bool crew,float dt)
        {
            if(s.actor!=actor){s.actor=actor;s.remaining=s.cooldown=0;s.serial=0;}
            if(s.remaining>0){s.remaining=Mathf.Max(0,s.remaining-dt);if(s.remaining==0){s.serial++;s.cooldown=2;}return;}
            if(s.cooldown>0){s.cooldown=Mathf.Max(0,s.cooldown-dt);return;}
            if(!Outside(p))return;if(!crew){lost=true;return;}s.remaining=RecoverySeconds;
        }
        public Slot For(int actor)=>first.actor==actor?first:second.actor==actor?second:null;
        public static bool Valid(SeaRecoveryState s)=>s!=null&&Valid(s.first)&&Valid(s.second)&&(s.second.actor<0||s.first.actor!=s.second.actor);
        static bool Valid(Slot s)=>s!=null&&s.actor>=-1&&s.serial>=0&&float.IsFinite(s.remaining)&&s.remaining>=0&&s.remaining<=RecoverySeconds&&float.IsFinite(s.cooldown)&&s.cooldown>=0&&s.cooldown<=2;
    }
}
