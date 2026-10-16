using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
namespace LighthouseKeepers.WatchDemo
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class WrenchHaptics : MonoBehaviour
    {
        public const float GrabAmplitude=.3f, GrabDuration=.06f, RepairAmplitude=.6f, RepairDuration=.12f;
        // Observes the request, not proof that a physical controller received it.
        public event Action<float,float> PulseRequested;
        XRGrabInteractable grab;
        XRBaseInputInteractor holdingHand;
        void OnEnable(){grab=GetComponent<XRGrabInteractable>();grab.selectEntered.AddListener(OnGrab);grab.selectExited.AddListener(OnRelease);}
        void OnDisable(){if(grab){grab.selectEntered.RemoveListener(OnGrab);grab.selectExited.RemoveListener(OnRelease);}holdingHand=null;}
        void OnGrab(SelectEnterEventArgs args){holdingHand=args.interactorObject as XRBaseInputInteractor;Pulse(GrabAmplitude,GrabDuration);}
        void OnRelease(SelectExitEventArgs args){holdingHand=null;}
        public void RepairCompleted(){if(grab.isSelected)Pulse(RepairAmplitude,RepairDuration);}
        void Pulse(float amplitude,float duration){PulseRequested?.Invoke(amplitude,duration);if(holdingHand)holdingHand.SendHapticImpulse(amplitude,duration);}
        // Deliberately no Update, repeating coroutine, or vibration while holding/repairing.
    }
}
