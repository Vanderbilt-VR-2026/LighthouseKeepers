using System;using System.Collections;using System.Collections.Generic;using System.IO;using UnityEngine;using UnityEngine.AI;using UnityEditor;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.WatchDemo;
public sealed class HunterRouteWalk:MonoBehaviour
{
 string output;HunterNavigation navigation;NavMeshAgent agent;readonly List<string> rows=new();
 public static object Run(string output){EditorUtility.audioMasterMute=true;var g=new GameObject("Hunter route test agent");var t=g.AddComponent<HunterRouteWalk>();t.output=output;return "started actual NavMeshAgent traversal";}
 IEnumerator Start()
 {
  var tuning=HorrorTuning.Current;navigation=new HunterNavigation(tuning,Resources.Load<NavMeshData>("HunterNavigation"));
  var points=new[]{new Vector3(-.5f,0,-7),new Vector3(2.6f,0,0),new Vector3(-1,3.3f,-1),new Vector3(1,6.6f,0),new Vector3(1,9.9f,0),new Vector3(-.5f,0,-7)};
  if(!navigation.SampleFloor(points[0],out var first)){Finish(false,"House missing");yield break;}transform.position=first;
  agent=gameObject.AddComponent<NavMeshAgent>();agent.radius=tuning.radius;agent.height=tuning.navigationHeight;agent.speed=tuning.walkSpeed;agent.acceleration=8;agent.stoppingDistance=.05f;
  for(int i=1;i<points.Length;i++){
   if(!navigation.SampleFloor(points[i],out var goal)||!agent.SetDestination(goal)){Finish(false,"No destination "+i);yield break;}
   float started=Time.realtimeSinceStartup;var prior=transform.position;float distance=0,still=0;
   while(agent.pathPending||Vector3.Distance(transform.position,goal)>.15f){
    yield return null;var p=transform.position;float moved=Vector3.Distance(prior,p);distance+=moved;
    if(!agent.pathPending&&!navigation.SegmentClear(prior,p)){Finish(false,$"Collision {i}: {prior}->{p}");yield break;}
    if(!agent.pathPending&&agent.pathStatus!=NavMeshPathStatus.PathComplete){Finish(false,"Incomplete route "+i);yield break;}
    if(moved>tuning.walkSpeed*Time.deltaTime+.08f){Finish(false,"Unexpected displacement "+moved);yield break;}
    still=moved<.0001f?still+Time.deltaTime:0;if(still>tuning.stuckSeconds+1||Time.realtimeSinceStartup-started>90){Finish(false,"Stuck "+i+" at "+p);yield break;}
    prior=p;
   }
   rows.Add($"stop={i} goal={goal} actual={transform.position} walked={distance:F2}m seconds={Time.realtimeSinceStartup-started:F2}");
  }Finish(true,"House→F1→F2→F3→F4→House actual agent, 1.2m/s, no blocked/teleport/stuck steps");
 }
 void Finish(bool pass,string detail){Directory.CreateDirectory(output);File.WriteAllText(output+"/walk.json",JsonUtility.ToJson(new Report{passed=pass,detail=detail,rows=rows.ToArray()},true));Destroy(gameObject);}
 [Serializable]class Report{public bool passed;public string detail;public string[] rows;}
 void OnDestroy(){navigation?.Dispose();}
}
