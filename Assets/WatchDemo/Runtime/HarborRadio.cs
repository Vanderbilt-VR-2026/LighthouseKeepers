using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    // The host chooses cues; guests only play the cue/age in its authenticated watch snapshot.
    public sealed class HarborRadio : MonoBehaviour
    {
        public int Cue {get;private set;}
        public int Serial {get;private set;}
        public float Age {get;private set;}
        PAIntercom intercom;
        public string Subtitle=>catalog&&Age>=(intercom ? .3f : 0)&&catalog.Line(Cue)&&Age<catalog.Line(Cue).length+(intercom ? .3f : 0)?catalog.Caption(Cue):"";
        public Component ReplayButton(int i)=>intercom?intercom.Button(i):null;
        public bool RequestReplay()=>intercom&&intercom.Replay();
        public void ReplayLast(){if(Cue>0)Play(Cue,Serial+1,0);}
        float Extra=>intercom ? .7f : 0;
        WatchDemoController watch;ImmersionTuning tuning;RadioLineCatalog catalog;
        AudioSource wall,belt,wallNoise,beltNoise;AudioClip noise,click;
        Transform head,wrench;WatchPipeRepair pipe;readonly Queue<int> pending=new();
        int day,progress,lastJob;bool firstBolt,complete,staticActive,failed;float idle,quietSince;
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
            return r;
        }
        AudioSource Voice(Transform parent,string name,float volume,float near,float far)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=1;s.dopplerLevel=0;s.volume=volume;s.minDistance=near;s.maxDistance=far;s.rolloffMode=AudioRolloffMode.Linear;
            go.AddComponent<AudioHighPassFilter>().cutoffFrequency=300;go.AddComponent<AudioLowPassFilter>().cutoffFrequency=3400;go.AddComponent<AudioDistortionFilter>().distortionLevel=.08f;return s;
        }
        AudioSource Static(Transform parent)
        {
            var s=Voice(parent,"Radio static and squelch",.015f,.4f,7);s.clip=noise;s.loop=true;return s;
        }
        static AudioClip MakeNoise()
        {
            const int n=24000;var data=new float[n];var random=new System.Random(3091);
            for(int i=0;i<n;i++){float crackle=i%6100<75?.8f:.12f;data[i]=(float)(random.NextDouble()*2-1)*crackle;}
            var clip=AudioClip.Create("Original radio static crackle",n,1,24000,false);clip.SetData(data,0);return clip;
        }
        void Update()
        {
            var s=watch.State;
            var yaw=Quaternion.Euler(0,head.eulerAngles.y,0);belt.transform.parent.SetPositionAndRotation(head.position+Vector3.down*.75f+yaw*new Vector3(.18f,0,.02f),yaw);
            // The quieter receiver carries the same voice upstairs without doubling it beside the wall set.
            belt.volume=intercom?0:tuning.beltRadioVolume*Mathf.InverseLerp(2,5,Vector3.Distance(head.position,wall.transform.position));
            beltNoise.volume=.007f*Mathf.InverseLerp(2,5,Vector3.Distance(head.position,wall.transform.position));
            if(s.RemoteReplica)return;
            Age+=Time.deltaTime;
            if(day!=s.Day)
            {
                day=s.Day;pending.Clear();spoken.Clear();firstBolt=complete=failed=false;idle=0;quietSince=Time.time;progress=Progress();lastJob=CurrentJob();
                Enqueue(day==1?1:day==2?13:day==3?14:15);
                if(day==1)Enqueue(2);
                if(day==2)Enqueue(4);
            }
            if(s.Failed)
            {
                if(!failed){failed=true;pending.Clear();spoken.Add(18);Play(18,Serial+1,0);}
                var loss=catalog?catalog.Line(18):null;if(!loss||Age>loss.length+Extra+.2f)StopStatic();return;
            }
            int current=Progress(),job=CurrentJob();if(current!=progress||job!=lastJob){progress=current;lastJob=job;idle=0;}else idle+=Time.deltaTime;
            bool holding=watch.WrenchHeld;
            if(day==1&&!firstBolt&&holding&&Vector3.Distance(wrench.position,pipe.ActivePosition)<1.3f){firstBolt=true;Enqueue(3);}
            if(s.Complete&&!complete){complete=true;Enqueue(s.HasNextDay?5:17);}
            if(idle>=Mathf.Max(90,tuning.idleHintSeconds)&&Time.time-quietSince>=15&&!holding&&!AtStation(job))
                Enqueue(job);
            var clip=catalog?catalog.Line(Cue):null;
            if(pending.Count>0&&(Cue==0||!clip||Age>clip.length+Extra+.6f))
                while(pending.Count>0){int next=pending.Dequeue();if(Relevant(next)){spoken.Add(next);Play(next,Serial+1,0);break;}}
            if(clip&&Age>clip.length+Extra+.2f)StopStatic();
        }
        bool Relevant(int id)
        {
            var s=watch.State;
            if(id>=7&&id<=12&&(watch.WrenchHeld||AtStation(id)||idle<90||id!=CurrentJob()))return false;
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
        int CurrentJob()
        {
            var s=watch.State;
            if(s.Director!=null)return ObjectiveDirector.Hint(s.Director.First);
            return s.Complete?12:s.Engine!=null&&!s.Engine.Running?8:
                !s.BeaconShining?s.HasPower(PowerCircuit.Beacon)?10:9:!s.Repaired?7:11;
        }
        bool AtStation(int job)
        {
            Vector3 position=job==7?pipe.ActivePosition:job==8?generator.position:
                job==9?panel.position:job==10?beacon.position:job==11?radio.position:watch.BedPosition;
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
        void Enqueue(int id){if(id!=6&&!spoken.Contains(id)&&pending.Count<4&&!pending.Contains(id))pending.Enqueue(id);}
        public void ResetNight(){if(watch.State.ReadOnly)return;day=0;pending.Clear();spoken.Clear();Cue=0;Age=0;wall.Stop();belt.Stop();if(intercom)intercom.Stop();StopStatic();}
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
            Cue=id;Serial=serial;Age=age;quietSince=Time.time+(catalog&&catalog.Line(id)?catalog.Line(id).length+Extra:0);
            var clip=catalog?catalog.Line(id):null;
            if(!clip||age>=clip.length+Extra)return;
            if(intercom){intercom.Play(id,clip,age,Serial);return;}
            foreach(var source in new[]{wall,belt}){source.Stop();source.clip=clip;source.time=Mathf.Min(age,clip.length-.01f);source.Play();}
            staticActive=true;wallNoise.Play();beltNoise.Play();if(age<.1f&&click)wallNoise.PlayOneShot(click,.5f);
        }
        void StopStatic(){if(!staticActive)return;staticActive=false;wallNoise.Stop();beltNoise.Stop();if(click)wallNoise.PlayOneShot(click,.25f);}
        void OnDestroy(){if(noise)Destroy(noise);if(belt)Destroy(belt.transform.parent.gameObject);}
    }
}
