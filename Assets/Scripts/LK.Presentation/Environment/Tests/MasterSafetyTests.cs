#if UNITY_EDITOR
using System;using System.Collections;using System.Linq;using NUnit.Framework;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.TestTools;
namespace LighthouseKeepers.Presentation.Environment.Tests {
 public sealed class MasterSafetyTests {
  [UnityTest] public IEnumerator HunterRoutesRespectFloorsAndClearTheActualWalls()
  {
   UnityEditor.EditorUtility.audioMasterMute=true;
   var launch=Type.GetType("LighthouseKeepers.StormWatch.WatchDayLaunch, LighthouseKeepers.Runtime",false)??AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.StormWatch.WatchDayLaunch")).First(t=>t!=null);
   launch.GetProperty("RequestedDay").SetValue(null,3);
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   launch.GetProperty("RequestedDay").SetValue(null,1);
   var type=Type.GetType("LighthouseKeepers.WatchDemo.HunterNavigation, Assembly-CSharp",true);
   var tuning=Resources.Load("HorrorTuning");var data=Resources.Load<UnityEngine.AI.NavMeshData>("HunterNavigation");Assert.NotNull(tuning);Assert.NotNull(data);
   using var nav=(IDisposable)Activator.CreateInstance(type,tuning,data);
   var points=new[]{new Vector3(-.5f,0,-7),new Vector3(2.6f,0,0),new Vector3(-1,3.3f,-1),new Vector3(1,6.6f,0),new Vector3(1,9.9f,0)};
   for(int i=0;i<points.Length;i++)for(int j=0;j<points.Length;j++)if(i!=j){
    object[] args={points[i],points[j],null};Assert.True((bool)type.GetMethod("Route").Invoke(nav,args),$"Missing {i}->{j}");var corners=(Vector3[])args[2];
    Assert.Less(Mathf.Abs(corners[0].y-points[i].y),.35f);Assert.Less(Mathf.Abs(corners[^1].y-points[j].y),.35f);
    for(int k=1;k<corners.Length;k++)Assert.True((bool)type.GetMethod("SegmentClear").Invoke(nav,new object[]{corners[k-1],corners[k]}),$"Blocked {i}->{j}, segment{k}");
   }
   object[] wrongFloor={new Vector3(-1,6.6f,-1),null};Assert.False((bool)type.GetMethod("SampleFloor").Invoke(nav,wrongFloor),"Bunk target must not snap to stairs above it");
  }
  [UnityTest] public IEnumerator HandLanternsSurviveTheLightBudgetGuard()
  {
   UnityEditor.EditorUtility.audioMasterMute=true;
   var launch=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.StormWatch.WatchDayLaunch")).First(t=>t!=null);
   launch.GetProperty("RequestedDay").SetValue(null,2);
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   var type=Type.GetType("LighthouseKeepers.WatchDemo.WatchDemoController, Assembly-CSharp",true);var controller=UnityEngine.Object.FindAnyObjectByType(type);var state=type.GetProperty("State").GetValue(controller);var cycle=state.GetType().GetMethod("CycleLantern");
   for(int i=0;i<2;i++){Assert.True((bool)cycle.Invoke(state,new object[]{i}));Assert.True((bool)cycle.Invoke(state,new object[]{i}));}
   yield return new WaitForSeconds(.5f);
   var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.enabled&&l.lightmapBakeType==LightmapBakeType.Realtime).ToArray();Assert.LessOrEqual(lights.Length,2);
   Assert.AreEqual(2,lights.Count(l=>l.name=="Lantern shadowless warm pool"));Assert.True(lights.All(l=>l.shadows==LightShadows.None));
   var can=GameObject.Find("LANTERN_OIL_CAN");Assert.NotNull(can);
   foreach(var rig in UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None))foreach(var c in can.GetComponents<Collider>())Assert.True(Physics.GetIgnoreCollision(c,rig),"Oil can must not lift the player capsule");
   launch.GetProperty("RequestedDay").SetValue(null,1);
  }
  [UnityTest] public IEnumerator BakedPracticalLightingLeavesAtMostOneRealtimeSource(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   Assert.That(UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Count(l=>l.enabled&&l.lightmapBakeType==LightmapBakeType.Realtime),Is.LessThanOrEqualTo(1));Assert.That(LightmapSettings.lightmaps.Length,Is.GreaterThan(0));Assert.That(UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None).Count(p=>p.bakedTexture||p.customBakedTexture),Is.GreaterThanOrEqualTo(5));
  }
  [UnityTest] public IEnumerator DecorativeWorldDoesNotAddPlayerObstaclesAndWaterKeepsItsHostSurface(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(3);
   foreach(var name in new[]{"Previous keeper open logbook","Distant coast and inlet"}){var go=GameObject.Find(name);Assert.That(go,Is.Not.Null,name);Assert.False(go.GetComponentsInChildren<Collider>().Any(c=>c.enabled&&!c.isTrigger),name+" blocks the player");}
   // These were intentionally removed by the accepted House cleanup before the voice pass.
   foreach(var name in new[]{"Oilskin on wall hook","Wall stowed supply crate","Old stove and kettle"})Assert.IsNull(GameObject.Find(name),name+" was removed at the lead’s request");
   var flood=GameObject.Find("Visible first-floor flood");Assert.That(flood.GetComponent<Renderer>().sharedMaterial.shader.name,Is.EqualTo("LighthouseKeepers/Interior Flood Water"));Assert.That(flood.GetComponent<MeshFilter>().sharedMesh.vertexCount,Is.EqualTo(65),"Flood simulation plane remains unchanged");
   foreach(var skin in UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None).Where(s=>s.name=="Skinned glove"))Assert.That(skin.lightProbeUsage,Is.EqualTo(UnityEngine.Rendering.LightProbeUsage.BlendProbes));
  }
  [UnityTest] public IEnumerator EveryWalkableFloorBoundaryHasSupportOrGuard(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));for(int n=0;n<15;n++)yield return null;
   var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("WalkableEdgeAudit")).First(t=>t!=null);
   var output=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"LK-edge-regression.txt");var result=type.GetMethod("Run").Invoke(null,new object[]{output});
   Assert.That((int)result.GetType().GetProperty("gaps").GetValue(result),Is.Zero,System.IO.File.ReadAllText(output));
  }
  [UnityTest] public IEnumerator GalleryIsContinuousAndHouseClutterCannotStepThePlayerUp(){
   yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));for(int n=0;n<12;n++)yield return null;
   foreach(var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("SM_Wall_House_Side")))Assert.True(renderer.enabled,"Static-batched wall was incorrectly classified as duplicate");
   var guard=GameObject.Find("Continuous gallery perimeter guard");Assert.That(guard,Is.Not.Null);
   // At half-segment increments, a head-height capsule moving outwards must meet the guard.
   for(int n=0;n<128;n++){float a=n*Mathf.PI*2/128;var direction=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));var from=direction*3.85f+Vector3.up*9.9f;
    var hits=Physics.CapsuleCastAll(from+Vector3.up*.30f,from+Vector3.up*1.5f,.20f,direction,.65f,~0,QueryTriggerInteraction.Ignore);
    Assert.That(hits.Any(h=>h.collider.transform.IsChildOf(guard.transform)),Is.True,"Gallery gap at angle "+a);
   }
   foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)){var b=c.bounds;if(c.name.StartsWith("UCX_SM_Rock_Outcrop")&&b.center.z< -5&&Mathf.Abs(b.center.x)<3.5f&&b.max.y>0&&b.max.y<.2f)Assert.False(c.enabled,"House rock remains blocking: "+c.name);}
   var bed=GameObject.Find("Third-floor bed frame");Assert.That(bed,Is.Not.Null);Assert.Greater(bed.transform.position.x,2.5f,"Bunk must clear F3 transfer route");
   var overlap=Physics.OverlapCapsule(new Vector3(1.4f,6.9f,-.4f),new Vector3(1.4f,8.1f,-.4f),.25f,~0,QueryTriggerInteraction.Ignore);Assert.False(overlap.Any(c=>c.name.Contains("bed frame")||c.name.Contains("Mattress")),"Bed intersects F3 route");
  }
 }
}
#endif
