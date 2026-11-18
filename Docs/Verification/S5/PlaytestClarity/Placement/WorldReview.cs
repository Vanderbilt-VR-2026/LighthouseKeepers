using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using LighthouseKeepers.WatchDemo;
public static class WorldReview {
 public static object Run(string folder){EditorUtility.audioMasterMute=true;Directory.CreateDirectory(folder);void Need(bool v,string message){if(!v)throw new Exception(message);}
 Need(!GameObject.Find("Surveyed stair opening guard"),"Internal stair guards remain");Need(GameObject.Find("Continuous gallery perimeter guard"),"Exterior guard missing");Need(!GameObject.Find("Beacon motor service lamp")&&!GameObject.Find("Engraved lens cleaning tag"),"Floating beacon fittings remain");
 var support=UnityEngine.Object.FindAnyObjectByType<RadioSupportLayout>();Need(support&&support.RemovedBracketTriangles==48,"Expected exactly four old bracket cubes removed; got "+support?.RemovedBracketTriangles);
 var radio=UnityEngine.Object.FindAnyObjectByType<WatchRadioConsole>();float outside=-100;foreach(var f in radio.GetComponentsInChildren<MeshFilter>()){if(!f.sharedMesh||!f.sharedMesh.isReadable||!f.name.Contains("ConsoleBody")&&f.name!="Radio frame support arms")continue;foreach(var index in f.sharedMesh.triangles){var p=f.transform.TransformPoint(f.sharedMesh.vertices[index]);outside=Mathf.Max(outside,p.z+.414214f*Mathf.Abs(p.x)-2.735f);}}
 Need(outside<.015f,"Console or support crosses north glazing plane: "+outside);
 var lamps=UnityEngine.Object.FindObjectsByType<WatchLantern>(FindObjectsSortMode.None);Need(lamps.Length==2&&lamps.Any(x=>x.transform.position.y>3.3f&&x.transform.position.y<5)&&lamps.Any(x=>x.transform.position.z< -5),"Lantern homes do not cover House and F2");
 var notes=UnityEngine.Object.FindObjectsByType<KettleReadable>(FindObjectsSortMode.None);Need(notes.Length==4&&notes.All(n=>n.GetComponent<Renderer>().enabled),"Existing lore unexpectedly hidden until waking");
 File.WriteAllText(folder+"/checks.json",JsonUtility.ToJson(new Result{passed=true,removedBracketTriangles=support.RemovedBracketTriangles,glazingMargin=-outside,lanterns=lamps.Length,notes=notes.Length},true));return true;}
 [Serializable]class Result{public bool passed;public int removedBracketTriangles,lanterns,notes;public float glazingMargin;}
}
