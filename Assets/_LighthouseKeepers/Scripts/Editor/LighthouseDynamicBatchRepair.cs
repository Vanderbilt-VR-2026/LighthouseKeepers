using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Environment;
namespace LighthouseKeepers.Editor {
 public static class LighthouseDynamicBatchRepair {
  public static void RepairAndVerify(){
   foreach(var name in new[]{"LK_Level01_Plumbing","LK_Level04_Lantern"}){
    var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
    foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
     if(r.GetComponent<FloodSurface>()||r.GetComponentInParent<BeaconRotation>()){r.enabled=true;r.gameObject.isStatic=false;EditorUtility.SetDirty(r);}
    LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();LighthouseIntegratedValidation.Run();LighthouseLighting.BakeAndPreview();
  }
 }
}
