using System;using System.Collections;using System.IO;using UnityEngine;using UnityEditor;using Unity.XR.CoreUtils;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Lobby;
public sealed class CompanionProbe:MonoBehaviour {
 string folder;WatchDemoController w;CrewCompanion bot;string stage;float began;
 public static object Run(string folder){EditorUtility.audioMasterMute=true;var g=new GameObject("Companion check");var p=g.AddComponent<CompanionProbe>();p.folder=folder;return "started";}
 void Need(bool ok,string detail){if(!ok)throw new Exception(detail);}
 IEnumerator Start(){
  Directory.CreateDirectory(folder);w=FindAnyObjectByType<WatchDemoController>();bot=w.GetComponent<CrewCompanion>();began=Time.realtimeSinceStartup;stage="spawn";yield return new WaitForSecondsRealtime(2);
  Need(bot&&bot.Active&&w.State.ScriptedPartner,"No active companion");Capture("house",bot.Position);File.AppendAllText(folder+"/events.txt","Spawned with other duty: "+bot.Duty+"\n");
  // Isolate the physical valve support. Keep the existing broken pipe; other stations restored by fixture.
  w.State.Engine.Reset(.8f,0,true);w.State.Tick(.1);w.State.SetCircuit(0,true);w.State.SetCircuit(2,false);w.State.SetCircuit(3,false);w.State.SetCircuit(1,true);w.State.ResetBreaker();w.State.IgniteBeacon(true);w.State.AlignBeacon(w.State.Beacon.Target);w.State.WipeBeacon(1);w.State.Tick(.1);
  var valve=FindAnyObjectByType<WatchPressureValve>();FindAnyObjectByType<XROrigin>().MoveCameraToWorldLocation(valve.transform.position+valve.transform.forward*1.1f+Vector3.up*.6f);
  bot.SetCommand(CompanionCommand.Help);stage="walk to valve";float until=Time.realtimeSinceStartup+55;
  while(w.State.Pressure.Holder!=WatchState.CompanionActor&&Time.realtimeSinceStartup<until)yield return new WaitForSecondsRealtime(.25f);
  Need(w.State.Pressure.Holder==WatchState.CompanionActor,"Valve failed: "+bot.Status+" at "+bot.Position);Need(Vector3.Distance(bot.Position+Vector3.up,valve.transform.position)<1.2f,"Remote valve cheat");
  w.State.PipeWorkerActor=1;Need(w.State.PipeReady,"Human cannot tighten under bot support");Capture("valve",bot.Position);File.AppendAllText(folder+"/events.txt","Reached and held actual valve. Human permitted; bot cannot wrench.\n");
  bot.SetCommand(CompanionCommand.Wait);yield return null;Need(w.State.Pressure.Holder==0&&!w.State.PipeReady,"Wait did not release valve");
  w.State.CompleteRepair();w.State.SetCircuit(0,false);w.State.SetCircuit(2,true);w.State.ResetBreaker();w.State.Radio.ToggleMain();w.State.Radio.Tune(w.State.Radio.Program.targetMHz);w.State.Tick(2);
  Need(w.State.Radio.Step==2,"Radio fixture not at code");FindAnyObjectByType<XROrigin>().MoveCameraToWorldLocation(new Vector3(1,11.5f,0));bot.SetCommand(CompanionCommand.Work);stage="fetch telegram";until=Time.realtimeSinceStartup+55;
  while(bot.LearnedCode.Length==0&&Time.realtimeSinceStartup<until)yield return new WaitForSecondsRealtime(.25f);
  Need(bot.LearnedCode==CodeSource.CodeText(w.State.Radio),"Code not learned at paper: "+bot.Status+" at "+bot.Position);Need(Vector3.Distance(bot.Position+Vector3.up,CodeSource.Active.transform.position)<1.2f,"Read a distant clue");File.AppendAllText(folder+"/events.txt","Read the actual House paper after travel + reading dwell.\n");
  stage="return upstairs";until=Time.realtimeSinceStartup+100;while(bot.Position.y<9.4f&&Time.realtimeSinceStartup<until)yield return new WaitForSecondsRealtime(.25f);
  Need(bot.Position.y>9.4f,"Failed stair return: "+bot.Status+" at "+bot.Position);Capture("f4-slate",bot.Position);File.AppendAllText(folder+"/events.txt","Returned via stairs to F4 with matching slate.\n");
  w.State.SetKeeperCount(2);yield return null;yield return null;Need(!bot.Active&&!w.State.ScriptedPartner&&w.State.Pressure.Holder!=WatchState.CompanionActor,"Bot did not retire on human join");
  w.State.SetKeeperCount(1);yield return new WaitForSecondsRealtime(4);Need(bot.Active&&w.State.ScriptedPartner,"Bot did not return after leave");
  File.WriteAllText(folder+"/result.json",JsonUtility.ToJson(new Result{passed=true,seconds=Time.realtimeSinceStartup-began,notes="Actual navigation, valve hold/release, House clue read, F4 return, simulated roster handoff. Not a human timing or live transport test."},true));Destroy(gameObject);
 }
 [Serializable]class Result{public bool passed;public float seconds;public string notes;}
 void Update(){if(w&&bot)File.WriteAllText(folder+"/current.txt",stage+" | "+bot.Status+" | "+bot.Position+" | "+bot.Command);}
 void Capture(string name,Vector3 p){var c=new GameObject("Companion camera").AddComponent<Camera>();c.CopyFrom(Camera.main);c.enabled=false;c.nearClipPlane=.03f;c.fieldOfView=60;c.transform.position=p+GameObject.Find("Owen — scripted keeper").transform.forward*1.6f+GameObject.Find("Owen — scripted keeper").transform.right*.35f+Vector3.up*1.45f;if(name=="valve"){var v=FindAnyObjectByType<WatchPressureValve>();c.transform.position=v.transform.position+v.transform.forward*1.65f+Vector3.up*.35f;}c.transform.LookAt(p+Vector3.up);var old=RenderTexture.active;var rt=RenderTexture.GetTemporary(1400,1100,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1400,1100,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1400,1100),0,0);t.Apply();File.WriteAllBytes(folder+"/"+name+".png",t.EncodeToPNG());RenderTexture.active=old;c.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Destroy(t);Destroy(c.gameObject);}
}
