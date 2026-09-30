using System;
using UnityEngine;
using UnityEngine.Events;
namespace LighthouseKeepers.Flood {
public sealed class FloodController:MonoBehaviour {
 [SerializeField] FloodProfile profile;
 [SerializeField,Tooltip("Inspector preview height, in world metres.")] float height=-0.25f;
 [SerializeField] bool paused=true;
 [SerializeField,Min(0)] float riseSpeed=0.003f;
 [SerializeField,Min(0),Tooltip("Fractional rise acceleration per metre of water risen. The flood gets faster the higher it gets.")] float surgePerMetre=0.1f;
 [SerializeField] UnityEvent<float> onHeightChanged=new();
 public event Action<float> HeightChanged;
 public float Height=>height; public bool Paused {get=>paused;set=>paused=value;}
 public float RiseSpeed {get=>riseSpeed;set=>riseSpeed=Mathf.Max(0,value);}
 public float SurgePerMetre {get=>surgePerMetre;set=>surgePerMetre=Mathf.Max(0,value);}
 public FloodProfile Profile=>profile;
 public float MinimumHeight=>profile?profile.MinimumHeight:-0.25f;
 public float MaximumHeight=>profile?profile.MaximumHeight:10.5f;
 public float EffectiveRiseSpeed=>riseSpeed*(1+surgePerMetre*Mathf.Max(0,height-MinimumHeight));
 void Awake(){if(profile){riseSpeed=profile.RiseSpeed;surgePerMetre=profile.SurgePerMetre;}}
 void Start(){SetHeight(height);}
 void Update(){if(!paused&&riseSpeed>0&&height<MaximumHeight)SetHeight(height+EffectiveRiseSpeed*Time.deltaTime);}
 public void SetHeight(float value){height=Mathf.Clamp(value,MinimumHeight,MaximumHeight);HeightChanged?.Invoke(height);onHeightChanged.Invoke(height);}
 /// <summary>Seconds until a stationary world-space target is reached; unreachable targets have no ETA.</summary>
 public float TimeToHeight(float target){if(target<=height)return 0;if(target>MaximumHeight||paused||riseSpeed<=0)return float.PositiveInfinity;if(surgePerMetre<=0)return (target-height)/riseSpeed;float gained=1+surgePerMetre*(height-MinimumHeight),wanted=1+surgePerMetre*(target-MinimumHeight);return Mathf.Log(wanted/gained)/(riseSpeed*surgePerMetre);}
 [ContextMenu("Reset flood safely")] public void ResetFlood(){paused=true;SetHeight(MinimumHeight);}
 [ContextMenu("Resume rising")] public void ResumeFlood()=>paused=false;
 void OnValidate(){if(Application.isPlaying)SetHeight(height);}
}}
