using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor {
public static class LighthousePlayerRepair {
 public static void RepairAndPreview(){
  foreach(var name in new[]{"LK_Bootstrap","LK_DevGym"}){
   var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
   var rig=Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
   foreach(var manager in rig.GetComponentsInChildren<ControllerInputActionManager>(true)){manager.smoothMotionEnabled=manager.gameObject.name.Contains("Left");manager.smoothTurnEnabled=false;}
   CreateGloves(rig.gameObject);
   if(name=="LK_Bootstrap")PrefabUtility.SaveAsPrefabAsset(rig.gameObject,Root+"/Prefabs/Player/LK_QuestPlayer.prefab");
   EditorSceneManager.SaveScene(scene);
  }
  LighthouseFinish.Apply();LighthouseLighting.BakeAndPreview();
 }
 public static void CreateGloves(GameObject rig){
   OneLighthouseM1.InstallHands(rig);
 }

}}
