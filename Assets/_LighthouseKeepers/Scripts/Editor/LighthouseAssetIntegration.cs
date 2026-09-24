using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Audio;
using LighthouseKeepers.Environment;
using LighthouseKeepers.Puzzles;
namespace LighthouseKeepers.Editor {
public static class LighthouseAssetIntegration {
 const string Root=LighthouseVendorVariants.Root, Marker="LK Asset Integration Revision 1";
 const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static GameObject Place(string name,Vector3 p,float scale=1,float yaw=0){var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/LK_"+name+".prefab"));go.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));go.transform.localScale=Vector3.one*scale;return go;}
 static void Retire(Func<Transform,bool> match){foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(match)){foreach(var r in t.GetComponents<Renderer>())r.enabled=false;foreach(var c in t.GetComponents<Collider>())c.enabled=false;t.gameObject.isStatic=false;t.name="Retired visual - "+t.name;}}
 static string Anchors()=>string.Join("\n",UnityEngine.Object.FindObjectsByType<PuzzleSocket>(FindObjectsSortMode.None).OrderBy(s=>s.name).Select(s=>s.name+":"+EditorJsonUtility.ToJson(s)+":"+s.transform.position.ToString("F5")));
 public static void Apply(){
  if(Application.unityVersion!="6000.3.23f1")throw new Exception("Wrong Unity version");
  LighthouseVendorVariants.Create();
  foreach(string sceneName in new[]{"LK_Core","LK_Level01_Plumbing","LK_Level02_Generator","LK_Level03_Communications","LK_Level04_Lantern"}){
   var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(sceneName));if(GameObject.Find(Marker))continue;string anchors=Anchors();
   if(sceneName=="LK_Core"){
    Retire(t=>(t.name=="Maintenance worktop"||t.name=="Workbench leg")&&t.position.x< -7);
    Place("TableOffice",new Vector3(-8.3f,0,1.85f),1.07f);
    Place("MetalCabinet01_1",new Vector3(-10.65f,0,-1.9f),1,45);
    Place("FireExtinguisher01_1",new Vector3(-5.5f,.8f,2.1f),1,180);
   }
   if(sceneName=="LK_Level01_Plumbing"){
    Retire(t=>t.name=="Sump pump motor"||t.name=="Pump pressure chamber");
    Place("Crane01_Motor",new Vector3(-3.4f,.08f,-1.6f),1.25f,90);
    Place("Pipe",new Vector3(-3.8f,0,-1.5f),.77f);
    Place("Pipe",new Vector3(2.4f,0,-3.45f),.77f);
    Place("SmallMetalicCase",new Vector3(-3.65f,0,1.5f),.8f);
    Place("P_Lamp",new Vector3(0,2.55f,-3.9f),.65f);
   }
   if(sceneName=="LK_Level02_Generator"){
    Retire(t=>t.name=="Diesel engine housing"||t.name=="Engine cooling fin"||t.name=="Generator alternator"||t.name=="Oil storage drum");
    Place("Crane01_Motor",new Vector3(-.5f,3.5f,-3.5f),1.65f);
    Place("Crane01_Motor",new Vector3(.85f,3.5f,-3.5f),1.05f);
    Place("FanBig01Motor01",new Vector3(-2.3f,4.85f,-3.9f),.48f,180);
    for(int i=0;i<3;i++)Place("Barrel01a",new Vector3(-3.6f,3.2f,-1+i*.8f));
    Place("MetalCabinet01_1",new Vector3(2.8f,3.2f,2.5f),1,225);
    Place("FireExtinguisher01_1",new Vector3(-2.4f,4,3.5f),1,180);
    Place("P_Lamp",new Vector3(0,5.85f,-3.6f),.8f);
    var hum=UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).First(s=>s.name.Contains("Room-specific"));hum.transform.position=new Vector3(0,4,-3.5f);
   }
   if(sceneName=="LK_Level03_Communications"){
    // Preserve radio and all interaction anchors. Only retire table geometry.
    Retire(t=>(t.name=="Maintenance worktop"||t.name=="Workbench leg")&&t.position.x<2);
    Place("TableOffice",new Vector3(0,6.4f,-3.5f),1.07f,180);
    // Existing narrow east worktop stays: vendor table would reduce landing clearance.
    Place("ChairSchool",new Vector3(-1.5f,6.4f,-3.25f),.9f,115);
    Place("SmallMetalicCase",new Vector3(3.4f,6.4f,2.2f),.65f);
    Place("P_Lamp",new Vector3(0,9.05f,-3.6f),.7f);
   }
   if(sceneName=="LK_Level04_Lantern"){
    Place("Crane01_Motor",new Vector3(-.55f,9.75f,.25f),.55f);
    Place("P_Lamp",new Vector3(-2,12.1f,2),.6f,45);
   }
   if(Anchors()!=anchors)throw new Exception("Puzzle anchor changed in "+sceneName);
   new GameObject(Marker);LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
  }
  Exterior();Music();AssetDatabase.SaveAssets();LighthouseValidation.Validate();Debug.Log("ASSET INTEGRATION COMPLETE");
 }
 public static void LightingTouchup(){
  foreach(var entry in new[]{("LK_Core",new Vector3(-8,2.2f,0)),("LK_Level01_Plumbing",new Vector3(-2,2.3f,-2.5f)),("LK_Level02_Generator",new Vector3(0,5.6f,-2.7f)),("LK_Level03_Communications",new Vector3(0,8.8f,-2.7f))}){
   var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(entry.Item1));var go=GameObject.Find("Baked integration practical fill");if(!go)go=new GameObject("Baked integration practical fill");var light=go.GetComponent<Light>();if(!light)light=go.AddComponent<Light>();go.transform.position=entry.Item2;light.type=LightType.Point;light.lightmapBakeType=LightmapBakeType.Baked;light.shadows=LightShadows.None;light.range=6;light.intensity=8;light.color=entry.Item1.Contains("01")?new Color(.6f,.74f,.85f):new Color(1,.75f,.46f);EditorSceneManager.SaveScene(scene);
  }
  var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/LK_CoastalFog.mat");mat.SetColor("_BaseColor",new Color(.35f,.43f,.48f,.3f));EditorUtility.SetDirty(mat);AssetDatabase.SaveAssets();
  LighthouseLighting.BakeAndPreview();
 }
 static void Exterior(){
  var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Exterior"));if(GameObject.Find(Marker))return;
  var ocean=GameObject.Find("Storm ocean");var mesh=new Mesh{name="LK_OceanGrid"};const int n=64;var v=new Vector3[(n+1)*(n+1)];var tris=new int[n*n*6];int k=0;
  for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)v[z*(n+1)+x]=new Vector3((x/(float)n-.5f)*400,0,(z/(float)n-.5f)*400);
  for(int z=0;z<n;z++)for(int x=0;x<n;x++){int a=z*(n+1)+x;tris[k++]=a;tris[k++]=a+n+1;tris[k++]=a+1;tris[k++]=a+1;tris[k++]=a+n+1;tris[k++]=a+n+2;}
  mesh.vertices=v;mesh.triangles=tris;mesh.RecalculateNormals();mesh.bounds=new Bounds(Vector3.zero,new Vector3(400,4,400));AssetDatabase.CreateAsset(mesh,Root+"/Meshes/LK_OceanGrid.asset");
  var material=new Material(Shader.Find("LighthouseKeepers/Quest Procedural Ocean")){name="LK_QuestOcean",enableInstancing=true};AssetDatabase.CreateAsset(material,Root+"/Materials/LK_QuestOcean.mat");
  ocean.transform.localScale=Vector3.one;ocean.GetComponent<MeshFilter>().sharedMesh=mesh;ocean.GetComponent<Renderer>().sharedMaterial=material;ocean.GetComponent<Renderer>().enabled=true;ocean.isStatic=false;
  foreach(var c in ocean.GetComponents<Collider>())UnityEngine.Object.DestroyImmediate(c);
  // Preserve a supplied URP material for comparison; runtime uses the stereo-safe fallback.
  var vendorShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Procedural Water Shader/Shaders/ProceduralWater.shader");if(vendorShader){var comparison=new Material(vendorShader){name="LK_VendorOcean_EditorComparison"};AssetDatabase.CreateAsset(comparison,Root+"/Materials/LK_VendorOcean_EditorComparison.mat");}
  var fogMat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")){name="LK_CoastalFog"};fogMat.SetFloat("_Surface",1);fogMat.SetFloat("_Blend",0);fogMat.SetFloat("_ZWrite",0);fogMat.SetFloat("_SrcBlend",5);fogMat.SetFloat("_DstBlend",10);fogMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");fogMat.renderQueue=3000;
  fogMat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture>("Assets/Fog Particles/Texture/Smoke Sprite Sheet.png"));fogMat.SetColor("_BaseColor",new Color(.35f,.43f,.48f,.08f));AssetDatabase.CreateAsset(fogMat,Root+"/Materials/LK_CoastalFog.mat");
  for(int i=0;i<3;i++){
   var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Fog Particles/Prefabs/Bluish Fog.prefab"));go.name="LK Offshore fog "+i;go.transform.position=new Vector3(i==0?-22:i==1?22:0,-1,i==2?25:10);go.transform.localScale=Vector3.one;
   var ps=go.GetComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.maxParticles=8;main.startLifetime=12;main.startSize=6;main.startSpeed=0;main.startColor=new Color(.55f,.65f,.72f,.12f);main.simulationSpace=ParticleSystemSimulationSpace.World;
   var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(7,.25f,4);var emission=ps.emission;emission.rateOverTime=.2f;emission.SetBursts(new ParticleSystem.Burst[0]);var velocity=ps.velocityOverLifetime;velocity.enabled=false;var noise=ps.noise;noise.enabled=false;var collision=ps.collision;collision.enabled=false;var trails=ps.trails;trails.enabled=false;
   var r=ps.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=fogMat;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;r.maxParticleSize=.12f;go.AddComponent<CoastalFog>();
   if(i==0)PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/LK_CoastalFog.prefab");
  }
  new GameObject(Marker);EditorSceneManager.SaveScene(scene);
 }
 static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Flags).Invoke(target,args);
 static void Music(){
  string path=Root+"/Audio/LK_Pressure_SeamlessLoop.wav";var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.Streaming;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.5f;settings.preloadAudioData=false;importer.defaultSampleSettings=settings;importer.loadInBackground=true;importer.SaveAndReimport();
  var mixer=LighthouseAudioAssets.Create();var group=mixer.FindMatchingGroups("").FirstOrDefault(g=>g.name=="Ambience Music");
  if(!group){group=(AudioMixerGroup)Call(mixer,"CreateNewGroup","Ambience Music",true);var master=mixer.GetType().GetProperty("masterGroup",Flags).GetValue(mixer);Call(mixer,"AddChildToParent",group,master);foreach(var snapshot in (Array)mixer.GetType().GetProperty("snapshots",Flags).GetValue(mixer))Call(group,"SetValueForVolume",mixer,snapshot,-20f);
   EditorUtility.SetDirty(mixer);
  }
  var volume=Call(group,"GetGUIDForVolume");
  if(!(bool)Call(mixer,"ContainsExposedParameter",volume)){
   var type=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Audio.AudioGroupParameterPath");var parameter=Activator.CreateInstance(type,Flags,null,new object[]{group,volume},null);Call(mixer,"AddExposedParameter",parameter);
   var prop=mixer.GetType().GetProperty("exposedParameters",Flags);var entries=(Array)prop.GetValue(mixer);
   for(int i=0;i<entries.Length;i++){var entry=entries.GetValue(i);if(entry.GetType().GetField("guid",Flags).GetValue(entry).Equals(volume)){entry.GetType().GetField("name",Flags).SetValue(entry,"AmbienceMusicVolume");entries.SetValue(entry,i);}}
   prop.SetValue(mixer,entries);EditorUtility.SetDirty(mixer);
  }
  var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));if(!UnityEngine.Object.FindAnyObjectByType<AmbienceMusic>()){
   var go=new GameObject("Persistent horror ambience - pressure nofx");var director=UnityEngine.Object.FindAnyObjectByType<AcousticDirector>();go.transform.SetParent(director.transform,false);var source=go.AddComponent<AudioSource>();source.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);source.loop=true;source.playOnAwake=false;source.spatialBlend=0;source.volume=1;source.outputAudioMixerGroup=group;var music=go.AddComponent<AmbienceMusic>();LighthouseScenes.Set(music,"mixer",mixer);LighthouseScenes.Set(music,"thunder",UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).FirstOrDefault(s=>s.name.ToLower().Contains("thunder")));
   EditorSceneManager.SaveScene(scene);
  }
 }
}
}
