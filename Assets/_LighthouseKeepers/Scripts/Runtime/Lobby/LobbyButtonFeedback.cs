using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace LighthouseKeepers.Lobby
{
    /// <summary>Touch/ray feedback on a fixed hit target; no head movement or looping haptics.</summary>
    public sealed class LobbyButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        Button button;
        RectTransform face;
        Vector3 rest;
        public void Initialize(Button control, RectTransform label)
        {
            button = control; face = label; rest = face.localPosition;
        }
        public void OnPointerEnter(PointerEventData data)
        {
            if (!button.IsInteractable()) return;
            Pulse(data, .035f, .025f);
        }
        public void OnPointerDown(PointerEventData data)
        {
            if (!button.IsInteractable()) return;
            face.localPosition = rest + Vector3.forward * 1.5f;
            Pulse(data, .15f, .045f);
        }
        public void OnPointerUp(PointerEventData data) => ResetFace();
        public void OnPointerExit(PointerEventData data) => ResetFace();
        void OnDisable() => ResetFace();
        void ResetFace() { if (face) face.localPosition = rest; }
        static void Pulse(PointerEventData data, float amplitude, float duration)
        {
            if (data is TrackedDeviceEventData tracked && tracked.interactor is Component source)
            {
                var haptics = source.GetComponentInParent<HapticImpulsePlayer>();
                if (haptics) haptics.SendHapticImpulse(amplitude, duration);
            }
        }
    }
}
