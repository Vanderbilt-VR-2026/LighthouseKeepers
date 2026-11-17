using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Plain recordings retain their GUID when replaced by human voice with the same filenames.
    public sealed class RadioLineCatalog : ScriptableObject
    {
        public AudioClip[] lines;
        readonly System.Collections.Generic.Dictionary<int,AudioClip> extra=new();
        public const int LastLegacyLine=104;
        public const int LastLine=LighthouseKeepers.Core.Progression.StoryVoiceCatalog.LastCue;
        public static bool ValidCue(int id)=>id>=0&&id<=LastLegacyLine||StoryVoiceBridge.RadioCue(id);
        public string[] subtitles;
        public Vector2[] pauseCenters;
        public string Caption(int id)=>id>=200?LighthouseKeepers.Core.Progression.StoryVoiceCatalog.Get(id)?.text??"":id>=19&&id<=LastLegacyLine?KeeperStrings.Get("radio."+id):subtitles!=null&&id>0&&id<=subtitles.Length?subtitles[id-1]:"";
        public Vector2 Pauses(int id)=>pauseCenters!=null&&id>0&&id<=pauseCenters.Length?pauseCenters[id-1]:new Vector2(-10,-10);
        public AudioClip Line(int id){if(id>=200){var line=LighthouseKeepers.Core.Progression.StoryVoiceCatalog.Get(id);if(line==null||line.Antagonist)return null;if(!extra.TryGetValue(id,out var storyClip)){storyClip=Resources.Load<AudioClip>(line.resource);extra[id]=storyClip;}return storyClip;}if(id>0&&id<=lines.Length)return lines[id-1];if(id<19||id>LastLegacyLine)return null;if(!extra.TryGetValue(id,out var clip)){clip=Resources.Load<AudioClip>("MasterRadio/"+KeeperStrings.Get("radio.file."+id));extra[id]=clip;}return clip;}
    }
}
