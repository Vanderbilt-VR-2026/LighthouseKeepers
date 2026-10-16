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
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
namespace LighthouseKeepers.Presentation.Environment.Tests
{
    public sealed class WatchInteractionTests
    {
        float previous;
        [UnityTest] public IEnumerator GrabRepairDrainAndSleepAdvanceDayWithoutMovingHead()
        {
            previous=Time.captureDeltaTime;Time.captureDeltaTime=.05f;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(int i=0;i<5;i++)yield return null;
            var c=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(x=>x.GetType().Name=="WatchDemoController");var t=c.GetType();
            var wrench=UnityEngine.Object.FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None).Single(x=>x.name.StartsWith("Grab wrench"));
            Assert.That(t.GetMethod("TryBeginRepair").Invoke(c,null),Is.False);
            var hand=new GameObject("Test direct hand").AddComponent<XRDirectInteractor>();hand.transform.position=new Vector3(.8f,1.05f,-.15f);
            var sphere=hand.gameObject.AddComponent<SphereCollider>();sphere.isTrigger=true;sphere.radius=.08f;
            wrench.transform.position=hand.transform.position;
            int pulses=0;float lastAmplitude=0,lastDuration=0;
            var haptic=wrench.GetComponents<MonoBehaviour>().Single(x=>x.GetType().Name=="WrenchHaptics");
            Action<float,float> observe=(a,d)=>{pulses++;lastAmplitude=a;lastDuration=d;};haptic.GetType().GetEvent("PulseRequested").AddEventHandler(haptic,observe);
            var manager=UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
            yield return null;manager.SelectEnter((IXRSelectInteractor)hand,(IXRSelectInteractable)wrench);
            yield return null;Assert.That(pulses,Is.EqualTo(1));Assert.That(lastAmplitude,Is.EqualTo(.3f));Assert.That(lastDuration,Is.EqualTo(.06f));Assert.That(wrench.isSelected,Is.True);Assert.That(t.GetMethod("TryBeginRepair").Invoke(c,null),Is.True);
            for(int i=0;i<73;i++)yield return null;
            Assert.That(pulses,Is.EqualTo(2));Assert.That(lastAmplitude,Is.EqualTo(.6f));Assert.That(lastDuration,Is.EqualTo(.12f));
            var state=t.GetProperty("State").GetValue(c);Assert.That(state.GetType().GetProperty("Repaired").GetValue(state),Is.True);
            var flood=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(x=>x.GetType().Name=="FloodController");Assert.That(flood.GetType().GetProperty("Paused").GetValue(flood),Is.True);
            var head=(Transform)t.GetField("head",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(c);
            // Use an isolated test head to verify sleep does not move the tracked view.
            var testHead=new GameObject("Test sleep head").transform;testHead.position=new Vector3(0f,8.2f,-.3f);t.GetField("head",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(c,testHead);
            var start=testHead.position;Assert.That(t.GetMethod("TrySleep").Invoke(c,null),Is.True);
            for(int i=0;i<45;i++)yield return null;
            Assert.That(state.GetType().GetProperty("Day").GetValue(state),Is.EqualTo(2));Assert.That(testHead.position,Is.EqualTo(start));
            Assert.That(pulses,Is.EqualTo(2),"No repeated haptics while holding the wrench");
            t.GetField("head",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(c,head);
            manager.SelectExit((IXRSelectInteractor)hand,(IXRSelectInteractable)wrench);UnityEngine.Object.Destroy(hand.gameObject);UnityEngine.Object.Destroy(testHead.gameObject);
        }
        [TearDown] public void Restore(){Time.captureDeltaTime=previous;}
    }
}
#endif
