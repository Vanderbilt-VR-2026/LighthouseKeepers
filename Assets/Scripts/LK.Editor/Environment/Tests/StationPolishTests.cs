using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class StationPolishTests
    {
        static Type TypeOf(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.WatchDemo."+n)).First(t=>t!=null);
        static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n).Invoke(o,args);
        static T Get<T>(object o,string n)=>(T)o.GetType().GetProperty(n).GetValue(o);
        static object Pipe(int seed=1){var p=Activator.CreateInstance(TypeOf("PipeRepairState"),new object[]{Resources.Load("PipeRepair")});Call(p,"Reset",seed,false);return p;}
        static void Observe(object p,float seconds,float angle=0,bool contact=true,bool steady=true,bool canWork=true)=>Call(p,"Observe",seconds,angle,contact,steady,canWork);
        static void Seat(object p){Observe(p,.7f);if(Get<bool>(p,"NeedsSeal"))Observe(p,3.1f);}
        static void FinishBolt(object p){Seat(p);int initial=Get<int>(p,"BoltsDone");for(int i=0;i<3&&Get<int>(p,"BoltsDone")==initial;i++){Observe(p,1,90);if(Get<int>(p,"BoltsDone")==initial)Observe(p,1,-70);}}
        [Test] public void StationaryWrenchAndJitterCannotRepairButSixFullTurnsFinishThreeDistinctBolts()
        {
            var p=Pipe();Observe(p,60);Assert.That(Get<int>(p,"BoltsDone"),Is.Zero);
            for(int i=0;i<100;i++){Observe(p,.05f,1);Observe(p,.05f,-1);}Assert.That(Get<int>(p,"TurnsDone"),Is.Zero);
            var seen=new System.Collections.Generic.HashSet<int>();
            for(int i=0;i<3;i++){seen.Add(Get<int>(p,"ActiveBolt"));FinishBolt(p);Assert.That(Get<int>(p,"BoltsDone"),Is.EqualTo(i+1));Assert.That(Get<float>(p,"InflowFraction"),Is.EqualTo(1-(i+1)/3f).Within(.001));}
            Assert.That(seen.Count,Is.EqualTo(3));Assert.That(Get<bool>(p,"Complete"),Is.True);
        }
        [Test] public void NormalQuickQuarterTurnCountsWithoutForcingArtificiallySlowWrists()
        {
            var p=Pipe();Seat(p);Observe(p,.25f,90);
            Assert.That(Get<int>(p,"TurnsDone")+Get<int>(p,"BoltsDone"),Is.EqualTo(1));
        }
        [Test] public void ContactLossRetainsCompletedClicksAndOnlyPartOfUnfinishedStroke()
        {
            object p=null;for(int seed=1;seed<20;seed++){p=Pipe(seed);if(Get<int>(p,"ActiveBolt")!=2)break;}
            Seat(p);Observe(p,1,90);Assert.That(Get<int>(p,"TurnsDone"),Is.EqualTo(1));
            Observe(p,1,90);Assert.That(Get<int>(p,"TurnsDone"),Is.EqualTo(1),"Must return the ratchet");
            Observe(p,1,-70);Observe(p,.5f,40);Observe(p,.1f,0,false);
            Assert.That(Get<float>(p,"StrokeDegrees"),Is.EqualTo(26).Within(.01));Assert.That(Get<int>(p,"TurnsDone"),Is.EqualTo(1));
            Observe(p,.1f,0,false);Assert.That(Get<float>(p,"StrokeDegrees"),Is.EqualTo(26).Within(.01),"No repeated loss while away");
            Observe(p,10,90,true,true,false);Assert.That(Get<int>(p,"TurnsDone"),Is.EqualTo(1),"Watch failure blocks work");
        }
        [Test] public void SealRequiresContinuousSteadinessAndOrderVariesAcrossSeeds()
        {
            object p=null;var firsts=new System.Collections.Generic.HashSet<int>();for(int seed=0;seed<30;seed++){var q=Pipe(seed);firsts.Add(Get<int>(q,"ActiveBolt"));if(Get<int>(q,"ActiveBolt")==2)p=q;}
            Assert.That(firsts.Count,Is.EqualTo(3));Observe(p,1);Observe(p,1);Observe(p,.1f,0,true,false);Assert.That(Get<float>(p,"SealElapsed"),Is.Zero);
            Observe(p,2.9f);Assert.That(Get<bool>(p,"NeedsSeal"),Is.True);Observe(p,.2f);Observe(p,1,90);Assert.That(Get<int>(p,"BoltsDone"),Is.EqualTo(1));
        }
        static object Day(int day)=>Activator.CreateInstance(TypeOf("WatchState"),new object[]{Resources.Load<DayCatalog>("WatchDays").days,day});
        [TestCase(2,90)] [TestCase(3,75)] public void ContinuousDarkDeadlineLatchesFailureAndRetryResetsSameDay(int day,int limit)
        {
            var s=Day(day);Call(s,"Tick",(double)limit-.1);Assert.That(Get<bool>(s,"Failed"),Is.False);Assert.That(Get<double>(s,"BeaconDarkRemaining"),Is.EqualTo(.1).Within(.001));
            Call(s,"Tick",.11d);Assert.That(Get<bool>(s,"BeaconDarkFailed"),Is.True);Assert.That(Call(s,"StarterPulled"),Is.False);Assert.That(Call(s,"Sleep"),Is.False);
            Assert.That(Get<string>(s,"FailureMessage"),Does.Contain("BEACON DARK"));Call(s,"Retry");Assert.That(Get<bool>(s,"Failed"),Is.False);Assert.That(Get<int>(s,"Day"),Is.EqualTo(day));Assert.That(Get<double>(s,"BeaconDarkRemaining"),Is.EqualTo(limit));
        }
        [Test] public void TimelyGeneratorStopsDarkCountdownAndDayOneHasNoBeaconLoss()
        {
            var s=Day(3);Call(s,"Tick",74d);for(int i=0;i<3;i++)Call(s,"StarterPulled");Call(s,"Tick",100d);
            Assert.That(Get<bool>(s,"Failed"),Is.False);Assert.That(Get<double>(s,"BeaconDarkElapsed"),Is.Zero);
            s=Day(1);Call(s,"Tick",100d);Assert.That(Get<bool>(s,"BeaconDarkFailed"),Is.False);Assert.That(Get<double>(s,"BeaconDarkElapsed"),Is.Zero);
        }
    }
}
