using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    public interface IWatchFearThreat
    {
        bool Active { get; }
        Vector3 Position { get; }
        bool VisibleTo(int actor,Vector3 head);
        bool Hidden(int actor);
        bool HoldingBreath(int actor);
    }
    public sealed partial class WatchState
    {
        public KeeperFearState Fear {get;private set;}
        public IWatchFearThreat FearThreat {get;set;}
        float shipFear;
        public void SetShipFear(float danger){if(!ReadOnly&&float.IsFinite(danger))shipFear=Mathf.Clamp01(danger);}
        public void ObserveKeepers(float dt,int a,Vector3 p,int b,Vector3 q)
        {
            if(ReadOnly||Failed||Fear==null)return;
            if(!Finite(p)||!Finite(q))return;
            Fear.Observe(dt,a,FearAt(a,p,b==0?1:Mathf.InverseLerp(4,12,Vector3.Distance(p,q))),b,FearAt(b,q,Mathf.InverseLerp(4,12,Vector3.Distance(p,q))));
        }
        static bool Finite(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z);
        FearInput FearAt(int actor,Vector3 head,float isolation)
        {
            bool house=head.z<HouseFloodState.HouseNorthZ&&head.z>HouseFloodState.HouseSouthZ;
            float level=house?(HouseFlood!=null&&HouseFlood.WetAt(head)?HouseFlood.Height:0):LatestWater;
            bool threat=FearThreat?.Active==true;
            float floor=house?0:Mathf.Max(0,Mathf.Floor((head.y-.4f)/(Connected?Connected.floorHeight:3.3f)))*(Connected?Connected.floorHeight:3.3f);
            return new FearInput{
                dark=!HasPower(PowerCircuit.Lights),water=Mathf.Clamp01((level-floor)/.8f),shipDanger=shipFear,
                isolation=Complete?0:isolation,threatPresent=threat,threatDistance=threat?Vector3.Distance(head,FearThreat.Position):0,
                threatVisible=threat&&FearThreat.VisibleTo(actor,head),hidden=threat&&FearThreat.Hidden(actor),holdingBreath=threat&&FearThreat.HoldingBreath(actor)
            };
        }
    }
}
