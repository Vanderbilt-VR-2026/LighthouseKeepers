using System;using System.IO;using UnityEngine;using UnityEditor;using LighthouseKeepers.WatchDemo;
public static class WalterReview
{
 public static object Run(string output)
 {
  EditorUtility.audioMasterMute=true;Directory.CreateDirectory(output);
  var root=new GameObject("Walter review");root.transform.position=new Vector3(0,6.63f,0);var rig=WalterRig.Create(root.transform);
  var go=new GameObject("Review camera");var camera=go.AddComponent<Camera>();camera.fieldOfView=48;camera.nearClipPlane=.05f;camera.farClipPlane=60;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.025f,.03f);
  var rt=new RenderTexture(900,1100,24);camera.targetTexture=rt;
  void Take(string name,Vector3 view,float speed,float hunch){rig.Show(true,speed,.3f,hunch);camera.transform.position=root.transform.position+view;camera.transform.LookAt(root.transform.position+Vector3.up*1.15f);var skin=rig.GetComponent<SkinnedMeshRenderer>();var posed=new Mesh();skin.BakeMesh(posed);skin.enabled=false;var frozen=new GameObject("Posed review mesh");frozen.transform.SetParent(root.transform,false);frozen.AddComponent<MeshFilter>().sharedMesh=posed;var renderer=frozen.AddComponent<MeshRenderer>();renderer.sharedMaterial=skin.sharedMaterial;var block=new MaterialPropertyBlock();skin.GetPropertyBlock(block);renderer.SetPropertyBlock(block);camera.Render();UnityEngine.Object.DestroyImmediate(frozen);UnityEngine.Object.DestroyImmediate(posed);var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(output+"/"+name+".png",image.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}
  Take("front",new Vector3(0,1.4f,3.2f),1.2f,0);Take("side",new Vector3(2.5f,1.4f,1.6f),1.2f,0);Take("hunch",new Vector3(2.5f,1.4f,1.6f),1.2f,1);
  int triangles=rig.Triangles;var skin=rig.GetComponent<SkinnedMeshRenderer>();var baked=new Mesh();skin.BakeMesh(baked);float h=baked.bounds.max.y;
  UnityEngine.Object.DestroyImmediate(baked);UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(go);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
  return new{triangles,hunchedHeight=h};
 }
}
