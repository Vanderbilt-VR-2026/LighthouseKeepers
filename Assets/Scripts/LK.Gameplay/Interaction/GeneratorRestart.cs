using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Gameplay.Interaction
{
    [RequireComponent(typeof(XRSimpleInteractable), typeof(PhysicalGrip))]
    public sealed class GeneratorRestart : MonoBehaviour
    {
        [SerializeField] ConstrainedArc primer, choke;
        [SerializeField] Transform starterGrip;
        [SerializeField, Min(.1f)] float pullTravelM = .4f;
        [SerializeField, Min(.1f)] float returnSpeedMPerSec = 1.4f;
        [SerializeField] bool requiredChokeClosed = true;
        XRSimpleInteractable interactable;
        PhysicalGrip feedback;
        Transform referenceFrame;
        Vector3 restLocal, initialHand;
        float extension, initialExtension;
        bool attempted;
        readonly GeneratorStartState readiness = new GeneratorStartState();
        AudioClip failedCrank, engineCatch;
        public int PrimerStrokes => readiness.PrimerStrokes;
        public float ExtensionM => extension;
        public void Configure(ConstrainedArc primerLever, ConstrainedArc chokeLever, Transform grip)
        {
            if (primer) primer.StrokeCompleted -= Primed;
            primer = primerLever; choke = chokeLever; starterGrip = grip;
            referenceFrame = grip.parent; restLocal = grip.localPosition;
            if (primer) primer.StrokeCompleted += Primed;
            feedback.Configure(grip, HandPose.WrapGrip);
        }
        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>(); feedback = GetComponent<PhysicalGrip>();
            interactable.selectEntered.AddListener(Begin); interactable.selectExited.AddListener(End);
            failedCrank = Resources.Load<AudioClip>("Hands/failed_crank"); engineCatch = Resources.Load<AudioClip>("Hands/engine_catch");
        }
        void Primed(int actor) => readiness.Prime();
        void Begin(SelectEnterEventArgs args)
        { initialHand = args.interactorObject.transform.position; initialExtension = extension; attempted = false; }
        void End(SelectExitEventArgs args) { attempted = false; }
        void Update()
        {
            if (!starterGrip) return;
            var up = referenceFrame ? referenceFrame.up : Vector3.up;
            if (interactable.isSelected && interactable.interactorsSelecting.Count > 0)
            {
                var interactor = interactable.interactorsSelecting[0];
                extension = Mathf.Clamp(initialExtension + Vector3.Dot(initialHand - interactor.transform.position, up), 0, pullTravelM);
                var hand = feedback.HoldingHand; if (hand) hand.SetConstrainedGrip(starterGrip);
                if (!attempted && extension >= pullTravelM * .9f)
                {
                    attempted = true;
                    int actor = hand ? hand.ActorId : 0;
                    if (readiness.TryStart(choke && choke.ChokeClosed == requiredChokeClosed))
                    {
                        feedback.Play(engineCatch); if (hand) hand.Pulse(.8f, .12f);
                        InteractionEvents.StartGenerator(actor);
                    }
                    else
                    {
                        feedback.Play(failedCrank); if (hand) hand.Pulse(.35f, .16f);
                        InteractionEvents.FailedCrank(actor);
                    }
                }
            }
            else extension = Mathf.MoveTowards(extension, 0, returnSpeedMPerSec * Time.deltaTime);
            var rest = referenceFrame ? referenceFrame.TransformPoint(restLocal) : restLocal;
            starterGrip.position = rest - up * extension;
        }
        public void ResetInteraction()
        { readiness.Reset(); extension = 0; attempted = false; if (primer) primer.ResetInteraction(); if (choke) choke.ResetInteraction(); }
        void OnDisable() { if (feedback && feedback.HoldingHand) feedback.HoldingHand.ClearPose(); }
        void OnDestroy()
        {
            if (primer) primer.StrokeCompleted -= Primed;
            if (interactable) { interactable.selectEntered.RemoveListener(Begin); interactable.selectExited.RemoveListener(End); }
        }
    }
}
