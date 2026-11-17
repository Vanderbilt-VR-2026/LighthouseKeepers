using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Net;
namespace LighthouseKeepers.WatchDemo
{
    // Host schedules the scene; each client renders the same recovery frame. No camera animation.
    [DefaultExecutionOrder(1450)]
    public sealed class PossessionLesson:MonoBehaviour
    {
        WatchDemoController watch;XROrigin rig;HorrorTuning tuning;CrewCompanion bot;
        readonly List<GameObject> gates=new();readonly List<LineRenderer> stains=new();
        Material iron,blood;int lastStage=-1;float soundAfter;AudioSource sound;
        readonly RaycastHit[] clearance=new RaycastHit[24];
        public static void Create(WatchDemoController w){var c=w.gameObject.AddComponent<PossessionLesson>();c.watch=w;c.rig=FindAnyObjectByType<XROrigin>();c.tuning=HorrorTuning.Current;}
        static readonly RaycastHit[] floorHits=new RaycastHit[24];
        public static bool Inside(Vector3 eye){
            if(eye.y<3.8f||eye.y>6.35f)return false;
            int count=Physics.RaycastNonAlloc(eye,Vector3.down,floorHits,3,~0,QueryTriggerInteraction.Ignore);
            if(count==floorHits.Length)return false;
            float highest=float.NegativeInfinity;bool room=false;
            for(int i=0;i<count;i++){
                var h=floorHits[i];var c=h.collider;
                if(c.attachedRigidbody||c.GetComponentInParent<XROrigin>()||c.GetComponentInParent<CrewAvatarView>()||h.normal.y<.55f||h.point.y<=highest)continue;
                highest=h.point.y;room=h.normal.y>.85f&&Mathf.Abs(h.point.y-3.3f)<.12f;
            }
            return room;
        }
        public string Caption=>KeeperStrings.Get(!DrownedAppearance.Sightings?"lesson.quiet."+watch.State.Recovery.Lesson:"lesson.line."+watch.State.Recovery.Lesson);
        public string Speaker=>watch.State.Recovery.Lesson==2?"":KeeperStrings.Get(DrownedAppearance.Sightings&&(watch.State.Recovery.Lesson==1||watch.State.Recovery.Lesson==3)?"lesson.unknown":"subtitle.speaker");
        public static float SafeLift(Vector3 head,float wanted,float padding,RaycastHit[] buffer)
        {
            float result=Mathf.Max(0,wanted);int n=Physics.SphereCastNonAlloc(head+Vector3.up*.1f,.16f,Vector3.up,buffer,result+padding,~0,QueryTriggerInteraction.Ignore);
            if(n==buffer.Length)return 0;
            for(int i=0;i<n;i++){var c=buffer[i].collider;if(c.attachedRigidbody||c.GetComponentInParent<XROrigin>()||c.GetComponentInParent<CrewAvatarView>())continue;result=Mathf.Min(result,Mathf.Max(0,buffer[i].distance-padding));}return result;
        }
        void Start()
        {
            bot=watch.GetComponent<CrewCompanion>();
            iron=new Material(Shader.Find("Universal Render Pipeline/Lit"));iron.SetColor("_BaseColor",new Color(.085f,.085f,.07f));
            blood=new Material(Shader.Find("Universal Render Pipeline/Lit"));blood.SetColor("_BaseColor",new Color(.16f,.008f,.012f));blood.SetFloat("_Smoothness",.8f);
            using(var navigation=new HunterNavigation(tuning,Resources.Load<NavMeshData>("HunterNavigation"))){
                foreach(var destination in new[]{new Vector3(2.6f,0,0),new Vector3(1,6.6f,0)}){
                    if(!navigation.Route(tuning.lessonPartnerSpot,destination,out var points))continue;
                    for(int i=1;i<points.Length;i++){
                        float a=points[i-1].y,b=points[i].y;
                        if(Mathf.Abs(b-3.3f)<.3f)continue;
                        float y=3.3f+Mathf.Sign(b-3.3f)*.22f;
                        if((a-y)*(b-y)>0)continue;
                        var p=Vector3.Lerp(points[i-1],points[i],Mathf.InverseLerp(a,b,y));
                        Gate(p,points[i]-points[i-1]);break;
                    }
                }
            }
            // Surface-bound streaks, raycast once. No projected full-screen effect or added lights.
            for(int i=0;i<16;i++){
                float a=i*Mathf.PI*2/16;var dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                if(!Physics.Raycast(new Vector3(0,5.8f,0),dir,out var hit,5,~0,QueryTriggerInteraction.Ignore)||Vector3.Dot(hit.normal,Vector3.up)>.4f)continue;
                var g=new GameObject("F2 wet wall streak");g.transform.SetParent(transform,false);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=blood;l.useWorldSpace=true;l.positionCount=3;l.startWidth=.028f;l.endWidth=.004f;l.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                var top=hit.point+hit.normal*.014f;l.SetPosition(0,top);l.SetPosition(1,top+Vector3.down*.01f);l.SetPosition(2,top+Vector3.down*.02f);l.enabled=false;stains.Add(l);
            }
            sound=new GameObject("F2 lesson latch").AddComponent<AudioSource>();sound.transform.SetParent(transform,false);sound.transform.position=tuning.lessonPartnerSpot+Vector3.up;sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=1;sound.maxDistance=10;sound.volume=.18f;LighthouseKeepers.Core.KeeperAudioMix.Route(sound,"SFX_World");
        }
        void Gate(Vector3 floor,Vector3 direction)
        {
            var g=new GameObject("F2 sealed stair gate");g.transform.SetParent(transform,false);g.transform.SetPositionAndRotation(new Vector3(floor.x,3.32f,floor.z),Quaternion.LookRotation(Vector3.ProjectOnPlane(direction,Vector3.up)));
            var c=g.AddComponent<BoxCollider>();c.center=Vector3.up*1.4f;c.size=new Vector3(1.65f,2.8f,.12f);
            for(int i=0;i<10;i++){var bar=GameObject.CreatePrimitive(PrimitiveType.Cube);bar.name="Iron gate bar";bar.transform.SetParent(g.transform,false);bar.transform.localPosition=new Vector3(-.79f+i*.175f,1.4f,0);bar.transform.localScale=new Vector3(.027f,2.8f,.035f);Destroy(bar.GetComponent<Collider>());var r=bar.GetComponent<Renderer>();r.sharedMaterial=iron;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
            for(int i=0;i<2;i++){var bar=GameObject.CreatePrimitive(PrimitiveType.Cube);bar.name="Gate crossbar";bar.transform.SetParent(g.transform,false);bar.transform.localPosition=new Vector3(0,.45f+i*1.65f,0);bar.transform.localScale=new Vector3(1.65f,.05f,.05f);Destroy(bar.GetComponent<Collider>());bar.GetComponent<Renderer>().sharedMaterial=iron;}
            g.SetActive(false);gates.Add(g);
        }
        void Update()
        {
            if(!watch||watch.State==null)return;var s=watch.State;var r=s.Recovery;
            if(!s.ReadOnly&&!s.Failed&&!watch.Sleeping){
                watch.DrownedViewers(out int a,out var p,out _,out int b,out var q,out _);
                bool pair=s.HumanKeeperCount>1?b>0&&Inside(p)&&Inside(q):bot&&bot.Active&&Inside(p)&&Mathf.Abs(bot.Position.y-3.3f)<.12f&&Vector3.Distance(bot.Position,tuning.lessonPartnerSpot)<.55f;
                int victim=s.HumanKeeperCount>1?(int)((s.Coop as ICoopKeeperDuties)?.Duty(b)??CrewDuty.None):(int)(bot?bot.Duty:CrewDuty.None);
                r.AdvanceLesson(Time.deltaTime,s.Day,s.Elapsed,pair,victim,tuning.possessionLesson);
            }
            bool locked=r.RoomSealed&&!s.Failed;foreach(var g in gates)if(g.activeSelf!=locked)g.SetActive(locked);
            if(lastStage!=r.Lesson){lastStage=r.Lesson;if(r.LessonActive){var narrator=watch.GetComponentInChildren<HarborRadio>();if(!narrator)narrator=FindAnyObjectByType<HarborRadio>();if(narrator)narrator.RecordSilentLine(Caption,Speaker);}if(lastStage==2||lastStage==5){sound.clip=Resources.Load<AudioClip>("S4Audio/bolt-clunk");if(sound.clip)sound.Play();}}
            float amount=DrownedAppearance.Sightings&&r.Lesson>=3&&r.Lesson<=5?Mathf.Clamp01((r.Lesson==3?r.LessonAge:8)/8f):0;
            if(r.Lesson==5)amount*=1-Mathf.Clamp01(r.LessonAge/tuning.lessonReliefSeconds);
            for(int i=0;i<stains.Count;i++){var l=stains[i];l.enabled=amount>0;if(!l.enabled)continue;var top=l.GetPosition(0);l.SetPosition(1,top+Vector3.down*(.22f+ i%4*.07f)*amount);l.SetPosition(2,top+Vector3.down*(.65f+i%3*.14f)*amount);}
        }
        void OnDestroy(){if(iron)Destroy(iron);if(blood)Destroy(blood);}
    }
}
