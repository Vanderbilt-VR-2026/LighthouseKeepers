using TMPro;
using UnityEngine;

namespace LighthouseKeepers.Environment.Sprint3
{
    public sealed class FixtureLabel : MonoBehaviour
    {
        [SerializeField] string text;
        [SerializeField] float capHeightM = .025f;
        [SerializeField] float widthM = .5f;
        [SerializeField] Color color = Color.black;
        public void Prepare()
        {
            if (GetComponentInChildren<TextMeshPro>()) return;
            var label = new GameObject("Inscription", typeof(RectTransform), typeof(TextMeshPro)).GetComponent<TextMeshPro>();
            label.transform.SetParent(transform, false);
            label.transform.localRotation = Quaternion.Euler(0, 180, 0);
            label.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            label.color = color;
            label.richText = false;
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = new Vector2(widthM, .15f);
            label.fontSize = 10;
            label.text = "H";
            label.ForceMeshUpdate(true, true);
            var glyph = label.textInfo.characterInfo[0];
            float cap = Mathf.Abs(glyph.topRight.y - glyph.bottomLeft.y);
            if (cap > .00001f) label.fontSize *= capHeightM / cap;
            label.text = text;
        }
        void Awake() => Prepare();
    }
}
