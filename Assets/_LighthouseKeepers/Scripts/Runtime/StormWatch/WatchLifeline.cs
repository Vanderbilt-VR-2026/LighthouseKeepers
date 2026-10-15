using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace LighthouseKeepers.StormWatch
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class WatchLifeline : MonoBehaviour
    {
        [SerializeField] StormWatchDirector director;
        XRGrabInteractable grab;
        void Awake(){grab=GetComponent<XRGrabInteractable>();grab.selectExited.AddListener(Throw);}
        void Throw(SelectExitEventArgs args)
        {
            if(!director||!director.Ready)return;
            director.Local.aim=transform.forward;
            director.Command(WatchAction.CastLine);
        }
        void LateUpdate()
        {
            if(!director||!director.Ready||grab.isSelected)return;
            // The reusable float lives on the keeper's belt; release casts toward the other keeper.
            var head=director.Head;transform.position=head.position+head.right*.3f-Vector3.up*.55f;
            transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized);
            var body=GetComponent<Rigidbody>();body.linearVelocity=body.angularVelocity=Vector3.zero;
        }
        void OnDestroy(){if(grab)grab.selectExited.RemoveListener(Throw);}
    }
}
