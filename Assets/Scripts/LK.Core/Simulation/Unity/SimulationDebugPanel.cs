#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using LighthouseKeepers.Core.Simulation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LighthouseKeepers.Simulation.Unity
{
    /// <summary>Attach to a scene-owned, standing-height panel anchor facing the player.
    /// Uses the scene's existing EventSystem and XR UI ray. No camera or rig changes.</summary>
    public sealed class SimulationDebugPanel : MonoBehaviour
    {
        [SerializeField] SimulationHostBehaviour simulation;
        Text readout, targetLabel;
        Font font;
        float targetHeight = .9f;

        void Start()
        {
            if (!simulation) { Debug.LogError("Debug panel requires an explicit simulation host.", this); enabled = false; return; }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("Simulation debug canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster));
            root.transform.SetParent(transform, false);
            root.transform.localScale = Vector3.one * .001f;
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            var rect = (RectTransform)root.transform; rect.sizeDelta = new Vector2(1200, 1400);
            var background = root.AddComponent<Image>(); background.color = new Color(.025f, .04f, .055f, .98f);
            readout = Label(rect, new Vector2(0, 290), new Vector2(1100, 740), 38);
            AddButton(rect, "LEAK ON", new Vector2(-280, -155), () => Send(SimCommandKind.DebugSetLeak, 1));
            AddButton(rect, "LEAK OFF", new Vector2(280, -155), () => Send(SimCommandKind.DebugSetLeak, 0));
            AddButton(rect, "GENERATOR FAULT", new Vector2(-280, -275), () => simulation.ForceGeneratorFault());
            AddButton(rect, "RESET RUN", new Vector2(280, -275), () => Send(SimCommandKind.ResetRun));
            targetLabel = Label(rect, new Vector2(0, -395), new Vector2(1100, 90), 38);
            AddButton(rect, "HEIGHT -0.1 m", new Vector2(-280, -510), () => targetHeight -= .1f);
            AddButton(rect, "HEIGHT +0.1 m", new Vector2(280, -510), () => targetHeight += .1f);
            AddButton(rect, "APPLY HEIGHT", new Vector2(-280, -630), () => Send(SimCommandKind.DebugSetWaterHeight, targetHeight));
            AddButton(rect, "USE CURRENT", new Vector2(280, -630), () =>
            { if (simulation.IsInitialized) targetHeight = simulation.Current.WaterHeightM; });
            simulation.OnTick += Render;
            if (simulation.IsInitialized) Render(simulation.Current);
        }

        void Send(SimCommandKind kind, float value = 0)
        { if (simulation.IsInitialized) simulation.Enqueue(new SimCommand(kind, value: value)); }

        void Update() { if (targetLabel) targetLabel.text = $"Set world Y: {targetHeight:F2} m"; }

        void Render(SimSnapshot s)
        {
            string eta = float.IsPositiveInfinity(s.TimeToFailSeconds) ? "--" : $"{s.TimeToFailSeconds:F1} s";
            readout.text = $"SIMULATION / tick {s.Tick}\n" +
                $"Water Y    {s.WaterHeightM:F3} m\nVolume      {s.WaterVolumeM3:F3} m³\n" +
                $"Inflow        {s.InflowM3PerSec:F3} m³/s\nOutflow      {s.OutflowM3PerSec:F3} m³/s\n" +
                $"Net             {s.InflowM3PerSec - s.OutflowM3PerSec:+0.000;-0.000;0.000} m³/s\n" +
                $"Fail ETA     {eta} (no new strokes)\nPower        {s.PowerAvailable:P0}\n" +
                $"Leak            {(s.LeakActive ? "ACTIVE" : "SEALED")}\nPhase         {s.Phase}\n" +
                $"Fuel             {s.FuelRemainingSeconds:F0} s\n" +
                $"Plinth          {(s.GeneratorGeometryAvailable ? (s.GeneratorFlooded ? "FLOODED" : "DRY") : "MISSING")}\n" +
                $"{s.LastRejectReason}";
        }

        Text Label(RectTransform parent, Vector2 position, Vector2 size, int fontSize)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var label = go.GetComponent<Text>(); label.font = font; label.fontSize = fontSize;
            label.color = Color.white; label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.raycastTarget = false;
            return label;
        }

        void AddButton(RectTransform parent, string caption, Vector2 position, Action action)
        {
            var go = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(525, 100);
            go.GetComponent<Image>().color = new Color(.14f, .24f, .32f);
            var button = go.GetComponent<Button>(); button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(() => action());
            var label = Label(rect, Vector2.zero, new Vector2(490, 95), 34);
            label.alignment = TextAnchor.MiddleCenter; label.text = caption;
        }

        void OnDestroy() { if (simulation) simulation.OnTick -= Render; }
    }
}
#endif
