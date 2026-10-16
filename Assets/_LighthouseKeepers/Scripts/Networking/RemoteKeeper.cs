using UnityEngine;

/// <summary>Visual-only remote head, torso and hands. No camera, listener, controller or input component.</summary>
public sealed class RemoteKeeper : MonoBehaviour
{
    Transform head, left, right, torso;
    Material material;
    KeeperPose target;
    int sequence = -1;
    bool initialized;
    public void Build(int player)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) shader = Shader.Find("Standard");
        if (shader) material = new Material(shader) { color = Color.HSVToRGB((player * .23f) % 1, .45f, .85f) };
        head = Part("Head", PrimitiveType.Sphere, new Vector3(.23f, .25f, .23f));
        torso = Part("Torso", PrimitiveType.Capsule, new Vector3(.3f, .35f, .22f));
        left = Part("Left hand", PrimitiveType.Cube, new Vector3(.09f, .12f, .16f));
        right = Part("Right hand", PrimitiveType.Cube, new Vector3(.09f, .12f, .16f));
    }
    Transform Part(string name, PrimitiveType shape, Vector3 scale)
    {
        var part = GameObject.CreatePrimitive(shape);
        part.name = name; part.transform.SetParent(transform); part.transform.localScale = scale;
        var collider = part.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
        if (material) part.GetComponent<Renderer>().sharedMaterial = material;
        return part.transform;
    }
    public void SetPose(KeeperPose pose, int nextSequence)
    {
        if (nextSequence <= sequence) return;
        sequence = nextSequence; target = pose;
        if (!initialized) { initialized = true; Apply(1); }
    }
    void LateUpdate() { if (initialized) Apply(1 - Mathf.Exp(-20 * Time.unscaledDeltaTime)); }
    void Apply(float blend)
    {
        Move(head, target.Head, target.HeadRotation, blend);
        Move(left, target.LeftHand, target.LeftRotation, blend);
        Move(right, target.RightHand, target.RightRotation, blend);
        var forward = Vector3.ProjectOnPlane(target.HeadRotation * Vector3.forward, Vector3.up);
        var yaw = forward.sqrMagnitude > .01f ? Quaternion.LookRotation(forward) : target.RootRotation;
        Move(torso, target.Head - Vector3.up * .6f, yaw, blend);
    }
    static void Move(Transform part, Vector3 position, Quaternion rotation, float blend)
    {
        part.position = Vector3.Lerp(part.position, position, blend);
        part.rotation = Quaternion.Slerp(part.rotation, rotation, blend);
    }
    void OnDestroy() { if (material) Destroy(material); }
}
