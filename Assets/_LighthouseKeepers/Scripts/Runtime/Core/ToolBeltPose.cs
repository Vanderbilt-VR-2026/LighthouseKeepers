using UnityEngine;
namespace LighthouseKeepers.Core {
 public enum ToolBeltSlot {Radio,Flashlight,Wrench}
 public static class ToolBeltPose {
  public const float SnapReach=.18f;
  // Local body presentation supplies its animated hip without a gameplay-to-network dependency.
  public static System.Func<Transform> TrackedHips;
  // Positions are relative to the animated Hips joint, not the HMD. The fallback
  // uses the avatar's reference eye/hip spacing and its .18 m body offset.
  public static Vector3 HipOffset(ToolBeltSlot slot)=>slot==ToolBeltSlot.Radio?new Vector3(.24f,.09f,.02f):new Vector3(-.24f,.09f,-.035f);
  public static Vector3 Offset(ToolBeltSlot slot)=>HipOffset(slot)+new Vector3(0,-.725f,-.18f);
  public static Quaternion Rotation(ToolBeltSlot slot,Quaternion head)=>Quaternion.Euler(0,head.eulerAngles.y,0)*(slot==ToolBeltSlot.Wrench?Quaternion.Euler(-90,0,0):Quaternion.identity);
  public static Vector3 Position(ToolBeltSlot slot,Vector3 eye,Quaternion rotation)=>eye+Quaternion.Euler(0,rotation.eulerAngles.y,0)*Offset(slot);
  public static Vector3 Position(ToolBeltSlot slot,Transform hips)=>hips.TransformPoint(HipOffset(slot));
  public static bool Near(ToolBeltSlot slot,Vector3 point,Vector3 eye,Quaternion rotation,float reach=SnapReach){float norm=Quaternion.Dot(rotation,rotation);return ValidSlot(slot)&&Finite(point)&&Finite(eye)&&float.IsFinite(norm)&&norm>.8f&&norm<1.2f&&float.IsFinite(reach)&&reach>0&&Vector3.Distance(point,Position(slot,eye,rotation))<=reach;}
  // The host receives head pose, not the wearer's smoothed torso yaw/stature.
  // Bound the carried object to the waist envelope without rejecting fast turns.
  public static bool NearHost(ToolBeltSlot slot,Vector3 point,Vector3 eye,Quaternion rotation){
   float norm=Quaternion.Dot(rotation,rotation);if(!ValidSlot(slot)||!Finite(point)||!Finite(eye)||!float.IsFinite(norm)||norm<.8f||norm>1.2f)return false;
   var d=point-eye;float drop=-d.y;return drop>=.30f&&drop<=1.10f&&d.x*d.x+d.z*d.z<=.60f*.60f;
  }
  public static bool ValidSlot(ToolBeltSlot slot)=>slot==ToolBeltSlot.Radio||slot==ToolBeltSlot.Wrench;
  static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
 }
}
