using UnityEngine;
namespace LighthouseKeepers.Core {
 // Same geometric constraint on client and host; carrying retains the existing prop lease.
 public static class RadioBeltPose {
  public const float Reach=.32f;
  public static Vector3 Position(Vector3 eye,Quaternion rotation)=>ToolBeltPose.Position(ToolBeltSlot.Radio,eye,rotation);
  public static bool Near(Vector3 point,Vector3 eye,Quaternion rotation)=>ToolBeltPose.Near(ToolBeltSlot.Radio,point,eye,rotation,Reach);
 }
}
