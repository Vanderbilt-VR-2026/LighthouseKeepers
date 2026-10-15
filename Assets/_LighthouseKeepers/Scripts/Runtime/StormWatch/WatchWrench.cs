using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace LighthouseKeepers.StormWatch
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class WatchWrench : MonoBehaviour
    {
        [SerializeField] StormWatchDirector director;
        XRGrabInteractable grab;
        Vector3 home, previous;
        Quaternion rotation;
        bool tracking;
        float returnAfter;
        void Awake(){grab=GetComponent<XRGrabInteractable>();home=transform.position;rotation=transform.rotation;}
        void Update()
        {
            if(!director||!director.Ready)return;
            director.HoldingWrench=grab.isSelected;
            if(grab.isSelected)
            {
                returnAfter=0;
                var axis=Vector3.ProjectOnPlane(transform.up,Vector3.forward).normalized;
                if(Vector3.Distance(transform.position,WatchStations.Pipe)<.6f&&tracking&&previous.sqrMagnitude>.5f&&axis.sqrMagnitude>.5f)
                {
                    float angle=Mathf.Abs(Vector3.SignedAngle(previous,axis,Vector3.forward));
                    if(angle>.15f&&angle<50)director.Work(WatchAction.Pipe,Mathf.Clamp01(angle/Mathf.Max(.001f,Time.deltaTime)/55));
                }
                previous=axis;tracking=true;
            }
            else
            {
                tracking=false;returnAfter+=Time.deltaTime;
                // A dropped tool must not make a short run unwinnable under the floor/water.
                if(transform.position.y<-.5f||returnAfter>8)
                {transform.SetPositionAndRotation(home,rotation);var body=GetComponent<Rigidbody>();body.linearVelocity=body.angularVelocity=Vector3.zero;returnAfter=0;}
            }
        }
        void OnDisable(){if(director)director.HoldingWrench=false;}
    }
}
