using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;

namespace LighthouseKeepers.WatchDemo
{
    public enum DrownedSoundCue { None, Approach, Reveal, Retreat }

    // A local playback receipt, never an encounter scheduler. Rejoin/Off cannot replay old stings.
    public sealed class DrownedCueCursor
    {
        int revision = -1;
        DrownedPhase phase;
        double since = -1;
        bool muted;
        public void Clear() { revision = -1; since = -1; muted = false; }
        public DrownedSoundCue Poll(int nextRevision, DrownedPhase next, double at, double now, bool enabled)
        {
            bool changed = revision != nextRevision || phase != next || since != at;
            revision = nextRevision; phase = next; since = at;
            if (!enabled) { muted = true; return DrownedSoundCue.None; }
            if (muted) { muted = false; return DrownedSoundCue.None; }
            if (!changed || now < at || now - at > 1.25) return DrownedSoundCue.None;
            return next == DrownedPhase.Warning ? DrownedSoundCue.Approach :
                next == DrownedPhase.Manifest ? DrownedSoundCue.Reveal :
                next == DrownedPhase.Retreat ? DrownedSoundCue.Retreat : DrownedSoundCue.None;
        }
    }

    // Two bounded spatial voices; the score reuses AdaptiveMusic's two existing decks.
    [DefaultExecutionOrder(250)]
    public sealed class DrownedHorror : MonoBehaviour
    {
        WatchDemoController watch;
        DrownedAppearance appearance;
        AudioSource events, breathing;
        AudioClip approach, revealA, revealB, retreat;
        readonly DrownedCueCursor cursor = new();
        float loopGain, speechGain = 1, eventGain;
        int revision = -1;
        double lastElapsed;
        public DrownedSoundCue LastCue { get; private set; }
        public int CueCount { get; private set; }
        public static bool ScoreWanted(DrownedPhase phase, bool enabled, bool ended) => enabled && !ended && phase == DrownedPhase.Manifest;
        public static float SpeechGain(bool voice, bool radio) => voice ? .45f : radio ? .65f : 1;
        public static void Create(WatchDemoController watch, DrownedAppearance appearance, Transform figure)
        {
            var p = figure.gameObject.AddComponent<DrownedHorror>();
            p.watch = watch; p.appearance = appearance; p.Setup();
        }
        void Setup()
        {
            AudioClip Load(string name) => Resources.Load<AudioClip>("S5Audio/DrownedHorror/" + name);
            approach = Load("approach"); revealA = Load("reveal_a"); revealB = Load("reveal_b"); retreat = Load("retreat");
            if(AudioPassSettings.Current&&AudioPassSettings.Current.demoSoundtrack){var hit=Resources.Load<AudioClip>("DemoHorror/scare");if(hit)revealA=revealB=hit;}
            events = Source("Drowned footsteps and reveal", false);
            breathing = Source("Drowned wet breathing", true); breathing.clip = Load("breathing");
        }
        AudioSource Source(string name, bool loop)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false); go.transform.localPosition = Vector3.up * 1.5f;
            var a = go.AddComponent<AudioSource>(); a.playOnAwake = false; a.loop = loop;
            a.spatialBlend = 1; a.dopplerLevel = 0; a.minDistance = 1.4f; a.maxDistance = 16;
            a.rolloffMode = AudioRolloffMode.Logarithmic; a.volume = 0;
            KeeperAudioMix.Route(a, "SFX_Critical"); return a;
        }
        void LateUpdate()
        {
            var s = watch.State; var d = s.Drowned;
            if (d == null) { events.Stop(); breathing.Stop(); return; }
            if (revision != watch.Revision || s.Elapsed < lastElapsed - .01) { revision = watch.Revision; events.Stop(); breathing.Stop(); loopGain = 0; CueCount = 0; cursor.Clear(); }
            lastElapsed = s.Elapsed;
            bool allowed = DrownedAppearance.Sightings && appearance.SafeAnchor && !watch.Sleeping && !s.Failed && !s.Recovery.LessonActive;
            if (!d.Anchored && DrownedAppearance.Sightings) { events.Stop(); breathing.Stop(); return; }
            double cueAt = d.Phase==DrownedPhase.Manifest ? d.RevealAt : d.Since;
            var cue = cursor.Poll(revision, d.Phase, cueAt, s.Elapsed, allowed);
            if (cue != DrownedSoundCue.None)
            {
                LastCue = cue; CueCount++;
                events.Stop(); events.clip = cue == DrownedSoundCue.Approach ? approach : cue == DrownedSoundCue.Retreat ? retreat : (d.Fault + s.Day) % 2 == 0 ? revealA : revealB;
                eventGain = cue == DrownedSoundCue.Reveal ? .5f : .45f;
                if (events.clip) { events.time = (float)System.Math.Min(s.Elapsed - cueAt, events.clip.length - .01f); events.Play(); }
            }
            if (!allowed) { events.Stop(); breathing.Stop(); loopGain = 0; return; }
            var mix = KeeperAudioMix.Instance;
            float target = SpeechGain(mix && mix.VoiceActive, mix && mix.RadioActive);
            speechGain = Mathf.MoveTowards(speechGain, target, Time.unscaledDeltaTime / (target < speechGain ? .05f : .6f));
            events.volume = eventGain * speechGain;
            bool present = d.Phase == DrownedPhase.Manifest;
            if (present && !breathing.isPlaying && breathing.clip)
            { breathing.time = (float)(System.Math.Max(0, s.Elapsed - d.Since) % breathing.clip.length); breathing.Play(); }
            loopGain = Mathf.MoveTowards(loopGain, present ? .36f : 0, Time.unscaledDeltaTime * .3f);
            breathing.volume = loopGain * speechGain;
            if (!present && loopGain <= 0) breathing.Stop();
        }
    }
}
