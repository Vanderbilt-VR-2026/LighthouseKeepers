using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    // Original oilskin silhouette; one material/mesh, no light, collider, camera effect or chase.
    public sealed class DrownedAppearance:MonoBehaviour
    {
        WatchDemoController watch;Transform figure;Mesh mesh,mistMesh;Material material,mistMaterial;MeshRenderer body,mist;MaterialPropertyBlock block,mistBlock;
        DrownedPlacement placement;float nextProbe;
        int revision=-1;float visibility;double manifestAt=-1,lastElapsed;
        public bool SafeAnchor { get; private set; }
        public const int MaximumTriangles=3000;
        public static void Create(WatchDemoController w){if(w.State.Drowned==null)return;var p=w.gameObject.AddComponent<DrownedAppearance>();p.watch=w;p.Build();}
        public static bool Sightings {get=>PlayerPrefs.GetInt("LK.DrownedSightings",1)!=0;set{PlayerPrefs.SetInt("LK.DrownedSightings",value?1:0);PlayerPrefs.Save();}}
        public static bool HasComfortClearance(Vector3 viewerHead,Vector3 figureFeet) => (viewerHead-(figureFeet+Vector3.up*1.6f)).sqrMagnitude>=2.25f;
        void Build(){
            block=new MaterialPropertyBlock();mesh=BuildMesh();material=new Material(Resources.Load<Shader>("S5Drowned")){name="Salt-wet oilskin"};
            figure=new GameObject("The Drowned — oilskin saboteur").transform;figure.SetParent(transform,false);figure.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;body=figure.gameObject.AddComponent<MeshRenderer>();body.sharedMaterial=material;body.shadowCastingMode=ShadowCastingMode.Off;body.receiveShadows=false;body.enabled=false;
            DrownedHorror.Create(watch,this,figure);
            var fog=new GameObject("Drowned local mist and dripping coat");fog.transform.SetParent(figure,false);
            mistMesh=DrownedSculpt.Effects();fog.AddComponent<MeshFilter>().sharedMesh=mistMesh;
            mistMaterial=new Material(Resources.Load<Shader>("S5DrownedMist")){name="Bounded cold breath and runoff"};
            mist=fog.AddComponent<MeshRenderer>();mist.sharedMaterial=mistMaterial;mist.shadowCastingMode=ShadowCastingMode.Off;mist.receiveShadows=false;mist.enabled=false;mistBlock=new MaterialPropertyBlock();
            var rig=Camera.main?Camera.main.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>():null;
            placement=new DrownedPlacement(figure,rig?rig.transform:null);watch.State.DrownedViewReady=false;
        }
        void PositionReveal(){
            var s=watch.State;var d=s.Drowned;
            if(s.ReadOnly||d.Phase!=DrownedPhase.Warning)return;
            watch.DrownedViewers(out int a,out var p,out var f,out int b,out var q,out var g);
            bool second=false;
            if(b!=0){var id=s.Scenario.faults[d.Fault].stationId;var point=id==WatchState.GeneratorStation?FindAnyObjectByType<WatchGenerator>().transform.position+Vector3.up*1.5f:id==WatchState.PipeStation?FindAnyObjectByType<WatchPipeRepair>().transform.position+Vector3.up:FindAnyObjectByType<WatchBeacon>().transform.position;
                second=(q-point).sqrMagnitude<(p-point).sqrMagnitude;}
            int actor=second?b:a;var head=second?q:p;var forward=second?g:f;var other=second?p:q;
            var feet=new Vector3(d.X,d.Y,d.Z);
            s.DrownedViewReady=d.Anchored&&d.Actor==actor&&placement.Visible(head,forward,feet);
            if(d.Anchored&&(head-feet).sqrMagnitude<64&&placement.ClearSight(head,feet+Vector3.up*1.65f))return;
            if(Time.unscaledTime<nextProbe)return;nextProbe=Time.unscaledTime+.35f;
            if(placement.Find(head,forward,other,b!=0,out feet)){
                var toward=Vector3.ProjectOnPlane(head-feet,Vector3.up);float yaw=Quaternion.LookRotation(toward).eulerAngles.y;
                d.SetAnchor(actor,feet.x,feet.y,feet.z,yaw,s.Elapsed);s.DrownedViewReady=true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"DROWNED_REVEAL_ANCHOR actor={actor} feet={feet} view={head} phase={d.Phase}");
#endif
            }
        }
        void Update(){
            var s=watch.State;var d=s.Drowned;if(d==null)return;
            if(revision!=watch.Revision||s.Elapsed<lastElapsed-.01){revision=watch.Revision;visibility=0;manifestAt=-1;nextProbe=0;if(!s.ReadOnly)s.DrownedViewReady=false;}
            lastElapsed=s.Elapsed;PositionReveal();if(d.Phase==DrownedPhase.Manifest)manifestAt=d.RevealAt;
            SafeAnchor=d.Anchored;
            if(d.Anchored)figure.SetPositionAndRotation(new Vector3(d.X,d.Y,d.Z),Quaternion.Euler(0,d.Yaw,0));
            if(Camera.main&&!HasComfortClearance(Camera.main.transform.position,figure.position))SafeAnchor=false;
            bool active=Sightings&&SafeAnchor&&!watch.Sleeping&&!s.Failed&&(d.Phase==DrownedPhase.Manifest||d.Phase==DrownedPhase.Retreat);
            float wanted=active&&d.Phase==DrownedPhase.Manifest?1:0;
            visibility=(!Sightings||!SafeAnchor||watch.Sleeping||s.Failed)?0:Mathf.MoveTowards(visibility,wanted,Time.deltaTime*(wanted>0?12f:.6f));
            body.enabled=visibility>.005f;block.SetFloat("_Visibility",visibility);block.SetFloat("_Breath",(float)s.Elapsed);block.SetFloat("_Arrival",(float)System.Math.Max(0,s.Elapsed-manifestAt));body.SetPropertyBlock(block);
            mist.enabled=body.enabled;mistBlock.SetFloat("_Visibility",visibility);mistBlock.SetFloat("_Clock",(float)s.Elapsed);mist.SetPropertyBlock(mistBlock);
        }
        public static Mesh BuildMesh()=>DrownedSculpt.Body();
        void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);if(mistMesh)Destroy(mistMesh);if(mistMaterial)Destroy(mistMaterial);}
    }
}
