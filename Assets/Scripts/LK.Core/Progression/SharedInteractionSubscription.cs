using System;
using System.Collections.Generic;
using System.Reflection;

namespace LighthouseKeepers.Core.Progression
{
    // A owns the event type. D binds it once, never declares a competing InteractionEvents class.
    public sealed class SharedInteractionSubscription : IDisposable
    {
        readonly List<EventInfo> events = new List<EventInfo>();
        readonly List<Action<int>> callbacks = new List<Action<int>>();
        public SharedInteractionSubscription(Action<int> pump, Action<int> repair, Action<int> start)
        {
            Type type = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType("LighthouseKeepers.Gameplay.Interaction.InteractionEvents", false);
                if (type != null) break;
            }
            if (type == null) throw new TypeLoadException("A's shared InteractionEvents is missing. See D-to-A.md.");
            // Validate all three before adding any delegate, so a partial bind cannot leak subscriptions.
            var names = new[] { "OnPumpStrokeCompleted", "OnLeakRepairCompleted", "OnGeneratorStartCompleted" };
            foreach (var name in names)
            {
                var info = type.GetEvent(name, BindingFlags.Public | BindingFlags.Static);
                if (info == null || info.EventHandlerType != typeof(Action<int>)) throw new MissingMemberException(type.FullName, name);
                events.Add(info);
            }
            callbacks.Add(pump); callbacks.Add(repair); callbacks.Add(start);
            int bound = 0;
            try { for (; bound < events.Count; bound++) events[bound].AddEventHandler(null, callbacks[bound]); }
            catch { for (int i = 0; i < bound; i++) events[i].RemoveEventHandler(null, callbacks[i]); throw; }
        }
        public void Dispose()
        {
            for (int i = 0; i < events.Count; i++) events[i].RemoveEventHandler(null, callbacks[i]);
            events.Clear(); callbacks.Clear();
        }
    }
}
