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
   foreach(var side in new[]{"Left Controller","Right Controller"}){
    var controller=rig.GetComponentsInChildren<Transform>(true).First(t=>t.name==side);
    foreach(var renderer in controller.GetComponentsInChildren<MeshRenderer>(true))renderer.enabled=false;
    foreach(var renderer in controller.GetComponentsInChildren<SkinnedMeshRenderer>(true))renderer.enabled=false;
    var old=controller.Find("LK Stylized Glove");if(old)Object.DestroyImmediate(old.gameObject);
    var hand=new GameObject("LK Stylized Glove").transform;hand.SetParent(controller,false);
    Primitive(PrimitiveType.Sphere,"Glove palm",new Vector3(0,-.018f,.01f),new Vector3(.085f,.037f,.11f),"Hands",hand,false);
    for(int i=0;i<4;i++){
     float x=-.03f+i*.02f,len=i==3?.052f:i==0?.064f:.076f;
     var finger=Primitive(PrimitiveType.Capsule,"Glove finger "+(i+1),new Vector3(x,-.02f,.05f+len*.34f),new Vector3(.017f,len*.5f,.017f),"Hands",hand,false);finger.transform.localRotation=Quaternion.Euler(80,0,0);
    }
    var thumb=Primitive(PrimitiveType.Capsule,"Glove thumb",new Vector3(side.StartsWith("Left")?.048f:-.048f,-.02f,.012f),new Vector3(.021f,.027f,.021f),"Hands",hand,false);thumb.transform.localRotation=Quaternion.Euler(70,side.StartsWith("Left")?35:-35,0);
    Primitive(PrimitiveType.Sphere,"Canvas cuff",new Vector3(0,-.015f,-.042f),new Vector3(.074f,.04f,.03f),"Cloth",hand,false);
   }
 }

}}
