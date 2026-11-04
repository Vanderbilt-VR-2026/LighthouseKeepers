using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class AudioPassTests
    {
        [Test] public void Phase2RadioClipsContainOneShortRecordedTake()
        {
            var catalog=Resources.Load<ScriptableObject>("HarborRadioLines");Assert.NotNull(catalog);
            var clips=((AudioClip[])catalog.GetType().GetField("lines").GetValue(catalog)).Where(c=>c).ToArray();
            Assert.AreEqual(17,clips.Length);
            foreach(var c in clips){Assert.Less(c.length,10,$"{c.name}: multiple takes or unexpectedly long speech");Assert.Greater(c.length,1);Assert.AreEqual(1,c.channels);Assert.AreEqual(44100,c.frequency);}
        }
    }
}
