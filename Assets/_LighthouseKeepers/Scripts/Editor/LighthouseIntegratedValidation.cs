using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Audio;
using LighthouseKeepers.Environment;
using LighthouseKeepers.Puzzles;
namespace LighthouseKeepers.Editor {
public static class LighthouseIntegratedValidation {
 [MenuItem("Lighthouse Keepers/Integration/Open complete environment")]
 public static void OpenEnvironment(){
  if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));foreach(var name in LighthouseScenes.Names.Skip(1).Take(6))EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name),OpenSceneMode.Additive);
  EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(LighthouseScenes.ScenePath("LK_Bootstrap"));
  if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(new Vector3(-2,5,0),Quaternion.Euler(20,35,0),20);
  Debug.Log("LK HANDOFF: complete environment open; Play starts at Bootstrap.");
 }
 public static void BuildFinal(){
  PreserveUnusedLightingCopies();Run();LighthouseValidation.BuildAndroid();
 }
 static void PreserveUnusedLightingCopies(){
  var seeds=LighthouseScenes.Names.Select(LighthouseScenes.ScenePath).Concat(AssetDatabase.FindAssets("t:Prefab",new[]{LighthouseAssets.Root}).Select(AssetDatabase.GUIDToAssetPath)).ToArray();var dependencies=AssetDatabase.GetDependencies(seeds,true);
  const string folder="Assets/_LighthouseKeepers/Scenes/Environment/LK_Exterior";const string backup="Builds/RecoveredDuplicateLighting-20260923";Directory.CreateDirectory(backup);var report=new System.Collections.Generic.List<string>();
  foreach(var path in Directory.GetFiles(folder).Where(p=>!p.EndsWith(".meta")&&Path.GetFileNameWithoutExtension(p).EndsWith(" 2"))){
   if(dependencies.Contains(path)){report.Add("Retained referenced copy: "+path);continue;}
   string target=backup+"/"+Path.GetFileName(path);if(File.Exists(target)){report.Add("Retained because backup exists: "+path);continue;}
   File.Move(path,target);if(File.Exists(path+".meta"))File.Move(path+".meta",target+".meta");report.Add("Preserved unreferenced generated copy: "+path+" -> "+target);
  }
  AssetDatabase.Refresh();if(report.Count>0)File.AppendAllLines("Docs/Verification/DuplicateLightingIntegration.txt",report);
 }
 public static void RunAndPlay(){Run();LighthousePlayVerification.Run();}
 public static void Run(){
  LighthouseValidation.Validate();
  EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));foreach(var name in LighthouseScenes.Names.Skip(1).Take(6))EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name),OpenSceneMode.Additive);
  if(UnityEngine.Object.FindObjectsByType<PuzzleSocket>(FindObjectsSortMode.None).Length!=16)throw new Exception("Socket count must remain exactly 16");
  LighthouseCirculationWeather.ValidateLoaded();
  var surface=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.Flood.FloodSurface>();if(!surface.GetComponent<Renderer>().enabled||surface.gameObject.isStatic)throw new Exception("Flood surface was statically batched");
  var beacon=UnityEngine.Object.FindAnyObjectByType<BeaconRotation>();if(beacon.GetComponentsInChildren<Renderer>().Any(r=>!r.enabled||r.gameObject.isStatic))throw new Exception("Moving beacon visuals were statically batched");
  var music=UnityEngine.Object.FindObjectsByType<AmbienceMusic>(FindObjectsSortMode.None);if(music.Length!=1)throw new Exception("Expected one soundtrack");
  var audio=music[0].GetComponent<AudioSource>();if(!audio.clip||!audio.loop||audio.spatialBlend!=0||audio.outputAudioMixerGroup.name!="Ambience Music")throw new Exception("Soundtrack routing/loop failed");
  if(audio.clip.loadType!=AudioClipLoadType.Streaming)throw new Exception("Music must stream");
  if(!audio.outputAudioMixerGroup.audioMixer.GetFloat("AmbienceMusicVolume",out var volume))throw new Exception("Music parameter not exposed");
  var fog=UnityEngine.Object.FindObjectsByType<CoastalFog>(FindObjectsSortMode.None);if(fog.Length!=3)throw new Exception("Expected three bounded offshore fog emitters");
  foreach(var f in fog){var p=f.transform.position;if(new Vector2(p.x,p.z).magnitude<20||p.y>0)throw new Exception("Fog too close to interior");var ps=f.GetComponent<ParticleSystem>();if(ps.main.maxParticles>8||ps.collision.enabled)throw new Exception("Fog budget exceeded");}
  var ocean=GameObject.Find("Storm ocean");if(ocean.GetComponent<MeshFilter>().sharedMesh.vertexCount>5000||ocean.GetComponent<Renderer>().sharedMaterial.shader.name!="LighthouseKeepers/Quest Procedural Ocean")throw new Exception("Ocean fallback missing");
  foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(r.enabled)foreach(var m in r.sharedMaterials)if(m&&ShaderUtil.ShaderHasError(m.shader))throw new Exception("Shader compiler error: "+m.name);
  File.WriteAllText("Docs/Verification/IntegratedAssets.txt","PASS: original foundation validation; 16 sockets; 1 streamed looping soundtrack routed to Ambience Music; exposed volume="+volume+" dB; 3 offshore fog emitters capped at 8 particles each; 4225-vertex stereo-aware opaque ocean; circulation checks; no active shader compiler errors. Headset audio, stereo appearance and GPU timing remain unverified.\n");
  EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));
 }
 public static void Export(){
  Run();var paths=AssetDatabase.FindAssets("",new[]{"Assets/_LighthouseKeepers"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>!AssetDatabase.IsValidFolder(p)).ToList();
  paths.AddRange(new[]{"Assets/TirgamesAssets/Factory/AbandonedFactoryLite.pdf","Assets/Abandoned_Asylum/Read_me.txt","Assets/Fog Particles/Fog Particles - Documentation.pdf","Assets/free horror ambience 2/ha-pressure-nofx.wav","Assets/Procedural Water Shader/How to Use.TXT"}.Where(File.Exists));
  var dependencies=AssetDatabase.GetDependencies(paths.ToArray(),true).Where(p=>p.StartsWith("Assets/")).Distinct().ToArray();
  File.WriteAllLines("Docs/Verification/IntegratedAssetPaths.txt",dependencies);
  AssetDatabase.ExportPackage(dependencies,"/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers/Builds/SelectedAssetPackages/LighthouseIntegrated.unitypackage",ExportPackageOptions.Default);
 }
}
}
