using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class StoryImportTests
    {
        [Test] public void ScriptRowsMatchEveryRuntimeCaptionAndFilename()
        {
            var rows=File.ReadAllLines("Docs/Story/02-Voice-Script.md").Where(l=>l.StartsWith("| HM_")||l.StartsWith("| ANT_")).ToArray();
            Assert.AreEqual(112,rows.Length);
            foreach(var row in rows){var cells=row.Trim('|').Split('|').Select(s=>s.Trim()).ToArray();var line=StoryVoiceCatalog.Get(cells[0]);Assert.NotNull(line);Assert.AreEqual(cells[2],line.text);Assert.AreEqual(cells[0].ToLowerInvariant()+".wav",Path.GetFileName(line.file));}
        }
        [Test] public void PaTransportAcceptsOnlyHarborMasterNeverPrivateAntagonistOrUnusedGap()
        {
            var type=Type.GetType("LighthouseKeepers.WatchDemo.RadioLineCatalog, Assembly-CSharp",true);var valid=type.GetMethod("ValidCue");
            foreach(var e in StoryVoiceCatalog.Lines)Assert.AreEqual(!e.Antagonist,valid.Invoke(null,new object[]{e.cue}),e.id);
            foreach(int unknown in new[]{-1,105,150,199,312,999})Assert.False((bool)valid.Invoke(null,new object[]{unknown}));
            Assert.True((bool)valid.Invoke(null,new object[]{0}));Assert.True((bool)valid.Invoke(null,new object[]{104}));
            Assert.AreEqual("LK-S5-LANTERN1",Type.GetType("LighthouseKeepers.Core.CrewNetworkVersion, LighthouseKeepers.Runtime",true).GetField("Matchmaking").GetRawConstantValue());
        }
        [Test] public void ExistingRecordingsStillResolveWithTheirOriginalExactCaptions()
        {
            var catalog=Resources.Load<ScriptableObject>("HarborRadioLines");var caption=catalog.GetType().GetMethod("Caption");
            foreach(var line in StoryVoiceCatalog.Lines)if(!line.Antagonist)Assert.AreEqual(line.text,caption.Invoke(catalog,new object[]{line.cue}));
            Assert.NotNull(catalog.GetType().GetMethod("Line").Invoke(catalog,new object[]{1}));
            // ANT cannot accidentally be broadcast through the PA, even after recordings are imported.
            foreach(var line in StoryVoiceCatalog.Lines.Where(x=>x.Antagonist))Assert.Null(catalog.GetType().GetMethod("Line").Invoke(catalog,new object[]{line.cue}));
        }
        [Test] public void UnsupportedDirectionsAndAbsentCompanionNeverReplaceWorkingInstructions()
        {
            var stateType=Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);
            var state=Activator.CreateInstance(stateType,Resources.Load<FiveNightCatalog>("M3Campaign").nights,1,Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"));
            var replace=Type.GetType("LighthouseKeepers.WatchDemo.StoryVoiceBridge, Assembly-CSharp",true).GetMethod("Replacement");
            foreach(int id in new[]{1,5,82,84,85,86,96})Assert.Null(replace.Invoke(null,new[]{(object)id,state}),"Unsupported legacy cue "+id);
            stateType.GetMethod("SetKeeperCount").Invoke(state,new object[]{2});
            Assert.AreEqual("HM_N1_ARRIVAL_COOP",replace.Invoke(null,new[]{(object)1,state}));
        }
        [Test] public void GeneratorRecoveryNeverAnnouncedWhileEngineIsStillStopped()
        {
            var stateType=Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);
            var state=Activator.CreateInstance(stateType,Resources.Load<FiveNightCatalog>("M3Campaign").nights,2,Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"));
            var engine=stateType.GetProperty("Engine").GetValue(state);
            Assert.False((bool)engine.GetType().GetProperty("Settled").GetValue(engine));
            var replace=Type.GetType("LighthouseKeepers.WatchDemo.StoryVoiceBridge, Assembly-CSharp",true).GetMethod("Replacement");
            Assert.Null(replace.Invoke(null,new[]{(object)21,state}));
        }
    }
}
