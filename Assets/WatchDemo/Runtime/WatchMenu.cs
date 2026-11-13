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
        Transform head, soundPage;
        GameObject panel, ownedEventSystem;
        Canvas canvas;
        Text status;
        Button retry, replay, leave, close;
        Button mute,turnMode;
        LighthouseKeepers.Lobby.ICrewVoice voice;
        bool previousMenu, exiting;
        int previousTerminal;
        float revealAt;
        Material inkMaterial;Texture2D paper;Sprite paperSprite;
        bool Book=>watch&&watch.State.Immersion&&watch.State.Immersion.logbook;
        Font InkFont=>Book?Resources.Load<Font>("Fonts/Kalam-Regular"):Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            if(Book){inkMaterial=new Material(Resources.Load<Shader>("ImmersionLogbook"));ImmersionLogbook.Decorate(panel,inkMaterial,out paper,out paperSprite);canvas.overrideSorting=true;canvas.sortingOrder=100;}
            Label("Title", Book?KeeperStrings.Get("menu.log"):KeeperStrings.Get("menu.watch"), 170, Book?55:42, Book?28:32);
            status = Label("Status", "", 100, 84, 23);
            retry = Action("RetryWatchDay", Book?KeeperStrings.Get("menu.retry_night"):KeeperStrings.Get("menu.retry_day"), 10, () => watch.RetryDay());
            replay = Action("ReplayWatchFromDayOne", KeeperStrings.Get("menu.replay"), 10, () => watch.ReplayFromDayOne());
            leave = Action("ReturnWatchToLobby", KeeperStrings.Get("menu.lobby"), -70, ReturnToLobby);
            close = Action("CloseWatchMenu", Book?KeeperStrings.Get("menu.close_log"):KeeperStrings.Get("menu.close"), -150, Close);
            voice=LighthouseKeepers.Lobby.CrewVoiceBridge.Current;
            if(voice!=null)
            {
                ((RectTransform)panel.transform).sizeDelta=new Vector2(600,510);
                mute=Action("ToggleCrewMicrophone",voice.StatusText,-225,()=>{if(voice!=null)voice.ToggleMute();Refresh();});
            }
            if(watch.State.Master&&watch.State.Master.movement){turnMode=Action("TurnComfortMode",KeeperStrings.Get("turn.snap"),-285,()=>{if(KeeperMovement.Current){KeeperMovement.Current.Toggle();turnMode.GetComponentInChildren<Text>().text=KeeperStrings.Get(KeeperMovement.Current.Smooth?"turn.smooth":"turn.snap");}});((RectTransform)panel.transform).sizeDelta=new Vector2(600,640);}
            WatchAudioSettings.Attach(panel,InkFont,inkMaterial,Refresh);
            soundPage=panel.transform.Find("Sound settings page");
            panel.SetActive(false);
        }

        Text Label(string name, string text, float y, float height, int size)
        {
            var rect = Rect(name, new Vector2(0, y), new Vector2(550, height));
            var label = rect.gameObject.AddComponent<Text>();
            label.font = InkFont; label.fontSize = size;
            label.text = text; label.color = Book?new Color(.15f,.095f,.055f):new Color(.95f, .91f, .79f);if(Book)label.material=inkMaterial; label.alignment = TextAnchor.MiddleCenter;
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
            var image = rect.gameObject.AddComponent<Image>(); image.color = Book?new Color(.55f,.42f,.27f,.12f):new Color(.17f, .27f, .27f);if(Book)image.material=inkMaterial;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            var text = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.rectTransform.SetParent(rect, false); text.rectTransform.sizeDelta = rect.sizeDelta;
            text.font = InkFont; text.fontSize = 26; text.text = caption;
            text.alignment = TextAnchor.MiddleCenter; text.color = Book?new Color(.16f,.08f,.045f):Color.white;if(Book)text.material=inkMaterial; text.raycastTarget = false;
            return button;
        }

        void Update()
        {
            if(revealAt>0&&Time.unscaledTime>=revealAt){revealAt=0;if(watch.State.Failed)Open();}
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool pressed);
            bool keyboard = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            if ((pressed && !previousMenu) || keyboard) { if (IsOpen) Close(); else Open(); }
            previousMenu = pressed;
            if(IsOpen&&mute&&voice!=null)mute.GetComponentInChildren<Text>().text=voice.StatusText;
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
            if (becameTerminal){if(Book&&watch.State.Failed)revealAt=Time.unscaledTime+1.05f;else Open();}
            else if (IsOpen && !exiting) Refresh();
        }
        void Refresh()
        {
            var state = watch.State;
            status.text = state.Failed ? state.FailureMessage + KeeperStrings.Get("menu.retry_hint") :
                state.Complete && !state.HasNextDay ? KeeperStrings.Get("menu.complete") :
                string.Format(KeeperStrings.Get("menu.day_status"),state.Day);
            if(state.PressureEnabled&&state.Failed)status.text=KeeperStrings.Get(state.HighWaterFailed?"log.flood":state.BeaconDarkFailed?"log.beacon":"log.time");
            else if(Book)status.text=state.Failed?(state.HighWaterFailed?KeeperStrings.Get("menu.old_flood"):state.BeaconDarkFailed?KeeperStrings.Get("menu.old_beacon"):KeeperStrings.Get("menu.old_time")):state.Complete&&!state.HasNextDay?KeeperStrings.Get("menu.survived"):string.Format(KeeperStrings.Get("menu.night_status"),state.Day);
            if(state.Sea.lost)status.text=KeeperStrings.Get("loss.sea");
            // Host snapshots can refresh every frame. Do not bring main-page controls
            // back over the active Sound page when the watch state changes.
            bool settingsOpen=soundPage&&soundPage.gameObject.activeSelf;
            retry.gameObject.SetActive(!settingsOpen && !state.NetworkRehearsal && state.Failed);
            close.gameObject.SetActive(!settingsOpen && (!Book||!state.Failed));
            replay.gameObject.SetActive(!settingsOpen && !state.NetworkRehearsal && state.Complete && !state.HasNextDay && !state.Failed);
        }

        public void ReturnToLobby()
        {
            if (exiting) return;
            if (!Application.CanStreamedLevelBeLoaded(LobbyScene)) { status.text = KeeperStrings.Get("menu.missing_lobby"); return; }
            exiting = true; retry.interactable = replay.interactable = leave.interactable = close.interactable = false;
            status.text = KeeperStrings.Get("menu.closing");
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
                    status.text = KeeperStrings.Get("menu.close_failed");
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
            if(inkMaterial)Destroy(inkMaterial);if(paperSprite)Destroy(paperSprite);if(paper)Destroy(paper);
        }
    }
}
