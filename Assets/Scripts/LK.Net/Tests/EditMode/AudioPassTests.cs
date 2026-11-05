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
        [Test] public void Phase3SnapshotRejectsInvalidMusicState()
        {
            var t=System.Type.GetType("LighthouseKeepers.WatchDemo.WatchSnapshot, Assembly-CSharp",true);var w=System.Activator.CreateInstance(System.Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true));var s=w.GetType().GetMethod("Capture").Invoke(w,new object[]{.02f});var valid=t.GetMethod("Valid");Assert.True((bool)valid.Invoke(s,null));
            t.GetField("musicMode").SetValue(s,3);Assert.False((bool)valid.Invoke(s,null));
            t.GetField("musicMode").SetValue(s,0);t.GetField("musicSince").SetValue(s,double.NaN);Assert.False((bool)valid.Invoke(s,null));
        }
        [Test] public void Phase3MusicHasThreeLoopsShortStingsAndAuthenticatedState()
        {
            foreach(var name in new[]{"calm","tension","crisis"}){var c=Resources.Load<AudioClip>("AudioPass/"+name);Assert.NotNull(c);Assert.Greater(c.length,30);Assert.AreEqual(AudioClipLoadType.Streaming,c.loadType);}
            foreach(var name in new[]{"fixed","survived","lost"}){var c=Resources.Load<AudioClip>("AudioPass/sting_"+name);Assert.NotNull(c);Assert.That(c.length,Is.InRange(2,6));}
            var type=System.Type.GetType("LighthouseKeepers.WatchDemo.AdaptiveMusic, Assembly-CSharp",true);var choose=type.GetMethod("Choose");
            Assert.AreEqual(0,choose.Invoke(null,new object[]{false,.1f,90d,false,50d}));
            Assert.AreEqual(1,choose.Invoke(null,new object[]{true,.1f,90d,false,200d}));
            Assert.AreEqual(2,choose.Invoke(null,new object[]{true,.8f,90d,false,200d}));
            Assert.AreEqual(2,choose.Invoke(null,new object[]{true,.1f,10d,true,200d}));
            Assert.AreEqual(2,choose.Invoke(null,new object[]{true,.1f,90d,false,50d}));
        }
        [Test] public void Phase4RainZonesAndSourceClipsAreDistinct()
        {
            foreach(var n in new[]{"exterior","interior","roof"}){var c=Resources.Load<AudioClip>("AudioPass/rain_"+n);Assert.NotNull(c);Assert.Greater(c.length,300);Assert.AreEqual(AudioClipLoadType.Streaming,c.loadType);}
            foreach(var n in new[]{"window","metal","puddle","drip"})Assert.NotNull(Resources.Load<AudioClip>("AudioPass/impact_"+n));
            var t=System.Type.GetType("LighthouseKeepers.WatchDemo.RainSoundscape, Assembly-CSharp",true);var f=t.GetMethod("Weights");
            Assert.Greater(((Vector3)f.Invoke(null,new object[]{new Vector3(0,1.65f,-8)})).y,.99f);
            Assert.Greater(((Vector3)f.Invoke(null,new object[]{new Vector3(0,11.4f,0)})).z,.99f);
            Assert.Greater(((Vector3)f.Invoke(null,new object[]{new Vector3(8,1.65f,0)})).x,.99f);
            var door=(Vector3)f.Invoke(null,new object[]{new Vector3(0,1.65f,-4.4f)});Assert.That(door.x,Is.InRange(.1f,.9f));Assert.That(door.x+door.y+door.z,Is.EqualTo(1).Within(.001f));
        }
    }
}
