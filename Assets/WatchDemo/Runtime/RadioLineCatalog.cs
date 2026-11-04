using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Plain recordings retain their GUID when replaced by human voice with the same filenames.
    public sealed class RadioLineCatalog : ScriptableObject
    {
        public AudioClip[] lines;
        public AudioClip Line(int id)=>id>0&&id<=lines.Length?lines[id-1]:null;
    }
}
