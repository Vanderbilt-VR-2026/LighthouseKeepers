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
        readonly Vector3[] places=new Vector3[4]; readonly bool[] supported=new bool[4];
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
            var anchors=new[]{FindAnyObjectByType<WatchPipeRepair>().transform,FindAnyObjectByType<WatchGenerator>().transform,FindAnyObjectByType<WatchRadioConsole>().transform,FindAnyObjectByType<WatchBeacon>().transform};
            Physics.SyncTransforms();
            for(int i=0;i<4;i++){
                float y=i==0?0:i==1?3.3f:9.9f;
                var radial=new Vector3(anchors[i].position.x,0,anchors[i].position.z).normalized;
                if(radial==Vector3.zero)radial=Vector3.right;
                for(int n=0;n<32;n++){
                    float angle=(35+n/2*12)*(n%2==0?1:-1);
                    var p=Quaternion.Euler(0,angle,0)*radial*(i>=2?2.2f:3.1f);p.y=y;
                    if(Physics.Raycast(p+Vector3.up*.8f,Vector3.down,out var hit,1.2f,~0,QueryTriggerInteraction.Ignore)&&hit.normal.y>.9f&&Mathf.Abs(hit.point.y-y)<.45f){
                        p.y=hit.point.y+.015f;
                        if(!Physics.CheckCapsule(p+Vector3.up*.35f,p+Vector3.up*1.9f,.35f,~0,QueryTriggerInteraction.Ignore)){places[i]=p;supported[i]=true;break;}
                    }
                }
            }

        }
        int Station(){var d=watch.State.Drowned;if(d==null||d.Fault<0)return -1;string id=watch.State.Scenario.faults[d.Fault].stationId;return id==WatchState.PipeStation?0:id==WatchState.GeneratorStation?1:id==WatchState.RadioStation?2:3;}
        void Update(){
            var s=watch.State;var d=s.Drowned;if(d==null)return;
            if(revision!=watch.Revision||s.Elapsed<lastElapsed-.01){revision=watch.Revision;visibility=0;manifestAt=-1;}
            lastElapsed=s.Elapsed;if(d.Phase==DrownedPhase.Manifest)manifestAt=d.Since;
            int station=Station();SafeAnchor=station>=0&&supported[station];
            if(station>=0&&supported[station]){figure.position=places[station];var inward=new Vector3(-figure.position.x,0,-figure.position.z);figure.rotation=Quaternion.LookRotation(inward.sqrMagnitude>.01f?inward:Vector3.forward);}
            if(Camera.main&&!HasComfortClearance(Camera.main.transform.position,figure.position))SafeAnchor=false; // no face pop at arm's length
            bool active=Sightings&&SafeAnchor&&!watch.Sleeping&&!s.Failed&&(d.Phase==DrownedPhase.Manifest||d.Phase==DrownedPhase.Retreat);
            float wanted=active&&d.Phase==DrownedPhase.Manifest?1:0;visibility=(!Sightings||!SafeAnchor||watch.Sleeping||s.Failed)?0:Mathf.MoveTowards(visibility,wanted,Time.deltaTime*(wanted>0?1.1f:.5f));
            body.enabled=visibility>.005f;block.SetFloat("_Visibility",visibility);block.SetFloat("_Breath",(float)s.Elapsed);block.SetFloat("_Arrival",(float)System.Math.Max(0,s.Elapsed-manifestAt));body.SetPropertyBlock(block);
            mist.enabled=body.enabled;mistBlock.SetFloat("_Visibility",visibility);mistBlock.SetFloat("_Clock",(float)s.Elapsed);mist.SetPropertyBlock(mistBlock);
        }
        public static Mesh BuildMesh()=>DrownedSculpt.Body();
        void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);if(mistMesh)Destroy(mistMesh);if(mistMaterial)Destroy(mistMaterial);}
    }
}
