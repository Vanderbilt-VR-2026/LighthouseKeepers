using System;using System.Collections;using System.Reflection;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;
namespace LighthouseKeepers.Net.Tests {
 public sealed class FlareSweepPlayTests {
  GameObject target,wall;Type hitType,sweepType;readonly Vector3 spot=new(180,5,180);
  [UnitySetUp]public IEnumerator Setup(){
#if UNITY_EDITOR
   UnityEditor.EditorUtility.audioMasterMute=true;
#endif
   hitType=Type.GetType("LighthouseKeepers.WatchDemo.FlareImpactTarget, Assembly-CSharp",true);sweepType=Type.GetType("LighthouseKeepers.WatchDemo.FlareCollision, Assembly-CSharp",true);target=new GameObject("Moving flare target");target.transform.position=spot;target.AddComponent(hitType);yield return null;
  }
  bool Sweep(Vector3 a,Vector3 b,out Component hit,out Vector3 point){object[] args={a,b,.06f,0f,1f,Vector3.zero,null};bool found=(bool)sweepType.GetMethod("Sweep").Invoke(null,args);point=(Vector3)args[5];hit=(Component)args[6];return found;}
  [UnityTest]public IEnumerator StillAndMovingTargetsAtTwoFiveTenTwentyMetresFromEitherHand(){
   var c=target.GetComponent(hitType);int checks=0;
   foreach(float distance in new[]{2f,5f,10f,20f})foreach(float hand in new[]{-.16f,.16f})foreach(bool moving in new[]{false,true}){
    var origin=spot+Vector3.right*hand;var end=origin+Vector3.forward*(distance+1);
    float cross=distance/(distance+1);target.transform.position=origin+Vector3.forward*distance+(moving?Vector3.left*(2*cross):Vector3.zero);hitType.GetMethod("ResetMotion").Invoke(c,null);
    if(moving)target.transform.position+=Vector3.right*2;Physics.SyncTransforms();
    Assert.True(Sweep(origin,end,out var hit,out var point),$"{distance} m moving={moving} hand={hand}");Assert.AreEqual(c,hit);Assert.Less(point.z,end.z);checks++;yield return null;
   }Assert.AreEqual(16,checks);
  }
  [UnityTest]public IEnumerator ThinWallStopsBeforeTargetAndMissDoesNotInventAHit(){
   target.transform.position=spot+Vector3.forward*5;hitType.GetMethod("ResetMotion").Invoke(target.GetComponent(hitType),null);
   wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=spot+Vector3.forward*2;wall.transform.localScale=new Vector3(2,2,.01f);Physics.SyncTransforms();
   Assert.True(Sweep(spot,spot+Vector3.forward*8,out var hit,out var point));Assert.IsNull(hit);Assert.Less(point.z,spot.z+2.1f);
   Assert.False(Sweep(spot+Vector3.right*5,spot+Vector3.right*5+Vector3.forward*8,out hit,out point));yield return null;
  }
  [UnityTearDown]public IEnumerator TearDown(){UnityEngine.Object.Destroy(target);if(wall)UnityEngine.Object.Destroy(wall);yield return null;}
 }
}
