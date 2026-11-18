using LighthouseKeepers.Lobby;
using LighthouseKeepers.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
namespace LighthouseKeepers.WatchDemo
{
    // Each keeper reads their own folder after spawning. Only the initial arrival holds the watch.
    public sealed class InWorldBriefing:MonoBehaviour
    {
        WatchDemoController watch;Transform head;GameObject canvasRoot;CrewBriefing folder;
        Material paper,ink;bool finished,placing;float nextSend,openedAt,stableFor,verticalSpeed;Vector3 previousHead;
        public bool IsOpen=>folder&&folder.gameObject.activeSelf;
        public CrewBriefing Folder=>folder;
        public static InWorldBriefing Create(WatchDemoController w,Transform head)
        {
            var b=w.gameObject.AddComponent<InWorldBriefing>();b.watch=w;b.head=head;b.Build();
            b.finished=CrewBriefing.Seen;
            if(w.State.Coop==null)w.State.HoldBriefing(!b.finished);
            if(!b.finished)b.Open();else b.folder.gameObject.SetActive(false);
            return b;
        }
        void Build()
        {
            canvasRoot=new GameObject("House arrival duty folder",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(TrackedDeviceGraphicRaycaster),typeof(GraphicRaycaster));
            canvasRoot.transform.SetParent(transform,false);
            var canvas=canvasRoot.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=head.GetComponent<Camera>();canvas.overrideSorting=true;canvas.sortingOrder=120;
            canvasRoot.GetComponent<TrackedDeviceGraphicRaycaster>().checkFor3DOcclusion=false;
            canvasRoot.GetComponent<TrackedDeviceGraphicRaycaster>().checkFor2DOcclusion=false;
            ((RectTransform)canvasRoot.transform).sizeDelta=new Vector2(1120,650);
            folder=CrewBriefing.Create(canvasRoot.transform,Finish);
            paper=new Material(Resources.Load<Shader>("ImmersionLogbook"));ink=new Material(Resources.Load<Material>("HarborSubtitleSdf"));ink.renderQueue=paper.renderQueue+10;
            foreach(var g in folder.GetComponentsInChildren<Graphic>()){if(g is TMP_Text t)t.fontSharedMaterial=ink;else g.material=paper;}
        }
        public void Open()
        {
            folder.Open();canvasRoot.SetActive(false);placing=true;openedAt=Time.unscaledTime;stableFor=0;previousHead=head.position;
        }
        void Place()
        {
            var forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;if(forward.sqrMagnitude<.5f)forward=Vector3.forward;
            const float distance=1.45f;
            // World-fixed folder, slightly below eye level. Never moves the tracked head or follows it.
            canvasRoot.transform.SetPositionAndRotation(head.position+forward*distance-Vector3.up*.16f,Quaternion.LookRotation(forward,Vector3.up));
            canvasRoot.transform.localScale=Vector3.one*.00115f;canvasRoot.SetActive(true);placing=false;verticalSpeed=0;
        }
        void LateUpdate()
        {
            if(!folder||!folder.gameObject.activeSelf)return;
            if(placing){
                float age=Time.unscaledTime-openedAt;
                stableFor=Mathf.Abs(head.position.y-previousHead.y)<.025f?stableFor+Time.unscaledDeltaTime:0;previousHead=head.position;
                var origin=head.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>();
                bool ready=!UnityEngine.XR.XRSettings.isDeviceActive||origin&&(origin.CurrentTrackingOriginMode&UnityEngine.XR.TrackingOriginModeFlags.Floor)!=0;
                if(ready&&age>=.5f&&stableFor>=.25f||age>=3f)Place();
            }else{
                // Keep readable for seated users and late Quest origin/height corrections.
                // Only height adjusts; the page never rotates with a head turn.
                var p=canvasRoot.transform.position;p.y=Mathf.SmoothDamp(p.y,head.position.y-.16f,ref verticalSpeed,.15f,Mathf.Infinity,Time.unscaledDeltaTime);canvasRoot.transform.position=p;
            }
        }
        void Finish(){finished=true;if(watch.State.Coop==null)watch.State.HoldBriefing(false);nextSend=0;}
        void Update()
        {
            if(!finished&&watch.State.Coop==null){watch.State.HoldBriefing(true);return;}
            if(!finished||Time.unscaledTime<nextSend)return;
            if(watch.State.Coop==null){watch.State.HoldBriefing(false);return;}
            if(!watch.State.BriefingHeld)return;
            nextSend=Time.unscaledTime+.5f;watch.State.Coop.Send(CoopAction.BriefingComplete,0,true);
        }
        void OnDestroy(){if(paper)Destroy(paper);if(ink)Destroy(ink);}
    }
    public sealed partial class WatchDemoController:ICoopBriefingView
    {
        public bool BriefingHeld=>state?.BriefingHeld==true;
        public void HoldBriefing(bool held)=>state.HoldBriefing(held);
    }
}
