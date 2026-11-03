using System;
using NUnit.Framework;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class NetworkPracticeFreezeTests
    {
        static object CreateWatch() => Activator.CreateInstance(Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true));
        static object Call(object target,string name,params object[] args) => target.GetType().GetMethod(name).Invoke(target,args);
        static T Value<T>(object target,string name) => (T)target.GetType().GetProperty(name).GetValue(target);

        [Test] public void PracticeDoesNotAdvanceTheClockOrAcceptIndependentRepairAndRadioProgress()
        {
            var state=CreateWatch();Call(state,"SetNetworkRehearsal",true);
            var pipe=Value<object>(state,"Pipe");var radio=Value<object>(state,"Radio");
            double elapsed=Value<double>(state,"Elapsed");
            Call(state,"Tick",120d);
            Call(pipe,"Observe",10f,90f,true,true,true);
            Assert.AreEqual(elapsed,Value<double>(state,"Elapsed"));
            Assert.AreEqual(0,Value<float>(pipe,"SeatElapsed"));
            Assert.IsFalse((bool)Call(state,"CompleteRepair"));
            Assert.IsFalse((bool)Call(radio,"ToggleMain"));
            Assert.IsTrue(Value<bool>(state,"NetworkRehearsal"));
        }
        [Test] public void PracticeCannotAdvanceToAnotherDayEvenAfterATaskWasCompleted()
        {
            var state=CreateWatch();Assert.IsTrue((bool)Call(state,"CompleteRepair"));
            Assert.IsTrue(Value<bool>(state,"Complete"));
            int day=Value<int>(state,"Day");Call(state,"SetNetworkRehearsal",true);
            Assert.IsFalse((bool)Call(state,"Sleep"));
            Assert.AreEqual(day,Value<int>(state,"Day"));
        }
    }
}
