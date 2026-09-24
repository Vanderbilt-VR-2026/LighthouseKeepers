using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor {
public static class LighthouseVisualFixes {
 public static void Apply(){
  M("Masonry").color=new Color(.72f,.74f,.71f);M("Rock").SetFloat("_Smoothness",.22f);M("Rock").SetFloat("_Metallic",0);M("Lens").SetColor("_EmissionColor",new Color(.035f,.12f,.13f));
  var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root+"/Settings/RenderPipeline/QuestPipeline.asset");var so=new SerializedObject(rp);so.FindProperty("m_AdditionalLightsRenderingMode").intValue=2;so.ApplyModifiedPropertiesWithoutUndo();
  string path=Root+"/Art/Materials/LK_StormSky.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(path);if(!sky){sky=new Material(Shader.Find("LighthouseKeepers/Storm Sky"));AssetDatabase.CreateAsset(sky,path);}
  foreach(var name in LighthouseScenes.Names.Take(7)){
   var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
   foreach(var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None)){var main=ps.main;main.startLifetime=.35f;main.startSize=.007f;var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.lengthScale=2;renderer.velocityScale=.015f;}
   foreach(var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))camera.clearFlags=CameraClearFlags.Skybox;
   if(name=="LK_Exterior"){
    RenderSettings.skybox=sky;RenderSettings.fogDensity=.009f;RenderSettings.ambientSkyColor=new Color(.35f,.42f,.46f);RenderSettings.ambientEquatorColor=new Color(.23f,.28f,.3f);
    var template=GameObject.CreatePrimitive(PrimitiveType.Sphere);var baseMesh=template.GetComponent<MeshFilter>().sharedMesh;
    foreach(var rock in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Jagged basalt outcrop")){
     var mesh=rock.sharedMesh;var v=baseMesh.vertices;for(int i=0;i<v.Length;i++){float n=Mathf.PerlinNoise(v[i].x*5+3,v[i].y*4+v[i].z*3+2);v[i]*=.8f+n*.5f;}mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
    }Object.DestroyImmediate(template);
   }
   // Apply collision layers only to solid architecture, leaving grab objects on Default interaction layers.
   foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(!c.isTrigger&&!c.GetComponentInParent<Rigidbody>()&&!c.GetComponent<CharacterController>())c.gameObject.layer=8;
   EditorSceneManager.SaveScene(scene);
  }
  AssetDatabase.SaveAssets();LighthouseOptimization.Optimize();LighthouseValidation.Validate();
 }
}}
