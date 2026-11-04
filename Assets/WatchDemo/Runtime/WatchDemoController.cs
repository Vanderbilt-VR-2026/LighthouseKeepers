using LighthouseKeepers.Core.Progression;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchDemoController : MonoBehaviour, LighthouseKeepers.Lobby.ICoopWatchView, LighthouseKeepers.Lobby.ICoopBedPresentation
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
        int shownSecond=-1,shownChalkLetters=-1;
        bool sleeping,previousGrip,previousTrigger;
        Quaternion originalWrenchRotation;
        Material fadeMaterial;
        WrenchHaptics wrenchHaptics;
        WatchMenu menu;
        S4WatchPresentation s4Presentation;
        WatchPowerPanel powerPanel;
        WatchEngineControls engineControls;
        WatchBeaconStation beaconStation;
        int sleepDay,bedMask;
        ImmersionWorld immersionWorld;
        ImmersionLogbook logbook;
        public bool WrenchHeld=>wrench.isSelected||coop?.RemoteHeld(wrench)==true;
        public void SetBedMask(int value){bedMask=value;if(immersionWorld)immersionWorld.SetBeds(value);}

        public Component Owner=>this;
        public void SetNetworkMode(bool practice,bool replica){state.SetNetworkRehearsal(practice);state.SetRemoteReplica(replica);}
        public string CaptureState()
        {
            var value=state.Capture(flood.Height);value.revision=Revision;value.sleeping=sleeping;value.fade=currentFade;
            value.beds=crewBeds;value.keepers=crewKeepers;value.notice=crewNotice;value.sleepDay=sleepDay;value.bedMask=bedMask;if(immersionWorld&&immersionWorld.Narrator)immersionWorld.Narrator.Capture(value);return JsonUtility.ToJson(value);
        }
        public bool ApplyState(string json)=>ApplyHostSnapshot(JsonUtility.FromJson<WatchSnapshot>(json));
        public string DescribeState()=>$"day={state.Day} elapsed={state.Elapsed:F2} water={flood.Height:F4} power={state.Power} tasks={state.CompletedTasks}";
        public WatchState State=>state;
        public float RepairProgress=>state==null?0:state.Pipe.BoltsDone+state.Pipe.StrokeDegrees/state.Pipe.Profile.quarterTurnDegrees;
        void Start()
        {
            var powerTuning=S4M2Tuning.Current;
            state = new WatchState(Resources.Load<DayCatalog>(powerTuning?S4M2Tuning.DayCatalogResource:"WatchDays").days, LighthouseKeepers.StormWatch.WatchDayLaunch.Consume(),S4Tuning.Current,powerTuning,ImmersionTuning.Current);
            if(state.Tuning&&state.Tuning.difficulty)
            {poweredLeakRiseMPerSecond=state.Tuning.poweredRiseMPerSecond;unpoweredLeakRiseMPerSecond=state.Tuning.unpoweredRiseMPerSecond;repairedDrainMPerSecond=state.Tuning.drainMPerSecond;}
            coop=LighthouseKeepers.Lobby.CoopWatchBridge.Attach?.Invoke(this);state.Coop=coop;
            state.Changed += StateChanged;
            if (generator) generator.Bind(state);
            if(emergencyLighting)emergencyLighting.Bind(state);
            if(radio)radio.Bind(state);
            if(pipeRepair)pipeRepair.Bind(state);
            if(beacon)beacon.Bind(state);
            if(state.Grid!=null)powerPanel=WatchPowerPanel.Create(this);
            if(state.Engine!=null)engineControls=WatchEngineControls.Create(this,generator);
            if(state.Beacon!=null)beaconStation=WatchBeaconStation.Create(this,beacon);
            wrenchHaptics=wrench.GetComponent<WrenchHaptics>();originalWrenchRotation=wrenchVisual.localRotation;fadeMaterial=fade.material;fade.enabled=false;
            LighthouseKeepers.StormWatch.SoloWatchSession.ConsumeRequest();
            menu=gameObject.AddComponent<WatchMenu>();menu.Initialize(this,head);
            if(state.Tuning)
            {s4Presentation=gameObject.AddComponent<S4WatchPresentation>();s4Presentation.Initialize(this,head,bed,generator,pipeRepair,radio,fade);}
            if(state.Immersion)
            {
                if(state.Immersion.physicalGuidance){clock.gameObject.SetActive(false);bedPrompt.gameObject.SetActive(false);repairPrompt.gameObject.SetActive(false);}
                immersionWorld=ImmersionWorld.Create(this,head,bed,board,wrench.transform,pipeRepair,generator,radio,beacon);
                if(state.Immersion.logbook)logbook=ImmersionLogbook.Create(head,transform);
            }
            BeginWatch();
            if(state.NetworkRehearsal){flood.Paused=true;clock.text="NETWORK PRACTICE · OBJECTIVES PAUSED";}
            if(state.RemoteReplica){flood.Paused=true;clock.text="WAITING FOR HOST STATE";}
        }
        void BeginWatch()
        {
            if(coop!=null&&!state.RemoteReplica)Revision++;
            flood.ResetFlood();flood.SetHeight(.02f);flood.RiseSpeed=state.HasPower(PowerCircuit.Pump)?poweredLeakRiseMPerSecond:unpoweredLeakRiseMPerSecond;flood.SurgePerMetre=.1f;flood.ResumeFlood();
            if(generator)generator.ResetInteraction();
            if(engineControls&&!state.RemoteReplica)engineControls.ResetProps();
            if(beaconStation&&!state.RemoteReplica)beaconStation.ResetProps();
            if(immersionWorld&&immersionWorld.Narrator&&!state.RemoteReplica)immersionWorld.Narrator.ResetNight();
            SetBedMask(0);
            if(!sleeping)SetFade(0);
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
            if(state.Immersion&&state.Immersion.physicalGuidance)
            {
                int letters=Mathf.Min(16,(int)(state.Elapsed/state.Immersion.chalkLetterSeconds));
                if(letters!=shownChalkLetters){shownChalkLetters=letters;RefreshBoard();}
            }
            if(state.RemoteReplica){UpdateReplicaInput();return;}
            if(sleeping)return;
            if(state.NetworkRehearsal){flood.Paused=true;return;}
            if(coop!=null && !coop.Ready){flood.Paused=true;clock.text="WAITING FOR KEEPER";return;}
            var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            left.TryGetFeatureValue(CommonUsages.gripButton,out bool lg);right.TryGetFeatureValue(CommonUsages.gripButton,out bool rg);
            left.TryGetFeatureValue(CommonUsages.triggerButton,out bool lt);right.TryGetFeatureValue(CommonUsages.triggerButton,out bool rt);
            bool grip=lg||rg,trigger=lt||rt;
            state.Tick(Time.deltaTime);
            state.ObserveWater(Time.deltaTime,flood.Height);
            int second=(int)state.Elapsed;if(second!=shownSecond){shownSecond=second;int remaining=Mathf.Max(0,(int)state.Duration-second);clock.text=state.Failed?state.FailureMessage+"\nTRIGGER TO RETRY":$"D{state.Day}   {remaining/60:00}:{remaining%60:00}   {state.CompletedTasks}/{state.Definition.tasks.Length} TASKS";RefreshBoard();}
            bool nearBed=Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)<1.8f;
            bedPrompt.text=nearBed?(state.Complete?(state.HasNextDay?SleepPrompt:"ALL WATCHES COMPLETE"):"Finish the board tasks before resting"):"";
            if(nearBed){var direction=bedPrompt.transform.position-head.position;direction.y=0;if(direction.sqrMagnitude>.01f)bedPrompt.transform.rotation=Quaternion.LookRotation(direction);}
            if(state.Failed)
            {
                flood.Paused=true;
                if(state.Immersion&&state.Immersion.logbook)SetFade(Mathf.MoveTowards(currentFade,1,Time.deltaTime));
                else if(trigger&&!previousTrigger&&(!menu||!menu.IsOpen))RetryDay();
            }
            else if((state.Complete||state.Immersion)&&nearBed&&grip&&!previousGrip&&(!menu||!menu.IsOpen))TrySleep();
            // Original Day 1 net rise/drain remains unchanged. Outage disables pumping.
            if(!state.Failed)
            {
                flood.Paused=state.Repaired;
                flood.RiseSpeed=(state.HasPower(PowerCircuit.Pump)?poweredLeakRiseMPerSecond:unpoweredLeakRiseMPerSecond)*state.Pipe.InflowFraction;
                if(state.Repaired && state.HasPower(PowerCircuit.Pump))
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
            if(state.Immersion&&!CanSleep&&!state.Failed&&!sleeping&&(!menu||!menu.IsOpen)&&Vector3.Distance(head.position,BedPosition)<1.8f)
            {
                if(coop!=null)coop.Send(LighthouseKeepers.Lobby.CoopAction.Bed);
                else if(immersionWorld&&immersionWorld.Narrator)immersionWorld.Narrator.EarlyBed();
                return false;
            }
            if(coop!=null)return CanSleep&&(!menu||!menu.IsOpen)&&Vector3.Distance(head.position,BedPosition)<1.8f&&coop.Send(LighthouseKeepers.Lobby.CoopAction.Bed);
            if(state.ReadOnly||sleeping||(menu&&menu.IsOpen)||!state.Complete||!state.HasNextDay||Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)>=1.8f)return false;
            SetBedMask(1);StartCoroutine(Sleep());return true;
        }
        public bool ApplyHostSnapshot(WatchSnapshot snapshot)
        {
            if(snapshot==null||snapshot.revision<Revision||!state.ApplyRemote(snapshot))return false;
            if(snapshot.revision!=Revision)
            {Revision=snapshot.revision;if(generator)generator.ResetInteraction();
            if(engineControls&&!state.RemoteReplica)engineControls.ResetProps();
            if(beaconStation&&!state.RemoteReplica)beaconStation.ResetProps();if(pipeRepair)pipeRepair.ResetView();if(radio)radio.ResetReplicaInteraction();whoosh.Play();if(menu)menu.Close();}
            SetBedMask(snapshot.bedMask);if(immersionWorld&&immersionWorld.Narrator)immersionWorld.Narrator.Apply(snapshot);
            sleeping=snapshot.sleeping;sleepDay=snapshot.sleepDay;replicaFadeTarget=snapshot.fade;
            if(s4Presentation)s4Presentation.Night(sleeping,sleepDay,snapshot.fade);
            if(logbook)logbook.Night(sleeping,sleepDay,snapshot.fade);
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
            if(state.Immersion&&state.Immersion.physicalGuidance)
            {
                string heading="NIGHT "+state.Day;
                int letters=Mathf.Clamp((int)(state.Elapsed/state.Immersion.chalkLetterSeconds),0,heading.Length);
                boardText.Append("Keep the light. Survive till dawn.\n").Append(heading,0,letters);
                if(letters<heading.Length)boardText.Append("<color=#FFFFFF00>").Append(heading,letters,heading.Length-letters).Append("</color>");
                boardText.Append(" — DAWN IN ").Append(Mathf.Max(0,(int)(state.Duration-state.Elapsed))/60).Append(':').Append((Mathf.Max(0,(int)(state.Duration-state.Elapsed))%60).ToString("00")).Append("\n");
            }
            else boardText.Append("KEEPER WATCH — DAY ").Append(state.Day).Append("\n\n");
            foreach(var task in state.Definition.tasks)
                boardText.Append(state.IsDone(task.stationId)?(state.Immersion?"✓ ":"[X] "):(state.Immersion?"○ ":"[ ] ")).Append(task.taskText).Append('\n');
            if(!state.Repaired)boardText.Append("Pipe: ").Append(state.Pipe.BoltsDone).Append("/3 bolts\n");
            if(state.Grid!=null)
            {boardText.Append("POWER: ").Append(state.Grid.Demand.ToString("0.##")).Append('/').Append(state.Grid.Capacity.ToString("0.##")).Append(state.Grid.Tripped?" — TRIPPED: RESET F2":!state.Grid.EngineRunning?" — ENGINE OFF":" — ");
                if(!state.Grid.Tripped&&state.Grid.EngineRunning)for(int i=0;i<4;i++)if(state.Grid.Selected((PowerCircuit)i))boardText.Append(((PowerCircuit)i).ToString().ToUpperInvariant()).Append(' ');
                boardText.Append('\n');}
            if(!state.BeaconShining&&state.Definition.beaconDarkLimitSeconds>0&&!state.Failed)boardText.Append("BEACON DARK: ").Append(Mathf.CeilToInt((float)state.BeaconDarkRemaining)).Append(" s TO LOSS\n");
            boardText.Append('\n');
            if(state.Failed)boardText.Append(state.FailureMessage).Append(state.Immersion?"\nOpen the logbook to try again.":"\nTrigger to retry this day / Menu for retry or lobby");
            else if(state.Complete)boardText.Append(state.HasNextDay?"GO TO BED — THIRD FLOOR":"ALL WATCHES COMPLETE\nMenu: Replay from Day 1 or Return to lobby");
            else boardText.Append(state.Beacon!=null?"PIPE F1 / POWER F2 / RADIO + BEACON F4\nFinish tasks, then sleep in F3":"PIPE F1 / GENERATOR F2 / RADIO F4\nFinish tasks, then sleep in F3");
            if(!state.Immersion&&!state.Failed&&(!state.Complete||state.HasNextDay))boardText.Append("\nLEFT MENU: Return to lobby (Editor: Esc)");
            if(crewBeds>0&&!sleeping)boardText.Append("\nWaiting for other keeper (").Append(crewBeds).Append('/').Append(crewKeepers).Append(" in bed)");
            if(!string.IsNullOrEmpty(crewNotice))boardText.Append('\n').Append(crewNotice);
            if(state.Immersion)boardText.Append("\nWater too high or light dark too long = lost.");
            board.text=boardText.ToString();
        }
        IEnumerator Sleep()
        {
            if(state.Tuning&&state.Tuning.nightSequence)
            {
                sleeping=true;sleepDay=state.Day;bedPrompt.text="";
                var tuning=state.Tuning;
                for(float t=0;t<tuning.fadeOutSeconds;t+=Time.deltaTime){SetFade(t/tuning.fadeOutSeconds);yield return null;}SetFade(1);
                yield return new WaitForSeconds(tuning.nightHoldSeconds);
                state.Sleep();BeginWatch();
                for(float t=0;t<tuning.fadeInSeconds;t+=Time.deltaTime){SetFade(1-t/tuning.fadeInSeconds);yield return null;}
                sleeping=false;sleepDay=0;SetFade(0);yield break;
            }
            sleeping=true;bedPrompt.text="";
            for(float t=0;t<1;t+=Time.deltaTime){SetFade(t);yield return null;}SetFade(1);
            yield return new WaitForSeconds(.2f);state.Sleep();BeginWatch();
            for(float t=0;t<.8f;t+=Time.deltaTime){SetFade(1-t/.8f);yield return null;}SetFade(0);sleeping=false;
        }
        void SetFade(float value){currentFade=value;fade.enabled=value>0;fadeMaterial.color=new Color(0,0,0,value);if(s4Presentation)s4Presentation.Night(sleeping,sleepDay,value);if(logbook)logbook.Night(sleeping,sleepDay,value);}
        string SleepPrompt
        {
            get
            {
#if UNITY_EDITOR
                if(state.Tuning&&state.Tuning.desktopSleep&&!UnityEngine.XR.XRSettings.isDeviceActive)return "PRESS F TO SLEEP";
#endif
                return "PRESS GRIP TO SLEEP";
            }
        }
        void OnDestroy(){if(state!=null)state.Changed-=StateChanged;if(fadeMaterial)Destroy(fadeMaterial);}
    }
}
