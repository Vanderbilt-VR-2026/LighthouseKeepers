using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class HandheldCrewRadio : MonoBehaviour, INetworkReleasedBody, INetworkPropRelease
    {
        WatchDemoController watch;int index;XRGrabInteractable grab;INetworkPropStatus lease;Transform head;Renderer lamp;TextMesh status;MaterialPropertyBlock block;
        bool trigger,previous,released;
        public bool ReleasedBodyIsKinematic=>!released;
        public void OnNetworkReleased(){released=true;unheld=0;}
        void Start(){grab.useDynamicAttach=true;}float nextSend,unheld;Vector3 home;Quaternion homeRotation;Rigidbody body;
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
            float y=index==0?1.2f:11.1f;var wall=PipeWallLayout.WallSurface(-1.8f,out var normal);
            if(index==1){float nearest=10;foreach(var hit in Physics.RaycastAll(new Vector3(0,y,-1.8f),Vector3.right,5,~0,QueryTriggerInteraction.Ignore))if(hit.collider.name.Contains("Wall")&&hit.distance<nearest){nearest=hit.distance;wall=hit.point;normal=hit.normal;}}
            wall.y=y;transform.SetPositionAndRotation(wall+normal*.12f,Quaternion.LookRotation(normal));home=transform.position;homeRotation=transform.rotation;
            // The dock stays on the wall when its radio is carried.
            var dock=M2StationParts.Root("Crew radio dock "+index,watch.transform,home-normal*.08f);dock.rotation=transform.rotation;
            p.Part("Radio wall shelf",dock,new Vector3(0,-.12f,.03f),new Vector3(.20f,.035f,.18f),p.Metal);
            p.Part("Radio dock back",dock,new Vector3(0,.02f,-.04f),new Vector3(.20f,.34f,.025f),p.Dark);
            p.Text("Radio dock stencil",PlayerStrings.Get("connected.radio"),dock,new Vector3(0,.15f,-.025f),.0025f);
            p.Part("Handheld enamel body",transform,Vector3.zero,new Vector3(.075f,.18f,.055f),p.Dark);
            for(int i=0;i<5;i++)p.Part("Speaker grille",transform,new Vector3(0,.035f-i*.012f,.03f),new Vector3(.05f,.003f,.004f),p.Metal);
            p.Part("Rubber aerial",transform,new Vector3(-.025f,.155f,0),new Vector3(.009f,.14f,.009f),p.Dark);
            p.Part("Push to talk button",transform,new Vector3(.041f,0,0),new Vector3(.012f,.04f,.025f),p.Brass);
            lamp=p.Part("Transmit indicator",transform,new Vector3(.02f,.07f,.031f),new Vector3(.012f,.012f,.008f),p.Brass).GetComponent<Renderer>();
            status=p.Text("Radio face stencil",PlayerStrings.Get("connected.radioUse"),transform,new Vector3(0,-.055f,.031f),.00065f);
            var col=gameObject.AddComponent<BoxCollider>();col.size=new Vector3(.08f,.19f,.06f);
            body=gameObject.AddComponent<Rigidbody>();body.mass=.25f;body.isKinematic=true;body.useGravity=true;
            grab=gameObject.AddComponent<XRGrabInteractable>();grab.colliders.Add(col);grab.movementType=XRBaseInteractable.MovementType.Kinematic;
            var attach=M2StationParts.Root("Radio palm contact",transform,new Vector3(0,-.035f,0));
            gameObject.AddComponent<PhysicalGrip>().Configure(attach,HandPose.WrapGrip);
            grab.useDynamicAttach=true;grab.matchAttachPosition=true;grab.matchAttachRotation=true;
            grab.activated.AddListener(Activate);grab.deactivated.AddListener(Deactivate);grab.selectExited.AddListener(Release);
            head=Camera.main?Camera.main.transform:null;
        }
        void Activate(ActivateEventArgs e){if(Held)trigger=true;}
        void Deactivate(DeactivateEventArgs e)=>trigger=false;
        void Release(SelectExitEventArgs e){trigger=false;Send(false);OnNetworkReleased();}
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
            if(Held||lease!=null&&lease.Owner!=0)unheld=0;else unheld+=Time.deltaTime;
            if((lease==null||lease.CanSimulateReleased)&&!Held){body.isKinematic=!released;
                if(unheld>20||transform.position.y< -1){if(!body.isKinematic)body.linearVelocity=body.angularVelocity=Vector3.zero;released=false;body.isKinematic=true;transform.SetPositionAndRotation(home,homeRotation);unheld=0;}}
        }
        void OnDisable(){if(previous&&watch)Send(false);trigger=false;}
    }
}
