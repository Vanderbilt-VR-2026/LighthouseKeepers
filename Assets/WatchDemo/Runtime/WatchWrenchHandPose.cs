using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.WatchDemo
{
    /// <summary>Presentation-only curl for the existing controller glove while it holds the demo wrench.</summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class WatchWrenchHandPose : MonoBehaviour
    {
        XRGrabInteractable grab;
        readonly List<Transform> digits=new List<Transform>();
        readonly List<Quaternion> openPose=new List<Quaternion>();
        Coroutine poseRoutine;

        void Awake(){grab=GetComponent<XRGrabInteractable>();}
        void OnEnable(){grab.selectEntered.AddListener(Grip);grab.selectExited.AddListener(Release);}
        void OnDisable(){if(grab){grab.selectEntered.RemoveListener(Grip);grab.selectExited.RemoveListener(Release);}RestoreImmediately();}

        void Grip(SelectEnterEventArgs args)
        {
            FindDigits(args.interactorObject.transform);
            SetGrip(true);
        }

        void Release(SelectExitEventArgs args){SetGrip(false);}

        void FindDigits(Transform interactor)
        {
            digits.Clear();openPose.Clear();
            Transform glove=null;
            for(var node=interactor;node!=null&&glove==null;node=node.parent)glove=node.Find("LK Stylized Glove");
            if(!glove)return;
            foreach(var child in glove.GetComponentsInChildren<Transform>(true))
            {
                if(!child.name.StartsWith("Glove finger")&&!child.name.Equals("Glove thumb"))continue;
                digits.Add(child);openPose.Add(child.localRotation);
            }
        }

        void SetGrip(bool closed)
        {
            if(poseRoutine!=null)StopCoroutine(poseRoutine);
            poseRoutine=StartCoroutine(Blend(closed));
        }

        IEnumerator Blend(bool closed)
        {
            var start=new Quaternion[digits.Count];
            var target=new Quaternion[digits.Count];
            for(int i=0;i<digits.Count;i++)
            {
                start[i]=digits[i].localRotation;
                bool thumb=digits[i].name.Equals("Glove thumb");
                target[i]=closed?openPose[i]*Quaternion.Euler(thumb?-38f:-58f,thumb?24f:0f,0):openPose[i];
            }
            const float duration=.08f;
            for(float elapsed=0;elapsed<duration;elapsed+=Time.deltaTime)
            {
                float eased=elapsed/duration;eased=eased*eased*(3f-2f*eased);
                for(int i=0;i<digits.Count;i++)if(digits[i])digits[i].localRotation=Quaternion.Slerp(start[i],target[i],eased);
                yield return null;
            }
            for(int i=0;i<digits.Count;i++)if(digits[i])digits[i].localRotation=target[i];
            poseRoutine=null;
        }

        void RestoreImmediately()
        {
            if(poseRoutine!=null)StopCoroutine(poseRoutine);
            for(int i=0;i<digits.Count;i++)if(digits[i])digits[i].localRotation=openPose[i];
        }
    }
}
