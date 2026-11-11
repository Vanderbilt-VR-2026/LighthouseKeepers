#if UNITY_EDITOR
using System;using System.Collections;using System.Linq;using NUnit.Framework;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.TestTools;
namespace LighthouseKeepers.Presentation.Environment.Tests {
 public sealed class IntegratedPresentationTests {
  static Type T(string name)=>Type.GetType("LighthouseKeepers.WatchDemo."+name+", Assembly-CSharp",true);
  [Test]public void GalleryRainIsExteriorInEveryDirectionWhileLanternAndHouseKeepTheirBeds(){
   Vector3 W(Vector3 p)=>(Vector3)T("RainSoundscape").GetMethod("Weights").Invoke(null,new object[]{p});
   for(int i=0;i<16;i++){var d=Quaternion.Euler(0,i*22.5f,0)*Vector3.forward;var gallery=W(d*3.6f+Vector3.up*11.5f);Assert.That(gallery.x,Is.GreaterThan(.99f),"Gallery angle "+i);var roof=W(d*2.1f+Vector3.up*11.5f);Assert.That(roof.z,Is.GreaterThan(.99f));}
   Assert.That(W(new Vector3(0,1.7f,-7.3f)).y,Is.GreaterThan(.99f));Assert.That(W(new Vector3(6,1.7f,0)).x,Is.GreaterThan(.99f));
   for(float r=2;r<4.3f;r+=.05f){var w=W(new Vector3(r,11.5f,0));Assert.That(w.x+w.y+w.z,Is.EqualTo(1).Within(.001f));Assert.That(Mathf.Min(w.x,w.y,w.z),Is.GreaterThanOrEqualTo(0));}
  }
  [UnityTest]public IEnumerator SeaRecoveryFadeNeverEntersSharedSleepSnapshot(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(2);
   var w=UnityEngine.Object.FindAnyObjectByType(T("WatchDemoController"));T("WatchDemoController").GetMethod("ShowSeaRecovery").Invoke(w,new object[]{1f,true});
   var json=(string)T("WatchDemoController").GetMethod("CaptureState").Invoke(w,null);var snapshot=JsonUtility.FromJson(json,T("WatchSnapshot"));Assert.Zero((float)T("WatchSnapshot").GetField("fade").GetValue(snapshot),"Only the fallen keeper should fade");Assert.False((bool)T("WatchSnapshot").GetField("sleeping").GetValue(snapshot));
   T("WatchDemoController").GetMethod("HideSeaRecovery").Invoke(w,null);
  }
  [UnityTest]public IEnumerator IslandRailBlocksEveryOutwardEdgeAndSeaRetryReturnsHome(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   var safety=T("IslandSafety");var edge=safety.GetMethod("Edge");
   for(int i=0;i<160;i++){var p=(Vector3)edge.Invoke(null,new object[]{i*Mathf.PI*2/160});var outward=new Vector3(p.x+.2f,0,p.z+3.2f).normalized;
    Assert.True(Physics.SphereCast(p-outward*.6f+Vector3.up*.8f,.2f,outward,out var hit,1.2f),"Unblocked edge "+i);Assert.IsTrue(hit.collider.name.StartsWith("Island edge rail"),hit.collider.name);
   }
   var rig=UnityEngine.Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();rig.transform.position=Vector3.down*4;yield return null;yield return null;
   var watch=UnityEngine.Object.FindAnyObjectByType(T("WatchDemoController"));var state=T("WatchDemoController").GetProperty("State").GetValue(watch);
   Assert.True((bool)T("WatchState").GetProperty("Failed").GetValue(state));Assert.AreEqual("Swept out to sea",T("WatchState").GetProperty("FailureMessage").GetValue(state));
   Assert.True((bool)T("WatchDemoController").GetMethod("RetryDay").Invoke(watch,null));yield return null;
   Assert.False((bool)T("WatchState").GetProperty("Failed").GetValue(state));Assert.Greater(rig.transform.position.y,-.1f);Assert.That(rig.Camera.transform.position.z,Is.EqualTo(-6.3f).Within(.15f));
  }
  [UnityTest]public IEnumerator GeneratorControlsMountInServiceOrderWithCanOnFloor(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   var root=GameObject.Find("S4 generator sequence").transform;
   var points=new[]{root.Find("FUEL_FILLER"),root.Find("GRIP_PrimerBulb"),root.Find("GRIP_ChokeControl"),GameObject.Find("GRIP_StarterHandle").transform};
   float prior=10;
   foreach(var p in points){var local=root.InverseTransformPoint(p.position);Assert.Less(local.x,prior,"Left-to-right service order");prior=local.x;Assert.That(local.y,Is.InRange(1.1f,1.3f),p.name);Assert.That(local.z,Is.InRange(1f,1.2f),p.name);}
   Assert.NotNull(root.Find("Generator service fascia"));Assert.IsNull(root.Find("Fuel can service shelf"));
   var can=root.Find("FUEL_CAN");Assert.That(can.localPosition.y,Is.EqualTo(.20f).Within(.01f));Assert.Greater(can.localPosition.x,.9f);
   Assert.That(root.Find("Engraved starting order").GetComponent<TextMesh>().text,Does.Contain("1 FUEL").And.Contain("5 OPEN"));
  }
  [UnityTest]public IEnumerator GeneratorAndPowerPanelHaveDistinctAccessibleStations(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   var generator=GameObject.Find("STN_GENERATOR").transform;var panel=GameObject.Find("S4 breaker panel").transform;
   Assert.Greater(Vector3.Distance(generator.position,panel.position),4f);
   Assert.AreEqual("POWER PANEL",panel.Find("Panel heading").GetComponent<TextMesh>().text);
   var cabinet=panel.Find("Enamel breaker cabinet").GetComponent<Renderer>();var c=cabinet.sharedMaterial.GetColor("_BaseColor");Assert.Greater(c.g,c.r*1.5f);
   var yellow=generator.GetComponentsInChildren<Renderer>().First(r=>r.name=="Diesel crankcase").sharedMaterial.GetColor("_BaseColor");Assert.Greater(yellow.r,.5f);Assert.Less(yellow.b,.15f);
   var back=panel.TransformPoint(new Vector3(0,1.28f,.01f));Assert.True(Physics.Raycast(back,-panel.forward,out var wall,.15f));Assert.True(wall.collider.name.Contains("Wall_F2"),wall.collider.name);
   Assert.NotNull(GameObject.Find("Generator to POWER PANEL conduit"));
  }
  [UnityTest]public IEnumerator FixturesClearStairRailsAndHouseClutterIsRemoved(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   var renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.enabled).ToArray();
   var rails=renderers.Where(r=>r.name.StartsWith("SM_Rail_")).ToArray();
   var fixtures=renderers.Where(r=>r.name=="Brass horn bell"||r.name=="Frosted service lamp"||r.name=="Caged practical bulb"||r.name.StartsWith("SM_Lamp_")||r.name.StartsWith("SM_EmergencyLamp_")).ToArray();
   Assert.Greater(fixtures.Length,20);Assert.AreEqual(6,fixtures.Count(r=>r.name=="Brass horn bell"));
   foreach(var fixture in fixtures)foreach(var rail in rails){var bounds=rail.bounds;bounds.Expand(.04f);Assert.False(bounds.Intersects(fixture.bounds),fixture.name+" intersects "+rail.name);}
   foreach(var name in new[]{"SM_Rock_Outcrop_7","SM_Rock_Outcrop_8","SM_Rock_Outcrop_9"})Assert.False(GameObject.Find(name).GetComponent<Renderer>().enabled,name);
   foreach(var name in new[]{"Oilskin on wall hook","Wall stowed supply crate","Coiled rope on supply crate","Dropped unlit keeper lantern","Old stove and kettle"})Assert.IsNull(GameObject.Find(name),name);
   var board=GameObject.Find("Watch checklist").GetComponent<TextMesh>();Assert.Less(board.color.grayscale,.05f);
   var block=new MaterialPropertyBlock();GameObject.Find("Wall-mounted checklist white face").GetComponent<Renderer>().GetPropertyBlock(block);Assert.Greater(block.GetColor("_BaseColor").grayscale,.9f);
   foreach(var shaft in renderers.Where(r=>r.name=="Brass slide shaft"))Assert.Less(shaft.sharedMaterial.GetColor("_EmissionColor").maxColorComponent,.001f);
  }
  [UnityTest]public IEnumerator OnlyCurrentDescentShaftAndThreeLandingsExist(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   Assert.AreEqual(1,all.Count(t=>t.name=="Continuous brass descent pole F4 to F1"));
   Assert.AreEqual(1,all.Count(t=>t.name=="Continuous brass slide shaft"));
   Assert.AreEqual(3,all.Count(t=>t.name.StartsWith("Circular pole landing F")));
   Assert.AreEqual(1,all.Count(t=>t.name=="Bedroom flush pole hatch"));
   Assert.AreEqual(1,all.Count(t=>t.name=="Bedroom hatch fall protection"));
   foreach(var t in all)Assert.AreNotEqual("Brass slide shaft",t.name,"Obsolete separate-floor shaft remains");
   foreach(var r in all.Where(t=>t.name=="Circular floor opening brass rim").Select(t=>t.GetComponent<Renderer>())){
    Assert.Less(r.sharedMaterial.GetColor("_EmissionColor").maxColorComponent,.001f,"Floor trim must not glow yellow");
    Assert.Less(r.sharedMaterial.GetColor("_BaseColor").maxColorComponent,.4f,"Floor trim must stay subdued");
   }
  }
  [UnityTest]public IEnumerator TrackerFollowsLazilyBelowEyesAndRecallRepositionsIt(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(2);
   var tracker=GameObject.Find("Transient objective tracker").transform;var hud=UnityEngine.Object.FindAnyObjectByType(T("ObjectiveHud"));var head=Camera.main.transform;T("ObjectiveHud").GetMethod("Reveal",Type.EmptyTypes).Invoke(hud,null);var pose=tracker.position;var rotation=tracker.rotation;
   foreach(var b in UnityEngine.Object.FindObjectsByType<Behaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(b.GetType().Name=="DesktopPreview"||b.GetType().Name=="TrackedPoseDriver")b.enabled=false;
   Assert.That(head.InverseTransformPoint(pose).y,Is.LessThan(-.1f));
   head.rotation=Quaternion.AngleAxis(80,Vector3.up)*head.rotation;
   Assert.That(Vector3.Distance(pose,tracker.position),Is.LessThan(.001f),"Head movement immediately snaps the tracker");
   yield return new WaitForSecondsRealtime(.25f);
   Assert.That(Vector3.Distance(pose,tracker.position),Is.GreaterThan(.05f));Assert.That(Quaternion.Angle(rotation,tracker.rotation),Is.GreaterThan(1));
   T("ObjectiveHud").GetMethod("Reveal",Type.EmptyTypes).Invoke(hud,null);Assert.That(Vector3.Distance(pose,tracker.position),Is.GreaterThan(.5f));
  }
  [UnityTest]public IEnumerator OnlyOneExteriorAndTwoBenchRadiosAndNoSupportFootStep(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(2);
   Assert.That(UnityEngine.Object.FindObjectsByType(T("StormSeascape"),FindObjectsSortMode.None).Length,Is.EqualTo(1));
   var radios=UnityEngine.Object.FindObjectsByType(T("HandheldCrewRadio"),FindObjectsSortMode.None).Cast<Component>().ToArray();Assert.That(radios.Length,Is.EqualTo(2));
   var bench=GameObject.Find("Wrench table top").GetComponent<Renderer>().bounds;
   foreach(var radio in radios){Assert.That(radio.transform.position.y,Is.EqualTo(bench.max.y+.10f).Within(.02f));Assert.That(radio.transform.position.x,Is.InRange(bench.min.x,bench.max.x));Assert.That(radio.transform.position.z,Is.InRange(bench.min.z,bench.max.z));}
   foreach(var collider in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name=="Support foot"))Assert.False(collider.enabled&&collider.gameObject.activeInHierarchy,"8cm foot must not bump the player");
   Assert.That(RenderSettings.skybox.shader.name,Is.EqualTo("LighthouseKeepers/Eli Storm Sky (opt in)"));
  }
 }
}
#endif
