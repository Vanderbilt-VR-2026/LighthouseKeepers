using System;
using System.Collections.Generic;
using System.IO;
using LighthouseKeepers.Core.Simulation;
using LighthouseKeepers.Gameplay.Interaction;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    /// <summary>One B host advances on the authority. Clients receive its immutable snapshots.</summary>
    public sealed class FusionSimulationAdapter : MonoBehaviour, ISimulationHost, IAuthority
    {
        SimulationHost host;
        FusionMessageChannel channel;
        SimSnapshot replica, previous;
        bool solo, configured, haveSnapshot;
        int sequence, receivedSequence;
        double receivedAt, window;
        readonly Dictionary<int,int> requests=new();
        const int Command=30, Snapshot=31;
        public bool IsAuthoritative => configured && (solo || channel && channel.IsHost);
        public SimSnapshot Current => host!=null?host.Current:replica;
        public event Action<SimSnapshot> OnTick;
        public Func<SimCommand,bool> ValidateInteraction {get;set;}
        public float PresentationWaterHeight => IsAuthoritative?Current.WaterHeightM:
            Mathf.Lerp(previous.WaterHeightM,replica.WaterHeightM,(float)((Time.unscaledTimeAsDouble-receivedAt)/.1));
        public void Configure(bool offline,FloodParameters profile,ISimulationGeometry geometry,Func<int,bool> repairGate)
        {
            if(configured)throw new InvalidOperationException("Configure simulation once per ship session.");
            solo=offline;channel=offline?null:FusionMessageChannel.Active;
            if(!solo && (!channel || !channel.Connected))throw new InvalidOperationException("The crew runner must establish authority before gameplay starts.");
            configured=true;
            if(IsAuthoritative){host=new SimulationHost(profile,geometry,this,canRepairPipe:repairGate);host.OnTick+=Publish;}
            else if(channel){channel.Received+=Receive;}
            if(channel && IsAuthoritative)channel.Received+=Receive;
            InteractionEvents.OnPumpStrokeCompleted+=Pump;
            InteractionEvents.OnLeakRepairCompleted+=Repair;
        }
        void Pump(int actor)=>Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke,actor));
        void Repair(int actor)=>Enqueue(new SimCommand(SimCommandKind.RepairLeak,actor));
        void Update(){if(host!=null && IsAuthoritative)host.Advance(Time.deltaTime);}
        public void ForceGeneratorFault()=>Enqueue(new SimCommand(SimCommandKind.ForceGeneratorFault));
        public void ActivateDayLeak(int dayIndex)=>Enqueue(new SimCommand(SimCommandKind.ActivateDayLeak,targetId:dayIndex));
        public void Enqueue(SimCommand command)
        {
            if(!configured)throw new InvalidOperationException("Ship simulation is not configured.");
            if(IsAuthoritative){host.Enqueue(command);return;}
            // Lifecycle and debug operations can never originate on a remote client.
            if(!channel || !channel.Connected || SimCommandPolicy.IsHostOnly(command.Kind))return;
            using var s=new MemoryStream(12);using var w=new BinaryWriter(s);
            w.Write((int)command.Kind);w.Write(command.TargetId);w.Write(command.Value);
            channel.SendToHost(Command,s.ToArray());
        }
        void Receive(int actor,int kind,byte[] bytes)
        {
            if(kind==Snapshot && !IsAuthoritative && actor==0)
            {
                if(!SimulationSnapshotWire.TryRead(bytes,out int serial,out var state) || serial<=receivedSequence ||
                   haveSnapshot && state.Tick<replica.Tick)return;
                previous=haveSnapshot?replica:state;replica=state;haveSnapshot=true;
                receivedSequence=serial;receivedAt=Time.unscaledTimeAsDouble;OnTick?.Invoke(state);
            }
            else if(kind==Command && IsAuthoritative && bytes.Length==12 && channel.IsActorConnected(actor))
            {
                if(Time.unscaledTimeAsDouble-window>=1){window=Time.unscaledTimeAsDouble;requests.Clear();}
                requests.TryGetValue(actor,out int count);if(count>=20)return;requests[actor]=count+1;
                using var r=new BinaryReader(new MemoryStream(bytes,false));
                var k=(SimCommandKind)r.ReadInt32();int target=r.ReadInt32();float value=r.ReadSingle();
                if(!Enum.IsDefined(typeof(SimCommandKind),k) || SimCommandPolicy.IsHostOnly(k) || target<0 || target>65535 || !KeeperPoseFrame.Finite(value))return;
                if(k==SimCommandKind.SetElectricPump && value!=0 && value!=1)return;
                var cmd=new SimCommand(k,actor,target,value); // Sender is the sole identity source.
                if(ValidateInteraction!=null && ValidateInteraction(cmd))host.Enqueue(cmd);
            }
        }
        void Publish(SimSnapshot state)
        {
            OnTick?.Invoke(state);
            if(channel && channel.IsHost)channel.Broadcast(Snapshot,SimulationSnapshotWire.Write(++sequence,state));
        }
        void OnDestroy()
        {
            if(host!=null)host.OnTick-=Publish;
            if(channel)channel.Received-=Receive;
            InteractionEvents.OnPumpStrokeCompleted-=Pump;InteractionEvents.OnLeakRepairCompleted-=Repair;
        }
    }
    public static class SimulationSnapshotWire
    {
        const int Version=2;
        public static byte[] Write(int sequence,SimSnapshot v)
        {
            using var s=new MemoryStream();using var w=new BinaryWriter(s);
            w.Write(Version);w.Write(sequence);w.Write(v.Tick);w.Write(v.WaterVolumeM3);w.Write(v.WaterHeightM);
            w.Write(v.InflowM3PerSec);w.Write(v.OutflowM3PerSec);w.Write(v.LeakActive);w.Write(v.GeneratorRunning);
            w.Write(v.PowerAvailable);w.Write(v.ElectricPumpRunning);w.Write((int)v.Phase);
            w.Write(v.LastRejectReason??"");w.Write(v.GeneratorFlooded);w.Write(v.GeneratorGeometryAvailable);
            w.Write(v.FuelRemainingSeconds);w.Write(v.SecondsAboveFail);w.Write(v.TimeToFailSeconds);
            w.Write(v.Paused);w.Write(v.DayIndex);w.Write(v.GeneratorFaultActive);return s.ToArray();
        }
        public static bool TryRead(byte[] bytes,out int sequence,out SimSnapshot v)
        {
            sequence=0;v=default;if(bytes==null || bytes.Length<60 || bytes.Length>2048)return false;
            try
            {
                using var s=new MemoryStream(bytes,false);using var r=new BinaryReader(s);
                if(r.ReadInt32()!=Version)return false;
                sequence=r.ReadInt32();int tick=r.ReadInt32();double volume=r.ReadDouble();float height=r.ReadSingle();
                float inflow=r.ReadSingle(),outflow=r.ReadSingle();bool leak=r.ReadBoolean(),generator=r.ReadBoolean();
                float power=r.ReadSingle();bool pump=r.ReadBoolean();var phase=(RunPhase)r.ReadInt32();string reason=r.ReadString();
                bool flooded=r.ReadBoolean(),geometry=r.ReadBoolean();float fuel=r.ReadSingle(),sustain=r.ReadSingle(),eta=r.ReadSingle();
                bool paused=r.ReadBoolean();int day=r.ReadInt32();bool fault=r.ReadBoolean();
                if(s.Position!=s.Length || sequence<=0 || tick<0 || double.IsNaN(volume) || double.IsInfinity(volume) || volume<0 ||
                   !KeeperPoseFrame.Finite(height) || !Nonnegative(inflow) || !Nonnegative(outflow) || !Nonnegative(fuel) || !Nonnegative(sustain) ||
                   !KeeperPoseFrame.Finite(power) || power<0 || power>1 || float.IsNaN(eta) || eta<0 || day<0 || day>7 || !Enum.IsDefined(typeof(RunPhase),phase))return false;
                v=new SimSnapshot(tick,volume,height,inflow,outflow,leak,generator,power,pump,phase,reason,flooded,geometry,fuel,sustain,eta,paused,day,fault);return true;
            }
            catch(IOException){return false;}catch(ArgumentException){return false;}
        }
        static bool Nonnegative(float v)=>KeeperPoseFrame.Finite(v)&&v>=0;
    }
}
