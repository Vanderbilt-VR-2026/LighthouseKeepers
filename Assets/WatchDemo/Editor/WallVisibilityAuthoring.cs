using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class WallVisibilityAuthoring {
 public static void Apply(){Directory.CreateDirectory("Assets/Art/VisualPass");AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");var log=new List<string>();var materials=new HashSet<Material>();
 foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
  string path=PathOf(r.transform);if(!path.Contains("Shell")||!path.Contains("Wall"))continue;var f=r.GetComponent<MeshFilter>();if(!f||!f.sharedMesh)continue;var b=f.sharedMesh.bounds;b.Expand(.002f);bool outside=f.sharedMesh.vertices.Any(v=>!b.Contains(v));log.Add(path+" | enabled="+r.enabled+" boundsContainAll="+!outside+" LOD="+(r.GetComponentInParent<LODGroup>()!=null));
  if(outside){var m=UnityEngine.Object.Instantiate(f.sharedMesh);m.RecalculateBounds();string dest="Assets/Art/VisualPass/WallBounds-"+r.GetInstanceID()+".asset";AssetDatabase.CreateAsset(m,dest);f.sharedMesh=m;}
  foreach(var m in r.sharedMaterials)if(m&&materials.Add(m)){log.Add("MATERIAL "+m.name+" cull="+(m.HasProperty("_Cull")?m.GetFloat("_Cull"):-1)+" surface="+(m.HasProperty("_Surface")?m.GetFloat("_Surface"):-1));if(m.HasProperty("_Cull"))m.SetFloat("_Cull",0);if(m.HasProperty("_Surface"))m.SetFloat("_Surface",0);m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");m.SetFloat("_ZWrite",1);m.SetOverrideTag("RenderType","Opaque");m.renderQueue=2000;EditorUtility.SetDirty(m);}
  var lod=r.GetComponentInParent<LODGroup>();if(lod){lod.fadeMode=LODFadeMode.None;var levels=lod.GetLODs();if(levels.Length>0){levels[levels.Length-1].screenRelativeTransitionHeight=0;lod.SetLODs(levels);lod.RecalculateBounds();}}
 }
 foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,FindObjectsSortMode.None)){c.nearClipPlane=.04f;c.useOcclusionCulling=false;}
 EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Directory.CreateDirectory("artifacts/s4-master/part-c/walls");File.WriteAllLines("artifacts/s4-master/part-c/walls/diagnosis.txt",log);
 }
 static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
}
