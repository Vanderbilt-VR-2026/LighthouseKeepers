using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace LighthouseKeepers.WatchDemo
{
    [DefaultExecutionOrder(100)]
    public sealed class WatchPipeRepair:MonoBehaviour
    {
        public Transform[] bolts;
        public Renderer[] indicators;
        public ParticleSystem[] sprays;
        public XRGrabInteractable wrench;
        public Transform jaw,head;
        public TextMesh prompt;
        public AudioSource audioSource,leakAudio;
        public AudioClip ratchetClick;
        WatchState watch;WrenchHaptics haptics;
        readonly bool[] finished=new bool[3];
        MaterialPropertyBlock active,done,idle;
        Quaternion previousRotation;Vector3 previousJaw;bool previousContact,failureShown;int lastBolt=-1;float nextText;
        public bool InContact{get;private set;}
        public Vector3 ActivePosition=>watch!=null&&watch.Pipe.ActiveBolt>=0?bolts[watch.Pipe.ActiveBolt].position:bolts[0].position;
        void Awake(){haptics=wrench.GetComponent<WrenchHaptics>();active=Block(new Color(1,.58f,.12f),1);done=Block(new Color(.1f,.65f,.2f),.3f);idle=Block(new Color(.2f,.25f,.27f),0);}
        static MaterialPropertyBlock Block(Color c,float glow){var p=new MaterialPropertyBlock();p.SetColor("_BaseColor",c);p.SetColor("_EmissionColor",c*glow);return p;}
        public void Bind(WatchState state)
        {watch=state;watch.Pipe.QuarterTurn+=TickFeedback;watch.Pipe.Completed+=CompleteFeedback;watch.Pipe.Changed+=Changed;ResetView();}
        public void ResetView(){for(int i=0;i<3;i++)finished[i]=false;previousContact=false;failureShown=false;lastBolt=-1;Changed();}
        void TickFeedback(){haptics.RatchetTick(watch.Pipe.Profile.tickAmplitude,watch.Pipe.Profile.tickSeconds);audioSource.PlayOneShot(ratchetClick,.7f);}
        void CompleteFeedback(){haptics.RepairCompleted();}
        void Changed()
        {
            if(lastBolt>=0&&watch.Pipe.ActiveBolt!=lastBolt)finished[lastBolt]=true;
            lastBolt=watch.Pipe.ActiveBolt;nextText=0;
            for(int i=0;i<3;i++)
            {
                indicators[i].SetPropertyBlock(finished[i]?done:i==lastBolt?active:idle);
                var emission=sprays[i].emission;emission.rateOverTime=watch.Pipe.Profile.sprayParticlesPerSecond*watch.Pipe.InflowFraction;
                if(i==lastBolt&&!watch.Failed){if(!sprays[i].isPlaying)sprays[i].Play();}
                else sprays[i].Stop(true,ParticleSystemStopBehavior.StopEmitting);
            }
            leakAudio.volume=.25f*watch.Pipe.InflowFraction;
            if(watch.Pipe.Complete||watch.Failed)leakAudio.Stop();else if(!leakAudio.isPlaying)leakAudio.Play();
        }
        void Update()
        {
            if(watch==null)return;
            var state=watch.Pipe;int index=state.ActiveBolt;
            InContact=index>=0&&wrench.isSelected&&Vector3.Distance(jaw.position,bolts[index].position)<=state.Profile.jawContactRadius;
            float degrees=0,speed=0;
            if(InContact&&previousContact)
            {
                var delta=wrench.transform.rotation*Quaternion.Inverse(previousRotation);delta.ToAngleAxis(out float angle,out var axis);if(angle>180)angle-=360;
                degrees=angle*Vector3.Dot(axis,bolts[index].forward);speed=Vector3.Distance(jaw.position,previousJaw)/Mathf.Max(.001f,Time.deltaTime);
                if(!float.IsFinite(degrees))degrees=0;
            }
            previousContact=InContact;previousRotation=wrench.transform.rotation;previousJaw=jaw.position;
            state.Observe(Time.deltaTime,degrees,InContact,Mathf.Abs(degrees)<=state.Profile.steadyDegreesPerSecond*Time.deltaTime&&speed<=state.Profile.steadyMetresPerSecond,!watch.Failed);
            if(watch.Failed){if(!failureShown){Changed();prompt.text="WATCH LOST — PRESS TRIGGER TO RETRY";failureShown=true;}return;}
            if(Time.unscaledTime<nextText)return;nextText=Time.unscaledTime+.1f;
            if(state.Complete){prompt.text="PIPE SEALED  3/3";return;}
            index=state.ActiveBolt;prompt.transform.position=bolts[index].position+Vector3.up*.38f;
            var facing=prompt.transform.position-head.position;if(facing.sqrMagnitude>.01f)prompt.transform.rotation=Quaternion.LookRotation(facing);
            if(Vector3.Distance(head.position,bolts[index].position)>3){prompt.text="";return;}
            string name=((char)('A'+index)).ToString();
            string instruction=!InContact?"SEAT WRENCH JAW ON GLOWING BOLT":state.NeedsSeal?$"HOLD STEADY  {Mathf.CeilToInt(state.Profile.sealSeconds-state.SealElapsed)}s":state.NeedsReturn?"RETURN WRIST / REGRIP":$"TURN WRIST  {state.TurnsDone}/{state.Profile.quarterTurns[index]} CLICKS";
            prompt.text=$"COUPLING {name}  /  PIPE {state.BoltsDone}/3\n{instruction}";
        }
        void OnDestroy(){if(watch!=null){watch.Pipe.QuarterTurn-=TickFeedback;watch.Pipe.Completed-=CompleteFeedback;watch.Pipe.Changed-=Changed;}}
    }
}
