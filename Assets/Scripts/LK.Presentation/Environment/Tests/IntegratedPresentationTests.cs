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
  [UnityTest]public IEnumerator TrackerIsWorldFixedAndRecallRepositionsIt(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(2);
   var tracker=GameObject.Find("Transient objective tracker").transform;var hud=UnityEngine.Object.FindAnyObjectByType(T("ObjectiveHud"));var head=Camera.main.transform;T("ObjectiveHud").GetMethod("Reveal",Type.EmptyTypes).Invoke(hud,null);var pose=tracker.position;var rotation=tracker.rotation;
   head.rotation=Quaternion.AngleAxis(80,Vector3.up)*head.rotation;yield return null;yield return null;
   Assert.That(Vector3.Distance(pose,tracker.position),Is.LessThan(.001f));Assert.That(Quaternion.Angle(rotation,tracker.rotation),Is.LessThan(.1f));
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
