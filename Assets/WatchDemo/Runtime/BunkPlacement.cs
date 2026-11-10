using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
 // One authored pose shared by the builder and restored-shell runtime adaptation.
 public static class BunkPlacement
 {
  public static Vector3 FramePosition=>new Vector3(2.95f,6.9f,0f);
  public static void Apply(Transform frame)
  {
   frame.position=FramePosition;
   var mattress=GameObject.Find("Mattress and blanket");if(mattress)mattress.transform.position=FramePosition+Vector3.up*.28f;
   var pillow=GameObject.Find("Pillow");if(pillow)pillow.transform.position=FramePosition+new Vector3(0,.42f,.7f);
   Physics.SyncTransforms();
  }
 }
}
