#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
namespace LighthouseKeepers.Presentation.Environment.Tests
{
    public sealed class EnvironmentPresentationTests
    {
        [UnityTest] public IEnumerator LabUsesOnePlayerAndLightingUpdatesAllocateNoManagedMemory()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(int i=0;i<10;i++)yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            var components=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Assert.That(components.Count(c=>c&&c.GetType().FullName=="Unity.XR.CoreUtils.XROrigin"),Is.EqualTo(1));
            var group=UnityEngine.Object.FindFirstObjectByType<BeaconAndMainsLightingGroup>();Assert.That(group,Is.Not.Null);
            group.Powered=true;yield return null;yield return null;
            Assert.That(LightmapSettings.lightmaps.Length,Is.GreaterThan(0));var powered=LightmapSettings.lightmaps[0].lightmapColor;
            group.Powered=false;yield return null;yield return null;Assert.That(LightmapSettings.lightmaps[0].lightmapColor,Is.Not.SameAs(powered));
            group.Powered=true;yield return null;yield return null;
            var method=typeof(BeaconAndMainsLightingGroup).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            var update=(Action)Delegate.CreateDelegate(typeof(Action),group,method);update();long allocated=0;
            // Measure only the environment component's update. Editor/XRI/test-runner allocations
            // outside this scope are not evidence about this component, and are not claimed zero.
            for(int frame=0;frame<600;frame++) {yield return null;long before=GC.GetAllocatedBytesForCurrentThread();update();allocated+=GC.GetAllocatedBytesForCurrentThread()-before;}
            Assert.That(allocated,Is.EqualTo(0),"Environment lighting steady-state update allocation over 600 frames");
        }
    }
}
#endif
