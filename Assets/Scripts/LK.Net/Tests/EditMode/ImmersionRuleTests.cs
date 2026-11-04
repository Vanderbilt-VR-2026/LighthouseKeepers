using System;
using LighthouseKeepers.Core;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class ImmersionRuleTests
    {
        [Test] public void ExtraPullsCannotFloodImmersionEngineButChokeDeadlineStillMatters()
        {
            var tuning=ScriptableObject.CreateInstance<S4M2Tuning>();
            try
            {
                var engine=new WatchEngineState(tuning){IgnoreExtraPulls=true};engine.Reset(.8f,0,false);engine.SetChoke(true);
                for(int i=0;i<10;i++){engine.Pull();engine.Tick(.5f);}Assert.Zero(engine.FloodRemaining);Assert.False(engine.Running);
                for(int i=0;i<3;i++){Assert.True(engine.Prime());engine.Tick(.4f);}
                for(int i=0;i<3;i++){Assert.True(engine.Pull());engine.Tick(.5f);}Assert.True(engine.Running);
                for(int i=0;i<5;i++)Assert.False(engine.Pull());Assert.Zero(engine.FloodRemaining);
                engine.SetChoke(false);engine.Tick(10);Assert.True(engine.Running);Assert.True(engine.Settled);
            }
            finally{UnityEngine.Object.DestroyImmediate(tuning);}
        }
        static Type Type(string name)=>System.Type.GetType("LighthouseKeepers.WatchDemo."+name+", Assembly-CSharp",true);
        static object Call(object obj,string method,params object[] args)=>obj.GetType().GetMethod(method).Invoke(obj,args);
        static T Get<T>(object obj,string name)=>(T)obj.GetType().GetProperty(name).GetValue(obj);
        [Test] public void EveryBoltUsesTwistOnlySeparationRetainsProgressAndFlowDropsEachClick()
        {
            var profile=Resources.Load("ImmersionPipeRepair");Assert.NotNull(profile);
            var pipe=Activator.CreateInstance(Type("PipeRepairState"),profile,true);Call(pipe,"Reset",91,false);
            Call(pipe,"Observe",.1f,0f,true,true,true);Call(pipe,"Observe",.1f,45f,true,false,true);
            float partial=Get<float>(pipe,"StrokeDegrees");Assert.Greater(partial,0);
            Call(pipe,"Observe",.1f,0f,false,false,true);Assert.AreEqual(partial,Get<float>(pipe,"StrokeDegrees"));
            float previous=1;int ticks=0,lastTurns=0,lastBolts=0;
            for(int i=0;i<500&&!Get<bool>(pipe,"Complete");i++)
            {
                Assert.False(Get<bool>(pipe,"NeedsSeal"));Assert.False(Get<bool>(pipe,"NeedsReturn"));
                Call(pipe,"Observe",.1f,15f,true,false,true);
                float flow=Get<float>(pipe,"InflowFraction");Assert.LessOrEqual(flow,previous+.0001f);previous=flow;
                int b=Get<int>(pipe,"BoltsDone"),t=Get<int>(pipe,"TurnsDone");if(b>lastBolts||t>lastTurns)ticks++;lastBolts=b;lastTurns=t;
            }
            Assert.True(Get<bool>(pipe,"Complete"));Assert.AreEqual(9,ticks);Assert.Zero(previous);
        }
        [Test] public void SnapshotRejectsUnknownNarrationInvalidAgeAndBedMask()
        {
            var watch=Activator.CreateInstance(Type("WatchState"));var snapshot=Call(watch,"Capture",.02f);
            Assert.True((bool)Call(snapshot,"Valid"));
            foreach(var invalid in new[]{("radioCue",(object)99),("radioAge",(object)float.NaN),("bedMask",(object)4)})
            {var field=snapshot.GetType().GetField(invalid.Item1);var saved=field.GetValue(snapshot);field.SetValue(snapshot,invalid.Item2);Assert.False((bool)Call(snapshot,"Valid"));field.SetValue(snapshot,saved);}
        }
    }
}
