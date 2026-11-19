using UnityEngine;
namespace LighthouseKeepers.Core {
 public enum ToolBeltSlot {Radio,Flashlight,Wrench}
 public static class ToolBeltPose {
  public const float SnapReach=.18f,HostTolerance=.24f;
  public static Vector3 Offset(ToolBeltSlot slot)=>slot==ToolBeltSlot.Radio?new Vector3(.24f,-.74f,-.03f):slot==ToolBeltSlot.Flashlight?new Vector3(-.22f,-.74f,.18f):new Vector3(-.29f,-.74f,-.15f);
  public static Quaternion Rotation(ToolBeltSlot slot,Quaternion head)=>Quaternion.Euler(0,head.eulerAngles.y,0)*(slot==ToolBeltSlot.Wrench?Quaternion.Euler(-90,0,0):Quaternion.identity);
  public static Vector3 Position(ToolBeltSlot slot,Vector3 eye,Quaternion rotation)=>eye+Quaternion.Euler(0,rotation.eulerAngles.y,0)*Offset(slot);
  public static bool Near(ToolBeltSlot slot,Vector3 point,Vector3 eye,Quaternion rotation,float reach=SnapReach){float norm=Quaternion.Dot(rotation,rotation);return ValidSlot(slot)&&Finite(point)&&Finite(eye)&&float.IsFinite(norm)&&norm>.8f&&norm<1.2f&&float.IsFinite(reach)&&reach>0&&Vector3.Distance(point,Position(slot,eye,rotation))<=reach;}
  public static bool ValidSlot(ToolBeltSlot slot)=>slot>=ToolBeltSlot.Radio&&slot<=ToolBeltSlot.Wrench;
  static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
 }
}
