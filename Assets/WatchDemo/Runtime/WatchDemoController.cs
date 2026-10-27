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
        [SerializeField] TextMesh board,clock,dayTitle,bedPrompt,repairPrompt;
        [SerializeField] Renderer fade;
        [SerializeField] AudioSource whoosh,leakAudio;
        WatchState state=new WatchState();
        float repairProgress,titleUntil;
        int shownSecond=-1;
        bool sleeping,previousGrip,previousTrigger;
        Quaternion originalWrenchRotation;
        Material fadeMaterial;
        WrenchHaptics wrenchHaptics;
        public WatchState State=>state;
        public float RepairProgress=>repairProgress;
        void Start()
        {
            wrenchHaptics=wrench.GetComponent<WrenchHaptics>();originalWrenchRotation=wrenchVisual.localRotation;fadeMaterial=fade.material;
            LighthouseKeepers.StormWatch.SoloWatchSession.ConsumeRequest();
            BeginWatch();
        }
        void BeginWatch()
        {
            flood.ResetFlood();flood.SetHeight(.02f);flood.RiseSpeed=.0015f;flood.SurgePerMetre=.1f;flood.ResumeFlood();
            CancelRepair();leak.Play();if(leakAudio)leakAudio.Play();titleUntil=Time.time+3;dayTitle.text="DAY "+state.Day;whoosh.Play();repairProgress=0;shownSecond=-1;RefreshBoard();
        }
        void Update()
        {
            if(sleeping)return;
            var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            left.TryGetFeatureValue(CommonUsages.gripButton,out bool lg);right.TryGetFeatureValue(CommonUsages.gripButton,out bool rg);
            left.TryGetFeatureValue(CommonUsages.triggerButton,out bool lt);right.TryGetFeatureValue(CommonUsages.triggerButton,out bool rt);
            bool grip=lg||rg,trigger=lt||rt;
            state.Tick(Time.deltaTime);
            int second=(int)state.Elapsed;if(second!=shownSecond){shownSecond=second;clock.text=$"{second/60:00}:{second%60:00} / 07:00";}
            if(Time.time>titleUntil)dayTitle.text="";
            bool nearBed=Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)<1.8f;
            bedPrompt.text=nearBed?(state.Repaired?"PRESS GRIP TO SLEEP":"Repair the leak before resting"):"";
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
                if(repairProgress>0)CancelRepair();flood.Paused=true;board.text="WATCH ENDED\n\n[ ] Fix the flood\n\nPress trigger to retry";
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
                flood.Paused=true;flood.SetHeight(Mathf.MoveTowards(flood.Height,flood.MinimumHeight,Time.deltaTime*.012f));
                if(nearBed&&grip&&!previousGrip)TrySleep();
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
            if(sleeping||!state.Repaired||Vector3.Distance(head.position,bed.position+Vector3.up*1.3f)>=1.8f)return false;
            StartCoroutine(Sleep());return true;
        }
        void CancelRepair(){repairProgress=0;wrenchVisual.localRotation=originalWrenchRotation;}
        void RefreshBoard(){board.text="KEEPER WATCH — DAY "+state.Day+"\n\n"+(state.Repaired?"[X] FIX THE FLOOD\n\n[ ] GO TO BED\nTHIRD FLOOR":"[ ] FIX THE FLOOD\n\n1. TAKE WRENCH\n2. FIND LEAK\n3. REPAIR PIPE");}
        IEnumerator Sleep()
        {
            sleeping=true;bedPrompt.text="";
            for(float t=0;t<1;t+=Time.deltaTime){SetFade(t);yield return null;}SetFade(1);
            yield return new WaitForSeconds(.2f);state.Sleep();BeginWatch();
            for(float t=0;t<.8f;t+=Time.deltaTime){SetFade(1-t/.8f);yield return null;}SetFade(0);sleeping=false;
        }
        void SetFade(float value){fadeMaterial.color=new Color(0,0,0,value);}
        void OnDestroy(){if(fadeMaterial)Destroy(fadeMaterial);}
    }
}
