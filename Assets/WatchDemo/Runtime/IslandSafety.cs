using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class IslandSafety : MonoBehaviour
    {
        WatchDemoController watch;XROrigin rig;Mesh mesh;int serial,revision=-1;float recoveryFade;bool recovering;
        public static void Create(WatchDemoController w){var s=w.gameObject.AddComponent<IslandSafety>();s.watch=w;s.rig=FindAnyObjectByType<XROrigin>();s.Build();}
        public static Vector3 Edge(float angle)=>new Vector3(-.2f+6.9f*Mathf.Sin(angle),0,-3.2f+12.2f*Mathf.Cos(angle));
        void Build()
        {
            var root=new GameObject("Island safety rail perimeter");root.transform.SetParent(transform,false);
            var parts=root.AddComponent<M2StationParts>();parts.Initialize();var combine=new List<CombineInstance>();
            for(int i=0;i<80;i++){
                var a=Edge(i*Mathf.PI*2/80);var b=Edge((i+1)*Mathf.PI*2/80);var mid=(a+b)*.5f;float ground=-.15f;
                foreach(var hit in Physics.RaycastAll(mid+Vector3.up*2,Vector3.down,8))if(hit.collider.name.StartsWith("UCX_SM_Rock")&&hit.point.y<.5f)ground=Mathf.Max(ground,hit.point.y);
                var t=M2StationParts.Root("Island edge rail "+i,root.transform,mid+Vector3.up*(ground+.65f));t.rotation=Quaternion.LookRotation(b-a);
                // Tall below ground too: blocks stepping down rock ledges under the rail.
                var c=t.gameObject.AddComponent<BoxCollider>();c.center=Vector3.down*2;c.size=new Vector3(.12f,5.4f,Vector3.Distance(a,b)+.12f);
                foreach(float y in new[]{-.35f,.5f})Add(parts,t,new Vector3(0,y,0),new Vector3(.055f,.055f,Vector3.Distance(a,b)+.1f),combine);
                Add(parts,t,Vector3.zero,new Vector3(.065f,1.3f,.065f),combine);
            }
            mesh=new Mesh{name="Combined island guard rails"};mesh.CombineMeshes(combine.ToArray());var rails=new GameObject("Weathered island rails");rails.transform.SetParent(root.transform,false);rails.AddComponent<MeshFilter>().sharedMesh=mesh;var r=rails.AddComponent<MeshRenderer>();r.sharedMaterial=parts.Dark;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            Physics.SyncTransforms();
        }
        void Add(M2StationParts p,Transform t,Vector3 pos,Vector3 scale,List<CombineInstance> pieces){var o=p.Part("Rail source",t,pos,scale,p.Dark);pieces.Add(new CombineInstance{mesh=o.GetComponent<MeshFilter>().sharedMesh,transform=o.transform.localToWorldMatrix});o.SetActive(false);Destroy(o);}
        void LateUpdate()
        {
            if(!rig||!rig.Camera)return;var state=watch.State;var locations=state.Coop as ICoopKeeperLocations;int actor=locations?.LocalActor??0;
            if(revision!=watch.Revision){revision=watch.Revision;serial=0;}
            if(!state.ReadOnly&&!state.Failed){
                int a=actor,b=0;Vector3 p=rig.Camera.transform.position,q=default;
                if(locations!=null){locations.Locations(out a,out p,out b,out q);if(a==0){a=actor;p=rig.Camera.transform.position;}}
                state.ObserveSea(a,p,b,q,Time.deltaTime);
            }
            var slot=state.Sea.For(actor);bool active=!state.Failed&&slot!=null&&slot.remaining>0;
            if(active&&!recovering){foreach(var hand in FindObjectsByType<LighthouseKeepers.Gameplay.Interaction.HandRig>(FindObjectsSortMode.None)){
                var interactor=hand.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
                if(interactor)while(interactor.interactablesSelected.Count>0)interactor.interactionManager.SelectExit(interactor,interactor.interactablesSelected[0]);
            }}
            if(slot!=null&&slot.serial!=serial){serial=slot.serial;ReturnToHouse();}
            recovering=active;recoveryFade=Mathf.MoveTowards(recoveryFade,active?1:0,Time.deltaTime*2);
            if(active||recoveryFade>0)watch.ShowSeaRecovery(recoveryFade,active);
            else if(!state.Failed)watch.HideSeaRecovery();
        }
        public void ReturnToHouse(){var c=rig.GetComponent<CharacterController>();bool on=c&&c.enabled;if(c)c.enabled=false;var head=rig.Camera.transform.position;var delta=new Vector3(0,.04f,-6.3f)-rig.transform.position;delta.x=-head.x;delta.z=-6.3f-head.z;rig.transform.position+=delta;if(c)c.enabled=on;Physics.SyncTransforms();}
        void OnDestroy(){if(mesh)Destroy(mesh);}
    }
    public sealed partial class WatchDemoController
    {
        bool seaPage;
        public void ShowSeaRecovery(float value,bool active){SetFade(value);if(logbook)logbook.Recovery(active,head);seaPage=true;}
        public void HideSeaRecovery(){if(!seaPage)return;seaPage=false;SetFade(0);if(logbook)logbook.Recovery(false,head);}
    }
}
