using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Plain recordings retain their GUID when replaced by human voice with the same filenames.
    public sealed class RadioLineCatalog : ScriptableObject
    {
        public AudioClip[] lines;
        readonly System.Collections.Generic.Dictionary<int,AudioClip> extra=new();
        public const int LastLine=43;
        public string[] subtitles;
        public Vector2[] pauseCenters;
        public string Caption(int id)=>id>=19&&id<=LastLine?KeeperStrings.Get("radio."+id):subtitles!=null&&id>0&&id<=subtitles.Length?subtitles[id-1]:"";
        public Vector2 Pauses(int id)=>pauseCenters!=null&&id>0&&id<=pauseCenters.Length?pauseCenters[id-1]:new Vector2(-10,-10);
        public AudioClip Line(int id){if(id>0&&id<=lines.Length)return lines[id-1];if(id<19||id>LastLine)return null;if(!extra.TryGetValue(id,out var clip)){clip=Resources.Load<AudioClip>("MasterRadio/"+KeeperStrings.Get("radio.file."+id));extra[id]=clip;}return clip;}
    }
}
