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
        public UnityEvent<int,float,float> LeftPoseChanged = new();
        public UnityEvent<int,float,float> RightPoseChanged = new();
        NetworkObject identity;
        Transform head, left, right, coat;
        GameObject visuals;
        TextMeshPro nameplate;
        Material fallback;
        Camera observer;
        float nextName;
        bool bodyPlaced;
        void Awake()
        {
            identity = GetComponent<NetworkObject>();
            visuals = new GameObject("Keeper appearance"); visuals.transform.SetParent(transform,false);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader) fallback = new Material(shader) { name = "Remote keeper grey", color = new Color(.48f,.48f,.48f) };
            head = Part("Keeper head",PrimitiveType.Sphere,new Vector3(.24f,.28f,.24f));
            var visor = Part("Dark forward visor",PrimitiveType.Cube,Vector3.one);
            visor.SetParent(head,false); visor.localPosition = new Vector3(0,0,.46f);
            visor.localScale = new Vector3(.88f,.24f,.16f);
            // Reuse the single grey material; only the visor's colour differs.
            var tint = new MaterialPropertyBlock(); tint.SetColor("_BaseColor",new Color(.035f,.045f,.05f));
            visor.GetComponent<Renderer>().SetPropertyBlock(tint);
            coat = Part("Upright grey torso",PrimitiveType.Capsule,new Vector3(.38f,.30f,.25f));
            left = Hand(leftHandVisual,"Left hand"); right = Hand(rightHandVisual,"Right hand");
            nameplate = new GameObject("Keeper name").AddComponent<TextMeshPro>();
            nameplate.transform.SetParent(visuals.transform,false); nameplate.font = TMP_Settings.defaultFontAsset;
            nameplate.fontSize = .9f; nameplate.rectTransform.sizeDelta = new Vector2(2,.3f); nameplate.alignment = TextAlignmentOptions.Center;
            nameplate.richText = false; nameplate.textWrappingMode = TextWrappingModes.NoWrap;
            visuals.SetActive(false);
        }
        Transform Hand(GameObject reference,string label)
        {
            if (!reference)
            {
                var anchor=new GameObject(label);anchor.transform.SetParent(visuals.transform,false);
                anchor.AddComponent<RemoteHandVisual>().left=label.StartsWith("Left");return anchor.transform;
            }
            return Instantiate(reference,visuals.transform).transform;
        }
        Transform Part(string label,PrimitiveType type,Vector3 scale)
        {
            var part = GameObject.CreatePrimitive(type); part.name = label; part.transform.SetParent(visuals.transform,false); part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            if (fallback) part.GetComponent<Renderer>().sharedMaterial = fallback;
            return part.transform;
        }
        void LateUpdate()
        {
            var stream = CrewPoseStream.Active;
            bool visible = identity && identity.IsValid && stream && identity.Runner == stream.Channel.Runner &&
                identity.InputAuthority != identity.Runner.LocalPlayer && stream.SampleVisual(identity.InputAuthority.RawEncoded,out _);
            visuals.SetActive(visible);
            if (!visible || !stream.SampleVisual(identity.InputAuthority.RawEncoded,out var pose)) return;
            head.SetPositionAndRotation(pose.Head,pose.HeadRotation);
            left.SetPositionAndRotation(pose.Left,pose.LeftRotation); right.SetPositionAndRotation(pose.Right,pose.RightRotation);
            var forward = Vector3.ProjectOnPlane(pose.HeadRotation*Vector3.forward,Vector3.up);
            if (forward.sqrMagnitude > .01f)
            {
                var yaw = Quaternion.LookRotation(forward,Vector3.up);
                coat.rotation = bodyPlaced ? Quaternion.Slerp(coat.rotation,yaw,1-Mathf.Exp(-6*Time.unscaledDeltaTime)) : yaw;
            }
            coat.position = pose.Head - Vector3.up*.48f;
            bodyPlaced = true;
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
