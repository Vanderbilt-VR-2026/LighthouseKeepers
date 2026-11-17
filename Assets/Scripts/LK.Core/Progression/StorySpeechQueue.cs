using System;
using System.Collections.Generic;
namespace LighthouseKeepers.Core.Progression
{
    public readonly struct StorySpeechContext
    {
        public readonly int Night, HumanKeepers;
        public readonly bool Calm, VoiceChat, Alone, LowLight, FullScares;
        public StorySpeechContext(int night,int humans,bool calm,bool chat=false,bool alone=false,bool lowLight=false,bool fullScares=false)
        { Night=night; HumanKeepers=humans; Calm=calm; VoiceChat=chat; Alone=alone; LowLight=lowLight; FullScares=fullScares; }
    }
    public sealed class StorySpeechRequest
    {
        public readonly StoryVoiceLine Line;
        // Distinguishes ship/event instances. A repeated observer cannot duplicate one event.
        public readonly int Occurrence;
        public readonly double NotBefore, Expires;
        internal StorySpeechRequest(StoryVoiceLine line,int occurrence,double notBefore,double expires)
        { Line=line; Occurrence=occurrence; NotBefore=notBefore; Expires=expires; }
    }
    // Host-side policy only. No AudioSource, clock advancement, player movement or hidden scene searches.
    // Callers supply actual trigger validity and clip duration immediately before playback.
    public sealed class StorySpeechQueue
    {
        readonly List<StorySpeechRequest> pending = new();
        readonly HashSet<string> run = new(StringComparer.Ordinal);
        readonly HashSet<(string,int)> occurrences = new();
        readonly Dictionary<string,int> nightCounts = new(StringComparer.Ordinal);
        readonly Dictionary<string,double> last = new(StringComparer.Ordinal);
        double lastEnd=double.NegativeInfinity, harborEnd=double.NegativeInfinity, antStarted=double.NegativeInfinity;
        int p2Burst, antCount;
        StorySpeechRequest active;
        public int Count => pending.Count;
        public int AntagonistCount => antCount;
        public StorySpeechRequest Active => active;
        public double ActiveUntil => lastEnd;
        public void BeginNight(bool newRun=false)
        {
            pending.Clear(); occurrences.Clear(); nightCounts.Clear(); last.Clear(); active=null;
            p2Burst=antCount=0; lastEnd=harborEnd=antStarted=double.NegativeInfinity;
            if(newRun)run.Clear();
        }
        public bool Played(string id) => run.Contains(id) || nightCounts.ContainsKey(id);
        public bool Enqueue(StoryVoiceLine line,double now,int occurrence=0,double delay=0,double lifetime=30)
        {
            if(line==null || !double.IsFinite(now) || !double.IsFinite(delay) || !double.IsFinite(lifetime) || delay<0 || lifetime<=0 || pending.Count>=24 || !Allowed(line,occurrence,now))return false;
            foreach(var p in pending)if(p.Line.id==line.id&&p.Occurrence==occurrence)return false;
            // Priority cancellation applies to waiting speech, not the active utterance.
            pending.RemoveAll(p=>p.Line.priority>line.priority);
            pending.Add(new StorySpeechRequest(line,occurrence,now+delay,now+delay+lifetime));return true;
        }
        bool Allowed(StoryVoiceLine line,int occurrence,double now)
        {
            if(line.OncePerRun && run.Contains(line.id))return false;
            if(line.PerShip && occurrences.Contains((line.id,occurrence)))return false;
            if(nightCounts.TryGetValue(line.id,out int n)&&n>=line.NightLimit)return false;
            return !last.TryGetValue(line.id,out double at)||now-at>=line.Cooldown;
        }
        public static bool HelpReady(double secondsIdleNearStation,int failedAttempts,bool actionUnderway) =>
            !actionUnderway && (secondsIdleNearStation>=20||failedAttempts>=2);
        public StorySpeechRequest Poll(double now,StorySpeechContext context,Func<StorySpeechRequest,bool> stillRelevant,Func<StoryVoiceLine,double> duration)
        {
            if(!double.IsFinite(now))return null;
            // Stale conditions are checked even while another utterance is playing.
            for(int i=pending.Count-1;i>=0;i--)
            {var p=pending[i];if(now>p.Expires||!Allowed(p.Line,p.Occurrence,now)||!stillRelevant(p))pending.RemoveAt(i);}
            if(context.VoiceChat)return null; // Do not begin/interrupt over a teammate's sentence.
            if(now-lastEnd>=8)p2Burst=0;
            StorySpeechRequest next=null;
            foreach(var p in pending)
            {
                var l=p.Line;if(now<p.NotBefore)continue;
                if(l.mode=="C"&&context.HumanKeepers<2||l.mode=="S"&&context.HumanKeepers!=1)continue;
                if(l.priority==3&&!l.Antagonist&&!context.Calm)continue;
                if(now<lastEnd && (l.priority!=1||active==null||active.Line.priority==1))continue;
                if(l.priority!=1 && (now<lastEnd+1 || p2Burst>=2&&now<lastEnd+8))continue;
                if(l.Antagonist)
                {
                    if(context.Night<=1||!context.FullScares||!context.Alone||!context.LowLight||antCount>=6||now-antStarted<90||now-harborEnd<4)continue;
                    bool hmQueued=false;foreach(var other in pending)if(!other.Line.Antagonist){hmQueued=true;break;}
                    if(hmQueued)continue;
                }
                if(next==null||l.priority<next.Line.priority)next=p;
            }
            if(next==null)return null;
            double seconds=duration(next.Line);
            // Missing audio must not claim to have spoken, or hold the queue forever.
            if(!double.IsFinite(seconds)||seconds<=0){pending.Remove(next);return null;}
            if(active!=null&&!active.Line.Antagonist&&now<lastEnd)harborEnd=now;
            pending.Remove(next); active=next; lastEnd=now+seconds;
            var e=next.Line;last[e.id]=now;nightCounts.TryGetValue(e.id,out int count);nightCounts[e.id]=count+1;
            if(e.OncePerRun)run.Add(e.id);if(e.PerShip)occurrences.Add((e.id,next.Occurrence));
            if(e.Antagonist){antStarted=now;antCount++;}else harborEnd=lastEnd;
            if(e.priority==2)p2Burst++;
            return next;
        }
    }
}
