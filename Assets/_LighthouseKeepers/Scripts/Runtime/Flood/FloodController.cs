using System;
using UnityEngine;
using UnityEngine.Events;
namespace LighthouseKeepers.Flood {
public sealed class FloodController:MonoBehaviour {
 [SerializeField] FloodProfile profile;
 [SerializeField,Tooltip("Inspector preview height, in world metres.")] float height=-0.25f;
 [SerializeField] bool paused=true;
 [SerializeField,Min(0)] float riseSpeed=0.003f;
 [SerializeField] UnityEvent<float> onHeightChanged=new();
 public event Action<float> HeightChanged;
 public float Height=>height; public bool Paused {get=>paused;set=>paused=value;}
 public float RiseSpeed {get=>riseSpeed;set=>riseSpeed=Mathf.Max(0,value);}
 public FloodProfile Profile=>profile;
 void Awake(){if(profile)riseSpeed=profile.RiseSpeed;}
 void Start(){SetHeight(height);}
 void Update(){if(!paused)SetHeight(height+riseSpeed*Time.deltaTime);}
 public void SetHeight(float value){height=Mathf.Clamp(value,profile?profile.MinimumHeight:-0.25f,profile?profile.MaximumHeight:10.5f);HeightChanged?.Invoke(height);onHeightChanged.Invoke(height);}
 [ContextMenu("Reset flood safely")] public void ResetFlood(){paused=true;SetHeight(profile?profile.MinimumHeight:-0.25f);}
 [ContextMenu("Resume rising")] public void ResumeFlood()=>paused=false;
 void OnValidate(){if(Application.isPlaying)SetHeight(height);}
}}
