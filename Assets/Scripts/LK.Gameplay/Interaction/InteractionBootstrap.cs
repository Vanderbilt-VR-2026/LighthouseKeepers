using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LighthouseKeepers.Gameplay.Interaction
{
    // Compatibility installer for existing Sprint 2 rigs. E can embed the same resource prefabs.
    public sealed class InteractionBootstrap : MonoBehaviour
    {
        static InteractionBootstrap instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetStatics() => instance = null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (instance) return;
            instance = new GameObject("Session A / physical interactions").AddComponent<InteractionBootstrap>();
            DontDestroyOnLoad(instance.gameObject);
        }
        void OnEnable() => SceneManager.sceneLoaded += Loaded;
        void OnDisable() => SceneManager.sceneLoaded -= Loaded;
        void Loaded(Scene scene, LoadSceneMode mode) => Bind();
        IEnumerator Start()
        {
            // Bounded, infrequent binding accommodates additive scenes and network-created rigs.
            var interval = new WaitForSeconds(1);
            while (true) { Bind(); yield return interval; }
        }
        void Bind()
        {
            foreach (var manager in FindObjectsByType<XRInteractionManager>(FindObjectsSortMode.None))
                if (!manager.GetComponent<DirectGameplayFilter>()) manager.gameObject.AddComponent<DirectGameplayFilter>();
            foreach (var origin in FindObjectsByType<XROrigin>(FindObjectsSortMode.None))
            {
                // Only an enabled camera represents the local rig. Never animate remote-avatar hands from local input.
                if (!origin.Camera || !origin.Camera.isActiveAndEnabled) continue;
                foreach (var t in origin.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name != "Left Controller" && t.name != "Right Controller") continue;
                    foreach (var renderer in t.GetComponentsInChildren<Renderer>(true))
                        if (!(renderer is LineRenderer) && !renderer.GetComponentInParent<HandRig>(true)) renderer.enabled = false;
                    foreach (var oldDirect in t.GetComponentsInChildren<XRDirectInteractor>(true))
                        if (!oldDirect.GetComponentInParent<HandRig>(true)) oldDirect.enabled = false;
                    if (t.GetComponentInChildren<HandRig>(true)) continue;
                    var prefab = Resources.Load<GameObject>(t.name.StartsWith("Left") ? "Hands/PF_Hand_L" : "Hands/PF_Hand_R");
                    if (!prefab) { Debug.LogError("Missing Session A hand prefab.", t); continue; }
                    var old = t.Find("LK Stylized Glove"); if (old) old.gameObject.SetActive(false);
                    Instantiate(prefab, t, false);
                    foreach (var oldDirect in t.GetComponentsInChildren<XRDirectInteractor>(true))
                        if (!oldDirect.GetComponentInParent<HandRig>(true)) oldDirect.enabled = false;
                }
            }
            foreach (var grab in FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None))
            {
                var feedback = grab.GetComponent<PhysicalGrip>();
                if (!feedback) feedback = grab.gameObject.AddComponent<PhysicalGrip>();
                feedback.Configure(grab.attachTransform ? grab.attachTransform : feedback.AttachTransform, HandPose.WrapGrip);
                if (grab.name.ToLowerInvariant().Contains("wrench")) WrenchInteraction.Install(grab);
            }
            StationInteractionBinding.BindLoadedScenes();
        }
    }
}
