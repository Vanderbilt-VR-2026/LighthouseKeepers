using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
namespace LighthouseKeepers.WatchDemo.Editor
{
 public static class KeeperMixSetup
 {
  [MenuItem("Lighthouse Keepers/Audio/Install master ceiling")]
  public static void Install()
  {
   var m=Resources.Load<AudioMixer>("KeeperAudio");var mt=m.GetType();var group=mt.GetProperty("masterGroup").GetValue(m);var property=group.GetType().GetProperty("effects");var old=(Array)property.GetValue(group);var et=old.GetValue(0).GetType();
   var e=old.Cast<object>().FirstOrDefault(x=>(string)et.GetProperty("effectName").GetValue(x)=="Compressor");
   if(e==null){e=Activator.CreateInstance(et,new object[]{"Compressor"});AssetDatabase.AddObjectToAsset((UnityEngine.Object)e,m);var array=Array.CreateInstance(et,old.Length+1);Array.Copy(old,array,old.Length);array.SetValue(e,old.Length);property.SetValue(group,array);}
   string[] names={"Threshold","Attack","Release"};float[] values={-1.5f,10,80};
   foreach(var snapshot in (Array)mt.GetProperty("snapshots").GetValue(m))for(int i=0;i<names.Length;i++)et.GetMethod("SetValueForParameter").Invoke(e,new object[]{m,snapshot,names[i],values[i]});
   EditorUtility.SetDirty((UnityEngine.Object)e);EditorUtility.SetDirty(m);AssetDatabase.SaveAssets();
  }
 }
}
