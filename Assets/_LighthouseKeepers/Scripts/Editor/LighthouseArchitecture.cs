using System;
using UnityEngine;
using UnityEditor;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor {
public static class LighthouseArchitecture {
 public static Vector3 Polar(float r,float degrees,float y){float a=degrees*Mathf.Deg2Rad;return new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r);}
 public static void Core(){
  for(int level=0;level<4;level++){
   float y=level*3.2f;
   MeshObject("Level "+(level+1)+" annular floor",Annulus("Floor_"+level,level==0?0:2.65f,5,y,0,0,360,64),level==0?"Interior":"Wood");
   // Radial walls include real window openings and a protected glass boundary.
   for(int i=0;i<32;i++){
    float angle=i*11.25f;bool door=(level==0&&i==16)||(level==3&&i==0);bool window=i%8==0&&!door;
    if(level==3)window=i%2==0;
    if(door){Wall("Entrance lintel",angle,y+2.75f,.9f,.8f);continue;}
    if(window){Wall("Window sill masonry",angle,y+.45f,.9f,.85f);Wall("Window lintel masonry",angle,y+2.85f,.9f,.7f);
     for(int side=-1;side<=1;side+=2){var jamb=Box("Window jamb",Polar(4.92f,angle+side*5.1f,y+1.7f),new Vector3(.17f,1.65f,.38f),"Masonry");jamb.transform.rotation=Quaternion.Euler(0,-angle,0);}
     var glass=Box("Clear safety window",Polar(5,angle,y+1.6f),new Vector3(.08f,1.5f,.85f),"Iron");UnityEngine.Object.DestroyImmediate(glass.GetComponent<MeshRenderer>());
     var bar=Box("Window iron mullion",Polar(4.91f,angle,y+1.6f),new Vector3(.08f,1.55f,.04f),"Iron");bar.transform.rotation=Quaternion.Euler(0,-angle,0);
    }else Wall("Salt stained tower wall",angle,y+1.6f,.99f,3.2f);
   }
   // A 1.6m wide spiral rises one full revolution per floor; collider follows a smooth ramp.
   if(level<3){MeshObject("Smooth spiral collision and stair bed",Annulus("SpiralRamp_"+level,1.0f,2.6f,y+.015f,3.2f,0,360,96,true),"Iron");
    for(int step=0;step<32;step++){
     float a=step*11.25f,sy=y+step*.1f;
     var tread=MeshObject("Stair tread",Annulus("Tread_"+level+"_"+step,1,2.6f,sy+.012f,0,a,11.25f,3),"Wood",false);
     // Visual steps have no colliders; the single ramp avoids headset vibration.
     if(step%2==0){Pipe("Spiral railing post",Polar(1.03f,a,sy),Polar(1.03f,a,sy+1.05f),.045f,"Iron");Pipe("Spiral handrail",Polar(1.03f,a,sy+1.05f),Polar(1.03f,a+22.5f,sy+1.25f),.05f,"Iron");}
    }
   }
   if(level>0){for(int i=2;i<31;i++){float a=i*11.25f;Pipe("Landing shaft rail",Polar(2.67f,a,y+1.05f),Polar(2.67f,a+11.25f,y+1.05f),.045f,"Iron");if(i%2==0)Pipe("Landing rail post",Polar(2.67f,a,y),Polar(2.67f,a,y+1.05f),.04f,"Iron");
     var barrier=Box("Shaft fall guard",Polar(2.68f,a+5.6f,y+.55f),new Vector3(.08f,1.1f,.56f),"Iron");barrier.transform.rotation=Quaternion.Euler(0,-a-5.6f,0);UnityEngine.Object.DestroyImmediate(barrier.GetComponent<MeshRenderer>());
    }}
   Pipe("Vertical utility riser",new Vector3(3.4f,y,-3.15f),new Vector3(3.4f,y+3.2f,-3.15f),.18f,"Rust");
   Lamp(new Vector3(2.8f,y+2.25f,.4f));
  }
  Cylinder("Central masonry spine",new Vector3(0,4.8f,0),new Vector3(1.8f,4.8f,1.8f),"Masonry");
  House();
  var probes=new GameObject("Interior light probes").AddComponent<LightProbeGroup>();var positions=new System.Collections.Generic.List<Vector3>();for(int l=0;l<4;l++)for(int a=0;a<8;a++)positions.Add(Polar(3.65f,a*45,l*3.2f+1.3f));probes.probePositions=positions.ToArray();
 }
 static void Wall(string name,float angle,float y,float width,float height){var go=Box(name,Polar(5,angle,y),new Vector3(.4f,height,width),"Masonry");go.transform.rotation=Quaternion.Euler(0,-angle,0);}
 public static void Lamp(Vector3 p){var go=Box("Caged tungsten service lamp",p,new Vector3(.18f,.28f,.15f),"Lamp",null,false);var light=go.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.65f,.31f);light.range=3.7f;light.intensity=1.8f;light.shadows=LightShadows.None;light.lightmapBakeType=LightmapBakeType.Mixed;}
 static void House(){
  Box("Keeper house floor",new Vector3(-8,-.12f,0),new Vector3(6,.24f,5),"Wood");
  Box("House end wall",new Vector3(-11,1.5f,0),new Vector3(.3f,3,5),"Masonry");
  foreach(int side in new[]{-1,1}){
   Box("House window sill wall",new Vector3(-8,.5f,side*2.5f),new Vector3(6,1,.3f),"Masonry");
   Box("House lintel",new Vector3(-8,2.65f,side*2.5f),new Vector3(6,.7f,.3f),"Masonry");
   foreach(float x in new[]{-10.7f,-8,-5.3f})Box("House wall pier",new Vector3(x,1.7f,side*2.5f),new Vector3(.4f,1.4f,.3f),"Masonry");
   var guard=Box("Storm window safety glass",new Vector3(-8,1.7f,side*2.5f),new Vector3(5.5f,1.4f,.08f),"Iron");UnityEngine.Object.DestroyImmediate(guard.GetComponent<MeshRenderer>());
  }
  foreach(int side in new[]{-1,1})Box("Tower entry surround",new Vector3(-5,1.5f,side*1.55f),new Vector3(.35f,3,1.9f),"Masonry");
  Box("Entry ceiling",new Vector3(-8,3.05f,0),new Vector3(6.4f,.15f,5.5f),"Wood");
  for(int side=-1;side<=1;side+=2){var roof=Box("Weathered slate roof",new Vector3(-8,3.7f,side*1.4f),new Vector3(6.7f,.15f,3.1f),"Slate");roof.transform.rotation=Quaternion.Euler(side*25,0,0);
   for(int j=0;j<9;j++){var tile=Box("Uneven roof repair",new Vector3(-11+j*.75f,3.76f,side*1.4f),new Vector3(.65f,.055f,3.13f),j%4==0?"Iron":"Slate",null,false);tile.transform.rotation=Quaternion.Euler(side*25,0,j%3-1);}}
  Box("Locked storm door",new Vector3(-10.82f,1.1f,0),new Vector3(.12f,2.2f,1.1f),"Wood");Box("Storm door brace",new Vector3(-10.7f,1.15f,0),new Vector3(.12f,.16f,1.4f),"Iron");
  Lamp(new Vector3(-8,2.65f,0));
  LighthouseRooms.Table(new Vector3(-8.8f,0,1.7f));
  Box("Salt damaged keeper logbook",new Vector3(-8.8f,.88f,1.7f),new Vector3(.35f,.06f,.25f),"Paper",null,false);
  for(int i=0;i<3;i++){Box("Raincoat on hook",new Vector3(-6.6f+i*.5f,1.5f,-2.18f),new Vector3(.36f,.95f,.16f),"Cloth",null,false);Box("Boots below coat",new Vector3(-6.6f+i*.5f,.18f,-2.12f),new Vector3(.3f,.35f,.38f),"Iron");}
 }
 public static void Exterior(){
  Cylinder("Cliff foundation",new Vector3(0,-2.1f,0),new Vector3(17,2,14),"Rock");
  var random=new System.Random(71);
  for(int i=0;i<48;i++){float a=i*7.5f;var p=Polar(8+(float)random.NextDouble()*4,a,-1.5f);var rock=Primitive(PrimitiveType.Sphere,"Jagged basalt outcrop",p,new Vector3(2+(float)random.NextDouble()*3,2+(float)random.NextDouble()*3,2),"Rock",null,false);rock.transform.rotation=Quaternion.Euler(i*23,i*37,i*17);var mesh=UnityEngine.Object.Instantiate(rock.GetComponent<MeshFilter>().sharedMesh);var vertices=mesh.vertices;for(int j=0;j<vertices.Length;j++)vertices[j]*=.8f+(float)random.NextDouble()*.35f;mesh.vertices=vertices;mesh.RecalculateNormals();rock.GetComponent<MeshFilter>().sharedMesh=SaveMesh(mesh,"Basalt_"+i);var lod=rock.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.025f,new[]{rock.GetComponent<Renderer>()}),new LOD(.008f,Array.Empty<Renderer>())});lod.RecalculateBounds();}
  Box("Storm ocean",new Vector3(0,-2.3f,0),new Vector3(400,.1f,400),"Water",null,false);
  for(int i=0;i<12;i++){var p=Polar(9,i*31,-.5f);Pipe("Dead coastal branch",p,p+new Vector3(.4f,1.5f,.1f),.07f,"Wood");Pipe("Wind bent branch",p+Vector3.up*.6f,p+new Vector3(1.1f,1.05f,.2f),.04f,"Wood");}
  for(int l=0;l<=4;l++)MeshObject("Exterior masonry course",Annulus("Cornice_"+l,5.02f,5.27f,l*3.2f+.05f,0,0,360,64),"Masonry",false);
  var sun=new GameObject("Storm daylight").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(.63f,.76f,.9f);sun.intensity=.65f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(38,-35,0);sun.lightmapBakeType=LightmapBakeType.Mixed;
  RenderSettings.sun=sun;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.26f,.34f,.42f);RenderSettings.ambientEquatorColor=new Color(.15f,.21f,.25f);RenderSettings.ambientGroundColor=new Color(.075f,.10f,.12f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.28f,.36f,.41f);RenderSettings.fogDensity=.018f;RenderSettings.skybox=null;
  foreach(int side in new[]{-1,1})Rain(new Vector3(-8,4,side*3),new Vector3(7,.1f,1.2f),false);
  Rain(new Vector3(0,14,0),new Vector3(12,.1f,12),true);
  for(int i=0;i<4;i++)Rain(Polar(5.6f,i*90,12),new Vector3(1.5f,.1f,1.5f),false);
  LighthouseAudioAssets.Emitter("Exterior rain on slate",new Vector3(-8,3,0),"RainExterior","Weather Exterior",.55f);
  LighthouseAudioAssets.Emitter("Surf below cliff",new Vector3(0,-2,0),"Ocean","Ocean",.8f).maxDistance=50;
 }
 public static GameObject Rain(Vector3 p,Vector3 size,bool follow){var go=new GameObject(follow?"Recycled balcony rain":"Window rain volume");go.transform.position=p;var ps=go.AddComponent<ParticleSystem>();var main=ps.main;main.startLifetime=1.5f;main.startSpeed=0;main.startSize=.022f;main.maxParticles=220;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startColor=new Color(.55f,.7f,.8f,.3f);var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=size;var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=-1.8f;velocity.y=-8;velocity.z=.3f;var emission=ps.emission;emission.rateOverTime=80;var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=4;renderer.velocityScale=.08f;renderer.sharedMaterial=M("Lens");var weather=go.AddComponent<LighthouseKeepers.Environment.WeatherVolume>();LighthouseScenes.Set(weather,"followOutsidePlayer",follow);return go;}
}}
