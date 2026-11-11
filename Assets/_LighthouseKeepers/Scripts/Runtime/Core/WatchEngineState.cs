using System;
using UnityEngine;
namespace LighthouseKeepers.Core
{
    // Pure host-owned sequence. The accepted starter gesture reports one complete stroke.
    public sealed class WatchEngineState
    {
        readonly S4M2Tuning tuning;
        float primerWait, crankWait, drain, notifyAfter;
        public event Action Changed;
        public bool ReadOnly {get;set;}
        public bool IgnoreExtraPulls {get;set;}
        public bool Guided {get;set;}
        public float Fuel {get;private set;}
        public int Primer {get;private set;}
        public int Pulls {get;private set;}
        public int ClosedPulls {get;private set;}
        public bool ChokeClosed {get;private set;}
        public bool Running {get;private set;}
        public bool Settled {get;private set;}
        public float OpenRemaining {get;private set;}
        public float FloodRemaining {get;private set;}
        public int CoughSerial {get;private set;}
        public bool EnoughFuel=>Fuel>=tuning.minimumFuel;
        public bool LowFuel=>Fuel<tuning.lowFuelWarning;
        public bool Primed=>Primer>=tuning.primerSqueezes;
        public WatchEngineState(S4M2Tuning value){tuning=value;}
        public void SetConsumption(float value){if(!ReadOnly)drain=Mathf.Max(0,value);}
        public void Reset(float fuel,float consumption,bool running)
        {Fuel=Mathf.Clamp01(fuel);drain=Mathf.Max(0,consumption);Primer=Pulls=ClosedPulls=CoughSerial=0;ChokeClosed=false;Running=Settled=running;OpenRemaining=FloodRemaining=primerWait=crankWait=0;Changed?.Invoke();}
        public bool Prime()
        {if(ReadOnly||Running||FloodRemaining>0||primerWait>0||Guided&&Primed)return false;primerWait=tuning.primerCooldown;Primer=Math.Min(tuning.primerSqueezes,Primer+1);Changed?.Invoke();return true;}
        public bool SetChoke(bool closed)
        {
            if(ReadOnly||ChokeClosed==closed)return false;
            ChokeClosed=closed;
            if(!closed)ClosedPulls=0;
            if(Running&&!closed){Settled=true;OpenRemaining=0;}
            else if(Running&&closed){Settled=false;OpenRemaining=tuning.chokeOpenSeconds;}
            Changed?.Invoke();return true;
        }
        public bool Pull()
        {
            if(IgnoreExtraPulls && Running)return false;
            if(ReadOnly||FloodRemaining>0||crankWait>0||Running&&!ChokeClosed)return false;
            crankWait=tuning.crankCooldown;
            if(!IgnoreExtraPulls&&ChokeClosed&&++ClosedPulls>=tuning.floodAtPulls){Stop();FloodRemaining=tuning.engineFloodSeconds;CoughSerial++;Changed?.Invoke();return true;}
            if(!EnoughFuel||!Primed||!ChokeClosed){CoughSerial++;Changed?.Invoke();return true;}
            Pulls++;
            if(!Running&&Pulls>=tuning.starterPulls){Running=true;OpenRemaining=tuning.chokeOpenSeconds;}
            Changed?.Invoke();return true;
        }
        public void Pour(float seconds)
        {if(ReadOnly||!float.IsFinite(seconds)||seconds<=0||Fuel>=1)return;Fuel=Mathf.Min(1,Fuel+tuning.fuelPourPerSecond*Mathf.Min(seconds,.1f));}
        void Stop(){Running=Settled=false;OpenRemaining=0;Primer=Pulls=ClosedPulls=0;}
        public void Tick(float seconds)
        {
            if(ReadOnly||!float.IsFinite(seconds)||seconds<=0)return;
            primerWait=Mathf.Max(0,primerWait-seconds);crankWait=Mathf.Max(0,crankWait-seconds);
            bool wasRunning=Running;notifyAfter-=seconds;
            if(FloodRemaining>0){FloodRemaining=Mathf.Max(0,FloodRemaining-seconds);}
            if(Running)
            {
                if(drain>0){Fuel=Mathf.Max(0,Fuel-drain*seconds);}
                if(ChokeClosed&&!Guided){OpenRemaining=Mathf.Max(0,OpenRemaining-seconds);if(OpenRemaining<=0){Stop();CoughSerial++;}}
                if(Fuel<=.000001f){Fuel=0;Stop();CoughSerial++;}
            }
            if(wasRunning!=Running||notifyAfter<=0){notifyAfter=.2f;Changed?.Invoke();}
        }
        public void ApplyReplica(float fuel,int primer,int pulls,bool closed,bool running,bool settled,float open,float flooded,int cough,float consumption=0,int closedPulls=0)
        {if(!ReadOnly)return;drain=consumption;ClosedPulls=closedPulls;Fuel=fuel;Primer=primer;Pulls=pulls;ChokeClosed=closed;Running=running;Settled=settled;OpenRemaining=open;FloodRemaining=flooded;CoughSerial=cough;}
    }
}
