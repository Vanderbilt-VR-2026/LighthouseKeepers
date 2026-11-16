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
        [Test]public void RunningRequiresMovementAndAlternatingFreeArmsAndStopsSmoothly()
        {
            var type=Runtime("ArmSwingRun+Gait");var gait=Activator.CreateInstance(type);var tick=type.GetMethod("Tick");
            float Step(float l,float r,bool moving=true,bool free=true)=>(float)tick.Invoke(gait,new object[]{l,r,moving,free,.05f});
            for(int i=0;i<30;i++)Assert.AreEqual(1,Step(1,-1,false));
            for(int i=0;i<30;i++)Assert.AreEqual(1,Step(1,1));
            for(int i=0;i<60;i++){float sign=(i/5)%2==0?1:-1;Step(sign,-sign);}
            Assert.Greater(Step(1,-1),1.4f);
            float prior=Step(0,0,false);for(int i=0;i<20;i++){float value=Step(0,0,false);Assert.LessOrEqual(value,prior);prior=value;}Assert.AreEqual(1,prior);
            for(int i=0;i<40;i++)Assert.AreEqual(1,Step(i%2==0?1:-1,i%2==0?-1:1,true,false));
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
        {var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.Core.KeeperAudioMix")).First(t=>t!=null);var m=type.GetMethod("MusicSpeechDuck");Assert.AreEqual(-3f,m.Invoke(null,new object[]{false,true}));Assert.AreEqual(-6f,m.Invoke(null,new object[]{true,true}));}
    }
}
