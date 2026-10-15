using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace LighthouseKeepers.Progression
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class FirstWatchValve : MonoBehaviour
    {
        [SerializeField] FirstWatchDirector director;
        [SerializeField] int valveIndex;
        [SerializeField] bool isBrace;
        [SerializeField] Transform wheel;
        [SerializeField] Renderer indicator;
        [SerializeField, Min(.01f), Tooltip("Maximum hand distance from the control, in metres. Distant ray selection cannot turn a valve.")]
        float handReach = .65f;
        XRSimpleInteractable interactable;
        Vector3 previousHandDirection;
        bool tracking;
        MaterialPropertyBlock color;
        public bool IsHeld => interactable && interactable.isSelected && HandIsNear();

        void Awake() { interactable = GetComponent<XRSimpleInteractable>(); color = new MaterialPropertyBlock(); }
        bool HandIsNear()
        {
            if (interactable.interactorsSelecting.Count == 0) return false;
            var hand = interactable.interactorsSelecting[0].transform;
            return Vector3.Distance(hand.position, transform.position) < handReach;
        }
        void Update()
        {
            if (!director || !director.Ready) return;
            bool held = IsHeld;
            if (!isBrace && held)
            {
                // Wrist orientation works with either near-grab hand, without moving the player's rig.
                var hand = interactable.interactorsSelecting[0].transform;
                var direction = Vector3.ProjectOnPlane(hand.up, transform.forward).normalized;
                if (tracking && direction.sqrMagnitude > .5f && previousHandDirection.sqrMagnitude > .5f)
                {
                    float delta = Mathf.Abs(Vector3.SignedAngle(previousHandDirection, direction, transform.forward));
                    if (delta > .2f && delta < 45)
                    {
                        var result = director.TurnValve(valveIndex, delta);
                        if (result == ValveResult.Progress || result == ValveResult.ValveSealed || result == ValveResult.RepairComplete)
                            wheel.Rotate(0, 0, delta, Space.Self);
                        if (result == ValveResult.ValveSealed || result == ValveResult.RepairComplete)
                        {
                            var haptics = hand.GetComponentInParent<HapticImpulsePlayer>();
                            if (haptics) haptics.SendHapticImpulse(.25f, .08f);
                        }
                    }
                }
                previousHandDirection = direction; tracking = true;
            }
            else tracking = false;
            bool sealedValve = !isBrace && director.Rules.IsSealed(valveIndex);
            Color tint = sealedValve ? new Color(.25f,.85f,.48f) : isBrace && director.Braced ? new Color(.25f,.85f,.48f) : new Color(.83f,.55f,.21f);
            if (indicator) { color.SetColor("_BaseColor", tint); color.SetColor("_Color", tint); indicator.SetPropertyBlock(color); }
        }
    }
}
