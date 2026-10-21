using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LighthouseKeepers.Environment.Sprint3
{
    /// <summary>Presentation only. D supplies the heading and completion flags.</summary>
    public sealed class BriefingBoardView : MonoBehaviour
    {
        [SerializeField] TMP_FontAsset font;
        // Frozen station is 0.20m inboard of the actual north wall face.
        [SerializeField] float wallSurfaceOffsetM = -.20f;
        [SerializeField] string initialHeading = "KEEPER WATCH";
        [SerializeField] string[] initialRows = { "Take the wrench", "Find the leak", "Seal the pipe", "Restore power", "Regroup upstairs" };
        [SerializeField] TextMeshPro heading;
        [SerializeField] TextMeshPro[] labels = new TextMeshPro[5];
        [SerializeField] Transform[] ticks = new Transform[5];
        public const float BodyCapHeightM = .045f;
        public const float HeadingCapHeightM = .065f;
        public const int MaximumRows = 5;

        void Awake()
        {
            Prepare();
            if (string.IsNullOrEmpty(heading.text))
            {
                var rows = new (string, bool)[Mathf.Min(MaximumRows, initialRows.Length)];
                for (int i = 0; i < rows.Length; i++) rows[i] = (initialRows[i], false);
                SetLines(initialHeading, rows);
            }
        }

        // Also called by the editor authoring pass so TMP children are serialized into the prefab.
        public void Prepare()
        {
            if (!font) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (!font) throw new InvalidOperationException("Briefing board requires its TMP font asset.");
            if (!heading) heading = CreateText("Heading", new Vector3(-.54f, 1.70f, .047f), HeadingCapHeightM, 1.08f);
            for (int i = 0; i < MaximumRows; i++)
            {
                if (!labels[i]) labels[i] = CreateText("Checklist row " + (i + 1), new Vector3(-.435f, 1.535f - i * .105f, .047f), BodyCapHeightM, .975f);
                if (!ticks[i]) ticks[i] = transform.Find("Tick " + (i + 1));
            }
        }

        TextMeshPro CreateText(string name, Vector3 position, float capM, float widthM)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(TextMeshPro)).GetComponent<TextMeshPro>();
            text.transform.SetParent(transform, false);
            text.transform.localPosition = position;
            // TMP faces local -Z; station +Z faces the operator.
            text.transform.localRotation = Quaternion.Euler(0, 180, 0);
            // A half-turn reverses local X: move the left origin to the station's right.
            text.transform.localPosition = new Vector3(-position.x, position.y, position.z + wallSurfaceOffsetM);
            text.font = font;
            text.color = Color.black;
            text.fontSharedMaterial = font.material; // TMP distance-field shader is unlit.
            text.richText = false;
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Truncate;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.rectTransform.pivot = new Vector2(0, 1);
            text.rectTransform.sizeDelta = new Vector2(widthM, .09f);
            text.fontSize = 10;
            text.text = "H";
            text.ForceMeshUpdate(true, true);
            if (text.textInfo.characterCount != 1) throw new InvalidOperationException("Cannot measure TMP capital H.");
            var glyph = text.textInfo.characterInfo[0];
            float actual = Mathf.Abs(glyph.topRight.y - glyph.bottomLeft.y);
            if (actual < .00001f) throw new InvalidOperationException("TMP has zero cap height.");
            text.fontSize *= capM / actual;
            text.text = "";
            return text;
        }

        public void SetLines(string title, IReadOnlyList<(string, bool)> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (rows.Count > MaximumRows) throw new ArgumentException("The board has five checklist rows.", nameof(rows));
            Prepare();
            heading.text = title ?? "";
            for (int i = 0; i < MaximumRows; i++)
            {
                labels[i].text = i < rows.Count ? rows[i].Item1 ?? "" : "";
                labels[i].gameObject.SetActive(i < rows.Count);
                // The checkbox outline and tick are authored mesh glyphs: no missing Unicode fallback.
                var outline = transform.Find("Checkbox " + (i + 1));
                if (outline) outline.gameObject.SetActive(i < rows.Count);
                if (ticks[i]) ticks[i].gameObject.SetActive(i < rows.Count && rows[i].Item2);
            }
        }
    }
}
