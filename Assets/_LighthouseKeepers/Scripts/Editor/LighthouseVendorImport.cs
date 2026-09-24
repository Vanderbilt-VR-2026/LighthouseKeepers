using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace LighthouseKeepers.Editor
{
 public static class LighthouseVendorImport
 {
  const string source="/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers/Builds/SelectedAssetPackages/";
  public static readonly string[] Factory={"Crane01_Motor","FanBig01Motor01","PowerBox01_1","PowerBox02_1","MetalCabinet01_1","Barrel01a","GasBallone01_2","FireExtinguisher01_1","Crane01_Wires04"};
  static readonly string[] Packages={"FactoryInspection","Abandoned Asylum","PBR - Hospital Horror Pack Free","Free Horror Ambience 2","Fog Particles","Procedural Water Shader","WaterURP"};
  static int index;
  public static void ImportAndInspect(){
   AssetDatabase.importPackageCompleted+=Completed;
   AssetDatabase.importPackageFailed+=(name,error)=>{Debug.LogError(name+": "+error);EditorApplication.Exit(1);};
   Next();
  }
  static void Next(){
   if(index==Packages.Length){Inspect();EditorApplication.Exit(0);return;}
   Debug.Log("IMPORTING SELECTED PACK "+Packages[index]);
   AssetDatabase.ImportPackage(source+Packages[index]+".unitypackage",false);
  }
  static void Completed(string name){index++;EditorApplication.delayCall+=Next;}
  public static void Inspect(){
   var report=new StringBuilder();
   foreach(var folder in new[]{"Assets/TirgamesAssets/Factory/Prefabs","Assets/Abandoned_Asylum/Prefabs","Assets/Dnk_Dev/HospitalHorrorPack/Prefab","Assets/Fog Particles/Prefabs"}){
    foreach(var path in AssetDatabase.FindAssets("t:Prefab",new[]{folder}).Select(AssetDatabase.GUIDToAssetPath)){
     if(folder.Contains("Tirgames")&&!Factory.Contains(Path.GetFileNameWithoutExtension(path)))continue;
     var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var renderers=prefab.GetComponentsInChildren<Renderer>(true);var bounds=new Bounds();bool first=true;
     foreach(var r in renderers){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
     int triangles=prefab.GetComponentsInChildren<MeshFilter>(true).Where(m=>m.sharedMesh).Sum(m=>(int)m.sharedMesh.GetIndexCount(0)/3);
     report.AppendLine(path+" | bounds="+bounds.ToString("F3")+" | triangles(submesh0)="+triangles+" | materials="+string.Join(",",renderers.SelectMany(r=>r.sharedMaterials).Where(m=>m).Select(m=>m.name+":"+m.shader.name).Distinct())+" | behaviours="+string.Join(",",prefab.GetComponentsInChildren<MonoBehaviour>(true).Select(b=>b?b.GetType().Name:"MISSING")));
    }
   }
   Directory.CreateDirectory("Docs/Verification");File.WriteAllText("Docs/Verification/VendorPrefabInventory.txt",report.ToString());
   var selected=Factory.Select(n=>"Assets/TirgamesAssets/Factory/Prefabs/"+n+".prefab").Concat(new[]{"Assets/TirgamesAssets/Factory/AbandonedFactoryLite.pdf"}).ToArray();
   AssetDatabase.ExportPackage(selected,source+"FactorySelected.unitypackage",ExportPackageOptions.IncludeDependencies);
   File.WriteAllLines("Docs/Verification/FactoryDependencies.txt",AssetDatabase.GetDependencies(selected,true));
   Debug.Log("VENDOR IMPORT AND INVENTORY COMPLETE");
  }
 }
}
