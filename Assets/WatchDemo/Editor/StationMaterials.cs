using System;using System.IO;using UnityEngine;using UnityEditor;
public static class StationMaterials {
 [MenuItem("Lighthouse Keepers/Visual Pass/Apply Station Materials")]
 public static void Apply(){
  var iron=Resources.Load<Material>("VisualMaterials/Iron");var brass=Resources.Load<Material>("VisualMaterials/Brass");
  foreach(var path in new[]{"Assets/Art/Sprint3/Materials/M_C_YellowEnamel.mat","Assets/Art/Sprint3/Materials/M_C_Iron.mat","Assets/Art/Sprint3/Materials/M_C_Steel.mat","Assets/WatchDemo/Radio/Cabinet.mat","Assets/WatchDemo/Radio/Brass.mat","Assets/WatchDemo/Polish/Brass.mat"}){
   var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m)continue;var colour=m.GetColor("_BaseColor");var source=path.Contains("Brass")?brass:iron;m.shader=source.shader;m.CopyPropertiesFromMaterial(source);if(path.Contains("Yellow")){m.SetTexture("_BaseMap",brass.GetTexture("_BaseMap"));m.SetColor("_BaseColor",new Color(.68f,.43f,.12f));}else if(!path.Contains("Brass"))m.SetColor("_BaseColor",colour);m.SetTextureScale("_BaseMap",new Vector2(2,2));m.SetTextureScale("_BumpMap",new Vector2(2,2));m.SetFloat("_Smoothness",path.Contains("Brass")?.6f:.3f);EditorUtility.SetDirty(m);
  }AssetDatabase.SaveAssets();
 }
}
