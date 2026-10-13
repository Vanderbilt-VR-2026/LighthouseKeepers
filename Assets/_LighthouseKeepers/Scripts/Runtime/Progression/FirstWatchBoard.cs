using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LighthouseKeepers.Progression
{
    public enum WatchBoardRole { Briefing, Station, Landing }

    /// <summary>One room-anchored UI for tracked pointers and desktop mouse input.</summary>
    public sealed class FirstWatchBoard : MonoBehaviour
    {
        [SerializeField] FirstWatchDirector director;
        [SerializeField] WatchBoardRole role;
        TMP_Text title, body, status, actionText;
        Button action;
        Image progress;
        Canvas canvas;
        Vector3 home;
        Quaternion homeRotation;
        bool showingResult;
        float nextRefresh;
        static readonly Color Paper = new(.94f, .88f, .73f);
        static readonly Color Brass = new(.85f, .60f, .28f);

        void Awake()
        {
            home = transform.position; homeRotation = transform.rotation;
            Build();
            if (!FindAnyObjectByType<EventSystem>())
                new GameObject("First Watch pointer input", typeof(EventSystem), typeof(XRUIInputModule));
        }
        void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .1f;
            if (!canvas.worldCamera) canvas.worldCamera = Camera.main;
            Refresh();
        }
        void Refresh()
        {
            if (!director || !director.Ready) { body.text = "Preparing the lighthouse…"; action.interactable = false; return; }
            var rules = director.Rules;
            bool terminal = rules.Phase == WatchPhase.Failed || rules.Phase == WatchPhase.Complete;
            if (role == WatchBoardRole.Briefing && terminal != showingResult)
            {
                showingResult = terminal;
                if (terminal && Camera.main)
                {
                    var head = Camera.main.transform;
                    var forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
                    if (forward.sqrMagnitude < .1f) forward = Vector3.forward;
                    transform.position = head.position + forward * 1.6f;
                    transform.rotation = Quaternion.LookRotation(forward);
                }
                else transform.SetPositionAndRotation(home, homeRotation);
            }
            title.text = role == WatchBoardRole.Station ? "PLUMBING / EMERGENCY SEALS" : role == WatchBoardRole.Landing ? "FIRST LANDING / REFUGE" : "FIRST WATCH";
            status.text = rules.SubmergedSeconds > 0 ? $"GET ABOVE WATER • AIR {Mathf.CeilToInt(FirstWatchRules.AirSeconds - rules.SubmergedSeconds)}s" :
                $"WATER {director.WaterHeight:F2}m    •    SEALS {rules.SealedCount}/3";
            status.color = rules.SubmergedSeconds > 0 ? new Color(1,.4f,.3f) : Brass;
            action.gameObject.SetActive(true);
            action.interactable = false;
            actionText.text = "WATCH IN PROGRESS";
            progress.rectTransform.localScale = new Vector3((rules.SealedCount + rules.TurnDegrees / FirstWatchRules.DegreesPerValve) / 3f, 1, 1);
            if (terminal)
            {
                title.text = rules.Phase == WatchPhase.Complete ? "FIRST WATCH SURVIVED" : "THE WATER CAUGHT YOU";
                body.text = rules.Phase == WatchPhase.Complete ?
                    $"You sealed the leak and reached shelter.\n{rules.Elapsed:F0}s • {rules.Mistakes} pressure surges\nThis slice ends here. The lantern is still above." :
                    "Get your head above water before your air runs out.\nBrace first. Follow the marked valve order.\nTry again with a fresh sequence.";
                actionText.text = "RESTART WATCH"; action.interactable = true; return;
            }
            switch (rules.Phase)
            {
                case WatchPhase.Briefing:
                    body.text = role == WatchBoardRole.Station ? $"SEAL ORDER     {director.Sequence}\nGrip the brass brace. Grip and twist each wheel.\nThe clamp can hold the brace for you." :
                        "SURVIVE • REPAIR • CLIMB\nSeal the ground-floor leak, then reach the first landing.\nSolo practice: use both hands, or the timed clamp.";
                    actionText.text = "BEGIN WATCH"; action.interactable = true; break;
                case WatchPhase.Repair:
                    body.text = role == WatchBoardRole.Station ?
                        $"SEAL ORDER     {director.Sequence}\n{(director.Braced ? "BRACE SECURE" : "HOLD BRACE BEFORE TURNING")}    •    {(director.ClampRemaining > 0 ? $"CLAMP {director.ClampRemaining:F0}s" : "GRIP + TWIST THE WHEEL")}\n{(string.IsNullOrEmpty(director.Feedback) ? "Green means sealed. Regrip to keep turning." : director.Feedback)}" :
                        "Contain the leak before retreating upstairs.\nFind the brass controls beside the plumbing manifold.\nHold the brace and turn the wheels in the posted order.";
                    actionText.text = "LATCH CLAMP / 20s";
                    action.interactable = role == WatchBoardRole.Station && director.AtStation; break;
                case WatchPhase.Escape:
                    body.text = "LEAK CONTAINED • TIME BOUGHT\nTake the spiral stairs up one floor.\nStep onto the first landing and catch your breath.";
                    action.gameObject.SetActive(false); break;
                case WatchPhase.Recovery:
                    body.text = $"YOU MADE IT ABOVE THE FLOOD\nTake a breath. Water is holding below.\nRegroup on this floor: {Mathf.CeilToInt(rules.RecoveryRemaining)}s";
                    action.gameObject.SetActive(false); break;
            }
        }
        void Act()
        {
            if (!director) return;
            if (director.Rules.Phase == WatchPhase.Briefing) director.BeginWatch();
            else if (director.Rules.Phase == WatchPhase.Repair) director.LatchClamp();
            else director.RestartWatch();
            Refresh();
        }
        void Build()
        {
            var root = Rect("Watch canvas", transform, Vector2.zero, new(1100, 620));
            root.localScale = Vector3.one * .0014f;
            canvas = root.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            root.gameObject.AddComponent<GraphicRaycaster>();
            root.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            var background = root.gameObject.AddComponent<Image>(); background.color = new(.035f,.08f,.085f,.98f);
            title = Label(root, "Title", new(0,240), new(1020,70), 43, Brass);
            body = Label(root, "Instructions", new(0,55), new(1020,270), 34, Paper);
            status = Label(root, "Status", new(0,-105), new(1020,48), 28, Brass);
            var bar = Rect("Repair progress",root,new(0,-150),new(1000,12)).gameObject.AddComponent<Image>(); bar.color = new(.15f,.25f,.23f);
            progress = Rect("Seals completed",bar.transform,Vector2.zero,new(1000,12)).gameObject.AddComponent<Image>();
            progress.color = new(.32f,.8f,.49f); progress.raycastTarget = false;
            progress.rectTransform.pivot = new Vector2(0,.5f);
            progress.rectTransform.anchoredPosition = new Vector2(-500,0);
            var face = Rect("Watch action",root,new(0,-225),new(1000,95));
            var image = face.gameObject.AddComponent<Image>(); image.color = Brass;
            action = face.gameObject.AddComponent<Button>(); action.targetGraphic = image;
            var colors = action.colors; colors.highlightedColor = new(1,.9f,.65f); colors.pressedColor = new(.6f,.8f,.65f); action.colors = colors;
            actionText = Label(face,"Action text",Vector2.zero,new(950,85),36,new(.04f,.09f,.09f));
            action.onClick.AddListener(Act);
#if UNITY_EDITOR
            var keys = Label(root,"Desktop controls",new(0,-290),new(1060,35),19,Paper);
            keys.text = "WASD move • Q/E turn • Enter begin • Hold B + 1/2/3 to turn • C clamp • R retry";
#endif
        }
        static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false); rect.anchorMin = rect.anchorMax = new(.5f,.5f); rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
        }
        static TMP_Text Label(Transform parent,string name,Vector2 position,Vector2 size,float fontSize,Color color)
        {
            var text = Rect(name,parent,position,size).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset; text.fontSize = fontSize; text.color = color;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal; return text;
        }
    }
}
