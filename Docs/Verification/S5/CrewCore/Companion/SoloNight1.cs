using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.WatchDemo;
using LighthouseKeepers.Flood;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Editor
{
    public static class ImmersionSoloReplay
    {
        const string Key = "S4SoloReplay";
        static string Output;
        static readonly List<string> evidence = new();
        static readonly List<string> errors = new();
        static Camera preview;
        static WatchDemoController watch;
        static FloodController flood;
        static XROrigin rig;
        static IEnumerator loop;
        static CustomYieldInstruction pending;
        static int lastFrame;
        static double deadline;
        public static object Run(string output)
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode in the untouched lobby before running this replay.");
            EditorUtility.audioMasterMute=true;
            Output=output;Directory.CreateDirectory(Output);
            Application.runInBackground=true;Application.targetFrameRate=72;
            Application.logMessageReceived+=Log;evidence.Clear();errors.Clear();
            loop=Loop();pending=null;lastFrame=-1;deadline=EditorApplication.timeSinceStartup+1300;
            EditorApplication.update+=Tick;
            return new {started=true,output=Output};
        }
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup>deadline) { errors.Add("Capture timeout"); Finish(); return; }
            if(Time.frameCount==lastFrame || (pending!=null && pending.keepWaiting)) return;
            lastFrame=Time.frameCount;
            try
            {
                if (!loop.MoveNext()) { Finish(); return; }
                pending=loop.Current as CustomYieldInstruction;
            }
            catch(Exception e) { errors.Add(e.ToString()); Finish(); }
        }
        static void Log(string text, string trace, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(text + "\n" + trace); }
        static T Field<T>(object o, string name) => (T)o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(o);
        static void Require(bool condition, string text) { if (!condition) throw new InvalidOperationException(text); }
        static void Record(string step) => evidence.Add($"{step}: elapsed={watch.State.Elapsed:F3}s realtime={Time.realtimeSinceStartup:F3} day={watch.State.Day} repaired={watch.State.Repaired} water={flood.Height:F5}m");
        static IEnumerator Loop()
        {
            float end=Time.realtimeSinceStartup+60;
            while(!GameObject.Find("Play")&&Time.realtimeSinceStartup<end)yield return null;
            yield return new WaitForSecondsRealtime(1);
            Capture(Camera.main,"01-lobby-play-solo");
            LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=true;LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=1;
            GameObject.Find("Play").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("HostPlay").GetComponent<Button>().onClick.Invoke();
            while(!GameObject.Find("StartCrew").GetComponent<Button>().interactable && Time.realtimeSinceStartup<end)yield return null; GameObject.Find("StartCrew").GetComponent<Button>().onClick.Invoke();
            while(!(watch=Object.FindFirstObjectByType<WatchDemoController>())&&Time.realtimeSinceStartup<end)yield return null;
            Require(watch,"Day 3 shortcut failed");yield return null;
            rig=Object.FindFirstObjectByType<XROrigin>();flood=Object.FindFirstObjectByType<FloodController>();
            Require(watch.State.Day==1&&watch.State.Power==PowerState.On,"Day 1 initial state changed");
            typeof(WatchState).GetField("repairSeed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(watch.State,99);watch.State.Retry();
            typeof(WatchDemoController).GetMethod("BeginWatch",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(watch,null);
            evidence.Add("Deterministic test-only pipe order seed 99; real state and XRI interactions, no authored asset changes.");
            Require(Object.FindObjectsByType<TunnelingVignetteController>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==0,"Vignette");
            preview=new GameObject("M4 evidence camera").AddComponent<Camera>();preview.CopyFrom(Camera.main);preview.enabled=false;preview.tag="Untagged";preview.fieldOfView=65;preview.nearClipPlane=.03f;preview.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().requiresDepthOption=UnityEngine.Rendering.Universal.CameraOverrideOption.On;
            var hud=Camera.main.transform.Find("Watch camera HUD");// Keep HUD/fade visible for the requested night evidence.
            var gen=Object.FindFirstObjectByType<WatchGenerator>();
            Require(!GameObject.Find("Watch readout")&&!GameObject.Find("Repair feedback")&&!GameObject.Find("Sleep prompt"),"Floating guidance still active");
            var radio=Object.FindFirstObjectByType<WatchRadioConsole>();
            foreach(var m in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true))m.enabled=false;
            var hands=rig.GetComponentsInChildren<HandRig>(true);
            foreach(var hand in hands){var c=hand.transform.parent;c.gameObject.SetActive(true);foreach(var d in c.GetComponents<UnityEngine.InputSystem.XR.TrackedPoseDriver>())d.enabled=false;}
            var right=hands.Single(h=>h.Node==XRNode.LeftHand);var handObject=right.transform.parent;var direct=right.Direct;
            direct.selectInput=new XRInputButtonReader("M4 test grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);
            View(new Vector3(-.9f,1.65f,-8.4f),new Vector3(-3.44f,1.5f,-8.4f));Capture(preview,"02-day-one-house");Record("Day 1 from PLAY SOLO, power on");
            Require(watch.State.Day==1&&watch.State.Power==PowerState.On&&watch.State.Campaign,"Night 1 arrival failed");
            IEnumerator work;Record("Focused Night 3 replay; prior Nights 1–2 already passed");
            Record("START focused fast Night3 XRI replay; original paced actions passed before the late fuel exhaustion");
            Capture(Camera.main,"01-day3-house-pipe-first");
            yield return null; // arrival briefing, board, bench and measured House/F1 route
            work=Repair(handObject,direct,"day-three");while(work.MoveNext())yield return work.Current;
            Record("PIPE finished");
            var run=watch.GetComponent<ArmSwingRun>();Require(run,"Arm swing driver missing");
            Require(Field<HandRig>(run,"left")&&Field<HandRig>(run,"right")&&Field<UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.ContinuousMoveProvider>(run,"move"),"Arm swing failed to bind real gameplay rig/hands");
            evidence.Add("Arm swing driver bound to both real hands and locomotion provider; human gesture still pending.");
            for(int i=0;i<2200&&!watch.State.Complete;i++)watch.State.Tick(.1f);
            yield return null;Require(watch.CanSleep,"Night 1 could not sleep after safe preview ship");
            MoveHead(watch.BedPosition);yield return new WaitForSecondsRealtime(.3f);Require(watch.TrySleep(),"Night 1 bed failed");
            yield return new WaitForSecondsRealtime(1.5f);Capture(Camera.main,"night1-survived");yield return new WaitForSecondsRealtime(4);
            Require(watch.State.Day==2&&!watch.State.Failed,"Night 2 did not start");Record("Solo Night 1 reached Night 2");
        }
        static bool MoveHead(Vector3 target){var c=rig.GetComponent<CharacterController>();bool enabled=c&&c.enabled;if(c)c.enabled=false;bool moved=rig.MoveCameraToWorldLocation(target);Physics.SyncTransforms();if(c)c.enabled=enabled;Require(moved&&Vector3.Distance(rig.Camera.transform.position,target)<.02f,"Test head placement failed");return moved;}
        static IEnumerator AwaitShips(Transform hand,XRDirectInteractor direct)
        {
            if(watch.State.Ships==null)yield break;
            var station=Object.FindFirstObjectByType<WatchBeaconStation>();var wheel=station.Wheel;
            MoveHead(new Vector3(-3.25f,11.55f,0));
            float until=Time.realtimeSinceStartup+300;Time.timeScale=3;bool seated=false;int frames=0;
            while(!watch.State.Ships.Finished&&!watch.State.Failed&&Time.realtimeSinceStartup<until){
                if(watch.State.AimingShip>=0){
                    if(!seated){hand.SetPositionAndRotation(wheel.transform.position,wheel.transform.rotation);direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)wheel.GetComponent<XRSimpleInteractable>());seated=true;yield return null;}
                    float delta=(watch.State.ShipWheelTarget-Field<float>(wheel,"working"))*270;
                    hand.rotation=Quaternion.AngleAxis(Mathf.Clamp(delta,-10,10),wheel.transform.forward)*hand.rotation;
                    if(++frames==60){View(new Vector3(-3.25f,11.55f,0),new Vector3(-42,2,0));Capture(preview,"ship-wheel-tracking");}
                }
                if(watch.State.Beacon!=null&&watch.State.Beacon.Soot>.4f){
                    if(seated){direct.EndManualInteraction();direct.selectInput.manualPerformed=false;seated=false;}
                    Time.timeScale=1;var rag=station.Rag;
                    hand.SetPositionAndRotation(rag.transform.position,rag.transform.rotation);direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)rag.Grab);yield return new WaitForSecondsRealtime(.3f);
                    for(int n=0;n<65&&watch.State.Beacon.Soot>.15f;n++){hand.position+=station.LensPoint+Vector3.right*(n%2==0?.045f:-.045f)-rag.Tip.position;yield return new WaitForSecondsRealtime(.12f);}
                    direct.EndManualInteraction();direct.selectInput.manualPerformed=false;Record("Rag wiped lens during ship watch");Time.timeScale=3;
                }
                yield return null;
            }
            if(seated){direct.EndManualInteraction();direct.selectInput.manualPerformed=false;}
            Time.timeScale=1;Require(!watch.State.Failed&&watch.State.Ships.Finished,"Authored ships did not pass: "+watch.State.FailureMessage);Record("All authored ships passed");
            var ship=Object.FindObjectsByType<LighthouseKeepers.Presentation.Environment.SteamerView>(FindObjectsSortMode.None).FirstOrDefault();
            if(ship){View(new Vector3(-3.6f,11.55f,-.6f),ship.transform.position+Vector3.up*4);Capture(preview,"night-"+watch.State.Day+"-ship-passed");}
        }
        static IEnumerator Generator(WatchGenerator gen,Transform handObject,XRDirectInteractor direct)
        {
            var controls=Object.FindFirstObjectByType<WatchEngineControls>();View(gen.transform.TransformPoint(new Vector3(.2f,1.6f,2)),gen.transform.position+Vector3.up);Capture(preview,"generator-step-fuel-or-prime");
            if(!watch.State.Engine.EnoughFuel)
            {
                var can=controls.FuelCan;handObject.SetPositionAndRotation(can.transform.TransformPoint(new Vector3(0,.30f,0)),can.transform.rotation);direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)can.Grab);yield return new WaitForSecondsRealtime(.3f);
                handObject.rotation=Quaternion.Euler(0,gen.transform.eulerAngles.y,105);yield return new WaitForSecondsRealtime(.3f);
                Require(can.Grab.isSelected,"Can was not grabbed");for(int i=0;i<8;i++){handObject.position+=Vector3.ClampMagnitude(controls.Filler.position+Vector3.up*.06f-can.Tip.position,.20f);yield return new WaitForSecondsRealtime(.1f);}
                float fillDeadline=Time.realtimeSinceStartup+15;while(watch.State.Engine.Fuel<.72f&&Time.realtimeSinceStartup<fillDeadline){handObject.position+=Vector3.ClampMagnitude(controls.Filler.position+Vector3.up*.04f-can.Tip.position,.04f);yield return new WaitForSecondsRealtime(.1f);}Require(watch.State.Engine.Fuel>.65f,"Fuel can refill failed");
                View(gen.transform.TransformPoint(new Vector3(.6f,1.65f,2.3f)),controls.Filler.position);Capture(preview,"fuel-pour-day-"+watch.State.Day);
                direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return new WaitForSecondsRealtime(.3f);
            }
            MoveHead(gen.transform.TransformPoint(new Vector3(0,1.65f,1.5f)));
            handObject.position=controls.Primer.transform.position+controls.Primer.transform.forward*.08f;
            var feed=direct.gameObject.AddComponent<PrimerInputFeed>();feed.direct=direct;
            for(int i=0;i<3;i++){
                feed.value=1;direct.selectInput.manualValue=1;direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)controls.Primer.GetComponent<XRSimpleInteractable>());
                yield return new WaitForSecondsRealtime(.4f);Require(watch.State.Engine.Primer==i+1,"Grip squeeze "+i);
                feed.value=0;direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return new WaitForSecondsRealtime(.35f);
            }
            Object.Destroy(feed);Require(watch.State.Engine.Primed,"Primer sequence failed");yield return null;Capture(preview,"generator-step-close-choke");
            var action=Press(controls.Choke,handObject,direct);while(action.MoveNext())yield return action.Current;
            yield return null;Capture(preview,"generator-step-pull-starter");var starter=gen.Starter;var start=starter.transform.position;var forward=gen.transform.forward;
            MoveHead(gen.transform.TransformPoint(new Vector3(.24f,1.65f,1.35f)));
            foreach(var testHand in rig.GetComponentsInChildren<HandRig>(true))
            {
                var control=testHand.transform.parent;control.SetPositionAndRotation(start,Quaternion.LookRotation(-forward));
                var grab=testHand.Direct;grab.selectInput.manualPerformed=true;grab.selectInput.manualValue=1;
                grab.StartManualInteraction((IXRSelectInteractable)starter.GetComponent<XRSimpleInteractable>());
                yield return new WaitForSecondsRealtime(.4f);
                View(gen.transform.TransformPoint(new Vector3(.24f,1.55f,1.25f)),starter.transform.position);Capture(preview,"starter-"+testHand.Node+"-first-person");
                View(gen.transform.TransformPoint(new Vector3(.75f,1.38f,1f)),starter.transform.position);Capture(preview,"starter-"+testHand.Node+"-side");
                grab.EndManualInteraction();grab.selectInput.manualPerformed=false;testHand.transform.parent.position=gen.transform.position+Vector3.down*10;yield return new WaitForSecondsRealtime(.3f);
            }
            handObject.SetPositionAndRotation(start,Quaternion.LookRotation(-forward));
            direct.selectInput.manualPerformed=true;direct.selectInput.manualValue=1;direct.StartManualInteraction((IXRSelectInteractable)starter.GetComponent<XRSimpleInteractable>());
            yield return new WaitForSecondsRealtime(.2f);
            for(int pull=1;pull<=3;pull++){handObject.position=start;yield return new WaitForSecondsRealtime(.2f);handObject.position=start+forward*.31f;yield return new WaitForSecondsRealtime(.3f);Require(watch.State.StarterPulls==pull,"Generator stroke regression");}
            direct.EndManualInteraction();direct.selectInput.manualPerformed=false;
            action=Press(controls.Choke,handObject,direct);while(action.MoveNext())yield return action.Current;
            yield return new WaitForSecondsRealtime(1.8f);Require(watch.State.Engine.Settled,"Choke open failed");
            Require(watch.State.Power==PowerState.On,"Power not restored");
            var e=Object.FindFirstObjectByType<EmergencyLighting>();Require(e.WarmBlend==1&&!Field<AudioSource>(e,"alarm").isPlaying,"Warm fade/alarm regression");Record("Three actual generator XRI pulls");
        }
        static IEnumerator Press(Component control,Transform hand,XRDirectInteractor direct)
        {hand.position=control.transform.position+control.transform.forward*.07f;direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)control.GetComponent<XRSimpleInteractable>());yield return new WaitForSecondsRealtime(.2f);Require(control.GetComponent<XRSimpleInteractable>().isSelected,"XRI control "+control.name);if(control is EngineControl){bool before=watch.State.Engine.ChokeClosed;hand.position+=Vector3.up*(before?-.18f:.18f);yield return new WaitForSecondsRealtime(.3f);Require(watch.State.Engine.ChokeClosed!=before,"Physical choke detent not reached");}direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return new WaitForSecondsRealtime(.2f);}
        static IEnumerator Beacon(Transform hand,XRDirectInteractor direct)
        {
            var station=Object.FindFirstObjectByType<WatchBeaconStation>();MoveHead(new Vector3(-3.25f,11.55f,0));
            var e=Press(station.Ignition,hand,direct);while(e.MoveNext())yield return e.Current;
            var wheel=station.Wheel;hand.SetPositionAndRotation(wheel.transform.position,wheel.transform.rotation);direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)wheel.GetComponent<XRSimpleInteractable>());yield return new WaitForSecondsRealtime(.2f);
            var rotor=Object.FindFirstObjectByType<WatchBeacon>().rotor;var rotorStart=rotor.rotation;var initial=hand.rotation;float turn=(.5f-watch.State.Beacon.Alignment)*270;
            for(int i=1;i<=60;i++){hand.rotation=Quaternion.AngleAxis(turn*i/60,wheel.transform.forward)*initial;yield return null;}
            direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return new WaitForSecondsRealtime(.3f);Require(watch.State.BeaconShining,"Beacon wheel alignment failed");Require(Quaternion.Angle(rotorStart,rotor.rotation)>60,"Wheel did not rotate actual lens");
            View(station.transform.TransformPoint(new Vector3(.1f,1.4f,1.45f)),station.transform.TransformPoint(new Vector3(0,1.25f,.2f)));Capture(preview,"beacon-aligned");
            var beam=Object.FindFirstObjectByType<WatchBeacon>().beam.transform;View(beam.position+beam.right*8+Vector3.up*2,beam.position+beam.forward*8);Capture(preview,"beacon-outside-sweep");
        }
        static IEnumerator Repair(Transform hand,XRDirectInteractor direct,string prefix)
        {
            var pipe=Object.FindFirstObjectByType<WatchPipeRepair>();var state=watch.State.Pipe;
            Require(watch.State.PipeReady,"Solo pipe must work without clamp");
            var valve=Object.FindFirstObjectByType<WatchPressureValve>();
            Require(!valve.Grip.enabled,"Solo valve must not be selectable");
            Require(!valve.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&r.gameObject.activeInHierarchy),"Solo valve must not be visible");
            Record("Solo repair ready without clamp; valve hidden");

            MoveHead(new Vector3(2.9f,1.65f,-.5f));
            View(new Vector3(.4f,1.65f,-2.5f),new Vector3(3.75f,1.05f,0));Capture(preview,prefix+"-pipe-on-wall");
            View(pipe.ActivePosition+new Vector3(-.3f,.3f,-.15f),pipe.ActivePosition);Capture(preview,prefix+"-hex-and-steam");
            View(new Vector3(2.8f,1.6f,pipe.ActivePosition.z),pipe.ActivePosition);Capture(preview,prefix+"-markings-from-approach");
            View(new Vector3(2.8f,1.35f,.43f),new Vector3(3.64f,1.05f,.43f));Capture(preview,prefix+"-gauge-and-clutch-plate");
            hand.position=pipe.wrench.transform.position;direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)pipe.wrench);yield return new WaitForSecondsRealtime(.3f);
            Require(pipe.wrench.isSelected,"Wrench grab failed");
            for(int align=0;align<2;align++){hand.rotation=Quaternion.LookRotation(-(Quaternion.AngleAxis(45,Vector3.up)*Vector3.ProjectOnPlane(rig.Camera.transform.position-pipe.ActivePosition,Vector3.up)),Vector3.up)*Quaternion.Inverse(pipe.wrench.transform.rotation)*hand.rotation;yield return new WaitForSecondsRealtime(.25f);}
            for(int bolt=0;bolt<3;bolt++)
            {
                if(bolt==1)
                {
                    hand.position+=Vector3.left*.45f;yield return new WaitForSecondsRealtime(.4f);
                    direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return new WaitForSecondsRealtime(1.2f);
                    var dropped=pipe.wrench.transform.position;View(new Vector3(dropped.x-.3f,Mathf.Min(dropped.y+.05f,flood.Height-.012f),dropped.z-.3f),dropped);Capture(preview,prefix+"-wrench-in-water");
                    Record("Released wrench: position="+dropped+" kinematic="+pipe.wrench.GetComponent<Rigidbody>().isKinematic+" water="+flood.Height);
                    var opposite=rig.GetComponentsInChildren<HandRig>(true).Single(h=>h.Node==XRNode.RightHand);hand=opposite.transform.parent;direct=opposite.Direct;
                    direct.selectInput=new XRInputButtonReader("Immersion right grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);
                    hand.position=pipe.wrench.transform.position;direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)pipe.wrench);yield return new WaitForSecondsRealtime(.2f);
                }
                hand.rotation=Quaternion.LookRotation(-(Quaternion.AngleAxis(45,Vector3.up)*Vector3.ProjectOnPlane(rig.Camera.transform.position-pipe.ActivePosition,Vector3.up)),Vector3.up)*Quaternion.Inverse(pipe.wrench.transform.rotation)*hand.rotation;yield return new WaitForSecondsRealtime(.25f);
                int active=state.ActiveBolt;var axis=pipe.bolts[active].forward;
                float stop=Time.realtimeSinceStartup+30;
                // Inject the same XRI trigger lifecycle used by the controller.
                while(state.SeatElapsed<.7f){hand.position+=Vector3.ClampMagnitude((pipe.ActivePosition-pipe.jaw.position)*.2f,.12f);yield return null;Require(Time.realtimeSinceStartup<stop,$"Cannot seat jaw contact={pipe.InContact} jaw={pipe.jaw.position} bolt={pipe.ActivePosition} selected={pipe.wrench.isSelected} snapped={pipe.wrench.GetComponent<ImmersionWrenchSocket>().IsSnapped} seat={state.SeatElapsed} transformers={pipe.wrench.singleGrabTransformersCount}");}
                float rest=Time.realtimeSinceStartup+(state.NeedsSeal?3.4f:1);
                while(Time.realtimeSinceStartup<rest){hand.position+=Vector3.ClampMagnitude((pipe.ActivePosition-pipe.jaw.position)*.2f,.12f);yield return null;}
                Require(state.TurnsDone==0,"Stationary wrench repaired itself");
                View(pipe.ActivePosition+new Vector3(-.45f,.45f,-.20f),pipe.ActivePosition);Capture(preview,prefix+"-bolt-"+(char)('A'+active)+"-seated");
                Capture(preview,prefix+"-wrench-"+direct.GetComponentInParent<HandRig>().Node+"-first-person");
                View(pipe.ActivePosition+new Vector3(-.2f,.35f,.45f),pipe.ActivePosition);Capture(preview,prefix+"-wrench-"+direct.GetComponentInParent<HandRig>().Node+"-side");
                bool turnedShot=false,midShot=false;
                while(state.BoltsDone==bolt)
                {
                    Vector3 pivot=pipe.ActivePosition,leverRest=hand.position-pivot;
                    var toward=Vector3.ProjectOnPlane(rig.Camera.transform.position-pivot,Vector3.up).normalized;
                    float pullSign=Vector3.Dot(Quaternion.AngleAxis(1,Vector3.up)*leverRest-leverRest,toward)>=0?1:-1;
                    for(float t=0;t<.45f&&state.BoltsDone==bolt;t+=Time.deltaTime)
                    {
                        float crankAngle=20*Mathf.Clamp01(t/.4f);hand.position=pivot+Quaternion.AngleAxis(pullSign*crankAngle,Vector3.up)*leverRest;
                        yield return null;
                        if(!midShot&&crankAngle>10)
                        {midShot=true;View(pivot+new Vector3(-.6f,.45f,-.25f),pivot);Capture(preview,prefix+"-mid-pull-front-"+direct.GetComponentInParent<HandRig>().Node);View(pivot+new Vector3(-.2f,.35f,.45f),pivot);Capture(preview,prefix+"-mid-pull-side-"+direct.GetComponentInParent<HandRig>().Node);}
                        if(state.TurnsDone>0&&!turnedShot){turnedShot=true;Capture(preview,prefix+"-wrench-"+direct.GetComponentInParent<HandRig>().Node+"-turned");}

                    }
                    yield return new WaitForSecondsRealtime(.2f);
                    if(state.BoltsDone!=bolt)break;
                    hand.position=pivot+leverRest;yield return new WaitForSecondsRealtime(.2f);
                    while(Vector3.Distance(pipe.jaw.position,pipe.ActivePosition)>.025f)
                    {hand.position+=Vector3.ClampMagnitude((pipe.ActivePosition-pipe.jaw.position)*.3f,.08f);yield return null;Require(Time.realtimeSinceStartup<stop,"Cannot re-seat clutch");}
                    Require(Time.realtimeSinceStartup<stop,$"Clutched repair stalled bolt {active} contact={pipe.InContact} turns={state.TurnsDone} stroke={state.StrokeDegrees}");
                }
                Require(state.InflowFraction<1,"Inflow did not fall");Record(prefix+" physical bolt "+active+" complete; inflow="+state.InflowFraction);
                yield return null;
                View(new Vector3(-.9f,1.65f,-8.4f),new Vector3(-3.44f,1.5f,-8.4f));Capture(preview,prefix+"-board-bolts-"+(bolt+1));
            }
            direct.EndManualInteraction();direct.selectInput.manualPerformed=false;Require(watch.State.Repaired,"Pipe task not checked");
        }
        static void View(Vector3 p, Vector3 target) { preview.transform.position=p; preview.transform.LookAt(target); }
        static void Capture(Camera camera,string name)
        {
            var old=camera.targetTexture; var active=RenderTexture.active; var target=new RenderTexture(1600,1000,24);
            camera.targetTexture=target; Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active=target;
            var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,target.width,target.height),0,0); image.Apply();
            File.WriteAllBytes(Output+"/"+name+".png",image.EncodeToPNG()); camera.targetTexture=old; RenderTexture.active=active;
            Object.Destroy(image);target.Release();Object.Destroy(target);
        }
        static void Finish()
        {
            EditorApplication.update-=Tick; Application.logMessageReceived-=Log; SessionState.SetBool(Key,false);

            File.WriteAllLines(Output+(evidence.Any(e=>e.StartsWith("HAND LIGHT"))?"/hand-lighting-evidence.txt":"/loop-evidence.txt"),evidence.Concat(errors));
            File.WriteAllText(Output+"/result.json", "{\"passed\":"+(errors.Count==0?"true":"false")+",\"errors\":"+errors.Count+"}");
            if(errors.Count==0)EditorApplication.isPlaying=false;
        }
    }
}
[DefaultExecutionOrder(50)] public sealed class PrimerInputFeed:MonoBehaviour {public XRDirectInteractor direct;public float value;void Update(){direct.selectInput.manualValue=value;direct.selectInput.manualPerformed=value>.5f;}}
