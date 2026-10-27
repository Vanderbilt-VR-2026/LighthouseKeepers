using LighthouseKeepers.Core.Progression;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Flood;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class WatchDemoController : MonoBehaviour
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
        [SerializeField, Min(0)] float poweredLeakRiseMPerSecond=.0015f;
        [SerializeField, Min(0)] float unpoweredLeakRiseMPerSecond=.0045f;
        [SerializeField, Min(0)] float repairedDrainMPerSecond=.012f;
        WatchState state;
        readonly System.Text.StringBuilder boardText = new System.Text.StringBuilder(256);
        float repairProgress;
        int shownSecond=-1;
        bool sleeping,previousGrip,previousTrigger;
        Quaternion originalWrenchRotation;
        Material fadeMaterial;
        WrenchHaptics wrenchHaptics;
        public WatchState State=>state;
        public float RepairProgress=>repairProgress;
        void Start()
        {
            state = new WatchState(Resources.Load<DayCatalog>("WatchDays").days, LighthouseKeepers.StormWatch.WatchDayLaunch.Consume());
            state.Changed += RefreshBoard;
            if (generator) generator.Bind(state);
            if(emergencyLighting)emergencyLighting.Bind(state);
            if(radio)radio.Bind(state);
            wrenchHaptics=wrench.GetComponent<WrenchHaptics>();originalWrenchRotation=wrenchVisual.localRotation;fadeMaterial=fade.material;fade.enabled=false;
            LighthouseKeepers.StormWatch.SoloWatchSession.ConsumeRequest();
            BeginWatch();
        }
        void BeginWatch()
        {
            flood.ResetFlood();flood.SetHeight(.02f);flood.RiseSpeed=state.Power==PowerState.On?poweredLeakRiseMPerSecond:unpoweredLeakRiseMPerSecond;flood.SurgePerMetre=.1f;flood.ResumeFlood();
            if(generator)generator.ResetInteraction();
            CancelRepair();
            if(!state.Repaired){leak.Play();if(leakAudio)leakAudio.Play();}
            else {leak.Stop(true,ParticleSystemStopBehavior.StopEmitting);if(leakAudio)leakAudio.Stop();}
            whoosh.Play();repairProgress=0;shownSecond=-1;RefreshBoard();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"WATCH_DAY day={state.Day} power={state.Power} tasks={state.Definition.tasks.Length} duration={state.Duration}");
