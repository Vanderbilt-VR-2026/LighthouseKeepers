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
        static object Call(object o,string m,params object[] a)=>o.GetType().GetMethod(m).Invoke(o,a);
        static T Field<T>(object o,string p)=>(T)o.GetType().GetField(p,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
        [UnityTest] public IEnumerator LabUsesOnePlayerAndLightingUpdatesAllocateNoManagedMemory()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(int i=0;i<10;i++)yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            var components=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Assert.That(components.Count(c=>c&&c.GetType().FullName=="Unity.XR.CoreUtils.XROrigin"),Is.EqualTo(1));
            var emergency=components.Single(c=>c.GetType().Name=="EmergencyLighting");
            var watch=components.Single(c=>c.GetType().Name=="WatchDemoController");var state=watch.GetType().GetProperty("State").GetValue(watch);
            var group=UnityEngine.Object.FindFirstObjectByType<BeaconAndMainsLightingGroup>();Assert.That(group.enabled,Is.False,"Only the central controller owns lighting now");
            Assert.That(LightmapSettings.lightmaps.Length,Is.GreaterThan(0));var bake=LightmapSettings.lightmaps[0].lightmapColor;
            var lights=Field<Light[]>(emergency,"interiorLights");var warm=lights.Select(l=>l.color).ToArray();
            Call(state,"CompleteRepair");Call(state,"Sleep");yield return new WaitForSeconds(.6f);
            Assert.That(LightmapSettings.lightmaps[0].lightmapColor,Is.SameAs(bake),"Keep accepted bake, centrally modulate red surfaces and lights");
            Assert.That(Field<AudioSource>(emergency,"alarm").isPlaying,Is.True);
            foreach(var light in lights){Assert.That(light.color.r,Is.GreaterThan(light.color.g*5));Assert.That(light.intensity,Is.GreaterThan(0));}
            var method=emergency.GetType().GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            var update=(Action)Delegate.CreateDelegate(typeof(Action),emergency,method);update();long allocated=0;
            // Measure only central lighting, including its off-state pulse; not the whole player.
            for(int frame=0;frame<600;frame++){yield return null;long before=GC.GetAllocatedBytesForCurrentThread();update();allocated+=GC.GetAllocatedBytesForCurrentThread()-before;}
            Assert.That(allocated,Is.EqualTo(0),"Central emergency pulse managed allocations over 600 frames");
            for(int i=0;i<3;i++)Call(state,"StarterPulled");yield return null;
            Assert.That((float)emergency.GetType().GetProperty("WarmBlend").GetValue(emergency),Is.LessThan(1),"Warm recovery must fade");
            Assert.That(Field<AudioSource>(emergency,"alarm").isPlaying,Is.False);
            yield return new WaitForSeconds(1.2f);
            for(int i=0;i<lights.Length;i++)Assert.That(lights[i].color,Is.EqualTo(warm[i]));
        }
    }
}
#endif
