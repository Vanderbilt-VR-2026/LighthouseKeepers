using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace LighthouseKeepers.WatchDemo
{
    // Constrain the existing grabbable, not the tracked controller. Its regular palm attach
    // and first-grab network lease remain in charge; the host observes the resulting tool pose.
    public sealed class ImmersionWrenchSocket : XRBaseGrabTransformer
    {
        WatchPipeRepair pipe;
        WatchState watch;
        ImmersionTuning tuning;
        Vector3 jawLocal, handAtSeat;
        Quaternion wristPrevious, seatedRotation;
        Transform controller, palm;
        HandRig hand;
        int bolt = -1, frame = -1;
        float angle;
        public bool IsSnapped => bolt >= 0;
        public int BoltIndex => bolt;

        public void Initialize(WatchPipeRepair owner, WatchState state)
        {
            pipe = owner; watch = state; tuning = state.Immersion;
            jawLocal = transform.InverseTransformPoint(pipe.jaw.position);
            palm = new GameObject("Seated wrench palm").transform;
            palm.SetParent(transform, false);
        }

        public override void Process(XRGrabInteractable grab, XRInteractionUpdateOrder.UpdatePhase phase,
            ref Pose targetPose, ref Vector3 localScale)
        {
            if (!pipe || !tuning || !tuning.forgivingPipe || watch.Failed || watch.Pipe.Complete ||
                !grab.isSelected || grab.interactorsSelecting.Count != 1)
            { Unseat(); return; }
            var holder = grab.interactorsSelecting[0];
            var holdingHand = holder.transform.GetComponentInParent<HandRig>();
            // Direct-interactor frames differ between mirrored hands. Wrist roll belongs to
            // the tracked controller parent, not the palm/interactor's authored local axes.
            var tracked = holdingHand ? holdingHand.transform.parent : holder.transform;
            int active = watch.Pipe.ActiveBolt;
            if (bolt >= 0 && (active != bolt || controller != tracked ||
                (tuning.leverPipe ? OutsideLever(tracked.position, bolt) : Vector3.Distance(tracked.position, handAtSeat) > tuning.wrenchReleaseMetres)))
            { Unseat(); return; }

            if (bolt < 0)
            {
                var predictedJaw = targetPose.position + targetPose.rotation * Vector3.Scale(jawLocal, localScale);
                if (Vector3.Distance(predictedJaw, pipe.SocketPosition(active)) > tuning.wrenchSnapMetres) return;
                bolt = active; controller = tracked; handAtSeat = tracked.position;
                wristPrevious = tracked.rotation; angle = 0;
                Vector3 normal = pipe.bolts[bolt].forward;
                if (Vector3.Dot(targetPose.rotation * Vector3.up, normal) < 0) normal = -normal;
                seatedRotation = Quaternion.FromToRotation(targetPose.rotation * Vector3.up, normal) * targetPose.rotation;
                hand = holdingHand;
                if (hand)
                {
                    var visual = hand.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (visual)
                    {
                        palm.SetPositionAndRotation(visual.transform.position + visual.transform.rotation * new Vector3(0, -.018f, .025f), visual.transform.rotation);
                        hand.SetConstrainedGrip(palm);
                    }
                    hand.Pulse(.4f, .06f);
                }
                var clunk = Resources.Load<AudioClip>("S4Audio/bolt-clunk");
                if (clunk) pipe.audioSource.PlayOneShot(clunk, .6f);
                frame = Time.frameCount;
            }
            if (tuning.leverPipe)
            {
                // The hand, not wrist orientation, is the lever. Never add an angular
                // offset on snap: doing so makes the tool fight the player's arm.
                var pivot = pipe.SocketPosition(bolt);
                var normal = pipe.bolts[bolt].forward;
                var radial = Vector3.ProjectOnPlane(tracked.position - pivot, normal);
                if (radial.sqrMagnitude > .0016f)
                    seatedRotation = Quaternion.LookRotation(-radial.normalized, normal);
                targetPose.rotation = seatedRotation;
                targetPose.position = pivot - seatedRotation * Vector3.Scale(jawLocal, localScale);
                return;
            }
            if (frame != Time.frameCount)
            {
                var delta = tracked.rotation * Quaternion.Inverse(wristPrevious);
                var rollAxis = (wristPrevious * Vector3.forward + tracked.forward).normalized;
                float roll = WatchPipeRepair.SignedTwist(delta,rollAxis);
                if (float.IsFinite(roll)) angle += Mathf.Clamp(roll, -720 * Time.deltaTime, 720 * Time.deltaTime);
                wristPrevious = tracked.rotation; frame = Time.frameCount;
            }
            targetPose.rotation = Quaternion.AngleAxis(angle, pipe.bolts[bolt].forward) * seatedRotation;
            targetPose.position = pipe.SocketPosition(bolt) - targetPose.rotation * Vector3.Scale(jawLocal, localScale);
        }
        bool OutsideLever(Vector3 position, int index)
        {
            var offset = position - pipe.SocketPosition(index);
            var normal = pipe.bolts[index].forward;
            return Mathf.Abs(Vector3.Dot(offset, normal)) > tuning.leverReleaseHeight ||
                Vector3.ProjectOnPlane(offset, normal).magnitude > tuning.leverReleaseRadius;
        }
        public override void OnGrab(XRGrabInteractable grab) => Unseat();
        void Update() { if (pipe && !pipe.wrench.isSelected) Unseat(); }
        void Unseat()
        {
            if (hand) hand.SetConstrainedGrip(null);
            hand = null; controller = null; bolt = -1; angle = 0; frame = -1;
        }
        protected override void OnDestroy() { Unseat(); base.OnDestroy(); }
    }
}
