using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Player;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
namespace LighthouseKeepers.Editor {
public static class LighthouseCollisionFix {
 public static void Apply(){
  foreach(var name in LighthouseScenes.Names){var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
   foreach(var capsule in Object.FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None)){var filter=capsule.GetComponent<MeshFilter>();if(!filter||filter.sharedMesh.name!="Cylinder")continue;var go=capsule.gameObject;Object.DestroyImmediate(capsule);go.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;}
   foreach(var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)){var vignette=camera.GetComponentInChildren<TunnelingVignetteController>();if(vignette&&!camera.GetComponent<HeadBoundaryComfort>()){var safety=camera.gameObject.AddComponent<HeadBoundaryComfort>();LighthouseScenes.Set(safety,"vignette",vignette);}}
   foreach(var origin in Object.FindObjectsByType<Unity.XR.CoreUtils.XROrigin>(FindObjectsSortMode.None))if(!origin.GetComponent<RecenterPlayer>())origin.gameObject.AddComponent<RecenterPlayer>();
   EditorSceneManager.SaveScene(scene);
  }
  LighthouseValidation.Validate();LighthouseLighting.Bake();
 }
}}
