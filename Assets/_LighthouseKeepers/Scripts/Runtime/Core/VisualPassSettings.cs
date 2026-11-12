using UnityEngine;
namespace LighthouseKeepers.Core {
 [CreateAssetMenu(menuName="Lighthouse Keepers/Visual pass switches")]
 public sealed class VisualPassSettings:ScriptableObject {
  public bool eliStormSky=false;
  public bool renderQuality=true,materials=true,lighting=true,water=true,setDressing=true,audioGaps=true;
  [Range(0,1)]public float fixedFoveation=.5f;
  public static VisualPassSettings Current=>System.Environment.GetEnvironmentVariable("LK_VISUALS_DISABLED")=="1"?null:Resources.Load<VisualPassSettings>("VisualPassSettings");
 }
}