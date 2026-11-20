using System;using System.Linq;using NUnit.Framework;using UnityEngine;using UnityEditor.SceneManagement;using LighthouseKeepers.Lobby;using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests {
 public sealed class EquipmentWorldTests {
  [Test]public void SavedStairBackingStopsBodiesOnEveryFloor(){
   var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");
   try{var walls=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)).Where(r=>r.name.StartsWith("Stairwell liner")).ToArray();Assert.Greater(walls.Length,50);foreach(var wall in walls){var c=wall.GetComponent<BoxCollider>();Assert.NotNull(c,wall.name);Assert.True(c.enabled,wall.name);Assert.False(c.isTrigger,wall.name);Assert.Greater(c.size.x*c.size.y*c.size.z,0,wall.name);}}
   finally{EditorSceneManager.ClosePreviewScene(scene);}
  }
  [TestCase(KeeperGameMode.Campaign)][TestCase(KeeperGameMode.Survival)]public void EveryLiveNightHasNoClothRequirement(KeeperGameMode mode){
   var t=Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);var data=Resources.Load("M3Campaign");
   for(int day=1;day<=5;day++){var s=Activator.CreateInstance(t,data.GetType().GetField("nights").GetValue(data),day,Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"),mode);
    try{t.GetMethod("Tick").Invoke(s,new object[]{90d});var b=t.GetProperty("Beacon").GetValue(s);b.GetType().GetMethod("Tick").Invoke(b,new object[]{600f,true});Assert.AreEqual(0f,b.GetType().GetProperty("Soot").GetValue(b),$"{mode} day {day}");}
    finally{t.GetMethod("DisposeSurvival").Invoke(s,null);}
   }
  }
 }
}
