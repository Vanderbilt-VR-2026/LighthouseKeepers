using LighthouseKeepers.Core.Progression;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Flood;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchDemoController : MonoBehaviour, LighthouseKeepers.Lobby.ICoopWatchView
    {
        [SerializeField] FloodController flood;
        [SerializeField] Transform head,repairPoint,bed,wrenchVisual;
        [SerializeField] XRGrabInteractable wrench;
        [SerializeField] ParticleSystem leak;
        [SerializeField] TextMesh board,clock,bedPrompt,repairPrompt;
        [SerializeField] Renderer fade;
        [SerializeField] AudioSource whoosh,leakAudio;
        [SerializeField] WatchGenerator generator;
        [SerializeField] EmergencyLighting emergencyLighting;
        [SerializeField] WatchRadioConsole radio;
        [SerializeField] WatchPipeRepair pipeRepair;
        [SerializeField] WatchBeacon beacon;
        [SerializeField, Min(0)] float poweredLeakRiseMPerSecond=.0015f;
        [SerializeField, Min(0)] float unpoweredLeakRiseMPerSecond=.0045f;
        [SerializeField, Min(0)] float repairedDrainMPerSecond=.012f;
        WatchState state;
        LighthouseKeepers.Lobby.ICoopWatchLink coop;
        public float WaterHeight=>flood.Height;
        public Transform BoardTransform=>board.transform;
        readonly System.Text.StringBuilder boardText = new System.Text.StringBuilder(256);
        float repairProgress;
        int shownSecond=-1;
        bool sleeping,previousGrip,previousTrigger;
        Quaternion originalWrenchRotation;
        Material fadeMaterial;
        WrenchHaptics wrenchHaptics;
        WatchMenu menu;
        public Component Owner=>this;
        public void SetNetworkMode(bool practice,bool replica){state.SetNetworkRehearsal(practice);state.SetRemoteReplica(replica);}
        public string CaptureState()
        {
            var value=state.Capture(flood.Height);value.revision=Revision;value.sleeping=sleeping;value.fade=currentFade;
            value.beds=crewBeds;value.keepers=crewKeepers;value.notice=crewNotice;return JsonUtility.ToJson(value);
        }
        public bool ApplyState(string json)=>ApplyHostSnapshot(JsonUtility.FromJson<WatchSnapshot>(json));
        public string DescribeState()=>$"day={state.Day} elapsed={state.Elapsed:F2} water={flood.Height:F4} power={state.Power} tasks={state.CompletedTasks}";
        public WatchState State=>state;
        public float RepairProgress=>state==null?0:state.Pipe.BoltsDone+state.Pipe.StrokeDegrees/state.Pipe.Profile.quarterTurnDegrees;
        void Start()
        {
            state = new WatchState(Resources.Load<DayCatalog>("WatchDays").days, LighthouseKeepers.StormWatch.WatchDayLaunch.Consume());
            coop=LighthouseKeepers.Lobby.CoopWatchBridge.Attach?.Invoke(this);state.Coop=coop;
            state.Changed += StateChanged;
            if (generator) generator.Bind(state);
            if(emergencyLighting)emergencyLighting.Bind(state);
            if(radio)radio.Bind(state);
            if(pipeRepair)pipeRepair.Bind(state);
            if(beacon)beacon.Bind(state);
            wrenchHaptics=wrench.GetComponent<WrenchHaptics>();originalWrenchRotation=wrenchVisual.localRotation;fadeMaterial=fade.material;fade.enabled=false;
            LighthouseKeepers.StormWatch.SoloWatchSession.ConsumeRequest();
            menu=gameObject.AddComponent<WatchMenu>();menu.Initialize(this,head);
            BeginWatch();
            if(state.NetworkRehearsal){flood.Paused=true;clock.text="NETWORK PRACTICE · OBJECTIVES PAUSED";}
            if(state.RemoteReplica){flood.Paused=true;clock.text="WAITING FOR HOST STATE";}
        }
        void BeginWatch()
        {
            if(coop!=null&&!state.RemoteReplica)Revision++;
            flood.ResetFlood();flood.SetHeight(.02f);flood.RiseSpeed=state.Power==PowerState.On?poweredLeakRiseMPerSecond:unpoweredLeakRiseMPerSecond;flood.SurgePerMetre=.1f;flood.ResumeFlood();
            if(generator)generator.ResetInteraction();
            CancelRepair();
            leak.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);if(leakAudio)leakAudio.Stop();
            if(pipeRepair)pipeRepair.ResetView();
            whoosh.Play();repairProgress=0;shownSecond=-1;RefreshBoard();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"WATCH_DAY day={state.Day} power={state.Power} tasks={state.Definition.tasks.Length} duration={state.Duration}");
