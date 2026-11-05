#if UNITY_EDITOR
using System.Collections;using System.Linq;using NUnit.Framework;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.TestTools;
namespace LighthouseKeepers.Presentation.Environment.Tests {
 public sealed class MasterSafetyTests {
  [UnityTest] public IEnumerator GalleryIsContinuousAndHouseClutterCannotStepThePlayerUp(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));for(int n=0;n<12;n++)yield return null;
   var guard=GameObject.Find("Continuous gallery perimeter guard");Assert.That(guard,Is.Not.Null);
   // At half-segment increments, a head-height capsule moving outwards must meet the guard.
   for(int n=0;n<128;n++){float a=n*Mathf.PI*2/128;var direction=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));var from=direction*3.85f+Vector3.up*9.9f;
    var hits=Physics.CapsuleCastAll(from+Vector3.up*.30f,from+Vector3.up*1.5f,.20f,direction,.65f,~0,QueryTriggerInteraction.Ignore);
    Assert.That(hits.Any(h=>h.collider.transform.IsChildOf(guard.transform)),Is.True,"Gallery gap at angle "+a);
   }
   foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)){var b=c.bounds;if(c.name.StartsWith("UCX_SM_Rock_Outcrop")&&b.center.z< -5&&Mathf.Abs(b.center.x)<3.5f&&b.max.y>0&&b.max.y<.2f)Assert.False(c.enabled,"House rock remains blocking: "+c.name);}
   var bed=GameObject.Find("Third-floor bed frame");Assert.That(bed,Is.Not.Null);Assert.Greater(bed.transform.position.x,2.5f,"Bunk must clear F3 transfer route");
   var overlap=Physics.OverlapCapsule(new Vector3(1.4f,6.9f,-.4f),new Vector3(1.4f,8.1f,-.4f),.25f,~0,QueryTriggerInteraction.Ignore);Assert.False(overlap.Any(c=>c.name.Contains("bed frame")||c.name.Contains("Mattress")),"Bed intersects F3 route");
  }
 }
}
#endif
