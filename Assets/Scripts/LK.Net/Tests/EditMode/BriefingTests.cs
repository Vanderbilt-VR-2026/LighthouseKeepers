using NUnit.Framework;using UnityEngine;using UnityEngine.UI;using TMPro;
namespace LighthouseKeepers.Net.Tests {
 public sealed class BriefingTests {
  GameObject root;int prior;bool had;
  [SetUp]public void SetUp(){had=PlayerPrefs.HasKey(CrewBriefing.Preference);prior=PlayerPrefs.GetInt(CrewBriefing.Preference);PlayerPrefs.DeleteKey(CrewBriefing.Preference);root=new GameObject("Briefing test",typeof(RectTransform));}
  [TearDown]public void TearDown(){Object.DestroyImmediate(root);if(had)PlayerPrefs.SetInt(CrewBriefing.Preference,prior);else PlayerPrefs.DeleteKey(CrewBriefing.Preference);}
  [Test]public void ThreePagesBackFinishAndSkipRememberLocally(){int closed=0;var b=CrewBriefing.Create(root.transform,()=>closed++);Assert.False(CrewBriefing.Seen);b.Open();b.Back();Assert.Zero(b.Page);b.Advance();Assert.AreEqual(1,b.Page);b.Advance();Assert.AreEqual(2,b.Page);Assert.False(CrewBriefing.Seen);b.Back();Assert.AreEqual(1,b.Page);b.Advance();b.Advance();Assert.AreEqual(1,closed);Assert.True(CrewBriefing.Seen);b.Open();Assert.Zero(b.Page);b.Skip();Assert.AreEqual(2,closed);}
  [Test]public void TwoPlayersReadIndependentlyAndAllPagesHaveTextAndDrawing(){var a=CrewBriefing.Create(root.transform,()=>{});var b=CrewBriefing.Create(root.transform,()=>{});a.Open();b.Open();a.Advance();a.Advance();Assert.Zero(b.Page);for(int i=0;i<3;i++){b.Open();for(int j=0;j<i;j++)b.Advance();var text=b.transform.Find("Briefing instructions").GetComponent<TMP_Text>();Assert.Greater(text.text.Length,100);Assert.AreEqual(i,b.GetComponentInChildren<BriefingIllustration>().Page);Assert.True(b.transform.Find("BriefingSkip").GetComponent<Button>().interactable);} }
 }
}
