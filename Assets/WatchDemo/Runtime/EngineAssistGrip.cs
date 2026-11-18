using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo {
 public sealed class EngineAssistGrip:MonoBehaviour {
  WatchDemoController watch;XRSimpleInteractable grip;Transform lever;bool held;float next;
  public static EngineAssistGrip Create(WatchDemoController w,Transform parent,M2StationParts parts){
   var t=M2StationParts.Root("GRIP_StartAssist",parent,new Vector3(.28f,1.10f,1.10f));t.gameObject.SetActive(false);
   var c=t.gameObject.AddComponent<EngineAssistGrip>();c.watch=w;
   parts.Part("Compression cable socket",t,Vector3.zero,new Vector3(.08f,.12f,.06f),parts.Metal);
   c.lever=M2StationParts.Root("Compression release lever",t,new Vector3(0,0,.06f));
   parts.Part("Lever steel shaft",c.lever,new Vector3(0,.05f,0),new Vector3(.018f,.12f,.018f),parts.Metal);
   parts.Part("Lever red grip",c.lever,new Vector3(0,.12f,0),new Vector3(.13f,.035f,.035f),parts.Red);
   parts.Text("Start assist brass stencil",KeeperStrings.Get("starter.assistPlate"),t,new Vector3(0,-.13f,.025f),.0018f);
   var col=t.gameObject.AddComponent<BoxCollider>();col.isTrigger=true;col.center=new Vector3(0,.07f,.07f);col.size=new Vector3(.20f,.20f,.17f);
   c.grip=t.gameObject.AddComponent<XRSimpleInteractable>();c.grip.colliders.Add(col);
   t.gameObject.AddComponent<PhysicalGrip>().Configure(c.lever,HandPose.WrapGrip);
   c.grip.selectEntered.AddListener(c.Grab);c.grip.selectExited.AddListener(c.Release);t.gameObject.SetActive(true);return c;
  }
  void Grab(SelectEnterEventArgs e){held=true;next=0;Send(true);}
  void Release(SelectExitEventArgs e){held=false;Send(false);}
  void Send(bool value){if(watch.State.Coop!=null)watch.State.Coop.Send(CoopAction.EngineAssist,0,value);else watch.EngineAssistCommand(1,value);}
  void Update(){if(held&&Time.unscaledTime>=next){next=Time.unscaledTime+.1f;Send(true);}lever.localRotation=Quaternion.Slerp(lever.localRotation,Quaternion.Euler(watch.State.StartAssist.Holder>0?35:0,0,0),1-Mathf.Exp(-12*Time.deltaTime));}
  void OnDisable(){if(held&&watch)Send(false);held=false;}
 }
}
