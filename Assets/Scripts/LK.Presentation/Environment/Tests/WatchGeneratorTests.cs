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
    public sealed class WatchGeneratorTests
    {
        static Type Find(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        static object Get(object o,string p)=>o.GetType().GetProperty(p).GetValue(o);
        static object Call(object o,string m,params object[] args)=>o.GetType().GetMethod(m).Invoke(o,args);
        static T Field<T>(object o,string p)=>(T)o.GetType().GetField(p,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
        [UnityTest] public IEnumerator RepairFirstWaitsForPowerAndThreeRealPullsRestorePump()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity",new LoadSceneParameters(LoadSceneMode.Single));
            float deadline=Time.realtimeSinceStartup+30;
            while(!GameObject.Find("DevStartDay2") && Time.realtimeSinceStartup<deadline)yield return null;
            var shortcut=GameObject.Find("DevStartDay2");Assert.That(shortcut,Is.Not.Null);
            shortcut.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            while(SceneManager.GetActiveScene().name!="LK_Lab_Environment" && Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;yield return null;
            var watch=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c=>c.GetType().Name=="WatchDemoController");
            var state=Get(watch,"State");Assert.That(Get(state,"Day"),Is.EqualTo(2));Assert.That(Get(state,"Power").ToString(),Is.EqualTo("Off"));
            Assert.That(Find("LighthouseKeepers.StormWatch.WatchDayLaunch").GetMethod("Consume").Invoke(null,null),Is.EqualTo(1),"Dev request must be one-shot");
            var starter=GameObject.Find("GRIP_StarterHandle").GetComponents<MonoBehaviour>().Single(c=>c.GetType().Name=="GeneratorRestart");
            var simple=starter.GetComponent<XRSimpleInteractable>();var rest=starter.transform.position;var direction=GameObject.Find("STN_GENERATOR").transform.forward;
            var hand=new GameObject("Generator test hand").AddComponent<XRDirectInteractor>();var sphere=hand.gameObject.AddComponent<SphereCollider>();sphere.isTrigger=true;sphere.radius=.04f;
            var manager=UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();hand.transform.position=rest;yield return null;
            Call(state,"CompleteRepair");yield return null;
            var flood=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c=>c.GetType().Name=="FloodController");
            var height=Get(flood,"Height");yield return new WaitForSeconds(.2f);Assert.That(Get(flood,"Height"),Is.EqualTo(height),"No drainage without electric power");
            Assert.That(Get(state,"Complete"),Is.False);
            for(int i=1;i<=3;i++)
            {
                hand.transform.position=rest;manager.SelectEnter((IXRSelectInteractor)hand,(IXRSelectInteractable)simple);yield return null;
                hand.transform.position=rest+direction*.15f;yield return null;Assert.That(Get(state,"StarterPulls"),Is.EqualTo(i-1));
                hand.transform.position=rest+direction*.31f;yield return null;yield return null;
                Assert.That(Get(state,"StarterPulls"),Is.EqualTo(i));
                yield return null;Assert.That(Get(state,"StarterPulls"),Is.EqualTo(i),"Held extension repeats");
                manager.SelectExit((IXRSelectInteractor)hand,(IXRSelectInteractable)simple);
                yield return new WaitForSeconds(.3f);Assert.That((float)Get(starter,"ExtensionM"),Is.LessThan(.001f));
            }
            Assert.That(Get(state,"Power").ToString(),Is.EqualTo("On"));Assert.That(Get(state,"Complete"),Is.True);
            yield return new WaitForSeconds(.2f);Assert.That((float)Get(flood,"Height"),Is.LessThan((float)height));
            Assert.That(Field<TextMesh>(watch,"board").text,Does.Contain("[X] Restart the generator").And.Contain("[X] Repair the pipe"));
            Call(state,"Retry");yield return null;Assert.That(Get(state,"Power").ToString(),Is.EqualTo("Off"));Assert.That(Get(state,"StarterPulls"),Is.Zero);
            UnityEngine.Object.Destroy(hand.gameObject);
        }
    }
}
#endif
