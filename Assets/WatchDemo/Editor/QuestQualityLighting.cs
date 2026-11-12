using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Editor.Environment;
using LighthouseKeepers.Presentation.Environment;

public static class QuestQualityLighting
{
    [MenuItem("Lighthouse Keepers/Quality/Rebake Quest Lighting")]
    public static void Bake()
    {
        EditorUtility.audioMasterMute=true;
        EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");
        var group=UnityEngine.Object.FindAnyObjectByType<BeaconAndMainsLightingGroup>();
        if(!group)throw new Exception("Missing existing lighting variant controller");
        group.enabled=false;
        foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            if(r.GetComponentInParent<Rigidbody>()||r.GetComponentInParent<Animator>())continue;
            var flags=GameObjectUtility.GetStaticEditorFlags(r.gameObject);
            if(r.gameObject.isStatic||r.name.StartsWith("SM_"))GameObjectUtility.SetStaticEditorFlags(r.gameObject,flags|StaticEditorFlags.ContributeGI);
        }
        var old=GameObject.Find("Quest quality route probes");if(!old)old=new GameObject("Quest quality route probes");
        var probe=old.GetComponent<LightProbeGroup>();if(!probe)probe=old.AddComponent<LightProbeGroup>();var points=new List<Vector3>();
        // Stair route and outdoor House approach supplement existing room grids.
        for(int floor=0;floor<4;floor++)for(int i=0;i<8;i++){float angle=(i*45+15)*Mathf.Deg2Rad;points.Add(new Vector3(Mathf.Cos(angle)*3.2f,floor*3.3f+1.4f,Mathf.Sin(angle)*3.2f));}
        for(int i=0;i<10;i++)points.Add(new Vector3(-3.8f+i*.8f,1.3f,-11.8f));probe.probePositions=points.ToArray();
        var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .Where(r=>(GameObjectUtility.GetStaticEditorFlags(r.gameObject)&StaticEditorFlags.ContributeGI)!=0)
            .OrderBy(r=>EnvironmentLightingBake.PathOf(r.transform),StringComparer.Ordinal).ToArray();
        var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var original=lights.Select(l=>(l.enabled,l.color,l.intensity)).ToArray();
        foreach(var l in lights){l.lightmapBakeType=LightmapBakeType.Baked;l.shadows=LightShadows.Soft;}
        var settings=Lightmapping.lightingSettings;
        if(!settings)throw new Exception("Missing existing LightingSettings");
        settings.lightmapper=LightingSettings.Lightmapper.ProgressiveGPU;settings.bakedGI=true;settings.realtimeGI=false;
        settings.lightmapResolution=10;settings.lightmapMaxSize=1024;settings.directSampleCount=64;settings.indirectSampleCount=128;settings.environmentSampleCount=64;
        EditorUtility.SetDirty(settings);
        var capture=typeof(EnvironmentLightingBake).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static);
        var variants=new List<EnvironmentLightingPreset>();var stats=new List<string>();Directory.CreateDirectory(QuestQualityBaseline.Output);
        foreach(bool powered in new[]{true,false}) {
            for(int i=0;i<lights.Length;i++){lights[i].enabled=original[i].enabled;lights[i].color=powered?original[i].color:Color.Lerp(original[i].color,new Color(.5f,.09f,.035f),.6f);lights[i].intensity=original[i].intensity*(powered?1:.12f);}
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            var start=DateTime.UtcNow;bool baked=Lightmapping.Bake();
            if(!baked&&Application.platform==RuntimePlatform.OSXEditor){Debug.LogWarning("ProgressiveGPU unavailable on this Mac; documented ProgressiveCPU bake fallback, not a Quest performance measurement.");settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;baked=Lightmapping.Bake();}
            if(!baked)throw new Exception("Quest lighting bake failed: "+(powered?"Powered":"Emergency")+". See log; do not ship stale maps.");
            var preset=(EnvironmentLightingPreset)capture.Invoke(null,new object[]{powered?"Powered":"Emergency",renderers});variants.Add(preset);
            long bytes=preset.colorMaps.Concat(preset.directionMaps).Concat(preset.shadowMasks).Where(t=>t).Sum(t=>new FileInfo(AssetDatabase.GetAssetPath(t)).Length);
            stats.Add($"{preset.name}: maps={preset.colorMaps.Length}, filesMiB={bytes/1048576f:F2}, seconds={(DateTime.UtcNow-start).TotalSeconds:F1}, requested=ProgressiveGPU, settingsAfter={settings.lightmapper}, probes={LightmapSettings.lightProbes?.count}, staticRenderers={renderers.Length}");
        }
        for(int i=0;i<lights.Length;i++){lights[i].enabled=original[i].enabled;lights[i].color=original[i].color;lights[i].intensity=original[i].intensity;}
        var so=new SerializedObject(group);so.FindProperty("poweredPreset").objectReferenceValue=variants[0];so.FindProperty("emergencyPreset").objectReferenceValue=variants[1];
        var a=so.FindProperty("bakedRenderers");a.arraySize=renderers.Length;for(int i=0;i<renderers.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=renderers[i];so.ApplyModifiedPropertiesWithoutUndo();
        EnvironmentLightingBake.Apply(variants[0],renderers);group.enabled=true;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        File.WriteAllLines(Path.Combine(QuestQualityBaseline.Output,"bake.txt"),stats);Debug.Log("QUEST_BAKE_FINISHED "+string.Join("; ",stats));
    }
}
