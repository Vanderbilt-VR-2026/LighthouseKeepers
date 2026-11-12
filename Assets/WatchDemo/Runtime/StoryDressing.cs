using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo {
 public sealed class StoryDressing:MonoBehaviour {
  WatchDemoController watch;M2StationParts parts;WatchBeacon beacon;Material ink,paper,oilskin;Renderer core;readonly List<Material> owned=new();MaterialPropertyBlock block;Mesh ropeMesh;
  public static void Create(WatchDemoController w){if(!VisualPassSettings.Current||!VisualPassSettings.Current.setDressing)return;var p=w.gameObject.AddComponent<StoryDressing>();p.watch=w;}
  void Start(){parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();beacon=FindAnyObjectByType<WatchBeacon>();block=new MaterialPropertyBlock();
   // Keep the shell's actual glazing. Detached procedural rain panes floated outside
   // the F1 recess and read as a sheet of white dots in the headset.
   var bulb=parts.Part("Amber lamp inside glass lens",beacon.rotor,new Vector3(0,.44f,0),new Vector3(.16f,.64f,.16f),parts.Brass,PrimitiveType.Capsule);core=bulb.GetComponent<Renderer>();
   paper=Mat("Logbook paper",new Color(.58f,.49f,.30f));ink=Mat("Aged photograph",new Color(.12f,.11f,.08f));oilskin=Mat("Keeper oilskin",new Color(.43f,.30f,.08f));
   var desk=GameObject.Find("Wrench table top");if(desk){var bounds=desk.GetComponent<Renderer>().bounds;var p=new Vector3(bounds.center.x,bounds.max.y+.018f,bounds.max.z-.22f);var book=M2StationParts.Root("Previous keeper open logbook",transform,Vector3.zero);book.position=p;book.rotation=Quaternion.Euler(0,90,0);parts.Part("Leather logbook cover",book,Vector3.zero,new Vector3(.34f,.02f,.25f),parts.Dark);parts.Part("Open logbook paper",book,new Vector3(0,.013f,0),new Vector3(.32f,.008f,.23f),paper);var text=parts.Text("Previous keeper entry",KeeperStrings.Get("story.logbook"),book,new Vector3(0,.020f,0),.0014f);text.transform.localRotation=Quaternion.Euler(90,0,0);text.color=new Color(.08f,.07f,.05f);}
   var photo=M2StationParts.Root("Old keeper photograph",transform,new Vector3(-3.43f,1.55f,-9.9f));photo.rotation=Quaternion.Euler(0,90,0);parts.Part("Photograph frame",photo,Vector3.zero,new Vector3(.28f,.34f,.03f),Resources.Load<Material>("VisualMaterials/Wood"));parts.Part("Sepia photograph paper",photo,new Vector3(0,0,.02f),new Vector3(.23f,.29f,.008f),paper);parts.Part("Photograph head silhouette",photo,new Vector3(0,.055f,.027f),new Vector3(.07f,.085f,.004f),ink,PrimitiveType.Sphere);parts.Part("Photograph coat silhouette",photo,new Vector3(0,-.055f,.027f),new Vector3(.14f,.14f,.004f),ink);
   for(int n=0;n<7;n++){var tally=parts.Part("Previous watch tally scratch",transform,new Vector3(-3.43f,1.17f,-9.5f+n*.035f),new Vector3(.002f,.08f,.008f),paper);tally.transform.localRotation=Quaternion.Euler(n==4?35:0,0,0);}
  }
  Material Mat(string name,Color colour){var m=new Material(Resources.Load<Material>("VisualMaterials/Wood")){name=name};m.SetTexture("_BaseMap",null);m.SetTexture("_BumpMap",null);m.DisableKeyword("_NORMALMAP");m.SetColor("_BaseColor",colour);owned.Add(m);return m;}
  void LateUpdate(){if(core){block.Clear();block.SetColor("_BaseColor",new Color(.9f,.55f,.2f));block.SetColor("_EmissionColor",watch.State.BeaconShining?new Color(1.2f,.65f,.18f):Color.black);core.SetPropertyBlock(block);}}
  void OnDestroy(){if(ropeMesh)Destroy(ropeMesh);foreach(var m in owned)if(m)Destroy(m);}
 }
}
