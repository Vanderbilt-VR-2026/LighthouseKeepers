using System;using System.Collections.Generic;using System.IO;using System.Linq;
using UnityEngine;using UnityEngine.AI;using UnityEditor;using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo.Editor
{
 public static class HunterNavigationAuthoring
 {
  public const string Path="Assets/WatchDemo/Resources/HunterNavigation.asset";
  // Author from the fully assembled Play Mode scene, including its durable safety guards.
  [MenuItem("Lighthouse Keepers/Horror/Bake hunter navigation from Play Mode")]
  public static void Bake(){BakeReport("artifacts/s5/hunter/navigation");}
  public static string BakeReport(string output)
  {
   EditorUtility.audioMasterMute=true;
   if(!EditorApplication.isPlaying||!UnityEngine.Object.FindAnyObjectByType<WatchDemoController>())throw new InvalidOperationException("Open the actual lighthouse in Play Mode first.");
   var tuning=HorrorTuning.Current;if(!tuning)throw new InvalidOperationException("Missing HorrorTuning asset");Physics.SyncTransforms();
   var sources=new List<NavMeshBuildSource>();var marks=new List<NavMeshBuildMarkup>();
   foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
    if(c.isTrigger||c.GetComponentInParent<Rigidbody>()||c.GetComponentInParent<CharacterController>()||c.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())marks.Add(new NavMeshBuildMarkup{root=c.transform,ignoreFromBuild=true});
   var bounds=new Bounds(new Vector3(0,6,-3),new Vector3(22,18,26));
   NavMeshBuilder.CollectSources(bounds,~0,NavMeshCollectGeometry.PhysicsColliders,0,marks,sources);
   var settings=NavMesh.GetSettingsByIndex(0);settings.agentRadius=tuning.radius;settings.agentHeight=tuning.navigationHeight;settings.agentClimb=tuning.navClimb;settings.agentSlope=tuning.navSlope;settings.overrideVoxelSize=true;settings.voxelSize=tuning.navVoxel;settings.minRegionArea=.1f;
   var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);if(!data)throw new InvalidOperationException("No navigation baked");
   // Do not overwrite a native navigation asset while live agents still have it registered.
   // Restart Play Mode after this authoring operation to recreate their handles.
   NavMesh.RemoveAllNavMeshData();
   var existing=AssetDatabase.LoadAssetAtPath<NavMeshData>(Path);
   if(existing){EditorUtility.CopySerialized(data,existing);UnityEngine.Object.DestroyImmediate(data);data=existing;EditorUtility.SetDirty(data);}else AssetDatabase.CreateAsset(data,Path);
   AssetDatabase.SaveAssets();Directory.CreateDirectory(output);
   using var navigation=new HunterNavigation(tuning,data);
   var points=new[]{new Vector3(-.5f,0,-7),new Vector3(2.6f,0,0),new Vector3(-1,3.3f,-1),new Vector3(1,6.6f,0),new Vector3(1,9.9f,0)};
   var rows=new List<string>();int complete=0,clear=0;
   for(int i=0;i<points.Length;i++)for(int j=0;j<points.Length;j++)if(i!=j){
    bool route=navigation.Route(points[i],points[j],out var corners),walk=route;string failed="";
    if(route){complete++;for(int k=1;k<corners.Length;k++)if(!navigation.SegmentClear(corners[k-1],corners[k])){walk=false;failed=$"segment{k} {corners[k-1]}->{corners[k]}";break;}}
    if(walk)clear++;rows.Add($"{i}->{j}: complete={route} clear={walk} {failed}");
   }
   File.WriteAllLines(output+"/routes.txt",rows);
   File.WriteAllLines(output+"/sources.txt",sources.Select(s=>$"{s.shape} {s.component?.name} {s.size} {s.transform.GetColumn(3)}"));
   return $"sources={sources.Count} complete={complete}/20 collisionClear={clear}/20";
  }
 }
}
