using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.Puzzles {
[RequireComponent(typeof(XRGrabInteractable))]
public sealed class LeverSwitch : MonoBehaviour {
 [SerializeField] Vector3 localPivotAxis = Vector3.right;
 [SerializeField] float onAngle = -60f;
 [SerializeField] float toggleThreshold = 0.6f;
 [SerializeField] PuzzleSocket boundSocket;

 XRGrabInteractable grab;
 IXRSelectInteractor activeInteractor;
 Quaternion lastInteractorRotation;
 Quaternion restRotation;
 float currentAngle;
 bool isOn;

 public bool IsOn => isOn;
 public float NormalizedPosition => onAngle == 0f ? 0f : Mathf.Clamp01(currentAngle / onAngle);

 void Awake() {
  grab = GetComponent<XRGrabInteractable>();
  grab.trackPosition = false;
  grab.trackRotation = false;
  restRotation = transform.rotation;
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
  if (activeInteractor == null) return;
  Quaternion current = activeInteractor.GetAttachTransform(grab).rotation;
  Quaternion delta = current * Quaternion.Inverse(lastInteractorRotation);
  lastInteractorRotation = current;
  delta.ToAngleAxis(out float angle, out Vector3 axis);
  if (angle > 180f) angle -= 360f;
  if (Mathf.Abs(angle) < 0.01f) return;
  Vector3 worldPivotAxis = transform.TransformDirection(localPivotAxis.normalized);
  float signedDegrees = angle * Vector3.Dot(axis, worldPivotAxis);
  float low = Mathf.Min(0f, onAngle), high = Mathf.Max(0f, onAngle);
  currentAngle = Mathf.Clamp(currentAngle + signedDegrees, low, high);
  transform.rotation = restRotation * Quaternion.AngleAxis(currentAngle, localPivotAxis.normalized);
  bool shouldBeOn = Mathf.Abs(currentAngle) >= Mathf.Abs(onAngle) * toggleThreshold;
  if (shouldBeOn != isOn) {
   isOn = shouldBeOn;
   boundSocket?.SignalInteraction();
  }
 }
}}
