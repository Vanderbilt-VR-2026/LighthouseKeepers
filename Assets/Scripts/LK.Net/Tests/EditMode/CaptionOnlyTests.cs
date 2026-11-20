using System;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class CaptionOnlyTests
    {
        static Type Catalog=>Type.GetType("LighthouseKeepers.WatchDemo.RadioLineCatalog, Assembly-CSharp",true);
        [Test] public void ShippingNarrationIsTextOnlyAndKeepsHumanRecordingsAvailable()
        {
            var c=Resources.Load<ScriptableObject>("HarborRadioLines");
            Assert.True((bool)Catalog.GetField("captionsOnly").GetValue(c));
            Assert.NotNull(Catalog.GetMethod("Line").Invoke(c,new object[]{1}),"Audio is preserved for the final recording pass, not deleted.");
            float seconds=(float)Catalog.GetMethod("PresentationSeconds").Invoke(c,new object[]{1});
            Assert.That(seconds,Is.InRange(4f,18f));
            Assert.AreEqual(0f,Catalog.GetMethod("PresentationSeconds").Invoke(c,new object[]{0}));
        }
        [Test] public void MissingAudioDoesNotSuppressReadableLineOrCollapseItsQueueDuration()
        {
            var c=ScriptableObject.CreateInstance(Catalog);
            try{
                Catalog.GetField("lines").SetValue(c,new AudioClip[1]);
                Catalog.GetField("subtitles").SetValue(c,new[]{"Keep the light on. Your partner needs you."});
                Assert.True((bool)Catalog.GetMethod("HasLine").Invoke(c,new object[]{1}));
                Assert.GreaterOrEqual((float)Catalog.GetMethod("PresentationSeconds").Invoke(c,new object[]{1}),4);
                Catalog.GetField("captionsOnly").SetValue(c,false);
                Assert.AreEqual(0f,Catalog.GetMethod("PresentationSeconds").Invoke(c,new object[]{1}));
            }finally{UnityEngine.Object.DestroyImmediate(c);}
        }
        [Test] public void PageSelectionStartsAtFirstPageAndEndsAtLast()
        {
            var c=Resources.Load<ScriptableObject>("HarborRadioLines");
            var go=new GameObject("Caption test");var t=Type.GetType("LighthouseKeepers.WatchDemo.HarborRadio, Assembly-CSharp",true);
            var radio=go.AddComponent(t);
            try{
                t.GetField("catalog",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(radio,c);
                t.GetProperty("Cue").SetValue(radio,1);t.GetProperty("Age").SetValue(radio,0f);
                Assert.AreEqual(1,t.GetMethod("SubtitlePage").Invoke(radio,new object[]{3}));
                string text=(string)t.GetProperty("Subtitle").GetValue(radio);
                Assert.AreEqual(text.Length,t.GetProperty("SubtitleCharacters").GetValue(radio));
                t.GetProperty("Age").SetValue(radio,30f);
                Assert.AreEqual(3,t.GetMethod("SubtitlePage").Invoke(radio,new object[]{3}));
                Assert.AreEqual(0f,t.GetProperty("SubtitleAlpha").GetValue(radio));
            }finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
}
