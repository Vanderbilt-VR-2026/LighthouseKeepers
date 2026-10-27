using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class ReleaseSoloOverrideTests
    {
        [Test] public void DayShortcutHasNoMutableRequestInRelease()
        {
            var type=System.Type.GetType("LighthouseKeepers.StormWatch.WatchDayLaunch, LighthouseKeepers.Runtime",true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Assert.IsNotNull(type.GetProperty("RequestedDay").SetMethod);
#else
            Assert.IsNull(type.GetProperty("RequestedDay"));
            Assert.That(type.GetMethod("Consume").Invoke(null,null),Is.EqualTo(1));
            Assert.IsEmpty(type.GetFields(System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static));
#endif
        }
        [Test] public void ReleaseBuildCannotEnableSoloOverride()
        {
            var property=typeof(DebugSoloOverride).GetProperty(nameof(DebugSoloOverride.Enabled));
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Assert.IsTrue(DebugSoloOverride.CompiledIn);
            Assert.IsNotNull(property.SetMethod);
#else
            Assert.IsFalse(DebugSoloOverride.CompiledIn);Assert.IsFalse(DebugSoloOverride.Enabled);
            Assert.IsNull(property.SetMethod,"Release must contain no setter, including one callable by reflection.");
            Assert.IsEmpty(typeof(DebugSoloOverride).GetFields(System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic),
                "Release must not contain a mutable hidden toggle.");
            var gate=new CoopStationGate();gate.Observe(1,10,true,0);gate.Observe(1,20,true,0);
            Assert.IsFalse(gate.AllowsForDevelopment(10,20,.1));
#endif
        }
    }
}
