using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Net;
namespace LighthouseKeepers.WatchDemo
{
    [RequireComponent(typeof(XRGrabInteractable),typeof(Rigidbody))]
    public sealed class WatchWrenchReturn : MonoBehaviour,INetworkPropRelease
    {
        public const float ReturnSeconds=8f;
        XRGrabInteractable grab; Rigidbody body;
        Vector3 home;Quaternion rotation;float returnAt=-1;
        FusionGrabBinding network;
        int previousOwner;
        void Awake(){grab=GetComponent<XRGrabInteractable>();body=GetComponent<Rigidbody>();home=transform.position;rotation=transform.rotation;}
        void OnEnable(){grab.selectEntered.AddListener(Grab);grab.selectExited.AddListener(Drop);}
        void OnDisable(){grab.selectEntered.RemoveListener(Grab);grab.selectExited.RemoveListener(Drop);}
        void Grab(SelectEnterEventArgs args){returnAt=-1;}
        void Drop(SelectExitEventArgs args){returnAt=Time.time+ReturnSeconds;}
        public void OnNetworkReleased(){returnAt=Time.time+ReturnSeconds;previousOwner=0;}
        void Update()
        {
            if(!network && FusionMessageChannel.Active && FusionMessageChannel.Active.Connected)TryGetComponent(out network);
            if(network)
            {
                if(network.Owner!=previousOwner)
                {returnAt=network.Owner==0?Time.time+ReturnSeconds:-1;previousOwner=network.Owner;}
                if(!network.CanSimulateReleased)return;
            }
            if(returnAt>=0 && !grab.isSelected && Time.time>=returnAt)ReturnHome();
        }
        public void ReturnHome()
        {
            if(grab.isSelected || network && !network.CanSimulateReleased)return;
            if(!body.isKinematic){body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;}
            body.isKinematic=true;body.position=home;body.rotation=rotation;transform.SetPositionAndRotation(home,rotation);returnAt=-1;
        }
    }
}
