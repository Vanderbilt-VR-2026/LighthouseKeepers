using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

namespace LighthouseKeepers.Audio
{
    /// <summary>Local keeper footsteps from completed rig movement, including collision response.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] GravityProvider gravity;
        [SerializeField] AudioSource source;
        [SerializeField] AudioClip[] clips;
        [SerializeField, Min(.1f)] float strideLength = .65f;
        [SerializeField, Range(0, 1)] float volume = .28f;

        readonly FootstepCadence cadence = new();
        CharacterController character;
        Vector3 previousPosition;
        Quaternion previousRotation;
        int previousClip = -1;
        bool paused;

        public int StepsPlayed { get; private set; }

        void Awake() => character = GetComponent<CharacterController>();
        void OnEnable() => ResetMotion();
        void OnDisable()
        {
            cadence.Reset();
            if (source) source.Stop();
        }
        void OnApplicationPause(bool isPaused)
        {
            paused = isPaused;
            ResetMotion();
        }

        void ResetMotion()
        {
            previousPosition = transform.position;
            previousRotation = transform.rotation;
            cadence.Reset();
        }

        void LateUpdate()
        {
            // Snap/smooth turns rotate the origin around the tracked head. Compensate for that
            // translation, without counting headset bobbing/leaning as walking or changing tracking.
            var bodyOffset = head ? transform.InverseTransformPoint(head.position) : Vector3.zero;
            bodyOffset.y = 0;
            bodyOffset = Vector3.Scale(bodyOffset, transform.lossyScale);
            var displacement = transform.position - previousPosition +
                transform.rotation * bodyOffset - previousRotation * bodyOffset;
            previousPosition = transform.position;
            previousRotation = transform.rotation;

            float distance = Vector3.ProjectOnPlane(displacement, transform.up).magnitude /
                Mathf.Max(.001f, transform.lossyScale.x);
            bool grounded = !paused && character && character.enabled &&
                gravity && gravity.isActiveAndEnabled && gravity.isGrounded;
            if (!cadence.Advance(distance, Time.deltaTime, grounded, strideLength)) return;
            if (!source || clips == null || clips.Length == 0) return;

            // Avoid immediate repeats, but keep volume/pitch variation subtle under the storm mix.
            int index = Random.Range(0, clips.Length);
            if (clips.Length > 1 && index == previousClip) index = (index + 1) % clips.Length;
            if (!clips[index]) return;
            previousClip = index;
            source.pitch = Random.Range(.94f, 1.06f);
            source.PlayOneShot(clips[index], volume * Random.Range(.9f, 1f));
            StepsPlayed++;
        }
    }
}
