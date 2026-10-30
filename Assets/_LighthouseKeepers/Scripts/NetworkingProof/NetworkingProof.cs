using System;
using System.Collections;
using System.IO;
using System.Linq;
using LighthouseKeepers.Networking;
using LighthouseKeepers.StormWatch;
using LighthouseKeepers.StormWatchNetworking;
using UnityEngine;

namespace LighthouseKeepers.NetworkingProof
{
    /// <summary>Two real Fusion players; scripted local hands, received partner, first-person camera.</summary>
    public sealed class NetworkingProof : MonoBehaviour
    {
        [SerializeField] Shader proofShader;
        [SerializeField, Min(6)] float durationSeconds = 7;
        const float PoseInterval = .2f, StateInterval = .5f;
        const int CaptureRate = 15;
        readonly KeeperInput local = new();
        readonly WatchState clock = new();
        FusionLobbyTransport crew;
        StormWatchFusionLink link;
        Avatar self, partner;
        Camera view;
        Material blue, amber, gloves, dark;
        bool host, started, finished, contact;
        int sent, received, lastSequence = -1, lastFrame = -1, frames, errors;
        float deadline, sendAt, publishAt, lastSnapshotAt, maxHandMotion;
        Vector3 previousHand;
        WatchState previousSnapshot;
        string output, failure = "";
        float Clock => host ? clock.elapsed : link && link.ReceivedState != null ? link.ReceivedState.elapsed : 0;
        float PoseClock => host ? Clock : Clock + Mathf.Min(.65f, Time.unscaledTime - lastSnapshotAt);

        IEnumerator Start()
        {
            host = Arg("-proof-role", "host") == "host";
            output = Arg("-proof-output", "");
            Application.runInBackground = true;
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
            Screen.SetResolution(1280, 720, false);
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
            if (!crew || !link) return;
            if (finished) { if (crew.IsOnline) Transmit(); return; }
            if (Time.unscaledTime > deadline) { Finish("Capture timed out"); return; }
            if (!crew.IsOnline) { if (started) Finish("Partner disconnected"); return; }
            if (link.ReceivedState != previousSnapshot)
            { previousSnapshot = link.ReceivedState; lastSnapshotAt = Time.unscaledTime; }
            Pose(PoseClock);
            Transmit();
            var remote = link.RemoteInput;
            if (remote.connected && remote.sequence != lastSequence)
            {
                if (received > 0) maxHandMotion = Mathf.Max(maxHandMotion, Vector3.Distance(previousHand, remote.right));
                previousHand = remote.right; lastSequence = remote.sequence; received++;
            }
            if (host && remote.connected)
            { started = true; clock.elapsed += Mathf.Min(Time.unscaledDeltaTime, .2f); }
            if (!host && Clock > 0) started = true;
            self.Apply(local);
            partner.root.SetActive(remote.connected);
            if (remote.connected)
            {
                partner.Apply(remote);
                if (started && Vector3.Distance(local.right, remote.right) < .16f) contact = true;
            }
            // This is a desktop camera on the local keeper's eyes, never an XR tracked camera.
            view.transform.SetPositionAndRotation(local.head, local.headRotation);
            if (started && Clock >= durationSeconds) Finish("");
        }
        void Transmit()
        {
            if (!link.PartnerConnected) return;
            if (Time.unscaledTime >= sendAt)
            { local.sequence++; link.SendInput(local); sent++; sendAt = Time.unscaledTime + PoseInterval; }
            if (host && Time.unscaledTime >= publishAt)
            { link.Publish(clock); publishAt = Time.unscaledTime + StateInterval; }
        }
        void Pose(float t)
        {
            float side = host ? -1 : 1;
            Vector3 forward = Vector3.left * side, right = Vector3.forward * side;
            local.connected = true;
            local.feet = new Vector3(side * .85f, 0, 0);
            local.head = local.feet + Vector3.up * 1.65f;
            local.headRotation = Quaternion.LookRotation(forward);
            local.left = local.feet + Vector3.up * 1.1f + forward * .26f - right * .27f;
            Vector3 resting = local.feet + Vector3.up * 1.25f + forward * .3f + right * .27f;
            Vector3 meeting = new(0, 1.57f, 0);
            float reach = t < 2.5f ? Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - 1.3f) / 1.2f)) :
                1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - 3.2f) / 1.0f));
            local.right = Vector3.Lerp(resting, meeting, reach);
            if (!host && (t < 1.3f || t > 4.5f))
                local.right = local.feet + forward * .28f + right * (.28f + Mathf.Sin(t * 8) * .1f) + Vector3.up * 1.88f;
            local.leftRotation = local.rightRotation = local.headRotation;
        }
        IEnumerator Capture()
        {
            while (!finished)
            {
                yield return new WaitForEndOfFrame();
                if (!started || Clock < .5f || output.Length == 0) continue;
                int frame = Mathf.FloorToInt(PoseClock * CaptureRate);
                if (frame <= lastFrame) continue;
                lastFrame = frame;
                ScreenCapture.CaptureScreenshot(Path.Combine(output, "frame-" + frame.ToString("D5") + ".png"));
                frames++;
            }
        }
        [Serializable] sealed class Result
        {
            public int processId, received, sent, capturedFrames, errors;
            public float elapsed, maxRemoteHandMotion, poseRateHz = 5, stateRateHz = 2;
            public bool passed, partnerConnected, handContactObserved;
            public string role, failure;
        }
        void Finish(string reason)
        {
            if (finished) return;
            failure = reason; finished = true;
            if (host && crew && crew.IsOnline) link.Publish(clock);
            var result = new Result { role = host ? "host" : "client", processId = System.Diagnostics.Process.GetCurrentProcess().Id,
                received = received, sent = sent, capturedFrames = frames, errors = errors, elapsed = Clock,
                maxRemoteHandMotion = maxHandMotion, partnerConnected = link && link.RemoteInput.connected,
                handContactObserved = contact, failure = failure };
            result.passed = failure.Length == 0 && result.partnerConnected && received >= 20 && contact && maxHandMotion > .05f && errors == 0;
            if (output.Length > 0) File.WriteAllText(Path.Combine(output, "result.json"), JsonUtility.ToJson(result, true));
            Debug.Log("PROOF_RESULT " + JsonUtility.ToJson(result));
            if (Arg("-proof-quit", "0") == "1") StartCoroutine(Quit(result.passed ? 0 : 1));
        }
        IEnumerator Quit(int code) { yield return new WaitForSecondsRealtime(3); Application.Quit(code); }
        void Log(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
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
                head.SetPositionAndRotation(p.head, p.headRotation); body.position = p.head - Vector3.up * .62f;
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
