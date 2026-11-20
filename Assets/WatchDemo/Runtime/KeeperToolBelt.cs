using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Core;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo {
 [DefaultExecutionOrder(900)]public sealed class KeeperToolBelt:MonoBehaviour {
  public Transform Head {get;private set;}
  public WatchDemoController Watch {get;private set;}
  readonly List<BeltTool> tools=new();readonly List<(Transform clip,ToolBeltSlot slot)> clips=new();
  public static void Create(WatchDemoController watch,Transform head,XRGrabInteractable wrench){var belt=watch.gameObject.AddComponent<KeeperToolBelt>();belt.Watch=watch;belt.Head=head;belt.Build(wrench);}
  void Build(XRGrabInteractable wrench){
   var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();
   foreach(var slot in new[]{ToolBeltSlot.Wrench}){
    var t=M2StationParts.Root(slot+" waist clip",transform,Vector3.zero);clips.Add((t,slot));
    parts.Part("Leather tool loop",t,new Vector3(0,-.035f,-.025f),new Vector3(.065f,.075f,.016f),parts.Dark);
    parts.Part("Brass retention clip",t,new Vector3(.028f,0,0),new Vector3(.012f,.040f,.045f),parts.Brass);
    parts.Part("Lower loop",t,new Vector3(0,-.055f,0),new Vector3(.065f,.012f,.05f),parts.Dark);
   }
   Add(wrench,ToolBeltSlot.Wrench);
  }
  void Add(XRGrabInteractable grab,ToolBeltSlot slot){var tool=grab.gameObject.AddComponent<BeltTool>();tool.Initialize(this,grab,slot);tools.Add(tool);}
  public bool Occupied(ToolBeltSlot slot,BeltTool except){foreach(var tool in tools)if(tool&&tool!=except&&tool.BeltSlot==slot&&tool.Carried)return true;return false;}
  void LateUpdate(){if(!Head)return;foreach(var p in clips)p.clip.SetPositionAndRotation(ToolBeltPose.Position(p.slot,Head.position,Head.rotation),Quaternion.Euler(0,Head.eulerAngles.y,0));}
 }
 [DefaultExecutionOrder(900)]public sealed class BeltTool:MonoBehaviour,INetworkPropCarry,INetworkBeltSlot {
  KeeperToolBelt belt;XRGrabInteractable grab;Rigidbody body;AudioSource sound;AudioClip click;bool taught,throwNormally;
  public ToolBeltSlot BeltSlot {get;private set;}
  public bool Carried {get;private set;}
  public void Initialize(KeeperToolBelt owner,XRGrabInteractable interaction,ToolBeltSlot slot){
   belt=owner;grab=interaction;BeltSlot=slot;body=GetComponent<Rigidbody>();throwNormally=grab.throwOnDetach;
   foreach(var collider in grab.colliders)foreach(var rig in FindObjectsByType<CharacterController>(FindObjectsSortMode.None))Physics.IgnoreCollision(collider,rig,true);
   sound=new GameObject("Belt clip sound").AddComponent<AudioSource>();sound.transform.SetParent(transform,false);sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=.2f;sound.maxDistance=3;KeeperAudioMix.Route(sound,"SFX_World");click=Resources.Load<AudioClip>("Hands/clunk");
   grab.selectEntered.AddListener(Pickup);grab.selectExited.AddListener(Release);
  }
  void Pickup(SelectEnterEventArgs args){Carried=false;grab.throwOnDetach=throwNormally;if(!taught){taught=true;belt.Watch.GetComponent<ObjectiveHud>()?.ShowHint(KeeperStrings.Get(BeltSlot==ToolBeltSlot.Wrench?"belt.wrench":"belt.flashlight"),5);}}
  void Release(SelectExitEventArgs args){var head=belt.Head;if(args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor||!head||belt.Occupied(BeltSlot,this)||!ToolBeltPose.Near(BeltSlot,transform.position,head.position,head.rotation))return;
   Carried=true;grab.throwOnDetach=false;Follow();if(click)sound.PlayOneShot(click,.22f);args.interactorObject.transform.GetComponentInParent<HandRig>()?.Pulse(.2f,.05f);
  }
  void Follow(){if(!belt.Head)return;if(!body.isKinematic)body.linearVelocity=body.angularVelocity=Vector3.zero;body.isKinematic=true;transform.SetPositionAndRotation(ToolBeltPose.Position(BeltSlot,belt.Head.position,belt.Head.rotation),ToolBeltPose.Rotation(BeltSlot,belt.Head.rotation));}
  void LateUpdate(){if(Carried&&!grab.isSelected)Follow();}
  public void DropCarry(){Carried=false;}
  void OnDestroy(){if(grab){grab.selectEntered.RemoveListener(Pickup);grab.selectExited.RemoveListener(Release);}}
 }
}
