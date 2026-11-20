using NUnit.Framework;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class PlaytestClarityTests
 {
  [Test]public void CaptionOnlyRewriteDoesNotRelabelTheArchivedRecording(){var c=UnityEngine.Resources.Load<UnityEngine.ScriptableObject>("HarborRadioLines");string Call(string name)=>(string)c.GetType().GetMethod(name).Invoke(c,new object[]{1});Assert.That(Call("Caption"),Does.StartWith("Kettle Rock"));Assert.That(Call("RecordedCaption"),Does.StartWith("Keeper, you hear me?"));}

  [Test]public void RoutineStatusAndUnsolicitedFlareDoNotInterruptTheWatch(){for(int i=44;i<=75;i++)Assert.False(HarborEditorialPolicy.Allows(i));foreach(int cue in new[]{27,28,30,31,32,33,37,38,39,40,41,42,78,79,80,81,103,104})Assert.False(HarborEditorialPolicy.Allows(cue),cue.ToString());}
  [Test]public void EssentialAllyMomentsStillPlay(){foreach(int cue in new[]{1,4,5,13,29,76,87,89,92,96,97,98,99})Assert.True(HarborEditorialPolicy.Allows(cue),cue.ToString());foreach(string id in new[]{"HM_N2_LANTERN_INTRO","HM_GEN_RUNNING","HM_SHIP_DANGER"})Assert.True(HarborEditorialPolicy.Allows(StoryVoiceCatalog.Get(id).cue));}
  [Test]public void StoryChatterAndRoutineHelpDoNotBypassThePolicy(){foreach(string id in new[]{"HM_N1_RULE_REASON","HM_N2_LANTERN_OIL","HM_N2_WALTER","HM_HELP_GEN_PRIME"})Assert.False(HarborEditorialPolicy.Allows(StoryVoiceCatalog.Get(id).cue));}
 }
}
