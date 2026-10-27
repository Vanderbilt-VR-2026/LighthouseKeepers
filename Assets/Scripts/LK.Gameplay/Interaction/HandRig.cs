using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;
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
            if (!gloveMaterial)
            {
                ownedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                ownedMaterial.SetColor("_BaseColor", new Color(.62f, .52f, .43f));
                ownedMaterial.SetFloat("_Smoothness", .22f); gloveMaterial = ownedMaterial;
            }
            skin.sharedMaterial = gloveMaterial;
            ConfigureInteractor();
        }
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
            else { visual.localPosition = Vector3.zero; visual.localRotation = Quaternion.identity; }
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
