using UnityEngine;
using UnityEditor;
using LighthouseKeepers.Puzzles;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Environment;
using LighthouseKeepers.Audio;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor {
public static class LighthouseRooms {
 public static void Table(Vector3 p){Box("Maintenance worktop",p+Vector3.up*.82f,new Vector3(1.6f,.1f,.7f),"Wood");foreach(float x in new[]{-.65f,.65f})foreach(float z in new[]{-.22f,.22f})Box("Workbench leg",p+new Vector3(x,.4f,z),new Vector3(.09f,.8f,.09f),"Iron");}
 public static PuzzleSocket Socket(string station,string role,Vector3 position,string intent){var go=new GameObject(station+" - "+role);go.transform.position=position;var anchor=new GameObject("Interaction Anchor").transform;anchor.SetParent(go.transform,false);var socket=go.AddComponent<PuzzleSocket>();LighthouseScenes.Set(socket,"stationId",station);LighthouseScenes.Set(socket,"interactionRole",role);LighthouseScenes.Set(socket,"futureIntent",intent);LighthouseScenes.Set(socket,"interactionAnchor",anchor);return socket;}
 public static GameObject Grab(string name,Vector3 p,Vector3 size,string mat){var go=Box(name,p,size,mat);go.isStatic=false;go.AddComponent<Rigidbody>().mass=.4f;go.AddComponent<XRGrabInteractable>();return go;}
 static void Gauge(Vector3 p){var dial=Cylinder("Analog pressure dial",p,new Vector3(.24f,.04f,.24f),"Paper",null,false);dial.transform.rotation=Quaternion.Euler(90,0,0);Box("Gauge needle",p+new Vector3(.03f,.04f,-.05f),new Vector3(.015f,.09f,.015f),"Iron",null,false);}
 static void Valve(Vector3 p){var wheel=Cylinder("Shutoff valve handwheel",p,new Vector3(.3f,.025f,.3f),"Rust");wheel.transform.rotation=Quaternion.Euler(90,0,0);Box("Valve spindle",p+Vector3.forward*.1f,new Vector3(.07f,.07f,.2f),"Brass");}
 static void Chart(string name,Vector3 p){Box(name,p,new Vector3(1,.7f,.03f),"Paper",null,false);for(int i=0;i<5;i++)Box("Faded chart rule",p+new Vector3(0,-.22f+i*.11f,-.02f),new Vector3(.85f,.008f,.005f),"Slate",null,false);}
 static void Common(int level,string clip,string group){float y=level*3.2f;var threshold=new GameObject("Level "+(level+1)+" flood threshold").AddComponent<FloodThreshold>();LighthouseScenes.Set(threshold,"threshold",y+.15f);
 var warning=Box("Flood warning lamp",new Vector3(3,y+2.3f,1),new Vector3(.12f,.2f,.12f),"Warning",null,false);warning.SetActive(false);UnityEditor.Events.UnityEventTools.AddPersistentListener(threshold.OnSubmergedChanged,warning.SetActive);
 var emitter=LighthouseAudioAssets.Emitter("Room-specific placeholder "+clip,new Vector3(-3,y+1,1.8f),clip,group,.3f);emitter.gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=3500;
 var reverb=new GameObject("Enclosed room acoustic zone").AddComponent<AudioReverbZone>();reverb.transform.position=new Vector3(0,y+1.5f,0);reverb.minDistance=2;reverb.maxDistance=4.5f;reverb.reverbPreset=level==0?AudioReverbPreset.Bathroom:AudioReverbPreset.Room;
 var creak=LighthouseAudioAssets.Emitter("Travelling structure creak "+level,new Vector3(2,y+2.8f,-2),"Creak","Structure",.25f,false);var random=creak.gameObject.AddComponent<RandomSoundEmitter>();LighthouseScenes.SetArray(random,"clips",new Object[]{Clip("Creak")});
 }
 public static void Plumbing(){Common(0,"Drip","Water");
  for(int i=0;i<3;i++){float z=-3.7f+i*.36f;Pipe("Corroded pressure main",new Vector3(-2,1.3f+i*.3f,z),new Vector3(2,1.3f+i*.3f,z),.13f+i*.07f,"Rust");for(int j=-1;j<=1;j++)Gauge(new Vector3(j*.75f,1.8f+i*.3f,z-.14f));}
  for(int i=-1;i<=1;i++)Valve(new Vector3(i*.75f,1.25f,-3.45f));
  Box("Sump pump motor",new Vector3(-3.4f,.55f,-1.6f),new Vector3(1.1f,1.1f,.85f),"Iron");Cylinder("Pump pressure chamber",new Vector3(-3.4f,1.3f,-1.6f),new Vector3(.7f,.45f,.7f),"Rust");
  Pipe("Damaged split pipe left",new Vector3(-2.3f,1,-3.2f),new Vector3(-.3f,1,-3.2f),.23f,"Iron");Pipe("Damaged split pipe right",new Vector3(.1f,1,-3.2f),new Vector3(2.1f,1,-3.2f),.23f,"Iron");
  Table(new Vector3(3.2f,0,-1.8f));Grab("Maintenance wrench",new Vector3(3.2f,.94f,-1.8f),new Vector3(.09f,.04f,.4f),"Iron");
  foreach(var data in new[]{("Wrench",new Vector3(-.8f,1,-2.6f)),("Brace",new Vector3(.8f,1,-2.6f)),("Leak",new Vector3(-.1f,1,-3.2f)),("Repair status",new Vector3(1.8f,1.6f,-3))})Socket("Plumbing Repair",data.Item1,data.Item2,"Hold or brace while another role tightens; no participant count enforced.");
  Socket("Pressure Sequence","Lower gauges",new Vector3(1.2f,1.8f,-3.4f),"Read changing pressure while remote role relays chart.");
  var water=MeshObject("Rising flood boundary",Annulus("FloodDisc",0,4.8f,0,0,0,360,64),"Water",false);water.AddComponent<FloodSurface>();
  for(int i=0;i<6;i++)Box("Floor drainage grate",new Vector3(-2+i*.8f,.02f,-3),new Vector3(.04f,.02f,.5f),"Iron",null,false);
  for(int i=0;i<3;i++)Box("Previous flood mark",new Vector3(-1+i*.6f,.65f,-4.73f),new Vector3(.48f,.035f,.02f),"Rust",null,false);
  var leak=LighthouseArchitecture.Rain(new Vector3(-.1f,1.1f,-3.2f),new Vector3(.1f,.05f,.1f),false);leak.name="Active pipe leak";
 }
 public static void Generator(){Common(1,"Generator","Machinery");float y=3.2f;
  Box("Generator vibration plinth",new Vector3(0,y+.15f,-3.5f),new Vector3(2.7f,.3f,1.3f),"Interior");
  Box("Diesel engine housing",new Vector3(-.3f,y+.85f,-3.5f),new Vector3(1.8f,1.15f,1),"Iron");
  for(int i=0;i<7;i++)Box("Engine cooling fin",new Vector3(-1+i*.24f,y+1,-3.5f),new Vector3(.08f,1.2f,1.1f),"Slate");
  var alternator=Cylinder("Generator alternator",new Vector3(1,y+.9f,-3.5f),new Vector3(.9f,.5f,.9f),"Brass");alternator.transform.rotation=Quaternion.Euler(0,0,90);
  Pipe("Exhaust to service shaft",new Vector3(-.8f,y+1.5f,-3.5f),new Vector3(-.8f,y+2.8f,-3.5f),.17f,"Rust");
  for(int i=0;i<3;i++)Cylinder("Oil storage drum",new Vector3(-3.6f,y+.5f,-1+i*.8f),new Vector3(.65f,.5f,.65f),i==1?"Rust":"Iron");
  Table(new Vector3(3.25f,y,-1.5f));Grab("Oil can",new Vector3(3.2f,y+1.05f,-1.5f),new Vector3(.22f,.32f,.18f),"Rust");
  Box("Generator fill port",new Vector3(-1.25f,y+1.3f,-3.3f),new Vector3(.25f,.12f,.25f),"Brass");Box("Separated starter lever",new Vector3(1.7f,y+1.1f,-3.25f),new Vector3(.12f,.5f,.12f),"Rust");
  Socket("Generator Service","Oil intake",new Vector3(-1.2f,y+1.4f,-2.8f),"Supply oil at intake; starter is separate.");Socket("Generator Service","Starter",new Vector3(1.7f,y+1.2f,-2.8f),"Operate starter; future power-state event hook.");Socket("Generator Service","Oil storage",new Vector3(3.2f,y+1,-1.5f),"Oil-can docking anchor.");
  Box("Breaker coordination panel",new Vector3(0,y+1.4f,4.65f),new Vector3(1.4f,1.3f,.2f),"Iron");for(int i=0;i<4;i++)Box("Breaker switch",new Vector3(-.45f+i*.3f,y+1.4f,4.48f),new Vector3(.12f,.25f,.1f),"Brass");Socket("Breaker Coordination","Breaker bank",new Vector3(0,y+1.4f,4.35f),"Coordinate with lantern status indicators.");Chart("Generator maintenance chart",new Vector3(1.5f,y+1.8f,4.2f));
 }
 public static void Communications(){Common(2,"Radio","Radio");float y=6.4f;
  Box("Keeper bunk frame",new Vector3(-3.5f,y+.4f,0),new Vector3(1.2f,.2f,2.2f),"Wood");Box("Folded wool bedding",new Vector3(-3.5f,y+.58f,0),new Vector3(1.1f,.2f,2.05f),"Cloth");Box("Pillow",new Vector3(-3.5f,y+.75f,.7f),new Vector3(.75f,.15f,.4f),"Paper",null,false);
  Table(new Vector3(0,y,-3.5f));Box("Radio receiver",new Vector3(0,y+1.12f,-3.5f),new Vector3(.9f,.5f,.4f),"Iron");for(int i=0;i<3;i++)Gauge(new Vector3(-.27f+i*.27f,y+1.18f,-3.27f));
  Grab("Handheld radio",new Vector3(.65f,y+1.05f,-3.4f),new Vector3(.12f,.28f,.09f),"Iron");
  Socket("Radio Relay","Push to talk",new Vector3(.25f,y+1,-3),"Physical radio event only; no voice or networking.");Socket("Radio Relay","Frequency dial",new Vector3(-.25f,y+1.2f,-3),"Future shared frequency clue.");Socket("Radio Relay","Walkie-talkie dock",new Vector3(.65f,y+1,-3.4f),"Future handheld radio docking.");
  Chart("Salt damaged weather map",new Vector3(0,y+2,4.65f));Socket("Pressure Sequence","Remote instruction chart",new Vector3(-.6f,y+1.6f,4.3f),"Relay lower-floor valve order.");Socket("Lantern Alignment","Bearing information",new Vector3(.6f,y+1.6f,4.3f),"Relay bearing to lantern control.");
  Table(new Vector3(3.4f,y,0));Box("Mess tin stove",new Vector3(3.4f,y+1,0),new Vector3(.55f,.25f,.4f),"Iron");Cylinder("Enamel mug",new Vector3(3.9f,y+.95f,.15f),new Vector3(.1f,.08f,.1f),"Paper",null,false);
  for(int i=0;i<3;i++)Box("Keeper shelf",new Vector3(3.2f,y+.5f+i*.6f,2),new Vector3(1.3f,.08f,.5f),"Wood");Box("Personal photograph frame",new Vector3(3.1f,y+1.35f,2),new Vector3(.22f,.25f,.05f),"Brass",null,false);
 }
 public static void Lantern(){Common(3,"Generator","Machinery");float y=9.6f;
  Cylinder("Beacon drive housing",new Vector3(0,y+.55f,0),new Vector3(1.6f,.55f,1.6f),"Iron");
  var lens=new GameObject("Rotating Fresnel assembly");lens.transform.position=new Vector3(0,y+1.2f,0);lens.AddComponent<BeaconRotation>();
  for(int i=0;i<9;i++)Cylinder("Fresnel prism ring",new Vector3(0,i*.15f,0),new Vector3(1.5f-Mathf.Abs(i-4)*.1f,.045f,1.5f-Mathf.Abs(i-4)*.1f),"Lens",lens.transform,false);
  for(int i=0;i<4;i++)Box("Lens brass frame",LighthouseArchitecture.Polar(.83f,i*90,.65f),new Vector3(.06f,1.5f,.06f),"Brass",lens.transform,false);
  var beam=new GameObject("Beacon beam light").AddComponent<Light>();beam.transform.SetParent(lens.transform,false);beam.transform.localPosition=new Vector3(0,.6f,0);beam.type=LightType.Spot;beam.color=new Color(.8f,.94f,1);beam.intensity=5;beam.range=28;beam.spotAngle=27;beam.shadows=LightShadows.None;
  MeshObject("Lantern balcony",Annulus("Balcony",5.1f,6.65f,y,0,0,360,96),"Iron");
  for(int i=0;i<48;i++){float a=i*7.5f;Pipe("Balcony railing",LighthouseArchitecture.Polar(6.55f,a,y+1.05f),LighthouseArchitecture.Polar(6.55f,a+7.5f,y+1.05f),.06f,"Iron");if(i%2==0)Pipe("Balcony post",LighthouseArchitecture.Polar(6.55f,a,y),LighthouseArchitecture.Polar(6.55f,a,y+1.05f),.045f,"Iron");var guard=Box("Balcony fall guard",LighthouseArchitecture.Polar(6.55f,a+3.75f,y+.55f),new Vector3(.08f,1.1f,.9f),"Iron");guard.transform.rotation=Quaternion.Euler(0,-a-3.75f,0);UnityEngine.Object.DestroyImmediate(guard.GetComponent<MeshRenderer>());}
  Box("Lantern controls",new Vector3(0,y+1,3.5f),new Vector3(1.4f,.35f,.7f),"Iron");for(int i=0;i<3;i++)Gauge(new Vector3(-.4f+i*.4f,y+1.3f,3.3f));Socket("Lantern Alignment","Bearing controls",new Vector3(0,y+1.3f,3.1f),"Align beacon using remote bearing information.");Socket("Breaker Coordination","Beacon status",new Vector3(1,y+1.4f,3.2f),"Status indicators linked to future breaker logic.");
  Cylinder("Lantern roof cap",new Vector3(0,y+3.3f,0),new Vector3(10.7f,.16f,10.7f),"Slate");
 }
}}
