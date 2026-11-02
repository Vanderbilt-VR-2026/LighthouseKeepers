using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class WatchReplicaTests
    {
        static object Watch()=>Activator.CreateInstance(Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true));
        static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name).Invoke(target,args);
        static T Get<T>(object target,string name)=>(T)target.GetType().GetProperty(name).GetValue(target);
        [Test] public void HostDayPowerAndTaskStateAppliesToReadOnlyGuestWhileGuestCannotAdvanceAnything()
        {
            var host=Watch();var guest=Watch();Call(guest,"SetRemoteReplica",true);
            Call(host,"Tick",12d);Call(host,"CompleteRepair");Call(host,"Sleep"); // Day 2 starts without power.
            Call(host,"Tick",5d);
            Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.12f)));
            Assert.AreEqual(2,Get<int>(guest,"Day"));Assert.AreEqual(5d,Get<double>(guest,"Elapsed"));
            Assert.AreEqual("Off",Get<object>(guest,"Power").ToString());
            Call(guest,"Tick",100d);Assert.IsFalse((bool)Call(guest,"StarterPulled"));Assert.IsFalse((bool)Call(guest,"CompleteRepair"));
            Assert.IsFalse((bool)Call(guest,"Sleep"));Call(guest,"Retry");Assert.AreEqual(5d,Get<double>(guest,"Elapsed"));
            var radio=Get<object>(guest,"Radio");Assert.IsFalse((bool)Call(radio,"ToggleMain"));
            for(int i=0;i<3;i++)Call(host,"StarterPulled");Call(host,"CompleteRepair");
            Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.02f)));
            Assert.AreEqual("On",Get<object>(guest,"Power").ToString());Assert.AreEqual(2,Get<int>(guest,"CompletedTasks"));
            Assert.IsFalse((bool)Call(guest,"Sleep"));Call(host,"Sleep");
            Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.02f)));Assert.AreEqual(3,Get<int>(guest,"Day"));
        }
        [Test] public void HostLossContinuationPreservesTheEntirePipeOrderAndUnfinishedRatchet()
        {
            var host=Watch();var guest=Watch();var pipe=Get<object>(host,"Pipe");
            Call(pipe,"Reset",17,false);
            for(int i=0;i<32;i++)StepPipe(pipe);
            Call(guest,"SetRemoteReplica",true);
            Assert.IsTrue((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.2f)));
            var copy=Get<object>(guest,"Pipe");
            Call(guest,"SetRemoteReplica",false);
            for(int i=0;i<1000&&!Get<bool>(pipe,"Complete");i++)
            {
                StepPipe(pipe);StepPipe(copy);
                Assert.AreEqual(Get<int>(pipe,"ActiveBolt"),Get<int>(copy,"ActiveBolt"));
                Assert.AreEqual(Get<int>(pipe,"TurnsDone"),Get<int>(copy,"TurnsDone"));
                Assert.AreEqual(Get<float>(pipe,"StrokeDegrees"),Get<float>(copy,"StrokeDegrees"));
                Assert.AreEqual(Get<bool>(pipe,"NeedsReturn"),Get<bool>(copy,"NeedsReturn"));
            }
            Assert.IsTrue(Get<bool>(host,"Repaired"));Assert.IsTrue(Get<bool>(guest,"Repaired"));
            Assert.IsTrue((bool)Call(guest,"Sleep"));Assert.AreEqual(2,Get<int>(guest,"Day"));
        }
        static void StepPipe(object pipe)
        {Call(pipe,"Observe",.1f,Get<bool>(pipe,"NeedsReturn")?-7f:7f,true,true,true);}
        [Serializable] sealed class Wire
        {public string protocol="LK-Lab-Coop-v2",world=new string('a',32),nonce=new string('b',32),state;public int sequence=100000;}
        [Test] public void CompleteSnapshotFitsExistingReliableChannelAndRejectsCorruptPipePermutation()
        {
            var host=Watch();var guest=Watch();Call(guest,"SetRemoteReplica",true);
            var snapshot=Call(host,"Capture",.2f);
            snapshot.GetType().GetField("notice").SetValue(snapshot,new string('x',160));
            var packet=UnityEngine.JsonUtility.ToJson(new Wire{state=UnityEngine.JsonUtility.ToJson(snapshot)});
            Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(packet),FusionMessageChannel.MaxBytes);
            snapshot.GetType().GetField("order1").SetValue(snapshot,snapshot.GetType().GetField("order0").GetValue(snapshot));
            Assert.IsFalse((bool)Call(guest,"ApplyRemote",snapshot));
            Assert.AreEqual(0d,Get<double>(guest,"Elapsed"));
        }
        [UnityTest] public IEnumerator SameDayHostRetryResetsGuestRadioAudioAfterPreviousBroadcast()
        {
            yield return SceneManager.LoadSceneAsync("LK_Lab_Environment");yield return null;
            var view=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(x=>x.GetType().FullName=="LighthouseKeepers.WatchDemo.WatchDemoController");
            Call(view,"SetNetworkMode",false,true);
            var host=Watch();Call(host,"CompleteRepair");Call(host,"Sleep");
            for(int i=0;i<3;i++)Call(host,"StarterPulled");Call(host,"CompleteRepair");Call(host,"Sleep");
            for(int i=0;i<3;i++)Call(host,"StarterPulled");var radio=Get<object>(host,"Radio");
            Call(radio,"ToggleMain");Call(radio,"Tune",156.8f);Call(host,"Tick",1.1d);
            foreach(int symbol in new[]{0,2,1,3})Call(radio,"PressCode",symbol);
            Call(radio,"SetMicrophone",true,true);Call(host,"Tick",3.1d);Assert.IsTrue(Get<bool>(radio,"Completed"));
            var first=Call(host,"Capture",.12f);first.GetType().GetField("revision").SetValue(first,1);
            Assert.IsTrue((bool)Call(view,"ApplyHostSnapshot",first));yield return null;
            Call(host,"Retry");for(int i=0;i<3;i++)Call(host,"StarterPulled");Call(radio,"ToggleMain");Call(host,"Tick",.1d);
            var retry=Call(host,"Capture",.02f);retry.GetType().GetField("revision").SetValue(retry,2);
            Assert.IsTrue((bool)Call(view,"ApplyHostSnapshot",retry));yield return null;
            var console=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(x=>x.GetType().FullName=="LighthouseKeepers.WatchDemo.WatchRadioConsole");
            var hiss=(AudioSource)console.GetType().GetField("hiss").GetValue(console);
            Assert.Greater(hiss.volume,.05f,"Guest retained the previous broadcast's muted tuning audio after a same-day retry");
        }
        [Test] public void SoloRejectsRemoteWritesAndReplicaRejectsMalformedOrUnknownDay()
        {
            var host=Watch();var guest=Watch();var snapshot=Call(host,"Capture",.02f);
            Assert.IsFalse((bool)Call(host,"ApplyRemote",snapshot));Call(guest,"SetRemoteReplica",true);
            snapshot.GetType().GetField("day").SetValue(snapshot,999);Assert.IsFalse((bool)Call(guest,"ApplyRemote",snapshot));
            snapshot=Call(host,"Capture",float.NaN);Assert.IsFalse((bool)Call(guest,"ApplyRemote",snapshot));
            Assert.AreEqual(1,Get<int>(guest,"Day"));Assert.AreEqual(0d,Get<double>(guest,"Elapsed"));
        }
    }
}
