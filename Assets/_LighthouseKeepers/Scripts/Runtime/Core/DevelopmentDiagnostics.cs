using UnityEngine;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Progression;
namespace LighthouseKeepers.Core {
public sealed class DevelopmentDiagnostics:MonoBehaviour {
 [SerializeField] bool showOverlay;
 float frameTime;
 void Update(){frameTime=Mathf.Lerp(frameTime,Time.unscaledDeltaTime,0.03f);}
 void OnGUI(){if(!showOverlay||(!Application.isEditor&&!Debug.isDebugBuild))return;GUILayout.BeginArea(new Rect(15,15,300,210),GUI.skin.box);GUILayout.Label("Lighthouse Keepers development / "+(1/Mathf.Max(frameTime,0.001f)).ToString("F0")+" FPS");
 var flood=FindAnyObjectByType<FloodController>();if(flood){flood.Paused=GUILayout.Toggle(flood.Paused,"Pause flood");float h=GUILayout.HorizontalSlider(flood.Height,-0.25f,10.5f);if(!Mathf.Approximately(h,flood.Height))flood.SetHeight(h);if(GUILayout.Button("Reset flood"))flood.ResetFlood();}
 var day=FindAnyObjectByType<DayDirector>();if(day)for(int i=1;i<=5;i++)if(GUILayout.Button("Preview day "+i))day.ApplyDay(i);GUILayout.EndArea();}
}}
