using System;using System.Collections;using System.IO;using UnityEngine;using UnityEngine.AI;using UnityEditor;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.WatchDemo;
// Run through the Editor bridge in Night 3 with the tutorial door open. No gameplay state changes.
public sealed class CrewRecoveryRouteProbe:MonoBehaviour
{
 string output;HunterNavigation navigation;NavMeshAgent agent;float seconds,distance;
 public static object Run(string output){EditorUtility.audioMasterMute=true;var g=new GameObject("Recovery route probe");var p=g.AddComponent<CrewRecoveryRouteProbe>();p.output=output;return "Measuring actual F4 to F1 navigation; not a rescue implementation.";}
 IEnumerator Start(){
  var t=HorrorTuning.Current;navigation=new HunterNavigation(t,Resources.Load<NavMeshData>("HunterNavigation"));
  if(!navigation.SampleFloor(new Vector3(1,9.9f,0),out var start)||!navigation.SampleFloor(new Vector3(2.6f,0,0),out var goal)){Finish(false,"Missing floor point");yield break;}
  transform.position=start;agent=gameObject.AddComponent<NavMeshAgent>();agent.radius=t.radius;agent.height=t.navigationHeight;agent.speed=CrewCompanionTuning.Current.walkSpeed;agent.acceleration=6;agent.stoppingDistance=.15f;
  if(!agent.SetDestination(goal)){Finish(false,"No route");yield break;}
  float began=Time.realtimeSinceStartup,still=0;Vector3 prior=transform.position;
  while(agent.pathPending||Vector3.Distance(transform.position,goal)>.25f){
   yield return null;var pos=transform.position;float moved=Vector3.Distance(prior,pos);distance+=moved;seconds=Time.realtimeSinceStartup-began;
   if(!agent.pathPending&&(!navigation.SegmentClear(prior,pos)||agent.pathStatus!=NavMeshPathStatus.PathComplete)){Finish(false,"Blocked path at "+pos);yield break;}
   if(moved>agent.speed*Time.deltaTime+.08f){Finish(false,"Unexpected displacement");yield break;}
   still=moved<.0001f?still+Time.deltaTime:0;if(still>3||seconds>60){Finish(false,"Stuck/timeout at "+pos);yield break;}prior=pos;
  }
  Finish(true,"Actual agent traversal at helper speed, collision-checked each frame. Three-second hand hold is proposed, not implemented. Worn human escape/rescue remains unverified.");
 }
 void Finish(bool passed,string detail){Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"route.json"),JsonUtility.ToJson(new Report{passed=passed,detail=detail,seconds=seconds,metres=distance,speed=CrewCompanionTuning.Current.walkSpeed,rescueBudget=HorrorTuning.Current.reviveSeconds,proposedGripSeconds=3,spare=HorrorTuning.Current.reviveSeconds-seconds-3},true));Destroy(gameObject);}
 [Serializable]class Report{public bool passed;public string detail;public float seconds,metres,speed,rescueBudget,proposedGripSeconds,spare;}
 void OnDestroy(){navigation?.Dispose();}
}
