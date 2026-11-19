using UnityEngine;
using UnityEngine.AI;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Lobby;

namespace LighthouseKeepers.WatchDemo
{
    // Host observes real poses/navigation. Clients only present the validated scene frame.
    [DefaultExecutionOrder(1500)]
    public sealed class NightmareScene : MonoBehaviour
    {
        WatchDemoController watch;HorrorTuning tuning;HunterNavigation navigation;ScareKit kit;
        Vector3[] path;int corner;float nextPath;int seenAttempt,seenBeat=-1;bool advanced;
        readonly NightmareDoor[] doors=new NightmareDoor[2];
        public Component DoorTarget(float value){if(!float.IsFinite(value)||value<0||value>3)return null;int i=Mathf.FloorToInt(value/2);return i<2?doors[i]?.Handle:null;}
        readonly RaycastHit[] hits=new RaycastHit[32];
        public static void Create(WatchDemoController w)
        {if(w.State.Survival)return;var n=w.gameObject.AddComponent<NightmareScene>();n.watch=w;n.tuning=HorrorTuning.Current;}
        void Start(){navigation=new HunterNavigation(tuning,Resources.Load<NavMeshData>("HunterNavigation"));kit=new ScareKit(transform);doors[0]=NightmareDoor.Create(watch,0,new Vector3(-.55f,0,-3.86f));doors[1]=NightmareDoor.Create(watch,1,new Vector3(-.55f,0,-9.86f));}
        public bool Begin(bool developer=false)
        {
            var s=watch.State;
            if(s.ReadOnly||s.Survival||s.Day!=2||!tuning.nightmareEnabled||!navigation.Ready||!developer&&!s.Complete)return false;
            if(!navigation.Route(tuning.nightmareStart,tuning.nightmareCorner,out path))return false;
            int humans=s.HumanKeeperCount>1?3:(int)watch.CurrentDuty;
            if(!s.Scare.Begin(humans,path[0],PlayerPrefs.GetInt("LK.Nightmare.Seen",0)>0))return false;
            s.Recovery.SetHumans(humans);corner=1;advanced=false;return true;
        }
        public bool Retry()
        {if(!watch.State.RetryScare())return false;corner=1;path=null;nextPath=0;advanced=false;return true;}
        public void Skip(){if(watch.State.Scare.Skip())watch.State.ResetSceneRecovery();}
        public string Instruction
        {
            get{
                var c=watch.State.Scare;bool down=watch.State.Recovery.Downed((int)watch.CurrentDuty);
                if(down)return c.CanRescue?"Your partner is coming. Hold out your hand.":"Your partner must escape outside, then return for you.";
                return c.Beat switch{
                    ScareBeat.Wake=>"3:00 AM — follow the child",
                    ScareBeat.Follow=>"Follow her downstairs",
                    ScareBeat.Corner=>"Look at the child",
                    ScareBeat.Turn=>"Keep the stairs clear",
                    ScareBeat.Warning or ScareBeat.Quiet or ScareBeat.Escape=>"RUN — hold A",
                    ScareBeat.Return=>"Go back — GRIP your partner’s hand",
                    ScareBeat.Relief=>"You're awake. Stay together.",_=>""};
            }
        }
        public string Location=>watch.State.Scare.Beat switch{ScareBeat.Wake=>"Stairs · FLOOR 3",ScareBeat.Follow or ScareBeat.Corner or ScareBeat.Turn=>"Generator room · FLOOR 2",ScareBeat.Warning or ScareBeat.Quiet or ScareBeat.Escape=>"FLOOR 1 → House → outside · GRIP doors, pull",ScareBeat.Return=>"Return to where your partner was caught",_=>""};
        public string Caption=>watch.State.Scare.Beat switch{
            ScareBeat.Wake=>"Can you come with me? I'm scared.",
            ScareBeat.Turn=>"You're not him.",
            ScareBeat.Relief=>"Keepers? You've been silent for ten minutes. What happened up there?",_=>""};
        public string Speaker=>string.IsNullOrEmpty(Caption)?"":watch.State.Scare.Beat==ScareBeat.Relief?"HARBOR MASTER":"Unknown";
        int Duty(int actor)=>watch.State.Coop is ICoopKeeperDuties roles?(int)roles.Duty(actor):(int)watch.CurrentDuty;
        bool Floor(Vector3 eye,out Vector3 foot)
        {
            foot=default;int count=Physics.RaycastNonAlloc(eye,Vector3.down,hits,3,~0,QueryTriggerInteraction.Ignore);float best=float.MaxValue;
            if(count==hits.Length)return false;
            for(int i=0;i<count;i++){var h=hits[i];if(h.normal.y<.65f||h.collider.attachedRigidbody||h.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())continue;if(h.distance<best){best=h.distance;foot=h.point;}}
            return best<float.MaxValue&&navigation.SampleFloor(foot,out foot);
        }
        bool Seen(Vector3 eye,Vector3 forward,Vector3 point)
        {
            var delta=point-eye;if(delta.magnitude>6||Vector3.Dot(delta.normalized,forward)<.72f)return false;
            int count=Physics.RaycastNonAlloc(eye,delta.normalized,hits,delta.magnitude,~0,QueryTriggerInteraction.Ignore);
            if(count==hits.Length)return false;
            for(int i=0;i<count;i++)if(!hits[i].collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>()&&!hits[i].collider.attachedRigidbody)return false;
            return true;
        }
        public static bool Outside(Vector3 eye)=>eye.z< -10.1f&&Mathf.Abs(eye.x)<2.5f&&eye.y<3;
        void Update()
        {
            if(!watch||kit==null)return;
            var s=watch.State;var c=s.Scare;
            if(!s.ReadOnly&&c.Active){
                int humans=s.HumanKeeperCount>1?3:(int)watch.CurrentDuty;
                if(c.CrewChanged(humans,tuning.nightmareStart)){s.ResetSceneRecovery();path=null;nextPath=0;}
                watch.DrownedViewers(out int a,out var p,out var f,out int b,out var q,out var g);
                int da=Duty(a),db=b>0?Duty(b):0;
                bool seen=Seen(p,f,c.Body+Vector3.up*1.1f)||b>0&&Seen(q,g,c.Body+Vector3.up*1.1f);
                int outside=(Outside(p)?da:0)|(b>0&&Outside(q)?db:0);
                bool atCorner=(c.Body-tuning.nightmareCorner).sqrMagnitude<.15f;
                if(c.Beat==ScareBeat.Follow){
                    if(path==null&&navigation.Route(c.Body,tuning.nightmareCorner,out path))corner=1;
                    // Wait for the rear keeper; no timed disappearance or through-wall catch-up.
                    float distance=Vector3.Distance(c.Body,p-Vector3.up*1.5f);
                    if(b>0)distance=Mathf.Max(distance,Vector3.Distance(c.Body,q-Vector3.up*1.5f));
                    if(distance<tuning.nightmareFollowDistance)Move(tuning.nightmareFollowSpeed*Time.deltaTime);
                }
                if(c.Beat==ScareBeat.Escape){
                    bool first=(c.Caught&da)==0&&(c.Escaped&da)==0&&!Outside(p),second=db!=0&&(c.Caught&db)==0&&(c.Escaped&db)==0&&!Outside(q);
                    int target=first&&(!second||(p-c.Body).sqrMagnitude<(q-c.Body).sqrMagnitude)?da:second?db:0;
                    var eye=target==da?p:q;
                    if(target!=0&&Floor(eye,out var feet)){
                        if(Time.unscaledTime>=nextPath){nextPath=Time.unscaledTime+tuning.navRepathSeconds;path=null;if(navigation.Route(c.Body,feet,out path))corner=1;}
                        float speed=c.Age>tuning.nightmareEscapeSeconds*.8f?tuning.nightmareLateChaseSpeed:tuning.nightmareChaseSpeed;
                        float gap=Vector3.Distance(c.Body,feet);
                        if(gap>tuning.nightmareCatchDistance)Move(Mathf.Min(speed*Time.deltaTime,gap-tuning.nightmareCatchDistance));
                        else if(navigation.SegmentClear(c.Body,feet)&&s.Recovery.Down(target))c.Catch(target,feet);
                    }
                }
                int down=(s.Recovery.Downed(1)?1:0)|(s.Recovery.Downed(2)?2:0);
                c.Tick(Mathf.Min(Time.deltaTime,.1f),seen,atCorner,outside,down);
                if(c.CanRescue)s.Recovery.Tick(Mathf.Min(Time.deltaTime,.1f));
            }
            if(seenAttempt!=c.Attempt||seenBeat!=(int)c.Beat){
                if(seenAttempt!=c.Attempt&&c.Attempt>1)watch.NightmareCheckpoint();
                seenAttempt=c.Attempt;seenBeat=(int)c.Beat;
                if(c.Active){watch.GetComponent<ObjectiveHud>()?.RevealFor(8);var radio=FindAnyObjectByType<HarborRadio>();if(radio&&!string.IsNullOrEmpty(Caption))radio.RecordSilentLine(Caption,Speaker);}
                kit.Beat(c.Beat,c.Age);
                if(c.Beat==ScareBeat.Turn)watch.GetComponent<AdaptiveMusic>()?.RequestAccent(c.Age);
                if(c.Beat==ScareBeat.Escape){path=null;nextPath=0;}
            }
            kit.Present(c,Camera.main?Camera.main.transform.position:Vector3.zero);
            if(!c.Paused&&!c.Finished)advanced=false;
            if(c.Finished&&!advanced){advanced=true;PlayerPrefs.SetInt("LK.Nightmare.Seen",1);PlayerPrefs.Save();if(!s.ReadOnly)watch.FinishNightmare();}
        }
        void Move(float metres)
        {
            if(path==null||corner>=path.Length)return;
            var c=watch.State.Scare;var p=c.Body;var next=Vector3.MoveTowards(p,path[corner],metres);
            if(!navigation.SegmentClear(p,next))return;
            var delta=next-p;float yaw=delta.sqrMagnitude>.00001f?Quaternion.LookRotation(Vector3.ProjectOnPlane(delta,Vector3.up)).eulerAngles.y:c.Yaw;
            c.Place(next,yaw);if((next-path[corner]).sqrMagnitude<.0025f)corner++;
        }
        void OnDestroy(){navigation?.Dispose();kit?.Dispose();}
    }
}
