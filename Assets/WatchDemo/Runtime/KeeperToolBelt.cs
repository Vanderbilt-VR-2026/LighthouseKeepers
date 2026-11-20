using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Core;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Net;
namespace LighthouseKeepers.WatchDemo {
 [DefaultExecutionOrder(900)]public sealed class KeeperToolBelt:MonoBehaviour {
  public Transform Head {get;private set;}
  public WatchDemoController Watch {get;private set;}
  readonly List<BeltTool> tools=new();readonly List<(Transform clip,ToolBeltSlot slot)> clips=new();
  public static void Create(WatchDemoController watch,Transform head,XRGrabInteractable wrench){var belt=watch.gameObject.AddComponent<KeeperToolBelt>();belt.Watch=watch;belt.Head=head;belt.Build(wrench);}
  Transform Hips {get {var body=LocalKeeperBody.Current?LocalKeeperBody.Current.Body:null;return body&&body.gameObject.activeInHierarchy?body.Joint("Hips"):null;}}
  public Vector3 Position(ToolBeltSlot slot){var hip=Hips;return hip?ToolBeltPose.Position(slot,hip):ToolBeltPose.Position(slot,Head.position,Head.rotation);}
  public Quaternion Rotation(ToolBeltSlot slot){var hip=Hips;return ToolBeltPose.Rotation(slot,hip?hip.rotation:Head.rotation);}
  public Quaternion Facing {get {var hip=Hips;var rotation=hip?hip.rotation:Head.rotation;return Quaternion.Euler(0,rotation.eulerAngles.y,0);}}
  void Build(XRGrabInteractable wrench){
   var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();
   foreach(var slot in new[]{ToolBeltSlot.Radio,ToolBeltSlot.Wrench}){
    var t=M2StationParts.Root(slot+" waist clip",transform,Vector3.zero);clips.Add((t,slot));
    float side=slot==ToolBeltSlot.Radio?-1:1;
    parts.Part("Leather belt backing",t,new Vector3(side*.032f,-.015f,0),new Vector3(.014f,.11f,.065f),parts.Dark);
    parts.Part("Belt keeper",t,new Vector3(side*.034f,.045f,0),new Vector3(.014f,.055f,.07f),parts.Brass);
    parts.Part("Lower support",t,new Vector3(0,slot==ToolBeltSlot.Radio?-.095f:-.052f,0),new Vector3(.055f,.012f,.065f),parts.Dark);
    parts.Part("Outer retaining strap",t,new Vector3(-side*.030f,-.035f,0),new Vector3(.012f,.025f,.065f),parts.Dark);
   }
   Add(wrench,ToolBeltSlot.Wrench);
   UpdateClips();
  }
  void Add(XRGrabInteractable grab,ToolBeltSlot slot){var tool=grab.gameObject.AddComponent<BeltTool>();tool.Initialize(this,grab,slot);tools.Add(tool);}
  public bool Occupied(ToolBeltSlot slot,BeltTool except){foreach(var tool in tools)if(tool&&tool!=except&&tool.BeltSlot==slot&&tool.Carried)return true;return false;}
  void UpdateClips(){if(!Head)return;foreach(var p in clips)p.clip.SetPositionAndRotation(Position(p.slot),Facing);}
  void LateUpdate()=>UpdateClips();
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
  void Release(SelectExitEventArgs args){var head=belt.Head;if(args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor||!head||belt.Occupied(BeltSlot,this)||Vector3.Distance(transform.position,belt.Position(BeltSlot))>ToolBeltPose.SnapReach)return;
   Carried=true;grab.throwOnDetach=false;Follow();if(click)sound.PlayOneShot(click,.22f);args.interactorObject.transform.GetComponentInParent<HandRig>()?.Pulse(.2f,.05f);
  }
  void Follow(){if(!belt.Head)return;if(!body.isKinematic)body.linearVelocity=body.angularVelocity=Vector3.zero;body.isKinematic=true;transform.SetPositionAndRotation(belt.Position(BeltSlot),belt.Rotation(BeltSlot));}
  void LateUpdate(){if(Carried&&!grab.isSelected)Follow();}
  public void DropCarry(){Carried=false;}
  void OnDestroy(){if(grab){grab.selectEntered.RemoveListener(Pickup);grab.selectExited.RemoveListener(Release);}}
 }
}
