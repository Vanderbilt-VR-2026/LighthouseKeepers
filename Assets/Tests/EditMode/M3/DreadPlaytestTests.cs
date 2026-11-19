using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
    public sealed class DreadPlaytestTests
    {
        static Type Runtime(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
        [Test]public void HoldARunsUntilExhaustedAndReleaseRefillsInThreeSeconds(){
            var t=HorrorTuning.Current;var s=new KeeperStamina(t);
            Assert.AreEqual(1,s.Tick(.1f,false,true,true));Assert.AreEqual(1,s.Tick(.1f,true,false,true));
            bool warning=false;for(int i=0;i<40;i++){Assert.Greater(s.Tick(.1f,true,true,true),1);warning|=s.Warning;}
            for(int i=0;i<6;i++){s.Tick(.1f,true,true,true);warning|=s.Warning;}Assert.True(warning);Assert.AreEqual(0,s.Remaining);
            Assert.AreEqual(1,s.Tick(2,true,true,true));Assert.AreEqual(0,s.Remaining);
            s.Tick(3,false,true,true);Assert.AreEqual(1,s.Remaining);Assert.Greater(s.Tick(.1f,true,true,true),1);
        }
        [Test]public void MenuAndPossessionCannotRunAndInvalidTimeCannotSpendStamina(){
            var s=new KeeperStamina(HorrorTuning.Current);Assert.AreEqual(1,s.Tick(.1f,true,true,false));Assert.AreEqual(1,s.Remaining);
            s.Tick(float.NaN,true,true,true);Assert.AreEqual(1,s.Remaining);
        }
        [Test]public void BriefEncounterEndsWithoutWaitingTwelveSecondsAndReplicasSharePose()
        {
            var traces=new SabotageState(new NightScenario{faults=new[]{new NightFault{stationId="engine"}}});traces.Warn(0,80);traces.Broke(0,83);
            foreach(int pose in new[]{0,1,2}){
                var h=new DrownedState(traces,false,brief:true);h.Tick(83,true,false,false);h.SetAnchor(1,0,pose==2?3:0,2,0,83,pose);h.Tick(86,true,false,false);
                Assert.AreEqual(DrownedPhase.Manifest,h.Phase);var g=new DrownedState(traces,false,brief:true){ReadOnly=true};Assert.True(g.Apply(h.Capture(),86));Assert.AreEqual(pose,g.PoseKind);
                h.Tick(86+(pose==1?4.9:pose==2?1.9:1.2),true,false,false);Assert.AreEqual(DrownedPhase.Retreat,h.Phase);
                var invalid=g.Capture();invalid.poseKind=3;Assert.False(g.Apply(invalid,87));
            }
        }
        [Test]public void VoltageSagsAreBroadBoundedAndTutorialIsStable()
        {
            var method=Runtime("DrownedMotion").GetMethod("LampDip");float last=1,min=1;
            for(int i=0;i<600;i++){float t=i*.05f;Assert.AreEqual(1,(float)method.Invoke(null,new object[]{(double)t,1,2}));float v=(float)method.Invoke(null,new object[]{(double)t,2,2});Assert.That(v,Is.InRange(.61f,1.001f));Assert.Less(Mathf.Abs(v-last),.06f);last=v;min=Mathf.Min(min,v);}Assert.Less(min,.7f);
        }
        [Test]public void NarrationDucksMusicButDoesNotSilenceIt()
        {var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.Core.KeeperAudioMix")).First(t=>t!=null);var m=type.GetMethod("MusicSpeechDuck");Assert.AreEqual(-7f,m.Invoke(null,new object[]{false,true}));Assert.AreEqual(-7f,m.Invoke(null,new object[]{true,true}));Assert.AreEqual(0f,m.Invoke(null,new object[]{false,false}));}
    }
}
