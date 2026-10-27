using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    [RequireComponent(typeof(XRSimpleInteractable),typeof(PhysicalGrip))]
    public sealed class RadioControl : MonoBehaviour
    {
        public enum Kind { Button, Toggle, Knob }
        public Kind kind;
        public bool mainPower, frequency;
        public int codeColour=-1;
        public Transform visual;
        public WatchRadioConsole console;
        public float Normalized {get;private set;}
        public bool Toggled {get;private set;}
        XRSimpleInteractable interaction;PhysicalGrip grip;
        Quaternion previousHand;Vector3 visualHome;Quaternion visualRotation;
        float depression,detent;
        void Awake()
        {
            interaction=GetComponent<XRSimpleInteractable>();grip=GetComponent<PhysicalGrip>();
            visualHome=visual.localPosition;visualRotation=visual.localRotation;
            interaction.selectEntered.AddListener(Selected);interaction.selectExited.AddListener(Released);
        }
        void Selected(SelectEnterEventArgs e)
        {
            previousHand=e.interactorObject.transform.rotation;
            if(kind==Kind.Button){depression=.018f;console.Press(this);}
            else if(kind==Kind.Toggle)
            {if(!mainPower||console.ToggleMain())Toggled=!Toggled;console.Click();}
            else console.Click();
        }
        void Released(SelectExitEventArgs e){depression=0;}
        public void SyncMain(bool value){if(mainPower)Toggled=value;}
        void Update()
        {
            if(kind==Kind.Knob && interaction.isSelected && interaction.interactorsSelecting.Count>0)
            {
                // Signed wrist twist around the console-facing shaft; tracking transform stays untouched.
                var hand=interaction.interactorsSelecting[0].transform.rotation;
                var delta=hand*Quaternion.Inverse(previousHand);previousHand=hand;
                delta.ToAngleAxis(out float angle,out var axis);if(angle>180)angle-=360;
                float degrees=angle*Vector3.Dot(axis,transform.forward);
                if(float.IsFinite(degrees))Normalized=Mathf.Clamp01(Normalized+degrees/270f);
                if(frequency)console.Tune(Normalized);
                if(Mathf.Abs(Normalized-detent)>.04f){detent=Normalized;console.Click(.2f);}
            }
            if(kind==Kind.Knob)visual.localRotation=visualRotation*Quaternion.AngleAxis(-135+Normalized*270,Vector3.forward);
            else if(kind==Kind.Toggle)visual.localRotation=visualRotation*Quaternion.Euler(Toggled?-28:28,0,0);
            else visual.localPosition=Vector3.Lerp(visual.localPosition,visualHome-Vector3.forward*depression,1-Mathf.Exp(-35*Time.deltaTime));
        }
        public void ResetControl(){Normalized=0;Toggled=false;depression=0;}
        void OnDestroy(){if(interaction){interaction.selectEntered.RemoveListener(Selected);interaction.selectExited.RemoveListener(Released);}}
    }
}
