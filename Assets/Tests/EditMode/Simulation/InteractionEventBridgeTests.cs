using System;
using LighthouseKeepers.Core.Simulation;
using LighthouseKeepers.Simulation.Unity;
using NUnit.Framework;

namespace LighthouseKeepers.Tests.Simulation
{
    public sealed class InteractionEventBridgeTests
    {
        public static class FakeEvents
        {
            public static event Action<int> OnPumpStrokeCompleted;
            public static event Action<int> OnLeakRepairCompleted;
            public static event Action<int> OnGeneratorStartCompleted;
            public static void Raise()
            { OnPumpStrokeCompleted?.Invoke(7); OnLeakRepairCompleted?.Invoke(8); OnGeneratorStartCompleted?.Invoke(9); }
        }
        sealed class RecordingHost : ISimulationHost
        {
            public SimSnapshot Current => default;
            public readonly System.Collections.Generic.List<SimCommand> Commands = new System.Collections.Generic.List<SimCommand>();
            public void Enqueue(SimCommand command) => Commands.Add(command);
            public void ForceGeneratorFault() { }
            public event Action<SimSnapshot> OnTick { add { } remove { } }
        }

        [Test] public void SharedEventsEnqueueCommandsWithActorIdsAndDisposeUnsubscribes()
        {
            var host = new RecordingHost(); var bridge = new InteractionEventBridge(host);
            try
            {
                Assert.That(bridge.Bind(typeof(FakeEvents)), Is.True);
                Assert.That(bridge.Bind(typeof(FakeEvents)), Is.True, "Rebind must not double-subscribe.");
                FakeEvents.Raise();
                Assert.That(host.Commands.Count, Is.EqualTo(2));
                Assert.That(host.Commands[0].Kind, Is.EqualTo(SimCommandKind.ManualPumpStroke));
                Assert.That(host.Commands[0].ActorId, Is.EqualTo(7));
                Assert.That(host.Commands[1].Kind, Is.EqualTo(SimCommandKind.RepairLeak));
                Assert.That(host.Commands[1].ActorId, Is.EqualTo(8));
                Assert.That(host.Commands.Exists(c => c.Kind == SimCommandKind.StartGenerator), Is.False,
                    "D validates generator setup before sending StartGenerator; the raw event must not bypass it.");
            }
            finally { bridge.Dispose(); }
            FakeEvents.Raise(); Assert.That(host.Commands.Count, Is.EqualTo(2));
        }
    }
}
