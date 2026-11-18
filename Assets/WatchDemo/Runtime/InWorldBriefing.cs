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
        Material paper,ink;bool finished;float nextSend;
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
            var forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;if(forward.sqrMagnitude<.5f)forward=Vector3.forward;
            const float distance=1.45f;
            // World-fixed folder, slightly below eye level. Never moves the tracked head or follows it.
            canvasRoot.transform.SetPositionAndRotation(head.position+forward*distance-Vector3.up*.16f,Quaternion.LookRotation(forward,Vector3.up));
            canvasRoot.transform.localScale=Vector3.one*.00115f;folder.Open();
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
