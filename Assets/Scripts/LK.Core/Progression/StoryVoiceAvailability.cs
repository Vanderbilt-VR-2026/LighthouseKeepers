using System;
using System.Linq;
using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    // Content availability is distinct from safe trigger eligibility. A recording never invents a mechanic.
    public static class StoryVoiceAvailability
    {
        [Serializable] public sealed class Manifest
        {
            public int schema=1;
            public string[] selected=Array.Empty<string>(),recorded=Array.Empty<string>();
            public bool Enabled(string id)=>id!=null&&(Array.IndexOf(selected,id)>=0||Array.IndexOf(recorded,id)>=0);
            public void MarkRecorded(string id)
            {
                if(StoryVoiceCatalog.Get(id)==null)throw new ArgumentException("Unknown voice ID",nameof(id));
                if(Array.IndexOf(recorded,id)<0)recorded=recorded.Concat(new[]{id}).ToArray();
            }
        }
        static Manifest data;
        public static bool Enabled(string id)
        {
            if(data==null){var asset=Resources.Load<TextAsset>("StoryVoiceAvailability");data=asset?JsonUtility.FromJson<Manifest>(asset.text):new Manifest();}
            return data.schema==1&&data.Enabled(id);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate()=>data=null;
    }
}
