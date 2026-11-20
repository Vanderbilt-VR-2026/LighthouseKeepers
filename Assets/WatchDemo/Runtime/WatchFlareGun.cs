using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Lobby;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.WatchDemo
{
    public sealed class WatchFlareGun : MonoBehaviour, IHandPoseTarget, INetworkReleasedBody, INetworkPropRelease
    {
        public HandPose RequiredPose => HandPose.WrapGrip;
        public XRGrabInteractable Grab { get; private set; }
        public Transform Muzzle { get; private set; }
        public bool ReleasedBodyIsKinematic => !released;
        public int Slot { get; private set; }
        WatchDemoController watch; Rigidbody body; INetworkPropStatus lease; HandRig hand;
        Renderer[] renderers; bool previouslyUsable = true; Mesh[] meshes; Material[] materials; Transform visual, hammer; Renderer loadedLamp;
        Vector3 home; Quaternion homeRotation; bool released, shownShot; double lastElapsed;
        Transform muzzleFlash;float unheld, dryAt, recoil, flashUntil; AudioSource sound; AudioClip shotSound, drySound;
        public static WatchFlareGun Create(WatchDemoController w, int slot)
        {
            var go=new GameObject("KEEPER_FLARE_GUN_"+slot);go.SetActive(false);go.transform.SetParent(w.transform,false);
            var gun=go.AddComponent<WatchFlareGun>();gun.watch=w;gun.Slot=slot;gun.Build();go.SetActive(true);return gun;
        }
        void Build()
        {
            // One shared pistol lies on the wooden bench beside the wrench. Preserve its lease path.
            var bench=GameObject.Find("Wrench table top").GetComponent<Collider>().bounds;
            home=new Vector3(bench.center.x+.43f,bench.max.y+.035f,bench.center.z-.14f);homeRotation=Quaternion.Euler(0,25,90);
            transform.SetPositionAndRotation(home,homeRotation);
            var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();
            var iron=new Material(parts.Metal){name="Flare pistol worn blue iron"};iron.SetColor("_BaseColor",new Color(.12f,.16f,.18f));iron.SetFloat("_Metallic",.75f);iron.SetFloat("_Smoothness",.36f);iron.SetColor("_EmissionColor",Color.black);
            var enamel=new Material(parts.Metal){name="Weathered signal orange enamel"};enamel.SetColor("_BaseColor",new Color(.62f,.19f,.055f));enamel.SetFloat("_Metallic",.22f);enamel.SetFloat("_Smoothness",.30f);enamel.SetColor("_EmissionColor",Color.black);
            var wood=new Material(Resources.Load<Material>("VisualMaterials/Wood")){name="Flare pistol dark fitted stocks"};wood.SetColor("_BaseColor",new Color(.22f,.13f,.07f));wood.SetFloat("_Smoothness",.27f);wood.SetColor("_EmissionColor",Color.black);
            materials=new[]{iron,enamel,wood};meshes=FlareGunMesh.Build();visual=M2StationParts.Root("Signal pistol moving assembly",transform,Vector3.zero);
            for(int i=0;i<meshes.Length;i++){var g=new GameObject(meshes[i].name);g.transform.SetParent(visual,false);g.AddComponent<MeshFilter>().sharedMesh=meshes[i];var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=materials[i];r.shadowCastingMode=ShadowCastingMode.Off;}
            Muzzle=M2StationParts.Root("Flare muzzle",transform,FlareGunMesh.Muzzle);
            var spark=GameObject.CreatePrimitive(PrimitiveType.Sphere);spark.name="Flare muzzle flash";Destroy(spark.GetComponent<Collider>());spark.transform.SetParent(Muzzle,false);spark.transform.localScale=new Vector3(.075f,.075f,.18f);spark.transform.localPosition=Vector3.forward*.06f;
            var glow=new Material(Resources.Load<Shader>("FlareGlow"));glow.SetColor("_BaseColor",new Color(1,.25f,.02f,1));spark.GetComponent<Renderer>().sharedMaterial=glow;materials=new[]{iron,enamel,wood,glow};muzzleFlash=spark.transform;muzzleFlash.gameObject.SetActive(false);
            var chamber=parts.Part("Loaded cartridge rim",visual,new Vector3(0,.061f,-.037f),new Vector3(.036f,.004f,.036f),parts.Brass,PrimitiveType.Cylinder);chamber.transform.localRotation=Quaternion.Euler(90,0,0);loadedLamp=chamber.GetComponent<Renderer>();
            hammer=M2StationParts.Root("Hammer feedback",visual,new Vector3(0,.08f,-.07f));
            body=gameObject.AddComponent<Rigidbody>();body.mass=.65f;body.isKinematic=true;body.useGravity=true;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
            var handle=gameObject.AddComponent<BoxCollider>();handle.center=FlareGunMesh.GripCentre;handle.size=new Vector3(.045f,.105f,.055f);
            var barrel=gameObject.AddComponent<BoxCollider>();barrel.center=new Vector3(0,.057f,.075f);barrel.size=new Vector3(.06f,.065f,.25f);
            Grab=gameObject.AddComponent<SignalPistolGrab>();Grab.colliders.Add(handle);Grab.colliders.Add(barrel);Grab.movementType=XRBaseInteractable.MovementType.Kinematic;Grab.useDynamicAttach=false;Grab.throwOnDetach=true;Grab.retainTransformParent=false;
            Grab.attachTransform=M2StationParts.Root("Fitted handle centre",transform,FlareGunMesh.GripCentre);
            Grab.selectEntered.AddListener(Hold);Grab.selectExited.AddListener(Release);Grab.activated.AddListener(Fire);
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=.5f;sound.maxDistance=20;sound.dopplerLevel=0;KeeperAudioMix.Route(sound,"SFX_Critical");
            renderers=visual.GetComponentsInChildren<Renderer>();
            shotSound=Resources.Load<AudioClip>("S5Audio/flare_shot");drySound=Resources.Load<AudioClip>("Hands/clunk");
        }
        void Hold(SelectEnterEventArgs e){hand=e.interactorObject.transform.GetComponentInParent<HandRig>();if(hand){hand.SetPose(RequiredPose,this);hand.Pulse(.25f,.05f);}unheld=0;}
        void Release(SelectExitEventArgs e){if(hand)hand.ClearPose();hand=null;OnNetworkReleased();}
        public void OnNetworkReleased(){released=true;unheld=0;}
        void Fire(ActivateEventArgs e)
        {
            if(!Grab.isSelected||watch.Sleeping||watch.State.Failed)return;
            if(watch.State.Flares?.Loaded(Slot)!=true){if(Time.unscaledTime>=dryAt){dryAt=Time.unscaledTime+.5f;if(drySound)sound.PlayOneShot(drySound,.18f);hand?.Pulse(.12f,.03f);}return;}
            if(watch.State.Coop!=null)watch.State.Coop.Send(CoopAction.FlareFire,Slot,true);
            else watch.FireFlare(Slot);
        }
        public bool HostFire()
        {
            bool held=Grab.isSelected||watch.State.Coop?.RemoteHeld(Grab)==true;
            if(!held||watch.Sleeping)return false;
            // Reject a muzzle embedded in scenery; do not spend ammo on an impossible shot.
            var centre=transform.TransformPoint(FlareGunMesh.GripCentre);var delta=Muzzle.position-centre;
            foreach(var hit in Physics.RaycastAll(centre,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
                if(!hit.transform.IsChildOf(transform)&&!hit.collider.GetComponentInParent<HandRig>()&&!hit.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())return false;
            return watch.State.FireFlare(Slot,Muzzle.position,Muzzle.forward);
        }
        void Update()
        {
            lease??=GetComponent<INetworkPropStatus>();var state=watch.State;var frame=state.Flares?.Shot(Slot);
            if(state.Elapsed<lastElapsed-.01){shownShot=false;ReturnHome();}lastElapsed=state.Elapsed;
            bool usable=state.Flares!=null;
            if(previouslyUsable!=usable||lastElapsed==0){foreach(var r in renderers)r.enabled=usable;Grab.enabled=usable;foreach(var c in Grab.colliders)c.enabled=usable;previouslyUsable=usable;}
            if(frame!=null&&frame.fired&&!shownShot){shownShot=true;if(state.Flares.Clock-frame.at>=0&&state.Flares.Clock-frame.at<1.25){recoil=1;flashUntil=Time.unscaledTime+.09f;if(shotSound)sound.PlayOneShot(shotSound,.55f);hand?.Pulse(.65f,.10f);}}
            if(frame!=null&&!frame.fired)shownShot=false;
            if(loadedLamp)loadedLamp.enabled=usable&&frame!=null&&!frame.fired;
            muzzleFlash.gameObject.SetActive(Time.unscaledTime<flashUntil);
            recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*7);visual.localRotation=Quaternion.Euler(-recoil*7,0,0);
            bool held=Grab.isSelected||lease!=null&&lease.Owner!=0;
            unheld=held?0:unheld+Time.deltaTime;
            if(!held&&(lease==null||lease.CanSimulateReleased)){
                body.isKinematic=!released;
                if(transform.position.y<-.5f||unheld>20)ReturnHome();
            }
        }
        void ReturnHome(){if(Grab.isSelected||lease!=null&&!lease.CanSimulateReleased)return;if(!body.isKinematic)body.linearVelocity=body.angularVelocity=Vector3.zero;released=false;body.isKinematic=true;transform.SetPositionAndRotation(home,homeRotation);unheld=0;}
        void OnDestroy(){if(meshes!=null)foreach(var m in meshes)Destroy(m);if(materials!=null)foreach(var m in materials)Destroy(m);}
    }
    public sealed class SignalPistolGrab : XRGrabInteractable
    {
        XRBaseInteractor holder;Transform prior,socket;
        protected override void OnSelectEntering(SelectEnterEventArgs e)
        {
            var hand=e.interactorObject.transform.GetComponentInParent<HandRig>();holder=e.interactorObject as XRBaseInteractor;
            if(hand&&holder){var skin=hand.GetComponentInChildren<SkinnedMeshRenderer>();socket=new GameObject("Signal pistol palm socket").transform;socket.SetParent(skin.transform,false);socket.localPosition=WatchWrenchGrab.PalmCentre;socket.rotation=e.interactorObject.transform.rotation;prior=holder.attachTransform;holder.attachTransform=socket;}
            GetComponent<Rigidbody>().isKinematic=false;base.OnSelectEntering(e);
        }
        protected override void OnSelectExited(SelectExitEventArgs e){base.OnSelectExited(e);if(holder)holder.attachTransform=prior;if(socket)Destroy(socket.gameObject);holder=null;socket=null;}
    }
}
