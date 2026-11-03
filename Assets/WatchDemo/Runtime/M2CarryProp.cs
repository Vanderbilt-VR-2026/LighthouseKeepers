using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class M2CarryProp : MonoBehaviour,IHandPoseTarget,INetworkPropRelease
    {
        public HandPose RequiredPose=>HandPose.WrapGrip;
        public XRGrabInteractable Grab {get;private set;}
        public Transform Tip {get;private set;}
        Transform left,right;Rigidbody body;Vector3 home;Quaternion rotation;float dropped;INetworkPropStatus network;
        public static M2CarryProp Create(M2StationParts parts,Transform parent,string name,Vector3 position,bool rag)
        {
            var t=M2StationParts.Root(name,parent,position);t.gameObject.SetActive(false);
            var size=rag?new Vector3(.16f,.045f,.12f):new Vector3(.25f,.38f,.16f);
            parts.Part(rag?"Folded cleaning cloth":"Pressed red fuel tank",t,Vector3.zero,size,rag?parts.Brass:parts.Red);
            if(!rag)
            {
                for(int x=-1;x<=1;x+=2)parts.Part("Handle side "+x,t,new Vector3(x*.09f,.245f,0),new Vector3(.035f,.12f,.035f),parts.Red);
                parts.Part("Carry bar",t,new Vector3(0,.30f,0),new Vector3(.20f,.035f,.035f),parts.Metal);
                parts.Part("Pour spout",t,new Vector3(.1f,.235f,.055f),new Vector3(.035f,.12f,.035f),parts.Brass);
                parts.Text("Fuel can label","FUEL",t,new Vector3(0,0,.083f),.005f);
            }
            var body=t.gameObject.AddComponent<Rigidbody>();body.mass=rag?.15f:1.2f;body.isKinematic=true;body.interpolation=RigidbodyInterpolation.Interpolate;
            var col=t.gameObject.AddComponent<BoxCollider>();col.size=size;
            var grab=t.gameObject.AddComponent<XRGrabInteractable>();grab.colliders.Add(col);grab.movementType=XRBaseInteractable.MovementType.Kinematic;grab.useDynamicAttach=false;
            var c=t.gameObject.AddComponent<M2CarryProp>();c.Grab=grab;c.body=body;c.left=c.Palm("Left palm",new Vector3(-.014f,rag?.03f:.31f,.02f));c.right=c.Palm("Right palm",new Vector3(.014f,rag?.03f:.31f,.02f));grab.attachTransform=c.right;
            c.Tip=M2StationParts.Root("Working tip",t,rag?new Vector3(0,-.025f,0):new Vector3(.1f,.30f,.055f));c.home=t.position;c.rotation=t.rotation;t.gameObject.SetActive(true);return c;
        }
        Transform Palm(string name,Vector3 pos){var t=M2StationParts.Root(name,transform,pos);t.localRotation=Quaternion.Euler(0,180,0);return t;}
        void Awake(){Grab.selectEntered.AddListener(OnGrab);Grab.selectExited.AddListener(OnRelease);}
        void OnGrab(SelectEnterEventArgs e){var hand=e.interactorObject.transform.GetComponentInParent<HandRig>();Grab.attachTransform=hand&&hand.Node==XRNode.LeftHand?left:right;if(hand){hand.SetPose(RequiredPose,this);hand.Pulse(.35f,.07f);}}
        void OnRelease(SelectExitEventArgs e){var hand=e.interactorObject.transform.GetComponentInParent<HandRig>();if(hand)hand.ClearPose();OnNetworkReleased();}
        public void OnNetworkReleased(){dropped=Time.time;body.isKinematic=false;}
        public void ResetHome(){if(Grab.isSelected)Grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)Grab);body.isKinematic=true;transform.SetPositionAndRotation(home,rotation);dropped=0;}
        void Update(){network??=GetComponent<INetworkPropStatus>();if(Grab.isSelected||network?.CanSimulateReleased==false)return;if(dropped>0&&Time.time-dropped>8)ResetHome();}
        void OnDestroy(){if(Grab){Grab.selectEntered.RemoveListener(OnGrab);Grab.selectExited.RemoveListener(OnRelease);}}
    }
}
