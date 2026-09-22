using UnityEngine;
namespace LighthouseKeepers.Progression {
[CreateAssetMenu(menuName="Lighthouse Keepers/Day Profile")]
public sealed class DayProfile:ScriptableObject {
 [SerializeField,Min(1)] int day=1;
 [SerializeField,Range(0.1f,1)] float stormIntensity=0.3f;
 [SerializeField,Min(1)] float lightningInterval=50;
 [SerializeField,Range(0,1)] float thunderProximity=0.2f, lightFailureFrequency=0.05f, machineryInstability=0.1f,radioInterference=0.1f;
 [SerializeField,Min(1)] int leakCount=2;
 [SerializeField,Min(0)] float floodRiseRate=0.003f;
 [SerializeField,Min(0.1f)] float puzzleTimePressure=1, environmentalAudioIntensity=0.7f;
 public int Day=>day;public float StormIntensity=>stormIntensity;public float LightningInterval=>lightningInterval;
 public float ThunderProximity=>thunderProximity;public float LightFailureFrequency=>lightFailureFrequency;
 public int LeakCount=>leakCount;public float FloodRiseRate=>floodRiseRate;public float MachineryInstability=>machineryInstability;
 public float RadioInterference=>radioInterference;public float PuzzleTimePressure=>puzzleTimePressure;public float AudioIntensity=>environmentalAudioIntensity;
}}
