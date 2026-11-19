using System;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Lobby;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class NightmareIntegrationTests
    {
        bool previous;
        static Type Watch => Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);
        static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
        static T Get<T>(object o,string n)=>(T)o.GetType().GetProperty(n).GetValue(o);
        static object New(int day=2,KeeperGameMode mode=KeeperGameMode.Campaign)
        {
            var c=Resources.Load("M3Campaign");
            return Activator.CreateInstance(Watch,c.GetType().GetField("nights").GetValue(c),day,
                Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"),mode);
        }
        [SetUp]public void Setup(){UnityEditor.EditorUtility.audioMasterMute=true;previous=HorrorTuning.Current.nightmareEnabled;HorrorTuning.Current.nightmareEnabled=true;}
        [TearDown]public void Cleanup()=>HorrorTuning.Current.nightmareEnabled=previous;
        static void Chase(ScareSequenceState s)
        {
            Assert.True(s.Begin(3,Vector3.zero));
            for(int i=0;i<200&&s.Beat!=ScareBeat.Escape;i++)s.Tick(.1f,true,true,0,0);
            Assert.AreEqual(ScareBeat.Escape,s.Beat);
        }
        [Test]public void WatchClockFloodFuelAndPowerResumeWithoutDiscardingCompletedJobs()
        {
            var w=New();Call(w,"CompleteRepair");
            var engine=Get<object>(w,"Engine");Call(engine,"Reset",.8f,.01f,true);
            float fuel=Get<float>(engine,"Fuel");double elapsed=Get<double>(w,"Elapsed");
            var grid=Get<PowerGrid>(w,"Grid");grid.SetEngine(true);
            bool power=(bool)Call(w,"HasPower",PowerCircuit.Beacon);
            var s=Get<ScareSequenceState>(w,"Scare");Chase(s);
            Assert.False((bool)Call(w,"HasPower",PowerCircuit.Beacon));
            Call(w,"Tick",120d);Call(w,"ObserveWater",120d,5f);
            Assert.AreEqual(elapsed,Get<double>(w,"Elapsed"));Assert.AreEqual(fuel,Get<float>(engine,"Fuel"));Assert.False(Get<bool>(w,"Failed"));
            s.Tick(.1f,false,false,3,0);Assert.AreEqual(ScareBeat.Return,s.Beat);
            Assert.AreEqual(power,Call(w,"HasPower",PowerCircuit.Beacon));Assert.True(Get<bool>(w,"Repaired"));
        }
        [Test]public void FailedNightmareRetryPreservesNightAndWorkAndClearsDownedBodies()
        {
            var w=New();Call(w,"CompleteRepair");var s=Get<ScareSequenceState>(w,"Scare");Chase(s);
            var r=Get<CrewRecoveryState>(w,"Recovery");r.SetHumans(3);r.Down(1);s.Catch(1,Vector3.zero);r.Down(2);s.Catch(2,Vector3.right);
            Assert.True(Get<bool>(w,"Failed"));StringAssert.Contains("wake-up",Get<string>(w,"FailureMessage"));
            Assert.True((bool)Call(w,"RetryScare"));Assert.False(Get<bool>(w,"Failed"));Assert.True(Get<bool>(w,"Repaired"));
            Assert.AreEqual(2,Get<int>(w,"Day"));Assert.False(Get<CrewRecoveryState>(w,"Recovery").AnyDown);Assert.True(s.Reposition);
        }
        [Test]public void LateJoinRestoresSceneClockAndVictimWithoutAcceptingInvalidFrame()
        {
            var host=New();Call(host,"SetKeeperCount",2);var s=Get<ScareSequenceState>(host,"Scare");Chase(s);
            Get<CrewRecoveryState>(host,"Recovery").SetHumans(3);Get<CrewRecoveryState>(host,"Recovery").Down(2);s.Catch(2,Vector3.right);
            var packet=Call(host,"Capture",.1f);Assert.True((bool)Call(packet,"Valid"));
            var guest=New();Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",packet));
            Assert.AreEqual(s.Clock,Get<ScareSequenceState>(guest,"Scare").Clock);Assert.True(Get<CrewRecoveryState>(guest,"Recovery").Downed(2));
            var frame=(ScareSequenceFrame)packet.GetType().GetField("scare").GetValue(packet);frame.clock=float.NaN;
            Assert.False((bool)Call(guest,"ApplyRemote",packet));Assert.AreEqual(s.Clock,Get<ScareSequenceState>(guest,"Scare").Clock);
        }
        [Test]public void DisabledFeatureRejectsScenePayloadBeforeChangingGuest()
        {
            var h=New();Get<ScareSequenceState>(h,"Scare").Begin(1,Vector3.zero);var packet=Call(h,"Capture",.1f);
            HorrorTuning.Current.nightmareEnabled=false;var g=New();Call(g,"SetRemoteReplica",true);
            Assert.False((bool)Call(g,"ApplyRemote",packet));Assert.AreEqual(ScareBeat.Idle,Get<ScareSequenceState>(g,"Scare").Beat);
        }
        [Test]public void NightThreeCannotAcceptDayTwoScarePayload()
        {
            var h=New(3);Get<ScareSequenceState>(h,"Scare").Begin(1,Vector3.zero);var packet=Call(h,"Capture",.1f);
            var g=New(3);Call(g,"SetRemoteReplica",true);Assert.False((bool)Call(g,"ApplyRemote",packet));
            Assert.AreEqual(ScareBeat.Idle,Get<ScareSequenceState>(g,"Scare").Beat);
        }
    }
}
