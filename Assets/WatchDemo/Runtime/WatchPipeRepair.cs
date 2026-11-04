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
        float queuedTwistDegrees;
        LighthouseKeepers.Interaction.INetworkPropStatus network;
        Quaternion previousRotation;Vector3 previousJaw;bool previousContact,failureShown;int lastBolt=-1;float nextText;
        public bool InContact{get;private set;}
        public Vector3 SocketPosition(int index)
        {
            if(watch==null||!watch.Pipe.Forgiving)return bolts[index].position;
            var p=watch.Pipe;float fraction=p.ActiveBolt==index?(p.TurnsDone+p.StrokeDegrees/p.Profile.quarterTurnDegrees)/p.Profile.quarterTurns[index]:0;
            return bolts[index].TransformPoint(new Vector3(0,0,.033f-.022f*fraction));
        }
        public Vector3 ActivePosition=>SocketPosition(watch!=null&&watch.Pipe.ActiveBolt>=0?watch.Pipe.ActiveBolt:0);
        void Awake(){haptics=wrench.GetComponent<WrenchHaptics>();active=Block(new Color(1,.58f,.12f),1);done=Block(new Color(.1f,.65f,.2f),.3f);idle=Block(new Color(.2f,.25f,.27f),0);}
        static MaterialPropertyBlock Block(Color c,float glow){var p=new MaterialPropertyBlock();p.SetColor("_BaseColor",c);p.SetColor("_EmissionColor",c*glow);return p;}
        public void Bind(WatchState state)
        {watch=state;watch.Pipe.QuarterTurn+=TickFeedback;watch.Pipe.Completed+=CompleteFeedback;watch.Pipe.Changed+=Changed;
            if(state.Immersion&&state.Immersion.forgivingPipe&&!wrench.GetComponent<ImmersionWrenchSocket>())
            {
                // XRI adds its ordinary follow transformer lazily only when the list is empty.
                // Preserve free carrying first, then constrain its computed pose near a bolt.
                if(wrench.singleGrabTransformersCount==0)
                {
                    var follow=wrench.GetComponent<UnityEngine.XR.Interaction.Toolkit.Transformers.XRGeneralGrabTransformer>();
                    if(!follow)follow=wrench.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Transformers.XRGeneralGrabTransformer>();
                    wrench.AddSingleGrabTransformer(follow);
                }
                wrench.gameObject.AddComponent<ImmersionWrenchSocket>().Initialize(this,state);
            }
            ResetView();}
        public void ResetView(){for(int i=0;i<3;i++)finished[i]=false;if(watch!=null&&watch.RemoteReplica)for(int i=0;i<watch.Pipe.BoltsDone;i++)finished[watch.Pipe.Order(i)]=true;previousContact=false;failureShown=false;lastBolt=-1;Changed();}
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
            bool remoteHeld=watch.Coop!=null&&watch.Coop.RemoteHeld(wrench);
            InContact=index>=0&&(wrench.isSelected||remoteHeld)&&Vector3.Distance(jaw.position,SocketPosition(index))<=state.Profile.jawContactRadius+(state.Forgiving?.025f:0);
            if(remoteHeld&&network==null)network=wrench.GetComponent<LighthouseKeepers.Interaction.INetworkPropStatus>();
            float sampleSeconds=remoteHeld&&network!=null?network.PoseInterval:Time.deltaTime;
            float degrees=0,speed=0;
            if(InContact&&previousContact)
            {
                var delta=wrench.transform.rotation*Quaternion.Inverse(previousRotation);
                if(state.Forgiving)degrees=SignedTwist(delta,bolts[index].forward);
                else{delta.ToAngleAxis(out float angle,out var axis);if(angle>180)angle-=360;degrees=angle*Vector3.Dot(axis,bolts[index].forward);}speed=Vector3.Distance(jaw.position,previousJaw)/Mathf.Max(.001f,sampleSeconds);
                if(!float.IsFinite(degrees))degrees=0;
            }
            previousContact=InContact;previousRotation=wrench.transform.rotation;previousJaw=jaw.position;
            bool steady=Mathf.Abs(degrees)<=state.Profile.steadyDegreesPerSecond*sampleSeconds&&speed<=state.Profile.steadyMetresPerSecond;
            // Network and kinematic-tool poses arrive slower than Update. Spread their
            // motion across frames so the speed limit preserves legitimate wrist travel.
            if((remoteHeld||state.Forgiving)&&InContact)
            {queuedTwistDegrees+=degrees;degrees=Mathf.Clamp(queuedTwistDegrees,-state.Profile.maximumDegreesPerSecond*Time.deltaTime,state.Profile.maximumDegreesPerSecond*Time.deltaTime);queuedTwistDegrees-=degrees;}
            else queuedTwistDegrees=0;
            state.Observe(Time.deltaTime,degrees,InContact,steady,!watch.Failed);
            if(watch.Failed){if(!failureShown){Changed();prompt.text="WATCH LOST — PRESS TRIGGER TO RETRY";failureShown=true;}return;}
            if(watch.Immersion&&watch.Immersion.physicalGuidance)return;
            if(Time.unscaledTime<nextText)return;nextText=Time.unscaledTime+.1f;
            if(state.Complete){prompt.text="PIPE SEALED  3/3";return;}
            index=state.ActiveBolt;prompt.transform.position=bolts[index].position+Vector3.up*.38f;
            var facing=prompt.transform.position-head.position;if(facing.sqrMagnitude>.01f)prompt.transform.rotation=Quaternion.LookRotation(facing);
            if(Vector3.Distance(head.position,bolts[index].position)>3){prompt.text="";return;}
            string name=((char)('A'+index)).ToString();
            string instruction=!InContact?"SEAT WRENCH JAW ON GLOWING BOLT":state.NeedsSeal?$"HOLD STEADY  {Mathf.CeilToInt(state.Profile.sealSeconds-state.SealElapsed)}s":state.NeedsReturn?"RETURN WRIST / REGRIP":$"TURN WRIST  {state.TurnsDone}/{state.Profile.quarterTurns[index]} CLICKS";
            prompt.text=$"COUPLING {name}  /  PIPE {state.BoltsDone}/3\n{instruction}";
        }
        // atan2 preserves sub-degree motion at high Editor frame rates; acos(w) in
        // ToAngleAxis rounds tiny rotations to zero and made slow turns inconsistent.
        internal static float SignedTwist(Quaternion delta,Vector3 axis)
        {
            if(delta.w<0)delta=new Quaternion(-delta.x,-delta.y,-delta.z,-delta.w);
            return 2*Mathf.Atan2(Vector3.Dot(new Vector3(delta.x,delta.y,delta.z),axis),delta.w)*Mathf.Rad2Deg;
        }
        void OnDestroy(){if(watch!=null){watch.Pipe.QuarterTurn-=TickFeedback;watch.Pipe.Completed-=CompleteFeedback;watch.Pipe.Changed-=Changed;}}
    }
}
