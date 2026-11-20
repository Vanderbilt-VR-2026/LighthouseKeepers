using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class ArrivalBriefingTests
    {
        [Test]public void BothHumansMustFinishButNeedNotReadTogether()
        {var g=new CrewArrivalGate();int[] crew={1,2};Assert.True(g.Hold(crew,true));Assert.True(g.Acknowledge(2,crew));Assert.True(g.Hold(crew,true));Assert.True(g.Acknowledge(1,crew));Assert.False(g.Hold(crew,true));}
        [Test]public void SkipOrFinishDoesNotReleaseBeforeTransportIsReady()
        {var g=new CrewArrivalGate();int[] crew={5};g.Acknowledge(5,crew);Assert.True(g.Hold(crew,false));Assert.False(g.Hold(crew,true));}
        [Test]public void DisconnectionCannotStrandTheRemainingKeeper()
        {var g=new CrewArrivalGate();g.Acknowledge(1,new[]{1,2});Assert.True(g.Hold(new[]{1,2},true));Assert.False(g.Hold(new[]{1},true));}
        [Test]public void LateJoinCannotPauseAWatchThatAlreadyStarted()
        {var g=new CrewArrivalGate();g.Acknowledge(1,new[]{1});Assert.False(g.Hold(new[]{1},true));Assert.False(g.Hold(new[]{1,2},false));Assert.False(g.Hold(new[]{1,2},true));}
        [Test]public void UnknownActorsAndEmptyRosterNeverReleaseTheGate()
        {var g=new CrewArrivalGate();Assert.False(g.Acknowledge(9,new[]{1}));Assert.False(g.Acknowledge(0,new[]{1}));Assert.True(g.Hold(new int[0],true));Assert.True(g.Hold(new[]{1},true));}
        [Test]public void AuthenticatedCommandWindowAcceptsArrivalCompletion()
        {Assert.True(new CoopCommandWindow().Accept(1,1,1,1,CoopAction.BriefingComplete,0));}
        [Test]public void HostClocksPauseAndReplicaCannotReleaseTheHold()
        {
            var t=System.Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);
            var cat=Resources.Load("WatchDaysS4M2");var days=cat.GetType().GetField("days").GetValue(cat);
            object New()=>System.Activator.CreateInstance(t,days,3,Resources.Load<LighthouseKeepers.Core.S4Tuning>("S4Tuning"),Resources.Load<LighthouseKeepers.Core.S4M2Tuning>("S4M2Tuning"),Resources.Load<LighthouseKeepers.Core.ImmersionTuning>("ImmersionTuning"));
            object Call(object s,string method,params object[] a)=>t.GetMethod(method).Invoke(s,a);
            object Get(object s,string prop)=>t.GetProperty(prop).GetValue(s);
            var host=New();Call(host,"HoldBriefing",true);Assert.False((bool)Call(host,"AdvanceWatchGate",90d));Call(host,"Tick",90d);
            Assert.AreEqual(0d,Get(host,"Elapsed"));Assert.AreEqual(0d,Get(host,"WatchGateElapsed"));
            var guest=New();Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.02f)));
            Call(guest,"HoldBriefing",false);Assert.True((bool)Get(guest,"BriefingHeld"));
            Call(host,"HoldBriefing",false);Call(host,"Tick",1d);Assert.AreEqual(1d,Get(host,"Elapsed"));
        }
        [Test]public void ArtExistsAndControlsKeepTheExactNativeDiagram()
        {var root=new GameObject("Arrival test",typeof(RectTransform));try{var b=CrewBriefing.Create(root.transform,()=>{});b.Open();var art=b.transform.Find("Duty illustration").GetComponent<RawImage>();Assert.True(art.enabled);Assert.NotNull(art.texture);b.Advance();Assert.True(art.enabled);Assert.NotNull(art.texture);b.Advance();Assert.False(art.enabled);Assert.True(b.GetComponentInChildren<BriefingIllustration>().enabled);}finally{Object.DestroyImmediate(root);}}
    }
}
