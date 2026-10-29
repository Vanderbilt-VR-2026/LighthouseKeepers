using System;
using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class WatchReplicaTests
    {
        static object Watch()=>Activator.CreateInstance(Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true));
        static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name).Invoke(target,args);
        static T Get<T>(object target,string name)=>(T)target.GetType().GetProperty(name).GetValue(target);
        [Test] public void HostDayPowerAndTaskStateAppliesToReadOnlyGuestWhileGuestCannotAdvanceAnything()
        {
            var host=Watch();var guest=Watch();Call(guest,"SetRemoteReplica",true);
            Call(host,"Tick",12d);Call(host,"CompleteRepair");Call(host,"Sleep"); // Day 2 starts without power.
            Call(host,"Tick",5d);
            Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.12f)));
            Assert.AreEqual(2,Get<int>(guest,"Day"));Assert.AreEqual(5d,Get<double>(guest,"Elapsed"));
            Assert.AreEqual("Off",Get<object>(guest,"Power").ToString());
            Call(guest,"Tick",100d);Assert.IsFalse((bool)Call(guest,"StarterPulled"));Assert.IsFalse((bool)Call(guest,"CompleteRepair"));
            Assert.IsFalse((bool)Call(guest,"Sleep"));Call(guest,"Retry");Assert.AreEqual(5d,Get<double>(guest,"Elapsed"));
            var radio=Get<object>(guest,"Radio");Assert.IsFalse((bool)Call(radio,"ToggleMain"));
            for(int i=0;i<3;i++)Call(host,"StarterPulled");Call(host,"CompleteRepair");
            Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.02f)));
            Assert.AreEqual("On",Get<object>(guest,"Power").ToString());Assert.AreEqual(2,Get<int>(guest,"CompletedTasks"));
            Assert.IsFalse((bool)Call(guest,"Sleep"));Call(host,"Sleep");
            Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.02f)));Assert.AreEqual(3,Get<int>(guest,"Day"));
        }
        [Test] public void SoloRejectsRemoteWritesAndReplicaRejectsMalformedOrUnknownDay()
        {
            var host=Watch();var guest=Watch();var snapshot=Call(host,"Capture",.02f);
            Assert.IsFalse((bool)Call(host,"ApplyRemote",snapshot));Call(guest,"SetRemoteReplica",true);
            snapshot.GetType().GetField("day").SetValue(snapshot,999);Assert.IsFalse((bool)Call(guest,"ApplyRemote",snapshot));
            snapshot=Call(host,"Capture",float.NaN);Assert.IsFalse((bool)Call(guest,"ApplyRemote",snapshot));
            Assert.AreEqual(1,Get<int>(guest,"Day"));Assert.AreEqual(0d,Get<double>(guest,"Elapsed"));
        }
    }
}
