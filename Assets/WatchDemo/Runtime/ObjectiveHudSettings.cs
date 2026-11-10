using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
 public static class ObjectiveHudSettings
 {
  public const float Duration=5f, Distance=1.5f;
  public static float Height { get=>Mathf.Clamp(PlayerPrefs.GetFloat("LK.Objective.Height",-13.5f),-20,-5); set {PlayerPrefs.SetFloat("LK.Objective.Height",Mathf.Clamp(value,-20,-5));PlayerPrefs.Save();} }
  public static Quaternion Facing(Quaternion head)=>head*Quaternion.Euler(-Height,0,0);
 }
}
