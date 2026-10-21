#if LK_SIMULATION_STANDALONE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using LighthouseKeepers.Core.Simulation;
using NUnit.Framework;

namespace LighthouseKeepers.Tests.Simulation
{
    /// <summary>Runs the actual NUnit test methods outside Unity when Editor IPC is unavailable.
    /// AssetDatabase setup is replaced with reading the same serialized tuning numbers.
    /// This is arithmetic/managed-code evidence, never Editor or player evidence.</summary>
    public static class StandaloneRunner
    {
        public static int Main(string[] args)
        {
            var values = new Dictionary<string, double>();
            foreach (string line in File.ReadAllLines("Assets/Data/Tuning/FloodTuning.asset"))
            {
                string[] pair = line.Trim().Split(':');
                if (pair.Length == 2 && double.TryParse(pair[1].Trim(), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double value))
                    values[pair[0]] = (double)(float)value; // Match ScriptableObject float serialization.
            }
            var tuning = new FloodParameters(values["leakInflowM3PerSec"], values["manualPumpStrokeM3"],
                values["manualPumpMinStrokeSeconds"], values["electricPumpM3PerSec"], values["floorAreaM2"],
                values["failHeightM"], values["safeHeightM"], values["failSustainSeconds"],
                values["electricPumpStartPower"], values["initialFuelSeconds"]);
            int passed = 0, failed = 0, skipped = 0;
            foreach (Type type in new[] { typeof(FloodBalanceTests), typeof(SimulationHostTests),
                typeof(InteractionEventBridgeTests), typeof(NoNetworkingLeakageTest) })
            foreach (MethodInfo method in type.GetMethods().Where(m => m.IsDefined(typeof(TestAttribute), false)))
            {
                object instance = Activator.CreateInstance(type);
                type.GetField("tuning", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(instance, tuning);
                try { method.Invoke(instance, null); Console.WriteLine("PASS " + method.Name); passed++; }
                catch (Exception error)
                { Console.WriteLine("FAIL " + method.Name + "\n" + (error.InnerException ?? error)); failed++; }
            }
            Console.WriteLine($"Managed NUnit methods: {passed} passed, {failed} failed, {skipped} not run.");
            Console.WriteLine($"Manual/leak={tuning.ManualPumpStrokeM3 / tuning.ManualPumpMinStrokeSeconds / tuning.LeakInflowM3PerSec:P3}; " +
                $"electric/manual={tuning.ElectricPumpM3PerSec / (tuning.ManualPumpStrokeM3 / tuning.ManualPumpMinStrokeSeconds):F3}; " +
                $"ideal drain={(tuning.FailHeightM - tuning.SafeHeightM) * tuning.FloorAreaM2 / tuning.ElectricPumpM3PerSec:F2}s");
            var idle = new SimulationHost(tuning, new FixedSimulationGeometry(0, 3.5), new LocalAuthority());
            while (idle.Current.Phase == RunPhase.Running && idle.Current.Tick < 3000) idle.Advance(.1);
            var recovery = new SimulationHost(tuning, new FixedSimulationGeometry(0, 3.5), new LocalAuthority(),
                tuning.FailHeightM * tuning.FloorAreaM2);
            recovery.Enqueue(new SimCommand(SimCommandKind.RepairLeak));
            recovery.Enqueue(new SimCommand(SimCommandKind.StartGenerator));
            recovery.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 1));
            while (recovery.Current.Phase == RunPhase.Running && recovery.Current.Tick < 900) recovery.Advance(.1);
            Console.WriteLine($"Measured core: idle failure at {idle.Current.Tick * .1:F1}s; " +
                $"electric recovery at {recovery.Current.Tick * .1:F1}s. No headset execution.");
            return failed == 0 ? 0 : 1;
        }
    }
}
#endif
