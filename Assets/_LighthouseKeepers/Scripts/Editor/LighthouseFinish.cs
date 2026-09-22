using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Environment;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor {
public static class LighthouseFinish {
 public static void Apply(){
  var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Level01_Plumbing"));
  if(!GameObject.Find("Day driven ceiling leaks")){
   var root=new GameObject("Day driven ceiling leaks");var response=root.AddComponent<DayEnvironmentResponse>();var leaks=new ParticleSystem[10];
   for(int i=0;i<10;i++){var p=LighthouseArchitecture.Polar(3.8f,15+i*28,2.7f);var go=LighthouseArchitecture.Rain(p,new Vector3(.1f,.05f,.1f),false);go.transform.SetParent(root.transform,true);go.name="Ceiling drip "+(i+1);var ps=go.GetComponent<ParticleSystem>();var main=ps.main;main.startLifetime=.35f;main.startSize=.007f;main.maxParticles=12;var weather=go.GetComponent<WeatherVolume>();LighthouseScenes.Set(weather,"baselineEmission",4f);go.GetComponent<ParticleSystemRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/Materials/LK_Rain.mat");leaks[i]=ps;}
   LighthouseScenes.SetArray(response,"leaks",leaks.Cast<Object>().ToArray());
  }
  EditorSceneManager.SaveScene(scene);
  var bootstrap=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));var systems=Object.FindAnyObjectByType<LighthouseKeepers.Core.EnvironmentBootstrap>();if(!systems.GetComponent<LighthouseKeepers.Core.DeviceDiagnostics>())systems.gameObject.AddComponent<LighthouseKeepers.Core.DeviceDiagnostics>();EditorSceneManager.SaveScene(bootstrap);AssetDatabase.SaveAssets();LighthouseValidation.Validate();
 }
}}
