using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.Gameplay.Interaction
{
    [RequireComponent(typeof(XRGrabInteractable), typeof(PhysicalGrip))]
    public sealed class WrenchInteraction : MonoBehaviour
    {
        [SerializeField, Min(.1f)] float repairReachM = .22f;
        [SerializeField, Min(30)] float repairDegrees = 540;
        [SerializeField] Vector3 boltAxis = Vector3.forward;
        XRGrabInteractable grab;
        PhysicalGrip feedback;
        Transform leak;
        float progress;
        Vector3 previous;
        bool tracking, repaired, transferPending;
        XRNode lastHand;
        int lastActor;
        float nextTransferAttempt;
        public static void Install(XRGrabInteractable grab)
        {
            if (grab.GetComponent<WrenchInteraction>()) return;
            bool wasEnabled = grab.enabled; grab.enabled = false;
            var authored = StationInteractionBinding.Find(grab.transform, "ATTACH_Wrench");
            var attach = authored ? authored : grab.attachTransform;
            if (!attach || attach.name == "AttachTransform")
            {
                attach = new GameObject("AttachTransform / wrench handle").transform;
                attach.SetParent(grab.transform, false);
                var renderers = grab.GetComponentsInChildren<Renderer>();
                bool hasBounds = false; var bounds = new Bounds();
                foreach (var renderer in renderers)
                {
                    var world = renderer.bounds;
                    for (int i = 0; i < 8; i++)
                    {
                        var point = grab.transform.InverseTransformPoint(world.center + Vector3.Scale(world.extents,
                            new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1)));
                        if (!hasBounds) { bounds = new Bounds(point, Vector3.zero); hasBounds = true; } else bounds.Encapsulate(point);
                    }
                }
                if (hasBounds) attach.localPosition = new Vector3(bounds.center.x, bounds.center.y, Mathf.Lerp(bounds.min.z, bounds.max.z, .27f));
                // Closed fingers wrap about palm X; the canonical wrench shaft points along mesh +Z.
                attach.localRotation = Quaternion.Euler(0, -90, 0);
            }
            grab.attachTransform = attach; grab.useDynamicAttach = false;
            grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            grab.throwOnDetach = true; grab.throwVelocityScale = 1; grab.throwAngularVelocityScale = 1;
            grab.GetComponent<PhysicalGrip>().Configure(attach, HandPose.WrapGrip);
            // A trigger is forgiving for grabbing; the mesh's original colliders retain physical contact.
            var forgiving = grab.gameObject.AddComponent<BoxCollider>(); forgiving.isTrigger = true;
            var scale = grab.transform.lossyScale;
            forgiving.size = new Vector3(.13f / Mathf.Max(.001f, Mathf.Abs(scale.x)), .10f / Mathf.Max(.001f, Mathf.Abs(scale.y)), .28f / Mathf.Max(.001f, Mathf.Abs(scale.z)));
            forgiving.center = grab.transform.InverseTransformPoint(attach.position);
            // Collider list is populated in XRI Awake, so register the additional trigger explicitly.
            grab.colliders.Add(forgiving);
            var body = grab.GetComponent<Rigidbody>(); body.mass = .3f; body.isKinematic = false; body.useGravity = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; body.interpolation = RigidbodyInterpolation.Interpolate;
            grab.gameObject.AddComponent<WrenchInteraction>();
            // Re-register so XRI maps the newly added forgiving collider to this interactable.
            grab.enabled = wasEnabled;
        }
        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>(); feedback = GetComponent<PhysicalGrip>();
            grab.selectEntered.AddListener(Persist); grab.selectExited.AddListener(Released);
        }
        void Persist(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
        {
            // Detach only this tool: never persist its table, floor, or an unrelated scene root.
            var hand = args.interactorObject.transform.GetComponentInParent<HandRig>();
            if (hand) { lastHand = hand.Node; lastActor = hand.ActorId; }
            transferPending = false;
            transform.SetParent(null, true); DontDestroyOnLoad(gameObject);
        }
        void Released(SelectExitEventArgs args)
        {
            // XRI cancellation during rig/scene teardown differs from a physical grip release.
            transferPending = args.isCanceled;
            nextTransferAttempt = 0;
        }
        void RestoreCarry()
        {
            if (!transferPending || Time.unscaledTime < nextTransferAttempt) return;
            nextTransferAttempt = Time.unscaledTime + .25f;
            var device = InputDevices.GetDeviceAtXRNode(lastHand);
            if (device.TryGetFeatureValue(CommonUsages.grip, out float gripValue) && gripValue < .45f) { transferPending = false; return; }
            foreach (var hand in FindObjectsByType<HandRig>(FindObjectsSortMode.None))
            {
                if (hand.Node != lastHand || hand.ActorId != lastActor || !hand.Direct || !hand.Direct.isActiveAndEnabled) continue;
                if (!hand.Direct.selectInput.manualPerformed || hand.Direct.hasSelection) continue;
                if (hand.Direct.interactionManager) hand.Direct.interactionManager.SelectEnter((IXRSelectInteractor)hand.Direct, (IXRSelectInteractable)grab);
                return;
            }
        }
        public void BindLeak(Transform target) { if (leak == target) return; leak = target; ResetRepair(); }
        public void ResetRepair() { repaired = false; progress = 0; tracking = false; }
        void Update()
        {
            RestoreCarry();
            if (!leak || !grab.isSelected || repaired) { tracking = false; return; }
            var head = transform.Find("TIP_Wrench");
            var point = head ? head.position : transform.TransformPoint(new Vector3(0, 0, .16f));
            if (Vector3.Distance(point, leak.position) > repairReachM) { tracking = false; return; }
            var axis = leak.TransformDirection(boltAxis.normalized);
            var direction = Vector3.ProjectOnPlane(transform.up, axis).normalized;
            if (tracking && direction.sqrMagnitude > .5f)
            {
                float degrees = Vector3.SignedAngle(previous, direction, axis);
                // Signed progress prevents back-and-forth wrist jitter from counting as a full repair.
                if (Mathf.Abs(degrees) < 50) progress = Mathf.Max(0, progress + degrees);
                if (progress >= repairDegrees)
                {
                    repaired = true; var hand = feedback.HoldingHand;
                    if (hand) hand.Pulse(.75f, .1f);
                    InteractionEvents.Repair(hand ? hand.ActorId : 0);
                }
            }
            previous = direction; tracking = true;
        }
        void OnDestroy() { if (grab) { grab.selectEntered.RemoveListener(Persist); grab.selectExited.RemoveListener(Released); } }
    }
}
