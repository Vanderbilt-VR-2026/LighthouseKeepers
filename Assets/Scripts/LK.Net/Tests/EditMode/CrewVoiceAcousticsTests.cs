using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class CrewVoiceAcousticsTests
    {
        CrewVoiceAcousticsSettings s;
        [SetUp] public void Create() => s = ScriptableObject.CreateInstance<CrewVoiceAcousticsSettings>();
        [TearDown] public void Destroy() => Object.DestroyImmediate(s);

        [Test] public void SettingsAssetLoadsFromResourcesWithOrderedFloorFalloff()
        {
            var asset = Resources.Load<CrewVoiceAcousticsSettings>(CrewVoiceAcousticsSettings.ResourceName);
            Assert.NotNull(asset);
            Assert.Less(asset.oneFloorGain, 1); Assert.LessOrEqual(asset.twoFloorsGain, asset.oneFloorGain);
            Assert.Less(asset.oneFloorCutoffHz, asset.houseDoorCutoffHz); Assert.LessOrEqual(asset.twoFloorsCutoffHz, asset.oneFloorCutoffHz);
            Assert.Less(asset.radioLowCutHz, asset.radioHighCutHz); Assert.Less(asset.squelchCloseRms, asset.squelchOpenRms);
        }
        [Test] public void FloorIndexMatchesShellStoreysAndObjectiveHudThresholds()
        {
            Assert.AreEqual(0, CrewVoiceAcoustics.FloorIndex(1.6f, s));
            Assert.AreEqual(0, CrewVoiceAcoustics.FloorIndex(3.35f, s)); // ObjectiveHud.FloorAt: head y < 3.4 is floor 1
            Assert.AreEqual(1, CrewVoiceAcoustics.FloorIndex(3.45f, s));
            Assert.AreEqual(1, CrewVoiceAcoustics.FloorIndex(4.9f, s));
            Assert.AreEqual(2, CrewVoiceAcoustics.FloorIndex(8.2f, s));
            Assert.AreEqual(3, CrewVoiceAcoustics.FloorIndex(11.5f, s));
            Assert.AreEqual(0, CrewVoiceAcoustics.FloorIndex(-2, s));
            Assert.AreEqual(3, CrewVoiceAcoustics.FloorIndex(40, s));
            Assert.True(CrewVoiceAcoustics.InHouse(new Vector3(-2, 1.6f, -8), s));
            Assert.False(CrewVoiceAcoustics.InHouse(new Vector3(0, 1.6f, 0), s));
            Assert.False(CrewVoiceAcoustics.InHouse(new Vector3(0, 4.9f, -8), s), "Only the ground floor has a House");
        }
        [Test] public void SameFloorIsClearOneFloorIsMuffledTwoFloorsIsSilent()
        {
            CrewVoiceAcoustics.Occlusion(0, false, s, out float g0, out float c0);
            Assert.AreEqual(1, g0); Assert.GreaterOrEqual(c0, CrewVoiceAcoustics.BypassCutoffHz);
            CrewVoiceAcoustics.Occlusion(1, false, s, out float g1, out float c1);
            Assert.AreEqual(s.oneFloorGain, g1); Assert.AreEqual(s.oneFloorCutoffHz, c1);
            Assert.Less(g1, .5f); Assert.Less(c1, 1000);
            CrewVoiceAcoustics.Occlusion(-1, false, s, out float down, out _);
            Assert.AreEqual(g1, down, "Separation is symmetric");
            CrewVoiceAcoustics.Occlusion(2, false, s, out float g2, out float c2);
            Assert.AreEqual(0, g2); Assert.LessOrEqual(c2, c1);
            CrewVoiceAcoustics.Occlusion(3, false, s, out float g3, out _);
            Assert.AreEqual(0, g3);
        }
        [Test] public void HouseDoorwayAttenuatesAndCombinesWithFloors()
        {
            CrewVoiceAcoustics.Occlusion(new Vector3(-2, 1.6f, -8), new Vector3(0, 1.6f, 0), s, out float door, out float doorCut);
            Assert.AreEqual(s.houseDoorGain, door, 1e-5f); Assert.AreEqual(s.houseDoorCutoffHz, doorCut);
            CrewVoiceAcoustics.Occlusion(new Vector3(-2, 1.6f, -8), new Vector3(0, 4.9f, 0), s, out float up, out float upCut);
            Assert.AreEqual(s.houseDoorGain * s.oneFloorGain, up, 1e-5f); Assert.AreEqual(Mathf.Min(s.oneFloorCutoffHz, s.houseDoorCutoffHz), upCut);
            CrewVoiceAcoustics.Occlusion(new Vector3(1, 8.2f, 0), new Vector3(-1, 8.2f, 1), s, out float same, out _);
            Assert.AreEqual(1, same);
        }
        [Test] public void DistanceGainMatchesUnityLinearRolloff()
        {
            Assert.AreEqual(1, CrewVoiceAcoustics.DistanceGain(1, 2, 12));
            Assert.AreEqual(.5f, CrewVoiceAcoustics.DistanceGain(7, 2, 12), 1e-5f);
            Assert.AreEqual(0, CrewVoiceAcoustics.DistanceGain(20, 2, 12));
        }
        [Test] public void HoldingRadioWhenFarRoutesThroughRadioAndNearStaysProximity()
        {
            float threshold = s.radioWhenProximityBelow;
            Assert.AreEqual(CrewVoiceRoute.Radio, CrewVoiceAcoustics.Route(true, true, true, true, 0, threshold), "two floors apart, both holding");
            Assert.AreEqual(CrewVoiceRoute.Radio, CrewVoiceAcoustics.Route(true, true, true, true, s.oneFloorGain * .9f, threshold), "one floor apart is muffled enough");
            Assert.AreEqual(CrewVoiceRoute.Proximity, CrewVoiceAcoustics.Route(true, true, true, true, 1, threshold), "same room: just talk");
            Assert.AreEqual(CrewVoiceRoute.Proximity, CrewVoiceAcoustics.Route(true, false, true, true, 0, threshold), "listener has no radio");
            Assert.AreEqual(CrewVoiceRoute.Proximity, CrewVoiceAcoustics.Route(true, true, false, true, 0, threshold), "partner has no radio");
            Assert.AreEqual(CrewVoiceRoute.Radio, CrewVoiceAcoustics.Route(true, true, false, false, 0, threshold), "local-only fallback when ownership is not replicated");
        }
        [Test] public void SoloNeverRoutesVoice()
        {
            Assert.AreEqual(CrewVoiceRoute.None, CrewVoiceAcoustics.Route(false, true, true, true, 0, s.radioWhenProximityBelow));
            Assert.AreEqual(CrewVoiceRoute.None, CrewVoiceAcoustics.Route(false, false, false, false, 1, s.radioWhenProximityBelow));
        }
        [Test] public void SquelchOpensOnSpeechHoldsThroughGapsAndClosesOnceAfterHang()
        {
            var gate = new CrewSquelchGate();
            Assert.AreEqual(0, gate.Step(.001f, .02f, .02f, .01f, .3f));
            Assert.AreEqual(1, gate.Step(.05f, .02f, .02f, .01f, .3f));
            Assert.AreEqual(0, gate.Step(.05f, .02f, .02f, .01f, .3f), "no repeated open click");
            Assert.AreEqual(0, gate.Step(.001f, .2f, .02f, .01f, .3f), "short pause between words");
            Assert.AreEqual(0, gate.Step(.04f, .02f, .02f, .01f, .3f));
            Assert.AreEqual(0, gate.Step(.001f, .2f, .02f, .01f, .3f));
            Assert.AreEqual(-1, gate.Step(.001f, .2f, .02f, .01f, .3f));
            Assert.False(gate.Open);
            Assert.AreEqual(0, gate.Step(.001f, 1, .02f, .01f, .3f), "no repeated close click");
        }
        [Test] public void OcclusionLowPassMufflesHighsKeepsLowsAndBypassesWhenClear()
        {
            var lp = new CrewVoiceLowPass(); lp.SetCutoff(550, 48000);
            var high = new float[4096]; for (int i = 0; i < high.Length; i++) high[i] = (i / 2) % 2 == 0 ? .5f : -.5f; // 12 kHz stereo square
            lp.Process(high, 2); Assert.Less(Peak(high, 1024), .01f);
            var dc = new float[4096]; for (int i = 0; i < dc.Length; i++) dc[i] = .5f;
            lp.Process(dc, 2); Assert.AreEqual(.5f, dc[dc.Length - 1], .01f);
            lp.SetCutoff(22000, 48000); Assert.True(lp.Bypassed);
            var copy = new float[256]; for (int i = 0; i < copy.Length; i++) copy[i] = Mathf.Sin(i * .9f) * .3f; var expect = (float[])copy.Clone();
            lp.Process(copy, 2); CollectionAssert.AreEqual(expect, copy);
        }
        [Test] public void RadioStripPassesSpeechBandRejectsLowRumbleAndStaysBounded()
        {
            float voice = BandPeak(1000), rumble = BandPeak(60);
            Assert.Greater(voice, .2f); Assert.Less(rumble, voice * .25f);
            var strip = new CrewRadioStrip(); strip.Configure(s.radioLowCutHz, s.radioHighCutHz, s.radioCrushRateHz, s.radioBits, s.radioDrive, s.radioHiss, 48000);
            strip.HissEnabled = true;
            var loud = new float[4096]; for (int i = 0; i < loud.Length; i++) loud[i] = Mathf.Sin(i * .13f) * 3;
            strip.Process(loud, 2); Assert.Greater(strip.InputRms, 1);
            foreach (var x in loud) { Assert.False(float.IsNaN(x)); Assert.LessOrEqual(Mathf.Abs(x), 1.0001f); }
        }

        float BandPeak(float hz)
        {
            var strip = new CrewRadioStrip(); strip.Configure(s.radioLowCutHz, s.radioHighCutHz, 48000, 16, 1, 0, 48000);
            var data = new float[48000]; for (int i = 0; i < data.Length; i += 2) data[i] = data[i + 1] = Mathf.Sin(2 * Mathf.PI * hz * (i / 2) / 48000f) * .5f;
            strip.Process(data, 2); return Peak(data, data.Length / 2);
        }
        static float Peak(float[] data, int from) { float p = 0; for (int i = from; i < data.Length; i++) p = Mathf.Max(p, Mathf.Abs(data[i])); return p; }
    }
}
