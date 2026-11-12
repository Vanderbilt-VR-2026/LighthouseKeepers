using System;
using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Script text and word times are generated from the read-only original recording alignment.
    [Serializable] public sealed class HarborVoiceEntry
    {
        public int id,number;
        public string file,caption;
        public float duration;
        public float[] wordStarts;
        public int[] characterEnds;
        public bool needsCheck,longSingleTake;
        public int VisibleCharacters(float age)
        {
            if(age<0)return 0;
            if(age>=duration-.08f)return caption?.Length??0;
            if(wordStarts==null||characterEnds==null||wordStarts.Length!=characterEnds.Length)
                return Mathf.CeilToInt((caption?.Length??0)*Mathf.Clamp01(age/Mathf.Max(.1f,duration)));
            int count=0;for(int i=0;i<wordStarts.Length&&wordStarts[i]<=age;i++)count=characterEnds[i];return count;
        }
    }
    public static class HarborVoiceTiming
    {
        [Serializable] sealed class Table {public HarborVoiceEntry[] entries;}
        static Dictionary<int,HarborVoiceEntry> entries;
        public static HarborVoiceEntry Get(int id)
        {
            if(entries==null){entries=new();var asset=Resources.Load<TextAsset>("HarborVoiceTiming");if(asset)foreach(var e in JsonUtility.FromJson<Table>(asset.text).entries)entries[e.id]=e;}
            return entries.TryGetValue(id,out var value)?value:null;
        }
        public static void Invalidate(){entries=null;}
    }
}
