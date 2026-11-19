using System;using System.IO;using System.Collections;using UnityEngine;using UnityEditor;using Unity.XR.CoreUtils;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;
public class NightmareLifecycle:MonoBehaviour{
 string folder;int failures;WatchDemoController w;
 public static object Begin(string folder){EditorUtility.audioMasterMute=true;var t=new GameObject("Nightmare sleep lifecycle fixture").AddComponent<NightmareLifecycle>();t.folder=folder;Directory.CreateDirectory(folder);t.StartCoroutine(t.Run());return true;}
 void Check(bool yes,string message){File.AppendAllText(folder+"/checks.txt",(yes?"PASS ":"FAIL ")+message+"\n");if(!yes)failures++;}
 IEnumerator Run(){
 w=FindAnyObjectByType<WatchDemoController>();var s=w.State;HorrorTuning.Current.nightmareEnabled=true;
 w.enabled=false;s.SetScriptedPartner(false);s.EngageWatch();for(int n=0;n<70;n++)s.PourFuel(.1f);
 for(int n=0;n<3;n++){for(int i=0;i<100&&!s.StarterInWindow;i++)s.Tick(.05);s.PullStarterAs(1);s.Tick(.4);}
 s.CompleteRepair();s.SetCircuit(2,true);s.Radio.ToggleMain();s.Radio.Tune(s.Radio.TargetMHz);s.Tick(3);
 for(int n=0;n<s.Radio.CodeLength;n++)s.Radio.PressCode(s.Radio.CodeSymbol(n));s.Radio.SetMicrophone(true,true);s.Tick(5);s.IgniteBeacon(true);
 for(int i=0;i<5000&&!s.Complete&&!s.Failed;i++){s.AlignBeacon(s.ShipWheelTarget);s.Tick(.1);}
 File.WriteAllText(folder+"/seed.json",JsonUtility.ToJson(s.Capture(0),true));Check(s.Complete&&!s.Failed,"Seed completed Day 2 through station and ship rules: "+s.FailureMessage);if(!s.Complete||s.Failed){w.enabled=true;yield break;}w.enabled=true;
 var rig=FindAnyObjectByType<XROrigin>();foreach(var b in rig.GetComponentsInChildren<Behaviour>(true))if(b.GetType().Name.Contains("TrackedPose")||b.GetType().Name.Contains("InputModality"))b.enabled=false;
 var cc=rig.GetComponent<CharacterController>();if(cc)cc.enabled=false;
 rig.MoveCameraToWorldLocation(w.BedPosition);Physics.SyncTransforms();yield return null;
 double doneTime=s.Elapsed;Check(w.TrySleep(),"Actual bed action accepted after completed Day 2");yield return new WaitForSeconds(.3f);
 Check(s.Day==2&&s.Scare.Beat==ScareBeat.Wake,"Bed intercepts morning and begins nightmare");Check(!s.Complete,"Normal sleep cannot run through the active scene");
 var nscene=w.GetComponent<NightmareScene>();nscene.enabled=false;
 for(int i=0;i<200&&s.Scare.Beat!=ScareBeat.Escape;i++)s.Scare.Tick(.1f,true,true,0,0);
 Check(s.Scare.Catch((int)w.CurrentDuty,Vector3.zero),"Solo fixture catch accepted");Check(s.Failed,"Solo scene catch loses the nightmare");
 w.RetryDay();yield return new WaitForSeconds(.2f);nscene.enabled=true;yield return new WaitForSeconds(.2f);
 Check(s.Scare.Beat==ScareBeat.Wake&&!s.Failed,"Retry returns to wake-up");Check(s.Day==2&&s.Repaired&&Math.Abs(s.Elapsed-doneTime)<.01,"Retry preserves completed watch and watch time");Check(s.Scare.CanSkip,"Replay skip is available after failure");
 nscene.Skip();yield return new WaitForSeconds(7);
 Check(s.Day==3&&!s.Failed,"Replay completion advances through ordinary shared sleep into Day 3");Check(s.Scare.Beat==ScareBeat.Idle,"Next day clears nightmare frame");
 File.WriteAllText(folder+"/result.json","{\"passed\":"+(failures==0?"true":"false")+",\"failures\":"+failures+"}");Destroy(gameObject);
 }
}
