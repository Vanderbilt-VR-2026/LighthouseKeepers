using System;
using System.Collections.Generic;
using System.Reflection;
using LighthouseKeepers.Core.Simulation;

namespace LighthouseKeepers.Simulation.Unity
{
    /// <summary>Optional adapter for Session A, which is absent on the Sprint 3 base.
    /// Exact shared event names only; no alternate interaction implementation.</summary>
    public sealed class InteractionEventBridge : IDisposable
    {
        readonly ISimulationHost host;
        readonly List<KeyValuePair<EventInfo, Action<int>>> subscriptions =
            new List<KeyValuePair<EventInfo, Action<int>>>();

        public InteractionEventBridge(ISimulationHost host)
        { this.host = host ?? throw new ArgumentNullException(nameof(host)); }

        public bool Bind(Type interactionEvents)
        {
            Dispose();
            if (interactionEvents == null) return false;
            string[] names = { "OnPumpStrokeCompleted", "OnLeakRepairCompleted", "OnGeneratorStartCompleted" };
            SimCommandKind[] kinds = { SimCommandKind.ManualPumpStroke, SimCommandKind.RepairLeak, SimCommandKind.StartGenerator };
            for (int i = 0; i < names.Length; i++)
            {
                var ev = interactionEvents.GetEvent(names[i], BindingFlags.Public | BindingFlags.Static);
                if (ev == null || ev.EventHandlerType != typeof(Action<int>)) { Dispose(); return false; }
                SimCommandKind kind = kinds[i];
                Action<int> handler = actor => host.Enqueue(new SimCommand(kind, actorId: actor));
                ev.AddEventHandler(null, handler);
                subscriptions.Add(new KeyValuePair<EventInfo, Action<int>>(ev, handler));
            }
            return true;
        }

        public bool BindAvailableAssembly()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType("LighthouseKeepers.Gameplay.Interaction.InteractionEvents", false);
                if (type != null) return Bind(type);
            }
            return false;
        }

        public void Dispose()
        {
            foreach (var subscription in subscriptions)
                subscription.Key.RemoveEventHandler(null, subscription.Value);
            subscriptions.Clear();
        }
    }
}
