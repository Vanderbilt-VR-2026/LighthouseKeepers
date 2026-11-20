using System;
using System.Linq;
using NUnit.Framework;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
    public sealed class StoryRulesTests
    {
        static StoryVoiceLine L(string id,int priority=2,string repeat="1/night",bool ant=false)=>new StoryVoiceLine{id=id,speaker=ant?"W":"HM",priority=priority,repeat=repeat,mode="B"};
        static StorySpeechContext Context(bool calm=true,bool chat=false,int night=3,bool alone=true,bool dark=true,bool full=true)=>new(night,2,calm,chat,alone,dark,full);
        static StorySpeechRequest Poll(StorySpeechQueue q,double now,StorySpeechContext? c=null,Func<StorySpeechRequest,bool> valid=null)=>q.Poll(now,c??Context(),valid??(_=>true),_=>2);
        [Test] public void ApprovedCatalogHasAllExactUniqueRolesAndNoLegacyIds()
        {
            var lines=StoryVoiceCatalog.Lines;Assert.AreEqual(112,lines.Count);Assert.AreEqual(89,lines.Count(x=>!x.Antagonist));Assert.AreEqual(23,lines.Count(x=>x.Antagonist));
            Assert.AreEqual(112,lines.Select(x=>x.cue).Distinct().Count());Assert.AreEqual(112,lines.Select(x=>x.id).Distinct().Count());
            Assert.That(lines.All(x=>x.cue>=200));Assert.AreEqual("That'll hold. Good work.",StoryVoiceCatalog.Get("HM_PIPE_FIXED").text);
            Assert.AreEqual("He stood in the light until it went out.",StoryVoiceCatalog.Get("ANT_N5_TOM").text);
        }
        [Test] public void RepairedFaultIsDiscardedEvenWhileAnotherLinePlays()
        {var q=new StorySpeechQueue();q.Enqueue(L("intro"),0);Poll(q,0);q.Enqueue(L("fault"),.1);Assert.Null(Poll(q,1,valid:r=>r.Line.id!="fault"));Assert.AreEqual(0,q.Count);Assert.False(q.Played("fault"));}
        [Test] public void TwoInstructionsThenEightSecondsOfBreathingRoom()
        {var q=new StorySpeechQueue();foreach(var id in new[]{"a","b","c"})q.Enqueue(L(id),0);Assert.NotNull(Poll(q,0));Assert.NotNull(Poll(q,3));Assert.Null(Poll(q,6));Assert.Null(Poll(q,12.99));Assert.NotNull(Poll(q,13));}
        [Test] public void UrgentDangerCancelsQueuedStoryInterruptsLowerButNeverVoiceChat()
        {var q=new StorySpeechQueue();q.Enqueue(L("speaking",3),0);Poll(q,0);q.Enqueue(L("story",3),.1);q.Enqueue(L("danger",1),.2);Assert.AreEqual(1,q.Count);Assert.Null(Poll(q,.2,Context(chat:true)));Assert.AreEqual("danger",Poll(q,.3).Line.id);Assert.False(q.Played("story"));}
        [Test] public void StoryWaitsForCalmAndSpecificPrerequisiteStillHasToHold()
        {var q=new StorySpeechQueue();q.Enqueue(L("confession",3),0);Assert.Null(Poll(q,0,Context(calm:false)));Assert.NotNull(Poll(q,1));q.Enqueue(L("choke"),4);Assert.Null(Poll(q,4,valid:_=>false));}
        [Test] public void SameShipIsNotAnnouncedTwiceButAnotherShipIs()
        {var q=new StorySpeechQueue();var l=L("ship",2,"1/ship");q.Enqueue(l,0,2);Poll(q,0);Assert.False(q.Enqueue(l,3,2));Assert.True(q.Enqueue(l,3,3));Assert.NotNull(Poll(q,3));}
        [Test] public void RunReceiptsSurviveWakingButResetForNewCampaign()
        {var q=new StorySpeechQueue();var l=L("rule",2,"1/run");q.Enqueue(l,0);Poll(q,0);q.BeginNight();Assert.False(q.Enqueue(l,0));q.BeginNight(true);Assert.True(q.Enqueue(l,0));}
        [Test] public void CooldownMeasuredFromActualPlayAndMissingAudioDoesNotConsumeReceipt()
        {var q=new StorySpeechQueue();var l=L("help",2,"60 s cd");q.Enqueue(l,0);Assert.Null(q.Poll(0,Context(),_=>true,_=>0));Assert.False(q.Played("help"));q.Enqueue(l,1);Poll(q,1);Assert.False(q.Enqueue(l,60));Assert.True(q.Enqueue(l,61));}
        [Test] public void HelpNeverInterruptsActionAndRequiresNearIdleOrTwoFailures()
        {Assert.False(StorySpeechQueue.HelpReady(90,5,true));Assert.False(StorySpeechQueue.HelpReady(19,1,false));Assert.True(StorySpeechQueue.HelpReady(20,0,false));Assert.True(StorySpeechQueue.HelpReady(0,2,false));}
        [TestCase(1,true,true,true)][TestCase(3,false,true,true)][TestCase(3,true,false,true)][TestCase(3,true,true,false)]
        public void WhisperRequiresNightTwoFullDarkAndAlone(int night,bool alone,bool dark,bool full)
        {var q=new StorySpeechQueue();q.Enqueue(L("ANT_x",3,"1/night",true),0);Assert.Null(Poll(q,0,Context(night:night,alone:alone,dark:dark,full:full)));}
        [Test] public void WhisperWaitsForHarborQueueFourSecondsThenNinetySecondCooldown()
        {var q=new StorySpeechQueue();q.Enqueue(L("ANT_a",3,"1/night",true),0,lifetime:200);q.Enqueue(L("hm",3),0);Assert.AreEqual("hm",Poll(q,0).Line.id);Assert.Null(Poll(q,5.9));Assert.AreEqual("ANT_a",Poll(q,6).Line.id);q.Enqueue(L("ANT_b",3,"1/night",true),8,lifetime:200);Assert.Null(Poll(q,95.9));Assert.NotNull(Poll(q,96));}
        [Test] public void WhisperBudgetIsSixEvenWhenAllDifferentLines()
        {var q=new StorySpeechQueue();for(int i=0;i<7;i++){q.Enqueue(L("ANT_"+i,3,"1/night",true),i*100);var p=Poll(q,i*100);Assert.AreEqual(i<6,p!=null);}Assert.AreEqual(6,q.AntagonistCount);}
        [TestCase(false,false,false,false,false)][TestCase(true,true,false,false,false)][TestCase(true,false,true,false,false)][TestCase(true,false,false,true,false)][TestCase(true,false,false,false,true)]
        public void NoTakingWithoutKnownUnwatchedDarkUnhiddenVictim(bool known,bool watched,bool powered,bool high,bool hidden)
        {Assert.False(new KeeperExposure(known,watched,powered,high,hidden).CanBeTaken);}
        [Test] public void HuntNeedsTutorialProtectionAndVerifiedEscape()
        {var p=new KeeperExposure(true,false,false,false);Assert.True(p.CanBeTaken);Assert.True(KeeperWitnessRules.CanStartHunt(3,60,false,true,true,p));Assert.False(KeeperWitnessRules.CanStartHunt(3,59,false,true,true,p));Assert.False(KeeperWitnessRules.CanStartHunt(2,100,false,true,true,p));Assert.False(KeeperWitnessRules.CanStartHunt(3,60,true,true,true,p));Assert.False(KeeperWitnessRules.CanStartHunt(3,60,false,true,false,p));}
        [Test] public void SabotageNeedsUnwatchedEvidenceAndSpacing()
        {Assert.True(KeeperWitnessRules.CanSabotage(true,false,15,15));Assert.False(KeeperWitnessRules.CanSabotage(false,false,30,30));Assert.False(KeeperWitnessRules.CanSabotage(true,true,30,30));Assert.False(KeeperWitnessRules.CanSabotage(true,false,14.99,30));Assert.False(KeeperWitnessRules.CanSabotage(true,false,30,14.99));}
    }
}
