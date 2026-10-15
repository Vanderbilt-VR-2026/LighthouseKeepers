using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Puzzles {
[RequireComponent(typeof(XRBaseInteractable))]
public sealed class PushToTalkButton : MonoBehaviour {
 [SerializeField] PuzzleSocket boundSocket;

 XRBaseInteractable interactable;
 bool talking;

 public bool IsTalking => talking;

 void Awake() {
  interactable = GetComponent<XRBaseInteractable>();
  interactable.activated.AddListener(OnActivated);
  interactable.deactivated.AddListener(OnDeactivated);
 }

 void OnDestroy() {
  interactable.activated.RemoveListener(OnActivated);
  interactable.deactivated.RemoveListener(OnDeactivated);
 }

 void OnActivated(ActivateEventArgs args) {
  talking = true;
  boundSocket?.SignalInteraction();
 }

 void OnDeactivated(DeactivateEventArgs args) {
  talking = false;
 }
}}
