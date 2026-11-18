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
            // Always offer the folder on arrival. Returning players can skip immediately;
            // a saved read receipt must not silently remove the first in-world instruction.
            b.finished=false;
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
            folder.Open();Place();
        }
        void Place()
        {
            const float distance=1.45f;
            // This reading-only HUD follows the actual tracked eyes, including seated height,
            // recentering and late origin corrections. It never moves the camera.
            canvasRoot.transform.SetPositionAndRotation(head.TransformPoint(new Vector3(0,-.12f,distance)),head.rotation);
            canvasRoot.transform.localScale=Vector3.one*.00105f;canvasRoot.SetActive(true);
        }
        void LateUpdate()
        {
            if(!folder||!folder.gameObject.activeSelf)return;
            Place();
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
