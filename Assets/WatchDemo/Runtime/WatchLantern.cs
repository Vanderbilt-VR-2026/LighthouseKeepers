using UnityEngine;using UnityEngine.Rendering;using UnityEngine.XR.Interaction.Toolkit;using UnityEngine.XR.Interaction.Toolkit.Interactables;using LighthouseKeepers.Core;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Gameplay.Interaction;using LighthouseKeepers.Interaction;using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo {
 // Legacy component/command identifiers keep the existing two prop slots and replica payload intact.
 [DefaultExecutionOrder(100)] public sealed class WatchLantern:MonoBehaviour,IHandPoseTarget,INetworkReleasedBody,INetworkPropRelease {
  public int Slot {get;private set;} public XRGrabInteractable Grab {get;private set;} public bool ReleasedBodyIsKinematic=>!released;public HandPose RequiredPose=>HandPose.WrapGrip;
  public bool Held=>Grab.isSelected||lease?.Owner>0;public float Oil=>watch.State.Lanterns?.Oil(Slot)??0;
  WatchDemoController watch;Rigidbody body;INetworkPropStatus lease;HandRig hand;Vector3 home;bool released,usable=true;int lastWick=-1;float activateAt;Light pool;Transform leftPalm,rightPalm;Renderer[] surfaces;AudioSource sound;AudioClip clunk;Material metal,glass;Mesh mesh,globe;MaterialPropertyBlock block;readonly RaycastHit[] sightHits=new RaycastHit[24];Renderer lensRenderer;
  public static WatchLantern Create(WatchDemoController w,int slot,Vector3 basePosition){var go=new GameObject("KEEPER_FLASHLIGHT_"+slot);go.SetActive(false);go.transform.SetParent(w.transform,false);go.transform.position=basePosition;var l=go.AddComponent<WatchLantern>();l.watch=w;l.Slot=slot;l.home=basePosition;l.Build();go.SetActive(true);return l;}
  void Build(){
   metal=new Material(Resources.Load<Material>("VisualMaterials/Iron")){name="Worn olive steel flashlight"};metal.SetColor("_BaseColor",new Color(.15f,.17f,.105f));metal.SetFloat("_Smoothness",.32f);metal.SetColor("_EmissionColor",Color.black);mesh=FlashlightMesh.Body();Surface("Ribbed flashlight handle and angled head",mesh,metal);
   glass=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Flashlight reflector and lens"};glass.SetColor("_BaseColor",new Color(.40f,.43f,.37f));glass.SetFloat("_Metallic",.4f);glass.SetFloat("_Smoothness",.8f);glass.EnableKeyword("_EMISSION");globe=FlashlightMesh.Lens();lensRenderer=Surface("Recessed flashlight lens",globe,glass).GetComponent<Renderer>();block=new MaterialPropertyBlock();
   pool=new GameObject("Focused flashlight beam").AddComponent<Light>();pool.transform.SetParent(transform,false);pool.transform.localPosition=FlashlightMesh.Emitter+Vector3.forward*.004f;pool.type=LightType.Spot;pool.spotAngle=HorrorTuning.Current.lanternHighBeamAngle;pool.innerSpotAngle=HorrorTuning.Current.lanternHighBeamInner;pool.shadows=LightShadows.None;pool.color=new Color(1,.85f,.64f);pool.enabled=false;
   body=gameObject.AddComponent<Rigidbody>();body.mass=.65f;body.isKinematic=true;body.useGravity=true;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
   var c=gameObject.AddComponent<BoxCollider>();c.center=new Vector3(0,-.005f,0);c.size=new Vector3(.036f,.20f,.036f);var head=gameObject.AddComponent<BoxCollider>();head.center=new Vector3(0,.115f,.024f);head.size=new Vector3(.074f,.074f,.083f);
   foreach(var rig in FindObjectsByType<CharacterController>(FindObjectsSortMode.None)){Physics.IgnoreCollision(c,rig,true);Physics.IgnoreCollision(head,rig,true);}
   Grab=gameObject.AddComponent<XRGrabInteractable>();Grab.colliders.Add(c);Grab.colliders.Add(head);Grab.movementType=XRBaseInteractable.MovementType.Instantaneous;Grab.attachTransform=M2StationParts.Root("Flashlight handle grip",transform,FlashlightMesh.Handle);Grab.useDynamicAttach=false;Grab.throwOnDetach=false;Grab.smoothPosition=false;Grab.smoothRotation=false;Grab.selectEntered.AddListener(Hold);Grab.selectExited.AddListener(Release);Grab.activated.AddListener(Wick);
   leftPalm=M2StationParts.Root("Left wrap around flashlight",transform,FlashlightMesh.Handle);leftPalm.localRotation=Quaternion.LookRotation(Vector3.left,Vector3.back);
   rightPalm=M2StationParts.Root("Right wrap around flashlight",transform,FlashlightMesh.Handle);rightPalm.localRotation=Quaternion.LookRotation(Vector3.right,Vector3.back);
   surfaces=GetComponentsInChildren<Renderer>();sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=.25f;sound.maxDistance=8;sound.dopplerLevel=0;KeeperAudioMix.Route(sound,"SFX_World");clunk=Resources.Load<AudioClip>("Hands/clunk");}
  Transform Surface(string name,Mesh m,Material material){var g=new GameObject(name);g.transform.SetParent(transform,false);g.AddComponent<MeshFilter>().sharedMesh=m;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;return g.transform;}
  Texture2D cageCookie;
  void Start(){
   cageCookie=new Texture2D(64,64,TextureFormat.RGBA32,false){name="Flashlight reflector aperture",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
   var pixels=new Color[64*64];for(int y=0;y<64;y++)for(int x=0;x<64;x++){
    float u=(x+.5f)/32-1,v=(y+.5f)/32-1;float edge=1-Mathf.SmoothStep(.45f,1,Mathf.Sqrt(u*u+v*v));
    pixels[y*64+x]=new Color(edge,edge,edge,edge);
   }cageCookie.SetPixels(pixels);cageCookie.Apply(false,true);pool.cookie=cageCookie;
  }
  void Hold(SelectEnterEventArgs e){if(PlayerPrefs.GetInt("LK.Flashlight.InputLearned",0)==0){watch.GetComponent<ObjectiveHud>()?.ShowHint(KeeperStrings.Get("lantern.firstGrip"),8);PlayerPrefs.SetInt("LK.Flashlight.InputLearned",1);PlayerPrefs.Save();}hand=e.interactorObject.transform.GetComponentInParent<HandRig>();hand?.SetPose(RequiredPose,this);hand?.Pulse(.2f,.04f);}
  void Release(SelectExitEventArgs e){hand?.ClearPose();hand=null;OnNetworkReleased();}
  public void OnNetworkReleased(){released=true;}
  void Wick(ActivateEventArgs e){if(!Grab.isSelected||Time.time<activateAt||watch.Sleeping||watch.State.Failed)return;activateAt=Time.time+.25f;if(watch.State.Coop!=null)watch.State.Coop.Send(CoopAction.LanternWick,Slot,true);else watch.State.CycleLantern(Slot);}
  void Update(){lease??=GetComponent<INetworkPropStatus>();var state=watch.State.Lanterns;bool active=state!=null;
   if(usable!=active){foreach(var r in surfaces)r.enabled=active;foreach(var c in Grab.colliders)c.enabled=active;Grab.enabled=active;usable=active;}
   if(!watch.State.ReadOnly&&active&&transform.TransformPoint(FlashlightMesh.Emitter).y<watch.WaterHeightAt(transform.position))state.Extinguish(Slot);
   if(hand&&Grab.isSelected)hand.SetToolGrip(hand.Node==UnityEngine.XR.XRNode.LeftHand?leftPalm:rightPalm,FlashlightMesh.HandContact);
   bool lit=active&&state.Lit(Slot);pool.enabled=lit&&!watch.Sleeping&&!watch.State.Failed;var wick=state?.Wick(Slot)??LanternWick.Off;
   pool.spotAngle=HorrorTuning.Current.lanternHighBeamAngle;
   pool.innerSpotAngle=HorrorTuning.Current.lanternHighBeamInner;
   pool.range=watch.State.Sprint5.lanternHighRange;
   pool.intensity=watch.State.Sprint5.lanternHighIntensity;
   block.SetColor("_BaseColor",lit?new Color(.8f,.78f,.65f):new Color(.14f,.16f,.14f));block.SetColor("_EmissionColor",lit?new Color(1,.85f,.64f)*1.5f:Color.black);lensRenderer.SetPropertyBlock(block);
   if(lastWick!=(int)wick){if(lastWick>=0&&clunk)sound.PlayOneShot(clunk,.12f);hand?.Pulse(.18f,.04f);lastWick=(int)wick;}
   if(!Held&&(lease==null||lease.CanSimulateReleased)&&released){body.isKinematic=false;var hook=watch.NearestLanternHook(transform.TransformPoint(FlashlightMesh.Hook));if(hook.HasValue)SetRest(hook.Value-FlashlightMesh.Hook);else if(transform.position.y< -2f)SetRest(home);}
   if(!watch.State.ReadOnly&&active)ObserveRepulsion();
  }
  void ObserveRepulsion(){var d=watch.State.Drowned;bool present=d?.Phase==DrownedPhase.Manifest&&d.Anchored;var target=present?new Vector3(d.X,d.Y+1.2f,d.Z):Vector3.zero;var from=pool.transform.position;var delta=target-from;float range=watch.State.Sprint5.lanternHighRange;bool aim=present&&delta.sqrMagnitude<range*range&&Vector3.Dot(transform.forward,delta.normalized)>Mathf.Cos(HorrorTuning.Current.lanternHighBeamInner*.5f*Mathf.Deg2Rad);
   if(aim){int count=Physics.RaycastNonAlloc(from,delta.normalized,sightHits,delta.magnitude,~0,QueryTriggerInteraction.Ignore);if(count==sightHits.Length)aim=false;for(int i=0;i<count&&aim;i++){var h=sightHits[i];if(!h.transform.IsChildOf(transform)&&!h.collider.GetComponentInParent<HandRig>()&&!h.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())aim=false;}}
   watch.State.RepelWithLantern(Slot,Time.deltaTime,Held,aim);}
  void SetRest(Vector3 p){if(!body.isKinematic)body.linearVelocity=body.angularVelocity=Vector3.zero;released=false;body.isKinematic=true;body.position=p;body.rotation=Quaternion.identity;if(clunk)sound.PlayOneShot(clunk,.12f);}
  void OnDestroy(){foreach(var m in new[]{metal,glass})if(m)Destroy(m);if(cageCookie)Destroy(cageCookie);if(mesh)Destroy(mesh);if(globe)Destroy(globe);}
 }
}
