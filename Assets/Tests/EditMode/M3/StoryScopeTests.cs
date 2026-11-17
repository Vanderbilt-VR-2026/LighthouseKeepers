using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LighthouseKeepers.Core.Progression;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class StoryScopeTests
    {
        [Test] public void ExactlySheetZeroIsActiveWithoutARecording()
        {
            var sheet=File.ReadAllText("Docs/Story/03-Recording-Sheets.md").Split("# OPTIONAL LINES")[0];
            var ids=Regex.Matches(sheet,@"\| \d+ \| `(\w+)\.wav`").Select(m=>m.Groups[1].Value.ToUpperInvariant()).ToArray();
            Assert.AreEqual(52,ids.Length);Assert.AreEqual(52,ids.Distinct().Count());
            var manifest=JsonUtility.FromJson<StoryVoiceAvailability.Manifest>(File.ReadAllText("Assets/Data/Strings/Resources/StoryVoiceAvailability.json"));
            CollectionAssert.AreEquivalent(ids,manifest.selected);
            foreach(var line in StoryVoiceCatalog.Lines)Assert.AreEqual(ids.Contains(line.id)||manifest.recorded.Contains(line.id),StoryVoiceAvailability.Enabled(line.id),line.id);
        }
        [Test] public void OptionalRecordingEnablesOnlyItsOwnIdAndUnknownNamesFail()
        {
            var manifest=new StoryVoiceAvailability.Manifest{selected=new[]{"HM_PIPE_FIXED"}};
            Assert.False(manifest.Enabled("HM_N2_WALTER"));manifest.MarkRecorded("HM_N2_WALTER");manifest.MarkRecorded("HM_N2_WALTER");
            Assert.True(manifest.Enabled("HM_N2_WALTER"));Assert.AreEqual(1,manifest.recorded.Length);
            Assert.True(manifest.Enabled("HM_PIPE_FIXED"));Assert.False(manifest.Enabled("HM_N5_OWEN"));
            Assert.Throws<ArgumentException>(()=>manifest.MarkRecorded("TYPO"));
        }
        [Test] public void SelectedClipsAreShortMonoAndCaptionedExactly()
        {
            var sheet=File.ReadAllText("Docs/Story/03-Recording-Sheets.md").Split("# OPTIONAL LINES")[0];
            foreach(Match match in Regex.Matches(sheet,@"\| \d+ \| `(\w+)\.wav` \| (.*?) \|"))
            {
                var line=StoryVoiceCatalog.Get(match.Groups[1].Value.ToUpperInvariant());
                Assert.AreEqual(match.Groups[2].Value,line.text,line.id);
                var clip=Resources.Load<AudioClip>(line.resource);Assert.NotNull(clip,line.id);
                Assert.AreEqual(1,clip.channels,line.id);Assert.Less(clip.length,10,line.id);Assert.Greater(clip.length,.2,line.id);
            }
        }
        [Test] public void DisabledStoryCannotLeakThroughPaEvenIfItsCueIsValid()
        {
            var catalog=Resources.Load<ScriptableObject>("HarborRadioLines");var lookup=catalog.GetType().GetMethod("Line");
            foreach(var e in StoryVoiceCatalog.Lines.Where(x=>!StoryVoiceAvailability.Enabled(x.id)||x.Antagonist))
                Assert.Null(lookup.Invoke(catalog,new object[]{e.cue}),e.id);
        }
    }
}
