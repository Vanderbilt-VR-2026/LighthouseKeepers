using UnityEngine;
using UnityEngine.Audio;
namespace LighthouseKeepers.Audio {
public sealed class AcousticDirector:MonoBehaviour {
 [SerializeField] AudioMixer mixer;
 [SerializeField] AudioLowPassFilter thunderFilter;
 string current=""; LighthouseKeepers.Flood.FloodController flood; AudioZone[] zones; float refresh;
 void Start(){flood=FindAnyObjectByType<LighthouseKeepers.Flood.FloodController>();}
 public AudioMixer Mixer=>mixer;
 void Update(){if(!Camera.main)return;var p=Camera.main.transform.position;bool exterior=p.y>9&&new Vector2(p.x,p.z).magnitude>5.1f;
 if(Time.time>refresh){zones=FindObjectsByType<AudioZone>(FindObjectsSortMode.None);refresh=Time.time+2;}
 string next=exterior?"Lantern Balcony":flood&&flood.Height>p.y-1?"Flood Emergency":p.y<3.2f?"Lower Mechanical":p.y<6.4f?"Generator Running":"Interior Normal";
 int priority=int.MinValue;if(zones!=null)foreach(var zone in zones)if(zone&&zone.Contains(p)&&zone.Priority>=priority){next=zone.Snapshot;exterior=zone.Exterior;priority=zone.Priority;}
 if(flood&&flood.Height>p.y-1)next="Flood Emergency";Transition(next,exterior);}
 public void Transition(string snapshot,bool exterior){if(snapshot!=current&&mixer){var s=mixer.FindSnapshot(snapshot);if(s)s.TransitionTo(1.5f);current=snapshot;}
 if(thunderFilter)thunderFilter.cutoffFrequency=Mathf.Lerp(thunderFilter.cutoffFrequency,exterior?18000:1600,Time.deltaTime*2);}
}}
