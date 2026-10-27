using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.Puzzles
{
    /// <summary>Fixed local station control. Grip, then twist the wrist; release to regrip.</summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public abstract class WristTwistControl : MonoBehaviour
    {
        [SerializeField] protected Vector3 localTwistAxis = Vector3.forward;
        [SerializeField, Min(.01f), Tooltip("Physical hand reach in metres. A distant ray cannot turn the control.")]
        float handReach = .65f;
        XRSimpleInteractable interactable;
        IXRSelectInteractor trackedHand;
        Quaternion previousRotation;
        protected Quaternion RestRotation { get; private set; }

        protected virtual void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            RestRotation = transform.localRotation;
        }

        protected virtual void OnDisable() => trackedHand = null;

        void Update()
        {
            if (interactable.interactorsSelecting.Count == 0) { trackedHand = null; return; }
            var hand = interactable.interactorsSelecting[0];
            if (Vector3.Distance(hand.transform.position, transform.position) > handReach)
            { trackedHand = null; return; }
            // Use the physical controller pose, never a far-ray attach point at the target.
            var rotation = hand.transform.rotation;
            if (!ReferenceEquals(hand, trackedHand))
            { trackedHand = hand; previousRotation = rotation; return; }
            var delta = rotation * Quaternion.Inverse(previousRotation);
            previousRotation = rotation;
            var axis = transform.TransformDirection(localTwistAxis.normalized);
            // Swing-twist projection rejects wrist swing perpendicular to the control's axis.
            float projection = Vector3.Dot(new Vector3(delta.x, delta.y, delta.z), axis);
            if (projection * projection + delta.w * delta.w < .000001f) return;
            float degrees = Mathf.DeltaAngle(0, 2 * Mathf.Atan2(projection, delta.w) * Mathf.Rad2Deg);
            // Tracking jumps cannot complete a control; establish a fresh baseline next frame.
            if (float.IsNaN(degrees) || float.IsInfinity(degrees) || Mathf.Abs(degrees) > 90) return;
            if (Mathf.Abs(degrees) > .01f) ApplyTwist(degrees);
        }

        protected abstract void ApplyTwist(float degrees);
    }
}
