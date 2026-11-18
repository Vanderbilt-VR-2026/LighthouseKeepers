using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class HandheldCrewRadio : MonoBehaviour, INetworkReleasedBody, INetworkPropRelease, INetworkPropCarry
    {
        WatchDemoController watch;int index;XRGrabInteractable grab;INetworkPropStatus lease;Transform head;Renderer lamp;TextMesh status;MaterialPropertyBlock block;
        bool trigger,previous,released,carried,taughtCarry;
        public bool Carried=>carried;
        public void DropCarry(){if(carried)released=true;carried=false;}
        public bool ReleasedBodyIsKinematic=>carried||!released;
        public void OnNetworkReleased(){if(carried)return;released=true;unheld=0;}
        void Start(){grab.useDynamicAttach=false;grab.throwOnDetach=false;}float nextSend,unheld;Vector3 home;Quaternion homeRotation;Rigidbody body;
        public XRGrabInteractable Grab=>grab;
        public bool Held=>grab&&grab.isSelected;
        public bool WantsTransmit=>Held&&(trigger||head&&Vector3.Distance(transform.position,head.position)<=watch.State.Connected.radioHeadDistance);
        public static HandheldCrewRadio Create(WatchDemoController w,int index)
        {
            var root=new GameObject("CREW_RADIO_"+index);root.SetActive(false);root.transform.SetParent(w.transform,false);
            var r=root.AddComponent<HandheldCrewRadio>();r.watch=w;r.index=index;r.Build();root.SetActive(true);return r;
        }
        void Build()
        {
            var p=gameObject.AddComponent<M2StationParts>();p.Initialize();block=new MaterialPropertyBlock();
            // Eli's House bench placement, using our existing authenticated grab/transmit route.
            Vector3 spot=new Vector3(-2.90f,.88f,-9.07f);
            var top=GameObject.Find("Wrench table top");
            if(!top)top=GameObject.Find("Desk top");
            var surface=top?top.GetComponent<Renderer>():null;
            if(surface){var b=surface.bounds;spot=new Vector3(b.center.x+.12f,b.max.y,b.min.z+.14f);}
            transform.SetPositionAndRotation(spot+new Vector3(0,.10f,index*.18f),Quaternion.Euler(0,90,0));
            home=transform.position;homeRotation=transform.rotation;
            p.Part("Handheld enamel body",transform,Vector3.zero,new Vector3(.075f,.18f,.055f),p.Dark);
            for(int i=0;i<5;i++)p.Part("Speaker grille",transform,new Vector3(0,.035f-i*.012f,.03f),new Vector3(.05f,.003f,.004f),p.Metal);
            p.Part("Rubber aerial",transform,new Vector3(-.025f,.155f,0),new Vector3(.009f,.14f,.009f),p.Dark);
            p.Part("Push to talk button",transform,new Vector3(.041f,0,0),new Vector3(.012f,.04f,.025f),p.Brass);
            lamp=p.Part("Transmit indicator",transform,new Vector3(.02f,.07f,.031f),new Vector3(.012f,.012f,.008f),p.Brass).GetComponent<Renderer>();
            status=p.Text("Radio face stencil",PlayerStrings.Get("connected.radioUse"),transform,new Vector3(0,-.055f,.031f),.00065f);
            var col=gameObject.AddComponent<BoxCollider>();col.size=new Vector3(.08f,.19f,.06f);
            body=gameObject.AddComponent<Rigidbody>();body.mass=.25f;body.isKinematic=true;body.useGravity=true;
            grab=gameObject.AddComponent<XRGrabInteractable>();grab.colliders.Add(col);grab.movementType=XRBaseInteractable.MovementType.Instantaneous;grab.smoothPosition=false;grab.smoothRotation=false;grab.attachEaseInTime=.08f;grab.throwOnDetach=false;
            var attach=M2StationParts.Root("Radio palm contact",transform,new Vector3(0,-.035f,0));
            gameObject.AddComponent<PhysicalGrip>().Configure(attach,HandPose.WrapGrip);
            grab.useDynamicAttach=false;
            foreach(var rig in FindObjectsByType<Unity.XR.CoreUtils.XROrigin>(FindObjectsSortMode.None))foreach(var c in rig.GetComponentsInChildren<Collider>(true))Physics.IgnoreCollision(col,c,true);
            grab.selectEntered.AddListener(e=>{carried=false;if(!taughtCarry){taughtCarry=true;FindAnyObjectByType<HarborRadio>()?.RecordSilentLine(KeeperStrings.Get("radio.belt"),"");watch.GetComponent<ObjectiveHud>()?.RevealFor(5);}});grab.activated.AddListener(Activate);grab.deactivated.AddListener(Deactivate);grab.selectExited.AddListener(Release);
            head=Camera.main?Camera.main.transform:null;
        }
        void Activate(ActivateEventArgs e){if(!Held)return;trigger=true;if(watch.State.ScriptedPartner)watch.GetComponent<CrewCompanion>()?.RequestReport();}
        void Deactivate(DeactivateEventArgs e)=>trigger=false;
        void Release(SelectExitEventArgs e){trigger=false;Send(false);
            // Retain the existing prop lease while clipped; the host checks the carry pose at the belt.
            if(head&&RadioBeltPose.Near(transform.position,head.position,head.rotation)&&!OtherOnBelt()){
                carried=true;released=false;body.isKinematic=true;
                watch.GetComponent<ObjectiveHud>()?.RevealFor(3);
            }else OnNetworkReleased();
        }
        bool OtherOnBelt(){foreach(var r in watch.GetComponentsInChildren<HandheldCrewRadio>())if(r!=this&&r.Carried)return true;return false;}
        void Send(bool active){watch.State.Coop?.Send(CoopAction.HandheldRadio,index,active);previous=active;nextSend=Time.unscaledTime+.1f;}
        void Update()
        {
            if(lease==null)lease=GetComponent<INetworkPropStatus>();
            bool allowed=CrewVoiceBridge.Current is ICrewVoiceCapture capture&&capture.MicrophoneAllowed&&!capture.Muted;
            bool active=allowed&&WantsTransmit;
            if(watch.State.Coop!=null&&(Time.unscaledTime>=nextSend||active!=previous)&&(active||previous))Send(active);
            bool transmitting=watch.RadioTransmitter(index)!=0;
            block.Clear();var c=transmitting?new Color(.15f,.7f,.25f):new Color(.25f,.13f,.03f);block.SetColor("_BaseColor",c);block.SetColor("_EmissionColor",transmitting?c*.7f:c*.12f);lamp.SetPropertyBlock(block);
            status.text=PlayerStrings.Get(transmitting?"connected.transmitting":"connected.radioUse");
            if(carried&&head&&!Held){body.isKinematic=true;transform.SetPositionAndRotation(RadioBeltPose.Position(head.position,head.rotation),Quaternion.Euler(0,head.eulerAngles.y,0));return;}
            if(Held||lease!=null&&lease.Owner!=0)unheld=0;else unheld+=Time.deltaTime;
            if((lease==null||lease.CanSimulateReleased)&&!Held){body.isKinematic=!released;
                if(unheld>20||transform.position.y< -1){if(!body.isKinematic)body.linearVelocity=body.angularVelocity=Vector3.zero;released=false;body.isKinematic=true;transform.SetPositionAndRotation(home,homeRotation);unheld=0;}}
        }
        void OnDisable(){if(previous&&watch)Send(false);trigger=false;}
    }
}
