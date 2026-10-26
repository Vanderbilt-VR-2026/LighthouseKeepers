using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.Puzzles {
[RequireComponent(typeof(XRGrabInteractable))]
public sealed class ValveHandwheel : MonoBehaviour {
 [SerializeField] Vector3 localTwistAxis = Vector3.up;
 [SerializeField] float turnsToClose = 1.5f;
 [SerializeField] PuzzleSocket repairSocket;
 [Tooltip("Sibling visual pieces (e.g. spoke handles) that share this wheel's pivot and should spin with it, without being reparented.")]
 [SerializeField] Transform[] syncedVisuals = System.Array.Empty<Transform>();

 XRGrabInteractable grab;
 IXRSelectInteractor activeInteractor;
 Quaternion lastInteractorRotation;
 float accumulatedDegrees;
 bool closed;

 public float NormalizedProgress => Mathf.Clamp01(accumulatedDegrees / (turnsToClose * 360f));
 public bool IsClosed => closed;
 public Transform[] SyncedVisuals => syncedVisuals;

 void Awake() {
  grab = GetComponent<XRGrabInteractable>();
  grab.trackPosition = false;
  grab.trackRotation = false;
  grab.selectEntered.AddListener(OnSelectEntered);
  grab.selectExited.AddListener(OnSelectExited);
 }

 void OnDestroy() {
  grab.selectEntered.RemoveListener(OnSelectEntered);
  grab.selectExited.RemoveListener(OnSelectExited);
 }

 void OnSelectEntered(SelectEnterEventArgs args) {
  activeInteractor = args.interactorObject;
  lastInteractorRotation = activeInteractor.GetAttachTransform(grab).rotation;
 }

 void OnSelectExited(SelectExitEventArgs args) {
  if (ReferenceEquals(activeInteractor, args.interactorObject)) activeInteractor = null;
 }

 void Update() {
  if (closed || activeInteractor == null) return;
  Quaternion current = activeInteractor.GetAttachTransform(grab).rotation;
  Quaternion delta = current * Quaternion.Inverse(lastInteractorRotation);
  lastInteractorRotation = current;
  delta.ToAngleAxis(out float angle, out Vector3 axis);
  if (angle > 180f) angle -= 360f;
  if (Mathf.Abs(angle) < 0.01f) return;
  Vector3 worldTwistAxis = transform.TransformDirection(localTwistAxis.normalized);
  float signedDegrees = angle * Vector3.Dot(axis, worldTwistAxis);
  transform.Rotate(worldTwistAxis, signedDegrees, Space.World);
  foreach (var visual in syncedVisuals) if (visual) visual.Rotate(worldTwistAxis, signedDegrees, Space.World);
  accumulatedDegrees += Mathf.Abs(signedDegrees);
  if (accumulatedDegrees >= turnsToClose * 360f) {
   closed = true;
   repairSocket?.SignalInteraction();
  }
 }
}}
