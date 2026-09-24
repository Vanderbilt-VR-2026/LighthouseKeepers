using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace LighthouseKeepers.Editor
{
 // Local, explicit file requests for the user's integration task. No sockets, credentials or runtime code.
 [InitializeOnLoad] public static class LighthouseEditorTask
 {
  [Serializable] sealed class Request { public string action;public string path; }
  static LighthouseEditorTask(){EditorApplication.update+=Poll;
   AssetDatabase.importPackageCompleted+=name=>{File.WriteAllText("Builds/EditorTaskResult.txt","IMPORT COMPLETED: "+name);if(SessionState.GetBool("LKImportBatch",false)){SessionState.SetBool("LKImportBatch",false);EditorApplication.delayCall+=()=>EditorApplication.Exit(0);}};
   AssetDatabase.importPackageFailed+=(name,error)=>File.WriteAllText("Builds/EditorTaskResult.txt","FAILED: "+name+": "+error);
  }
  public static void ImportPrepared(){SessionState.SetBool("LKImportBatch",true);Poll();}
  static void Poll(){
   const string file="Library/LKEditorTask.json";
   if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(file))return;
   var r=JsonUtility.FromJson<Request>(File.ReadAllText(file));File.Delete(file);
   try{
    if(r.action=="refresh")AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    else if(r.action=="import"){
     var full=Path.GetFullPath(r.path);var allowed=Path.GetFullPath("Builds/SelectedAssetPackages")+Path.DirectorySeparatorChar;
     if(!full.StartsWith(allowed,StringComparison.Ordinal)||!full.EndsWith(".unitypackage"))throw new Exception("Only prepared asset packages under Builds/SelectedAssetPackages are accepted");
     for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new Exception("Unsaved scene: "+SceneManager.GetSceneAt(i).path+". Save your scene before importing.");
     File.WriteAllText("Builds/EditorTaskResult.txt","IMPORT STARTED: "+full);
     AssetDatabase.ImportPackage(full,false);return;
    }else throw new Exception("Unknown integration request");
    File.WriteAllText("Builds/EditorTaskResult.txt",r.action+" completed: "+r.path);
   }catch(Exception e){File.WriteAllText("Builds/EditorTaskResult.txt","FAILED: "+e);}
  }
 }
}
