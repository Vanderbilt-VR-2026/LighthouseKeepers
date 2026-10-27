// Enable LK_SPRINT3_SIMULATION only after merging B and adding B's assembly to LK.Net.asmdef.
// This source deliberately does not create a substitute simulation in E's checkout.
#if LK_SPRINT3_SIMULATION
using System;
using System.Collections.Generic;
using System.IO;
using LighthouseKeepers.Core.Simulation;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    /// <summary>The sole ISimulationHost facade for gameplay on either peer. Only the host backend advances.</summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class FusionSimulationAdapter : MonoBehaviour, ISimulationHost
    {
        [SerializeField] MonoBehaviour simulationBackend;
        ISimulationHost backend;
        FusionMessageChannel channel;
        SimSnapshot current, previous;
        bool haveSnapshot;
        int serial, receivedSerial;
        double receivedAt;
        const int CommandKind = 30, SnapshotKind = 31;
        readonly Dictionary<int,int> commandsThisSecond = new();
        double window;
        // Composition must validate station identity, proximity, held-object lease and action completion.
        // Missing validation fails closed; do not accept a remote completion merely because it has the right enum.
        public Func<SimCommand,bool> ValidateInteraction { private get; set; }
        public SimSnapshot Current => current;
        public event Action<SimSnapshot> OnTick;
        public float PresentationWaterHeight => channel && channel.IsHost ? current.WaterHeightM :
            Mathf.Lerp(previous.WaterHeightM,current.WaterHeightM,(float)((Time.unscaledTimeAsDouble-receivedAt)/.1));
        void Awake()
        {
            backend = simulationBackend as ISimulationHost;
            if (simulationBackend) simulationBackend.enabled = false;
            if (backend == null) { Debug.LogError("Assign B's ISimulationHost backend before enabling network simulation.",this); enabled=false; }
        }
        void Update()
        {
            if (channel == FusionMessageChannel.Active) return;
            Unbind(); channel=FusionMessageChannel.Active;
            if (!channel) return;
            channel.Received+=Receive;channel.SessionChanged+=AuthorityChanged;
            backend.OnTick+=Publish;AuthorityChanged();
        }
        void AuthorityChanged()
        {
            simulationBackend.enabled=channel && channel.IsHost;
            if (!channel || !channel.Connected)
            { haveSnapshot=false;serial=receivedSerial=0;commandsThisSecond.Clear(); }
        }
        public void Enqueue(SimCommand command)
        {
            if (!channel || !channel.Connected) return;
            using var stream=new MemoryStream(16);using var writer=new BinaryWriter(stream);
            writer.Write((int)command.Kind);writer.Write(command.TargetId);writer.Write(command.Value);
            // No ActorId on the command wire: Fusion authenticates it from the sending connection.
            channel.SendToHost(CommandKind,stream.ToArray());
        }
        bool Accept(int actor,SimCommandKind kind,int target,float value)
        {
            if (!channel.IsHost || !channel.IsActorConnected(actor) || !KeeperPoseFrame.Finite(value) || target<0 || target>65535 ||
                !Enum.IsDefined(typeof(SimCommandKind),kind)) return false;
            if (kind==SimCommandKind.BeginDay || kind==SimCommandKind.ResetRun)
                return actor==channel.LocalActorId;
            if (kind==SimCommandKind.SetElectricPump && value!=0 && value!=1) return false;
            var command=new SimCommand(kind,actor,target,value);
            return ValidateInteraction!=null && ValidateInteraction(command);
        }
        void Receive(int actor,int kind,byte[] bytes)
        {
            if (kind==CommandKind && channel.IsHost && bytes.Length==12)
            {
                if (Time.unscaledTimeAsDouble-window>=1) { window=Time.unscaledTimeAsDouble;commandsThisSecond.Clear(); }
                commandsThisSecond.TryGetValue(actor,out int count);
                if (count>=20) return;commandsThisSecond[actor]=count+1;
                using var reader=new BinaryReader(new MemoryStream(bytes,false));
                var commandKind=(SimCommandKind)reader.ReadInt32();int target=reader.ReadInt32();float value=reader.ReadSingle();
                if (Accept(actor,commandKind,target,value)) backend.Enqueue(new SimCommand(commandKind,actor,target,value));
            }
            else if (kind==SnapshotKind && actor==0 && !channel.IsHost && bytes.Length==40)
            {
                using var r=new BinaryReader(new MemoryStream(bytes,false));
                int next=r.ReadInt32();int tick=r.ReadInt32();double volume=r.ReadDouble();float height=r.ReadSingle();
                float inflow=r.ReadSingle(),outflow=r.ReadSingle();bool leak=r.ReadBoolean(),generator=r.ReadBoolean();
                float power=r.ReadSingle();bool pump=r.ReadBoolean();var phase=(RunPhase)r.ReadInt32();byte version=r.ReadByte();
                if (version!=1 || next<=receivedSerial || tick<0 || double.IsNaN(volume)||double.IsInfinity(volume)||volume<0 ||
                    !KeeperPoseFrame.Finite(height)||!KeeperPoseFrame.Finite(inflow)||!KeeperPoseFrame.Finite(outflow)||
                    !KeeperPoseFrame.Finite(power)||inflow<0||outflow<0||power<0||power>1||!Enum.IsDefined(typeof(RunPhase),phase)) return;
                receivedSerial=next;
                var state=new SimSnapshot(tick,volume,height,inflow,outflow,leak,generator,power,pump,phase);
                previous=haveSnapshot?current:state;current=state;haveSnapshot=true;receivedAt=Time.unscaledTimeAsDouble;OnTick?.Invoke(current);
            }
        }
        void Publish(SimSnapshot state)
        {
            if (!channel || !channel.IsHost) return;
            previous=current;current=state;OnTick?.Invoke(state);
            using var stream=new MemoryStream(40);using var w=new BinaryWriter(stream);
            w.Write(++serial);w.Write(state.Tick);w.Write(state.WaterVolumeM3);w.Write(state.WaterHeightM);
            w.Write(state.InflowM3PerSec);w.Write(state.OutflowM3PerSec);w.Write(state.LeakActive);w.Write(state.GeneratorRunning);
            w.Write(state.PowerAvailable);w.Write(state.ElectricPumpRunning);w.Write((int)state.Phase);w.Write((byte)1);
            channel.Broadcast(SnapshotKind,stream.ToArray());
        }
        void Unbind()
        {
            if (simulationBackend) simulationBackend.enabled=false;
            if (backend!=null) backend.OnTick-=Publish;
            if (channel) { channel.Received-=Receive;channel.SessionChanged-=AuthorityChanged; }
            channel=null;
        }
        void OnDestroy() => Unbind();
    }
}
#endif
