using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    // Three original forms share one encounter, one material and one active body renderer.
    public sealed class DrownedAppearance:MonoBehaviour
    {
        WatchDemoController watch;Transform figure;Mesh mesh,mistMesh;Mesh[] forms;MeshFilter bodyFilter;DrownedForm activeForm=(DrownedForm)(-1);Material material,mistMaterial;MeshRenderer body,mist;MaterialPropertyBlock block,mistBlock;
        DrownedPlacement placement;float nextProbe;
        int revision=-1;float visibility;double manifestAt=-1,lastElapsed;
        public bool SafeAnchor { get; private set; }
        public const int MaximumTriangles=DrownedForms.TriangleBudget;
        public DrownedForm Form=>activeForm;
        public static void Create(WatchDemoController w){if(w.State.Drowned==null)return;var p=w.gameObject.AddComponent<DrownedAppearance>();p.watch=w;p.Build();}
        public static bool Sightings {get=>PlayerPrefs.GetInt("LK.DrownedSightings",1)!=0;set{PlayerPrefs.SetInt("LK.DrownedSightings",value?1:0);PlayerPrefs.Save();}}
        public static bool HasComfortClearance(Vector3 viewerHead,Vector3 figureFeet) => (viewerHead-(figureFeet+Vector3.up*1.6f)).sqrMagnitude>=2.25f;
        void Build(){
            block=new MaterialPropertyBlock();forms=new[]{DrownedForms.Build(DrownedForm.SeaHag),DrownedForms.Build(DrownedForm.Doll),DrownedForms.Build(DrownedForm.Gaunt)};mesh=forms[0];material=new Material(Resources.Load<Shader>("S5Drowned")){name="Salt-wet Drowned skin cloth and hair"};
            figure=new GameObject("The Drowned — three forms").transform;figure.SetParent(transform,false);bodyFilter=figure.gameObject.AddComponent<MeshFilter>();bodyFilter.sharedMesh=mesh;body=figure.gameObject.AddComponent<MeshRenderer>();body.sharedMaterial=material;body.shadowCastingMode=ShadowCastingMode.Off;body.receiveShadows=false;body.enabled=false;
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
            s.DrownedViewReady=d.Anchored&&d.Actor==actor&&(d.PoseKind==2?placement.ClearSight(head,DrownedMotion.Centre(d,s.Elapsed,DrownedForms.Height(activeForm))):placement.Visible(head,forward,feet,DrownedForms.FaceHeight(activeForm)));
            if(d.Anchored&&s.DrownedViewReady)return;
            if(Time.unscaledTime<nextProbe)return;nextProbe=Time.unscaledTime+.35f;
            int style=s.Sprint5.groundedHorror?DrownedMotion.Style(s.Day,d.Fault):1;
            if(style==2&&placement.Ceiling(head,forward,other,b!=0,out var ceiling,out float ceilingYaw)){d.SetAnchor(actor,ceiling.x,ceiling.y,ceiling.z,ceilingYaw,s.Elapsed,2);s.DrownedViewReady=true;return;}
            if(placement.Find(head,forward,other,b!=0,out feet,DrownedForms.FaceHeight(activeForm),DrownedForms.Height(activeForm)+.10f)){
                var toward=Vector3.ProjectOnPlane(head-feet,Vector3.up);float yaw=Quaternion.LookRotation(toward).eulerAngles.y;
                d.SetAnchor(actor,feet.x,feet.y,feet.z,yaw,s.Elapsed,style==2?1:style);s.DrownedViewReady=true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"DROWNED_REVEAL_ANCHOR actor={actor} feet={feet} view={head} phase={d.Phase}");
#endif
            }
        }
        void Update(){
            var s=watch.State;var d=s.Drowned;if(d==null)return;
            if(revision!=watch.Revision||s.Elapsed<lastElapsed-.01){revision=watch.Revision;visibility=0;manifestAt=-1;nextProbe=0;if(!s.ReadOnly)s.DrownedViewReady=false;}
            lastElapsed=s.Elapsed;var form=DrownedForms.Select(s.Day,d.Fault);if(form!=activeForm){activeForm=form;mesh=forms[(int)form];bodyFilter.sharedMesh=mesh;body.localBounds=new Bounds(mesh.bounds.center,mesh.bounds.size+Vector3.one*.6f);}PositionReveal();if(d.Phase==DrownedPhase.Manifest)manifestAt=d.RevealAt;
            SafeAnchor=d.Anchored;
            if(d.Anchored){DrownedMotion.Pose(d,s.Elapsed,out var position,out var rotation);figure.SetPositionAndRotation(position,rotation);}
            if(Camera.main){var head=Camera.main.transform.position;float face=DrownedForms.FaceHeight(activeForm);if((head-figure.TransformPoint(Vector3.up*face)).sqrMagnitude<2.25f)SafeAnchor=false;}
            bool active=Sightings&&SafeAnchor&&!watch.Sleeping&&!s.Failed&&!s.Recovery.LessonActive&&(d.Phase==DrownedPhase.Manifest||d.Phase==DrownedPhase.Retreat);
            float wanted=active&&d.Phase==DrownedPhase.Manifest?1:0;
            visibility=(!Sightings||!SafeAnchor||watch.Sleeping||s.Failed||s.Recovery.LessonActive)?0:Mathf.MoveTowards(visibility,wanted,Time.deltaTime*(wanted>0?16f:10f));
            body.enabled=visibility>.005f;block.SetFloat("_Visibility",visibility);block.SetFloat("_Style",d.PoseKind);block.SetFloat("_Height",DrownedForms.Height(activeForm));block.SetFloat("_Breath",(float)s.Elapsed);block.SetFloat("_Arrival",(float)System.Math.Max(0,s.Elapsed-manifestAt));body.SetPropertyBlock(block);
            mist.enabled=body.enabled;mistBlock.SetFloat("_Visibility",visibility);mistBlock.SetFloat("_Clock",(float)s.Elapsed);mistBlock.SetFloat("_Height",DrownedForms.Height(activeForm));mistBlock.SetFloat("_FaceHeight",DrownedForms.FaceHeight(activeForm));mist.SetPropertyBlock(mistBlock);
        }
        public static Mesh BuildMesh()=>DrownedForms.Build(DrownedForm.SeaHag);
        void OnDestroy(){if(forms!=null)foreach(var form in forms)if(form)Destroy(form);if(material)Destroy(material);if(mistMesh)Destroy(mistMesh);if(mistMaterial)Destroy(mistMaterial);}
    }
}
