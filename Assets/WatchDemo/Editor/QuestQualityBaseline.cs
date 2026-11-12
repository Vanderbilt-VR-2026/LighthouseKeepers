using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

// §14: one repeatable authoring entry. Never alter rendering resolution during play.
public static class QuestQualityBaseline
{
    public const string PipelinePath = "Assets/_LighthouseKeepers/Settings/RenderPipeline/QuestPipeline.asset";
    public const string RendererPath = "Assets/_LighthouseKeepers/Settings/RenderPipeline/QuestRenderer.asset";
    public const string Output = "artifacts/s5/quality";
    public static UniversalRenderPipelineAsset Pipeline => AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
    public static UniversalRendererData Renderer => AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
    static SerializedObject Quality => new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
    static void Set(UnityEngine.Object o, string field, object value)
    {
        var s = new SerializedObject(o); var p = s.FindProperty(field);
        if(p == null) throw new InvalidOperationException(o.name + " missing serialized field " + field);
        if(value is bool b) p.boolValue=b; else if(value is int i) p.intValue=i;
        else if(value is float f) p.floatValue=f; else p.objectReferenceValue=(UnityEngine.Object)value;
        s.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(o);
    }
    public static string[] TexturePaths() => AssetDatabase.FindAssets("t:Texture", new[]{"Assets/Art", "Assets/WatchDemo", "Assets/Sprint5"}.Where(AssetDatabase.IsValidFolder).ToArray())
        .Select(AssetDatabase.GUIDToAssetPath).Where(p=>AssetImporter.GetAtPath(p) is TextureImporter).Distinct().ToArray();
    public static bool IsWorldTexture(TextureImporter t) => t.textureType != TextureImporterType.Sprite &&
        t.textureType != TextureImporterType.GUI && !t.assetPath.Contains("/Fonts/") && !t.assetPath.Contains("/TextMesh Pro/");
    public static void Audit(string filename)
    {
        Directory.CreateDirectory(Output);
        var p = new SerializedObject(Pipeline); var r = new SerializedObject(Renderer);
        var lines = new List<string>{"UTC " + DateTime.UtcNow.ToString("O"), "Android default: " + File.ReadLines("ProjectSettings/QualitySettings.asset").Last(l=>l.TrimStart().StartsWith("Android:"))};
        foreach(var name in new[]{"m_MSAA","m_RenderScale","m_SupportsHDR","m_RequireDepthTexture","m_RequireOpaqueTexture","m_MainLightRenderingMode","m_MainLightShadowsSupported","m_MainLightShadowmapResolution","m_ShadowDistance","m_ShadowCascadeCount","m_AdditionalLightsRenderingMode","m_AdditionalLightsPerObjectLimit","m_AdditionalLightShadowsSupported","m_SoftShadowsSupported"})
        {var v=p.FindProperty(name);lines.Add(name+" = "+(v.propertyType==SerializedPropertyType.Boolean?v.boolValue.ToString():v.propertyType==SerializedPropertyType.Float?v.floatValue.ToString():v.intValue.ToString()));}
        lines.Add("renderer="+EditorJsonUtility.ToJson(Renderer));
        lines.Add("OpenXR Android="+EditorJsonUtility.ToJson(OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android)));
        lines.Add("APIs="+string.Join(",",PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)));
        foreach(var path in TexturePaths()) {var t=(TextureImporter)AssetImporter.GetAtPath(path);var a=t.GetPlatformTextureSettings("Android");lines.Add($"texture {path}: world={IsWorldTexture(t)} mip={t.mipmapEnabled} aniso={t.anisoLevel} override={a.overridden} format={a.format}");}
        File.WriteAllLines(Path.Combine(Output,filename),lines);
    }
    public static void ApplyFromMenu(){if(!EditorApplication.ExecuteMenuItem("Lighthouse Keepers/Apply Quest Quality Baseline"))throw new Exception("Quality menu unavailable");var errors=Validate();if(errors.Length>0)throw new Exception(errors);}
    [MenuItem("Lighthouse Keepers/Apply Quest Quality Baseline")]
    public static void Apply()
    {
        EditorUtility.audioMasterMute=true;
        if(!File.Exists(Path.Combine(Output,"before.txt")))Audit("before.txt");
        var p=Pipeline;
        p.supportsHDR=false;p.msaaSampleCount=4;p.renderScale=1f;p.supportsCameraDepthTexture=false;p.supportsCameraOpaqueTexture=false;
        p.shadowDistance=15;p.shadowCascadeCount=1;p.mainLightShadowmapResolution=1024;Set(p,"m_MainLightShadowsSupported",false);Set(p,"m_SoftShadowsSupported",false);
        Set(p,"m_AdditionalLightsRenderingMode",(int)LightRenderingMode.PerPixel);p.maxAdditionalLightsCount=2;Set(p,"m_AdditionalLightShadowsSupported",false);
        Set(p,"m_MainLightRenderingMode",1);Set(p,"m_UseAdaptivePerformance",false);EditorUtility.SetDirty(p);
        // Only level zero is selectable on Android; do not downgrade the desktop Editor quality.
        var q=Quality;var levels=q.FindProperty("m_QualitySettings");
        for(int i=0;i<levels.arraySize;i++) {
            var level=levels.GetArrayElementAtIndex(i);var excluded=level.FindPropertyRelative("excludedTargetPlatforms");
            bool androidExcluded=Enumerable.Range(0,excluded.arraySize).Any(n=>excluded.GetArrayElementAtIndex(n).stringValue=="Android");
            if(!androidExcluded) {level.FindPropertyRelative("customRenderPipeline").objectReferenceValue=p;level.FindPropertyRelative("antiAliasing").intValue=4;level.FindPropertyRelative("anisotropicTextures").intValue=(int)AnisotropicFiltering.Enable;}
        }
        q.ApplyModifiedPropertiesWithoutUndo();
        Set(Renderer,"m_RenderingMode",0);Set(Renderer,"m_DepthPrimingMode",0);Renderer.postProcessData=null;
        foreach(var feature in Renderer.rendererFeatures.Where(f=>f && !(f is OnTilePostProcessFeature)))feature.SetActive(false);
        if(!Renderer.rendererFeatures.Any(f=>f is OnTilePostProcessFeature)) {
            var feature=ScriptableObject.CreateInstance<OnTilePostProcessFeature>();feature.name="On Tile Post Processing";
            AssetDatabase.AddObjectToAsset(feature,Renderer);Renderer.rendererFeatures.Add(feature);
        }
        EditorUtility.SetDirty(Renderer);
        // Unity 6.3 without URP_COMPATIBILITY_MODE always runs Render Graph; its setter is intentionally unavailable.
        if(GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>().enableRenderCompatibilityMode)throw new Exception("Remove URP_COMPATIBILITY_MODE before applying the Quest baseline.");
        // Only the two permitted grade components are present. No vignette, grain or classic effects.
        const string profilePath="Assets/Settings/QuestOnTileVolume.asset";
        var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,profilePath);var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.Neutral);var grade=profile.Add<ColorAdjustments>();grade.postExposure.Override(0);foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);}
        Set(p,"m_VolumeProfile",profile);EditorUtility.SetDirty(profile);
        var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        Set(xr,"m_renderMode",1);Set(xr,"m_latencyOptimization",1);Set(xr,"m_depthSubmissionMode",0);Set(xr,"m_foveatedRenderingApi",1);
        // Projection/culling optimizations stay at their measured prior values until a worn A/B check.
        var foveation=xr.GetFeature<FoveatedRenderingFeature>();if(!foveation)throw new Exception("Missing Android foveation feature");foveation.enabled=true;Set(foveation,"useEyeTracking",false);
        foreach(var f in xr.GetFeatures<OpenXRFeature>())if(f.GetType().Name.Contains("AutomaticDynamicResolution")){f.enabled=false;EditorUtility.SetDirty(f);}
        var ed=AssetDatabase.LoadAllAssetsAtPath("Assets/XR/Settings/OpenXR Editor Settings.asset").First();Set(ed,"m_vulkanOffscreenSwapchainNoMainDisplay",true);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.Vulkan});
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        int changed=0;
        foreach(var path in TexturePaths()) {
            var t=(TextureImporter)AssetImporter.GetAtPath(path);if(!IsWorldTexture(t))continue;
            var a=t.GetPlatformTextureSettings("Android");var format=t.textureType==TextureImporterType.NormalMap?TextureImporterFormat.ASTC_4x4:TextureImporterFormat.ASTC_6x6;
            int aniso=path.IndexOf("wood",StringComparison.OrdinalIgnoreCase)>=0||path.IndexOf("wall",StringComparison.OrdinalIgnoreCase)>=0||path.IndexOf("stone",StringComparison.OrdinalIgnoreCase)>=0||path.IndexOf("floor",StringComparison.OrdinalIgnoreCase)>=0?4:2;
            if(t.mipmapEnabled&&t.anisoLevel>=aniso&&a.overridden&&a.format==format)continue;
            t.mipmapEnabled=true;t.anisoLevel=aniso;a.overridden=true;a.format=format;t.SetPlatformTextureSettings(a);t.SaveAndReimport();changed++;
        }
        AssetDatabase.SaveAssets();Audit("after.txt");
        Debug.Log($"QUEST_QUALITY_APPLIED: QuestPipeline MSAA4 scale1 Vulkan; textures changed={changed}; device GPU pending.");
    }
    public static string Validate()
    {
        var errors=new List<string>();void Check(bool ok,string text){if(!ok)errors.Add(text);}
        var p=Pipeline;var r=Renderer;
        Check(p&&!p.supportsHDR&&p.msaaSampleCount==4&&Mathf.Approximately(p.renderScale,1),"HDR/MSAA/render scale");
        Check(!p.supportsCameraDepthTexture&&!p.supportsCameraOpaqueTexture,"Depth/Opaque copy");
        Check(p.additionalLightsRenderingMode==LightRenderingMode.PerPixel&&p.maxAdditionalLightsCount<=2&&!p.supportsAdditionalLightShadows,"Lantern light exception");
        Check(p.shadowDistance<=15&&p.shadowCascadeCount==1&&!p.supportsMainLightShadows&&!p.supportsSoftShadows,"Main shadows");
        var rs=new SerializedObject(r);Check(rs.FindProperty("m_RenderingMode").intValue==0&&rs.FindProperty("m_DepthPrimingMode").intValue==0&&!r.postProcessData,"Forward/depth priming/classic post");
        Check(r.rendererFeatures.Count(f=>f&&f.isActive&&f is OnTilePostProcessFeature)==1&&!r.rendererFeatures.Any(f=>f&&f.isActive&&!(f is OnTilePostProcessFeature)),"Only on-tile feature");
        Check(File.ReadLines("ProjectSettings/QualitySettings.asset").Last(l=>l.TrimStart().StartsWith("Android:")).Trim()=="Android: 0", "Android default level zero");
        Check(QualitySettings.GetRenderPipelineAssetAt(0)==p,"Quest quality asset");
        var levels=Quality.FindProperty("m_QualitySettings");
        for(int i=0;i<levels.arraySize;i++){var level=levels.GetArrayElementAtIndex(i);var excluded=level.FindPropertyRelative("excludedTargetPlatforms");bool android= !Enumerable.Range(0,excluded.arraySize).Any(n=>excluded.GetArrayElementAtIndex(n).stringValue=="Android");if(android)Check(level.FindPropertyRelative("customRenderPipeline").objectReferenceValue==p&&level.FindPropertyRelative("anisotropicTextures").intValue==(int)AnisotropicFiltering.Enable,"Selectable Android quality "+i);}
        Check(!GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>().enableRenderCompatibilityMode,"Render Graph enabled");
        var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);var x=new SerializedObject(xr);
        Check(x.FindProperty("m_renderMode").intValue==1&&x.FindProperty("m_latencyOptimization").intValue==1&&x.FindProperty("m_depthSubmissionMode").intValue==0,"OpenXR rendering/input/depth");
        Check(xr.GetFeature<FoveatedRenderingFeature>().enabled,"Foveation enabled");
        Check(!xr.GetFeatures<OpenXRFeature>().Any(f=>f.enabled&&f.GetType().Name.Contains("AutomaticDynamicResolution")),"No viewport dynamic resolution");
        var offscreen=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("Assets/XR/Settings/OpenXR Editor Settings.asset").First());Check(offscreen.FindProperty("m_vulkanOffscreenSwapchainNoMainDisplay").boolValue,"Offscreen rendering");
        var ffr=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/WatchDemo/Resources/VisualPassSettings.asset");var fs=new SerializedObject(ffr);Check(fs.FindProperty("renderQuality").boolValue&&fs.FindProperty("fixedFoveation").floatValue>=.5f&&fs.FindProperty("fixedFoveation").floatValue<=.75f,"Runtime fixed foveation policy");
        Check(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).SequenceEqual(new[]{GraphicsDeviceType.Vulkan}),"Vulkan only");
        Check(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)==ScriptingImplementation.IL2CPP&&PlayerSettings.Android.targetArchitectures==AndroidArchitecture.ARM64,"IL2CPP ARM64");
        foreach(var path in TexturePaths()){var t=(TextureImporter)AssetImporter.GetAtPath(path);if(!IsWorldTexture(t))continue;var a=t.GetPlatformTextureSettings("Android");Check(t.mipmapEnabled&&a.overridden&&a.format.ToString().StartsWith("ASTC_"),"Texture mip/ASTC: "+path);}
        return string.Join("\n",errors);
    }
}
public sealed class QuestQualityBuildGuard:IPreprocessBuildWithReport
{
    public int callbackOrder=>11000;
    public void OnPreprocessBuild(BuildReport report){if(report.summary.platform!=BuildTarget.Android)return;var errors=QuestQualityBaseline.Validate();if(errors.Length>0)throw new BuildFailedException("Apply Quest Quality Baseline before building:\n"+errors);}
}
