using UnityEngine;
namespace LighthouseKeepers.WatchDemo {
 [CreateAssetMenu(menuName="Lighthouse/Crew companion")]
 public sealed class CrewCompanionTuning:ScriptableObject {
  public bool enabled;
  public float walkSpeed=1.8f,followDistance=1.4f,arriveDistance=.3f,readSeconds=2,reach=1.15f,repathSeconds=.5f,stuckSeconds=5;
  public Vector3 start=new Vector3(-.5f,0,-7);
  public static CrewCompanionTuning Current=>Resources.Load<CrewCompanionTuning>("CrewCompanionTuning");
 }
}
