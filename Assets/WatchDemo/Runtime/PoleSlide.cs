using UnityEngine;using Unity.XR.CoreUtils;using UnityEngine.XR.Interaction.Toolkit;using UnityEngine.XR.Interaction.Toolkit.Interactables;using UnityEngine.XR.Interaction.Toolkit.Locomotion;using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo {
 // Two short drops use existing stair voids. Floors are never disabled or teleported through.
 // Releasing grip brakes to a pause on the pole; regrip continues. Bottom restores ordinary movement.
 public sealed class PoleSlide:MonoBehaviour {
  WatchDemoController watch;XROrigin rig;XRSimpleInteractable grab;XRBodyTransformer body;GravityProvider gravity;ContinuousMoveProvider move;readonly XROriginMovement motion=new();
  readonly System.Collections.Generic.List<Collider> openedGuards=new();Vector3 axis;float top,bottom,speed,stalled,previousY;bool latched,oldGravity,oldMove;BoxCollider gate;CharacterController capsule;Transform gateHinge;AudioSource sound;HandRig hand;
  public float Top=>top;public float Bottom=>bottom;public bool Latched=>latched;public float Speed=>speed;
  public static void Create(WatchDemoController w,XROrigin rig){var points=new[]{new Vector3(-2.20f,6.6f,2f),new Vector3(-1.40f,9.9f,1.20f)};foreach(var p in points){if(!Physics.CapsuleCast(p+Vector3.up*.25f,p+Vector3.up*1.5f,.24f,Vector3.down,out var hit,3.8f,~0,QueryTriggerInteraction.Ignore)||hit.distance<2){Debug.LogWarning("Pole shaft blocked at "+p);continue;}var go=new GameObject("Brass descent pole F"+(1+Mathf.RoundToInt(p.y/3.3f)));go.SetActive(false);var c=go.AddComponent<PoleSlide>();c.watch=w;c.rig=rig;c.axis=p;c.top=p.y;c.bottom=hit.point.y+.06f;c.Build();go.SetActive(true);}}
  void Build(){var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();parts.Brass.SetColor("_BaseColor",new Color(.32f,.20f,.07f));parts.Brass.SetColor("_EmissionColor",Color.black);parts.Brass.DisableKeyword("_EMISSION");parts.Brass.SetFloat("_Smoothness",.18f);parts.Brass.SetFloat("_Metallic",.55f);parts.Brass.SetFloat("_Cull",2);float shaftTop=top+2.6f;
   var shaftPosition=axis+Vector3.left*.28f;
   if(Physics.Raycast(shaftPosition+Vector3.up*1.3f,Vector3.up,out var ceiling,2.5f,~0,QueryTriggerInteraction.Ignore))shaftTop=ceiling.point.y;
   float height=shaftTop-bottom;transform.position=shaftPosition;
   parts.Part("Brass slide shaft",transform,new Vector3(0,(shaftTop+bottom)*.5f-top,0),new Vector3(.055f,height*.5f,.055f),parts.Brass,PrimitiveType.Cylinder);
   parts.Part("Pole lower mounting shoe",transform,new Vector3(0,bottom-top+.025f,0),new Vector3(.14f,.025f,.14f),parts.Dark,PrimitiveType.Cylinder);
   parts.Part("Pole ceiling mounting shoe",transform,new Vector3(0,shaftTop-top-.025f,0),new Vector3(.14f,.025f,.14f),parts.Dark,PrimitiveType.Cylinder);
   var trigger=gameObject.AddComponent<BoxCollider>();trigger.isTrigger=true;trigger.center=new Vector3(0,(bottom-top)*.5f+.6f,0);trigger.size=new Vector3(.25f,top-bottom+1.2f,.25f);grab=gameObject.AddComponent<XRSimpleInteractable>();grab.colliders.Clear();grab.colliders.Add(trigger);grab.selectMode=InteractableSelectMode.Multiple;grab.selectEntered.AddListener(Grab);
   var approach=(top<8?new Vector3(.39f,0,.39f):Vector3.forward*.55f).normalized;
   var across=Vector3.Cross(Vector3.up,approach);
   var surround=M2StationParts.Root("Pole landing railing",transform,Vector3.zero);surround.position=axis;surround.rotation=Quaternion.LookRotation(approach);
   // One enclosure replaces the overlapping visual survey rails. Existing collision stays intact.
   foreach(float x in new[]{-.36f,.36f})foreach(float z in new[]{-.32f,.44f})parts.Part("Pole landing upright",surround,new Vector3(x,.52f,z),new Vector3(.035f,1.04f,.035f),parts.Dark);
   foreach(float y in new[]{.5f,1.02f}){
    foreach(float x in new[]{-.36f,.36f})parts.Part("Pole landing side rail",surround,new Vector3(x,y,.06f),new Vector3(.035f,.035f,.76f),parts.Dark);
    parts.Part("Pole landing back rail",surround,new Vector3(0,y,-.32f),new Vector3(.72f,.035f,.035f),parts.Dark);
   }
   parts.Part("Pole brace collar",transform,new Vector3(0,1.02f,0),new Vector3(.075f,.025f,.075f),parts.Dark,PrimitiveType.Cylinder);
   gateHinge=M2StationParts.Root("Pole gate hinge",transform,Vector3.zero);
   gateHinge.position=axis+approach*.44f-across*.32f+Vector3.up*.85f;
   gateHinge.rotation=Quaternion.LookRotation(approach);
   var bar=parts.Part("Pole safety gate",gateHinge,new Vector3(.32f,0,0),new Vector3(.64f,.035f,.035f),parts.Dark);
   gate=bar.AddComponent<BoxCollider>();
   var plate=parts.Part("Pole engraved plate",gateHinge,new Vector3(.32f,-.10f,.025f),new Vector3(.30f,.10f,.018f),parts.Dark);
   parts.Text("Pole grip instruction",KeeperStrings.Get("pole.grip"),gateHinge,new Vector3(.32f,-.10f,.036f),.0024f).transform.localRotation=Quaternion.identity;
   capsule=rig.GetComponentInChildren<CharacterController>();
   body=rig.GetComponentInChildren<XRBodyTransformer>();gravity=rig.GetComponentInChildren<GravityProvider>();move=rig.GetComponentInChildren<ContinuousMoveProvider>();sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.loop=true;sound.spatialBlend=1;sound.minDistance=.5f;sound.maxDistance=6;sound.volume=.09f;sound.clip=Resources.Load<AudioClip>("ImmersionAudio/creak");
  }
  System.Collections.IEnumerator Start(){yield return null;
   foreach(var guard in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
    if(guard.name=="Surveyed stair opening guard"&&EntranceGuard(guard))foreach(var renderer in guard.GetComponentsInChildren<Renderer>())renderer.enabled=false;
  }
  bool EntranceGuard(BoxCollider guard)=>Mathf.Abs(guard.bounds.center.y-top-.55f)<.25f&&Vector3.ProjectOnPlane(guard.bounds.ClosestPoint(axis+Vector3.up*.55f)-axis,Vector3.up).magnitude<.85f;
  void Grab(SelectEnterEventArgs e){if(watch.State.Failed||watch.ObjectivePresentationBlocked||!body)return;if(!latched&&Mathf.Abs(rig.transform.position.y-top)>.35f)return;if(!latched){oldGravity=gravity&&gravity.enabled;oldMove=move&&move.enabled;if(gravity)gravity.enabled=false;if(move)move.enabled=false;latched=true;gate.enabled=false;gateHinge.Rotate(0,85,0,Space.Self);
   // Reconciled fall guards also cover the pole entrance. Open only this keeper's passage;
   // the other keeper and every other floor retain collision protection.
   foreach(var guard in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
    if(capsule&&guard.enabled&&(guard.name=="Surveyed stair opening guard"||guard.name.StartsWith(PlaytestWorldFixes.GuardPrefix))&&EntranceGuard(guard)){Physics.IgnoreCollision(capsule,guard,true);openedGuards.Add(guard);}
   previousY=rig.transform.position.y;stalled=0;}hand=e.interactorObject.transform.GetComponentInParent<HandRig>();if(hand)hand.Pulse(.18f,.08f);}
  void Update(){if(!latched)return;float dt=Time.deltaTime;bool held=grab.isSelected&&!watch.ObjectivePresentationBlocked;speed=Mathf.MoveTowards(speed,held?watch.State.Master.poleSpeed:0,watch.State.Master.poleAcceleration*dt);
   var p=rig.Camera.transform.position;p.y=rig.transform.position.y;var delta=Vector3.ProjectOnPlane(axis-p,Vector3.up);float vertical=delta.magnitude<.08f?Mathf.Min(speed*dt,Mathf.Max(0,p.y-bottom)):0;motion.motion=(held?Vector3.ClampMagnitude(delta,dt*.7f):Vector3.zero)+Vector3.down*vertical;body.QueueTransformation(motion,100);
   if(p.y<=bottom+.08f){Finish();return;}if(held&&Mathf.Abs(p.y-previousY)<.0001f&&delta.magnitude<.08f)stalled+=dt;else stalled=0;previousY=p.y;if(stalled>1){Finish();return;}
   if(speed>.1f){if(sound.clip&&!sound.isPlaying)sound.Play();}else sound.Stop();
  }
  void Finish(){foreach(var guard in openedGuards)if(guard&&capsule)Physics.IgnoreCollision(capsule,guard,false);openedGuards.Clear();latched=false;speed=0;if(gravity)gravity.enabled=oldGravity;if(move)move.enabled=oldMove;if(gate){gate.enabled=true;gateHinge.Rotate(0,-85,0,Space.Self);}if(sound)sound.Stop();}
  void OnDestroy(){if(latched)Finish();if(grab)grab.selectEntered.RemoveListener(Grab);}
 }
}