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
        [UnityTest]public IEnumerator RadioHintsWaitNinetySecondsStayQuietAtStationsAndPlayOnce()
        {
            var radio=Find("HarborRadio");yield return null;
            var origin=Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            Action<string,object> set=(n,v)=>radio.GetType().GetField(n,flags).SetValue(radio,v);
            var pending=Field<System.Collections.Generic.Queue<int>>(radio,"pending");pending.Clear();
            // Advance the narration clock only; the watch, station, proximity and audio route are real.
            Action tick=()=>radio.GetType().GetMethod("Update",flags).Invoke(radio,null);
            Action<float> idle=seconds=>{set("idle",seconds);set("quietSince",-100f);set("<Age>k__BackingField",1000f);};
            origin.MoveCameraToWorldLocation(new Vector3(0,1.65f,-8));
            int serial=Value<int>(radio,"Serial");idle(89);tick();Assert.AreEqual(serial,Value<int>(radio,"Serial"),"Early hint");
            var generator=Find("WatchGenerator");origin.MoveCameraToWorldLocation(generator.transform.position+Vector3.up*1.6f);
            idle(91);tick();Assert.AreEqual(serial,Value<int>(radio,"Serial"),"Hint while already at the generator");
            origin.MoveCameraToWorldLocation(new Vector3(0,1.65f,-8));
            var pipe=Find("WatchPipeRepair");var wrench=Field<XRGrabInteractable>(pipe,"wrench");var hand=hands[0];
            hand.transform.parent.position=wrench.transform.position;hand.Direct.selectInput.manualPerformed=true;
            hand.Direct.StartManualInteraction((IXRSelectInteractable)wrench);yield return null;
            idle(91);tick();Assert.AreEqual(serial,Value<int>(radio,"Serial"),"Hint while carrying the wrench");
            hand.Direct.EndManualInteraction();hand.Direct.selectInput.manualPerformed=false;yield return null;
            idle(91);tick();Assert.AreEqual(8,Value<int>(radio,"Cue"),"Idle generator hint not delivered");
            serial=Value<int>(radio,"Serial");idle(200);tick();Assert.AreEqual(serial,Value<int>(radio,"Serial"),"Hint repeated in same night");
            Call(radio,"EarlyBed");pending.Clear(); // Deliberate interaction is still a once-per-night cue.
            Assert.False(Field<System.Collections.Generic.HashSet<int>>(radio,"spoken").Contains(6),"Retired wrench hint played");
        }
        [UnityTest]public IEnumerator DroppedWrenchReturnsAfterEightSecondsEvenBesideKeeper()
        {
            var pipe=Find("WatchPipeRepair");var grab=Field<XRGrabInteractable>(pipe,"wrench");
            var recovery=grab.GetComponents<MonoBehaviour>().Single(c=>c.GetType().Name=="WatchWrenchReturn");
            var home=grab.transform.position;var origin=Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var hand=hands[0];var controller=hand.transform.parent;
            origin.MoveCameraToWorldLocation(new Vector3(1.7f,1.65f,0));yield return null;
            controller.position=home;hand.Direct.selectInput.manualPerformed=true;hand.Direct.StartManualInteraction((IXRSelectInteractable)grab);yield return new WaitForSeconds(.2f);
            controller.position=new Vector3(1.7f,1.1f,0);yield return new WaitForSeconds(.5f);
            // Drop from a stationary carry, not the throw velocity of the bench-to-pipe teleport.
            var body=grab.GetComponent<Rigidbody>();float settle=Time.realtimeSinceStartup+4;
            while((Vector3.Distance(grab.transform.position,controller.position)>.3f||body.linearVelocity.sqrMagnitude>.0025f)&&Time.realtimeSinceStartup<settle)yield return null;
            Assert.Less(Vector3.Distance(grab.transform.position,controller.position),.3f,"Carry did not settle before drop");
            yield return new WaitForSeconds(.3f);
            hand.Direct.EndManualInteraction();hand.Direct.selectInput.manualPerformed=false;
            yield return new WaitForSeconds(2f);
            // Follow the actual resting position: XRI throw velocity may carry it beyond the intended drop.
            origin.MoveCameraToWorldLocation(grab.transform.position+Vector3.up*1.65f);
            yield return new WaitForSeconds(.3f);
            Assert.AreEqual(0,Field<float>(recovery,"awaySeconds"),.01f,"Nearby keeper did not reset recovery timer");
            Assert.Greater(Vector3.Distance(home,grab.transform.position),3,"Dropped tool returned while keeper is beside it");
            // Proximity no longer suppresses a lost/submerged tool recovery.
            yield return new WaitForSeconds(4);Assert.Greater(Vector3.Distance(home,grab.transform.position),3,"Recovery happened before 8 seconds: "+Value<string>(recovery,"LastReturnReason"));
            yield return new WaitForSeconds(2);Assert.Less(Vector3.Distance(home,grab.transform.position),.01f);Assert.True(grab.GetComponent<Rigidbody>().isKinematic);
            // An out-of-room drop is recovered immediately instead of waiting out the timeout.
            Call(recovery,"OnNetworkReleased");grab.transform.position=new Vector3(4.5f,.2f,0);yield return null;yield return null;
            Assert.Less(Vector3.Distance(home,grab.transform.position),.01f);
        }
        [UnityTest]public IEnumerator WrenchFollowsArmFromBothHandsAndThreeStancesKeepsProgressAndDrops()
        {
            var pipe=Find("WatchPipeRepair");var grab=Field<XRGrabInteractable>(pipe,"wrench");var jaw=Field<Transform>(pipe,"jaw");
            var state=Value<object>(Value<object>(watch,"State"),"Pipe");
            var socket=grab.GetComponents<MonoBehaviour>().Single(c=>c.GetType().Name=="ImmersionWrenchSocket");
            var recovery=grab.GetComponents<MonoBehaviour>().Single(c=>c.GetType().Name=="WatchWrenchReturn");
            var origin=Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            Func<float> progress=()=>Value<int>(state,"TurnsDone")*30+Value<float>(state,"StrokeDegrees");
            foreach(var hand in hands)
            for(int stance=0;stance<3;stance++)
            {
                Call(state,"Reset",91,false);Call(pipe,"ResetView");Call(recovery,"ReturnHome");
                yield return null;Assert.False(grab.isSelected,"Previous stance still owns wrench");
                var pivot=Value<Vector3>(pipe,"ActivePosition");
                Assert.That(pivot.x,Is.InRange(3.5f,3.9f));Assert.That(pivot.y,Is.InRange(1f,1.1f));
                var bolts=Field<Transform[]>(pipe,"bolts");Assert.Greater(Vector3.Dot(bolts[Value<int>(state,"ActiveBolt")].forward,Vector3.up),.999f);
                origin.MoveCameraToWorldLocation(new Vector3(2.85f,stance==1?1.15f:1.65f,pivot.z+(stance-1)*.4f));
                var direct=hand.Direct;var controller=hand.transform.parent;
                controller.SetPositionAndRotation(grab.transform.position,Quaternion.Euler(0,stance*25,0));
                direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)grab);yield return new WaitForSeconds(.2f);
                Assert.True(grab.isSelected,"Grip failed: "+hand.Node);Assert.AreSame(direct,grab.interactorsSelecting[0],"Wrong holder");
                Debug.Log($"CLUTCH_TEST begin hand={hand.Node} stance={stance} controller={controller.position} jaw={jaw.position} active={direct.isActiveAndEnabled} timescale={Time.timeScale}");
                // Start off-centre so a short arc has room to travel toward the
                // keeper; a handle already pointing directly at them is at its
                // nearest point. Exercise mirrored strokes with the two hands.
                var keeperRadial=Vector3.ProjectOnPlane(origin.Camera.transform.position-pivot,Vector3.up).normalized;
                var face=Quaternion.LookRotation(-(Quaternion.AngleAxis(hand.Node==UnityEngine.XR.XRNode.LeftHand?45:-45,Vector3.up)*keeperRadial),Vector3.up);
                for(int align=0;align<2;align++){controller.rotation=face*Quaternion.Inverse(grab.transform.rotation)*controller.rotation;yield return new WaitForSeconds(.25f);}
                for(int stroke=0;stroke<10&&Value<int>(state,"BoltsDone")==0;stroke++)
                {
                    float before=progress();
                    // Carry from the bench to the station, then let XRI settle before approaching.
                    if(stroke==0){controller.position+=pivot+Vector3.left*.20f-jaw.position;yield return new WaitForSeconds(.6f);}
                    float deadline=Time.realtimeSinceStartup+5;
                    while(Vector3.Distance(jaw.position,pivot)>.025f&&Time.realtimeSinceStartup<deadline)
                    {controller.position+=Vector3.ClampMagnitude((pivot-jaw.position)*.5f,.04f);yield return new WaitForSeconds(.06f);}
                    Assert.Less(Vector3.Distance(jaw.position,pivot),.03f,"Visible jaw did not reach bolt before trigger: "+hand.Node+" stance "+stance+" selected="+grab.isSelected+" holder="+(grab.isSelected?grab.interactorsSelecting[0].transform.name:"none")+" controller="+controller.position+" jaw="+jaw.position+" pivot="+pivot+" failed="+Value<bool>(Value<object>(watch,"State"),"Failed"));
                    Assert.False(Value<bool>(socket,"IsSnapped"),"Auto-lock happened without trigger");
                    Assert.That(progress(),Is.EqualTo(before).Within(.5f),"Unlocked readjustment repaired bolt");
                    grab.activated.Invoke(new ActivateEventArgs{interactorObject=direct,interactableObject=grab});
                    yield return new WaitForSeconds(.12f);
                    Assert.Less(controller.position.x,pivot.x,"Test hand must approach from the room, not through the wall");
                    Assert.True(Value<bool>(socket,"IsSnapped"),hand.Node+" stance "+stance+" trigger failed to lock: held="+Value<bool>(socket,"ClutchHeld")+" selected="+grab.isSelected+" distance="+Vector3.Distance(jaw.position,pivot)+" stroke="+stroke);
                    if(stroke==0)
                    {
                        before=progress();var original=controller.rotation;controller.rotation=Quaternion.Euler(60,35,120)*controller.rotation;yield return new WaitForSeconds(.15f);
                        Assert.That(progress(),Is.EqualTo(before).Within(.5f),"Wrist rotation alone repaired bolt");controller.rotation=original;
                    }
                    var rest=Vector3.ProjectOnPlane(controller.position-pivot,Vector3.up);float height=controller.position.y-pivot.y;
                    var toward=Vector3.ProjectOnPlane(origin.Camera.transform.position-pivot,Vector3.up).normalized;
                    float pullSign=Vector3.Dot(Quaternion.AngleAxis(1,Vector3.up)*rest-rest,toward)>=0?1:-1;
                    for(float t=0;t<.35f&&Value<int>(state,"BoltsDone")==0;t+=Time.deltaTime)
                    {
                        controller.position=pivot+Quaternion.AngleAxis(pullSign*16*Mathf.Clamp01(t/.3f),Vector3.up)*rest+Vector3.up*height;
                        yield return null;
                        if(Value<bool>(socket,"IsSnapped"))Assert.Greater(Vector3.Dot(-grab.transform.forward,Vector3.ProjectOnPlane(controller.position-pivot,Vector3.up).normalized),.98f,"Tool fights hand direction");
                    }
                    if(Value<int>(state,"BoltsDone")>0)break;
                    float earned=progress();Assert.Greater(earned,before+5,"Small arm stroke did not accumulate");
                    grab.deactivated.Invoke(new DeactivateEventArgs{interactorObject=direct,interactableObject=grab});yield return new WaitForSeconds(.1f);
                    Assert.False(Value<bool>(socket,"IsSnapped"),"Trigger release did not free wrench");
                    controller.position=pivot+rest+Vector3.up*height;yield return new WaitForSeconds(.2f);
                    Assert.That(progress(),Is.EqualTo(earned).Within(.5f),"Trigger release/readjust lost or added progress");
                }
                Assert.Greater(Value<int>(state,"BoltsDone"),0,"Repeated small clutched pulls did not finish bolt");
                grab.deactivated.Invoke(new DeactivateEventArgs{interactorObject=direct,interactableObject=grab});
                controller.position+=Vector3.left*.55f;yield return new WaitForSeconds(.5f);
                direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return new WaitForSeconds(.1f);
                Assert.False(grab.GetComponent<Rigidbody>().isKinematic,"Released wrench floats");
                float settleDeadline=Time.realtimeSinceStartup+4f;
                while(grab.transform.position.y>=.5f&&Time.realtimeSinceStartup<settleDeadline)yield return null;
                Assert.Less(grab.transform.position.y,.5f,"Wrench did not fall to floor/water");
            }
        }
        [UnityTest]public IEnumerator PipeBracketsTouchRealShellAndMarkingsFaceRoom()
        {
            yield return null;Physics.SyncTransforms();
            for(int i=0;i<3;i++)
            {
                var plate=GameObject.Find("Bracket wall plate "+i).transform;
                var hits=Physics.RaycastAll(plate.position+plate.forward*.03f,-plate.forward,.2f);
                var wall=hits.Where(h=>h.collider.name.StartsWith("UCX_SM_Wall_F1_")).OrderBy(h=>h.distance).First();
                Assert.Less(wall.distance-.03f,.03f,"Bracket does not reach shell wall");
                Assert.Less(Vector3.Angle(plate.forward,wall.normal),2);
                var chalk=GameObject.Find("Chalk coupling "+(char)('A'+i)).transform;
                Assert.Greater(Vector3.Dot(chalk.up,-wall.normal),.98f,"Chalk reads upside down from room centre");
            }
            Assert.Greater(Vector3.Dot(GameObject.Find("Pressure dial scale").transform.up,Vector3.up),.99f);
        }

    }
}
#endif
