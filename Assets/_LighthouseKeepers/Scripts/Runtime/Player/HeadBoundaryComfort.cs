using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
namespace LighthouseKeepers.Player {
/// <summary>Hide wall penetration from physical room-scale leaning without moving the tracked camera.</summary>
public sealed class HeadBoundaryComfort:MonoBehaviour,ITunnelingVignetteProvider {
 [SerializeField] TunnelingVignetteController vignette;
 readonly VignetteParameters parameters=new(){apertureSize=0,featheringEffect=.05f,easeInTime=.12f,easeOutTime=.25f};
 public VignetteParameters vignetteParameters=>parameters;
 void Update(){if(!vignette)return;if(Physics.CheckSphere(transform.position,.09f,1<<8,QueryTriggerInteraction.Ignore))vignette.BeginTunnelingVignette(this);else vignette.EndTunnelingVignette(this);}
 void OnDisable(){if(vignette)vignette.EndTunnelingVignette(this);}
}}
