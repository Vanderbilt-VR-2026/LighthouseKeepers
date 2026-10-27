using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Puzzles
{
    // Adapted from Tapan's radio-interaction PR. Physical press state only; no voice transport.
    [RequireComponent(typeof(XRBaseInteractable))]
    public sealed class PushToTalkButton : MonoBehaviour
    {
        [SerializeField] PuzzleSocket boundSocket;
        [SerializeField] UnityEvent<bool> onTalkingChanged = new();
        XRBaseInteractable interactable;
        public bool IsTalking { get; private set; }
        public UnityEvent<bool> OnTalkingChanged => onTalkingChanged;

        void Awake() => interactable = GetComponent<XRBaseInteractable>();
        void OnEnable()
        {
            interactable.activated.AddListener(OnActivated);
            interactable.deactivated.AddListener(OnDeactivated);
            interactable.lastSelectExited.AddListener(OnReleased);
        }
        void OnDisable()
        {
            interactable.activated.RemoveListener(OnActivated);
            interactable.deactivated.RemoveListener(OnDeactivated);
            interactable.lastSelectExited.RemoveListener(OnReleased);
            SetTalking(false);
        }
        void OnActivated(ActivateEventArgs args) { if (interactable.isSelected) SetTalking(true); }
        void OnDeactivated(DeactivateEventArgs args) => SetTalking(false);
        void OnReleased(SelectExitEventArgs args) => SetTalking(false);
        void SetTalking(bool value)
        {
            if (value == IsTalking) return;
            IsTalking = value;
            onTalkingChanged.Invoke(value);
            if (value && boundSocket) boundSocket.SignalInteraction();
        }
    }
}
