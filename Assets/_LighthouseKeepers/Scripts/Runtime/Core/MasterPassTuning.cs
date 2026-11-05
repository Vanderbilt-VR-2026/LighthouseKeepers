using UnityEngine;
namespace LighthouseKeepers.Core {
 [CreateAssetMenu(menuName="Lighthouse/Master pass tuning")]
 public sealed class MasterPassTuning:ScriptableObject {
  public bool objectiveDirector=true,gripOnlyWrench=true,friendlyGenerator=true;
  public bool pressureCues=true,stormClock=false,onboarding=false,movement=false,worldSafety=false;
  public float speechSpacingSeconds=8,statusIntervalSeconds=120;
  public float guidancePulseSeconds=1.2f,idleTickSeconds=10,assignmentSeconds=5;
  public static MasterPassTuning Current=>Resources.Load<MasterPassTuning>("MasterPassTuning");
 }
}