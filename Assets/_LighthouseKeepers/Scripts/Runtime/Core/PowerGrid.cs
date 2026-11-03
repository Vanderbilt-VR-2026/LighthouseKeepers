using System;

namespace LighthouseKeepers.Core
{
    public enum PowerCircuit { Pump, Beacon, Radio, Lights }

    // The engine and its distribution breaker are separate. Only the watch authority
    // changes this model; a replica accepts complete, validated host state.
    public sealed class PowerGrid
    {
        readonly float[] costs;
        public event Action Changed;
        public bool ReadOnly { get; set; }
        public bool EngineRunning { get; private set; }
        public bool Tripped { get; private set; }
        public float Capacity { get; private set; }
        public int SelectedMask { get; private set; }
        public float Demand
        {
            get { float total=0; for(int i=0;i<4;i++) if((SelectedMask & (1<<i))!=0) total+=costs[i]; return total; }
        }
        public PowerGrid(float pump, float beacon, float radio, float lights)
        {
            costs=new[]{pump,beacon,radio,lights};
            foreach(float cost in costs) if(!float.IsFinite(cost)||cost<0) throw new ArgumentOutOfRangeException(nameof(cost));
        }
        public bool Selected(PowerCircuit circuit) => ValidCircuit(circuit) && (SelectedMask & (1<<(int)circuit))!=0;
        public bool Powered(PowerCircuit circuit) => EngineRunning && !Tripped && Selected(circuit);
        public bool SetCircuit(PowerCircuit circuit,bool selected)
        {
            if(ReadOnly || !ValidCircuit(circuit) || Selected(circuit)==selected) return false;
            if(selected) SelectedMask|=1<<(int)circuit; else SelectedMask&=~(1<<(int)circuit);
            CheckOverload(); Changed?.Invoke(); return true;
        }
        public bool ResetBreaker()
        {
            if(ReadOnly || !Tripped || Demand>Capacity+.0001f) return false;
            Tripped=false; Changed?.Invoke(); return true;
        }
        public void SetEngine(bool running)
        {
            if(ReadOnly || running==EngineRunning) return;
            EngineRunning=running; CheckOverload(); Changed?.Invoke();
        }
        public void Reset(float capacity,int selectedMask,bool running)
        {
            if(ReadOnly) return;
            if(!ValidState(capacity,selectedMask)) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity=capacity;SelectedMask=selectedMask;EngineRunning=running;Tripped=false;
            CheckOverload();Changed?.Invoke();
        }
        public bool ApplyReplica(float capacity,int selectedMask,bool running,bool tripped)
        {
            if(!ReadOnly || !ValidState(capacity,selectedMask)) return false;
            float demand=0;for(int i=0;i<4;i++)if((selectedMask&(1<<i))!=0)demand+=costs[i];
            if(running && !tripped && demand>capacity+.0001f)return false;
            Capacity=capacity;SelectedMask=selectedMask;EngineRunning=running;Tripped=tripped;Changed?.Invoke();return true;
        }
        static bool ValidCircuit(PowerCircuit circuit)=>circuit>=PowerCircuit.Pump&&circuit<=PowerCircuit.Lights;
        public static bool ValidState(float capacity,int mask)=>float.IsFinite(capacity)&&capacity>0&&capacity<=100&&mask>=0&&mask<=15;
        void CheckOverload(){if(EngineRunning&&Demand>Capacity+.0001f)Tripped=true;}
    }
}
