using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace LighthouseKeepers.StormWatch
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class WatchControl : MonoBehaviour
    {
        [SerializeField] StormWatchDirector director;
        [SerializeField] WatchAction action;
        [SerializeField] Transform movingPart;
        [SerializeField] Renderer indicator;
        [SerializeField,Min(.05f)] float handReach=.65f;
        XRSimpleInteractable interactable;
        Vector3 previous;
        bool tracking;
        float recentlyMoving;
        MaterialPropertyBlock tint;
        int feedbackVersion=-1;
        void Awake(){interactable=GetComponent<XRSimpleInteractable>();tint=new();}
        void Update()
        {
            if(!director||!director.Ready)return;
            bool held=interactable.isSelected&&interactable.interactorsSelecting.Count>0;
            Transform hand=held?interactable.interactorsSelecting[0].transform:null;
            held=held&&Vector3.Distance(hand.position,transform.position)<handReach;
            if(held)
            {
                if(action==WatchAction.Brace||action==WatchAction.Radio)director.Work(action);
                else if(action==WatchAction.CatchLine){director.Command(action);}
                else if(action==WatchAction.Pump||action==WatchAction.PullLine)
                {
                    if(tracking&&Vector3.Distance(hand.position,previous)>.002f)recentlyMoving=.15f;
                    recentlyMoving=Mathf.Max(0,recentlyMoving-Time.deltaTime);
                    if(recentlyMoving>0)director.Work(action);
                    previous=hand.position;
                    if(movingPart)movingPart.localPosition=new Vector3(0,Mathf.Clamp(transform.InverseTransformPoint(hand.position).y,-.16f,.16f),0);
                }
                else if(action!=WatchAction.Pipe||director.HoldingWrench)
                {
                    var axis=Vector3.ProjectOnPlane(hand.up,transform.forward).normalized;
                    if(tracking&&previous.sqrMagnitude>.5f&&axis.sqrMagnitude>.5f)
                    {
                        float angle=Mathf.Abs(Vector3.SignedAngle(previous,axis,transform.forward));
                        if(angle>.15f&&angle<50)
                        {
                            director.Work(action,Mathf.Clamp01(angle/Mathf.Max(.001f,Time.deltaTime)/55));
                            if(movingPart)movingPart.Rotate(0,0,angle,Space.Self);
                        }
                    }
                    previous=axis;
                }
                if(feedbackVersion!=director.State.messageRevision)
                {
                    feedbackVersion=director.State.messageRevision;
                    var haptics=hand.GetComponentInParent<HapticImpulsePlayer>();if(haptics)haptics.SendHapticImpulse(.2f,.07f);
                }
            }
            tracking=held;
            if(!held)recentlyMoving=0;
            if(indicator)
            {
                Color color=held?new(.25f,.85f,.5f):interactable.isHovered?new(1,.88f,.35f):new(.8f,.5f,.16f);
                if(action==WatchAction.Generator&&!director.State.power&&!held)color=Color.Lerp(new Color(.5f,.08f,.025f),new Color(1,.35f,.07f),.5f+.5f*Mathf.Sin(Time.time*5));
                tint.SetColor("_BaseColor",color);indicator.SetPropertyBlock(tint);
            }
        }
    }
}
