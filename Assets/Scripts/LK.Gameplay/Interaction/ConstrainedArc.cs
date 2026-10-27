using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Gameplay.Interaction
{
    [RequireComponent(typeof(XRSimpleInteractable), typeof(PhysicalGrip))]
    public sealed class ConstrainedArc : MonoBehaviour
    {
        public enum Purpose { ManualPump, Primer, Choke }
        [SerializeField] Transform pivot;
        [SerializeField] Transform grip;
        [SerializeField] Vector3 localAxis = Vector3.left;
        [SerializeField, Range(5, 90)] float travelDegrees = 45;
        [SerializeField] Purpose purpose;
        [SerializeField] ParticleSystem discharge;
        [SerializeField] InteractionSettings settings;
        [SerializeField, Min(.1f)] float primerMinimumSeconds = .3f;
        XRSimpleInteractable interactable;
        PhysicalGrip feedback;
        Quaternion rest;
        Vector3 initialHandDirection, gripPivotOffset;
        Quaternion gripPivotRotation;
        bool gripIsChild;
        float grabAngle, normalized;
        float minimumSeconds;
        bool timingValid, chokeClosed;
        float nextResistance;
        readonly StrokeCycle cycle = new StrokeCycle();
        AudioClip topClip, gulpClip, detentClip;
        public float NormalizedTravel => normalized;
        public bool ChokeClosed => chokeClosed;
        public event System.Action<int> StrokeCompleted;
        public event System.Action<int, bool> ChokeChanged;
        public void Configure(Transform axisPivot, Transform gripAnchor, Purpose verb, InteractionSettings tuning, ParticleSystem surge = null)
        {
            pivot = axisPivot; grip = gripAnchor; purpose = verb; settings = tuning; discharge = surge;
            travelDegrees = verb == Purpose.ManualPump ? 45 : 25;
            rest = pivot.localRotation;
            gripPivotOffset = pivot.InverseTransformPoint(grip.position);
            gripPivotRotation = Quaternion.Inverse(pivot.rotation) * grip.rotation;
            gripIsChild = grip.IsChildOf(pivot);
            timingValid = verb != Purpose.ManualPump;
            minimumSeconds = primerMinimumSeconds;
            if (verb == Purpose.ManualPump) timingValid = settings && settings.TryGetMinimumStrokeSeconds(out minimumSeconds);
            if (!timingValid) Debug.LogError("Manual pump has no valid FloodTuning.manualPumpMinStrokeSeconds; stroke events are disabled until B's tuning is bound.", this);
            feedback.Configure(grip, HandPose.PumpHandle);
            feedback.IncludeVisuals(pivot);
        }
        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>(); feedback = GetComponent<PhysicalGrip>();
            topClip = Resources.Load<AudioClip>("Hands/clunk"); gulpClip = Resources.Load<AudioClip>("Hands/gulp"); detentClip = Resources.Load<AudioClip>("Hands/detent");
            interactable.selectEntered.AddListener(Begin); interactable.selectExited.AddListener(End);
        }
        void Begin(SelectEnterEventArgs args)
        {
            cycle.Reset();
            if (!pivot || !grip) return;
            grabAngle = normalized * travelDegrees;
            initialHandDirection = Direction(args.interactorObject.transform.position);
            if (normalized <= .1f) cycle.Advance(normalized, Time.unscaledTimeAsDouble, minimumSeconds);
        }
        void End(SelectExitEventArgs args)
        {
            cycle.Reset();
            if (purpose == Purpose.Choke && pivot)
            {
                normalized = chokeClosed ? 1 : 0;
                ApplyPose();
            }
        }
        Vector3 Direction(Vector3 handPosition)
        { return Vector3.ProjectOnPlane(handPosition - pivot.position, pivot.parent ? pivot.parent.TransformDirection(rest * localAxis.normalized) : rest * localAxis.normalized).normalized; }
        void Update()
        {
            if (!pivot || !grip || !interactable.isSelected || interactable.interactorsSelecting.Count == 0) return;
            var interactor = interactable.interactorsSelecting[0];
            var direction = Direction(interactor.transform.position);
            var axis = pivot.parent ? pivot.parent.TransformDirection(rest * localAxis.normalized) : rest * localAxis.normalized;
            if (direction.sqrMagnitude < .5f || initialHandDirection.sqrMagnitude < .5f) return;
            float previousTravel = normalized;
            normalized = Mathf.Clamp01((grabAngle + Vector3.SignedAngle(initialHandDirection, direction, axis)) / travelDegrees);
            var hand = feedback.HoldingHand;
            if (hand) hand.SetConstrainedGrip(grip);
            if (purpose == Purpose.Choke)
            {
                bool next = chokeClosed ? normalized > .4f : normalized >= .6f;
                if (next != chokeClosed)
                {
                    chokeClosed = next; feedback.Play(detentClip); if (hand) hand.Pulse(.4f, .05f);
                    int actor = hand ? hand.ActorId : 0; ChokeChanged?.Invoke(actor, next); InteractionEvents.Choke(actor, next);
                }
                normalized = chokeClosed ? 1 : 0;
                ApplyPose();
                return;
            }
            ApplyPose();
            if (hand && Mathf.Abs(normalized - previousTravel) > .002f && Time.unscaledTime >= nextResistance)
            { hand.Pulse(Mathf.Lerp(.07f, .22f, 1 - normalized), .035f); nextResistance = Time.unscaledTime + .1f; }
            if (!timingValid) return;
            bool stroke = cycle.Advance(normalized, Time.unscaledTimeAsDouble, minimumSeconds);
            if (cycle.TopReached) feedback.Play(topClip);
            if (!stroke) return;
            int id = hand ? hand.ActorId : 0;
            if (hand) hand.Pulse(.8f, .10f);
            nextResistance = Time.unscaledTime + .16f;
            feedback.Play(purpose == Purpose.ManualPump ? gulpClip : topClip);
            if (discharge) discharge.Emit(18);
            StrokeCompleted?.Invoke(id);
            if (purpose == Purpose.ManualPump) InteractionEvents.Pump(id); else InteractionEvents.Primer(id);
        }
        void ApplyPose()
        {
            pivot.localRotation = rest * Quaternion.AngleAxis(normalized * travelDegrees, localAxis.normalized);
            if (!gripIsChild && grip) grip.SetPositionAndRotation(pivot.TransformPoint(gripPivotOffset), pivot.rotation * gripPivotRotation);
        }
        public void ResetInteraction()
        { cycle.Reset(); normalized = 0; chokeClosed = false; if (pivot) ApplyPose(); }
        void OnDisable() { cycle.Reset(); if (feedback && feedback.HoldingHand) feedback.HoldingHand.ClearPose(); }
        void OnDestroy()
        { if (interactable) { interactable.selectEntered.RemoveListener(Begin); interactable.selectExited.RemoveListener(End); } }
    }
}
