using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LighthouseKeepers.Networking;
using LighthouseKeepers.StormWatch;
using LighthouseKeepers.StormWatchNetworking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LighthouseKeepers.NetworkingProof
{
    /// <summary>Independent desktop input on each Fusion peer; the partner uses received poses only.</summary>
    public sealed class NetworkingProof : MonoBehaviour
    {
        [SerializeField] Shader proofShader;
        [SerializeField, Min(6)] float durationSeconds = 7;
        [SerializeField, Min(.1f)] float moveSpeed = 1.2f;
        const float PoseInterval = .2f;
        const int CaptureRate = 15;
        readonly KeeperInput local = new();
        readonly List<string> trace = new();
        FusionLobbyTransport crew;
        StormWatchFusionLink link;
        Avatar self, partner;
        Camera view;
        Material blue, amber, gloves, dark;
        bool host, started, finished, hadPartner, lostPartner, jumpHeld, previousVisible;
        int sent, received, lastSequence = -1, lastFrame = -1, frames, errors;
        float deadline, sendAt, startedAt, pollAt, verticalSpeed, maxHandMotion, maxHeadMotion;
        Vector3 feet, previousHand, previousHead;
        string output, controlFile, failure = "";
        Control control = new();
        float Elapsed => started ? Time.unscaledTime - startedAt : 0;
        [Serializable] sealed class Control { public int revision; public float strafe, forward; public bool jump, wave, leave; }
        [Serializable] sealed class Sample
        {
            public string kind, utc;
            public float elapsed;
            public int sequence;
            public Vector3 head, right;
            public bool visible;
        }

        IEnumerator Start()
        {
            host = Arg("-proof-role", "host") == "host";
            output = Arg("-proof-output", ""); controlFile = Arg("-proof-control", "");
            feet = new Vector3(host ? -.85f : .85f, 0, 0);
            Application.runInBackground = true; Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0; Screen.SetResolution(1280, 720, false);
            if (output.Length > 0) Directory.CreateDirectory(output);
            Application.logMessageReceived += Log;
            CreateStage();
            crew = new GameObject("Fusion crew session").AddComponent<FusionLobbyTransport>();
            crew.SessionName = Arg("-proof-room", "LKFirstPersonProof");
            crew.SetLocalName(host ? "Blue" : "Amber");
            link = gameObject.AddComponent<StormWatchFusionLink>();
            deadline = Time.unscaledTime + 90;
            if (host) crew.HostGame(2); else crew.JoinGame(crew.SessionName);
            while (!crew.IsOnline && Time.unscaledTime < deadline)
            {
                if (!crew.IsBusy && crew.LastError.Length > 0) { Finish(crew.LastError); yield break; }
                yield return null;
            }
            if (!crew.IsOnline) { Finish("Connection timed out"); yield break; }
            link.BindSession(crew);
            Debug.Log("PROOF_CONNECTED role=" + (host ? "host" : "client"));
            StartCoroutine(Capture());
        }

        void Update()
        {
            if (!crew || !link || finished) return;
            if (Time.unscaledTime > deadline) { Finish("Capture timed out"); return; }
            if (!crew.IsOnline) { if (started) Finish("Session disconnected"); return; }
            if (!ReadInput()) return;
            UpdateLocalPose();
            if (link.PartnerConnected && Time.unscaledTime >= sendAt)
            {
                local.sequence++; link.SendInput(local); sent++; sendAt = Time.unscaledTime + PoseInterval;
                Record("tx", local, true);
            }
            var remote = link.RemoteInput;
            if (remote.connected && !started) { started = true; startedAt = Time.unscaledTime; }
            if (remote.connected)
            {
                hadPartner = true;
                if (remote.sequence != lastSequence)
                {
                    if (received > 0)
                    {
                        maxHandMotion = Mathf.Max(maxHandMotion, Vector3.Distance(previousHand, remote.right));
                        maxHeadMotion = Mathf.Max(maxHeadMotion, Vector3.Distance(previousHead, remote.head));
                    }
                    previousHand = remote.right; previousHead = remote.head; lastSequence = remote.sequence; received++;
                    Record("rx", remote, true);
                }
            }
            if (hadPartner && !link.PartnerConnected) lostPartner = true;
            self.Apply(local);
            partner.root.SetActive(remote.connected);
            if (remote.connected) partner.Apply(remote);
            if (previousVisible != remote.connected) { previousVisible = remote.connected; Record("visibility", remote, remote.connected); }
            view.transform.SetPositionAndRotation(local.head, local.headRotation);
            if (started && Elapsed >= durationSeconds && Arg("-proof-quit", "0") == "1") Finish("");
        }
        bool ReadInput()
        {
            if (controlFile.Length > 0)
            {
                // This optional test input is read only by the explicitly targeted player.
                // The host never reads the guest's input file or a shared animation timeline.
                if (Time.unscaledTime >= pollAt)
                {
                    pollAt = Time.unscaledTime + .05f;
                    try
                    {
                        if (File.Exists(controlFile))
                        {
                            var next = JsonUtility.FromJson<Control>(File.ReadAllText(controlFile));
                            if (next != null && next.revision > control.revision && float.IsFinite(next.strafe) && float.IsFinite(next.forward)) control = next;
                        }
                    }
                    catch (IOException) { }
                    catch (ArgumentException) { }
                }
            }
            else
            {
                var keys = Keyboard.current;
                control.strafe = keys == null ? 0 : (keys.dKey.isPressed ? 1 : 0) - (keys.aKey.isPressed ? 1 : 0);
                control.forward = keys == null ? 0 : (keys.wKey.isPressed ? 1 : 0) - (keys.sKey.isPressed ? 1 : 0);
                control.jump = keys != null && keys.spaceKey.isPressed;
                control.wave = keys != null && keys.eKey.isPressed;
                control.leave = keys != null && keys.escapeKey.wasPressedThisFrame;
            }
            if (!control.leave) return true;
            Finish("", true); crew.LeaveGame(); return false;
        }
        void UpdateLocalPose()
        {
            Vector3 forward = host ? Vector3.right : Vector3.left;
            Vector3 right = host ? Vector3.back : Vector3.forward;
            float dt = Mathf.Min(Time.unscaledDeltaTime, .1f);
            var movement = Vector2.ClampMagnitude(new Vector2(control.strafe, control.forward), 1);
            feet += (right * movement.x + forward * movement.y) * (moveSpeed * dt);
            feet.x = Mathf.Clamp(feet.x, -2, 2); feet.z = Mathf.Clamp(feet.z, -1.5f, 1.5f);
            if (control.jump && !jumpHeld && feet.y <= .001f) verticalSpeed = 3.2f;
            jumpHeld = control.jump;
            verticalSpeed -= 9.81f * dt; feet.y = Mathf.Max(0, feet.y + verticalSpeed * dt);
            if (feet.y == 0) verticalSpeed = 0;
            local.connected = true; local.feet = feet; local.head = feet + Vector3.up * 1.65f;
            local.headRotation = Quaternion.LookRotation(forward);
            local.left = feet + Vector3.up * 1.22f + forward * .36f - right * .27f;
            local.right = feet + Vector3.up * 1.22f + forward * .36f + right * .27f;
            if (control.wave) local.right = feet + forward * .28f + right * (.28f + Mathf.Sin(Time.unscaledTime * 8) * .1f) + Vector3.up * 1.88f;
            local.leftRotation = local.rightRotation = local.headRotation;
        }
        void Record(string kind, KeeperInput input, bool visible)
        {
            if (output.Length == 0 || trace.Count >= 2000) return;
            trace.Add(JsonUtility.ToJson(new Sample { kind = kind, utc = DateTime.UtcNow.ToString("O"), elapsed = Elapsed,
                sequence = input.sequence, head = input.head, right = input.right, visible = visible }));
        }
        IEnumerator Capture()
        {
            bool marked = false;
            while (!finished)
            {
                yield return new WaitForEndOfFrame();
                if (!started || Elapsed < .3f || output.Length == 0) continue;
                int frame = Mathf.FloorToInt(Elapsed * CaptureRate);
                if (frame <= lastFrame) continue;
                lastFrame = frame;
                ScreenCapture.CaptureScreenshot(Path.Combine(output, "frame-" + frame.ToString("D5") + ".png"));
                if (!marked) { File.WriteAllText(Path.Combine(output, "capture-started.txt"), DateTime.UtcNow.ToString("O")); marked = true; }
                frames++;
            }
        }
        [Serializable] sealed class Result
        {
            public int processId, received, sent, capturedFrames, errors;
            public float elapsed, maxRemoteHandMotion, maxRemoteHeadMotion, poseRateHz = 5;
            public bool passed, wasConnected, disconnectObserved, intentionalLeave, partnerVisibleAtEnd;
            public string role, failure, inputMode;
        }
        void Finish(string reason, bool intentionalLeave = false)
        {
            if (finished) return;
            failure = reason; finished = true;
            var result = new Result { role = host ? "host" : "client", processId = System.Diagnostics.Process.GetCurrentProcess().Id,
                received = received, sent = sent, capturedFrames = frames, errors = errors, elapsed = Elapsed,
                maxRemoteHandMotion = maxHandMotion, maxRemoteHeadMotion = maxHeadMotion,
                wasConnected = hadPartner, disconnectObserved = lostPartner, intentionalLeave = intentionalLeave,
                partnerVisibleAtEnd = partner != null && partner.root.activeSelf, failure = failure,
                inputMode = controlFile.Length == 0 ? "Keyboard (WASD / Space / E / Escape)" : "External commands delivered only to this process" };
            result.passed = failure.Length == 0 && hadPartner && received >= 10 && errors == 0 &&
                (host ? lostPartner && !result.partnerVisibleAtEnd && maxHeadMotion > .05f : intentionalLeave);
            if (output.Length > 0)
            {
                File.WriteAllText(Path.Combine(output, "result.json"), JsonUtility.ToJson(result, true));
                File.WriteAllLines(Path.Combine(output, "traffic.jsonl"), trace);
            }
            Debug.Log("PROOF_RESULT " + JsonUtility.ToJson(result));
            if (Arg("-proof-quit", "0") == "1") StartCoroutine(Quit(result.passed ? 0 : 1));
        }
        IEnumerator Quit(int code) { yield return new WaitForSecondsRealtime(2); Application.Quit(code); }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
        void OnDestroy() { Application.logMessageReceived -= Log; if (crew) crew.LeaveGame(); }
        static string Arg(string key, string fallback)
        { var args = System.Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback; }

        void CreateStage()
        {
            blue = Mat(new Color(.10f, .52f, .94f)); amber = Mat(new Color(1, .58f, .12f));
            gloves = Mat(new Color(.85f, .9f, .94f)); dark = Mat(new Color(.025f, .035f, .055f));
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .62f, .73f); RenderSettings.fog = false;
            var light = new GameObject("Soft light").AddComponent<Light>(); light.type = LightType.Directional;
            light.intensity = 1.25f; light.transform.rotation = Quaternion.Euler(42, -25, 0);
            view = new GameObject("Local keeper eyes").AddComponent<Camera>();
            view.clearFlags = CameraClearFlags.SolidColor; view.backgroundColor = new Color(.035f, .055f, .085f);
            view.fieldOfView = 68; view.nearClipPlane = .04f; view.farClipPlane = 30;
            Part("Floor", PrimitiveType.Cube, new(8, .2f, 8), Mat(new Color(.12f, .17f, .22f))).position = new Vector3(0, -.11f, 0);
            self = new Avatar(this, host ? blue : amber, true);
            partner = new Avatar(this, host ? amber : blue, false); partner.root.SetActive(false);
        }
        Material Mat(Color color) { var material = new Material(proofShader); material.color = color; return material; }
        Transform Part(string name, PrimitiveType type, Vector3 scale, Material material, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent ? parent : transform, false);
            go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(go.GetComponent<Collider>()); return go.transform;
        }
        sealed class Avatar
        {
            public readonly GameObject root;
            readonly Transform head, body, left, right, armL, armR, bootL, bootR;
            bool initialized;
            public Avatar(NetworkingProof owner, Material coat, bool local)
            {
                root = new GameObject(local ? "Local hands" : "Received keeper"); root.transform.SetParent(owner.transform);
                Transform P(string name, PrimitiveType type, Vector3 scale, Material material) => owner.Part(name, type, scale, material, root.transform);
                head = P("Helmet", PrimitiveType.Sphere, Vector3.one * .36f, coat);
                var visor = owner.Part("Visor", PrimitiveType.Cube, new(.72f, .28f, .25f), owner.dark, head); visor.localPosition = new Vector3(0, 0, .46f);
                body = P("Coat", PrimitiveType.Capsule, new(.46f, .46f, .34f), coat);
                left = P("Left glove", PrimitiveType.Sphere, Vector3.one * .15f, owner.gloves);
                right = P("Right glove", PrimitiveType.Sphere, Vector3.one * .15f, owner.gloves);
                armL = P("Left sleeve", PrimitiveType.Cylinder, Vector3.one, coat);
                armR = P("Right sleeve", PrimitiveType.Cylinder, Vector3.one, coat);
                bootL = P("Left boot", PrimitiveType.Capsule, new(.17f, .23f, .21f), owner.dark);
                bootR = P("Right boot", PrimitiveType.Capsule, new(.17f, .23f, .21f), owner.dark);
                if (local) { head.gameObject.SetActive(false); body.gameObject.SetActive(false); bootL.gameObject.SetActive(false); bootR.gameObject.SetActive(false); }
            }
            public void Apply(KeeperInput p)
            {
                float blend = initialized ? 1 - Mathf.Exp(-22 * Time.unscaledDeltaTime) : 1; initialized = true;
                head.SetPositionAndRotation(Vector3.Lerp(head.position, p.head, blend), p.headRotation); body.position = head.position - Vector3.up * .62f;
                left.position = Vector3.Lerp(left.position, p.left, blend); right.position = Vector3.Lerp(right.position, p.right, blend);
                var side = p.headRotation * Vector3.right;
                Sleeve(armL, p.head - Vector3.up * .4f - side * .24f, left.position);
                Sleeve(armR, p.head - Vector3.up * .4f + side * .24f, right.position);
                bootL.position = p.feet + Vector3.up * .26f - side * .13f; bootR.position = p.feet + Vector3.up * .26f + side * .13f;
            }
            static void Sleeve(Transform sleeve, Vector3 shoulder, Vector3 hand)
            { sleeve.position = (shoulder + hand) * .5f; sleeve.up = hand - shoulder; sleeve.localScale = new Vector3(.11f, Vector3.Distance(shoulder, hand) * .5f, .11f); }
        }
    }
}
