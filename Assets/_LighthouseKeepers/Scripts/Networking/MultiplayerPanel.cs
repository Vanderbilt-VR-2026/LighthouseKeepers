using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>Room controls available when opening Bootstrap directly. Uses the same canvas for mouse and XR rays.</summary>
public sealed class MultiplayerPanel : MonoBehaviour
{
    KeeperReplicationSession session;
    Canvas canvas;
    RectTransform content;
    TMP_Text status;
    TMP_InputField room, keeper;
    Button host, join, leave;
    int sceneHandle = -1;
    float nextRefresh;
    void Awake() => session = GetComponent<KeeperReplicationSession>();
    void Update()
    {
        var scene = SceneManager.GetSceneByName("LK_Bootstrap");
        if (!scene.isLoaded) { if (canvas) canvas.gameObject.SetActive(false); sceneHandle = -1; return; }
        if (!Camera.main) return;
        if (!canvas) Build();
        if (sceneHandle != scene.handle)
        {
            sceneHandle = scene.handle;
            var camera = Camera.main.transform;
            var forward = Vector3.ProjectOnPlane(camera.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
            canvas.transform.position = camera.position + forward * 2.2f + Vector3.Cross(forward, Vector3.up) * .65f;
            canvas.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            canvas.worldCamera = Camera.main;
            canvas.gameObject.SetActive(true);
            EnsureEventSystem();
        }
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + .2f;
        bool online = session.IsOnline, busy = session.IsConnecting;
        host.interactable = join.interactable = !online && !busy;
        leave.interactable = online || busy;
        keeper.interactable = room.interactable = !online && !busy;
        status.text = !string.IsNullOrEmpty(session.LastError) ? session.LastError : busy ? "Connecting..." : online
            ? session.RoomName + "  •  " + session.Players.Count + "/4 keepers\n" +
              (session.CanInteract ? "Crew connected. Move and grab props together." : "Synchronizing the lighthouse...")
            : "Offline practice. Host a room or join your crew's room.";
    }
    void EnsureEventSystem()
    {
        if (!FindAnyObjectByType<EventSystem>()) new GameObject("Crew EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
    }
    void Build()
    {
        var root = Rect("Crew room board", transform, Vector2.zero, new Vector2(720, 390));
        root.localScale = Vector3.one * .002f;
        canvas = root.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        root.gameObject.AddComponent<GraphicRaycaster>();
        root.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        root.gameObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2;
        content = Rect("Controls", root, Vector2.zero, new Vector2(720, 390));
        content.gameObject.AddComponent<Image>().color = new Color(.055f, .10f, .11f, .97f);
        Label(content, "LIGHTHOUSE CREW • UP TO 4 PLAYERS", new Vector2(0, 154), new Vector2(680, 42), 25);
        keeper = Input(content, new Vector2(-170, 88), "Keeper name", "Keeper");
        keeper.characterLimit = 24;
        room = Input(content, new Vector2(170, 88), "Room name", session.RoomName);
        room.characterLimit = 32;
        host = Button(content, "HOST ROOM", new Vector2(-222, 16), new Vector2(205, 55), () =>
        {
            session.RoomName = room.text; session.SetLocalName(keeper.text); session.HostGame(4);
        });
        join = Button(content, "JOIN ROOM", new Vector2(0, 16), new Vector2(205, 55), () =>
        {
            session.SetLocalName(keeper.text); session.JoinGame(room.text);
        });
        leave = Button(content, "LEAVE ROOM", new Vector2(222, 16), new Vector2(205, 55), session.LeaveGame);
        status = Label(content, "", new Vector2(0, -74), new Vector2(660, 105), 21);
        status.textWrappingMode = TextWrappingModes.Normal;
        // This stays available at the spawn so crew controls can be reopened with a controller ray.
        Button(root, "SHOW / HIDE CREW BOARD", new Vector2(0, -226), new Vector2(360, 52), () => content.gameObject.SetActive(!content.gameObject.activeSelf));
    }
    static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        return rect;
    }
    static TMP_Text Label(Transform parent, string text, Vector2 position, Vector2 size, float fontSize)
    {
        var label = Rect("Label", parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = fontSize; label.color = new Color(.93f, .88f, .74f);
        label.alignment = TextAlignmentOptions.Center; label.richText = false; label.raycastTarget = false;
        label.text = text;
        return label;
    }
    static Button Button(Transform parent, string text, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        var rect = Rect(text, parent, position, size);
        rect.gameObject.AddComponent<Image>().color = new Color(.25f, .34f, .31f);
        var button = rect.gameObject.AddComponent<Button>();
        Label(rect, text, Vector2.zero, size, 20);
        button.onClick.AddListener(action);
        return button;
    }
    static TMP_InputField Input(Transform parent, Vector2 position, string hint, string initial)
    {
        var root = Rect(hint, parent, position, new Vector2(310, 55));
        root.gameObject.AddComponent<Image>().color = new Color(.14f, .2f, .2f);
        var field = root.gameObject.AddComponent<TMP_InputField>();
        var viewport = Rect("Viewport", root, Vector2.zero, new Vector2(285, 52));
        viewport.gameObject.AddComponent<RectMask2D>();
        field.textViewport = viewport;
        field.textComponent = (TextMeshProUGUI)Label(viewport, "", Vector2.zero, new Vector2(280, 52), 22);
        field.placeholder = Label(viewport, hint, Vector2.zero, new Vector2(280, 52), 22);
        field.text = initial;
        return field;
    }
}
