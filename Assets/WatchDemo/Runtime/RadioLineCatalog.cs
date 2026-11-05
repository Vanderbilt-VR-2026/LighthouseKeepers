using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Plain recordings retain their GUID when replaced by human voice with the same filenames.
    public sealed class RadioLineCatalog : ScriptableObject
    {
        public AudioClip[] lines;
        public string[] subtitles;
        public Vector2[] pauseCenters;
        public string Caption(int id)=>subtitles!=null&&id>0&&id<=subtitles.Length?subtitles[id-1]:"";
        public Vector2 Pauses(int id)=>pauseCenters!=null&&id>0&&id<=pauseCenters.Length?pauseCenters[id-1]:new Vector2(-10,-10);
        public AudioClip Line(int id)=>id>0&&id<=lines.Length?lines[id-1]:null;
    }
}