#endif
        }
        void Update()
        {
            if(state.RemoteReplica){UpdateReplicaInput();return;}
            if(sleeping)return;
            if(state.NetworkRehearsal){flood.Paused=true;return;}
            if(coop!=null && !coop.Ready){flood.Paused=true;clock.text="WAITING FOR KEEPER";return;}
            var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            left.TryGetFeatureValue(CommonUsages.gripButton,out bool lg);right.TryGetFeatureValue(CommonUsages.gripButton,out bool rg);
            left.TryGetFeatureValue(CommonUsages.triggerButton,out bool lt);right.TryGetFeatureValue(CommonUsages.triggerButton,out bool rt);
            bool grip=lg||rg,trigger=lt||rt;
            state.Tick(Time.deltaTime);
            int second=(int)state.Elapsed;if(second!=shownSecond){shownSecond=second;int remaining=Mathf.Max(0,(int)state.Duration-second);clock.text=state.Failed?state.FailureMessage+"\nTRIGGER TO RETRY":$"D{state.Day}   {remaining/60:00}:{remaining%60:00}   {state.CompletedTasks}/{state.Definition.tasks.Length} TASKS";RefreshBoard();}
            bool nearBed=Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)<1.8f;
            bedPrompt.text=nearBed?(state.Complete?(state.HasNextDay?"PRESS GRIP TO SLEEP":"ALL WATCHES COMPLETE"):"Finish the board tasks before resting"):"";
            if(nearBed){var direction=bedPrompt.transform.position-head.position;direction.y=0;if(direction.sqrMagnitude>.01f)bedPrompt.transform.rotation=Quaternion.LookRotation(direction);}
            if(state.Failed)
            {
                flood.Paused=true;
                if(trigger&&!previousTrigger&&(!menu||!menu.IsOpen))RetryDay();
            }
            else if(state.Complete&&nearBed&&grip&&!previousGrip&&(!menu||!menu.IsOpen))TrySleep();
            // Original Day 1 net rise/drain remains unchanged. Outage disables pumping.
            if(!state.Failed)
            {
                flood.Paused=state.Repaired;
                flood.RiseSpeed=(state.Power==PowerState.On?poweredLeakRiseMPerSecond:unpoweredLeakRiseMPerSecond)*state.Pipe.InflowFraction;
                if(state.Repaired && state.Power==PowerState.On)
                    flood.SetHeight(Mathf.MoveTowards(flood.Height,flood.MinimumHeight,Time.deltaTime*repairedDrainMPerSecond));
            }
            previousGrip=grip;previousTrigger=trigger;
        }
        // Kept for historical tooling: seating never starts an automatic repair anymore.
        public bool TryBeginRepair()=>!state.Failed&&!state.Repaired&&!sleeping&&pipeRepair&&pipeRepair.InContact;
        public bool RetryDay(){if(state.RemoteReplica&&coop!=null)return state.Failed&&!sleeping&&coop.Send(LighthouseKeepers.Lobby.CoopAction.Retry);if(state.ReadOnly||!state.Failed||sleeping)return false;state.Retry();BeginWatch();if(menu)menu.Close();return true;}
        public bool ReplayFromDayOne()
        {
            if(state.RemoteReplica&&coop!=null)return !sleeping&&coop.Send(LighthouseKeepers.Lobby.CoopAction.Replay);
            if(state.ReadOnly||sleeping||!state.ReplayFromDayOne())return false;
            BeginWatch();if(menu)menu.Close();return true;
        }
        public bool TrySleep()
        {
            if(coop!=null)return CanSleep&&(!menu||!menu.IsOpen)&&Vector3.Distance(head.position,BedPosition)<1.8f&&coop.Send(LighthouseKeepers.Lobby.CoopAction.Bed);
            if(state.ReadOnly||sleeping||(menu&&menu.IsOpen)||!state.Complete||!state.HasNextDay||Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)>=1.8f)return false;
            StartCoroutine(Sleep());return true;
        }
        public bool ApplyHostSnapshot(WatchSnapshot snapshot)
        {
            if(snapshot==null||snapshot.revision<Revision||!state.ApplyRemote(snapshot))return false;
            if(snapshot.revision!=Revision)
            {Revision=snapshot.revision;if(generator)generator.ResetInteraction();if(pipeRepair)pipeRepair.ResetView();if(radio)radio.ResetReplicaInteraction();whoosh.Play();if(menu)menu.Close();}
            sleeping=snapshot.sleeping;replicaFadeTarget=snapshot.fade;
            crewBeds=snapshot.beds;crewKeepers=snapshot.keepers;crewNotice=snapshot.notice??"";
            flood.Paused=true;flood.SetHeight(snapshot.water);
            int remaining=Mathf.Max(0,(int)(state.Duration-state.Elapsed));
            clock.text=state.Failed?state.FailureMessage+"\nTRIGGER TO RETRY":$"D{state.Day}   {remaining/60:00}:{remaining%60:00}   {state.CompletedTasks}/{state.Definition.tasks.Length} TASKS";
            bedPrompt.text="";RefreshBoard();return true;
        }
        void CancelRepair(){repairProgress=0;}
        void StateChanged(){shownSecond=-1;RefreshBoard();}
        void RefreshBoard()
        {
            boardText.Clear();
            if(state.NetworkRehearsal)
            {board.text="NETWORK PRACTICE\n\nMove, turn and grab props together.\nObjectives and day clock are paused.\n\nLEFT MENU: Return to lobby";return;}
            boardText.Append("KEEPER WATCH — DAY ").Append(state.Day).Append("\n\n");
            foreach(var task in state.Definition.tasks)
                boardText.Append(state.IsDone(task.stationId)?"[X] ":"[ ] ").Append(task.taskText).Append('\n');
            if(!state.Repaired)boardText.Append("Pipe: ").Append(state.Pipe.BoltsDone).Append("/3 bolts\n");
            if(state.Power==PowerState.Off&&state.Definition.beaconDarkLimitSeconds>0&&!state.Failed)boardText.Append("BEACON DARK: ").Append(Mathf.CeilToInt((float)state.BeaconDarkRemaining)).Append(" s TO LOSS\n");
            boardText.Append('\n');
            if(state.Failed)boardText.Append(state.FailureMessage).Append("\nTrigger to retry this day / Menu for retry or lobby");
            else if(state.Complete)boardText.Append(state.HasNextDay?"GO TO BED — THIRD FLOOR":"ALL WATCHES COMPLETE\nMenu: Replay from Day 1 or Return to lobby");
            else boardText.Append("PIPE F1 / GENERATOR F2 / RADIO F4\nFinish tasks, then sleep in F3");
            if(!state.Failed&&(!state.Complete||state.HasNextDay))boardText.Append("\nLEFT MENU: Return to lobby (Editor: Esc)");
            if(crewBeds>0&&!sleeping)boardText.Append("\nWaiting for other keeper (").Append(crewBeds).Append('/').Append(crewKeepers).Append(" in bed)");
            if(!string.IsNullOrEmpty(crewNotice))boardText.Append('\n').Append(crewNotice);
            board.text=boardText.ToString();
        }
        IEnumerator Sleep()
        {
            sleeping=true;bedPrompt.text="";
            for(float t=0;t<1;t+=Time.deltaTime){SetFade(t);yield return null;}SetFade(1);
            yield return new WaitForSeconds(.2f);state.Sleep();BeginWatch();
            for(float t=0;t<.8f;t+=Time.deltaTime){SetFade(1-t/.8f);yield return null;}SetFade(0);sleeping=false;
        }
        void SetFade(float value){currentFade=value;fade.enabled=value>0;fadeMaterial.color=new Color(0,0,0,value);}
        void OnDestroy(){if(state!=null)state.Changed-=StateChanged;if(fadeMaterial)Destroy(fadeMaterial);}
    }
}
