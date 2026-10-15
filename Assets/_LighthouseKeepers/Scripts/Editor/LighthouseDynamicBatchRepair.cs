using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Environment;
using LighthouseKeepers.Puzzles;
namespace LighthouseKeepers.Editor {
 public static class LighthouseDynamicBatchRepair {
  [MenuItem("Lighthouse Keepers/Integration/Repair dynamic batches")]
  public static void RepairAndVerify(){
   foreach(var name in new[]{"LK_Level01_Plumbing","LK_Level04_Lantern"}){
    var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
    foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
     if(r.GetComponent<FloodSurface>()||r.GetComponentInParent<BeaconRotation>()){r.enabled=true;r.gameObject.isStatic=false;EditorUtility.SetDirty(r);}
    foreach(var valve in UnityEngine.Object.FindObjectsByType<ValveHandwheel>(FindObjectsSortMode.None)){
     if(valve.TryGetComponent<MeshRenderer>(out var selfRenderer)){selfRenderer.enabled=true;valve.gameObject.isStatic=false;EditorUtility.SetDirty(selfRenderer);}
     foreach(var visual in valve.SyncedVisuals)
      if(visual&&visual.TryGetComponent<MeshRenderer>(out var visualRenderer)){visualRenderer.enabled=true;visual.gameObject.isStatic=false;EditorUtility.SetDirty(visualRenderer);}
    }
    LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();LighthouseIntegratedValidation.Run();LighthouseLighting.BakeAndPreview();
  }
 }
}
