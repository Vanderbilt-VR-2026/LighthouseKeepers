using Fusion;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    /// <summary>Artist-supplied modular VR appearance; the existing view drives its anchors.</summary>
    [DisallowMultipleComponent]
    public sealed class CrewAvatarModel : MonoBehaviour
    {
        [SerializeField, Tooltip("Independent head anchor: origin at the eyes, facing local +Z. Put head/hat meshes below it.")]
        Transform head;
        [SerializeField, Tooltip("Independent body anchor. Put coat/torso meshes below it; do not parent this anchor to the head.")]
        Transform body;
        [SerializeField, Tooltip("Body pivot offset from the tracked eyes, in metres along the body's local axes.")]
        Vector3 bodyOffsetFromHead = new Vector3(0,-.48f,0);
        [SerializeField, Tooltip("Optional independent wrist anchor. Leave empty to retain the existing tracked glove.")]
        Transform leftHand;
        [SerializeField, Tooltip("Optional independent wrist anchor. Leave empty to retain the existing tracked glove.")]
        Transform rightHand;

        public Transform Head => head;
        public Transform Body => body;
        public Transform LeftHand => leftHand;
        public Transform RightHand => rightHand;
        public Vector3 BodyOffsetFromHead => bodyOffsetFromHead;

        bool Inside(Transform anchor) => anchor && anchor != transform && anchor.IsChildOf(transform);
        public bool TryValidate(out string error)
        {
            error = null;
            if (!Inside(head) || !Inside(body))
                error = "Assign head and body anchors inside the model prefab, below its root.";
            else if (head == body || head.IsChildOf(body) || body.IsChildOf(head))
                error = "Head and body must be separate branches. A full humanoid skeleton needs a dedicated IK adapter.";
            else if (leftHand && (!Inside(leftHand) || leftHand == head || leftHand == body) ||
                     rightHand && (!Inside(rightHand) || rightHand == head || rightHand == body) ||
                     leftHand && leftHand == rightHand)
                error = "Optional wrist anchors must be distinct transforms inside the model.";
            else if (leftHand && (head.IsChildOf(leftHand) || body.IsChildOf(leftHand)) ||
                     rightHand && (head.IsChildOf(rightHand) || body.IsChildOf(rightHand)))
                error = "A wrist anchor cannot contain the tracked head or body.";
            else if (leftHand && rightHand && (leftHand.IsChildOf(rightHand) || rightHand.IsChildOf(leftHand)))
                error = "Left and right wrists must be separate branches.";
            else if (!float.IsFinite(bodyOffsetFromHead.x) || !float.IsFinite(bodyOffsetFromHead.y) || !float.IsFinite(bodyOffsetFromHead.z))
                error = "Body offset must contain finite metre values.";
            else if (GetComponentInChildren<NetworkObject>(true) || GetComponentInChildren<NetworkRunner>(true))
                error = "Appearance prefabs must not contain network objects or runners; PF_PlayerRig owns the network identity.";
            else if (GetComponentInChildren<Camera>(true) || GetComponentInChildren<AudioListener>(true))
                error = "Appearance prefabs must not contain a camera or audio listener.";
            return error == null;
        }

        public void PrepareVisualOnly()
        {
            foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (var body in GetComponentsInChildren<Rigidbody>(true))
            { body.isKinematic = true; body.detectCollisions = false; }
        }

        [ContextMenu("Validate avatar model")]
        void ValidateModel()
        {
            if (TryValidate(out var error)) Debug.Log("Avatar anchors and visual-only component requirements are valid. Art alignment still needs a shared-session check.",this);
            else Debug.LogError(error,this);
        }
    }
}
