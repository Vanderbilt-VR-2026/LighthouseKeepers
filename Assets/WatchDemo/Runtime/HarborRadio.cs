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
        WatchDemoController watch;ImmersionTuning tuning;RadioLineCatalog catalog;
        AudioSource wall,belt,wallNoise,beltNoise;AudioClip noise,click;
        Transform head,wrench;WatchPipeRepair pipe;readonly Queue<int> pending=new();
        int day,progress;bool firstBolt,complete,staticActive;float idle,earlyNext;
        public static HarborRadio Create(WatchDemoController watch,Transform head,Transform wrench,WatchPipeRepair pipe,Transform cabinet)
        {
            var r=cabinet.gameObject.AddComponent<HarborRadio>();r.watch=watch;r.tuning=watch.State.Immersion;r.head=head;r.wrench=wrench;r.pipe=pipe;
            r.catalog=Resources.Load<RadioLineCatalog>("HarborRadioLines");
            r.noise=MakeNoise();r.click=Resources.Load<AudioClip>("Hands/clunk");
            r.wall=r.Voice(cabinet,"Harbor master wall voice",r.tuning.wallRadioVolume,1,10);
            var receiver=new GameObject("Keeper belt radio receiver").transform;receiver.SetParent(watch.transform,false);r.belt=r.Voice(receiver,"Harbor master belt voice",r.tuning.beltRadioVolume,.25f,2);
            var parts=watch.GetComponent<M2StationParts>();
            parts.Part("Belt radio case",receiver,Vector3.zero,new Vector3(.075f,.13f,.035f),parts.Dark);
            parts.Part("Belt radio aerial",receiver,new Vector3(-.025f,.10f,0),new Vector3(.008f,.075f,.008f),parts.Metal);
            for(int i=0;i<4;i++)parts.Part("Belt radio speaker slit",receiver,new Vector3(0,-.025f+i*.012f,.019f),new Vector3(.05f,.004f,.004f),parts.Brass);
            r.wallNoise=r.Static(cabinet);r.beltNoise=r.Static(receiver);
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
            belt.volume=tuning.beltRadioVolume*Mathf.InverseLerp(2,5,Vector3.Distance(head.position,wall.transform.position));
            beltNoise.volume=.007f*Mathf.InverseLerp(2,5,Vector3.Distance(head.position,wall.transform.position));
            if(s.RemoteReplica)return;
            Age+=Time.deltaTime;
            if(day!=s.Day)
            {
                day=s.Day;pending.Clear();firstBolt=complete=false;idle=0;progress=Progress();
                Enqueue(day==1?1:day==2?13:day==3?14:15);
                if(day==1)Enqueue(2);
                if(day==2)Enqueue(4);
            }
            if(s.Failed){pending.Clear();wall.Stop();belt.Stop();StopStatic();return;}
            int current=Progress();if(current!=progress){progress=current;idle=0;}else idle+=Time.deltaTime;
            bool holding=watch.WrenchHeld;
            if(day==1&&!firstBolt&&holding&&Vector3.Distance(wrench.position,pipe.ActivePosition)<1.3f){firstBolt=true;Enqueue(3);}
            if(s.Complete&&!complete){complete=true;Enqueue(5);}
            if(idle>=tuning.idleHintSeconds)
            {
                idle=0;
                int hint=s.Complete?12:s.Engine!=null&&!s.Engine.Running?8:!s.BeaconShining?s.HasPower(PowerCircuit.Beacon)?10:9:!s.Repaired?holding?7:6:11;
                Enqueue(hint);
            }
            var clip=catalog?catalog.Line(Cue):null;
            if(pending.Count>0&&(Cue==0||!clip||Age>clip.length+.6f))Play(pending.Dequeue(),Serial+1,0);
            if(clip&&Age>clip.length+.2f)StopStatic();
        }
        int Progress()
        {var s=watch.State;return s.Pipe.BoltsDone*1000+s.Pipe.TurnsDone*100+s.CompletedTasks*10000+(s.Engine==null?0:s.Engine.Primer*3+s.Engine.Pulls*10)+(s.Radio.Locked?1:0)+s.Radio.CodeProgress;}
        void Enqueue(int id){if(pending.Count<4&&!pending.Contains(id))pending.Enqueue(id);}
        public void ResetNight(){if(watch.State.ReadOnly)return;day=0;pending.Clear();Cue=0;Age=0;wall.Stop();belt.Stop();StopStatic();}
        public void EarlyBed()
        {if(watch.State.ReadOnly||Time.unscaledTime<earlyNext)return;earlyNext=Time.unscaledTime+8;Enqueue(16);}
        public void Capture(WatchSnapshot s){s.radioCue=Cue;s.radioSerial=Serial;s.radioAge=Age;}
        public void Apply(WatchSnapshot s)
        {
            if(s.radioSerial>Serial)Play(s.radioCue,s.radioSerial,s.radioAge);
            else Age=s.radioAge;
            var clip=catalog?catalog.Line(Cue):null;if(!clip||Age>clip.length+.2f)StopStatic();
        }
        void Play(int id,int serial,float age)
        {
            Cue=id;Serial=serial;Age=age;
            var clip=catalog?catalog.Line(id):null;
            if(!clip||age>=clip.length)return;
            foreach(var source in new[]{wall,belt}){source.Stop();source.clip=clip;source.time=Mathf.Min(age,clip.length-.01f);source.Play();}
            staticActive=true;wallNoise.Play();beltNoise.Play();if(age<.1f&&click)wallNoise.PlayOneShot(click,.5f);
        }
        void StopStatic(){if(!staticActive)return;staticActive=false;wallNoise.Stop();beltNoise.Stop();if(click)wallNoise.PlayOneShot(click,.25f);}
        void OnDestroy(){if(noise)Destroy(noise);if(belt)Destroy(belt.transform.parent.gameObject);}
    }
}
