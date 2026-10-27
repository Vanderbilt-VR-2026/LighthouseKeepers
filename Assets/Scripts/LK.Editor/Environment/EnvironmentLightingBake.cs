using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.Editor.Environment
{
    public static class EnvironmentLightingBake
    {
        public static void BakeAndValidate()
        {
            ShellBuildCache.VerifyTwice();BakeLab();
            if(!File.Exists("Assets/Settings/Lighting/LuminanceCalibration.json"))LuminanceValidator.CalibratePoweredReference();
            LuminanceValidator.Run();LuminanceValidator.CaptureViews();EnvironmentValidation.RunGeometry();
        }
        public static void BakeLab()
        {
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
            foreach(var old in UnityEngine.Object.FindObjectsByType<BeaconAndMainsLightingGroup>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(old.gameObject);
            var settings=new LightingSettings {name="Environment Lab lighting",bakedGI=true,realtimeGI=false,lightmapper=LightingSettings.Lightmapper.ProgressiveCPU,lightmapResolution=20,lightmapMaxSize=1024,directSampleCount=256,indirectSampleCount=512,environmentSampleCount=256};
            string settingsPath="Assets/Settings/Lighting/LabLightingSettings.lighting";
            var existing=AssetDatabase.LoadAssetAtPath<LightingSettings>(settingsPath);
            if(existing){EditorUtility.CopySerialized(settings,existing);UnityEngine.Object.DestroyImmediate(settings);settings=existing;}else AssetDatabase.CreateAsset(settings,settingsPath);
            Lightmapping.lightingSettings=settings;
            var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.gameObject.isStatic).OrderBy(r=>PathOf(r.transform),StringComparer.Ordinal).ToArray();
            var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            EnvironmentLightingPreset powered=null,emergency=null;
            var lampMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Environment/M_Environment_Lamp.mat");
            var emission=lampMaterial.GetColor("_EmissionColor");
            foreach(bool on in new[]{true,false})
            {
                lampMaterial.SetColor("_EmissionColor",on?emission:Color.black);
                foreach(var l in lights)l.enabled=on||l.name.StartsWith("Emergency",StringComparison.Ordinal)||l.name.StartsWith("Oil",StringComparison.Ordinal);
                RenderSettings.ambientLight=on?new Color(.22f,.24f,.27f):new Color(.045f,.055f,.07f);
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Settings/Lighting/LK_PoweredBake.unity");
                if(!Lightmapping.Bake())throw new Exception("Lightmap bake failed");
                var preset=Capture(on?"Powered":"Emergency",renderers);
                if(on)powered=preset;else emergency=preset;
            }
            lampMaterial.SetColor("_EmissionColor",emission);
            var group=new GameObject("Environment lighting presentation — Eli supplies Powered").AddComponent<BeaconAndMainsLightingGroup>();
            var so=new SerializedObject(group);so.FindProperty("poweredPreset").objectReferenceValue=powered;so.FindProperty("emergencyPreset").objectReferenceValue=emergency;
            var arr=so.FindProperty("bakedRenderers");arr.arraySize=renderers.Length;for(int i=0;i<renderers.Length;i++)arr.GetArrayElementAtIndex(i).objectReferenceValue=renderers[i];var emitters=renderers.Where(r=>r.name.StartsWith("SM_Lamp_",StringComparison.Ordinal)&&r.name!="SM_Lamp_House_N24").ToArray();
            var em=so.FindProperty("mainsEmitters");em.arraySize=emitters.Length;for(int i=0;i<emitters.Length;i++)em.GetArrayElementAtIndex(i).objectReferenceValue=emitters[i];so.ApplyModifiedPropertiesWithoutUndo();
            Apply(powered,renderers);foreach(var l in lights)l.enabled=true;
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),LighthouseBuilder.LabPath);AssetDatabase.SaveAssets();Debug.Log("LAB_LIGHTING_BAKES_PASS");
        }
        public static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
        static EnvironmentLightingPreset Capture(string name,MeshRenderer[] renderers)
        {
            string dir="Assets/Settings/Lighting/"+name;Directory.CreateDirectory(dir);AssetDatabase.Refresh();
            string path="Assets/Settings/Lighting/LightingPreset_"+name+".asset";
            var p=AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>(path);if(!p){p=ScriptableObject.CreateInstance<EnvironmentLightingPreset>();AssetDatabase.CreateAsset(p,path);}
            var maps=LightmapSettings.lightmaps;p.colorMaps=new Texture2D[maps.Length];p.directionMaps=new Texture2D[maps.Length];p.shadowMasks=new Texture2D[maps.Length];
            for(int i=0;i<maps.Length;i++){p.colorMaps[i]=Copy(maps[i].lightmapColor,dir,i,"Color");p.directionMaps[i]=Copy(maps[i].lightmapDir,dir,i,"Direction");p.shadowMasks[i]=Copy(maps[i].shadowMask,dir,i,"Shadow");}
            p.rendererIndices=renderers.Select(r=>r.lightmapIndex).ToArray();p.rendererScaleOffsets=renderers.Select(r=>r.lightmapScaleOffset).ToArray();var probes=LightmapSettings.lightProbes?LightmapSettings.lightProbes.bakedProbes:new UnityEngine.Rendering.SphericalHarmonicsL2[0];p.probeCoefficients=new float[probes.Length*27];for(int q=0;q<probes.Length;q++)for(int c=0;c<3;c++)for(int k=0;k<9;k++)p.probeCoefficients[q*27+c*9+k]=probes[q][c,k];
            p.mode=LightmapSettings.lightmapsMode;p.ambient=RenderSettings.ambientLight;EditorUtility.SetDirty(p);AssetDatabase.SaveAssets();return p;
        }
        static Texture2D Copy(Texture2D tex,string dir,int index,string kind)
        {
            if(!tex)return null;string source=AssetDatabase.GetAssetPath(tex),target=dir+"/"+index+"-"+kind+Path.GetExtension(source);
            if(File.Exists(target)){File.Copy(source,target,true);AssetDatabase.ImportAsset(target,ImportAssetOptions.ForceUpdate);}else if(!AssetDatabase.CopyAsset(source,target))throw new Exception("Lightmap copy failed "+source);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(target);
        }
        public static void Apply(EnvironmentLightingPreset p,MeshRenderer[] renderers)
        {
            LightmapSettings.lightmaps=p.colorMaps.Select((t,i)=>new LightmapData{lightmapColor=t,lightmapDir=p.directionMaps[i],shadowMask=p.shadowMasks[i]}).ToArray();LightmapSettings.lightmapsMode=p.mode;
            for(int i=0;i<renderers.Length;i++){renderers[i].lightmapIndex=p.rendererIndices[i];renderers[i].lightmapScaleOffset=p.rendererScaleOffsets[i];}RenderSettings.ambientLight=p.ambient;
            if(LightmapSettings.lightProbes&&p.probeCoefficients!=null){var probes=new UnityEngine.Rendering.SphericalHarmonicsL2[p.probeCoefficients.Length/27];for(int q=0;q<probes.Length;q++)for(int c=0;c<3;c++)for(int k=0;k<9;k++)probes[q][c,k]=p.probeCoefficients[q*27+c*9+k];if(probes.Length==LightmapSettings.lightProbes.count)LightmapSettings.lightProbes.bakedProbes=probes;}

        }
    }
}
