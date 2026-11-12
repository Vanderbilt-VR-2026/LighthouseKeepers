using System.Linq;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using LighthouseKeepers.WatchDemo;
namespace LighthouseKeepers.Editor {
 // Resources are included in the player; fail packaging rather than silently omit a broken bake.
 public sealed class TowerWiringBuildGuard : IPreprocessBuildWithReport {
  public int callbackOrder=>0;
  public void OnPreprocessBuild(BuildReport report){
   var d=Resources.Load<TowerWiringAsset>("TowerWiring/Layout");
   if(!d||!d.material||!d.material.mainTexture||d.floors==null||d.floors.Length!=5||d.floors.Any(m=>!m))throw new BuildFailedException("Tower wiring resources are missing. Capture/rebake Lab routes before packaging.");
   int count=d.floors.Sum(m=>m.triangles.Length/3);if(count!=d.triangles||count>15000)throw new BuildFailedException("Tower wiring bake is stale or over 15k triangles.");
   Debug.Log($"WIRING_BUILD_RESOURCE floors=5 triangles={count} endpoints={d.endpoints.Length} material={d.material.name}");
  }
 }
}
