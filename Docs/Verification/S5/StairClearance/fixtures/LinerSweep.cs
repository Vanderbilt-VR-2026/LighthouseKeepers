using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using LighthouseKeepers.Editor.Environment;
public static class LinerSweep {
 [Serializable]public class One {public float f2,f3;public int nav,work,capsules,width,other;public string[] examples,workExamples;}
 [Serializable]public class Output {public One[] cases;public string scope;}
 static string output,previous;static int step;static Transform[] panels;static Vector3[] positions,scales;static List<One> reports;static readonly (float,float)[] offsets={(0,0),(.095f,0),(.10f,0),(.105f,0),(.10f,-.06f),(.10f,-.03f),(.10f,.04f),(.10f,.08f)};
 public static object Begin(string target){
  EditorUtility.audioMasterMute=true;if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Refusing to replace an open or dirty scene");
  output=target;previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;reports=new List<One>();step=0;
  EditorSceneManager.OpenScene(LighthouseBuilder.LabPath);foreach(var c in UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Include,FindObjectsSortMode.None))c.enabled=false;
  panels=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.name.StartsWith("Stairwell liner F2 F3 ")||x.name.StartsWith("Stairwell liner F3 F4 ")).ToArray();
  positions=panels.Select(x=>x.position).ToArray();scales=panels.Select(x=>x.localScale).ToArray();if(panels.Length==0)throw new Exception("No stair liners found");
  EditorApplication.update+=Advance;return new{started=true,count=panels.Length};
 }
 static void Advance(){
  try {
   if(step>=offsets.Length){Finish();return;}
   var (f2,f3)=offsets[step];for(int i=0;i<panels.Length;i++){
    float d=panels[i].name.StartsWith("Stairwell liner F2 F3 ")?f2:f3;var p=positions[i];float r=Mathf.Sqrt(p.x*p.x+p.z*p.z);panels[i].position=new Vector3(p.x*(r+d)/r,p.y,p.z*(r+d)/r);
    var scale=scales[i];scale.x*=((r+d)/r);panels[i].localScale=scale;
   }
   Physics.SyncTransforms();var nav=NavigationClearanceValidator.Check();var work=WorkPositionValidator.Check();
   reports.Add(new One{f2=f2,f3=f3,nav=nav.failures.Count,work=work.failures.Count,capsules=nav.failures.Count(x=>x.Contains("capsule blocked")),width=nav.failures.Count(x=>x.Contains("clear-width obstruction")),other=nav.failures.Count(x=>!x.Contains("capsule blocked")&&!x.Contains("clear-width obstruction")),examples=nav.failures.Take(12).ToArray(),workExamples=work.failures.Take(8).ToArray()});step++;
  }catch(Exception e){File.WriteAllText(output+".error",e.ToString());Finish();}
 }
 static void Finish(){EditorApplication.update-=Advance;File.WriteAllText(output,string.Join("\n",reports.Select(c=>$"f2={c.f2:F3} f3={c.f3:F3} nav={c.nav} work={c.work} capsules={c.capsules} width={c.width} other={c.other} first={string.Join(" | ",c.examples.Take(3))} workDetail={string.Join(" | ",c.workExamples)}")));if(!string.IsNullOrEmpty(previous))EditorSceneManager.OpenScene(previous);}
}
