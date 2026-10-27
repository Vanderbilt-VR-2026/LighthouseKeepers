using UnityEngine;
using UnityEngine.Events;
using LighthouseKeepers.StormWatch;

namespace LighthouseKeepers.Puzzles
{
    /// <summary>Unlatched local dial: release/regrip preserves signed angle; no clue or bearing rule.</summary>
    public sealed class RotaryDial : WristTwistControl
    {
        [SerializeField] PuzzleSocket boundSocket;
        [SerializeField, Tooltip("Hide this legacy-only dial when Storm Watch supplies an authoritative control at the same location.")]
        bool hideDuringStormWatch;
        [SerializeField] UnityEvent<float> onAngleChanged = new();
        public float AngleDegrees { get; private set; }
        public UnityEvent<float> OnAngleChanged => onAngleChanged;

        protected override void Awake()
        {
            base.Awake();
            if (!hideDuringStormWatch) return;
            // The watch director is loaded before additive environment levels. One startup lookup
            // keeps the rehearsal dial from competing for selection with the centre beacon switch.
            var watch = FindAnyObjectByType<StormWatchDirector>();
            if (watch && watch.isActiveAndEnabled) gameObject.SetActive(false);
        }

        protected override void ApplyTwist(float degrees)
        {
            AngleDegrees = Mathf.Repeat(AngleDegrees + degrees, 360);
            transform.localRotation = RestRotation * Quaternion.AngleAxis(AngleDegrees, localTwistAxis.normalized);
            onAngleChanged.Invoke(AngleDegrees);
            if (boundSocket) boundSocket.SignalInteraction();
        }
    }
}
