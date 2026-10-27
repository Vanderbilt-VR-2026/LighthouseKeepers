using System.Collections;
using System.Collections.Generic;
using LighthouseKeepers.Lobby;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LighthouseKeepers.WatchDemo
{
    /// <summary>Scene-owned, on-demand actions. Moves its own canvas, never the tracked rig.</summary>
    public sealed class WatchMenu : MonoBehaviour
    {
        const string LobbyScene = "LK_NetworkLobby";
        WatchDemoController watch;
        Transform head;
        GameObject panel, ownedEventSystem;
        Canvas canvas;
        Text status;
        Button retry, replay, leave, close;
        bool previousMenu, exiting;
        int previousTerminal;
        public bool IsOpen => panel && panel.activeSelf;

        public void Initialize(WatchDemoController controller, Transform trackedHead)
        {
            watch = controller; head = trackedHead;
            EnsureUiInput();
            Build();
            watch.State.Changed += StateChanged;
            StateChanged();
        }

        void EnsureUiInput()
        {
            var events = FindAnyObjectByType<EventSystem>();
            if (!events)
            {
                ownedEventSystem = new GameObject("Watch menu EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
                ownedEventSystem.transform.SetParent(transform, false);
                events = ownedEventSystem.GetComponent<EventSystem>();
            }
            // A plain EventSystem or desktop-only module cannot receive the authored XR UI rays.
            var xr = events.GetComponent<XRUIInputModule>();
            if (!xr)
            {
                xr = events.gameObject.AddComponent<XRUIInputModule>();
            }
            foreach (var module in events.GetComponents<BaseInputModule>()) if (module != xr) module.enabled = false;
            xr.enabled = true; xr.enableXRInput = true; xr.enableMouseInput = true;
        }

        void Build()
        {
            panel = new GameObject("Watch action menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster), typeof(GraphicRaycaster), typeof(Image));
            panel.transform.SetParent(transform, false);
            panel.transform.localScale = Vector3.one * .0015f;
            ((RectTransform)panel.transform).sizeDelta = new Vector2(600, 430);
            canvas = panel.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = head.GetComponent<Camera>();
            // Utility actions stay targetable even when a nearby prop crosses the UI ray.
            var tracked = panel.GetComponent<TrackedDeviceGraphicRaycaster>();
            tracked.checkFor3DOcclusion = tracked.checkFor2DOcclusion = false;
            panel.GetComponent<GraphicRaycaster>().blockingObjects = GraphicRaycaster.BlockingObjects.None;
            panel.GetComponent<Image>().color = new Color(.035f, .06f, .065f, .98f);
            Label("Title", "KEEPER WATCH", 170, 42, 32);
            status = Label("Status", "", 100, 84, 23);
            retry = Action("RetryWatchDay", "RETRY THIS DAY", 10, () => watch.RetryDay());
            replay = Action("ReplayWatchFromDayOne", "REPLAY FROM DAY 1", 10, () => watch.ReplayFromDayOne());
            leave = Action("ReturnWatchToLobby", "RETURN TO LOBBY", -70, ReturnToLobby);
            close = Action("CloseWatchMenu", "CLOSE MENU", -150, Close);
            panel.SetActive(false);
        }

        Text Label(string name, string text, float y, float height, int size)
        {
            var rect = Rect(name, new Vector2(0, y), new Vector2(550, height));
            var label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = size;
            label.text = text; label.color = new Color(.95f, .91f, .79f); label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false; label.horizontalOverflow = HorizontalWrapMode.Wrap;
            return label;
        }
        RectTransform Rect(string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(panel.transform, false); rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }
        Button Action(string name, string caption, float y, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(name, new Vector2(0, y), new Vector2(520, 62));
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.17f, .27f, .27f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            var text = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.rectTransform.SetParent(rect, false); text.rectTransform.sizeDelta = rect.sizeDelta;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 26; text.text = caption;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false;
            return button;
        }

        void Update()
        {
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool pressed);
            bool keyboard = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            if ((pressed && !previousMenu) || keyboard) { if (IsOpen) Close(); else Open(); }
            previousMenu = pressed;
        }

        public void Open()
        {
            if (!head || !panel) return;
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < .5f) forward = Vector3.forward;
            float distance = 1.25f;
            if (Physics.Raycast(head.position, forward, out var hit, distance, ~0, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(.1f, hit.distance - .1f);
            // Bring the menu in front of a wall instead of embedding it. Scale proportionally
            // to keep the same readable angular size; the tracked view and hands stay untouched.
            panel.transform.localScale = Vector3.one * (.0015f * distance / 1.25f);
            panel.transform.SetPositionAndRotation(head.position + forward * distance, Quaternion.LookRotation(forward, Vector3.up));
            panel.SetActive(true); Refresh();
        }
        public void Close() { if (!exiting && panel) panel.SetActive(false); }
        void StateChanged()
        {
            int terminal = watch.State.Failed ? 1 : watch.State.Complete && !watch.State.HasNextDay ? 2 : 0;
            bool becameTerminal = terminal != 0 && terminal != previousTerminal;
            previousTerminal = terminal;
            if (becameTerminal) Open();
            else if (IsOpen && !exiting) Refresh();
        }
        void Refresh()
        {
            var state = watch.State;
            status.text = state.Failed ? state.FailureMessage + "\nRetry keeps this day." :
                state.Complete && !state.HasNextDay ? "ALL WATCHES COMPLETE\nReplay resets the full watch to Day 1." :
                "DAY " + state.Day + "\nThe watch continues while this menu is open.";
            retry.gameObject.SetActive(state.Failed);
            replay.gameObject.SetActive(state.Complete && !state.HasNextDay && !state.Failed);
        }

        public void ReturnToLobby()
        {
            if (exiting) return;
            if (!Application.CanStreamedLevelBeLoaded(LobbyScene)) { status.text = "Lobby is missing from this build."; return; }
            exiting = true; retry.interactable = replay.interactable = leave.interactable = close.interactable = false;
            status.text = "Closing crew connection…";
            StartCoroutine(Leave());
        }
        IEnumerator Leave()
        {
            // Depend only on the existing lobby seam. Fusion and the crew wrapper both make
            // LeaveGame idempotent; IsBusy remains true until their runner shutdown completes.
            var sessions = new List<MonoBehaviour>();
            foreach (var component in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (component is INetworkTransport) sessions.Add(component);
            foreach (var component in sessions) if (component) ((INetworkTransport)component).LeaveGame();
            float deadline = Time.realtimeSinceStartup + 30;
            while (true)
            {
                bool waiting = false;
                foreach (var component in sessions)
                    if (component && (((INetworkTransport)component).IsOnline || component is ISessionLobbyTransport session && session.IsBusy)) waiting = true;
                if (!waiting) break;
                if (Time.realtimeSinceStartup >= deadline)
                {
                    exiting = false; retry.interactable = replay.interactable = leave.interactable = close.interactable = true;
                    status.text = "Crew connection did not close. Try Return to lobby again.";
                    yield break;
                }
                yield return null;
            }
            // Allow transport-owned runner destruction to finish. A crew transport may already
            // have loaded the lobby and destroyed this menu; offline solo reaches the fallback.
            yield return null;
            LighthouseKeepers.StormWatch.SoloWatchSession.Cancel();
            foreach (var component in sessions) if (component) Destroy(component.gameObject);
            SceneManager.LoadSceneAsync(LobbyScene);
        }
        void OnDestroy()
        {
            if (watch && watch.State != null) watch.State.Changed -= StateChanged;
            if (ownedEventSystem) Destroy(ownedEventSystem);
        }
    }
}
