using UnityEngine;
namespace LighthouseKeepers.Core {
 [CreateAssetMenu(menuName="Lighthouse/Master pass tuning")]
 public sealed class MasterPassTuning:ScriptableObject {
  public bool objectiveDirector=true,gripOnlyWrench=true,friendlyGenerator=true;
  public bool pressureCues=true,stormClock=true,onboarding=true,movement=true,worldSafety=true;
  public float walkMultiplier=1.2f,poleSpeed=2,poleAcceleration=4;
  public float guidedFuelTarget=.65f;
  public float nightTwoOutageSeconds=75,firstGeneratorGraceSeconds=120,tutorialFloodCap=.28f,gateTimeoutSeconds=30;
  [Header("Integrated watch grace — seconds; authoritative for solo and crew")]
  public bool watchStartGate=true,firstStationGrace=true;
  public float watchStartGraceSeconds=45,firstStationGraceSeconds=60;
  public float day3SoloBeaconGraceSeconds=90,day3CrewBeaconGraceSeconds=60;
  public float stormRecoverySeconds=20;
  public float speechSpacingSeconds=8,statusIntervalSeconds=120;
  public float guidancePulseSeconds=1.2f,idleTickSeconds=10,assignmentSeconds=5;
  public static MasterPassTuning Current=>Resources.Load<MasterPassTuning>("MasterPassTuning");
 }
}