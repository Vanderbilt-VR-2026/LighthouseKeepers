using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class HarborRadio
    {
        readonly StorySpeechQueue storySpeech=new();
        readonly Dictionary<string,float> storyPlayedAt=new();
        bool storyPipeRepaired,storyEngineRunning;
        float engineRestoredAt=float.NegativeInfinity;
        void ResetStorySpeech()
        {
            storySpeech.BeginNight();storyPlayedAt.Clear();
            storyPipeRepaired=watch.State.Repaired;storyEngineRunning=watch.State.Engine?.Settled==true;
            engineRestoredAt=float.NegativeInfinity;
        }
        void OfferStory(string id,bool trigger,float delay=0)
        {
            var line=StoryVoiceCatalog.Get(id);
            // Availability and trigger validity are independent of the temporary caption-only mode.
            if(!trigger||line==null||!HarborEditorialPolicy.Allows(line.cue)||!catalog.HasLine(line.cue))return;
            storySpeech.Enqueue(line,Time.time,delay:delay,lifetime:120);
        }
        bool StoryRelevant(StorySpeechRequest request)
        {
            var s=watch.State;
            if(s.Failed||watch.Sleeping||storyPlayedAt.ContainsKey(request.Line.id))return false;
            switch(request.Line.id)
            {
                case "HM_N2_LANTERN_INTRO":return s.Lanterns?.FocusOnly==false&&!s.Engine.Running&&!watch.AnyLanternHeld;
                case "HM_N2_LANTERN_OIL":return s.Lanterns?.UnlimitedOil==false&&watch.AnyLanternHeld;
                case "HM_LANTERN_LOW_OIL":return s.Lanterns?.CanOil>0&&watch.HeldLanternLowOil;
                case "HM_PIPE_FIXED":return s.Repaired;
                case "HM_GEN_RUNNING":return s.Engine?.Settled==true;
                case "HM_N1_RULE":case "HM_N1_RULE_REASON":return s.Day==1&&s.HumanKeeperCount==2;
                case "HM_N2_WALTER":return s.Day==2&&s.Engine?.Settled==true;
                case "HM_N4_TOM":return s.Day==4;
                case "HM_N5_TRUTH_1":return s.Day==5&&s.Ships?.PassedMask>0;
                case "HM_N5_TRUTH_2":return s.Day==5&&(storyPlayedAt.ContainsKey("HM_N5_TRUTH_1")||!StoryVoiceAvailability.Enabled("HM_N5_TRUTH_1")&&s.Ships?.PassedMask>0);
                case "HM_N5_TRUTH_3":return s.Day==5&&storyPlayedAt.ContainsKey("HM_N5_TRUTH_2");
                case "HM_N5_TRUTH_4":return s.Day==5&&storyPlayedAt.ContainsKey("HM_N5_TRUTH_3");
                default:return false;
            }
        }
        void ObserveStorySpeech()
        {
            var s=watch.State;bool running=s.Engine?.Settled==true;
            OfferStory("HM_N2_LANTERN_INTRO",s.Day==2&&s.Lanterns?.FocusOnly==false&&!s.Engine.Running&&!watch.AnyLanternHeld);
            OfferStory("HM_N2_LANTERN_OIL",s.Lanterns?.UnlimitedOil==false&&watch.AnyLanternHeld);
            OfferStory("HM_LANTERN_LOW_OIL",s.Lanterns?.CanOil>0&&watch.HeldLanternLowOil);
            OfferStory("HM_PIPE_FIXED",!storyPipeRepaired&&s.Repaired);
            OfferStory("HM_GEN_RUNNING",!storyEngineRunning&&running);
            if(!storyEngineRunning&&running)engineRestoredAt=Time.time;
            storyPipeRepaired=s.Repaired;storyEngineRunning=running;
            OfferStory("HM_N1_RULE",s.Day==1&&s.HumanKeeperCount==2&&s.Elapsed>=3);
            OfferStory("HM_N1_RULE_REASON",s.Day==1&&s.HumanKeeperCount==2&&s.CompletedTasks==s.Definition.tasks.Length&&StoryDelay("HM_N1_RULE",20));
            OfferStory("HM_N2_WALTER",s.Day==2&&Time.time-engineRestoredAt>=30&&running&&float.IsFinite(engineRestoredAt));
            OfferStory("HM_N4_TOM",s.Day==4&&s.Elapsed>=60);
            OfferStory("HM_N5_TRUTH_1",s.Day==5&&s.Ships?.PassedMask>0);
            OfferStory("HM_N5_TRUTH_2",s.Day==5&&(StoryDelay("HM_N5_TRUTH_1",10)||!StoryVoiceAvailability.Enabled("HM_N5_TRUTH_1")&&s.Ships?.PassedMask>0));
            OfferStory("HM_N5_TRUTH_3",s.Day==5&&StoryDelay("HM_N5_TRUTH_2",30));
            OfferStory("HM_N5_TRUTH_4",s.Day==5&&StoryDelay("HM_N5_TRUTH_3",8));
            // Exactly one PA program owns playback. Existing urgent/mechanical requests win this frame.
            // Until all mechanics are migrated, legacy queue conditions still validate legacy cues.
            if(pending.Count>0||SilenceSeconds<1||ordinaryBurstCount>=2&&SilenceSeconds<8)return;
            bool calm=s.Drowned?.Phase!=DrownedPhase.Warning&&s.Drowned?.Phase!=DrownedPhase.Manifest&&s.Ships?.AnyWindow(s.Elapsed)!=true;
            // Opening and later faults both earn breathing room; never use only the night clock.
            calm&=s.CompletedTasks==s.Definition.tasks.Length||s.Elapsed>=10;
            if(s.Sabotage!=null)for(int i=0;i<s.Sabotage.Count;i++)
                if(s.Sabotage.Broken(i)>=0&&s.Sabotage.FixedAt(i)<0&&s.Elapsed-s.Sabotage.Broken(i)<10)calm=false;
            var mix=LighthouseKeepers.Core.KeeperAudioMix.Instance;
            var context=new StorySpeechContext(s.Day,s.HumanKeeperCount,calm,mix&&mix.VoiceActive);
            var next=storySpeech.Poll(Time.time,context,StoryRelevant,l=>PresentationSeconds(l.cue)+Extra);
            if(next==null)return;
            Play(next.Line.cue,Serial+1,0);
        }
        void NoteStoryPlayback(int cue)
        {
            var line=StoryVoiceCatalog.Get(cue);
            if(line!=null&&catalog.HasLine(cue))storyPlayedAt[line.id]=Time.time+PresentationSeconds(cue)+Extra;
        }
        bool StoryDelay(string id,float delay)=>storyPlayedAt.TryGetValue(id,out var at)&&Time.time-at>=delay;
    }
}
