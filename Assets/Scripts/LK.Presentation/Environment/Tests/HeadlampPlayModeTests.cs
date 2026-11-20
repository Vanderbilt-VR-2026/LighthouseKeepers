#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.XR.CoreUtils;

namespace LighthouseKeepers.Presentation.Environment.Tests
{
    public sealed class HeadlampPlayModeTests
    {
        [UnityTest] public IEnumerator GameplayStartsWithAHeadTrackedBeamAndNoHandheldFlashlights()
        {
            UnityEditor.EditorUtility.audioMasterMute=true;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(int i=0;i<40;i++)yield return null;
            var controller=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).FirstOrDefault(c=>c.GetType().Name=="KeeperHeadlamps");
            var rig=UnityEngine.Object.FindFirstObjectByType<XROrigin>();
            Assert.NotNull(controller);Assert.NotNull(rig);Assert.NotNull(rig.Camera);
            var beams=((System.Collections.Generic.IEnumerable<Light>)controller.GetType().GetProperty("Lights").GetValue(controller)).ToArray();
            Assert.AreEqual(2,beams.Length);var mine=beams.Single(l=>l.transform.IsChildOf(rig.Camera.transform));
            Assert.IsTrue(mine.enabled);Assert.AreEqual(LightType.Spot,mine.type);Assert.AreEqual(LightShadows.None,mine.shadows);
            Assert.That(mine.intensity,Is.EqualTo(48.6f).Within(.001f));
            var original=rig.Camera.transform.localRotation;rig.Camera.transform.localRotation=Quaternion.Euler(12,37,0);yield return null;
            Assert.That(Vector3.Angle(mine.transform.forward,rig.Camera.transform.forward),Is.LessThan(.01f));rig.Camera.transform.localRotation=original;
            Assert.IsFalse(UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Any(c=>c.GetType().Name=="WatchLantern"));
            Assert.IsFalse(UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Any(t=>t.name.Contains("flashlight hook")));
            var kit=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).First(c=>c.GetType().Name=="ArrivalEquipment");
            Assert.AreNotEqual("GrabLantern",kit.GetType().GetProperty("Step").GetValue(kit).ToString());
            Assert.AreNotEqual("LanternWick",kit.GetType().GetProperty("Step").GetValue(kit).ToString());
        }
        [UnityTest] public IEnumerator SolidWallBlocksHeadlampExposure()
        {
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.WatchDemo.KeeperHeadlamps")).First(t=>t!=null);
            var root=new GameObject("Headlamp ray test");var lamp=root.AddComponent(type);
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(200,50,202);
            var method=type.GetMethod("Clear",BindingFlags.Instance|BindingFlags.NonPublic);Physics.SyncTransforms();
            Assert.IsFalse((bool)method.Invoke(lamp,new object[]{new Vector3(200,50,200),new Vector3(200,50,204)}));
            wall.SetActive(false);Physics.SyncTransforms();
            Assert.IsTrue((bool)method.Invoke(lamp,new object[]{new Vector3(200,50,200),new Vector3(200,50,204)}));
            UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(wall);yield return null;
        }
    }
}
#endif
