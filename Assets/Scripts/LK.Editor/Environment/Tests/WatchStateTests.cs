using System;
using System.Linq;
using NUnit.Framework;
namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class WatchStateTests
    {
        static Type StateType=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.WatchDemo.WatchState")).First(t=>t!=null);
        [Test] public void SevenMinuteDeadlineAndRepairDuration()
        {var t=StateType;Assert.That(t.GetField("WatchSeconds").GetRawConstantValue(),Is.EqualTo(420d));Assert.That(t.GetField("RepairSeconds").GetRawConstantValue(),Is.EqualTo(3.5d));var s=Activator.CreateInstance(t);t.GetMethod("Tick").Invoke(s,new object[]{419.9d});Assert.That(t.GetProperty("Failed").GetValue(s),Is.False);t.GetMethod("Tick").Invoke(s,new object[]{.1d});Assert.That(t.GetProperty("Failed").GetValue(s),Is.True);Assert.That(t.GetMethod("CompleteRepair").Invoke(s,null),Is.False);}
        [Test] public void CannotSleepBeforeRepairButCanAdvanceImmediatelyAfter()
        {var t=StateType;var s=Activator.CreateInstance(t);Assert.That(t.GetMethod("Sleep").Invoke(s,null),Is.False);for(int day=2;day<=3;day++){t.GetMethod("Tick").Invoke(s,new object[]{10d});Assert.That(t.GetMethod("CompleteRepair").Invoke(s,null),Is.True);Assert.That(t.GetMethod("Sleep").Invoke(s,null),Is.True);Assert.That(t.GetProperty("Day").GetValue(s),Is.EqualTo(day));Assert.That(t.GetProperty("Elapsed").GetValue(s),Is.EqualTo(0d));Assert.That(t.GetProperty("Repaired").GetValue(s),Is.False);}}
        [Test] public void RetryClearsTimeoutWithoutAdvancingDay()
        {var t=StateType;var s=Activator.CreateInstance(t);t.GetMethod("Tick").Invoke(s,new object[]{500d});t.GetMethod("Retry").Invoke(s,null);Assert.That(t.GetProperty("Failed").GetValue(s),Is.False);Assert.That(t.GetProperty("Day").GetValue(s),Is.EqualTo(1));}
    }
}
