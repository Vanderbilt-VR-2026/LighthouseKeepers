using System.Collections.Generic;using UnityEngine;using UnityEngine.XR.Interaction.Toolkit.Interactables;using LighthouseKeepers.Core;using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class WatchDemoController {
  readonly WatchLantern[] lanterns=new WatchLantern[2];readonly List<Vector3> lanternHooks=new();M2CarryProp lanternOil;M2StationParts lanternParts;
  void CreateLanterns(){if(!state.Sprint5||!state.Sprint5.oilLanterns)return;lanternParts=gameObject.AddComponent<M2StationParts>();lanternParts.Initialize();lanternParts.Metal.SetColor("_BaseColor",new Color(.16f,.13f,.10f));lanternParts.Metal.SetColor("_EmissionColor",Color.black);
   // Wall brackets beside the House entrance; no free-floating fixture.
   var houseHook=WallLanternHook("House lantern hook 0",new Vector3(-2,1.45f,-6.1f),Vector3.left);
   if(houseHook.HasValue)lanterns[0]=WatchLantern.Create(this,0,houseHook.Value-LanternMesh.Handle);
   var engineHook=WallLanternHook("House lantern hook 1",new Vector3(-2,1.45f,-6.65f),Vector3.left);
   if(engineHook.HasValue)lanterns[1]=WatchLantern.Create(this,1,engineHook.Value-LanternMesh.Handle);
   if(!houseHook.HasValue||!engineHook.HasValue)Debug.LogError("Missing a supported lantern hook");
   foreach(var station in new[]{(Component)pipeRepair,engineControls,powerPanel,radio,beaconStation})if(station){var pos=station.transform.position;float y=Mathf.Floor((pos.y+.3f)/3.3f)*3.3f+1.45f;var radial=new Vector3(pos.x,0,pos.z).normalized;if(radial.sqrMagnitude<.5f)radial=Vector3.right;if(station==radio)radial=new Vector3(1,0,1).normalized;WallLanternHook(station.name+" lantern hook",new Vector3(0,y,0),radial);}
   var oilPosition=generator.transform.TransformPoint(new Vector3(-1.5f,.8f,.8f));
   var shelf=lanternParts.Part("Lantern oil shelf",transform,oilPosition-Vector3.up*.205f,new Vector3(.35f,.03f,.28f),lanternParts.Metal);
   // Shelf legs to the generator's plinth make its support explicit, with no rig collision.
   for(int i=-1;i<=1;i+=2)lanternParts.Part("Oil shelf bracket "+i,transform,oilPosition+new Vector3(i*.13f,-.5f,.09f),new Vector3(.025f,.58f,.025f),lanternParts.Metal);
   lanternOil=M2CarryProp.Create(lanternParts,transform,"LANTERN_OIL_CAN",oilPosition,false);lanternOil.SetHome(oilPosition,generator.transform.rotation);
   foreach(var t in lanternOil.GetComponentsInChildren<TextMesh>())t.text=KeeperStrings.Get("lantern.oil");
   foreach(var r in lanternOil.GetComponentsInChildren<Renderer>())if(r.name=="Pressed red fuel tank"||r.name.StartsWith("Handle side"))r.sharedMaterial=lanternParts.Dark;
   foreach(var rig in FindObjectsByType<CharacterController>(FindObjectsSortMode.None))foreach(var collider in lanternOil.Grab.colliders)Physics.IgnoreCollision(collider,rig,true);
   gameObject.AddComponent<LanternOilPour>().Initialize(this,lanternOil);
  }
  Vector3? WallLanternHook(string name,Vector3 from,Vector3 direction){RaycastHit chosen=default;float distance=20;
   for(int offset=0;offset<=45&&distance==20;offset+=5)foreach(int sign in new[]{-1,1})
    foreach(var h in Physics.RaycastAll(from,Quaternion.Euler(0,offset*sign,0)*direction,6,~0,QueryTriggerInteraction.Ignore))if((h.collider.name.Contains("Wall")||h.collider.name.Contains("Frame_Lantern"))&&h.distance<distance&&Mathf.Abs(h.normal.y)<.2f){chosen=h;distance=h.distance;}
   if(distance==20){float best=-2;foreach(var c in FindObjectsByType<Collider>(FindObjectsSortMode.None)){
    if(!c.enabled||!c.name.Contains("Frame_Lantern")||c.bounds.size.y<1||from.y<c.bounds.min.y||from.y>c.bounds.max.y)continue;
    var to=c.bounds.center;to.y=from.y;var d=(to-from).normalized;float alignment=Vector3.Dot(direction,d);if(alignment<=best)continue;
    if(c.Raycast(new Ray(from,d),out var hit,6)&&Mathf.Abs(hit.normal.y)<.2f){best=alignment;chosen=hit;distance=hit.distance;}
   }}
   if(distance==20)return null;var wall=chosen.point+chosen.normal*.025f;var hook=wall+chosen.normal*.18f;lanternHooks.Add(hook);
   var back=lanternParts.Part(name+" wall strap",transform,wall,new Vector3(.045f,.10f,.012f),lanternParts.Metal);back.transform.rotation=Quaternion.LookRotation(chosen.normal);
   var arm=lanternParts.Part(name+" bracket",transform,(wall+hook)*.5f,new Vector3(.018f,.018f,.18f),lanternParts.Metal);arm.transform.rotation=Quaternion.LookRotation(chosen.normal);
   lanternParts.Part(name+" turned hook",transform,hook-Vector3.up*.018f,new Vector3(.016f,.05f,.016f),lanternParts.Brass);
   var label=lanternParts.Text(name+" instruction",KeeperStrings.Get("lantern.hook"),transform,Vector3.zero,.002f);label.transform.SetPositionAndRotation(wall+Vector3.up*.16f+chosen.normal*.015f,Quaternion.LookRotation(-chosen.normal));return hook;
  }
  public Vector3? NearestLanternHook(Vector3 bail){Vector3? result=null;float best=.22f*.22f;foreach(var h in lanternHooks){float d=(h-bail).sqrMagnitude;if(d<best){result=h;best=d;}}return result;}
  public Component LanternTarget(float slot)=>float.IsFinite(slot)&&slot==(int)slot&&slot>=0&&slot<2?lanterns[(int)slot]?.Grab:null;
  public WatchLantern Lantern(int slot)=>slot>=0&&slot<2?lanterns[slot]:null;
  public bool AnyLanternHeld=>lanterns[0]&&lanterns[0].Held||lanterns[1]&&lanterns[1].Held;
  public bool HeldLanternLowOil=>lanterns[0]&&lanterns[0].Held&&lanterns[0].Oil<.2f||lanterns[1]&&lanterns[1].Held&&lanterns[1].Oil<.2f;
  public int HeldLantern=>lanterns[0]&&lanterns[0].Grab.isSelected?0:lanterns[1]&&lanterns[1].Grab.isSelected?1:-1;
 }
 public sealed class LanternOilPour:MonoBehaviour {
  WatchDemoController watch;M2CarryProp can;Renderer[] surfaces;bool active=true;
  public void Initialize(WatchDemoController w,M2CarryProp c){watch=w;can=c;surfaces=c.GetComponentsInChildren<Renderer>();}
  void Update(){var s=watch.State;bool enabled=s.Lanterns!=null;if(enabled!=active){active=enabled;foreach(var r in surfaces)r.enabled=enabled;can.Grab.enabled=enabled;foreach(var c in can.Grab.colliders)c.enabled=enabled;}
   if(s.ReadOnly||s.Failed||watch.Sleeping||s.Lanterns==null)return;bool held=can.Grab.isSelected||s.Coop?.RemoteHeld(can.Grab)==true;if(!held||Vector3.Dot(can.transform.up,Vector3.down)<.25f)return;
   for(int i=0;i<2;i++){var lamp=watch.Lantern(i);if(!lamp)continue;var d=can.Tip.position-lamp.Filler;if(d.y>=-.025f&&d.y<.3f&&new Vector2(d.x,d.z).sqrMagnitude<.13f*.13f){s.Lanterns.Pour(i,Time.deltaTime);break;}}
  }
 }
}
