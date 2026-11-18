using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests
{
    public sealed class RadioAuthenticityTests
    {
        HorrorTuning tuning;RadioAuthenticityState model;
        [SetUp] public void Setup(){tuning=ScriptableObject.CreateInstance<HorrorTuning>();model=new(tuning);}
        [TearDown] public void Teardown()=>Object.DestroyImmediate(tuning);
        void Tick(bool ready=true,bool approaching=false,bool gone=false,bool busy=false)=>model.Tick(1,true,true,ready,approaching,gone,busy);
        void Navy(){Tick();Assert.True(model.Command(0,true,true));Assert.True(model.Command(1,true,true));model.Tick(tuning.authResultSeconds,true,true,true,false,false,false);}
        [Test] public void CannotAcceptWithoutChallengeOrReplayResolvedChoice(){Tick();Assert.False(model.Command(1,true,true));Assert.True(model.Command(0,true,true));Assert.True(model.Command(1,true,true));Assert.True(model.RealDone);Assert.False(model.Command(1,true,true));}
        [Test] public void WrongChoiceOffersRecoveryWithoutCompleting(){Tick();model.Command(0,true,true);model.Command(2,true,true);Assert.AreEqual(3,model.Stage);Assert.False(model.RealDone);Assert.False(model.Command(1,true,true));model.Command(0,true,true);model.Command(1,true,true);Assert.True(model.RealDone);}
        [Test] public void FalseCallWaitsForRealLessonAndApproach(){Tick(approaching:true);Assert.AreEqual(1,model.Kind);Navy();Tick();Assert.AreEqual(0,model.Kind);Tick(approaching:true);Assert.AreEqual(2,model.Kind);}
        [Test] public void ExactlyOneFalseSymbolAndRejectCompletes(){Navy();Tick(approaching:true);for(int i=0;i<3;i++)Assert.AreEqual(i==1?2:1,model.ReplySymbol(i,1));model.Command(0,true,true);model.Command(2,true,true);Assert.True(model.FakeDone);Assert.AreEqual(2,model.Result);}
        [Test] public void ExpiredShipCancelsUnfinishedImpostor(){Navy();Tick(approaching:true);model.Command(0,true,true);Tick(gone:true);Assert.False(model.Visible);Assert.True(model.FakeDone);Assert.False(model.Command(1,true,true));}
        [Test] public void LostPowerCancelsAndRequiresFreshChallenge(){Tick();model.Command(0,true,true);Tick(ready:false);Assert.False(model.Visible);Assert.False(model.Command(1,true,false));Tick();Assert.AreEqual(1,model.Stage);}
        [Test] public void BusyCaptionDefersStartAndOtherNightsDisable(){Tick(busy:true);Assert.False(model.Visible);Tick();model.Tick(1,false,true,true,true,false,false);Assert.False(model.Visible);}
        [Test] public void GuestCannotAuthorAndSnapshotCopies(){Tick();var copy=model.Capture();var guest=new RadioAuthenticityState(tuning){ReadOnly=true};Assert.True(guest.Apply(copy));copy.kind=2;Assert.AreEqual(1,guest.Kind);guest.Tick(20,true,true,true,true,false,false);Assert.False(guest.Command(0,true,true));Assert.AreEqual(1,guest.Stage);}
        [Test] public void InvalidReplicaRejected(){var guest=new RadioAuthenticityState(tuning){ReadOnly=true};Assert.False(guest.Apply(new(){stage=2,kind=0}));Assert.False(guest.Apply(new(){age=float.NaN}));Assert.False(guest.Apply(new(){kind=1,stage=4,result=0}));}
    }
}
