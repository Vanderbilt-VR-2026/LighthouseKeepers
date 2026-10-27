using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace LighthouseKeepers.Gameplay.Interaction
{
    // Pose payloads are baked by author_hands.py; no Animator or vendor hand-tracking package.
    [DefaultExecutionOrder(-110)]
    public sealed class HandRig : MonoBehaviour
    {
        [Serializable] public sealed class Bone { public string name; public int parent; public Vector3 position; }
        [Serializable] public sealed class Pose { public string name; public Quaternion[] rotations; }
        [Serializable] public sealed class Model
        {
            public Bone[] bones; public Vector3[] vertices, normals; public int[] triangles, boneIndices;
            public float[] boneWeights; public Pose[] poses;
        }
        [SerializeField] TextAsset model;
        [SerializeField] Material gloveMaterial;
        [SerializeField] XRNode hand = XRNode.LeftHand;
        [SerializeField, Range(.06f, .09f)] float blendSeconds = .075f;
        [SerializeField] int actorId;
        [SerializeField, Tooltip("Fallback aim-to-grip pitch in degrees, used only without a tracked grip pose.")]
        float fallbackGripPitch = -75f;
        [SerializeField, Range(0,90), Tooltip("Mirrored visual palm roll about the grip axis; tracked controller and interactors are unchanged.")]
        float palmRoll = 65f;
        Transform trackingSpace;
        Quaternion measuredGripCorrection = Quaternion.identity;
        bool hasGripMeasurement;
        public Quaternion VisualRotation => visual ? visual.localRotation : Quaternion.identity;
        Transform[] bones;
        Model data;
        Mesh mesh;
        Material ownedMaterial;
        Transform visual, constrainedGrip;
        XRDirectInteractor direct;
        HandPose? selectedPose;
        UnityEngine.Object poseOwner;
        float gripBlend, triggerBlend;
        readonly Quaternion[] blendStart = new Quaternion[26];
        float poseTime;
        readonly AnalogButtonState gripButton = new AnalogButtonState(), triggerButton = new AnalogButtonState();
        
        public int ActorId { get => actorId; set => actorId = value; }
        public XRNode Node => hand;
        public XRDirectInteractor Direct => direct;

        void Awake()
        {
            if (!model) { Debug.LogError("Hand rig has no baked mesh/pose payload.", this); enabled = false; return; }
            var origin = GetComponentInParent<XROrigin>();
            if (origin && origin.CameraFloorOffsetObject) trackingSpace = origin.CameraFloorOffsetObject.transform;
            data = JsonUtility.FromJson<Model>(model.text);
            if (data.bones.Length != 26 || data.poses.Length != 4) throw new InvalidOperationException("Expected OpenXR 26 joints and four baked poses.");
            visual = transform.Find("Skinned glove");
            if (!visual) { visual = new GameObject("Skinned glove").transform; visual.SetParent(transform, false); }
            bones = new Transform[26];
            var bind = new Matrix4x4[26];
            for (int i = 0; i < bones.Length; i++)
            {
                var parent = data.bones[i].parent < 0 ? visual : bones[data.bones[i].parent];
                bones[i] = parent.Find(data.bones[i].name);
                if (!bones[i]) { bones[i] = new GameObject(data.bones[i].name).transform; bones[i].SetParent(parent, false); }
                bones[i].localPosition = data.bones[i].position;
                bind[i] = bones[i].worldToLocalMatrix * visual.localToWorldMatrix;
                blendStart[i] = Quaternion.identity;
            }
            var weights = new BoneWeight[data.vertices.Length];
            for (int i = 0; i < weights.Length; i++) weights[i] = new BoneWeight
            { boneIndex0 = data.boneIndices[i * 2], boneIndex1 = data.boneIndices[i * 2 + 1], weight0 = data.boneWeights[i], weight1 = 1 - data.boneWeights[i] };
            mesh = new Mesh { name = "Glove / 26 joint skin", vertices = data.vertices, normals = data.normals,
                triangles = data.triangles, boneWeights = weights, bindposes = bind };
            mesh.RecalculateBounds();
            var skin = visual.gameObject.AddComponent<SkinnedMeshRenderer>();
            skin.sharedMesh = mesh; skin.bones = bones; skin.rootBone = bones[0];
            skin.localBounds = new Bounds(new Vector3(0, -.035f, .025f), new Vector3(.3f, .3f, .3f));
            skin.shadowCastingMode = ShadowCastingMode.Off;
            // The restored shell has overbright baked probes (measured irradiance >170 at House).
            // Keep the skin Lit by ambient/realtime lights without clipping it to white near fixtures.
            skin.lightProbeUsage = LightProbeUsage.Off;
            if (!gloveMaterial)
            {
                ownedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                ownedMaterial.SetColor("_BaseColor", new Color(198f/255, 142f/255, 106f/255));
                ownedMaterial.SetFloat("_Smoothness", .22f); gloveMaterial = ownedMaterial;
            }
            skin.sharedMaterial = gloveMaterial;
            ConfigureInteractor();
            LogVisualDiagnostics("awake");
        }
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogVisualDiagnostics(string phase)
        {
            var device = InputDevices.GetDeviceAtXRNode(hand);
            device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion grip);
            device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked);
            Debug.Log($"HAND_DIAG {phase} {hand} device={device.name} tracked={tracked} grip={grip.eulerAngles} controllerLocal={transform.parent.localEulerAngles} gripCorrection={measuredGripCorrection.eulerAngles} measured={hasGripMeasurement} rootLocal={transform.localEulerAngles} visualLocal={(visual ? visual.localEulerAngles : Vector3.zero)} pipeline={GraphicsSettings.currentRenderPipeline} graphics={SystemInfo.graphicsDeviceType}");
            var skin = visual ? visual.GetComponent<SkinnedMeshRenderer>() : null;
            if (skin)
            {
                LightProbes.GetInterpolatedProbe(skin.bounds.center,null,out SphericalHarmonicsL2 sh);
                var values=new Color[1]; sh.Evaluate(new[]{Vector3.up},values);
                Debug.Log($"HAND_LIGHT {phase} {hand} probeUsage={skin.lightProbeUsage} sceneProbeUp={values[0].ToString("F3")} center={skin.bounds.center}");
            }
            foreach (var r in transform.parent.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.sharedMaterials)
                    Debug.Log($"HAND_DIAG {hand} renderer={r.name} type={r.GetType().Name} enabled={r.enabled} active={r.gameObject.activeInHierarchy} material={(m ? m.name : "NULL")} shader={(m && m.shader ? m.shader.name : "NULL")} supported={(m && m.shader && m.shader.isSupported)} base={(m && m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor").ToString("F3") : "none")} color={(m && m.HasProperty("_Color") ? m.GetColor("_Color").ToString("F3") : "none")} propertyBlock={r.HasPropertyBlock()} keywords={(m ? string.Join(",", m.shaderKeywords) : "none")}");
        }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        bool loggedTracked;
        System.Collections.IEnumerator Start()
        {
            yield return new WaitForSeconds(1.5f);
            LogVisualDiagnostics("settled");
        }