#endif
        }
        void Update()
        {
            if(sleeping)return;
            var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            left.TryGetFeatureValue(CommonUsages.gripButton,out bool lg);right.TryGetFeatureValue(CommonUsages.gripButton,out bool rg);
            left.TryGetFeatureValue(CommonUsages.triggerButton,out bool lt);right.TryGetFeatureValue(CommonUsages.triggerButton,out bool rt);
            bool grip=lg||rg,trigger=lt||rt;
            state.Tick(Time.deltaTime);
            int second=(int)state.Elapsed;if(second!=shownSecond){shownSecond=second;int remaining=Mathf.Max(0,(int)state.Duration-second);clock.text=$"D{state.Day}   {remaining/60:00}:{remaining%60:00}   {state.CompletedTasks}/{state.Definition.tasks.Length} TASKS";}
            bool nearBed=Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)<1.8f;
            bedPrompt.text=nearBed?(state.Complete?(state.HasNextDay?"PRESS GRIP TO SLEEP":"ALL WATCHES COMPLETE"):"Finish the board tasks before resting"):"";
            if(nearBed){var direction=bedPrompt.transform.position-head.position;direction.y=0;if(direction.sqrMagnitude>.01f)bedPrompt.transform.rotation=Quaternion.LookRotation(direction);}
            bool nearRepair=Vector3.Distance(wrench.transform.position,repairPoint.position)<.8f;
            // A nearby prompt is useful, but it must not float through the entire room.
            bool nearRepairPrompt=Vector3.Distance(head.position,repairPoint.position)<1.45f;
            repairPrompt.text=nearRepairPrompt?(state.Repaired?"REPAIRED":nearRepair?"HOLD WRENCH + PRESS TRIGGER":"REPAIR LEAK WITH WRENCH"):"";
            if(nearRepairPrompt)
            {
                var direction=repairPrompt.transform.position-head.position;direction.y=0;
                if(direction.sqrMagnitude>.01f)repairPrompt.transform.rotation=Quaternion.LookRotation(direction);
            }
            if(state.Failed)
            {
                if(repairProgress>0)CancelRepair();flood.Paused=true;
                if(trigger&&!previousTrigger){state.Retry();BeginWatch();}
            }
            else if(!state.Repaired)
            {
                if(repairProgress>0 && (!wrench.isSelected||!nearRepair))CancelRepair();
                if(repairProgress==0&&trigger&&!previousTrigger)TryBeginRepair();
                if(repairProgress>0)
                {
                    repairProgress+=Time.deltaTime;
                    wrenchVisual.localRotation=originalWrenchRotation*Quaternion.AngleAxis(Mathf.Sin(repairProgress*9)*55,Vector3.forward);
                    if(nearRepairPrompt)repairPrompt.text="TIGHTENING  "+Mathf.Min(100,Mathf.RoundToInt(repairProgress/(float)WatchState.RepairSeconds*100))+"%";
                    if(repairProgress>=WatchState.RepairSeconds){state.CompleteRepair();if(wrenchHaptics)wrenchHaptics.RepairCompleted();CancelRepair();flood.Paused=true;leak.Stop(true,ParticleSystemStopBehavior.StopEmitting);if(leakAudio)leakAudio.Stop();RefreshBoard();}
                }
            }
            else
            {
                if(nearBed&&grip&&!previousGrip)TrySleep();
            }
            // Original Day 1 net rise/drain remains unchanged. Outage disables pumping.
            if(!state.Failed)
            {
                flood.Paused=state.Repaired;
                flood.RiseSpeed=state.Power==PowerState.On?poweredLeakRiseMPerSecond:unpoweredLeakRiseMPerSecond;
                if(state.Repaired && state.Power==PowerState.On)
                    flood.SetHeight(Mathf.MoveTowards(flood.Height,flood.MinimumHeight,Time.deltaTime*repairedDrainMPerSecond));
            }
            previousGrip=grip;previousTrigger=trigger;
        }
        public bool TryBeginRepair()
        {
            if(state.Failed||state.Repaired||sleeping||!wrench.isSelected||Vector3.Distance(wrench.transform.position,repairPoint.position)>=.8f)return false;
            repairProgress=.001f;return true;
        }
        public bool TrySleep()
        {
            if(sleeping||!state.Complete||!state.HasNextDay||Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)>=1.8f)return false;
            StartCoroutine(Sleep());return true;
        }
        void CancelRepair(){repairProgress=0;wrenchVisual.localRotation=originalWrenchRotation;}
        void RefreshBoard()
        {
            shownSecond=-1; boardText.Clear();
            boardText.Append("KEEPER WATCH — DAY ").Append(state.Day).Append("\n\n");
            foreach(var task in state.Definition.tasks)
                boardText.Append(state.IsDone(task.stationId)?"[X] ":"[ ] ").Append(task.taskText).Append('\n');
            boardText.Append('\n');
            if(state.Failed)boardText.Append("WATCH ENDED\nPress trigger to retry");
            else if(state.Complete)boardText.Append(state.HasNextDay?"GO TO BED — THIRD FLOOR":"ALL WATCHES COMPLETE");
            else boardText.Append("PIPE F1 / GENERATOR F2 / RADIO F4\nFinish tasks, then sleep in F3");
            board.text=boardText.ToString();
        }
        IEnumerator Sleep()
        {
            sleeping=true;bedPrompt.text="";
            for(float t=0;t<1;t+=Time.deltaTime){SetFade(t);yield return null;}SetFade(1);
            yield return new WaitForSeconds(.2f);state.Sleep();BeginWatch();
            for(float t=0;t<.8f;t+=Time.deltaTime){SetFade(1-t/.8f);yield return null;}SetFade(0);sleeping=false;
        }
        void SetFade(float value){fade.enabled=value>0;fadeMaterial.color=new Color(0,0,0,value);}
        void OnDestroy(){if(state!=null)state.Changed-=RefreshBoard;if(fadeMaterial)Destroy(fadeMaterial);}
    }
}
