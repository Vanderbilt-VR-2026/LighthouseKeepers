using NUnit.Framework;using UnityEngine;using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests {
 public sealed class RadioBeltTests {
  [Test]public void BeltStaysBelowEyesAndIgnoresLookingUpOrDown(){var eye=new Vector3(2,1.7f,3);var a=RadioBeltPose.Position(eye,Quaternion.identity);Assert.AreEqual(1.065f,a.y,.001f);Assert.Less(Vector3.Distance(a,RadioBeltPose.Position(eye,Quaternion.Euler(65,0,0))),.001f);Assert.True(RadioBeltPose.Near(a,eye,Quaternion.identity));}
  [Test]public void CrouchAndTurningFollowTheWearer(){var e=new Vector3(0,1,0);var a=RadioBeltPose.Position(e,Quaternion.Euler(0,90,0));Assert.AreEqual(.365f,a.y,.001f);Assert.AreEqual(-.24f,a.z,.001f);}
  [Test]public void InvalidOrRemoteCarryCannotRenewLease(){var e=Vector3.up*1.7f;Assert.False(RadioBeltPose.Near(Vector3.right*30,e,Quaternion.identity));Assert.False(RadioBeltPose.Near(new Vector3(float.NaN,0,0),e,Quaternion.identity));Assert.False(RadioBeltPose.Near(e,e,new Quaternion(0,0,0,0)));}
 }
}
