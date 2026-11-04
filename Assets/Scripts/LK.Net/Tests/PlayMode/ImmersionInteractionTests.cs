#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.StormWatch;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Object=UnityEngine.Object;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class ImmersionInteractionTests
    {
        string oldM2,oldImmersion;
        MonoBehaviour watch;HandRig[] hands;
        static MonoBehaviour Find(string name)=>Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c=>c.GetType().Name==name);
        static T Value<T>(object o,string n)=>(T)o.GetType().GetProperty(n).GetValue(o);
        static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);
        static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n).Invoke(o,args);
        [UnitySetUp]public IEnumerator Setup()
        {
            oldM2=System.Environment.GetEnvironmentVariable("LK_S4M2_DISABLED");oldImmersion=System.Environment.GetEnvironmentVariable("LK_IMMERSION_DISABLED");
            System.Environment.SetEnvironmentVariable("LK_S4M2_DISABLED",null);System.Environment.SetEnvironmentVariable("LK_IMMERSION_DISABLED",null);
            WatchDayLaunch.RequestedDay=3;SoloWatchSession.Request();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;
            watch=Find("WatchDemoController");
            foreach(var b in Object.FindObjectsByType<Behaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(b.GetType().Name=="XRInputModalityManager"||b.GetType().Name=="TrackedPoseDriver")b.enabled=false;
            hands=Object.FindObjectsByType<HandRig>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(var hand in hands){hand.transform.parent.gameObject.SetActive(true);hand.Direct.selectInput=new XRInputButtonReader("Immersion test grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);}
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            foreach(var h in hands??Array.Empty<HandRig>())if(h&&h.Direct.isPerformingManualInteraction)h.Direct.EndManualInteraction();
            System.Environment.SetEnvironmentVariable("LK_S4M2_DISABLED",oldM2);System.Environment.SetEnvironmentVariable("LK_IMMERSION_DISABLED",oldImmersion);SoloWatchSession.Cancel();WatchDayLaunch.RequestedDay=1;
            yield return null;
        }
        [UnityTest]public IEnumerator FuelHandleGrabsWithEitherHandUprightDoesNotPourTiltRaisesHostFuel()
        {
            var controls=Find("WatchEngineControls");var can=Value<Component>(controls,"FuelCan");var grab=Value<XRGrabInteractable>(can,"Grab");var filler=Value<Transform>(controls,"Filler");var tip=Value<Transform>(can,"Tip");
            var state=Value<object>(watch,"State");var engine=Value<LighthouseKeepers.Core.WatchEngineState>(state,"Engine");
            Vector3 home=can.transform.position;
            foreach(var hand in hands)
            {
                Call(can,"ResetHome");engine.Reset(.04f,0,false);yield return null;
                var handle=can.transform.TransformPoint(new Vector3(0,.30f,0));
                Assert.IsTrue(grab.colliders.Any(c=>Vector3.Distance(c.ClosestPoint(handle),handle)<.005f),"Actual carry handle has no grab collider");
                var controller=hand.transform.parent;controller.SetPositionAndRotation(handle,can.transform.rotation);
                var direct=hand.Direct;direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)grab);yield return new WaitForSeconds(.2f);Assert.True(grab.isSelected);
                var before=can.transform.position;controller.position+=Vector3.right*.25f;yield return new WaitForSeconds(.2f);Assert.Greater(Vector3.Distance(before,can.transform.position),.15f,"Free carry stopped following grip");
                Assert.False((bool)Call(controls,"CanPour",true),"Can poured away from filler");
                controller.rotation=Quaternion.Euler(0,controls.transform.eulerAngles.y,105);yield return new WaitForSeconds(.2f);
                for(int i=0;i<8;i++){controller.position+=filler.position+Vector3.up*.06f-tip.position;yield return new WaitForSeconds(.05f);}
                Assert.True((bool)Call(controls,"CanPour",true),"Tilted spout over opening rejected for "+hand.Node);
                float fuel=engine.Fuel;yield return new WaitForSeconds(.6f);Assert.Greater(engine.Fuel,fuel+.02f,"Valid pour did not change host state");
                direct.EndManualInteraction();direct.selectInput.manualPerformed=false;Call(can,"ResetHome");Assert.Less(Vector3.Distance(home,can.transform.position),.001f);
            }
        }
        [UnityTest]public IEnumerator WrenchCarriesSnapsTwistsAndRetainsProgressForBothHands()
        {
            var pipe=Find("WatchPipeRepair");var grab=Field<XRGrabInteractable>(pipe,"wrench");var jaw=Field<Transform>(pipe,"jaw");var state=Value<object>(Value<object>(watch,"State"),"Pipe");
            var socket=grab.GetComponents<MonoBehaviour>().Single(c=>c.GetType().Name=="ImmersionWrenchSocket");
            Assert.GreaterOrEqual(grab.singleGrabTransformersCount,2,"Socket replaced the regular free-carry transformer");
            foreach(var hand in hands)
            {
                var direct=hand.Direct;var controller=hand.transform.parent;controller.position=grab.transform.position;direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)grab);yield return new WaitForSeconds(.2f);
                float deadline=Time.realtimeSinceStartup+8;
                while(!Value<bool>(socket,"IsSnapped")&&Time.realtimeSinceStartup<deadline){controller.position+=Vector3.ClampMagnitude((Value<Vector3>(pipe,"ActivePosition")-jaw.position)*.2f,.12f);yield return null;}
                Assert.True(Value<bool>(socket,"IsSnapped"),"No snap for "+hand.Node);
                yield return new WaitForSeconds(.2f);int ticks=Value<int>(state,"BoltsDone")*10+Value<int>(state,"TurnsDone");
                deadline=Time.realtimeSinceStartup+3;
                while(Value<int>(state,"BoltsDone")*10+Value<int>(state,"TurnsDone")==ticks&&Time.realtimeSinceStartup<deadline){controller.rotation=Quaternion.AngleAxis(100*Time.deltaTime,controller.forward)*controller.rotation;yield return null;}
                Assert.Greater(Value<int>(state,"BoltsDone")*10+Value<int>(state,"TurnsDone"),ticks,"Wrist roll did not turn bolt for "+hand.Node+" angle="+Field<float>(socket,"angle")+" contact="+Value<bool>(pipe,"InContact")+" seat="+Value<float>(state,"SeatElapsed")+" stroke="+Value<float>(state,"StrokeDegrees")+" direction="+Value<float>(state,"Direction"));
                float progress=Value<float>(state,"StrokeDegrees");controller.position+=Vector3.down*.30f;yield return new WaitForSeconds(.2f);
                Assert.False(Value<bool>(socket,"IsSnapped"));Assert.GreaterOrEqual(Value<float>(state,"StrokeDegrees"),progress-.1f);
                direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return null;
            }
        }
    }
}
#endif
