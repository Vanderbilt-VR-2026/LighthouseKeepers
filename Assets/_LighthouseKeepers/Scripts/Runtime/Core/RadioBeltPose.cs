using UnityEngine;
namespace LighthouseKeepers.Core {
 // Same geometric constraint on client and host; carrying retains the existing prop lease.
 public static class RadioBeltPose {
  public const float Reach=.32f;
  public static Vector3 Position(Vector3 eye,Quaternion rotation){var yaw=Quaternion.Euler(0,rotation.eulerAngles.y,0);return eye+Vector3.down*.74f+yaw*new Vector3(.24f,0,-.03f);}
  public static bool Near(Vector3 point,Vector3 eye,Quaternion rotation){float norm=Quaternion.Dot(rotation,rotation);return Finite(point)&&Finite(eye)&&!float.IsNaN(norm)&&norm>.8f&&norm<1.2f&&Vector3.Distance(point,Position(eye,rotation))<=Reach;}
  static bool Finite(Vector3 v)=>!float.IsNaN(v.x)&&!float.IsNaN(v.y)&&!float.IsNaN(v.z)&&!float.IsInfinity(v.x)&&!float.IsInfinity(v.y)&&!float.IsInfinity(v.z);
 }
}
