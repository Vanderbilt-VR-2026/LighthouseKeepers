using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Linq;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Progression;
namespace LighthouseKeepers.Tests {
public sealed class FoundationTests {
 [Test] public void FloodClampsAndResetPauses(){var go=new GameObject();try{var flood=go.AddComponent<FloodController>();flood.SetHeight(1000);Assert.AreEqual(10.5f,flood.Height);flood.ResumeFlood();flood.ResetFlood();Assert.IsTrue(flood.Paused);Assert.AreEqual(-.25f,flood.Height);}finally{Object.DestroyImmediate(go);}}
 [Test] public void ThresholdFiresOnlyOnCrossingAndSupportsReset(){var go=new GameObject();try{var threshold=go.AddComponent<FloodThreshold>();int count=0;threshold.OnSubmergedChanged.AddListener(_=>count++);threshold.Evaluate(1);threshold.Evaluate(2);Assert.AreEqual(1,count);threshold.Evaluate(-1);Assert.AreEqual(2,count);Assert.IsFalse(threshold.Submerged);}finally{Object.DestroyImmediate(go);}}
 [Test] public void DaysBecomeMeasurablyMoreIntense(){var days=AssetDatabase.FindAssets("t:DayProfile").Select(g=>AssetDatabase.LoadAssetAtPath<DayProfile>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(d=>d.Day).ToArray();Assert.AreEqual(5,days.Length);for(int i=1;i<5;i++){Assert.Greater(days[i].StormIntensity,days[i-1].StormIntensity);Assert.Greater(days[i].FloodRiseRate,days[i-1].FloodRiseRate);Assert.Less(days[i].LightningInterval,days[i-1].LightningInterval);Assert.Greater(days[i].LeakCount,days[i-1].LeakCount);}}
 [Test] public void BuildIncludesBootstrapFirstAndAllEightScenes(){var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).ToArray();Assert.AreEqual(8,scenes.Length);StringAssert.EndsWith("LK_Bootstrap.unity",scenes[0].path);Assert.AreEqual(8,scenes.Select(s=>s.path).Distinct().Count());}
}}
