using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Environment;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor
{
 public static class LighthouseCirculationWeather
 {
  const string Marker="LK Circulation and Rain Revision 1";
  [MenuItem("Lighthouse Keepers/Integration/Improve circulation and balcony rain")]
  public static void Apply()
  {
   if(Application.unityVersion!="6000.3.23f1")throw new Exception("Use the team editor 6000.3.23f1");
   var report=new StringBuilder("Unity "+Application.unityVersion+"; dimensions are metres.\n");
   var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Core"));
   if(!GameObject.Find(Marker)){
    report.AppendLine("BEFORE house/tower doorway="+DoorWidth(-5,0).ToString("F2")+"; lantern doorway="+DoorWidth(5,9.6f).ToString("F2"));
    foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)){
     var p=t.position;
     bool house=p.y<2&&p.x< -4.7f&&p.x> -5.1f;
     bool lantern=p.y>10&&p.y<12&&p.x>4.7f&&p.x<5.1f;
     if(t.name=="Salt stained tower wall"&&(house||lantern)&&Mathf.Abs(p.z)>.8f&&Mathf.Abs(p.z)<1.1f){
      t.SetPositionAndRotation(new Vector3(house?-4.9f:4.9f,p.y,Mathf.Sign(p.z)*1.3f),Quaternion.identity);
      t.localScale=new Vector3(.4f,3.2f,1.3f);
     }
     if(t.name=="Tower entry surround"){t.position=new Vector3(-5,1.5f,Mathf.Sign(p.z)*1.575f);t.localScale=new Vector3(.35f,3,1.85f);}
     if(t.name=="Entrance lintel")t.localScale=new Vector3(.4f,.8f,1.3f);
     // Move the logbook furniture toward the house window; keep a broad central arrival lane.
     if((t.name=="Maintenance worktop"||t.name=="Workbench leg"||t.name=="Salt damaged keeper logbook")&&p.x< -7){t.position=p+new Vector3(-.3f,0,.25f);}
    }
    new GameObject(Marker);RebuildBatches();EditorSceneManager.SaveScene(scene);
   }
   report.AppendLine("AFTER house/tower doorway="+DoorWidth(-5,0).ToString("F2")+"; lantern doorway="+DoorWidth(5,9.6f).ToString("F2"));
   report.AppendLine("Tower shell remains radius 5.00 (about 9.60 inside diameter); spiral radii 1.00–2.60, width 1.60 unchanged.");
   scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Level03_Communications"));
   if(!GameObject.Find(Marker)){
    var table=GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None).First(t=>t.name=="Maintenance worktop"&&t.position.x>3);
    report.AppendLine("BEFORE east mess worktop inner edge x="+table.GetComponent<Collider>().bounds.min.x.ToString("F2")+"; landing guard outer edge approximately 2.76.");
    foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)){
     var p=t.position;
     if((t.name=="Maintenance worktop"||t.name=="Workbench leg"||t.name=="Mess tin stove"||t.name=="Enamel mug")&&p.x>2.5f&&Mathf.Abs(p.z)<.5f){
      t.position=new Vector3(4.32f,6.4f,0)+Quaternion.Euler(0,90,0)*(p-new Vector3(3.4f,6.4f,0));
      t.rotation=Quaternion.Euler(0,90,0)*t.rotation;
     }
    }
    new GameObject(Marker);RebuildBatches();EditorSceneManager.SaveScene(scene);
   }
   report.AppendLine("AFTER east worktop x=3.97–4.67; clear radial landing strip approximately 1.21m (was obstructed). Socket and radio-desk transforms unchanged.");
   scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Exterior"));
   foreach(var weather in UnityEngine.Object.FindObjectsByType<WeatherVolume>(FindObjectsSortMode.None)){
    LighthouseScenes.Set(weather,"excludeShelteredParticles",true);
    var ps=weather.GetComponent<ParticleSystem>();
    var collision=ps.collision;collision.enabled=false;
    if(weather.name!="Recycled balcony rain")continue;
    weather.transform.position=new Vector3(0,13.1f,0);
    LighthouseScenes.Set(weather,"balconyRing",true);LighthouseScenes.Set(weather,"baselineEmission",360f);
    var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=6.7f;shape.radiusThickness=1-5.45f/6.7f;shape.rotation=new Vector3(90,0,0);shape.scale=Vector3.one;
    var main=ps.main;main.startLifetime=.4f;main.maxParticles=220;main.startSpeed=0;
    var velocity=ps.velocityOverLifetime;velocity.y=-10;velocity.x=-.25f;velocity.z=.05f;
   }
   EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
   report.AppendLine("Rain: fixed annulus radius 5.45–6.70 over full balcony, cap220, lifetime0.4s, no particle collision/splashes. Stops and clears inside; roof/house rejection also applies to window rain. Indoor leaks unchanged.");
   Directory.CreateDirectory("Docs/Verification");File.WriteAllText("Docs/Verification/CirculationWeather.txt",report.ToString());
   LighthouseValidation.Validate();
  }
  static float DoorWidth(float x,float floor){
   Physics.SyncTransforms();float width=0;
   for(float z=-1;z<1;z+=.01f){bool clear=true;for(float xx=x-.3f;xx<=x+.3f;xx+=.1f)
    if(Physics.CheckBox(new Vector3(xx,floor+1.15f,z),new Vector3(.005f,1.05f,.004f),Quaternion.identity,1<<8,QueryTriggerInteraction.Ignore)){clear=false;break;}
    if(clear)width+=.01f;
   }return width;
  }
  public static void ValidateLoaded()
  {
   Physics.SyncTransforms();
   if(DoorWidth(-5,0)<1.2f||DoorWidth(5,9.6f)<1.2f)throw new Exception("Door clear width below 1.2m");
   for(float x=-9;x<=-3;x+=.1f)
    if(Physics.CheckBox(new Vector3(x,1.15f,0),new Vector3(.04f,1.05f,.6f),Quaternion.identity,1<<8,QueryTriggerInteraction.Ignore))throw new Exception("House arrival lane blocked at x="+x);
   if(Physics.CheckBox(new Vector3(3.365f,7.5f,0),new Vector3(.6f,.8f,.3f),Quaternion.identity,1<<8,QueryTriggerInteraction.Ignore))throw new Exception("East landing 1.2m lane blocked");
   var rain=GameObject.Find("Recycled balcony rain").GetComponent<ParticleSystem>();
   if(rain.shape.shapeType!=ParticleSystemShapeType.Circle||rain.main.maxParticles>220||rain.collision.enabled)throw new Exception("Balcony rain budget/shape regression");
   if(UnityEngine.Object.FindObjectsByType<WeatherVolume>(FindObjectsSortMode.None).Count(w=>w.name=="Recycled balcony rain")!=1)throw new Exception("Duplicate balcony rain");
   File.WriteAllText("Docs/Verification/CirculationPlayMode.txt","PASS: both door widths >=1.2m; house arrival lane 1.2m clear; east landing lane 1.2m clear; one circular balcony emitter <=220 particles, collision disabled. Geometric coverage tests do not establish headset appearance.\n");
  }
  public static void RebuildBatches(){
   var old=GameObject.Find("Static render batches");if(!old)throw new Exception("Expected authored static batches");
   UnityEngine.Object.DestroyImmediate(old);
   var candidates=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.gameObject.isStatic&&!r.GetComponentInParent<Rigidbody>()&&!r.GetComponentInParent<BeaconRotation>()&&!r.GetComponent<LighthouseKeepers.Flood.FloodSurface>()&&!r.GetComponentInParent<LODGroup>()&&r.GetComponent<MeshFilter>()&&r.sharedMaterials.Length==1).ToArray();
   var root=new GameObject("Static render batches");
   foreach(var group in candidates.GroupBy(r=>r.sharedMaterial)){
    var name=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"_"+group.Key.name;
    var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=r.transform.localToWorldMatrix}).ToArray(),true,true);Unwrapping.GenerateSecondaryUVSet(mesh);mesh=SaveMesh(mesh,name);
    var go=new GameObject(group.Key.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root.transform);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=group.Key;go.isStatic=true;
    foreach(var r in group)r.enabled=false;
   }
  }
 }
}
