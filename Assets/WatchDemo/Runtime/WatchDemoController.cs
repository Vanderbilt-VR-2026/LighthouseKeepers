using LighthouseKeepers.Core.Progression;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchDemoController : MonoBehaviour, LighthouseKeepers.Lobby.ICoopWatchView, LighthouseKeepers.Lobby.ICoopBedPresentation, LighthouseKeepers.Lobby.ICoopPressureView, LighthouseKeepers.Lobby.ICoopRadioView
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
        public float WaterHeightAt(Vector3 position)
        {
            if(position.z>=LighthouseKeepers.Core.HouseFloodState.DoorNorthZ)return flood.Height;
            return state?.HouseFlood!=null&&state.HouseFlood.WetAt(position)?state.HouseFlood.Height:float.NegativeInfinity;
        }
        public Transform BoardTransform=>board.transform;
        readonly System.Text.StringBuilder boardText = new System.Text.StringBuilder(256);
        float repairProgress;
        int shownSecond=-1,shownChalkLetters=-1;
        bool sleeping,previousGrip,previousTrigger;
        public bool ObjectivePresentationBlocked=>sleeping||(state!=null&&state.Failed)||(menu&&menu.IsOpen);
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
        AdaptiveMusic adaptiveMusic;
        ImmersionLogbook logbook;
        public bool LocalWrenchHeld=>wrench.isSelected;
        public bool Sleeping=>sleeping;
        bool shownWatchHeld;
        double ChalkSeconds=>state.Elapsed+state.WatchGateElapsed;
        public bool WrenchHeld=>wrench.isSelected||coop?.RemoteHeld(wrench)==true;
        public void SetBedMask(int value){if(value!=bedMask&&value!=0)GetComponent<WorldAudioGaps>()?.BunkChanged();bedMask=value;if(immersionWorld)immersionWorld.SetBeds(value);}

        public Component Owner=>this;
        public void SetNetworkMode(bool practice,bool replica){state.SetNetworkRehearsal(practice);state.SetRemoteReplica(replica);}
        public string CaptureState()
        {
            var value=state.Capture(flood.Height);value.revision=Revision;value.sleeping=sleeping;value.fade=currentFade;
            value.beds=crewBeds;value.keepers=crewKeepers;value.notice=crewNotice;value.sleepDay=sleepDay;value.bedMask=bedMask;if(immersionWorld&&immersionWorld.Narrator)immersionWorld.Narrator.Capture(value);if(adaptiveMusic)adaptiveMusic.Capture(value);return JsonUtility.ToJson(value);
        }
        public bool ApplyState(string json)=>ApplyHostSnapshot(JsonUtility.FromJson<WatchSnapshot>(json));
        public string DescribeState()=>$"day={state.Day} elapsed={state.Elapsed:F2} water={flood.Height:F4} power={state.Power} tasks={state.CompletedTasks}";
        public WatchState State=>state;
        public float RepairProgress=>state==null?0:state.Pipe.BoltsDone+state.Pipe.StrokeDegrees/state.Pipe.Profile.quarterTurnDegrees;
        void Start()
        {
            var powerTuning=S4M2Tuning.Current;
            var campaign=Resources.Load<FiveNightCatalog>("M3Campaign");
            var definitions=campaign&&campaign.enabled?campaign.nights:Resources.Load<DayCatalog>(powerTuning?S4M2Tuning.DayCatalogResource:"WatchDays").days;
            state = new WatchState(definitions, LighthouseKeepers.StormWatch.WatchDayLaunch.Consume(),S4Tuning.Current,powerTuning,ImmersionTuning.Current);
            if(state.Tuning&&state.Tuning.difficulty)
            {poweredLeakRiseMPerSecond=state.Tuning.poweredRiseMPerSecond;unpoweredLeakRiseMPerSecond=state.Tuning.unpoweredRiseMPerSecond;repairedDrainMPerSecond=state.Tuning.drainMPerSecond;}
            coop=LighthouseKeepers.Lobby.CoopWatchBridge.Attach?.Invoke(this);state.Coop=coop;
            state.Changed += StateChanged;
            adaptiveMusic=AdaptiveMusic.Create(this);
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
                BunkPlacement.Apply(bed);
                immersionWorld=ImmersionWorld.Create(this,head,bed,board,wrench.transform,pipeRepair,generator,radio,beacon);
                if(state.Immersion.logbook)logbook=ImmersionLogbook.Create(head,transform);
            }
            BeginWatch();
            if(state.HouseFlood!=null)WatchHouseFlood.Create(this);
            pressureValve=WatchPressureValve.Create(this);
            CreateCrewRadios();
            InitializeDirector();
            if(state.PressureEnabled)PressurePresentation.Create(this,head);
            if(state.Storm!=null)StormPresentation.Create(this);
            if(state.Master&&state.Master.onboarding)OnboardingPresentation.Create(this,head,bed,pipeRepair);
            if(state.Master&&state.Master.worldSafety)MasterWorldSafety.Create(this);
            if(state.Master&&state.Master.movement)KeeperMovement.Create(this);
            // Aperture guards are structural even when optional movement features are disabled.
            PoleSlide.Create(this,FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>());
            ObjectiveHud.Create(this,head);
            KeeperFearPresentation.Create(this,head);
            ShipFleetPresentation.Create(this);
            QuestRenderQuality.Create(this);
            WorldLabelRaster.Create(this);
            PracticalAtmosphere.Create(this);
            WorldWaterPresentation.Create(this);
            // Exterior sea/coast comes from Eli's StormSeascape; no second coastline.
            StoryDressing.Create(this);
            WorldAudioGaps.Create(this);
            IslandSafety.Create(this);
            GeneratorIdentity.Create();
            ConnectedMachinePresentation.Create(this);
            if(state.NetworkRehearsal){flood.Paused=true;clock.text=LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.1");}
            if(state.RemoteReplica){flood.Paused=true;clock.text=LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.2");}
        }
        void BeginWatch()
        {
            if(coop!=null&&!state.RemoteReplica)Revision++;
            flood.ResetFlood();flood.SetHeight(.02f);flood.RiseSpeed=(state.HasPower(PowerCircuit.Pump)?poweredLeakRiseMPerSecond:unpoweredLeakRiseMPerSecond)*state.FloodRiseMultiplier;flood.SurgePerMetre=.1f;flood.ResumeFlood();
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
            if(KeeperAudioMix.Instance)KeeperAudioMix.Instance.PreScareQuiet=state.TensionQuiet;
            UpdateCrewRadios();
            UpdateDirector();
            if(state.Immersion&&state.Immersion.physicalGuidance)
            {
                int letters=Mathf.Min(16,(int)(ChalkSeconds/state.Immersion.chalkLetterSeconds));
                if(letters!=shownChalkLetters){shownChalkLetters=letters;RefreshBoard();}
            }
            if(state.RemoteReplica){UpdateReplicaInput();return;}
            if(sleeping)return;
            if(state.NetworkRehearsal){flood.Paused=true;return;}
            if(coop!=null && !coop.Ready){flood.Paused=true;clock.text=LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.3");return;}
            var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            left.TryGetFeatureValue(CommonUsages.gripButton,out bool lg);right.TryGetFeatureValue(CommonUsages.gripButton,out bool rg);
            left.TryGetFeatureValue(CommonUsages.triggerButton,out bool lt);right.TryGetFeatureValue(CommonUsages.triggerButton,out bool rt);
            bool grip=lg||rg,trigger=lt||rt;
            if(WrenchHeld)state.EngageWatch();
            if(state.Director!=null){state.OfferStationGrace(state.Director.First);state.OfferStationGrace(state.Director.Second);}
            bool watchRunning=state.AdvanceWatchGate(Time.deltaTime);
            if(watchRunning)state.Tick(Time.deltaTime);
            if(state.Tutorial&&flood.Height>state.Master.tutorialFloodCap)flood.SetHeight(state.Master.tutorialFloodCap);
            state.ObserveWater(Time.deltaTime,flood.Height);
            int second=(int)state.Elapsed;if(second!=shownSecond||shownWatchHeld!=state.WatchStartHeld){shownSecond=second;shownWatchHeld=state.WatchStartHeld;int remaining=Mathf.Max(0,(int)state.Duration-second);clock.text=state.Failed?state.FailureMessage+LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.4"):string.Format(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.5"),state.Day,remaining/60,remaining%60,state.CompletedTasks,state.Definition.tasks.Length);RefreshBoard();}
            bool nearBed=Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)<1.8f;
            bedPrompt.text=nearBed?(state.Complete?(state.HasNextDay?SleepPrompt:LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.6")):LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.7")):"";
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
                flood.Paused=state.Repaired||!watchRunning;
                flood.RiseSpeed=(state.HasPower(PowerCircuit.Pump)?poweredLeakRiseMPerSecond:unpoweredLeakRiseMPerSecond)*state.LeakStrength*state.FloodRiseMultiplier;
                if(state.Repaired && state.HasPower(PowerCircuit.Pump))
                    flood.SetHeight(Mathf.MoveTowards(flood.Height,flood.MinimumHeight,Time.deltaTime*repairedDrainMPerSecond));
            }
            if(watchRunning)state.AdvanceHouseFlood(Time.deltaTime,flood.Height);
            previousGrip=grip;previousTrigger=trigger;
        }
        // Kept for historical tooling: seating never starts an automatic repair anymore.
        public bool TryBeginRepair()=>!state.Failed&&!state.Repaired&&!sleeping&&pipeRepair&&pipeRepair.InContact;
        public bool RetryDay(){if(state.RemoteReplica&&coop!=null)return state.Failed&&!sleeping&&coop.Send(LighthouseKeepers.Lobby.CoopAction.Retry);if(state.ReadOnly||!state.Failed||sleeping)return false;if(state.Sea.lost)GetComponent<IslandSafety>()?.ReturnToHouse();state.Retry();BeginWatch();if(menu)menu.Close();return true;}
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
            if(adaptiveMusic)adaptiveMusic.Apply(snapshot);
            SetBedMask(snapshot.bedMask);if(immersionWorld&&immersionWorld.Narrator)immersionWorld.Narrator.Apply(snapshot);
            sleeping=snapshot.sleeping;sleepDay=snapshot.sleepDay;replicaFadeTarget=snapshot.fade;
            if(s4Presentation)s4Presentation.Night(sleeping,sleepDay,snapshot.fade);
            if(logbook)logbook.Night(sleeping,sleepDay,snapshot.fade);
            crewBeds=snapshot.beds;crewKeepers=snapshot.keepers;crewNotice=snapshot.notice??"";
            flood.Paused=true;flood.SetHeight(snapshot.water);
            int remaining=Mathf.Max(0,(int)(state.Duration-state.Elapsed));
            clock.text=state.Failed?state.FailureMessage+LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.4"):string.Format(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.5"),state.Day,remaining/60,remaining%60,state.CompletedTasks,state.Definition.tasks.Length);
            bedPrompt.text="";RefreshBoard();return true;
        }
        void CancelRepair(){repairProgress=0;}
        void StateChanged(){shownSecond=-1;RefreshBoard();}
        void RefreshBoard()
        {
            if(state.Director!=null){DirectorBoard();return;}
            boardText.Clear();
            if(state.NetworkRehearsal)
            {board.text=LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.8");return;}
            if(state.Immersion&&state.Immersion.physicalGuidance)
            {
                string heading=LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.35")+state.Day;
                int letters=Mathf.Clamp((int)(ChalkSeconds/state.Immersion.chalkLetterSeconds),0,heading.Length);
                boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.9")).Append(heading,0,letters);
                if(letters<heading.Length)boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.10")).Append(heading,letters,heading.Length-letters).Append("</color>");
                boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.11")).Append(Mathf.Max(0,(int)(state.Duration-state.Elapsed))/60).Append(':').Append((Mathf.Max(0,(int)(state.Duration-state.Elapsed))%60).ToString("00")).Append("\n");
            }
            else boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.12")).Append(state.Day).Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.13"));
            foreach(var task in state.Definition.tasks)
                boardText.Append(state.IsDone(task.stationId)?(state.Immersion?"✓ ":LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.14")):(state.Immersion?"○ ":"[ ] ")).Append(task.taskText).Append(state.IsPending(task.stationId)?LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.15"):"").Append('\n');
            if(!state.Repaired)boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.16")).Append(state.Pipe.BoltsDone).Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.36"));
            if(state.Grid!=null)
            {boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.17")).Append(state.Grid.Demand.ToString("0.##")).Append('/').Append(state.Grid.Capacity.ToString("0.##")).Append(state.Grid.Tripped?LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.18"):!state.Grid.EngineRunning?LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.19"):" — ");
                if(!state.Grid.Tripped&&state.Grid.EngineRunning)for(int i=0;i<4;i++)if(state.Grid.Selected((PowerCircuit)i))boardText.Append(((PowerCircuit)i).ToString().ToUpperInvariant()).Append(' ');
                boardText.Append('\n');}
            if(!state.BeaconShining&&state.BeaconGraceRemaining>0&&!state.Failed)boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.20")).Append(Mathf.CeilToInt((float)state.BeaconGraceRemaining)).Append(state.Power==PowerState.Off?LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.21"):LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.22"));
            else if((!state.BeaconShining||state.Ships?.Danger>0)&&state.BeaconDarkLimit>0&&!state.Failed)boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.23")).Append(Mathf.CeilToInt((float)state.BeaconDarkRemaining)).Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.24"));
            boardText.Append('\n');
            if(state.Failed)boardText.Append(state.FailureMessage).Append(state.Immersion?LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.25"):LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.37"));
            else if(state.Complete)boardText.Append(state.HasNextDay?LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.26"):LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.27"));
            else boardText.Append(state.Beacon!=null?LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.38"):LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.39"));
            if(!state.Immersion&&!state.Failed&&(!state.Complete||state.HasNextDay))boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.28"));
            if(crewBeds>0&&!sleeping)boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.29")).Append(crewBeds).Append('/').Append(crewKeepers).Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.30"));
            if(!string.IsNullOrEmpty(crewNotice))boardText.Append('\n').Append(crewNotice);
            if(state.Immersion)boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.31"));
            board.text=boardText.ToString();
        }
        IEnumerator Sleep()
        {
            if(state.Tuning&&state.Tuning.nightSequence)
            {
                sleeping=true;if(adaptiveMusic)adaptiveMusic.Survived();sleepDay=state.Day;bedPrompt.text="";
                var tuning=state.Tuning;
                for(float t=0;t<tuning.fadeOutSeconds;t+=Time.deltaTime){SetFade(t/tuning.fadeOutSeconds);yield return null;}SetFade(1);
                yield return new WaitForSeconds(tuning.nightHoldSeconds);
                state.Sleep();BeginWatch();
                for(float t=0;t<tuning.fadeInSeconds;t+=Time.deltaTime){SetFade(1-t/tuning.fadeInSeconds);yield return null;}
                sleeping=false;sleepDay=0;SetFade(0);yield break;
            }
            sleeping=true;if(adaptiveMusic)adaptiveMusic.Survived();bedPrompt.text="";
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
                if(state.Tuning&&state.Tuning.desktopSleep&&!UnityEngine.XR.XRSettings.isDeviceActive)return LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.32");
#endif
                return LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.33");
            }
        }
        void OnDestroy(){if(state!=null)state.Changed-=StateChanged;if(fadeMaterial)Destroy(fadeMaterial);}
    }
}
