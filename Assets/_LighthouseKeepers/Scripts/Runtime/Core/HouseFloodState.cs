using System;
using UnityEngine;
namespace LighthouseKeepers.Core
{
    // A shallow receiving basin fed through the authored zero-threshold doorway.
    // Tower level is the existing simulation's prescribed boundary, not a second flood authority.
    public sealed class HouseFloodState
    {
        public const float DoorWidth=1.1f, DoorNorthZ=-3.76f, HouseNorthZ=-5.55f, HouseSouthZ=-9.78f, HouseWidth=6.8f;
        public const float DoorLength=DoorNorthZ-HouseNorthZ, HouseLength=HouseNorthZ-HouseSouthZ;
        public const float Area=DoorWidth*DoorLength+HouseWidth*HouseLength;
        readonly S4Tuning tuning;
        public float Volume {get;private set;}
        public float Flux {get;private set;}
        public float Film=>Mathf.Max(.001f,tuning.houseWaterFilmDepth);
        public float DoorFront=>Mathf.Min(DoorLength,Volume/(Film*DoorWidth));
        public float RoomFront=>Mathf.Clamp((Volume-Film*DoorWidth*DoorLength)/(Film*HouseWidth),0,HouseLength);
        public float RoomNorthHalfWidth=>Mathf.Lerp(DoorWidth,HouseWidth,Mathf.Clamp01((RoomFront-.25f)/1.5f))*.5f;
        public float RoomSouthHalfWidth=>Mathf.Lerp(DoorWidth,HouseWidth,Mathf.Clamp01(RoomFront/1.5f))*.5f;
        public float Height=>Volume<=0?0:Mathf.Max(.004f,Volume>=Area*Film?Volume/Area:Film);
        public HouseFloodState(S4Tuning value){tuning=value;}
        public void Reset(){Volume=Flux=0;}
        public void Apply(float volume){Volume=volume;Flux=0;}
        public void Step(float seconds,float towerHeight,bool pumping)
        {
            if(!float.IsFinite(seconds)||seconds<0||!float.IsFinite(towerHeight))throw new ArgumentOutOfRangeException();
            if(seconds==0)return;
            float target=Mathf.Max(0,towerHeight)*Area;
            float head=towerHeight-Volume/Area;
            float flow=tuning.houseDoorFlowCoefficient*DoorWidth*Mathf.Pow(Mathf.Abs(head),1.5f);
            // Once the connected floor is below the pump intake, remove the remaining film too.
            if(pumping&&towerHeight<Film)flow=Mathf.Max(flow,tuning.drainMPerSecond*Area);
            float next=Mathf.MoveTowards(Volume,target,flow*seconds);
            Flux=(next-Volume)/seconds;Volume=next;
        }
        public bool WetAt(Vector3 p)
        {
            if(Volume<=.00001f)return false;
            if(Mathf.Abs(p.x)<=DoorWidth*.5f&&p.z<=DoorNorthZ&&p.z>=DoorNorthZ-DoorFront)return true;
            return RoomFront>0&&p.z<=HouseNorthZ&&p.z>=HouseNorthZ-RoomFront&&Mathf.Abs(p.x)<=Mathf.Lerp(RoomNorthHalfWidth,RoomSouthHalfWidth,(HouseNorthZ-p.z)/RoomFront);
        }
    }
}
