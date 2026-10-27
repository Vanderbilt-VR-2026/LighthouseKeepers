using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.Gameplay.Interaction
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public sealed class PhysicalGrip : MonoBehaviour, IHandPoseTarget, IXRSelectFilter, IXRHoverFilter
    {
        [SerializeField] HandPose pose = HandPose.WrapGrip;
        [SerializeField] Transform attachTransform;
        XRBaseInteractable interactable;
        AudioSource source;
        AudioClip grabSound, releaseSound, impactSound;
        Material rim;
        readonly List<HandRig> hovering = new List<HandRig>(2);
        public HandPose RequiredPose => pose;
        public Transform AttachTransform => attachTransform;
        public bool canProcess => isActiveAndEnabled;
        public HandRig HoldingHand { get; private set; }
        public void Configure(Transform attach, HandPose requiredPose)
        {
            attachTransform = attach; pose = requiredPose;
            if (interactable is XRGrabInteractable grab) grab.attachTransform = attach;
        }
        void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
            if (!attachTransform && interactable is XRGrabInteractable authoredGrab) attachTransform = authoredGrab.attachTransform;
            if (!attachTransform) { attachTransform = new GameObject("AttachTransform").transform; attachTransform.SetParent(transform, false); }
            if (interactable is XRGrabInteractable grab)
            { grab.attachTransform = attachTransform; grab.useDynamicAttach = false; grab.throwOnDetach = true; grab.retainTransformParent = false; }
            source = gameObject.AddComponent<AudioSource>(); source.spatialBlend = 1; source.playOnAwake = false; source.minDistance = .35f; source.maxDistance = 10;
            grabSound = Resources.Load<AudioClip>("Hands/grab"); releaseSound = Resources.Load<AudioClip>("Hands/release"); impactSound = Resources.Load<AudioClip>("Hands/impact");
            var shader = Resources.Load<Shader>("Hands/GripRim");
            if (shader)
            {
                rim = new Material(shader); IncludeVisuals(transform);
            }
            SetRim(0);
        }
        public void IncludeVisuals(Transform root)
        {
            if (!rim) return;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
                var original = r.sharedMaterials;
                bool present = false; foreach (var m in original) if (m == rim) present = true;
                if (present) continue;
                var materials = new Material[original.Length + 1];
                System.Array.Copy(original, materials, original.Length); materials[original.Length] = rim; r.sharedMaterials = materials;
            }
        }
        void OnEnable()
        {
            interactable.selectFilters.Add(this); interactable.hoverFilters.Add(this);
            interactable.hoverEntered.AddListener(Hover); interactable.hoverExited.AddListener(Unhover);
            interactable.selectEntered.AddListener(Grab); interactable.selectExited.AddListener(Release);
        }
        void OnDisable()
        {
            interactable.selectFilters.Remove(this); interactable.hoverFilters.Remove(this);
            interactable.hoverEntered.RemoveListener(Hover); interactable.hoverExited.RemoveListener(Unhover);
            interactable.selectEntered.RemoveListener(Grab); interactable.selectExited.RemoveListener(Release);
            if (HoldingHand) HoldingHand.ClearPose(); HoldingHand = null; hovering.Clear(); SetRim(0);
        }
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable target) => interactor is XRDirectInteractor;
        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable target) => interactor is XRDirectInteractor;
        static HandRig Hand(Transform t) => t.GetComponentInParent<HandRig>();
        void Hover(HoverEnterEventArgs e)
        { var hand = Hand(e.interactorObject.transform); if (hand && !hovering.Contains(hand)) { hovering.Add(hand); hand.Pulse(.3f, .06f); } SetRim(interactable.isSelected ? 1 : .4f); }
        void Unhover(HoverExitEventArgs e)
        { hovering.Remove(Hand(e.interactorObject.transform)); SetRim(interactable.isSelected ? 1 : hovering.Count > 0 ? .4f : 0); }
        void Grab(SelectEnterEventArgs e)
        { HoldingHand = Hand(e.interactorObject.transform); if (HoldingHand) { HoldingHand.SetPose(pose, this); HoldingHand.Pulse(.6f, .10f); } Play(grabSound); SetRim(1); }
        void Release(SelectExitEventArgs e)
        { var hand = Hand(e.interactorObject.transform); if (hand) { hand.ClearPose(); hand.Pulse(.2f, .04f); } HoldingHand = null; Play(releaseSound); SetRim(hovering.Count > 0 ? .4f : 0); }
        void SetRim(float strength) { if (rim) rim.SetFloat("_Strength", strength); }
        public void Play(AudioClip clip, float volume = 1) { if (clip && source) source.PlayOneShot(clip, volume); }
        void OnCollisionEnter(Collision collision)
        { if (collision.relativeVelocity.sqrMagnitude > .25f) Play(impactSound, Mathf.Clamp01(collision.relativeVelocity.magnitude / 3)); }
        void OnDestroy() { if (rim) Destroy(rim); }
    }
}
