using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;

namespace LighthouseKeepers.Tests.M3
{
    public sealed class DrownedHorrorTests
    {
        static Type Runtime(string name) => Type.GetType("LighthouseKeepers.WatchDemo." + name + ", Assembly-CSharp", true);
        static string Poll(object cursor,int revision,DrownedPhase phase,double at,double now,bool enabled=true)
            => cursor.GetType().GetMethod("Poll").Invoke(cursor,new object[]{revision,phase,at,now,enabled}).ToString();

        [Test] public void OneEventPerHostReceiptAndNoRepeatedReveal()
        {
            var c=Activator.CreateInstance(Runtime("DrownedCueCursor"));
            Assert.AreEqual("Approach",Poll(c,1,DrownedPhase.Warning,80,80.1));
            for(int i=0;i<30;i++)Assert.AreEqual("None",Poll(c,1,DrownedPhase.Warning,80,80.1+i*.02));
            Assert.AreEqual("Reveal",Poll(c,1,DrownedPhase.Manifest,83,83.1));
            Assert.AreEqual("None",Poll(c,1,DrownedPhase.Manifest,83,83.2));
            Assert.AreEqual("Retreat",Poll(c,1,DrownedPhase.Retreat,90,90.1));
        }
        [Test] public void LateJoinAndSightingsToggleDoNotReplayStings()
        {
            var c=Activator.CreateInstance(Runtime("DrownedCueCursor"));
            Assert.AreEqual("None",Poll(c,1,DrownedPhase.Manifest,83,88));
            Assert.AreEqual("None",Poll(c,1,DrownedPhase.Manifest,83,88.1,false));
            Assert.AreEqual("None",Poll(c,1,DrownedPhase.Manifest,83,88.2,true));
            Assert.AreEqual("Retreat",Poll(c,1,DrownedPhase.Retreat,90,90.1));
        }
        [Test] public void RetryCanPlayFreshReceiptAtSameWatchTime()
        {
            var c=Activator.CreateInstance(Runtime("DrownedCueCursor"));
            Assert.AreEqual("Reveal",Poll(c,1,DrownedPhase.Manifest,83,83.1));
            Assert.AreEqual("Reveal",Poll(c,2,DrownedPhase.Manifest,83,83.1));
            c.GetType().GetMethod("Clear").Invoke(c,null);
            Assert.AreEqual("Reveal",Poll(c,2,DrownedPhase.Manifest,83,83.1));
        }
        [Test] public void SpecialScoreOnlyAccompaniesAnEnabledLiveAppearance()
        {
            var method=Runtime("DrownedHorror").GetMethod("ScoreWanted");
            foreach(DrownedPhase p in Enum.GetValues(typeof(DrownedPhase)))
                Assert.AreEqual(p==DrownedPhase.Manifest,method.Invoke(null,new object[]{p,true,false}));
            Assert.False((bool)method.Invoke(null,new object[]{DrownedPhase.Manifest,false,false}));
            Assert.False((bool)method.Invoke(null,new object[]{DrownedPhase.Manifest,true,true}));
        }
        [Test] public void SpeechKeepsPriorityOverEntityEffects()
        {
            var method=Runtime("DrownedHorror").GetMethod("SpeechGain");
            Assert.AreEqual(1f,method.Invoke(null,new object[]{false,false}));
            Assert.Less((float)method.Invoke(null,new object[]{true,false}),(float)method.Invoke(null,new object[]{false,true}));
            Assert.Less((float)method.Invoke(null,new object[]{false,true}),1);
        }
        [Test] public void PaletteIsBoundedAndSpatialSoundsAreMono()
        {
            foreach(string n in new[]{"approach","reveal_a","reveal_b","breathing","retreat","score"})
            {
                var c=Resources.Load<AudioClip>("S5Audio/DrownedHorror/"+n);Assert.NotNull(c,n);
                Assert.AreEqual(44100,c.frequency);Assert.AreEqual(n=="score"?2:1,c.channels);
                Assert.LessOrEqual(c.length,n=="score"?24.1f:n=="breathing"?12.1f:4);
                Assert.Greater(c.length,2);
                if(n=="score")Assert.AreEqual(AudioClipLoadType.Streaming,c.loadType);
            }
        }
    }
}
