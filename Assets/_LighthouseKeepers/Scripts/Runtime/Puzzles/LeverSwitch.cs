using UnityEngine;
using UnityEngine.Events;

namespace LighthouseKeepers.Puzzles
{
    // Adapted from Tapan's generator-starter-lever PR; no generator power rule is added.
    public sealed class LeverSwitch : WristTwistControl
    {
        [SerializeField] float onAngle = -60;
        [SerializeField, Range(.1f, .9f)] float toggleThreshold = .6f;
        [SerializeField, Range(.01f, .3f)] float releaseHysteresis = .1f;
        [SerializeField] PuzzleSocket boundSocket;
        [SerializeField] UnityEvent<bool> onStateChanged = new();
        float angle;
        public bool IsOn { get; private set; }
        public float NormalizedPosition => Mathf.Abs(onAngle) < .01f ? 0 : Mathf.Clamp01(angle / onAngle);
        public UnityEvent<bool> OnStateChanged => onStateChanged;

        protected override void ApplyTwist(float degrees)
        {
            angle = Mathf.Clamp(angle + degrees, Mathf.Min(0, onAngle), Mathf.Max(0, onAngle));
            transform.localRotation = RestRotation * Quaternion.AngleAxis(angle, localTwistAxis.normalized);
            bool next = IsOn ? NormalizedPosition > Mathf.Max(0, toggleThreshold - releaseHysteresis) : NormalizedPosition >= toggleThreshold;
            if (next == IsOn) return;
            IsOn = next;
            onStateChanged.Invoke(IsOn);
            if (boundSocket) boundSocket.SignalInteraction();
        }
    }
}
