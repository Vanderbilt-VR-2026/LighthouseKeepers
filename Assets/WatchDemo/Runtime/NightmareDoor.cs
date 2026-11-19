using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
    // A real grip/pull arc. Only the authoritative, reach-checked progress opens its blocker.
    public sealed class NightmareDoor:MonoBehaviour
    {
        WatchDemoController watch;int index;Transform leaf;BoxCollider blocker;XRSimpleInteractable handle;
        IXRSelectInteractor hand;Vector3 start;float initial,nextSend;bool active;
        Material material;readonly List<Renderer> originals=new();readonly List<Collider> originalColliders=new();
        public Component Handle=>handle;
        public static NightmareDoor Create(WatchDemoController w,int i,Vector3 hinge)
        {var g=new GameObject("Nightmare pull door "+i);g.transform.SetParent(w.transform,false);g.transform.position=hinge;var d=g.AddComponent<NightmareDoor>();d.watch=w;d.index=i;d.Build();return d;}
        void Build()
        {
            material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetColor("_BaseColor",new Color(.09f,.12f,.10f));material.SetFloat("_Smoothness",.25f);
            leaf=new GameObject("Hinged leaf").transform;leaf.SetParent(transform,false);
            var panel=GameObject.CreatePrimitive(PrimitiveType.Cube);panel.name="Solid weathered door";panel.transform.SetParent(leaf,false);panel.transform.localPosition=new Vector3(.55f,1.03f,0);panel.transform.localScale=new Vector3(1.1f,2.06f,.055f);panel.GetComponent<Renderer>().sharedMaterial=material;blocker=panel.GetComponent<BoxCollider>();
            var grip=new GameObject("GRIP and pull door handle");grip.transform.SetParent(leaf,false);grip.transform.localPosition=new Vector3(.93f,1.02f,-.08f);
            var col=grip.AddComponent<SphereCollider>();col.radius=.13f;col.isTrigger=true;var rb=grip.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
            handle=grip.AddComponent<XRSimpleInteractable>();handle.colliders.Add(col);handle.selectEntered.AddListener(e=>{if(e.interactorObject is XRDirectInteractor){hand=e.interactorObject;start=hand.transform.position;initial=watch.State.Scare.Door(index);}else e.manager.SelectExit(e.interactorObject,e.interactableObject);});handle.selectExited.AddListener(_=>hand=null);
            var bar=GameObject.CreatePrimitive(PrimitiveType.Cube);bar.transform.SetParent(grip.transform,false);bar.transform.localScale=new Vector3(.03f,.19f,.04f);Object.Destroy(bar.GetComponent<Collider>());bar.GetComponent<Renderer>().sharedMaterial=material;
            leaf.gameObject.SetActive(false);
        }
        void Update()
        {
            var s=watch.State.Scare;bool wanted=s.Beat==ScareBeat.Escape;
            if(active!=wanted){active=wanted;leaf.gameObject.SetActive(active);if(active){string key=index==0?"SM_Door_Steel":"SM_Door_Green";foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(r.name==key&&r.enabled){originals.Add(r);r.enabled=false;}foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(c.name.StartsWith("UCX_"+key)&&c.enabled){originalColliders.Add(c);c.enabled=false;}}else Restore();}
            if(!active)return;
            float angle=s.Door(index);leaf.localRotation=Quaternion.Euler(0,angle*95,0);blocker.enabled=angle<.72f;
            if(hand!=null&&Time.unscaledTime>=nextSend){nextSend=Time.unscaledTime+.08f;float pull=Vector3.Distance(hand.transform.position,start)/(index==1?.60f:.42f);float value=index*2+Mathf.Clamp01(initial+pull);if(watch.State.Coop!=null)watch.State.Coop.Send(CoopAction.ScareDoor,value,true);else watch.State.Scare.PullDoor(value);}
        }
        void Restore(){foreach(var r in originals)if(r)r.enabled=true;originals.Clear();foreach(var c in originalColliders)if(c)c.enabled=true;originalColliders.Clear();hand=null;}
        void OnDestroy(){Restore();if(material)Destroy(material);}
    }
    public sealed partial class WatchDemoController
    {
        public void NightmareCheckpoint()
        {
            // Only an explicit Retry/crew-change checkpoint moves the rig, never a scare.
            var rig=FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();if(!rig)return;
            var cc=rig.GetComponent<CharacterController>();bool enabled=cc&&cc.enabled;if(cc)cc.enabled=false;
            var p=HorrorTuning.Current.nightmareStart;rig.transform.position+=new Vector3(p.x-head.position.x,p.y-rig.transform.position.y,p.z-head.position.z);
            if(cc)cc.enabled=enabled;Physics.SyncTransforms();
        }
    }
}
