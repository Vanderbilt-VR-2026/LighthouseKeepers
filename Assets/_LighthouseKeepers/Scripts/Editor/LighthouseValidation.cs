using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Progression;
using LighthouseKeepers.Puzzles;
namespace LighthouseKeepers.Editor {
public static class LighthouseValidation {
 [MenuItem("Lighthouse Keepers/03 Validate Sprint Foundation")]
 public static void Validate(){
  var errors=new List<string>();var notes=new List<string>();
  foreach(var name in LighthouseScenes.Names){var path=LighthouseScenes.ScenePath(name);if(!File.Exists(path))errors.Add("Missing scene "+name);if(!EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path==path))errors.Add("Build list missing "+name);}
  foreach(var layer in new[]{"Environment","Interactable","PlayerBody"})if(LayerMask.NameToLayer(layer)<0)errors.Add("Missing layer "+layer);
  var mixer=LighthouseAudioAssets.Create();foreach(var name in LighthouseAudioAssets.Groups)if(!mixer.FindMatchingGroups("").Any(g=>g.name==name))errors.Add("Missing mixer group "+name);
  foreach(var name in LighthouseAudioAssets.Snapshots)if(!mixer.FindSnapshot(name))errors.Add("Missing snapshot "+name);
  EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));foreach(var name in LighthouseScenes.Names.Skip(1).Take(6))EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name),OpenSceneMode.Additive);
  var roots=Enumerable.Range(0,SceneManager.sceneCount).SelectMany(i=>SceneManager.GetSceneAt(i).GetRootGameObjects()).ToArray();
  foreach(var root in roots)foreach(var t in root.GetComponentsInChildren<Transform>(true)){
   if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)errors.Add("Missing script: "+t.name);
   if(PrefabUtility.GetPrefabInstanceStatus(t.gameObject)==PrefabInstanceStatus.MissingAsset)errors.Add("Broken prefab: "+t.name);
   foreach(var r in t.GetComponents<Renderer>())foreach(var m in r.sharedMaterials)if(!m||!m.shader||m.shader.name.Contains("InternalError"))errors.Add("Missing/error material: "+t.name);
  }
  var origins=UnityEngine.Object.FindObjectsByType<XROrigin>(FindObjectsInactive.Exclude);if(origins.Length!=1)errors.Add("Expected one active XR Origin, got "+origins.Length);
  if(UnityEngine.Object.FindObjectsByType<AudioListener>().Length!=1)errors.Add("Expected one listener");
  var behaviours=roots.SelectMany(r=>r.GetComponentsInChildren<MonoBehaviour>(true)).Where(b=>b).ToArray();
  if(behaviours.Count(b=>b.GetType().Name=="InputActionManager")!=1)errors.Add("Expected one InputActionManager");
  foreach(var expected in new[]{"PlayerComfort","HeadBoundaryComfort","BodyPresence","ContinuousMoveProvider","SnapTurnProvider","ContinuousTurnProvider","GravityProvider"})if(!behaviours.Any(b=>b.GetType().Name==expected||b.GetType().BaseType?.Name==expected))errors.Add("Missing player component "+expected);
  var sockets=UnityEngine.Object.FindObjectsByType<PuzzleSocket>();foreach(var s in sockets)if(!s.Anchor||string.IsNullOrWhiteSpace(s.StationId)||string.IsNullOrWhiteSpace(s.Role))errors.Add("Incomplete socket: "+s.name);
  if(sockets.Select(s=>s.StationId).Distinct().Count()!=6)errors.Add("Expected six puzzle station identities");
  var thresholds=UnityEngine.Object.FindObjectsByType<FloodThreshold>().Select(t=>t.Threshold).OrderBy(v=>v).ToArray();if(thresholds.Length!=4||thresholds.Distinct().Count()!=4)errors.Add("Flood thresholds missing or duplicate");
  var days=AssetDatabase.FindAssets("t:DayProfile").Select(g=>AssetDatabase.LoadAssetAtPath<DayProfile>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(p=>p.Day).ToArray();
  if(days.Length!=5)errors.Add("Expected five day profiles");for(int i=1;i<days.Length;i++)if(days[i].StormIntensity<=days[i-1].StormIntensity||days[i].FloodRiseRate<=days[i-1].FloodRiseRate||days[i].LightningInterval>=days[i-1].LightningInterval)errors.Add("Day intensity ordering invalid");
  var flood=UnityEngine.Object.FindAnyObjectByType<FloodController>();if(!flood||!flood.Profile)errors.Add("Flood profile missing");else{flood.SetHeight(999);if(flood.Height!=flood.Profile.MaximumHeight)errors.Add("Flood upper clamp failed");flood.ResetFlood();if(!flood.Paused||flood.Height!=flood.Profile.MinimumHeight)errors.Add("Flood reset failed");}
  Physics.SyncTransforms();
  int rampSamples=0;
  for(int level=0;level<3;level++)for(int step=1;step<96;step++){
   float angle=step*360f/96;var p=LighthouseArchitecture.Polar(1.8f,angle,level*3.2f+step*3.2f/96+.015f);
   if(!Physics.Raycast(p+Vector3.up*.15f,Vector3.down,.3f,1<<8))errors.Add("Spiral support missing at "+p);
   var blockers=Physics.OverlapCapsule(p+Vector3.up*.32f,p+Vector3.up*1.65f,.2f,1<<8,QueryTriggerInteraction.Ignore);if(blockers.Length>0)errors.Add("Spiral head clearance blocked at "+p+": "+string.Join(",",blockers.Select(c=>c.name)));
   rampSamples++;
  }
  notes.Add("Spiral support/head-clearance samples="+rampSamples);
  notes.Add("Loaded seven environment scenes. XR Origins="+origins.Length+"; puzzle sockets="+sockets.Length+"; thresholds="+thresholds.Length+"; day profiles="+days.Length+".");
  foreach(var path in AssetDatabase.FindAssets("t:Prefab",new[]{LighthouseAssets.Root}).Select(AssetDatabase.GUIDToAssetPath)){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);foreach(var t in prefab.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)errors.Add("Missing prefab script "+path);}
  Directory.CreateDirectory("Docs/Verification");File.WriteAllLines("Docs/Verification/AssetValidation.txt",notes.Concat(errors.Count==0?new[]{"PASS: static asset and configuration checks"}:errors));
  EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));if(errors.Count>0)throw new Exception(string.Join("\n",errors));Debug.Log("LIGHTHOUSE ASSET VALIDATION PASSED");
 }
 public static void BuildAndroid(){
  LighthouseProjectSetup.Configure();LighthouseFinish.Apply();Validate();
  Directory.CreateDirectory("Builds");var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=LighthouseScenes.Names.Take(7).Select(LighthouseScenes.ScenePath).ToArray(),locationPathName="Builds/LighthouseKeepers-Development.apk",target=BuildTarget.Android,options=BuildOptions.Development});
  File.WriteAllText("Docs/Verification/AndroidBuild.txt",result.summary.result+"\nErrors="+result.summary.totalErrors+" Warnings="+result.summary.totalWarnings+" Bytes="+result.summary.totalSize+"\n"+result.summary.totalTime);
  if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Android build failed");
 }
}}
