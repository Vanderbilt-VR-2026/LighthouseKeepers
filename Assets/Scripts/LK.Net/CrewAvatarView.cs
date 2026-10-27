using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
namespace LighthouseKeepers.Net
{
    /// <summary>Visual-only network rig. The user's tracked camera always remains in their existing XR Origin.</summary>
    [RequireComponent(typeof(NetworkObject))]
    public sealed class CrewAvatarView : MonoBehaviour
    {
        [SerializeField] GameObject leftHandVisual;
        [SerializeField] GameObject rightHandVisual;
        [SerializeField] Material oilskinMaterial;
        public UnityEvent<int,float,float> LeftPoseChanged = new();
        public UnityEvent<int,float,float> RightPoseChanged = new();
        NetworkObject identity;
        Transform head, left, right, coat;
        GameObject visuals;
        TextMeshPro nameplate;
        Material fallback;
        Camera observer;
        float nextName;
        void Awake()
        {
            identity = GetComponent<NetworkObject>();
            visuals = new GameObject("Keeper appearance"); visuals.transform.SetParent(transform,false);
            if (!oilskinMaterial)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader) fallback = new Material(shader) { color = new Color(.72f,.49f,.12f) };
            }
            head = Part("Hood",PrimitiveType.Sphere,new Vector3(.24f,.28f,.24f));
            coat = Part("Oilskin jacket",PrimitiveType.Capsule,new Vector3(.42f,.39f,.28f));
            left = Hand(leftHandVisual,"Left hand"); right = Hand(rightHandVisual,"Right hand");
            nameplate = new GameObject("Keeper name").AddComponent<TextMeshPro>();
            nameplate.transform.SetParent(visuals.transform,false); nameplate.font = TMP_Settings.defaultFontAsset;
            nameplate.fontSize = 2; nameplate.rectTransform.sizeDelta = new Vector2(2,.3f); nameplate.alignment = TextAlignmentOptions.Center;
            nameplate.richText = false; nameplate.textWrappingMode = TextWrappingModes.NoWrap;
            visuals.SetActive(false);
        }
        Transform Hand(GameObject reference,string label)
        {
            if (!reference) return Part(label,PrimitiveType.Cube,new Vector3(.09f,.12f,.16f));
            return Instantiate(reference,visuals.transform).transform;
        }
        Transform Part(string label,PrimitiveType type,Vector3 scale)
        {
            var part = GameObject.CreatePrimitive(type); part.name = label; part.transform.SetParent(visuals.transform,false); part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            if (oilskinMaterial || fallback) part.GetComponent<Renderer>().sharedMaterial = oilskinMaterial ? oilskinMaterial : fallback;
            return part.transform;
        }
        void LateUpdate()
        {
            var stream = CrewPoseStream.Active;
            bool visible = identity && identity.IsValid && stream && identity.Runner == stream.Channel.Runner &&
                identity.InputAuthority != identity.Runner.LocalPlayer && stream.Sample(identity.InputAuthority.RawEncoded,out _);
            visuals.SetActive(visible);
            if (!visible || !stream.Sample(identity.InputAuthority.RawEncoded,out var pose)) return;
            head.SetPositionAndRotation(pose.Head,pose.HeadRotation);
            left.SetPositionAndRotation(pose.Left,pose.LeftRotation); right.SetPositionAndRotation(pose.Right,pose.RightRotation);
            var forward = Vector3.ProjectOnPlane(pose.HeadRotation*Vector3.forward,Vector3.up);
            if (forward.sqrMagnitude > .01f) coat.rotation = Quaternion.LookRotation(forward);
            coat.position = pose.Head - Vector3.up*.58f;
            LeftPoseChanged.Invoke(pose.LeftPose,pose.LeftGrip,pose.LeftTrigger);
            RightPoseChanged.Invoke(pose.RightPose,pose.RightGrip,pose.RightTrigger);
            if (!observer) observer = Camera.main;
            if (!observer) return;
            nameplate.transform.position = pose.Head + Vector3.up*.35f;
            nameplate.transform.rotation = observer.transform.rotation;
            nameplate.alpha = Mathf.Clamp01(7-Vector3.Distance(observer.transform.position,pose.Head));
            if (Time.unscaledTime >= nextName) { nextName = Time.unscaledTime + 1; nameplate.text = stream.NameFor(identity.InputAuthority.RawEncoded); }
        }
        void OnDestroy() { if (fallback) Destroy(fallback); }
    }
}
