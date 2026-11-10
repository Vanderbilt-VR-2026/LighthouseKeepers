using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    // The host chooses cues; guests only play the cue/age in its authenticated watch snapshot.
    public sealed partial class HarborRadio : MonoBehaviour
    {
        public int Cue {get;private set;}
        public int Serial {get;private set;}
        public float Age {get;private set;}
        PAIntercom intercom;
        readonly HarborRecallHistory recallHistory=new();
        int localReplayCue;float localReplayAge,nextLocalReplay;
        public System.Collections.Generic.IReadOnlyList<int> RecentCues=>recallHistory.Cues;
        public string LastCaption=>catalog&&recallHistory.Latest>0?catalog.Caption(recallHistory.Latest):"";
        public int SubtitleCue=>localReplayCue>0?localReplayCue:Cue;
        public float SpeechAge=>(localReplayCue>0?localReplayAge:Age)-(intercom ? .3f : 0);
        public bool ReplayLastLocally()
        {
            int id=recallHistory.Latest;var clip=catalog?catalog.Line(id):null;
            if(!clip||Time.unscaledTime<nextLocalReplay)return false;
            nextLocalReplay=Time.unscaledTime+3;localReplayCue=id;localReplayAge=0;
            if(intercom)intercom.Play(id,clip,0,Serial);
            else{foreach(var source in new[]{wall,belt}){source.Stop();source.clip=clip;source.time=0;source.Play();}wallNoise.Play();beltNoise.Play();staticActive=true;}
            return true;
        }
        public float SubtitleAlpha=>catalog&&catalog.Line(SubtitleCue)&&SpeechAge>=0?Mathf.Clamp01((catalog.Line(SubtitleCue).length+1.85f-SpeechAge)/.35f):0;
        public string Subtitle=>SubtitleAlpha>0?catalog.Caption(SubtitleCue):"";
        public int SubtitleCharacters=>HarborVoiceTiming.Get(SubtitleCue)?.VisibleCharacters(SpeechAge)??Mathf.CeilToInt(Subtitle.Length*Mathf.Clamp01(SpeechAge/Mathf.Max(.1f,catalog&&catalog.Line(SubtitleCue)?catalog.Line(SubtitleCue).length:1)));
        public bool DangerSubtitle=>HarborCueRules.Danger(SubtitleCue);
        public Component ReplayButton(int i)=>intercom?intercom.Button(i):null;
        public bool RequestReplay(){if(watch.State.RemoteReplica)return watch.State.Coop?.Send(LighthouseKeepers.Lobby.CoopAction.StatusRequest)==true;return intercom&&intercom.Replay();}
        public void ReplayLast(){if(Cue>0)Play(Cue,Serial+1,0);}
        public float SilenceSeconds=>Time.time-quietSince;
        float Extra=>intercom ? .7f : 0;
        WatchDemoController watch;ImmersionTuning tuning;RadioLineCatalog catalog;
        AudioSource wall,belt,wallNoise,beltNoise;AudioClip noise,click;
        Transform head,wrench;WatchPipeRepair pipe;readonly List<int> pending=new();readonly Dictionary<int,float> queuedAt=new();
        bool gripTaught,upperJobsCued;string announcedJob;
        int dangerMask,priorTasks;float nextStatus;
        int ordinaryBurstCount;int day,progress,lastJob;bool firstBolt,complete,staticActive,failed;float idle,quietSince;
        readonly HashSet<int> spoken=new();
        Transform generator,radio,beacon,panel;
        LighthouseKeepers.Interaction.INetworkKeeperProximity keepers;
        public static HarborRadio Create(WatchDemoController watch,Transform head,Transform wrench,WatchPipeRepair pipe,Transform cabinet)
        {
            var r=cabinet.gameObject.AddComponent<HarborRadio>();r.watch=watch;r.tuning=watch.State.Immersion;r.head=head;r.wrench=wrench;r.pipe=pipe;
            r.generator=Object.FindAnyObjectByType<WatchGenerator>().transform;
            r.radio=Object.FindAnyObjectByType<WatchRadioConsole>().transform;
            var beaconStation=Object.FindAnyObjectByType<WatchBeaconStation>();r.beacon=beaconStation?beaconStation.transform:Object.FindAnyObjectByType<WatchBeacon>().transform;
            var powerPanel=Object.FindAnyObjectByType<WatchPowerPanel>();r.panel=powerPanel?powerPanel.transform:r.generator;
            r.keepers=watch.GetComponent<LighthouseKeepers.Interaction.INetworkKeeperProximity>();
            r.catalog=Resources.Load<RadioLineCatalog>("HarborRadioLines");
            r.noise=MakeNoise();r.click=Resources.Load<AudioClip>("Hands/clunk");
            r.wall=r.Voice(cabinet,"Harbor master wall voice",r.tuning.wallRadioVolume,1,10);
            var receiver=new GameObject("Keeper belt radio receiver").transform;receiver.SetParent(watch.transform,false);r.belt=r.Voice(receiver,"Harbor master belt voice",r.tuning.beltRadioVolume,.25f,2);
            var parts=watch.GetComponent<M2StationParts>();
            parts.Part("Belt radio case",receiver,Vector3.zero,new Vector3(.075f,.13f,.035f),parts.Dark);
            parts.Part("Belt radio aerial",receiver,new Vector3(-.025f,.10f,0),new Vector3(.008f,.075f,.008f),parts.Metal);
            for(int i=0;i<4;i++)parts.Part("Belt radio speaker slit",receiver,new Vector3(0,-.025f+i*.012f,.019f),new Vector3(.05f,.004f,.004f),parts.Brass);
            r.wallNoise=r.Static(cabinet);r.beltNoise=r.Static(receiver);
            if(AudioPassSettings.Current&&AudioPassSettings.Current.paSpeakers){r.intercom=PAIntercom.Create(watch,head,r,r.catalog,r.noise,r.click);r.belt.transform.parent.gameObject.SetActive(false);r.wall.volume=0;}
            r.InitializeSituationalLines();return r;
        }
        AudioSource Voice(Transform parent,string name,float volume,float near,float far)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=1;s.dopplerLevel=0;s.volume=volume;s.minDistance=near;s.maxDistance=far;s.rolloffMode=AudioRolloffMode.Linear;
            go.AddComponent<AudioHighPassFilter>().cutoffFrequency=250;go.AddComponent<AudioLowPassFilter>().cutoffFrequency=4500;go.AddComponent<AudioDistortionFilter>().distortionLevel=.025f;return s;
        }
        AudioSource Static(Transform parent)
        {
            var s=Voice(parent,LighthouseKeepers.Core.PlayerStrings.Get("legacy.HarborRadio.1"),.006f,.4f,7);s.clip=noise;s.loop=true;return s;
        }
        static AudioClip MakeNoise()
        {
            const int n=24000;var data=new float[n];var random=new System.Random(3091);
            for(int i=0;i<n;i++){float crackle=i%6100<75?.8f:.12f;data[i]=(float)(random.NextDouble()*2-1)*crackle;}
            var clip=AudioClip.Create(LighthouseKeepers.Core.PlayerStrings.Get("legacy.HarborRadio.2"),n,1,24000,false);clip.SetData(data,0);return clip;
        }
        void Update()
        {
            var s=watch.State;
            if(localReplayCue>0){localReplayAge+=Time.deltaTime;var localClip=catalog.Line(localReplayCue);if(!localClip||localReplayAge>localClip.length+2.2f){localReplayCue=0;StopStatic();}}
            var yaw=Quaternion.Euler(0,head.eulerAngles.y,0);belt.transform.parent.SetPositionAndRotation(head.position+Vector3.down*.75f+yaw*new Vector3(.18f,0,.02f),yaw);
            // The quieter receiver carries the same voice upstairs without doubling it beside the wall set.
            belt.volume=intercom?0:tuning.beltRadioVolume*Mathf.InverseLerp(2,5,Vector3.Distance(head.position,wall.transform.position));
            beltNoise.volume=.003f*Mathf.InverseLerp(2,5,Vector3.Distance(head.position,wall.transform.position));
            if(s.RemoteReplica)return;
            Age+=Time.deltaTime;
            if(day!=s.Day)
            {
                day=s.Day;dangerMask=0;priorTasks=s.CompletedTasks;nextStatus=Time.time+120;pending.Clear();queuedAt.Clear();spoken.Clear();firstBolt=complete=failed=upperJobsCued=false;idle=0;quietSince=Time.time;progress=Progress();lastJob=CurrentJob();
                ResetSituationalLines();ordinaryBurstCount=0;announcedJob=null;
                int tonight=s.Campaign?s.Scenario.tonightCue:TonightCue(s);int opening=s.Campaign?s.Scenario.openingCue:day==2&&tonight==80?80:OpeningCue(day,s.Day3Balance);
                Enqueue(opening);if(s.WatchStartHeld)Enqueue(79);
                if(day>1&&tonight>0&&tonight!=opening&&!(day==3&&s.Day3Balance))Enqueue(tonight);
                if(s.Master&&s.Master.onboarding){/* Arrival covers stakes; board and current objective carry the loop. No queued lecture. */}
                else {if(day==1)Enqueue(2);if(day==2)Enqueue(4);}
            }
            if(s.Failed)
            {
                if(!failed){failed=true;pending.Clear();spoken.Add(18);Play(18,Serial+1,0);}
                var loss=catalog?catalog.Line(18):null;if(!loss||Age>loss.length+Extra+.2f)StopStatic();return;
            }
            PruneInstructions();ObserveSituationalLines();
            if(s.PressureEnabled)PressureCues();
            if(s.Director!=null&&s.Master.onboarding){
                var jobName=ObjectiveDirector.Job(s.Director.First);
                if(announcedJob!=jobName){if(announcedJob!=null&&!s.Complete){if(announcedJob==WatchState.PipeStation&&s.Repaired&&watch.WrenchHeld)Enqueue(78);else Enqueue(jobName==WatchState.GeneratorStation?38:jobName==WatchState.RadioStation?39:jobName==WatchState.BeaconStation?40:jobName==LighthouseKeepers.Core.PlayerStrings.Get("legacy.HarborRadio.3")?41:37);}announcedJob=jobName;}
                if(day==1&&!watch.WrenchHeld&&!s.Repaired&&!gripTaught&&idle>=10&&Vector3.Distance(head.position,wrench.position)<2){gripTaught=true;Enqueue(33);}
            }
            int current=Progress(),job=CurrentJob();if(current!=progress||job!=lastJob){progress=current;lastJob=job;idle=0;}else idle+=Time.deltaTime;
            bool holding=watch.WrenchHeld;
            if(!(s.Master&&s.Master.onboarding)&&day==1&&!firstBolt&&holding&&Vector3.Distance(wrench.position,pipe.ActivePosition)<1.3f){firstBolt=true;Enqueue(3);}
            if(!upperJobsCued&&s.Day3Balance&&s.Day3Stage==3){upperJobsCued=true;Enqueue(UpperJobsCue);}
            if(s.Complete&&!complete){complete=true;pending.RemoveAll(id=>id!=92&&id!=78);if(s.PeakDanger>.8f)Enqueue(93);
                if(!s.HasNextDay)Enqueue(17);else if(watch.WrenchHeld)Enqueue(78);else if(!broadcastReplyPending)Enqueue(5);}
            if(idle>=Mathf.Max(90,tuning.idleHintSeconds)&&Time.time-quietSince>=15&&!WrenchSuppressesHints(holding,s.Repaired)&&!AtStation(job))
                Enqueue(job);
            var clip=catalog?catalog.Line(Cue):null;
            if(pending.Count>0&&CanPlayQueued(pending[NextPendingIndex()]))
                while(pending.Count>0){int next=TakeNext();if(Relevant(next)&&(next<76||cueRules.Allows(next,Time.time))){if(next>=44&&next<=75)next=CompositeStatusCue(s);spoken.Add(next);Play(next,Serial+1,0);break;}}
            if(clip&&Age>clip.length+Extra+.2f)StopStatic();
        }
        bool Relevant(int id)
        {
            var s=watch.State;
            if(id==33)return !watch.WrenchHeld&&!s.Repaired;
            if(id==78)return s.Repaired&&watch.WrenchHeld;
            if(id==79)return s.WatchStartHeld;
            if(id==82)return s.Pressure?.CrewRequired==true&&!s.Repaired&&AtStation(7);
            if(id==83)return s.Pressure?.CrewRequired==true&&!s.Repaired&&s.Pressure.Holder==0;
            if(id==84)return s.Pressure!=null&&!s.Pressure.CrewRequired&&!s.Pressure.Clamped&&!s.Repaired&&watch.WrenchHeld&&AtStation(7);
            if(id==85)return KeepersSeparated();
            if(id==86)return UsefulPoleNearby();
            if(id==87)return s.Grid?.Tripped==true;
            if(id==88)return s.Engine?.LowFuel==true&&!s.IsPending(WatchState.GeneratorStation);
            if(id==89)return s.Engine!=null&&!s.Engine.Running;
            if(id==90)return s.Director!=null&&(s.Director.First==KeeperStep.BeaconWipe||s.Director.Second==KeeperStep.BeaconWipe);
            if(id==91)return !s.Radio.CodeAccepted&&!s.Radio.Completed;
            if(id>=37&&id<=42){var job=watch.State.Director?.First??KeeperStep.None;return id==(ObjectiveDirector.Job(job)==WatchState.PipeStation?37:ObjectiveDirector.Job(job)==WatchState.GeneratorStation?38:ObjectiveDirector.Job(job)==WatchState.RadioStation?39:ObjectiveDirector.Job(job)==WatchState.BeaconStation?40:ObjectiveDirector.Job(job)==LighthouseKeepers.Core.PlayerStrings.Get("legacy.HarborRadio.3")?41:42);}
            if(id>=7&&id<=12&&(WrenchSuppressesHints(watch.WrenchHeld,s.Repaired)||AtStation(id)||idle<90||id!=CurrentJob()))return false;
            if(id==UpperJobsCue)return !s.Complete&&!s.IsPending(WatchState.BeaconStation)&&!s.IsPending(WatchState.RadioStation);
            if(id==2)return !watch.WrenchHeld&&!s.Repaired;
            if(id==3||id==7)return !s.Repaired;
            if(id==4||id==8)return s.Power==PowerState.Off;
            if(id==5||id==12||id==17)return s.Complete;
            if(id==9)return !s.HasPower(PowerCircuit.Beacon);
            if(id==10)return !s.BeaconShining;
            if(id==11)return !s.IsDone(WatchState.RadioStation);
            if(id==16)return !s.Complete;
            return true;
        }
        public const int UpperJobsCue=77;
        public static int OpeningCue(int day,bool pipeStagedFirst)=>day==1?1:day==2?13:day==3?(pipeStagedFirst?76:UpperJobsCue):15;
        public static bool WrenchSuppressesHints(bool holding,bool repaired)=>holding&&!repaired;
        int CurrentJob()=>JobCue(watch.State);
        public static int JobCue(WatchState s)
        {
            if(s.Director!=null)return ObjectiveDirector.Hint(s.Director.First);
            return s.Complete?12:s.Engine!=null&&!s.Engine.Running?8:
                !s.BeaconShining?s.HasPower(PowerCircuit.Beacon)?10:9:!s.Repaired?7:11;
        }
        bool AtStation(int job)
        {
            Vector3 position=job==7?pipe.ActivePosition:job==8?generator.position:
                job==9?panel.position:job==10?beacon.position:job==11?radio.position:watch.BedPosition;
            return NearStationPoint(position)||job==7&&pressureValve&&NearStationPoint(pressureValve.position);
        }
        bool NearStationPoint(Vector3 position)
        {
            bool remote=keepers?.NearKeeper(position,1.8f)==true;
            var delta=head.position-position;
            return remote||Mathf.Abs(delta.y)<2.5f&&new Vector2(delta.x,delta.z).sqrMagnitude<1.8f*1.8f;
        }
        int Progress()
        {
            var s=watch.State;
            switch(CurrentJob())
            {
                case 7:return s.Pipe.BoltsDone*100000+s.Pipe.TurnsDone*1000+(int)(s.Pipe.StrokeDegrees*2);
                case 8:return s.Engine==null?0:s.Engine.Primer*1000+s.Engine.Pulls*100+(int)(s.Engine.Fuel*100)+(s.Engine.ChokeClosed?10000:0);
                case 9:return s.Grid==null?0:s.Grid.SelectedMask+(s.Grid.Tripped?16:0);
                case 10:return s.Beacon==null?0:(int)(s.Beacon.Alignment*1000)+(s.Beacon.Ignited?10000:0);
                case 11:return s.Radio.Step*10000+s.Radio.CodeProgress*1000+(int)(s.Radio.FrequencyMHz*100)+(int)(s.Radio.TransmitElapsed*10);
                default:return 0;
            }
        }
        public bool StatusUpdate() {
            if(watch.State.ReadOnly)return false;
            var s=watch.State;
            nextStatus=Time.time+s.Master.statusIntervalSeconds;
            int id=CompositeStatusCue(s);
            if(SilenceSeconds>=s.Master.speechSpacingSeconds){Play(id,Serial+1,0);return true;}if(pending.Count<16&&!pending.Contains(id))AddPending(id);return true;
        }
        public static int CompositeStatusCue(WatchState s) {
            int time=s.Tutorial?0:s.Duration-s.Elapsed>180?0:s.Duration-s.Elapsed>120?1:s.Duration-s.Elapsed>60?2:3;
            int water=s.WaterDanger>=.9f?3:s.WaterDanger>=.75f?2:s.WaterDanger>=.5f?1:0;
            return 44+time*8+water*2+(s.BeaconShining?0:1);
        }
        void PressureCues() {
            var s=watch.State;
            int id=0,bit=0;
            if(s.WaterDanger>=.9f&&(dangerMask&4)==0){id=29;bit=4;}
            else if(!s.BeaconShining&&s.BeaconGraceRemaining<=0&&s.BeaconDarkRemaining<=10&&(dangerMask&32)==0){id=32;bit=32;}
            else if(s.WaterDanger>=.75f&&(dangerMask&2)==0){id=28;bit=2;}
            else if(s.WaterDanger>=.5f&&(dangerMask&1)==0){id=27;bit=1;}
            else if(!s.BeaconShining&&s.BeaconGraceRemaining<=0&&s.BeaconDarkRemaining<=30&&(dangerMask&16)==0){id=31;bit=16;}
            else if(!s.BeaconShining&&(dangerMask&8)==0){id=30;bit=8;}
            bool critical=id==29||id==32;
            if(id>0&&(critical?(Cue!=29&&Cue!=32||SilenceSeconds>=0):SilenceSeconds>=s.Master.speechSpacingSeconds)){dangerMask|=bit;Play(id,Serial+1,0);}
            if(Time.time>=nextStatus&&SilenceSeconds>=s.Master.speechSpacingSeconds)StatusUpdate();
            if(s.CompletedTasks>priorTasks&&!(s.Master&&s.Master.onboarding)&&!(s.Radio.Completed&&lastJob==11)){Enqueue(s.Power==PowerState.On&&lastJob==8?21:lastJob==10?22:20);}priorTasks=s.CompletedTasks;
        }
        void Enqueue(int id){if(id>=76){QueueSituational(id);return;}if(id!=6&&!spoken.Contains(id)&&pending.Count<16&&!pending.Contains(id))AddPending(id);}
        void AddPending(int id){pending.Add(id);queuedAt[id]=Time.time;}
        void PruneInstructions(){float lifetime=watch.State.Master?watch.State.Master.instructionLifetimeSeconds:20;for(int i=pending.Count-1;i>=0;i--){int id=pending[i];if(!Relevant(id)||(HarborCueRules.InstructionExpires(id)&&queuedAt.TryGetValue(id,out float at)&&Time.time-at>lifetime)){pending.RemoveAt(i);queuedAt.Remove(id);}}}
        bool CanPlayQueued(int id)
        {
            if(Cue==0)return true;
            var clip=catalog?catalog.Line(Cue):null;float silence=Age-(clip?clip.length:0)-Extra;
            float recovery=watch.State.Master?watch.State.Master.narrationQuietSeconds:20f;
            return HarborCueRules.CanSpeak(ordinaryBurstCount,silence,HarborCueRules.Transition(id),recovery,watch.State.Master?watch.State.Master.speechSpacingSeconds:8f);
        }
        int NextPendingIndex(){int index=0; // Keep arrival first, then grace; situational guidance precedes background tutorial prose.
            if(Cue!=0&&pending[0]!=79){int priority=pending.FindIndex(id=>id>=76&&id<=93);if(priority>=0)index=priority;}
            return index;}
        int TakeNext(){int index=NextPendingIndex();int id=pending[index];pending.RemoveAt(index);return id;}
        public void ResetNight(){if(watch.State.ReadOnly)return;day=0;ordinaryBurstCount=0;pending.Clear();queuedAt.Clear();spoken.Clear();Cue=0;Age=0;wall.Stop();belt.Stop();if(intercom)intercom.Stop();StopStatic();}
        public void EarlyBed(){if(!watch.State.ReadOnly)Enqueue(16);}
        public void Capture(WatchSnapshot s){s.radioCue=Cue;s.radioSerial=Serial;s.radioAge=Age;}
        public void Apply(WatchSnapshot s)
        {
            if(s.radioSerial>Serial)Play(s.radioCue,s.radioSerial,s.radioAge);
            else Age=s.radioAge;
            var clip=catalog?catalog.Line(Cue):null;if(!clip||Age>clip.length+Extra+.2f)StopStatic();
        }
        void Play(int id,int serial,float age)
        {
            localReplayCue=0;recallHistory.Remember(id);
            if(!watch.State.RemoteReplica){
                cueRules.Played(id,Time.time);
                if(SilenceSeconds>=(watch.State.Master?watch.State.Master.narrationQuietSeconds:20f))ordinaryBurstCount=0;
                if(!HarborCueRules.Danger(id))ordinaryBurstCount++;
            }
            Debug.Log($"[HarborVoice] {(watch.State.RemoteReplica?"guest":"host")} night={watch.State.Day} time={Time.time:F2} cue={id} serial={serial} age={age:F2} previous={Cue} remaining={Mathf.Max(0,(catalog&&catalog.Line(Cue)?catalog.Line(Cue).length:0)+Extra-Age):F2}");
            Cue=id;Serial=serial;Age=age;quietSince=Time.time+(catalog&&catalog.Line(id)?catalog.Line(id).length+Extra:0);
            var clip=catalog?catalog.Line(id):null;
            if(!clip||age>=clip.length+Extra)return;
            if(intercom){intercom.Play(id,clip,age,Serial);return;}
            foreach(var source in new[]{wall,belt}){source.Stop();source.clip=clip;source.time=Mathf.Min(age,clip.length-.01f);source.Play();}
            staticActive=true;wallNoise.Play();beltNoise.Play();if(age<.1f&&click)wallNoise.PlayOneShot(click,.5f);
        }
        void StopStatic(){if(!staticActive)return;staticActive=false;wallNoise.Stop();beltNoise.Stop();if(click)wallNoise.PlayOneShot(click,.25f);}
        void OnDestroy(){if(watch!=null&&watch.State!=null){watch.State.Radio.CodeRejected-=WrongCode;watch.State.Radio.BroadcastCompleted-=BroadcastReply;}if(noise)Destroy(noise);if(belt)Destroy(belt.transform.parent.gameObject);}
    }
}
