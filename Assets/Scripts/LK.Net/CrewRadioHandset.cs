using System.Collections.Generic;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.Net
{
    /// <summary>
    /// A grabbable walkie-talkie. It owns no voice code: CrewVoiceSpatializer asks whether the local keeper holds one
    /// and whether a remote actor holds one (from the existing replicated grab lease, FusionGrabBinding.Owner).
    /// In solo nothing reads it; it is an ordinary prop.
    /// </summary>
    public sealed class CrewRadioHandset : MonoBehaviour, INetworkPropRelease
    {
        public const string SceneName = ShipWorld.GameplaySceneName, RootName = "Crew handheld radios";
        static readonly List<CrewRadioHandset> live = new();
        static Material body, trim, lamp;
        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        static readonly Color LampOff = new(.05f, .01f, 0), LampOn = new(1.6f, .55f, .05f);

        XRGrabInteractable grab;
        Rigidbody rb;
        FusionGrabBinding binding;
        Renderer lampRenderer;
        MaterialPropertyBlock lampBlock;
        Vector3 home;
        Quaternion homeRotation;
        bool lampLit;
        public Transform Speaker { get; private set; }

        FusionGrabBinding Binding => binding ? binding : binding = GetComponent<FusionGrabBinding>();
        public bool HeldLocally
        {
            get
            {
                if (!grab || !grab.isSelected) return false;
                bool hand = false;
                foreach (var i in grab.interactorsSelecting) if (!(i is XRSocketInteractor)) { hand = true; break; }
                return hand && !(Binding && Binding.IsRemotelyOwned);
            }
        }

        public static IReadOnlyList<CrewRadioHandset> Live => live;
        /// <summary>True when the radios are bound into the shared grab layout, so remote holding is known.</summary>
        public static bool OwnershipReplicated
        {
            get { foreach (var h in live) if (h && h.Binding) return true; return false; }
        }
        public static CrewRadioHandset LocalHeld()
        {
            foreach (var h in live) if (h && h.HeldLocally) return h;
            return null;
        }
        public static bool HeldBy(int actor)
        {
            if (actor <= 0) return false;
            foreach (var h in live) if (h && h.Binding && h.Binding.Owner == actor) return true;
            return false;
        }
        public void SetReceiving(bool on)
        {
            if (on == lampLit || !lampRenderer) return;
            lampLit = on; lampBlock.SetColor(EmissionColor, on ? LampOn : LampOff); lampRenderer.SetPropertyBlock(lampBlock);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { live.Clear(); SceneManager.sceneLoaded -= Loaded; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded;
            for (int i = 0; i < SceneManager.sceneCount; i++) Loaded(SceneManager.GetSceneAt(i), LoadSceneMode.Additive);
        }

        // Built during sceneLoaded, before FusionGrabAuthority's first prop scan (it waits for EnvironmentBootstrap
        // and rescans after every scene load), so every peer registers the same two handsets.
        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.IsValid() || !scene.isLoaded || scene.name != SceneName) return;
            var settings = CrewVoiceAcousticsSettings.Current;
            if (!settings.handheldRadios) return;
            foreach (var r in scene.GetRootGameObjects()) if (r.name == RootName) return;
            Vector3 a = new(-2.90f, .88f, -9.07f), step = new(0, 0, .16f);
            foreach (var r in scene.GetRootGameObjects())
            {
                var table = r.name == "Wrench table top" ? r.transform : r.transform.Find("Wrench table top");
                if (!table) foreach (var t in r.GetComponentsInChildren<Transform>(true)) if (t.name == "Wrench table top") { table = t; break; }
                if (!table || !table.TryGetComponent<Renderer>(out var top)) continue;
                var b = top.bounds; // House bench: radios stand at its south end, clear of the wrench and plate.
                a = new Vector3(b.center.x + .12f, b.max.y, b.min.z + .14f);
                break;
            }
            var root = new GameObject(RootName); SceneManager.MoveGameObjectToScene(root, scene);
            Create(root.transform, "Crew radio A", a);
            Create(root.transform, "Crew radio B", a + step);
        }

        static void Materials()
        {
            if (body) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) return;
            body = new Material(shader) { name = "Crew radio painted green", color = new Color(.12f, .22f, .14f) };
            body.SetFloat("_Smoothness", .35f);
            trim = new Material(shader) { name = "Crew radio brass", color = new Color(.62f, .42f, .14f) };
            trim.SetFloat("_Metallic", .8f); trim.SetFloat("_Smoothness", .5f);
            lamp = new Material(shader) { name = "Crew radio receive lamp", color = new Color(.25f, .08f, .02f) };
            lamp.EnableKeyword("_EMISSION"); lamp.SetColor(EmissionColor, LampOff);
        }

        static CrewRadioHandset Create(Transform parent, string label, Vector3 restOnSurface)
        {
            Materials();
            var size = new Vector3(.065f, .13f, .036f);
            var go = new GameObject(label); go.SetActive(false);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(restOnSurface + Vector3.up * (size.y * .5f + .002f), Quaternion.Euler(0, -90, 0)); // grille faces +X, toward the House spawn
            Part(go.transform, "Radio case", PrimitiveType.Cube, Vector3.zero, size, body);
            Part(go.transform, "Brass speaker grille", PrimitiveType.Cube, new Vector3(0, .018f, -.0185f), new Vector3(.05f, .05f, .003f), trim);
            Part(go.transform, "Antenna", PrimitiveType.Cylinder, new Vector3(.02f, .1f, 0), new Vector3(.009f, .035f, .009f), trim);
            Part(go.transform, "Talk key", PrimitiveType.Cube, new Vector3(-.034f, .015f, 0), new Vector3(.006f, .035f, .016f), trim);
            var lampPart = Part(go.transform, "Receive lamp", PrimitiveType.Sphere, new Vector3(-.018f, .056f, -.012f), Vector3.one * .011f, lamp);
            var speaker = new GameObject("Radio speaker").transform; speaker.SetParent(go.transform, false); speaker.localPosition = new Vector3(0, .018f, -.02f);
            var attach = new GameObject("Radio grip").transform; attach.SetParent(go.transform, false); attach.localPosition = new Vector3(0, -.02f, 0);
            var col = go.AddComponent<BoxCollider>(); col.size = size;
            var rb = go.AddComponent<Rigidbody>(); rb.mass = .3f; rb.isKinematic = true; rb.interpolation = RigidbodyInterpolation.Interpolate;
            var grab = go.AddComponent<XRGrabInteractable>();
            grab.colliders.Add(col); grab.attachTransform = attach; grab.useDynamicAttach = false;
            grab.movementType = XRBaseInteractable.MovementType.Kinematic; grab.throwOnDetach = true;
            var h = go.AddComponent<CrewRadioHandset>();
            h.grab = grab; h.rb = rb; h.Speaker = speaker; h.home = go.transform.position; h.homeRotation = go.transform.rotation;
            h.lampRenderer = lampPart ? lampPart.GetComponent<Renderer>() : null; h.lampBlock = new MaterialPropertyBlock();
            go.SetActive(true);
            return h;
        }

        static GameObject Part(Transform parent, string name, PrimitiveType shape, Vector3 pos, Vector3 scale, Material material)
        {
            var o = GameObject.CreatePrimitive(shape); o.name = name; o.layer = parent.gameObject.layer;
            o.transform.SetParent(parent, false); o.transform.localPosition = pos; o.transform.localScale = scale;
            var c = o.GetComponent<Collider>(); c.enabled = false; Destroy(c);
            var r = o.GetComponent<Renderer>(); if (material) r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return o;
        }

        void OnEnable() { if (!live.Contains(this)) live.Add(this); if (grab) grab.selectExited.AddListener(Released); }
        void OnDisable() { live.Remove(this); if (grab) grab.selectExited.RemoveListener(Released); }
        void Released(SelectExitEventArgs e) { if (!grab.isSelected) OnNetworkReleased(); }
        public void OnNetworkReleased() { if (rb) rb.isKinematic = false; }
        void Update()
        {
            // Fell out of the shell: only the simulating peer (host or solo) returns it to the bench.
            if (grab.isSelected || transform.position.y > home.y - 4) return;
            if (Binding && !Binding.CanSimulateReleased) return;
            rb.isKinematic = true; transform.SetPositionAndRotation(home, homeRotation);
        }
    }
}
