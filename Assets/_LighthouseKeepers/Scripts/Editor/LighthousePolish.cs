using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Audio;
using LighthouseKeepers.Environment;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor {
public static class LighthousePolish {
 public static void Apply(){
  var water=M("Water");water.shader=Shader.Find("LighthouseKeepers/Storm Ocean");EditorUtility.SetDirty(water);
  string rainPath=Root+"/Art/Materials/LK_Rain.mat";var rain=AssetDatabase.LoadAssetAtPath<Material>(rainPath);if(!rain){rain=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(rain,rainPath);}rain.SetColor("_BaseColor",new Color(.5f,.65f,.75f,.28f));rain.SetFloat("_Surface",1);rain.SetFloat("_Blend",0);rain.SetFloat("_SrcBlend",5);rain.SetFloat("_DstBlend",10);rain.SetFloat("_ZWrite",0);rain.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");rain.renderQueue=3000;
  foreach(var name in LighthouseScenes.Names.Skip(1)){
   var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
   if(!GameObject.Find("LK Polish Applied")){
    new GameObject("LK Polish Applied");
    foreach(var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=rain;
    if(name=="LK_Core"){
     for(int level=1;level<4;level++){var mesh=Object.Instantiate(AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Art/Models/Floor_"+level+".asset"));var v=mesh.vertices;for(int j=0;j<v.Length;j++)v[j].y-=.16f;mesh.vertices=v;var tri=mesh.triangles;for(int j=0;j<tri.Length;j+=3){int x=tri[j];tri[j]=tri[j+2];tri[j+2]=x;}mesh.triangles=tri;mesh.RecalculateNormals();MeshObject("Ceiling underside "+level,SaveMesh(mesh,"Ceiling_"+level),"Interior",false);}
     var interior=LighthouseAudioAssets.Emitter("Rain enclosed beneath keeper roof",new Vector3(-8,2.6f,0),"RainInterior","Weather Interior",.45f);interior.gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=2700;
     foreach(var lamp in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){var response=lamp.gameObject.AddComponent<DayEnvironmentResponse>();LighthouseScenes.Set(response,"practical",lamp);}
    }
    if(name.StartsWith("LK_Level")){
     int level=int.Parse(name.Substring(8,2))-1; // Level01 begins at character eight.
     var zone=new GameObject("Interior audio zone");zone.transform.position=new Vector3(0,level*3.2f+1.6f,0);var box=zone.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(9.8f,3.2f,9.8f);var audio=zone.AddComponent<AudioZone>();LighthouseScenes.Set(audio,"snapshot",level==0?"Lower Mechanical":level==1?"Generator Running":"Interior Normal");
     foreach(var emitter in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))if(emitter.name.Contains("Room-specific")){var response=emitter.gameObject.AddComponent<DayEnvironmentResponse>();LighthouseScenes.Set(response,level==2?"radio":"machinery",emitter);}
    }
   }
   EditorSceneManager.SaveScene(scene);
  }
  AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));
 }
}}
