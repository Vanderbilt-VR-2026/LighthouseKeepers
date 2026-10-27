using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace LighthouseKeepers.Gameplay.Interaction
{
    // Covers XRRayInteractor AND the Starter Assets NearFarInteractor. UI follows its separate path.
    [RequireComponent(typeof(XRInteractionManager))]
    public sealed class DirectGameplayFilter : MonoBehaviour, IXRSelectFilter, IXRHoverFilter
    {
        XRInteractionManager manager;
        public bool canProcess => isActiveAndEnabled;
        void Awake() => manager = GetComponent<XRInteractionManager>();
        void OnEnable() { manager.selectFilters.Add(this); manager.hoverFilters.Add(this); }
        void OnDisable() { manager.selectFilters.Remove(this); manager.hoverFilters.Remove(this); }
        public bool Process(IXRSelectInteractor hand, IXRSelectInteractable target)
            => target is BaseTeleportationInteractable || hand is XRDirectInteractor || hand is XRSocketInteractor;
        public bool Process(IXRHoverInteractor hand, IXRHoverInteractable target)
            => target is BaseTeleportationInteractable || hand is XRDirectInteractor || hand is XRSocketInteractor;
    }
}
