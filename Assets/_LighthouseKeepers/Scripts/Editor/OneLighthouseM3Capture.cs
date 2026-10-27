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
    public static class OneLighthouseM3Capture
    {
        const string Key = "OneLighthouseM3Capture";
        static string Output => OneLighthouseM3.Output;
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
        public static void ProbeHands() => Begin(true);
        public static void Run() { SessionState.SetBool(Key+"House",false); Begin(false); }
        public static void House() { SessionState.SetBool(Key+"House",true); Begin(false); }
        static void Begin(bool probeHands)
        {
            SessionState.SetBool(Key+"Hands",false);
            Directory.CreateDirectory(Output);
            OneLighthouseM3.Apply();
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
            loop=Loop(); pending=null; lastFrame=-1; deadline=EditorApplication.timeSinceStartup+300;
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
            while(!GameObject.Find("DevStartDay3")&&Time.realtimeSinceStartup<end)yield return null;
            yield return new WaitForSecondsRealtime(1);
            Capture(Camera.main,"01-lobby-day-three");
            GameObject.Find("DevStartDay3").GetComponent<Button>().onClick.Invoke();
            while(!(watch=Object.FindFirstObjectByType<WatchDemoController>())&&Time.realtimeSinceStartup<end)yield return null;
            Require(watch,"Day 3 shortcut failed");yield return null;
            rig=Object.FindFirstObjectByType<XROrigin>();flood=Object.FindFirstObjectByType<FloodController>();
            Require(watch.State.Day==3&&watch.State.Power==PowerState.Off,"Day 3 faults missing");
            Require(Object.FindObjectsByType<TunnelingVignetteController>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==0,"Vignette");
            preview=new GameObject("M3 evidence camera").AddComponent<Camera>();preview.CopyFrom(Camera.main);preview.enabled=false;preview.tag="Untagged";preview.fieldOfView=65;preview.nearClipPlane=.03f;
            var hud=Camera.main.transform.Find("Watch camera HUD");if(hud)foreach(var r in hud.GetComponentsInChildren<Renderer>())r.forceRenderingOff=true;
            View(new Vector3(-.9f,1.65f,-8.4f),new Vector3(-3.44f,1.5f,-8.4f));Capture(preview,"02-day-three-board");
            View(new Vector3(1.5f,1.65f,-2),new Vector3(.8f,1,.2f));Capture(preview,"03-red-emergency-f1");
            var gen=Object.FindFirstObjectByType<WatchGenerator>();
            View(new Vector3(-1.5f,4.95f,-1.55f),gen.transform.TransformPoint(new Vector3(0,1,.4f)));Capture(preview,"04-red-emergency-f2");
            var radio=Object.FindFirstObjectByType<WatchRadioConsole>();
            View(new Vector3(-.5f,11.35f,.95f),radio.transform.TransformPoint(new Vector3(0,1.4f,.3f)));Capture(preview,"05-radio-no-power");
            Require(!radio.ToggleMain()&&!watch.State.Radio.MainOn,"Console powered without mains");
            foreach(var m in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true))m.enabled=false;
            var hands=rig.GetComponentsInChildren<HandRig>(true);
            foreach(var hand in hands){var c=hand.transform.parent;c.gameObject.SetActive(true);foreach(var d in c.GetComponents<UnityEngine.InputSystem.XR.TrackedPoseDriver>())d.enabled=false;}
            var right=hands.Single(h=>h.Node==XRNode.RightHand);var handObject=right.transform.parent;var direct=right.Direct;
            direct.selectInput=new XRInputButtonReader("M3 test grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);
            var starter=gen.Starter;var start=starter.transform.position;var forward=gen.transform.forward;
            rig.MoveCameraToWorldLocation(gen.transform.TransformPoint(new Vector3(.24f,1.65f,1.35f)));
            handObject.SetPositionAndRotation(start,Quaternion.LookRotation(-forward));
            direct.selectInput.manualPerformed=true;direct.selectInput.manualValue=1;direct.StartManualInteraction((IXRSelectInteractable)starter.GetComponent<XRSimpleInteractable>());
            yield return new WaitForSecondsRealtime(.2f);
            for(int pull=1;pull<=3;pull++)
            {handObject.position=start;yield return new WaitForSecondsRealtime(.2f);handObject.position=start+forward*.31f;yield return new WaitForSecondsRealtime(.3f);Require(watch.State.StarterPulls==pull,"Generator stroke regression");}
            direct.EndManualInteraction();direct.selectInput.manualPerformed=false;
            yield return new WaitForSecondsRealtime(1.2f);
            Require(watch.State.Power==PowerState.On,"Power not restored");
            var emergency=Object.FindFirstObjectByType<EmergencyLighting>();Require(emergency.WarmBlend==1,"Warm fade incomplete");Require(!Field<AudioSource>(emergency,"alarm").isPlaying,"Alarm did not stop");
            View(new Vector3(-1.5f,4.95f,-1.55f),gen.transform.TransformPoint(new Vector3(0,1,.4f)));Capture(preview,"06-warm-f2");
            Record("Generator three actual XRI strokes; warm fade and alarm stop");
            rig.MoveCameraToWorldLocation(new Vector3(-.5f,11.55f,1.55f));
            View(new Vector3(1.263f,11.55f,-1.138f),radio.transform.TransformPoint(new Vector3(0,1.4f,.3f)));Capture(preview,"07-console-from-f4-stairs");
            View(new Vector3(-.5f,11.35f,.95f),radio.transform.TransformPoint(new Vector3(0,1.35f,.3f)));Capture(preview,"08-console-close-up");
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
            var wrench=Field<XRGrabInteractable>(watch,"wrench");rig.MoveCameraToWorldLocation(new Vector3(.8f,1.65f,-1.25f));
            handObject.position=Field<Transform>(watch,"repairPoint").position+new Vector3(0,0,-.15f);
            direct.selectInput.manualPerformed=true;direct.StartManualInteraction((IXRSelectInteractable)wrench);yield return new WaitForSecondsRealtime(.3f);
            Require(watch.TryBeginRepair(),"Wrench repair rejected");yield return new WaitForSecondsRealtime(3.8f);
            direct.EndManualInteraction();direct.selectInput.manualPerformed=false;
            Require(watch.State.Complete&&watch.State.CompletedTasks==3,"Day 3 not complete");
            while(flood.Height>flood.MinimumHeight+.001f)yield return null;
            View(new Vector3(-.9f,1.65f,-8.4f),new Vector3(-3.44f,1.5f,-8.4f));Capture(preview,"12-day-three-board-3-of-3");Record("Water drained; board 3/3");
            rig.MoveCameraToWorldLocation(new Vector3(1.25f,8.2f,-.3f));Require(watch.TrySleep(),"Day 3 bed rejected");yield return new WaitForSecondsRealtime(2.5f);
            Require(watch.State.Day==4&&watch.State.Power==PowerState.Off&&!watch.State.Radio.Completed,"Day 4 reset failed");
            Capture(preview,"13-day-four-board");Record("Bed -> authored Day 4 reset");
            evidence.Add("PASS: graphics Play Mode from DEV PLAY DAY 3; physical controller inputs injected in Editor, not headset usability or sound acceptance. Stair locomotion separately covered by ShellTraversalTests.");
        }
        static void FloodView() => View(new Vector3(2.0f,1.65f,-2.6f),new Vector3(.6f,.35f,.2f));
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
