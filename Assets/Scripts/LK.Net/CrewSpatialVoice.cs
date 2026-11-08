using UnityEngine;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.Net
{
    // Playback-only routing. Photon still captures one mono stream with its accepted AEC/NS chain.
    public sealed class CrewSpatialVoice : MonoBehaviour
    {
        public int Actor;
        public bool RadioMode {get;private set;}
        public bool Occluded {get;private set;}
        public float TargetGain {get;private set;}
        public float Cutoff=>low?low.cutoffFrequency:20000;
        AudioSource source,effects;AudioLowPassFilter low;AudioHighPassFilter high;CrewRadioColour colour;
        AudioClip squelch;ConnectedTasksTuning settings;Transform head;WatchCoopSession session;float nextScan,nextOcclusion;bool previousRadio;
        readonly RaycastHit[] hits=new RaycastHit[24];float baseVolume;
        void Awake()
        {
            settings=ConnectedTasksTuning.Current;source=GetComponent<AudioSource>();baseVolume=source.volume;
            low=gameObject.AddComponent<AudioLowPassFilter>();high=gameObject.AddComponent<AudioHighPassFilter>();high.enabled=false;
            colour=gameObject.AddComponent<CrewRadioColour>();colour.enabled=false;
            source.rolloffMode=AudioRolloffMode.Custom;source.SetCustomCurve(AudioSourceCurveType.CustomRolloff,AnimationCurve.Linear(0,1,1,1));source.maxDistance=100;
            var g=new GameObject("Handheld radio squelch");g.transform.SetParent(transform,false);effects=g.AddComponent<AudioSource>();effects.playOnAwake=false;effects.spatialBlend=1;effects.minDistance=.4f;effects.maxDistance=4;effects.dopplerLevel=0;KeeperAudioMix.Route(effects,"SFX_World");
            squelch=BuildSquelch();
        }
        public static float ProximityGain(float distance,float heightDifference,bool occluded,ConnectedTasksTuning t)
        {
            float distanceGain=Mathf.Lerp(1,.08f,Mathf.InverseLerp(t.voiceNearMetres,t.voiceFarMetres,distance));
            float floorGain=heightDifference>=t.floorHeight*1.7f?t.distantFloorGain:heightDifference>=t.floorHeight*.65f?.4f:1;
            return distanceGain*floorGain*(occluded?t.wallVoiceGain:1);
        }
        void LateUpdate()
        {
            if(!settings)return;
            if(Time.unscaledTime>=nextScan){nextScan=Time.unscaledTime+.5f;if(!head&&Camera.main)head=Camera.main.transform;if(!session)session=FindAnyObjectByType<WatchCoopSession>();}
            var poses=CrewPoseStream.Active;if(!head||!poses||!poses.SampleVisual(Actor,out var pose)){source.volume=0;return;}
            var view=session?session.ConnectedView:null;var receiver=view?.LocalRadio;
            RadioMode=settings.handheldRadios&&receiver&&view.IsRadioTransmission(Actor);
            Vector3 point=RadioMode?receiver.TransformPoint(new Vector3(0,.04f,.035f)):pose.Head;
            transform.position=point;
            if(Time.unscaledTime>=nextOcclusion){nextOcclusion=Time.unscaledTime+.15f;Occluded=false;var delta=pose.Head-head.position;int count=Physics.RaycastNonAlloc(head.position,delta.normalized,hits,delta.magnitude,~0,QueryTriggerInteraction.Ignore);for(int i=0;i<count;i++){var c=hits[i].collider;if(c.attachedRigidbody||c.GetComponentInParent<XROrigin>()||hits[i].distance<.12f||hits[i].distance>delta.magnitude-.15f)continue;Occluded=true;break;}}
            TargetGain=RadioMode?.9f:settings.proximityVoice?ProximityGain(Vector3.Distance(head.position,pose.Head),Mathf.Abs(head.position.y-pose.Head.y),Occluded,settings):1;
            source.volume=Mathf.Lerp(source.volume,baseVolume*TargetGain,1-Mathf.Exp(-10*Time.unscaledDeltaTime));
            float cutoff=RadioMode?settings.radioHighHz:settings.proximityVoice&&(Occluded||Mathf.Abs(head.position.y-pose.Head.y)>settings.floorHeight*.65f)?settings.wallCutoffHz:20000;
            low.cutoffFrequency=Mathf.Lerp(low.cutoffFrequency,cutoff,1-Mathf.Exp(-10*Time.unscaledDeltaTime));high.enabled=RadioMode;high.cutoffFrequency=settings.radioLowHz;colour.Radio=RadioMode;
            colour.enabled=true;
            if(RadioMode!=previousRadio){effects.PlayOneShot(squelch,RadioMode?.14f:.10f);previousRadio=RadioMode;}
        }
        static AudioClip BuildSquelch()
        {
            const int rate=22050;var a=new float[(int)(rate*.28f)];var random=new System.Random(941);float low=0;
            for(int i=0;i<a.Length;i++){float n=(float)random.NextDouble()*2-1;low+=.28f*(n-low);float envelope=Mathf.Sin(Mathf.PI*i/a.Length);a[i]=low*envelope*.5f;}
            var c=AudioClip.Create("Original handheld squelch",a.Length,1,rate,false);c.SetData(a,0);return c;
        }
        void OnDestroy(){if(squelch)Destroy(squelch);}
    }
    // A restrained received-radio crunch/static bed; no dropout that could erase a word.
    public sealed class CrewRadioColour:MonoBehaviour
    {
        public volatile bool Radio;uint random=92347;float blend;
        void OnAudioFilterRead(float[] data,int channels)
        {
            float target=Radio?1:0;
            for(int i=0;i<data.Length;i++){blend+=(target-blend)*.0004f;random=1664525*random+1013904223;float noise=((random>>8)/16777216f*2-1)*.002f;float x=data[i];float radio=(float)System.Math.Tanh(x*1.15)/1.15f+noise;data[i]=x+(radio-x)*blend;}
        }
    }
}
