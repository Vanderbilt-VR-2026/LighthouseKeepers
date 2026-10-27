using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class WatchStateTests
    {
        static Type StateType=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.WatchDemo.WatchState")).First(t=>t!=null);
        static object Call(object s,string method,params object[] args)=>StateType.GetMethod(method).Invoke(s,args);
        static T Get<T>(object s,string property)=>(T)StateType.GetProperty(property).GetValue(s);
        static object Day2(){var s=Activator.CreateInstance(StateType);Call(s,"CompleteRepair");Call(s,"Sleep");return s;}
        [Test] public void SevenMinuteDeadlineAndRepairDuration()
        {
            Assert.That(StateType.GetField("RepairSeconds").GetRawConstantValue(),Is.EqualTo(3.5d));var s=Activator.CreateInstance(StateType);
            Assert.That(Get<double>(s,"Duration"),Is.EqualTo(420));Call(s,"Tick",419.9d);Assert.That(Get<bool>(s,"Failed"),Is.False);
            Call(s,"Tick",.1d);Assert.That(Get<bool>(s,"Failed"),Is.True);Assert.That(Call(s,"CompleteRepair"),Is.False);
        }
        [TestCase(true)] [TestCase(false)] public void DayTwoAllowsEitherOrderAndRequiresBothBeforeSleep(bool repairFirst)
        {
            var s=Day2();Assert.That(Get<object>(s,"Power").ToString(),Is.EqualTo("Off"));
            if(repairFirst)Call(s,"CompleteRepair");
            for(int i=0;i<2;i++){Assert.That(Call(s,"StarterPulled"),Is.True);Assert.That(Call(s,"Sleep"),Is.False);}
            Assert.That(Get<object>(s,"Power").ToString(),Is.EqualTo("Off"));Call(s,"StarterPulled");
            Assert.That(Get<object>(s,"Power").ToString(),Is.EqualTo("On"));Assert.That(Call(s,"StarterPulled"),Is.False);
            if(!repairFirst){Assert.That(Call(s,"Sleep"),Is.False);Call(s,"CompleteRepair");}
            Assert.That(Get<int>(s,"CompletedTasks"),Is.EqualTo(2));Assert.That(Call(s,"Sleep"),Is.True);
            Assert.That(Get<int>(s,"Day"),Is.EqualTo(3));Assert.That(Get<double>(s,"Elapsed"),Is.Zero);
            Assert.That(Get<object>(s,"Power").ToString(),Is.EqualTo("Off"));Assert.That(Get<int>(s,"StarterPulls"),Is.Zero);
        }
        [Test] public void RetryClearsTimeoutAndPartialPullsWithoutAdvancingDay()
        {
            var s=Day2();Call(s,"StarterPulled");Call(s,"Tick",500d);Assert.That(Call(s,"StarterPulled"),Is.False);
            Call(s,"Retry");Assert.That(Get<bool>(s,"Failed"),Is.False);Assert.That(Get<int>(s,"Day"),Is.EqualTo(2));Assert.That(Get<int>(s,"StarterPulls"),Is.Zero);
            Assert.That(Call(s,"Sleep"),Is.False);
            foreach(double invalid in new[]{-1d,double.NaN,double.PositiveInfinity})Assert.Throws<System.Reflection.TargetInvocationException>(()=>Call(s,"Tick",invalid));
        }
        static void FinishRadio(object state)
        {
            var radio=Get<object>(state,"Radio");var t=radio.GetType();
            t.GetMethod("ToggleMain").Invoke(radio,null);t.GetMethod("Tune").Invoke(radio,new object[]{156.8f});Call(state,"Tick",1.1d);
            foreach(int c in new[]{0,2,1,3})t.GetMethod("PressCode").Invoke(radio,new object[]{c});
            t.GetMethod("SetMicrophone").Invoke(radio,new object[]{true,true});Call(state,"Tick",3.1d);
        }
        [Test] public void NewAssetIsDiscoveredWithoutCodeOrManualCatalogEdit()
        {
            var refresh=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.WatchDemo.Editor.WatchDayAuthoring")).First(t=>t!=null).GetMethod("RefreshCatalog");
            const string path="Assets/Data/Days/Day05-Test.asset";
            var fourth=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<DayDefinition>("Assets/Data/Days/Day03.asset"));fourth.day=5;fourth.dayLengthSeconds=123;
            try
            {
                AssetDatabase.CreateAsset(fourth,path);refresh.Invoke(null,null);
                var s=Activator.CreateInstance(StateType);
                for(int day=1;day<=4;day++){Call(s,"CompleteRepair");for(int i=0;i<3;i++)Call(s,"StarterPulled");FinishRadio(s);Assert.That(Call(s,"Sleep"),Is.True);}
                Assert.That(Get<int>(s,"Day"),Is.EqualTo(5));Assert.That(Get<double>(s,"Duration"),Is.EqualTo(123));
                Call(s,"CompleteRepair");for(int i=0;i<3;i++)Call(s,"StarterPulled");FinishRadio(s);Assert.That(Call(s,"Sleep"),Is.False,"No synthetic repeated days beyond the authored catalog");
            }
            finally{AssetDatabase.DeleteAsset(path);refresh.Invoke(null,null);}
        }
    }
}
