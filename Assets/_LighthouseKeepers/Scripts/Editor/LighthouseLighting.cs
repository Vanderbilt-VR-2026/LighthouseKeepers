using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace LighthouseKeepers.Editor {
public static class LighthouseLighting {
 public static void BakeAndPreview(){Bake();LighthousePlayVerification.Run();}
 public static void Bake(){
  const string root=LighthouseAssets.Root;
  string path=root+"/Art/Lighting/LK_BakedLighting.asset";var settings=AssetDatabase.LoadAssetAtPath<LightingSettings>(path);if(!settings){settings=new LightingSettings();AssetDatabase.CreateAsset(settings,path);}
  settings.bakedGI=true;settings.realtimeGI=false;settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;settings.lightmapResolution=5;settings.lightmapMaxSize=512;settings.lightmapPadding=2;settings.directSampleCount=16;settings.indirectSampleCount=32;settings.environmentSampleCount=32;settings.mixedBakeMode=MixedLightingMode.Subtractive;
  EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));foreach(var name in LighthouseScenes.Names){if(name=="LK_Bootstrap"||name=="LK_DevGym")continue;EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name),OpenSceneMode.Additive);}
  for(int i=0;i<SceneManager.sceneCount;i++){SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));Lightmapping.lightingSettings=settings;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.22f,.30f,.36f);RenderSettings.reflectionIntensity=0;}
  // Baked bounce fills preserve navigability without extra real-time lights on Quest.
  foreach(var name in new[]{"LK_Core","LK_Level01_Plumbing","LK_Level02_Generator","LK_Level03_Communications","LK_Level04_Lantern"}){
   var scene=SceneManager.GetSceneByName(name);SceneManager.SetActiveScene(scene);
   var existing=System.Array.Find(scene.GetRootGameObjects(),g=>g.name=="Baked navigation bounce");if(!existing)existing=new GameObject("Baked navigation bounce");
   var light=existing.GetComponent<Light>();if(!light)light=existing.AddComponent<Light>();light.type=LightType.Point;light.lightmapBakeType=LightmapBakeType.Baked;light.shadows=LightShadows.None;light.range=9;light.intensity=3;light.color=new Color(.52f,.68f,.8f);
   float floor=name.Contains("02")?3.2f:name.Contains("03")?6.4f:name.Contains("04")?9.6f:0;
   existing.transform.position=name=="LK_Core"?new Vector3(-8,2,0):new Vector3(2.5f,floor+2.3f,0);
  }
  SceneManager.SetActiveScene(SceneManager.GetSceneByName("LK_Exterior"));
  bool baked=Lightmapping.Bake();for(int i=0;i<SceneManager.sceneCount;i++)EditorSceneManager.SaveScene(SceneManager.GetSceneAt(i));
  File.WriteAllText("Docs/Verification/Lighting.txt","CPU bake result="+baked+"; lightmaps="+LightmapSettings.lightmaps.Length+"; resolution=5 texels/m; maximum atlas=512.\n");
  string quest=root+"/Settings/RenderPipeline/QuestPipeline.asset",preview=root+"/Settings/RenderPipeline/EditorPreviewPipeline.asset";
  if(!File.Exists(preview))AssetDatabase.CopyAsset(quest,preview);var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(preview);rp.shadowDistance=40;rp.mainLightShadowmapResolution=2048;EditorUtility.SetDirty(rp);
  QualitySettings.SetQualityLevel(0);QualitySettings.renderPipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(quest);QualitySettings.SetQualityLevel(1);QualitySettings.renderPipeline=rp;
  var quality=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);var array=quality.FindProperty("m_QualitySettings");array.GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue="Quest Development";array.GetArrayElementAtIndex(1).FindPropertyRelative("name").stringValue="Editor Preview";quality.ApplyModifiedPropertiesWithoutUndo();QualitySettings.SetQualityLevel(0);
  AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));
 }
}}
