using System;
using System.Collections;
using System.Linq;
using LighthouseKeepers.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class S4HardeningTests
    {
        string disabled;
        static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name).Invoke(target,args);
        static T Get<T>(object target,string name)=>(T)target.GetType().GetProperty(name).GetValue(target);
        static MonoBehaviour Find(string name)=>UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(x=>x.GetType().Name==name);
        static Type TypeFor(string name)=>Type.GetType("LighthouseKeepers.WatchDemo."+name+", Assembly-CSharp",true);
        [SetUp] public void EnableRequestedFeatures(){disabled=System.Environment.GetEnvironmentVariable("LK_S4_DISABLED");System.Environment.SetEnvironmentVariable("LK_S4_DISABLED",null);}
        [TearDown] public void RestoreSwitch()=>System.Environment.SetEnvironmentVariable("LK_S4_DISABLED",disabled);
        static object Replica(object host)
        {
            var catalog=Resources.Load("WatchDays");var days=catalog.GetType().GetField("days").GetValue(catalog);
            var copy=Activator.CreateInstance(TypeFor("WatchState"),days,1,S4Tuning.Current);Call(copy,"SetRemoteReplica",true);return copy;
        }
        [UnityTest] public IEnumerator HighWaterRequiresContinuousThreeSecondsThenFailsBothAndRetryClearsLoss()
        {
            yield return SceneManager.LoadSceneAsync("LK_Lab_Environment");yield return null;
            var view=Find("WatchDemoController");var state=Get<object>(view,"State");var flood=Find("FloodController");var t=S4Tuning.Current;
            Assert.NotNull(t);Assert.AreEqual(315,Get<double>(state,"Duration"),.01);
            Assert.IsFalse(UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Any(c=>c.GetType().Name=="CrewVoiceSession"),"Offline solo must not create voice or ask for mic permission");
            Call(flood,"SetHeight",t.failWaterHeightM+.01f);yield return new WaitForSeconds(1.5f);Assert.IsFalse(Get<bool>(state,"Failed"));
            Call(flood,"SetHeight",.04f);yield return null;yield return null;
            Assert.AreEqual(0,Get<double>(state,"HighWaterElapsed"));
            Call(flood,"SetHeight",t.failWaterHeightM+.01f);yield return new WaitForSeconds(2.6f);Assert.IsFalse(Get<bool>(state,"Failed"));
            yield return new WaitForSeconds(.55f);Assert.IsTrue(Get<bool>(state,"HighWaterFailed"));
            Assert.That(Get<string>(state,"FailureMessage"),Does.Contain("FLOODED"));
            Assert.IsTrue(Get<bool>(Find("WatchMenu"),"IsOpen"));
            var guest=Replica(state);Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(state,"Capture",.56f)));Assert.IsTrue(Get<bool>(guest,"Failed"));
            Call(guest,"ObserveWater",4d,0f);Call(guest,"Retry");Assert.IsTrue(Get<bool>(guest,"Failed"),"Guest cannot override the host's loss");
            Assert.IsTrue((bool)Call(view,"RetryDay"));yield return null;
            Assert.IsFalse(Get<bool>(state,"Failed"));Assert.Less(Get<float>(flood,"Height"),.03f);
            Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(state,"Capture",.02f)));Assert.IsFalse(Get<bool>(guest,"Failed"));
            Assert.AreEqual(0,Get<double>(guest,"HighWaterElapsed"));
        }
        [UnityTest] public IEnumerator BeaconDeadlineFailsBothAndRetryRestoresDayFaults()
        {
            yield return SceneManager.LoadSceneAsync("LK_Lab_Environment");yield return null;
            var view=Find("WatchDemoController");var state=Get<object>(view,"State");
            Call(state,"CompleteRepair");Assert.IsTrue((bool)Call(state,"Sleep"));
            Assert.AreEqual(2,Get<int>(state,"Day"));Assert.AreEqual(72,Get<double>(state,"BeaconDarkLimit"),.01);
            Call(state,"Tick",Get<double>(state,"BeaconDarkRemaining")-.1);Assert.IsFalse(Get<bool>(state,"Failed"));
            yield return new WaitForSeconds(.15f);Assert.IsTrue(Get<bool>(state,"BeaconDarkFailed"));
            Assert.IsTrue(Get<bool>(Find("WatchMenu"),"IsOpen"));
            var guest=Replica(state);Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(state,"Capture",.1f)));Assert.IsTrue(Get<bool>(guest,"BeaconDarkFailed"));
            Assert.IsTrue((bool)Call(view,"RetryDay"));yield return null;
            Assert.AreEqual(2,Get<int>(state,"Day"));Assert.IsFalse(Get<bool>(state,"Failed"));Assert.AreEqual("Off",Get<object>(state,"Power").ToString());
            Call(guest,"ApplyRemote",Call(state,"Capture",.02f));Assert.IsFalse(Get<bool>(guest,"Failed"));
        }
    }
}
