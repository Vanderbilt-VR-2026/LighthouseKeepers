using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Editor {
public static class LighthouseVendorVariants {
 public const string Root="Assets/_LighthouseKeepers/ThirdPartyVariants";
 public static readonly string[] Sources={
 "Assets/TirgamesAssets/Factory/Prefabs/Crane01_Motor.prefab",
 "Assets/TirgamesAssets/Factory/Prefabs/FanBig01Motor01.prefab",
 "Assets/TirgamesAssets/Factory/Prefabs/PowerBox02_1.prefab",
 "Assets/TirgamesAssets/Factory/Prefabs/MetalCabinet01_1.prefab",
 "Assets/TirgamesAssets/Factory/Prefabs/Barrel01a.prefab",
 "Assets/TirgamesAssets/Factory/Prefabs/FireExtinguisher01_1.prefab",
 "Assets/Abandoned_Asylum/Prefabs/TableKitchen.prefab",
 "Assets/Abandoned_Asylum/Prefabs/TableOffice.prefab",
 "Assets/Abandoned_Asylum/Prefabs/ChairSchool.prefab",
 "Assets/Abandoned_Asylum/Prefabs/Pipe.prefab",
 "Assets/Abandoned_Asylum/Prefabs/SmallMetalicCase.prefab",
 "Assets/Dnk_Dev/HospitalHorrorPack/Prefab/P_Lamp.prefab"};
 public static Bounds BoundsOf(GameObject go){var rs=go.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
 public static void Create(){
  foreach(var d in new[]{"Prefabs","Materials","Textures","Audio","Shaders","Meshes"})Directory.CreateDirectory(Root+"/"+d);AssetDatabase.Refresh();
  foreach(var path in Sources){
   string name=Path.GetFileNameWithoutExtension(path),target=Root+"/Prefabs/LK_"+name+".prefab";
   if(File.Exists(target))continue;
   var root=new GameObject("LK_"+name);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));model.transform.SetParent(root.transform);model.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
   foreach(var c in model.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(c);
   foreach(var c in model.GetComponentsInChildren<Light>(true))UnityEngine.Object.DestroyImmediate(c);
   foreach(var c in model.GetComponentsInChildren<MonoBehaviour>(true))if(c)UnityEngine.Object.DestroyImmediate(c);
   foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(Convert).ToArray();
   var b=BoundsOf(root);model.transform.position-=new Vector3(b.center.x,b.min.y,b.center.z);b=BoundsOf(root);
   var box=root.AddComponent<BoxCollider>();box.center=b.center;box.size=b.size;
   foreach(var t in root.GetComponentsInChildren<Transform>()){t.gameObject.layer=8;t.gameObject.isStatic=true;}
   PrefabUtility.SaveAsPrefabAsset(root,target);UnityEngine.Object.DestroyImmediate(root);
  }
  AssetDatabase.SaveAssets();
 }
 static Material Convert(Material source){
  if(!source)throw new Exception("Missing vendor material");
  string path=Root+"/Materials/"+source.name+"_"+AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)).Substring(0,8)+".mat";
  var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat)return mat;
  mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=source.name+"_Quest",enableInstancing=true};
  var tex=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):null;
  if(tex)mat.SetTexture("_BaseMap",TextureCopy(tex,false));
  mat.SetColor("_BaseColor",new Color(.82f,.86f,.89f));mat.SetFloat("_Smoothness",.25f);mat.SetFloat("_Metallic",source.HasProperty("_Metallic")?Mathf.Min(.5f,source.GetFloat("_Metallic")):0);
  if(source.HasProperty("_BumpMap")&&source.GetTexture("_BumpMap")){mat.SetTexture("_BumpMap",TextureCopy(source.GetTexture("_BumpMap"),true));mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.6f);}
  AssetDatabase.CreateAsset(mat,path);return mat;
 }
 static Texture TextureCopy(Texture source,bool normal){
  string src=AssetDatabase.GetAssetPath(source),path=Root+"/Textures/"+AssetDatabase.AssetPathToGUID(src)+Path.GetExtension(src);
  if(!File.Exists(path)){AssetDatabase.CopyAsset(src,path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.maxTextureSize=1024;imp.isReadable=false;imp.mipmapEnabled=true;imp.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
   var platform=imp.GetPlatformTextureSettings("Android");platform.overridden=true;platform.maxTextureSize=1024;platform.format=TextureImporterFormat.ASTC_6x6;imp.SetPlatformTextureSettings(platform);imp.SaveAndReimport();}
  return AssetDatabase.LoadAssetAtPath<Texture>(path);
 }
 public static void Preview(){
  Create();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  RenderSettings.ambientLight=new Color(.6f,.65f,.7f);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
  var light=new GameObject("Preview light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(40,-30,0);
  var camera=new GameObject("Preview camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.15f,.18f,.21f);camera.fieldOfView=40;camera.nearClipPlane=.01f;
  Directory.CreateDirectory("Docs/Verification/VendorPreviews");
  foreach(var path in Sources){string name=Path.GetFileNameWithoutExtension(path);var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/LK_"+name+".prefab"));var b=BoundsOf(go);float size=Mathf.Max(b.size.x,b.size.y,b.size.z);camera.transform.position=b.center+new Vector3(1,.65f,-1.7f).normalized*size*2.3f;camera.transform.LookAt(b.center);
   var rt=new RenderTexture(640,640,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var img=new Texture2D(640,640,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,640,640),0,0);img.Apply();File.WriteAllBytes("Docs/Verification/VendorPreviews/"+name+".png",img.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(img);UnityEngine.Object.DestroyImmediate(go);
  }Debug.Log("VENDOR VARIANTS AND PREVIEWS COMPLETE");
 }
}}
