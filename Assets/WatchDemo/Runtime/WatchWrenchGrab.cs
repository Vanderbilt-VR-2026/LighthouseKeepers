using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    /// <summary>The tool attaches to the curled palm; the accepted hand visual rotation is untouched.</summary>
    public sealed class WatchWrenchGrab : XRGrabInteractable
    {
        [SerializeField] Transform leftAttach,rightAttach;
        public static readonly Vector3 PalmCentre=new Vector3(0,-.052f,.022f);
        XRBaseInteractor holder;Transform originalAttach,socket;
        public override Transform GetAttachTransform(IXRInteractor interactor)
        {
            var hand=interactor.transform.GetComponentInParent<HandRig>();
            if(hand && leftAttach && rightAttach)return hand.Node==XRNode.LeftHand?leftAttach:rightAttach;
            return base.GetAttachTransform(interactor);
        }
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            var hand=args.interactorObject.transform.GetComponentInParent<HandRig>();
            holder=args.interactorObject as XRBaseInteractor;
            if(hand && holder)
            {
                var visual=hand.GetComponentInChildren<SkinnedMeshRenderer>().transform;
                socket=new GameObject("Wrench palm grip socket").transform;socket.SetParent(visual,false);socket.localPosition=PalmCentre;
                originalAttach=holder.attachTransform;holder.attachTransform=socket;
            }
            GetComponent<Rigidbody>().isKinematic=false;
            base.OnSelectEntering(args);
        }
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            if(holder)holder.attachTransform=originalAttach;
            if(socket)Destroy(socket.gameObject);holder=null;socket=null;
        }
    }
}
