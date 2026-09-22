using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace LighthouseKeepers.Editor {
public static class LighthouseAssets {
 public const string Root="Assets/_LighthouseKeepers";
 public static Dictionary<string,Material> Materials=new();
 public static void Create(){
  Material("Masonry",new Color(.49f,.55f,.56f),.08f,.35f);
  Material("Interior",new Color(.34f,.39f,.39f),.05f,.25f);
  Material("Slate",new Color(.095f,.14f,.17f),.1f,.4f);
  Material("Iron",new Color(.085f,.12f,.13f),.65f,.45f);
  Material("Rust",new Color(.30f,.15f,.075f),.45f,.25f);
  Material("Wood",new Color(.24f,.19f,.13f),0,.2f);
  Material("Rock",new Color(.07f,.105f,.12f),.05f,.5f);
  Material("Cloth",new Color(.25f,.30f,.26f),0,.1f);
  Material("Paper",new Color(.63f,.59f,.43f),0,.15f);
  Material("Water",new Color(.025f,.11f,.14f),.5f,.88f);
  Material("Brass",new Color(.47f,.31f,.12f),.7f,.55f);
  Material("Lens",new Color(.24f,.7f,.75f),.4f,.85f,new Color(.13f,.38f,.4f));
  Material("Lamp",new Color(1,.64f,.24f),0,.4f,new Color(1,.5f,.15f)*2);
  Material("Warning",new Color(.7f,.13f,.025f),0,.4f,new Color(.8f,.08f,.01f));
  Material("Hands",new Color(.46f,.56f,.52f),0,.4f,new Color(.035f,.055f,.045f));
  foreach(string kind in new[]{"RainExterior","RainInterior","Ocean","Generator","Radio","Drip","Creak","Thunder"}) Audio(kind);
 }
 static void Material(string name,Color color,float metal,float smooth,Color emission=default){
  string path=Root+"/Art/Materials/LK_"+name+".mat";
  var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m){Materials[name]=m;return;}if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
  m.color=color;m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;
  if(emission.maxColorComponent>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
  else{
   string texPath=Root+"/Art/Textures/LK_"+name+"_Wear.png";
   if(!File.Exists(texPath)){
    var tex=new Texture2D(128,128,TextureFormat.RGB24,false);var rng=new System.Random(137);
    for(int y=0;y<128;y++)for(int x=0;x<128;x++){
     float grain=(float)rng.NextDouble()*.12f;float streak=Mathf.PerlinNoise(x*.18f,y*.012f)*.22f;
     float mortar=(name=="Masonry"||name=="Interior")&&(y%32<2||(x+(y/32%2)*32)%64<2)?.65f:1;
     float v=(.78f+grain-streak)*mortar;tex.SetPixel(x,y,new Color(v,v,v));
    }tex.Apply();File.WriteAllBytes(texPath,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(texPath);
    var imp=(TextureImporter)AssetImporter.GetAtPath(texPath);imp.maxTextureSize=128;imp.wrapMode=TextureWrapMode.Repeat;imp.SaveAndReimport();
   }
   m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texPath));m.SetTextureScale("_BaseMap",new Vector2(2,2));
  }
  EditorUtility.SetDirty(m);Materials[name]=m;
 }
 public static Material M(string name)=>Materials.TryGetValue(name,out var m)?m:AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/Materials/LK_"+name+".mat");
 public static GameObject Box(string name,Vector3 position,Vector3 scale,string material,Transform parent=null,bool collision=true)=>Primitive(PrimitiveType.Cube,name,position,scale,material,parent,collision);
 public static GameObject Primitive(PrimitiveType type,string name,Vector3 position,Vector3 scale,string material,Transform parent=null,bool collision=true){
  var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=M(material);
  if(collision&&type==PrimitiveType.Cylinder){UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.AddComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;}
  if(!collision)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
  else GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);
  return go;
 }
 public static GameObject Cylinder(string name,Vector3 p,Vector3 size,string mat,Transform parent=null,bool collision=true)=>Primitive(PrimitiveType.Cylinder,name,p,size,mat,parent,collision);
 public static void Pipe(string name,Vector3 a,Vector3 b,float diameter,string mat,Transform parent=null){var go=Cylinder(name,(a+b)/2,new Vector3(diameter,Vector3.Distance(a,b)/2,diameter),mat,parent);go.transform.rotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);}
 public static GameObject RootObject(string name)=>new GameObject(name);
 public static Mesh SaveMesh(Mesh mesh,string name){string path=Root+"/Art/Models/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 public static GameObject MeshObject(string name,Mesh mesh,string mat,bool collision=true){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=M(mat);if(collision)go.AddComponent<MeshCollider>().sharedMesh=mesh;go.isStatic=true;return go;}
 public static Mesh Annulus(string name,float inner,float outer,float bottom,float rise,float start,float angle,int segments,bool slope=false){
  var v=new List<Vector3>();var tris=new List<int>();var uv=new List<Vector2>();
  for(int i=0;i<segments;i++){
   float t0=(float)i/segments,t1=(float)(i+1)/segments;float a=(start+t0*angle)*Mathf.Deg2Rad,b=(start+t1*angle)*Mathf.Deg2Rad;
   float y0=bottom+(slope?t0*rise:0),y1=bottom+(slope?t1*rise:0);
   int n=v.Count;v.Add(new Vector3(Mathf.Cos(a)*inner,y0,Mathf.Sin(a)*inner));v.Add(new Vector3(Mathf.Cos(b)*inner,y1,Mathf.Sin(b)*inner));v.Add(new Vector3(Mathf.Cos(b)*outer,y1,Mathf.Sin(b)*outer));v.Add(new Vector3(Mathf.Cos(a)*outer,y0,Mathf.Sin(a)*outer));
   tris.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});uv.AddRange(new[]{new Vector2(0,t0*10),new Vector2(0,t1*10),new Vector2(outer-inner,t1*10),new Vector2(outer-inner,t0*10)});
  }
  var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(tris,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(mesh);return SaveMesh(mesh,name);
 }
 public static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/Ambience/Placeholder_"+name+".wav");
 static void Audio(string kind){
  string path=Root+"/Audio/Ambience/Placeholder_"+kind+".wav";if(File.Exists(path))return;
  int rate=22050,count=rate*(kind=="Thunder"?7:kind=="Creak"?3:4);var random=new System.Random(313+kind.Length);float low=0,slow=0;
  using(var w=new BinaryWriter(File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);
   for(int i=0;i<count;i++){float t=(float)i/rate,n=(float)random.NextDouble()*2-1;low+=.06f*(n-low);slow+=.004f*(n-slow);float f=kind switch {
    "RainExterior"=>n*.20f+low*.5f,"RainInterior"=>low*.8f+Mathf.Sin(t*93)*.015f,
    "Ocean"=>slow*3*(.6f+.3f*Mathf.Sin(t*1.57f)),"Generator"=>Mathf.Sin(t*2*Mathf.PI*55)*.09f+Mathf.Sin(t*2*Mathf.PI*110)*.03f+low*.08f,
    "Radio"=>n*.06f+Mathf.Sin(t*2100)*.01f,"Drip"=>Mathf.Sin(t*3500)*Mathf.Exp(-(t%1.3f)*45)*.2f,
    "Creak"=>Mathf.Sin(t*(180+30*Mathf.Sin(t*4)))*.12f*Mathf.Sin(Mathf.PI*i/count)+low*.08f,
    _=>slow*7*Mathf.Exp(-t*.5f)+low*.4f*Mathf.Exp(-t*.8f)};
    float edge=Mathf.Min(1,Mathf.Min(i,count-i-1)/220f);w.Write((short)(Mathf.Clamp(f*edge,-.95f,.95f)*32767));}
  }AssetDatabase.ImportAsset(path);var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.CompressedInMemory;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.5f;importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
 }
}}
