using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor {
public static class LighthouseOptimization {
 [MenuItem("Lighthouse Keepers/04 Create Static Render Batches")]
 public static void Optimize(){
  foreach(var name in LighthouseScenes.Names.Skip(1).Take(6)){
   var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
   if(GameObject.Find("Static render batches"))continue;
   var root=new GameObject("Static render batches");
   var candidates=UnityEngine.Object.FindObjectsByType<MeshRenderer>().Where(r=>r.enabled&&r.gameObject.isStatic&&!r.GetComponentInParent<Rigidbody>()&&!r.GetComponentInParent<LighthouseKeepers.Environment.BeaconRotation>()&&!r.GetComponent<LighthouseKeepers.Flood.FloodSurface>()&&!r.GetComponentInParent<LODGroup>()&&r.GetComponent<MeshFilter>()&&r.sharedMaterials.Length==1).ToArray();
   foreach(var group in candidates.GroupBy(r=>r.sharedMaterial)){
    var mesh=new Mesh{name=name+"_"+group.Key.name,indexFormat=IndexFormat.UInt32};var combine=group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=r.transform.localToWorldMatrix}).ToArray();mesh.CombineMeshes(combine,true,true);Unwrapping.GenerateSecondaryUVSet(mesh);mesh=SaveMesh(mesh,mesh.name);
    var batch=new GameObject(group.Key.name,typeof(MeshFilter),typeof(MeshRenderer));batch.transform.SetParent(root.transform);batch.GetComponent<MeshFilter>().sharedMesh=mesh;batch.GetComponent<MeshRenderer>().sharedMaterial=group.Key;batch.isStatic=true;
    foreach(var renderer in group)renderer.enabled=false;
   }
   EditorSceneManager.SaveScene(scene);
  }
  // Standalone authored modules are useful to teammates without invoking generators.
  var prefabScene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var wall=Box("LK Masonry Module",Vector3.zero,new Vector3(.4f,3.2f,1),"Masonry");Save(wall,"Architecture/LK_MasonryModule");
  var pipe=Cylinder("LK Pipe Module",Vector3.zero,new Vector3(.2f,.5f,.2f),"Rust");Save(pipe,"Environment/LK_PipeModule");
  var wrench=LighthouseRooms.Grab("LK Grab Wrench",Vector3.zero,new Vector3(.09f,.04f,.4f),"Iron");Save(wrench,"Interaction/LK_GrabWrench");
  var socket=LighthouseRooms.Socket("Custom station","Interaction role",Vector3.zero,"Configure station identity and role, then subscribe to SignalInteraction.");Save(socket.gameObject,"PuzzleSockets/LK_PuzzleSocket");
  EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));AssetDatabase.SaveAssets();
 }
 static void Save(GameObject go,string name){string path=Root+"/Prefabs/"+name+".prefab";if(!File.Exists(path))PrefabUtility.SaveAsPrefabAsset(go,path);UnityEngine.Object.DestroyImmediate(go);}
}}
