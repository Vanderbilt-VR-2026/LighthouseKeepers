using System;
using System.Reflection;
using UnityEngine;

namespace LighthouseKeepers.Core.Progression
{
    // Temporary merge boundary for B's absent assembly. Only reads Current and calls public commands.
    // Cached reflection, polled at 10 Hz, avoids runtime generic code generation on IL2CPP.
    public sealed class SharedSimulationPort
    {
        readonly object host;
        readonly PropertyInfo current;
        readonly MethodInfo enqueue, fault, leak;
        readonly ConstructorInfo commandConstructor;
        readonly object resetKind, beginKind, startKind, runningPhase, failedPhase;
        readonly FieldInfo tick, height, generator, phase;
        public float SafeHeightM { get; }

        public SharedSimulationPort(MonoBehaviour simulationHost, UnityEngine.Object floodTuning)
        {
            if (!simulationHost || !floodTuning) throw new ArgumentException("Assign B's simulation host and FloodTuning asset.");
            host = simulationHost;
            Type contract = null;
            foreach (var type in host.GetType().GetInterfaces())
                if (type.FullName == "LighthouseKeepers.Core.Simulation.ISimulationHost") contract = type;
            if (contract == null) throw new ArgumentException("Host must implement the shared ISimulationHost.");
            current = contract.GetProperty("Current") ?? throw new MissingMemberException("ISimulationHost.Current");
            enqueue = contract.GetMethod("Enqueue") ?? throw new MissingMemberException("ISimulationHost.Enqueue");
            var snapshot = current.PropertyType;
            tick = RequireField(snapshot, "Tick"); height = RequireField(snapshot, "WaterHeightM");
            generator = RequireField(snapshot, "GeneratorRunning"); phase = RequireField(snapshot, "Phase");
            runningPhase = Enum.Parse(phase.FieldType, "Running"); failedPhase = Enum.Parse(phase.FieldType, "Failed");
            var parameters = enqueue.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType.FullName != "LighthouseKeepers.Core.Simulation.SimCommand")
                throw new ArgumentException("Enqueue must accept the shared SimCommand.");
            var command = parameters[0].ParameterType;
            var kind = RequireField(command, "Kind").FieldType;
            commandConstructor = command.GetConstructor(new[] { kind, typeof(int), typeof(int), typeof(float) });
            if (commandConstructor == null) throw new MissingMethodException("SimCommand needs (SimCommandKind, int, int, float). See D-to-B.md.");
            resetKind = Enum.Parse(kind, "ResetRun"); beginKind = Enum.Parse(kind, "BeginDay"); startKind = Enum.Parse(kind, "StartGenerator");
            fault = host.GetType().GetMethod("ForceGeneratorFault", Type.EmptyTypes);
            leak = host.GetType().GetMethod("ActivateDayLeak", new[] { typeof(int) });
            var tuningType = floodTuning.GetType();
            var tuningField = tuningType.GetField("safeHeightM", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var tuningProperty = tuningType.GetProperty("safeHeightM", BindingFlags.Public | BindingFlags.Instance);
            var safe = tuningField != null ? tuningField.GetValue(floodTuning) : tuningProperty?.GetValue(floodTuning);
            if (!(safe is float value) || float.IsNaN(value) || float.IsInfinity(value) || value < 0)
                throw new ArgumentException("FloodTuning must expose float safeHeightM. No fallback is used.");
            SafeHeightM = value;
        }

        static FieldInfo RequireField(Type type, string name) => type.GetField(name) ?? throw new MissingFieldException(type.FullName, name);

        public void Validate(DayPlan plan)
        {
            foreach (var incident in plan.Incidents)
            {
                if (incident.kind == DayIncidentKind.ActivateLeak && leak == null)
                    throw new MissingMethodException("B must supply ActivateDayLeak(int). See D-to-B.md.");
                if (incident.kind == DayIncidentKind.GeneratorFault && fault == null)
                    throw new MissingMethodException("B must supply ForceGeneratorFault(). See D-to-B.md.");
            }
        }

        public DayObservation Read(out bool running)
        {
            var snapshot = current.GetValue(host);
            var state = phase.GetValue(snapshot);
            running = state.Equals(runningPhase);
            return new DayObservation((int)tick.GetValue(snapshot), (float)height.GetValue(snapshot),
                (bool)generator.GetValue(snapshot), state.Equals(failedPhase));
        }

        public void Send(DayRequest request, int actorId, int day)
        {
            var kind = request == DayRequest.ResetRun ? resetKind : request == DayRequest.BeginDay ? beginKind : startKind;
            var command = commandConstructor.Invoke(new object[] { kind, actorId, request == DayRequest.BeginDay ? day : 0, 0f });
            enqueue.Invoke(host, new[] { command });
        }

        public bool Fire(IncidentDefinition incident, int day)
        {
            if (incident.kind == DayIncidentKind.GeneratorFault) fault.Invoke(host, null);
            else leak.Invoke(host, new object[] { day });
            return true;
        }
    }
}
