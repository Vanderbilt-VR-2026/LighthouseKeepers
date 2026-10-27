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
    public static class OneLighthouseLoopCapture
    {
        const string Key = "OneLighthouseM15Capture", Output = OneLighthouseM15.Output;
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
        public static void Run() => Begin(false);
        static void Begin(bool probeHands)
        {
            SessionState.SetBool(Key+"Hands",probeHands);
            Directory.CreateDirectory(Output);
            OneLighthouseM15.Apply();
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
            loop=Loop(); pending=null; lastFrame=-1; deadline=EditorApplication.timeSinceStartup+240;
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
            float deadline = Time.realtimeSinceStartup + 60;
            while (!GameObject.Find("PlaySolo") && Time.realtimeSinceStartup < deadline) yield return null;
            yield return new WaitForSecondsRealtime(1);
            Capture(Camera.main, "01-lobby");
            GameObject.Find("PlaySolo").GetComponent<Button>().onClick.Invoke();
            while (!(watch = Object.FindFirstObjectByType<WatchDemoController>()) && Time.realtimeSinceStartup < deadline) yield return null;
            Require(watch, "Lobby failed to load WatchDemo");
            yield return null;
            rig = Object.FindFirstObjectByType<XROrigin>(); flood = Object.FindFirstObjectByType<FloodController>();
            Require(SceneManager.sceneCount == 1 && watch.gameObject.scene.name == "LK_Lab_Environment", "Wrong world");
            Require(Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None).Length == 1, "Duplicate rig");
            Require(Object.FindObjectsByType<TunnelingVignetteController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0, "Vignette present");
            preview = new GameObject("M1.5 evidence camera").AddComponent<Camera>(); preview.CopyFrom(Camera.main);
            preview.enabled = false; preview.tag = "Untagged"; preview.fieldOfView = 65; preview.nearClipPlane = .03f;
            Record("flood t=0"); FloodView(); Capture(preview, "04-flood-t000");
            Require(Camera.main.transform.position.y-rig.transform.position.y < 2f, "Double standing height at House spawn");
            Capture(Camera.main, "02-house-spawn");
            var hud = Camera.main.transform.Find("Watch camera HUD");
            if (hud) foreach (var r in hud.GetComponentsInChildren<Renderer>()) r.forceRenderingOff = true;
            View(new Vector3(-2.15f,1.65f,-9.2f), new Vector3(-2.2f,1.45f,-7.5f)); Capture(preview, "03-keeper-watch-board");
            evidence.Add("House head=" + Camera.main.transform.position + "; board=" + Field<TextMesh>(watch,"board").text.Replace('\n','|'));
            foreach (var modality in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true)) modality.enabled = false;
            var hands = rig.GetComponentsInChildren<HandRig>(true);
            Require(hands.Length == 2, "Expected two Sprint 3 hands");
            foreach (var hand in hands)
            {
                var controller = hand.transform.parent; controller.gameObject.SetActive(true);
                foreach (var driver in controller.GetComponents<UnityEngine.InputSystem.XR.TrackedPoseDriver>()) driver.enabled = false;
                controller.SetPositionAndRotation(Camera.main.transform.TransformPoint(new Vector3(hand.Node == XRNode.LeftHand ? -.16f : .16f, -.16f, .42f)), Camera.main.transform.rotation);
            }
            yield return new WaitForSecondsRealtime(.2f);
            Capture(Camera.main, "05-hands-desktop-grip-reference");
            if(SessionState.GetBool(Key+"Hands",false))
            {
                SessionState.SetBool(Key+"Hands",false);
                foreach(var hand in hands)
                {
                    var skin=hand.GetComponentInChildren<SkinnedMeshRenderer>();
                    UnityEngine.Rendering.SphericalHarmonicsL2 sh;
                    LightProbes.GetInterpolatedProbe(skin.bounds.center,null,out sh);
                    var directions=new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back};
                    var values=new Color[6];sh.Evaluate(directions,values);
                    evidence.Add("HAND LIGHT "+hand.Node+" center="+skin.bounds.center+" probe="+skin.lightProbeUsage+" material="+skin.sharedMaterial.name+" base="+skin.sharedMaterial.GetColor("_BaseColor")+" emission="+skin.sharedMaterial.GetColor("_EmissionColor")+" SH="+string.Join(",",values.Select(v=>v.ToString("F3"))));
                    skin.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;
                }
                yield return new WaitForSecondsRealtime(.3f);
                Capture(Camera.main,"hands-probe-off");
                yield break;
            }
            var wrench = Field<XRGrabInteractable>(watch, "wrench");
            Require(!wrench.GetComponent<WrenchInteraction>() && !wrench.GetComponent<PhysicalGrip>(), "Sprint 3 installer replaced WatchDemo wrench behavior");
            var handObject = new GameObject("Editor injected direct grip");
            handObject.SetActive(false); handObject.transform.position = wrench.transform.position;
            var collider = handObject.AddComponent<SphereCollider>(); collider.isTrigger = true; collider.radius = .08f;
            var handBody = handObject.AddComponent<Rigidbody>(); handBody.isKinematic = true; handBody.useGravity = false;
            var direct = handObject.AddComponent<XRDirectInteractor>();
            direct.selectInput = new XRInputButtonReader("Editor grip", inputSourceMode: XRInputButtonReader.InputSourceMode.ManualValue);
            direct.selectInput.manualPerformed = true; direct.selectInput.manualValue = 1;
            handObject.SetActive(true); yield return null;
            var manager = Object.FindFirstObjectByType<XRInteractionManager>();
            manager.SelectEnter((IXRSelectInteractor)direct, (IXRSelectInteractable)wrench);
            yield return new WaitForSecondsRealtime(.2f);
            Require(wrench.isSelected, "Wrench not selected through XRI");
            var right = hands.Single(h=>h.Node == XRNode.RightHand);
            right.transform.parent.SetPositionAndRotation(handObject.transform.position, handObject.transform.rotation);
            right.SetPose(HandPose.WrapGrip, direct);
            View(new Vector3(-1.35f,1.55f,-9.0f), wrench.transform.position); Capture(preview,"06-real-wrench-grabbed"); Record("grab");
            Vector3 from = handObject.transform.position, to = Field<Transform>(watch,"repairPoint").position + new Vector3(0,0,-.15f);
            for (float t=0; t<3; t+=Time.deltaTime)
            {
                handObject.transform.position = Vector3.Lerp(from,to,t/3);
                right.transform.parent.position = handObject.transform.position;
                yield return null;
            }
            handObject.transform.position = to; right.transform.parent.position = to;
            rig.MoveCameraToWorldLocation(new Vector3(.8f,1.65f,-1.25f));
            View(new Vector3(1.8f,1.65f,-1.5f), new Vector3(.8f,1.05f,.15f));
            yield return new WaitForSecondsRealtime(.2f);
            Capture(preview,"07-carried-to-pipe"); Record("carried, no trigger yet");
            while (watch.State.Elapsed < 60) yield return null;
            Require(!watch.State.Repaired && flood.Height > .09f, "Unrepaired flood did not rise");
            FloodView(); Capture(preview,"08-flood-t060-unrepaired"); Record("flood t=60");
            Require(watch.TryBeginRepair(), "Held wrench failed repair trigger handler");
            var visual = Field<Transform>(watch,"wrenchVisual"); var startRotation = visual.localRotation;
            yield return new WaitForSecondsRealtime(1.1f);
            Require(watch.RepairProgress > 0 && Quaternion.Angle(startRotation,visual.localRotation)>1, "Automatic tightening not animated");
            View(new Vector3(1.7f,1.65f,-1.3f),new Vector3(.8f,1.1f,.15f)); Capture(preview,"09-automatic-tightening"); Record("tightening");
            yield return new WaitForSecondsRealtime(3);
            Require(watch.State.Repaired, "Repair did not complete");
            manager.SelectExit((IXRSelectInteractor)direct,(IXRSelectInteractable)wrench); direct.selectInput.manualPerformed = false;
            while (flood.Height > flood.MinimumHeight + .001f) yield return null;
            FloodView(); Capture(preview,"10-water-drained-after-repair"); Record("drained");
            rig.MoveCameraToWorldLocation(new Vector3(1.25f,8.2f,-.3f));
            View(new Vector3(1.6f,8.2f,-.3f),new Vector3(0,7.6f,-.3f));
            yield return new WaitForSecondsRealtime(.2f); Capture(preview,"11-f3-bed-sleep-prompt");
            var before = Camera.main.transform.position;
            Require(watch.TrySleep(), "Bed grip handler rejected repaired watch");
            yield return new WaitForSecondsRealtime(2.5f);
            Require(watch.State.Day == 2 && !watch.State.Repaired, "Sleep failed to start Day 2");
            Require(Vector3.Distance(before,Camera.main.transform.position)<.01f, "Sleep moved tracked camera");
            View(new Vector3(-2.15f,1.65f,-9.2f),new Vector3(-2.2f,1.45f,-7.5f)); Capture(preview,"12-day-two-board"); Record("Day 2");
            evidence.Add("PASS: actual lobby solo route; XRI direct selection and carry; original public trigger/grip handlers; real 60-second flood; animation, drainage and sleep/day reset. Desktop controller poses/input injected; physical headset usability not inferred.");
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
