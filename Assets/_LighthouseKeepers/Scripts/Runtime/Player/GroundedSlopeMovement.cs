using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

namespace LighthouseKeepers.Player
{
    /// <summary>Follow a supported walkable slope through XRI's queued, collision-constrained movement.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class GroundedSlopeMovement : MonoBehaviour
    {
        [SerializeField] LayerMask groundLayers = (1 << 8) | 1;
        [SerializeField, Min(.01f)] float probeDistance = .12f;

        CharacterController character;
        XRBodyTransformer body;
        ContinuousMoveProvider move;
        GravityProvider gravity;

        void Awake()
        {
            character = GetComponent<CharacterController>();
            body = GetComponentInChildren<XRBodyTransformer>();
            move = GetComponentInChildren<ContinuousMoveProvider>();
            gravity = GetComponentInChildren<GravityProvider>();
        }

        void OnEnable()
        {
            if (body) body.beforeApplyTransformations += FollowSlope;
        }

        void OnDisable()
        {
            if (body) body.beforeApplyTransformations -= FollowSlope;
        }

        void FollowSlope(XRBodyTransformer _)
        {
            if (!character || !character.enabled || !move || !move.isActiveAndEnabled || move.enableFly ||
                !gravity || !gravity.isActiveAndEnabled || !gravity.useGravity ||
                (!character.isGrounded && !gravity.isGrounded)) return;

            var motion = move.transformation.motion;
            var up = transform.up;
            // Leave flying, vertical locomotion and the separate gravity transformation alone.
            if (motion.sqrMagnitude < 1e-8f || Mathf.Abs(Vector3.Dot(motion, up)) > .001f) return;

            float scale = transform.lossyScale.y;
            var lowerSphere = transform.TransformPoint(character.center) -
                up * Mathf.Max(0, character.height * .5f - character.radius) * scale;
            float radius = Mathf.Max(.01f, character.radius - character.skinWidth) * scale;
            if (!Physics.SphereCast(lowerSphere, radius, -up, out var hit,
                    (probeDistance + character.skinWidth) * scale, groundLayers, QueryTriggerInteraction.Ignore)) return;

            float normalUp = Vector3.Dot(hit.normal, up);
            if (normalUp < Mathf.Cos(character.slopeLimit * Mathf.Deg2Rad)) return;

            // Horizontal-only input otherwise walks off the descending ramp until gravity catches up.
            // Add the plane's vertical travel before XRI performs its single constrained movement;
            // retain the requested horizontal speed and do not alter the tracked camera pose.
            var horizontal = Vector3.ProjectOnPlane(motion, up);
            move.transformation.motion = horizontal - up * (Vector3.Dot(horizontal, hit.normal) / normalUp);
        }
    }
}
