using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        public PressureValveState Pressure {get;private set;}
        public int PipeWorkerActor {get;set;}
        public float LeakStrength=>Pipe.InflowFraction*(Pressure?.Reduced==true?Connected.valveLeakFraction:1);
        public bool PipeReady=>Pressure==null||Pressure.Allows(PipeWorkerActor);
        void InitializePressure()
        {
            if(!Connected||!Connected.pressureValve)return;
            Pressure=new PressureValveState(Connected.holdTimeoutSeconds);
            Pressure.Changed+=()=>Changed?.Invoke();Pipe.CanTighten=()=>PipeReady;
        }
    }
}
