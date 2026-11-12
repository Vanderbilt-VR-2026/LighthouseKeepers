using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;

namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class QualityBaselineTests
    {
        [Test] public void AndroidQuestSettingsAndWorldTextureImportsMatchFieldManual()
        {
            EditorUtility.audioMasterMute=true;
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("QuestQualityBaseline")).First(t=>t!=null);
            var errors=(string)type.GetMethod("Validate",BindingFlags.Public|BindingFlags.Static).Invoke(null,null);
            Assert.That(errors,Is.Empty,errors);
        }
        [Test] public void RuntimeNeverUsesViewportScalingAndEnablesFixedFoveation()
        {
            var path="Assets/WatchDemo/Runtime/QuestRenderQuality.cs";
            string source=System.IO.File.ReadAllText(path);
            Assert.That(source,Does.Contain("foveatedRenderingLevel"));
            foreach(var file in System.IO.Directory.GetFiles("Assets","*.cs",System.IO.SearchOption.AllDirectories).Where(f=>!f.Contains("/Editor/")&&!f.Contains("/Tests/")))
                Assert.That(System.IO.File.ReadAllText(file),Does.Not.Contain("XRSettings.renderViewportScale"),file);
        }
    }
}
