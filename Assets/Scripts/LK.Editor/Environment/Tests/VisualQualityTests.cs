using System;using System.Linq;using NUnit.Framework;using UnityEngine;using UnityEditor;
namespace LighthouseKeepers.Editor.Environment.Tests {
 public sealed class VisualQualityTests {
  [Test]public void AndroidPipelineUsesMeasuredTwoSampleFallbackAndExplicitScale(){var asset=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/_LighthouseKeepers/Settings/RenderPipeline/QuestPipeline.asset");var so=new SerializedObject(asset);Assert.AreEqual(2,so.FindProperty("m_MSAA").intValue);Assert.That(so.FindProperty("m_RenderScale").floatValue,Is.EqualTo(1f).Within(.001f));Assert.AreEqual(asset,UnityEngine.QualitySettings.GetRenderPipelineAssetAt(0));}
  [Test]public void AuthoredSceneSurfacesDoNotFightForTheSamePlane(){var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("RenderAudit")).First(t=>t!=null);string path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"LK-CoplanarRegression");string result=(string)type.GetMethod("Run").Invoke(null,new object[]{path,false});Assert.That(result,Does.Contain("issues=0 "),System.IO.File.ReadAllText(System.IO.Path.Combine(path,"coplanar.txt")));}
 }
}
