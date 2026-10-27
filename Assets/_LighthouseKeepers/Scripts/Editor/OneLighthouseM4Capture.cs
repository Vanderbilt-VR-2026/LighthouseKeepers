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
    public static class OneLighthouseM4Capture
    {
        const string Key = "OneLighthouseM4Capture";
        static string Output => OneLighthouseM4.Output;
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
        [InitializeOnLoadMethod] static void Register() => EditorApplication.playModeStateChanged += Changed;
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            OneLighthouseM4.Apply();
            SessionState.SetString(Key + "Start", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(LighthouseNetworkLobby.ScenePath);
            EditorSceneManager.OpenScene(LighthouseNetworkLobby.ScenePath);
            SessionState.SetBool(Key, true);
            EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }
        static void Changed(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Key, false)) return;
            Application.runInBackground = true; Application.targetFrameRate = 72;
            Application.logMessageReceived += Log; evidence.Clear(); errors.Clear();
            loop=Loop(); pending=null; lastFrame=-1; deadline=EditorApplication.timeSinceStartup+480;
            EditorApplication.update += Tick;
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
            while(!GameObject.Find("DevStartDay2")&&Time.realtimeSinceStartup<end)yield return null;
            yield return new WaitForSecondsRealtime(1);
            Capture(Camera.main,"01-lobby-day-two");
            GameObject.Find("DevStartDay2").GetComponent<Button>().onClick.Invoke();
            while(!(watch=Object.FindFirstObjectByType<WatchDemoController>())&&Time.realtimeSinceStartup<end)yield return null;
            Require(watch,"Day 3 shortcut failed");yield return null;
            rig=Object.FindFirstObjectByType<XROrigin>();flood=Object.FindFirstObjectByType<FloodController>();
            Require(watch.State.Day==2&&watch.State.Power==PowerState.Off,"Day 3 faults missing");
            Require(Object.FindObjectsByType<TunnelingVignetteController>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==0,"Vignette");
            preview=new GameObject("M4 evidence camera").AddComponent<Camera>();preview.CopyFrom(Camera.main);preview.enabled=false;preview.tag="Untagged";preview.fieldOfView=65;preview.nearClipPlane=.03f;
            var hud=Camera.main.transform.Find("Watch camera HUD");if(hud)foreach(var r in hud.GetComponentsInChildren<Renderer>())r.forceRenderingOff=true;
            View(new Vector3(-.9f,1.65f,-8.4f),new Vector3(-3.44f,1.5f,-8.4f));Capture(preview,"02-day-two-board");
            View(new Vector3(1.5f,1.65f,-2),new Vector3(.8f,1,.2f));Capture(preview,"03-soft-emergency-f1");
            var gen=Object.FindFirstObjectByType<WatchGenerator>();
            View(new Vector3(-1.5f,4.95f,-1.55f),gen.transform.TransformPoint(new Vector3(0,1,.4f)));Capture(preview,"04-soft-emergency-f2");
            var radio=Object.FindFirstObjectByType<WatchRadioConsole>();
            View(new Vector3(-.5f,11.4f,.65f),radio.transform.TransformPoint(new Vector3(0,1.4f,.3f)));Capture(preview,"05-soft-emergency-f4");
            Require(!radio.ToggleMain()&&!watch.State.Radio.MainOn,"Console powered without mains");
            foreach(var m in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true))m.enabled=false;
            var hands=rig.GetComponentsInChildren<HandRig>(true);
            foreach(var hand in hands){var c=hand.transform.parent;c.gameObject.SetActive(true);foreach(var d in c.GetComponents<UnityEngine.InputSystem.XR.TrackedPoseDriver>())d.enabled=false;}
            var right=hands.Single(h=>h.Node==XRNode.RightHand);var handObject=right.transform.parent;var direct=right.Direct;
            direct.selectInput=new XRInputButtonReader("M4 test grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);
            var beacon=Object.FindFirstObjectByType<WatchBeacon>();
            Require(!beacon.Lit&&!beacon.beam.enabled,"Outage beacon still lit");
            View(new Vector3(1.6f,11.55f,-1.25f),beacon.transform.position+Vector3.up*1.2f);Capture(preview,"05b-dark-beacon");
            var emergency=Object.FindFirstObjectByType<EmergencyLighting>();
            yield return new WaitForSecondsRealtime(10.2f);
            Require(Field<AudioSource>(emergency,"alarm").volume<.025f,"Alarm did not fade to hum");
            var work=Generator(gen,handObject,direct);while(work.MoveNext())yield return work.Current;
            Require(beacon.Lit&&beacon.beam.enabled&&beacon.SpinFraction>.95f,"Beacon did not spin up");
            Capture(preview,"06-powered-beacon");
            work=Repair(handObject,direct,"day-two");while(work.MoveNext())yield return work.Current;
            Require(watch.State.Complete,"Day 2 incomplete");
            View(new Vector3(-.9f,1.65f,-8.4f),new Vector3(-3.44f,1.5f,-8.4f));Capture(preview,"06b-day-two-complete");
            rig.MoveCameraToWorldLocation(new Vector3(1.25f,8.2f,-.3f));Require(watch.TrySleep(),"Day 2 sleep rejected");yield return new WaitForSecondsRealtime(2.5f);
            Require(watch.State.Day==3&&watch.State.Power==PowerState.Off,"Day 3 faults missing");
            Capture(preview,"06c-day-three-dark-countdown");
            // Let the real day clock expire, rather than stepping the model or shortening data.
            while(!watch.State.Failed)yield return null;
            Require(watch.State.BeaconDarkFailed,"Wrong failure reason");Capture(preview,"06d-beacon-dark-failure");Record("Real 75-second dark beacon loss");
            Require(watch.RetryDay()&&watch.State.Day==3&&!watch.State.Failed,"Same-day retry failed");yield return new WaitForSecondsRealtime(.3f);
            Capture(preview,"06e-day-three-retry");
            work=Generator(gen,handObject,direct);while(work.MoveNext())yield return work.Current;
            View(new Vector3(-1.5f,4.95f,-1.55f),gen.transform.TransformPoint(new Vector3(0,1,.4f)));Capture(preview,"06f-warm-f2");
            rig.MoveCameraToWorldLocation(new Vector3(-.5f,11.55f,1.55f));
            View(new Vector3(1.263f,11.55f,-1.138f),radio.transform.TransformPoint(new Vector3(0,1.4f,.3f)));Capture(preview,"07-console-from-f4-stairs");
            View(new Vector3(-.5f,11.4f,.65f),radio.transform.TransformPoint(new Vector3(0,1.55f,.3f)));Capture(preview,"08-console-close-up");
            var main=radio.controls.Single(c=>c.mainPower);handObject.SetPositionAndRotation(main.transform.position,Quaternion.LookRotation(-radio.transform.forward));
            direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)main.GetComponent<XRSimpleInteractable>());yield return new WaitForSecondsRealtime(.2f);
            Require(watch.State.Radio.MainOn,"MAIN XRI failed");direct.EndManualInteraction();direct.selectInput.manualPerformed=false;
            var knob=radio.controls.Single(c=>c.frequency);handObject.position=knob.transform.position;var rotation=handObject.rotation;
            direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)knob.GetComponent<XRSimpleInteractable>());yield return new WaitForSecondsRealtime(.2f);
            float target=(radio.program.targetMHz-radio.program.minimumMHz)/(radio.program.maximumMHz-radio.program.minimumMHz)*270;
            for(float t=0;t<1;t+=Time.deltaTime){handObject.rotation=Quaternion.AngleAxis(target*t,radio.transform.forward)*rotation;yield return null;}
            handObject.rotation=Quaternion.AngleAxis(target,radio.transform.forward)*rotation;yield return new WaitForSecondsRealtime(1.2f);
            Require(watch.State.Radio.Locked,"Wrist tuning did not lock: "+watch.State.Radio.FrequencyMHz);direct.EndManualInteraction();direct.selectInput.manualPerformed=false;
            Capture(preview,"09-frequency-locked");
            foreach(int colour in new[]{1,0,2,1,3})
            {
                var button=radio.controls.Single(c=>c.codeColour==colour);handObject.position=button.transform.position;
                direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)button.GetComponent<XRSimpleInteractable>());yield return new WaitForSecondsRealtime(.15f);
                direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return null;
                if(colour==1&&!watch.State.Radio.CodeAccepted&&watch.State.Radio.CodeProgress==0)Require(watch.State.Radio.Locked,"Wrong code reset tuning");
            }
            Require(watch.State.Radio.CodeAccepted,"Code controls failed");
            var mic=radio.microphone.GetComponent<XRGrabInteractable>();handObject.position=mic.transform.position;
            direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)mic);yield return new WaitForSecondsRealtime(.2f);Require(mic.isSelected,"Mic not held");
            handObject.position=new Vector3(-.65f,11.35f,1.55f);
            mic.activated.Invoke(new ActivateEventArgs{interactorObject=direct,interactableObject=mic});yield return new WaitForSecondsRealtime(1);
            Require(watch.State.Radio.Transmitting,"PTT activation failed");Capture(preview,"10-transmitting");
            mic.deactivated.Invoke(new DeactivateEventArgs{interactorObject=direct,interactableObject=mic});yield return new WaitForSecondsRealtime(.2f);
            Require(watch.State.Radio.TransmitElapsed==0&&!watch.State.Radio.Completed,"Partial transmission survived trigger release");
            mic.activated.Invoke(new ActivateEventArgs{interactorObject=direct,interactableObject=mic});yield return new WaitForSecondsRealtime(3.2f);
            Require(watch.State.Radio.Completed&&watch.State.CompletedTasks==2,"Broadcast completion failed");
            mic.deactivated.Invoke(new DeactivateEventArgs{interactorObject=direct,interactableObject=mic});direct.EndManualInteraction();direct.selectInput.manualPerformed=false;
            Capture(preview,"11-broadcast-confirmed");Record("Actual XRI main/tuning/code/mic and activated/deactivated PTT; wrong code and released PTT reset only unfinished step");
            work=Repair(handObject,direct,"day-three");while(work.MoveNext())yield return work.Current;
            Require(watch.State.Complete&&watch.State.CompletedTasks==3,"Day 3 not complete");
            while(flood.Height>flood.MinimumHeight+.001f)yield return null;
            View(new Vector3(-.9f,1.65f,-8.4f),new Vector3(-3.44f,1.5f,-8.4f));Capture(preview,"12-day-three-board-3-of-3");Record("Water drained; board 3/3");
            rig.MoveCameraToWorldLocation(new Vector3(1.25f,8.2f,-.3f));Require(watch.TrySleep(),"Day 3 bed rejected");yield return new WaitForSecondsRealtime(2.5f);
            Require(watch.State.Day==4&&watch.State.Power==PowerState.Off&&!watch.State.Radio.Completed,"Day 4 reset failed");
            Capture(preview,"13-day-four-board");Record("Bed -> authored Day 4 reset");
            evidence.Add("PASS: graphics Play Mode from DEV PLAY DAY 2 through Day 3 loss/retry and Day 4; physical controller inputs injected in Editor, not headset usability or sound acceptance. Stair locomotion separately covered by ShellTraversalTests.");
        }
        static IEnumerator Generator(WatchGenerator gen,Transform handObject,XRDirectInteractor direct)
        {
            var starter=gen.Starter;var start=starter.transform.position;var forward=gen.transform.forward;
            rig.MoveCameraToWorldLocation(gen.transform.TransformPoint(new Vector3(.24f,1.65f,1.35f)));
            handObject.SetPositionAndRotation(start,Quaternion.LookRotation(-forward));
            direct.selectInput.manualPerformed=true;direct.selectInput.manualValue=1;direct.StartManualInteraction((IXRSelectInteractable)starter.GetComponent<XRSimpleInteractable>());
            yield return new WaitForSecondsRealtime(.2f);
            for(int pull=1;pull<=3;pull++){handObject.position=start;yield return new WaitForSecondsRealtime(.2f);handObject.position=start+forward*.31f;yield return new WaitForSecondsRealtime(.3f);Require(watch.State.StarterPulls==pull,"Generator stroke regression");}
            direct.EndManualInteraction();direct.selectInput.manualPerformed=false;yield return new WaitForSecondsRealtime(1.8f);
            Require(watch.State.Power==PowerState.On,"Power not restored");
            var e=Object.FindFirstObjectByType<EmergencyLighting>();Require(e.WarmBlend==1&&!Field<AudioSource>(e,"alarm").isPlaying,"Warm fade/alarm regression");Record("Three actual generator XRI pulls");
        }
        static IEnumerator Repair(Transform hand,XRDirectInteractor direct,string prefix)
        {
            var pipe=Object.FindFirstObjectByType<WatchPipeRepair>();var state=watch.State.Pipe;
            rig.MoveCameraToWorldLocation(new Vector3(.8f,1.65f,-1.25f));
            hand.position=pipe.wrench.transform.position;direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)pipe.wrench);yield return new WaitForSecondsRealtime(.3f);
            Require(pipe.wrench.isSelected,"Wrench grab failed");
            for(int bolt=0;bolt<3;bolt++)
            {
                int active=state.ActiveBolt;var axis=pipe.bolts[active].forward;
                float stop=Time.realtimeSinceStartup+30;
                // Move the real controller, keeping the jaw seated as the tool follows its palm socket.
                while(state.SeatElapsed<.7f){hand.position+=Vector3.ClampMagnitude((pipe.ActivePosition-pipe.jaw.position)*.2f,.12f);yield return null;Require(Time.realtimeSinceStartup<stop,"Cannot seat jaw");}
                float rest=Time.realtimeSinceStartup+(state.NeedsSeal?3.4f:1);
                while(Time.realtimeSinceStartup<rest){hand.position+=Vector3.ClampMagnitude((pipe.ActivePosition-pipe.jaw.position)*.2f,.12f);yield return null;}
                Require(state.TurnsDone==0,"Stationary wrench repaired itself");
                View(new Vector3(.8f,1.6f,-1.8f),pipe.ActivePosition);Capture(preview,prefix+"-bolt-"+(char)('A'+active)+"-seated");
                while(state.BoltsDone==bolt)
                {
                    float sign=state.NeedsReturn?-1:1;
                    hand.rotation=Quaternion.AngleAxis(70*Time.deltaTime*sign,axis)*hand.rotation;
                    hand.position+=Vector3.ClampMagnitude((pipe.ActivePosition-pipe.jaw.position)*.2f,.12f);yield return null;
                    Require(Time.realtimeSinceStartup<stop,$"Physical repair stalled bolt {active} contact={pipe.InContact} turns={state.TurnsDone} stroke={state.StrokeDegrees} seal={state.SealElapsed} return={state.NeedsReturn}");
                }
                Require(Mathf.Abs(state.InflowFraction-(2-bolt)/3f)<.001f,"Inflow not stepped");Record(prefix+" physical bolt "+active+" complete; inflow="+state.InflowFraction);
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
            var path=SessionState.GetString(Key+"Start",""); EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(path)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            File.WriteAllLines(Output+(evidence.Any(e=>e.StartsWith("HAND LIGHT"))?"/hand-lighting-evidence.txt":"/loop-evidence.txt"),evidence.Concat(errors));
            EditorApplication.Exit(errors.Count==0?0:1);
        }
    }
}
