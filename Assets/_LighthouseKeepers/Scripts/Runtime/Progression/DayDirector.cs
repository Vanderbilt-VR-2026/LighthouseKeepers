using UnityEngine;
using UnityEngine.Events;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Environment;
namespace LighthouseKeepers.Progression {
public sealed class DayDirector:MonoBehaviour {
 [SerializeField] DayProfile[] profiles;
 [SerializeField,Range(1,5)] int previewDay=1;
 [SerializeField] UnityEvent<DayProfile> onProfileApplied=new();
 public DayProfile Current {get;private set;}
 public DayProfile[] Profiles=>profiles;
 void Start()=>ApplyDay(previewDay);
 public void ApplyDay(int day){if(profiles==null||profiles.Length==0)return;previewDay=Mathf.Clamp(day,1,profiles.Length);Current=profiles[previewDay-1];
 var flood=FindAnyObjectByType<FloodController>();if(flood)flood.RiseSpeed=Current.FloodRiseRate;
 var storm=FindAnyObjectByType<StormController>();if(storm)storm.Apply(Current);
 onProfileApplied.Invoke(Current);}
 void OnValidate(){if(Application.isPlaying)ApplyDay(previewDay);}
}}
