using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class RadioStateTests
    {
        object radio;Type type;
        static Type Find(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.WatchDemo."+name)).First(t=>t!=null);
        object Call(string name,params object[] args)=>type.GetMethod(name).Invoke(radio,args);
        T Get<T>(string name)=>(T)type.GetProperty(name).GetValue(radio);
        [SetUp] public void Create(){type=Find("RadioState");radio=Activator.CreateInstance(type,new object[]{Resources.Load("StormRadio")});}
        void Lock(){Call("SetPower",true);Call("ToggleMain");Call("Tune",156.8f);Call("Tick",1.01d,true);}
        void Code(){foreach(int c in new[]{0,2,1,3})Assert.That(Call("PressCode",c),Is.EqualTo(1));}
        [Test] public void PowerAndOneContinuousSecondWithinToleranceAreRequired()
        {
            Assert.That(Call("ToggleMain"),Is.False);Call("Tune",156.8f);Call("Tick",2d,true);Assert.That(Get<bool>("Locked"),Is.False);
            Call("SetPower",true);Call("ToggleMain");Call("Tick",.9d,true);Assert.That(Get<bool>("Locked"),Is.False);
            Call("Tune",157f);Call("Tune",156.8f);Call("Tick",.2d,true);Assert.That(Get<bool>("Locked"),Is.False);
            Call("Tick",.8d,true);Assert.That(Get<bool>("Locked"),Is.True);
            Call("SetPower",false);Assert.That(Get<bool>("MainOn"),Is.False);Assert.That(Get<bool>("Locked"),Is.False);
        }
        [Test] public void WrongSymbolResetsOnlyCodeAndRepeatAfterAcceptanceIsIgnored()
        {
            Lock();Call("PressCode",0);Assert.That(Call("PressCode",3),Is.EqualTo(0));Assert.That(Get<int>("CodeProgress"),Is.Zero);
            Assert.That(Get<bool>("MainOn")&&Get<bool>("Locked"),Is.True);Code();Assert.That(Call("PressCode",0),Is.EqualTo(-1));
        }
        [TestCase(false,true)] [TestCase(true,false)] public void MicAndTriggerMustBothRemainHeldForThreeSeconds(bool held,bool trigger)
        {
            Lock();Code();Call("SetMicrophone",true,true);Call("Tick",2.9d,true);Assert.That(Get<bool>("Completed"),Is.False);
            Call("SetMicrophone",held,trigger);Call("Tick",.2d,true);Assert.That(Get<float>("TransmitElapsed"),Is.Zero);
            Call("SetMicrophone",true,true);Call("Tick",2.9d,true);Assert.That(Get<bool>("Completed"),Is.False);Call("Tick",.11d,true);Assert.That(Get<bool>("Completed"),Is.True);
            Call("Reset",false);Assert.That(Get<bool>("Completed"),Is.False);Assert.That(Get<int>("CodeProgress"),Is.Zero);
        }
        [Test] public void TimeoutBlocksTransmissionAndPowerLossCancelsHold()
        {
            Lock();Code();Call("SetMicrophone",true,true);Call("Tick",4d,false);Assert.That(Get<bool>("Completed"),Is.False);
            Call("Tick",2d,true);Call("SetPower",false);Call("Tick",4d,true);Assert.That(Get<bool>("Completed"),Is.False);Assert.That(Get<float>("TransmitElapsed"),Is.Zero);
        }
        [Test] public void EmergencyPulseIsContinuousNonBlackAndAtMostTwoThirdsHertz()
        {
            var pulse=Find("EmergencyLighting").GetMethod("PulseAt");float previous=(float)pulse.Invoke(null,new object[]{0f,1.5f});
            for(int i=1;i<=300;i++){float v=(float)pulse.Invoke(null,new object[]{i*.01f,1.5f});Assert.That(v,Is.InRange(.319f,.721f));Assert.That(Mathf.Abs(v-previous),Is.LessThan(.009f));previous=v;}
        }
    }
}
