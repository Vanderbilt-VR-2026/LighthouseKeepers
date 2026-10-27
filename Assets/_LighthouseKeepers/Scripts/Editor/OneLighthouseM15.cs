using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.WatchDemo;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Editor
{
    public static class OneLighthouseM15
    {
        public const string Output = "artifacts/one/M1.5";
        public static void Apply()
        {
            Directory.CreateDirectory(Output);
            ConfigureHandMaterial();
            var log = new StringBuilder();
            foreach (string path in LighthouseNetworkLobby.GameScenes)
            {
                var scene = EditorSceneManager.OpenScene(path);
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (PrefabUtility.IsPartOfPrefabInstance(root) && root.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.TunnelingVignetteController>(true).Length > 0)
                        PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    if (OneLighthouseM1.RemoveVignette(root)) log.AppendLine("REMOVE vignette " + root.name);
                    foreach (var origin in root.GetComponentsInChildren<XROrigin>(true))
                    { OneLighthouseM1.InstallHands(origin.gameObject); log.AppendLine("HANDS " + path + " / " + origin.name); }
                    if (path == LighthouseNetworkLobby.ScenePath) continue;
                    if (root.GetComponent<Fusion.FusionBootstrap>() || root.GetComponent<Fusion.NetworkRunner>())
                    { Object.DestroyImmediate(root); continue; }
                    foreach (var board in root.GetComponentsInChildren<LighthouseKeepers.StormWatch.StormWatchBoard>(true)) Object.DestroyImmediate(board.gameObject);
                    foreach (var diagnostic in root.GetComponentsInChildren<LighthouseKeepers.Core.DevelopmentDiagnostics>(true)) Object.DestroyImmediate(diagnostic);
                    foreach (var light in root.GetComponentsInChildren<Light>(true))
                    {
                        log.AppendLine($"LIGHT {light.name} color={light.color} intensity={light.intensity}");
                        if (light.name.IndexOf("beacon", StringComparison.OrdinalIgnoreCase) >= 0 || light.name.IndexOf("lantern", StringComparison.OrdinalIgnoreCase) >= 0)
                        { light.color = new Color(1f, .64f, .20f); light.useColorTemperature = false; }
                    }
                    foreach (var text in root.GetComponentsInChildren<TextMesh>(true)) log.AppendLine("KEEP authored text " + text.name + " : " + text.text.Replace('\n','|'));
                    foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        if (renderer.name.IndexOf("beacon", StringComparison.OrdinalIgnoreCase) < 0 && renderer.name.IndexOf("lantern", StringComparison.OrdinalIgnoreCase) < 0) continue;
                        log.AppendLine("BEACON renderer " + renderer.name + " materials=" + string.Join(",", renderer.sharedMaterials.Select(m=>m ? m.name : "NULL")));
                    }
                }
                if (path != LighthouseNetworkLobby.ScenePath && !Object.FindFirstObjectByType<WatchDemoController>()) throw new InvalidOperationException("Restored WatchDemo missing.");
                if(path != LighthouseNetworkLobby.ScenePath)
                {
                    LighthouseKeepers.WatchDemo.Editor.WatchHouseAuthoring.MountBoard();
                    LighthouseKeepers.WatchDemo.Editor.WatchHouseAuthoring.CleanHouse();
                    LighthouseKeepers.WatchDemo.Editor.WatchHouseAuthoring.FitWrench();
                    LighthouseKeepers.WatchDemo.Editor.WatchHouseAuthoring.CleanHud();
                    var board=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<TextMesh>(true)).Single(t=>t.name=="Watch checklist");
                    scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<TextMesh>(true)).Single(t=>t.name=="Repair feedback").characterSize=.012f;
                    board.color=new Color(.012f,.018f,.014f); // Dark ink remains legible against the lit board face.
                    log.AppendLine("BOARD: retain original text/layout; darken ink for contrast in restored lighting.");
                    var origin=Object.FindFirstObjectByType<XROrigin>();
                    var face=GameObject.Find("Wall-mounted checklist white face").transform;
                    origin.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(face.position-origin.transform.position,Vector3.up));
                    PrefabUtility.RecordPrefabInstancePropertyModifications(origin.transform);
                    log.AppendLine("HOUSE: initial rig heading faces the existing board; preserve spawn position and tracked camera.");
                }
                EditorSceneManager.SaveScene(scene);
            }
            var config = AssetDatabase.LoadAssetAtPath<LobbyConfig>("Assets/_LighthouseKeepers/Settings/NetworkLobbyConfig.asset");
            LighthouseScenes.Set(config, "startSceneName", ShipWorld.GameplaySceneName);
            LighthouseScenes.Set(config, "minPlayersToStart", 1);
            OneLighthouseM1.RemoveVolumeVignettes(); OneLighthouseM1.ApplyShipBuildScenes(); AssetDatabase.SaveAssets();
            File.WriteAllText(Output + "/authoring.txt", log.ToString());
            EditorSceneManager.OpenScene(LighthouseNetworkLobby.ScenePath);
        }
        public static void ConfigureHandMaterial()
        {
            var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Hands/Resources/Hands/M_Glove.mat");
            material.shader=Shader.Find("Universal Render Pipeline/Lit");
            var tan=new Color(198f/255,142f/255,106f/255,1);
            material.SetColor("_BaseColor",tan); material.SetColor("_Color",tan);
            material.SetFloat("_Metallic",0); material.SetFloat("_Smoothness",.08f);
            material.SetFloat("_SpecularHighlights",0); material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            material.SetFloat("_EnvironmentReflections",0); material.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            material.SetColor("_EmissionColor",Color.black); material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            // The hand renderer is made at runtime, so preserve its forward material variants explicitly; OpenXR supplies stereo variants.
            const string path="Assets/Art/Hands/Resources/Hands/HandShaders.shadervariants";
            var variants=AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path);
            if(!variants) { variants=new ShaderVariantCollection(); AssetDatabase.CreateAsset(variants,path); }
            variants.Clear();
            foreach(bool additional in new[]{false,true})
            {
                var keywords=new System.Collections.Generic.List<string>{"_SPECULARHIGHLIGHTS_OFF","_ENVIRONMENTREFLECTIONS_OFF"};
                if(additional)keywords.Add("_ADDITIONAL_LIGHTS");
                variants.Add(new ShaderVariantCollection.ShaderVariant(material.shader,PassType.ScriptableRenderPipeline,keywords.ToArray()));
            }
            EditorUtility.SetDirty(variants);
            var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var preloaded=graphics.FindProperty("m_PreloadedShaders");
            bool found=false;for(int i=0;i<preloaded.arraySize;i++)if(preloaded.GetArrayElementAtIndex(i).objectReferenceValue==variants)found=true;
            if(!found) { int i=preloaded.arraySize;preloaded.InsertArrayElementAtIndex(i);preloaded.GetArrayElementAtIndex(i).objectReferenceValue=variants;graphics.ApplyModifiedPropertiesWithoutUndo(); }
            AssetDatabase.SaveAssets();
        }
        public static void Build()
        {
            Directory.CreateDirectory(Output); Directory.CreateDirectory("Builds");
            OneLighthouseM1.ApplyShipBuildScenes();
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = LighthouseNetworkLobby.GameScenes,
                target = BuildTarget.Android, locationPathName = "Builds/LighthouseKeepers-M1.5.apk", options = BuildOptions.Development });
            File.WriteAllText(Output + "/build.txt", $"{report.summary.result}\nErrors={report.summary.totalErrors}\nWarnings={report.summary.totalWarnings}\nBytes={report.summary.totalSize}\nDuration={report.summary.totalTime}\n");
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0) throw new BuildFailedException("M1.5 Android build failed.");
        }
    }
}
