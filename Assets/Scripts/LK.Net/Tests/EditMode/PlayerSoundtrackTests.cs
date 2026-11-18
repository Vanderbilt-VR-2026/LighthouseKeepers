using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class PlayerSoundtrackTests
    {
        [Test]public void LongBedStreamsStereoAndAccentIsExactlyFiveSeconds()
        {
            var bed=Resources.Load<AudioClip>("PlayerScore/main-loop");var hit=Resources.Load<AudioClip>("PlayerScore/intense-hit");
            Assert.NotNull(bed);Assert.NotNull(hit);Assert.AreEqual(2,bed.channels);Assert.AreEqual(2,hit.channels);
            Assert.AreEqual(AudioClipLoadType.Streaming,bed.loadType);Assert.That(bed.length,Is.InRange(390,402));
            Assert.That(hit.length,Is.EqualTo(5).Within(.025));Assert.AreEqual(AudioClipLoadType.CompressedInMemory,hit.loadType);
        }
        [Test]public void RepeatedFramesAndOverlappingEventsDoNotStackOrRestart()
        {var g=new MusicAccentGate();Assert.True(g.Try(10,0,8,false));Assert.False(g.Try(10.01,0,8,true));Assert.False(g.Try(16,0,8,false));Assert.True(g.Try(18,0,8,false));}
        [Test]public void LateJoinAndStaleScareNeverProduceDelayedStingers()
        {var g=new MusicAccentGate();Assert.False(g.Try(10,3,8,false));Assert.True(g.Try(11,.25,8,false));}
        [Test]public void InvalidTimingNeverConsumesTheNextValidBeat()
        {var g=new MusicAccentGate();Assert.False(g.Try(double.NaN,0,8,false));Assert.False(g.Try(0,-1,8,false));Assert.False(g.Try(0,0,float.NaN,false));Assert.True(g.Try(0,0,8,false));}
        [Test]public void NewScoreKeepsExistingMusicSliderAndSevenDbSpeechDucking()
        {Assert.True(AudioPassSettings.Current.providedSoundtrack);Assert.AreEqual(-7,KeeperAudioMix.MusicSpeechDuck(true,false));Assert.AreEqual(-7,KeeperAudioMix.MusicSpeechDuck(false,true));Assert.AreEqual(0,KeeperAudioMix.MusicSpeechDuck(false,false));}
    }
}
