using UnityEngine;
namespace LighthouseKeepers.Player {
// Retained for old scene references/validation. The rigged keeper now supplies body presence.
// Hide even serialized scene overrides: the old cube otherwise appears as a board through the coat.
public sealed class BodyPresence:MonoBehaviour {
 [SerializeField] Transform head;
 [SerializeField] float yawSmoothing=3;
 void Awake(){foreach(var visual in GetComponentsInChildren<Renderer>(true))visual.enabled=false;enabled=false;}
}}
