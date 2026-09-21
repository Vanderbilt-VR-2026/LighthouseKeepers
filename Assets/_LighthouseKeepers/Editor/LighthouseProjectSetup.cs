using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager.UI;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace LighthouseKeepers.Editor
{
    /// <summary>Project-owned, repeatable setup. Never opens or copies the reference project.</summary>
    public static class LighthouseProjectSetup
    {
        public const string Root = "Assets/_LighthouseKeepers";
        [MenuItem("Lighthouse Keepers/01 Configure Quest and Import Starter Assets")]
        public static void Configure()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            UnityEditor.VersionControlSettings.mode = "Visible Meta Files";
            PlayerSettings.productName = "Lighthouse Keepers";
            PlayerSettings.companyName = "Lighthouse Keepers Team";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.lighthousekeepers.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]{GraphicsDeviceType.Vulkan,GraphicsDeviceType.OpenGLES3});
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            player.FindProperty("activeInputHandler").intValue = 1;
            player.FindProperty("androidApplicationEntry").intValue = 2;
            player.ApplyModifiedPropertiesWithoutUndo();
            ConfigureXR(); ConfigureRendering(); ImportSamples();
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Docs/Verification");
            File.WriteAllText("Docs/Verification/QuestSetup.txt", "Quest setup applied through Unity APIs. ARM64 / IL2CPP / API32 / Vulkan+GLES3 / Input System / OpenXR / MSAA4 / render scale1. Android platform switch is performed by the build entry point.\n");
            Debug.Log("LIGHTHOUSE QUEST SETUP PASSED");
        }
        static void ConfigureXR()
        {
            if(!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.settingsKey,out XRGeneralSettingsPerBuildTarget targets))
            {
                targets = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(targets,Root+"/Settings/XR/XRGeneralSettings.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey,targets,true);
            }
            if(!targets.HasSettingsForBuildTarget(BuildTargetGroup.Android)) targets.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
            var android = targets.SettingsForBuildTarget(BuildTargetGroup.Android);
            if(!android.Manager)
            {
                var manager=ScriptableObject.CreateInstance<XRManagerSettings>(); manager.name="Lighthouse Android XR";
                AssetDatabase.AddObjectToAsset(manager,targets); android.Manager=manager;
            }
            android.InitManagerOnStart=true;
            if(!XRPackageMetadataStore.AssignLoader(android.Manager,"UnityEngine.XR.OpenXR.OpenXRLoader",BuildTargetGroup.Android))
                throw new InvalidOperationException("Could not enable Android OpenXR loader.");
            FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
            var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if(!xr) throw new InvalidOperationException("Android OpenXR settings missing.");
            string[] required={"MetaQuestFeature","MetaQuestTouchPlusControllerProfile","OculusTouchControllerProfile"};
            foreach(var feature in xr.GetFeatures())
            {
                feature.enabled=required.Contains(feature.GetType().Name);
                EditorUtility.SetDirty(feature);
            }
            foreach(var type in required) if(!xr.GetFeatures().Any(f=>f.GetType().Name==type && f.enabled)) throw new InvalidOperationException("Missing OpenXR feature: "+type);
            xr.renderMode=OpenXRSettings.RenderMode.SinglePassInstanced;
            EditorUtility.SetDirty(xr); EditorUtility.SetDirty(android); EditorUtility.SetDirty(android.Manager); EditorUtility.SetDirty(targets);
            // OpenXR 1.18 predicates inspect the active platform even when passed Android.
            // Defer their validation until the Android build entry point selects that platform.
            if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
            {
                var rules=new List<OpenXRFeature.ValidationRule>();
                OpenXRProjectValidation.GetCurrentValidationIssues(rules,BuildTargetGroup.Android);
                foreach(var rule in rules) if(rule.fixItAutomatic && rule.fixIt!=null) rule.fixIt();
            }
        }
        static void ConfigureRendering()
        {
            string rpPath=Root+"/Settings/RenderPipeline/QuestPipeline.asset";
            string rendererPath=Root+"/Settings/RenderPipeline/QuestRenderer.asset";
            // Clone this project's template assets, retaining originals and all user edits.
            if(!File.Exists(rendererPath)) AssetDatabase.CopyAsset("Assets/Settings/Mobile_Renderer.asset",rendererPath);
            if(!File.Exists(rpPath)) AssetDatabase.CopyAsset("Assets/Settings/Mobile_RPAsset.asset",rpPath);
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            foreach(var feature in renderer.rendererFeatures) if(feature) feature.SetActive(false);
            var rendererSo=new SerializedObject(renderer); rendererSo.FindProperty("m_RenderingMode").intValue=0; rendererSo.ApplyModifiedPropertiesWithoutUndo();
            var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(rpPath);
            rp.renderScale=1; rp.msaaSampleCount=4; rp.shadowDistance=20; rp.mainLightShadowmapResolution=1024; rp.supportsHDR=false;
            var settings=new SerializedObject(rp);
            settings.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue=renderer;
            settings.FindProperty("m_AdditionalLightsRenderingMode").intValue=1;
            settings.FindProperty("m_AdditionalLightsPerObjectLimit").intValue=2;
            settings.FindProperty("m_AdditionalLightShadowsSupported").boolValue=false;
            settings.ApplyModifiedPropertiesWithoutUndo();
            GraphicsSettings.defaultRenderPipeline=rp;
            int previous=QualitySettings.GetQualityLevel();
            for(int i=0;i<QualitySettings.names.Length;i++) {QualitySettings.SetQualityLevel(i);QualitySettings.renderPipeline=rp;}
            QualitySettings.SetQualityLevel(previous); QualitySettings.vSyncCount=0;
            EditorUtility.SetDirty(rp);EditorUtility.SetDirty(renderer);
        }
        static void ImportSamples()
        {
            var sample=Sample.FindByPackage("com.unity.xr.interaction.toolkit","3.6.0").FirstOrDefault(s=>s.displayName=="Starter Assets");
            if(string.IsNullOrEmpty(sample.displayName)) throw new InvalidOperationException("XRI Starter Assets sample unavailable.");
            if(!sample.isImported && !sample.Import((Sample.ImportOptions)0)) throw new InvalidOperationException("Starter Assets import failed.");
            if(AssetDatabase.FindAssets("t:TMP_Settings").Length==0)
            {
                string ugui=UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui").resolvedPath;
                AssetDatabase.ImportPackage(Path.Combine(ugui,"Package Resources/TMP Essential Resources.unitypackage"),false);
            }
        }
    }
}
