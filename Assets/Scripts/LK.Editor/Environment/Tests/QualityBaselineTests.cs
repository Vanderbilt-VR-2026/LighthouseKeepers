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
        [Test] public void AuthoredLabHasBakedFixturesAndRouteProbes()
        {
            EditorUtility.audioMasterMute=true;
            const string path="Assets/Scenes/Lab/LK_Lab_Environment.unity";
            var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
            bool opened=!scene.isLoaded;
            if(opened)scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path,UnityEditor.SceneManagement.OpenSceneMode.Additive);
            try
            {
                var roots=scene.GetRootGameObjects();
                var lights=roots.SelectMany(g=>g.GetComponentsInChildren<UnityEngine.Light>(true)).ToArray();
                Assert.That(lights,Is.Not.Empty);
                Assert.That(lights.All(l=>l.lightmapBakeType==UnityEngine.LightmapBakeType.Baked),Is.True,"Fixed Lab lights must be baked; future handheld lanterns need an explicit exception.");
                var probes=roots.Single(g=>g.name=="Quest quality route probes").GetComponent<UnityEngine.LightProbeGroup>();
                Assert.That(probes.probePositions.Length,Is.GreaterThanOrEqualTo(42),"Stairs and outside approach need probe coverage.");
            }
            finally{if(opened)UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);}
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