#endif
        void ConfigureInteractor()
        {
            // The trigger follows tracking, while only the skinned visual follows a constrained handle.
            var go = new GameObject("Palm direct grab"); go.SetActive(false); go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, -.018f, .025f);
            var sphere = go.AddComponent<SphereCollider>(); sphere.radius = .08f; sphere.isTrigger = true;
            var body = go.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            direct = go.AddComponent<XRDirectInteractor>();
            direct.handedness = hand == XRNode.LeftHand ? InteractorHandedness.Left : InteractorHandedness.Right;
            direct.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;
            direct.selectInput = new XRInputButtonReader("Physical grip", inputSourceMode: XRInputButtonReader.InputSourceMode.ManualValue);
            direct.activateInput = new XRInputButtonReader("Physical trigger", inputSourceMode: XRInputButtonReader.InputSourceMode.ManualValue);
            direct.selectEntered.AddListener(Selected); direct.selectExited.AddListener(Released);
            go.SetActive(true);
        }
        void Update()
        {
            var device = InputDevices.GetDeviceAtXRNode(hand);
            bool tracked = device.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked) && isTracked;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (tracked && !loggedTracked) { loggedTracked = true; LogVisualDiagnostics("first-tracked"); }
#endif
            device.TryGetFeatureValue(CommonUsages.grip, out float g);
            device.TryGetFeatureValue(CommonUsages.trigger, out float t);
            g = tracked ? Mathf.Clamp01(g) : 0; t = tracked ? Mathf.Clamp01(t) : 0;
            WriteButton(direct.selectInput, gripButton, g);
            WriteButton(direct.activateInput, triggerButton, t);
            // Exponential response has 90% settling in the requested 75ms, independent of refresh rate.
            float blend = 1 - Mathf.Exp(-2.303f * Time.unscaledDeltaTime / blendSeconds);
            gripBlend = Mathf.Lerp(gripBlend, g, blend); triggerBlend = Mathf.Lerp(triggerBlend, t, blend);
            if (selectedPose.HasValue && !poseOwner) ClearPose();
            poseTime += Time.unscaledDeltaTime;
            float eased = Mathf.SmoothStep(0, 1, Mathf.Clamp01(poseTime / blendSeconds));
            for (int i = 0; i < bones.Length; i++)
            {
                Quaternion rotation;
                if (selectedPose.HasValue)
                    rotation = Quaternion.Slerp(blendStart[i], PoseRotation(selectedPose.Value, i), eased);
                else
                {
                    rotation = Quaternion.Slerp(data.poses[0].rotations[i], data.poses[1].rotations[i], gripBlend);
                    rotation = Quaternion.Slerp(rotation, data.poses[3].rotations[i], triggerBlend * (1 - gripBlend));
                    rotation = Quaternion.Slerp(blendStart[i], rotation, eased);
                }
                bones[i].localRotation = rotation;
            }
        }
        static void WriteButton(XRInputButtonReader reader, AnalogButtonState state, float value)
        {
            state.Update(value, Time.frameCount);
            reader.manualValue = value; reader.manualPerformed = state.Pressed;
            reader.manualFramePerformed = state.PressedFrame; reader.manualFrameCompleted = state.ReleasedFrame;
        }
        Quaternion PoseRotation(HandPose pose, int index) => data.poses[pose == HandPose.PumpHandle ? 2 : (int)pose].rotations[index];
        void LateUpdate()
        {
            if (!visual) return;
            if (constrainedGrip) visual.SetPositionAndRotation(constrainedGrip.position - constrainedGrip.rotation * new Vector3(0, -.018f, .025f), constrainedGrip.rotation);
            else
            {
                var device = InputDevices.GetDeviceAtXRNode(hand);
                hasGripMeasurement = trackingSpace && device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) && tracked
                    && device.TryGetFeatureValue(CommonUsages.deviceRotation, out _);
                if (hasGripMeasurement)
                {
                    device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion gripRotation);
                    // Starter Assets tracks pointer/aim. Convert the actual OpenXR grip into the
                    // visual parent's coordinates; never rotate the tracked controller or its rays.
                    measuredGripCorrection = Quaternion.Inverse(transform.rotation) * trackingSpace.rotation * gripRotation;
                }
                else measuredGripCorrection = Quaternion.Euler(fallbackGripPitch,0,0);
                visual.localRotation = measuredGripCorrection * Quaternion.Euler(0,0,hand == XRNode.LeftHand ? palmRoll : -palmRoll);
                // Rotate around the baked wrist, keeping it attached to the same tracking anchor.
                Vector3 wrist = data.bones[0].position;
                visual.localPosition = wrist - visual.localRotation * wrist;
            }
        }
        void Selected(SelectEnterEventArgs args)
        {
            var target = args.interactableObject.transform.GetComponent<IHandPoseTarget>();
            SetPose(target?.RequiredPose ?? HandPose.WrapGrip, args.interactableObject.transform);
        }
        void Released(SelectExitEventArgs args) { ClearPose(); }
        public void SetPose(HandPose pose, UnityEngine.Object owner, Transform grip = null)
        { Capture(); selectedPose = pose; poseOwner = owner; constrainedGrip = grip; }
        public void SetConstrainedGrip(Transform grip) { constrainedGrip = grip; }
        public void ClearPose() { Capture(); selectedPose = null; poseOwner = null; constrainedGrip = null; }
        void Capture() { if (bones != null) for (int i = 0; i < bones.Length; i++) blendStart[i] = bones[i].localRotation; poseTime = 0; }
        public void Pulse(float amplitude, float seconds)
        { var device = InputDevices.GetDeviceAtXRNode(hand); if (device.TryGetHapticCapabilities(out var h) && h.supportsImpulse) device.SendHapticImpulse(0, amplitude, seconds); }
        void OnDisable() { if (direct) { WriteButton(direct.selectInput, gripButton, 0); WriteButton(direct.activateInput, triggerButton, 0); direct.enabled = false; } ClearPose(); }
        void OnEnable() { if (direct) direct.enabled = true; }
        void OnDestroy() { if (mesh) Destroy(mesh); if (ownedMaterial) Destroy(ownedMaterial); }
    }
}
