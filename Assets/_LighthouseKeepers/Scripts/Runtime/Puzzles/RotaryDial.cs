using UnityEngine;
using UnityEngine.Events;

namespace LighthouseKeepers.Puzzles
{
    /// <summary>Unlatched local dial: release/regrip preserves signed angle; no clue or bearing rule.</summary>
    public sealed class RotaryDial : WristTwistControl
    {
        [SerializeField] PuzzleSocket boundSocket;
        [SerializeField] UnityEvent<float> onAngleChanged = new();
        public float AngleDegrees { get; private set; }
        public UnityEvent<float> OnAngleChanged => onAngleChanged;

        protected override void ApplyTwist(float degrees)
        {
            AngleDegrees = Mathf.Repeat(AngleDegrees + degrees, 360);
            transform.localRotation = RestRotation * Quaternion.AngleAxis(AngleDegrees, localTwistAxis.normalized);
            onAngleChanged.Invoke(AngleDegrees);
            if (boundSocket) boundSocket.SignalInteraction();
        }
    }
}
