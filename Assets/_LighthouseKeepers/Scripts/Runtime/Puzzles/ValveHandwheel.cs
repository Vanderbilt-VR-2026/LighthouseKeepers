using UnityEngine;

namespace LighthouseKeepers.Puzzles
{
    // Adapted from Tapan's station-anchor-binding PR. This is a local event hook,
    // not Storm Watch's authoritative repair rule or a cooperative puzzle solver.
    public sealed class ValveHandwheel : WristTwistControl
    {
        [SerializeField, Min(.01f)] float turnsToClose = 1.5f;
        [SerializeField] PuzzleSocket repairSocket;
        [SerializeField] Transform[] syncedVisuals = System.Array.Empty<Transform>();
        float accumulatedDegrees;
        public float NormalizedProgress => Mathf.Clamp01(accumulatedDegrees / (Mathf.Max(.01f, turnsToClose) * 360));
        public bool IsClosed { get; private set; }

        protected override void ApplyTwist(float degrees)
        {
            if (IsClosed) return;
            float previous = accumulatedDegrees;
            // Reverse motion undoes unfinished progress: rocking a wrist cannot farm a full turn.
            accumulatedDegrees = Mathf.Clamp(previous + degrees, 0, Mathf.Max(.01f, turnsToClose) * 360);
            float applied = accumulatedDegrees - previous;
            transform.localRotation = RestRotation * Quaternion.AngleAxis(accumulatedDegrees, localTwistAxis.normalized);
            foreach (var visual in syncedVisuals)
                if (visual && visual != transform && !visual.IsChildOf(transform))
                    visual.Rotate(transform.TransformDirection(localTwistAxis.normalized), applied, Space.World);
            if (NormalizedProgress < 1) return;
            IsClosed = true;
            if (repairSocket) repairSocket.SignalInteraction();
        }
    }
}
