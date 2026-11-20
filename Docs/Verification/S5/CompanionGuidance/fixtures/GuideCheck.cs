using System;using System.Collections;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using UnityEditor;using TMPro;using LighthouseKeepers.WatchDemo;
public class GuideCheck:MonoBehaviour {
 string folder;int failures;public static object Begin(string p){EditorUtility.audioMasterMute=true;var c=new GameObject("Companion objective probe").AddComponent<GuideCheck>();c.folder=p;return true;}
 void Check(bool ok,string msg){File.AppendAllText(folder+"/checks.txt",(ok?"PASS ":"FAIL ")+msg+"\n");if(!ok)failures++;}
 IEnumerator Start(){
  Directory.CreateDirectory(folder);var w=FindAnyObjectByType<WatchDemoController>();var buddy=w.GetComponent<CrewCompanion>();
  for(int i=0;i<200&&!buddy.Active;i++)yield return null;
  if(!buddy.Active){Check(false,"Owen spawned");Finish();yield break;}
  var kit=w.GetComponent<ArrivalEquipment>();typeof(ArrivalEquipment).GetField("radioTaken",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(kit,true);
  w.enabled=false;buddy.enabled=false;var agent=buddy.ActorTransform.GetComponent<UnityEngine.AI.NavMeshAgent>();if(agent)agent.enabled=false;
  var hud=w.GetComponent<ObjectiveHud>();var director=w.State.Director;var first=typeof(ObjectiveDirector).GetProperty("First");first.SetValue(director,KeeperStep.HelpPartner);hud.RecallNow();yield return null;
  var floor=FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None).Single(t=>t.name=="Relative floor");
  foreach(var sample in new[]{(new Vector3(-2,0,-6),"HOUSE"),(new Vector3(0,0,0),"FLOOR 1"),(new Vector3(0,3.3f,0),"FLOOR 2"),(new Vector3(0,6.6f,0),"FLOOR 3"),(new Vector3(0,9.9f,0),"FLOOR 4")}){
   buddy.ActorTransform.position=sample.Item1;yield return null;yield return null;
   Check(floor.text.Contains("Owen")&&floor.text.Contains(sample.Item2),"Owen guidance " + sample.Item2 + ": " + floor.text);
   Check(w.GetComponent<DirectorPresentation>().Target(KeeperStep.HelpPartner)==buddy.ActorTransform,"target follows Owen on "+sample.Item2);
  }
  first.SetValue(director,KeeperStep.ReadyShip);hud.RecallNow();yield return null;yield return null;Check(floor.text=="Your walkie","portable walkie has no fake floor: "+floor.text);
  Finish();
 }
 void Finish(){File.WriteAllText(folder+"/result.json","{\"passed\":"+(failures==0?"true":"false")+",\"failures\":"+failures+"}");Destroy(gameObject);}
}
