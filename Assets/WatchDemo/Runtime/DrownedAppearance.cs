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
        WatchDemoController watch;Transform figure;Mesh mesh;Material material;MeshRenderer body;MaterialPropertyBlock block;
        readonly Vector3[] places=new Vector3[4]; readonly bool[] supported=new bool[4];
        int revision=-1;float visibility;
        public bool SafeAnchor { get; private set; }
        public const int MaximumTriangles=3000;
        public static void Create(WatchDemoController w){if(w.State.Drowned==null)return;var p=w.gameObject.AddComponent<DrownedAppearance>();p.watch=w;p.Build();}
        public static bool Sightings {get=>PlayerPrefs.GetInt("LK.DrownedSightings",1)!=0;set{PlayerPrefs.SetInt("LK.DrownedSightings",value?1:0);PlayerPrefs.Save();}}
        void Build(){
            block=new MaterialPropertyBlock();mesh=BuildMesh();material=new Material(Resources.Load<Shader>("S5Drowned")){name="Salt-wet oilskin"};
            figure=new GameObject("The Drowned — oilskin saboteur").transform;figure.SetParent(transform,false);figure.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;body=figure.gameObject.AddComponent<MeshRenderer>();body.sharedMaterial=material;body.shadowCastingMode=ShadowCastingMode.Off;body.receiveShadows=false;body.enabled=false;
            DrownedHorror.Create(watch,this,figure);
            var anchors=new[]{FindAnyObjectByType<WatchPipeRepair>().transform,FindAnyObjectByType<WatchGenerator>().transform,FindAnyObjectByType<WatchRadioConsole>().transform,FindAnyObjectByType<WatchBeacon>().transform};
            Physics.SyncTransforms();
            for(int i=0;i<4;i++){
                float y=i==0?0:i==1?3.3f:9.9f;
                var radial=new Vector3(anchors[i].position.x,0,anchors[i].position.z).normalized;
                if(radial==Vector3.zero)radial=Vector3.right;
                for(int n=0;n<16;n++){
                    float angle=(35+n/2*12)*(n%2==0?1:-1);
                    var p=Quaternion.Euler(0,angle,0)*radial*(i>=2?2.2f:3.1f);p.y=y;
                    if(Physics.Raycast(p+Vector3.up*.8f,Vector3.down,out var hit,1.2f,~0,QueryTriggerInteraction.Ignore)&&hit.normal.y>.9f&&Mathf.Abs(hit.point.y-y)<.45f){
                        p.y=hit.point.y+.015f;
                        if(!Physics.CheckCapsule(p+Vector3.up*.35f,p+Vector3.up*1.65f,.23f,~0,QueryTriggerInteraction.Ignore)){places[i]=p;supported[i]=true;break;}
                    }
                }
            }

        }
        int Station(){var d=watch.State.Drowned;if(d==null||d.Fault<0)return -1;string id=watch.State.Scenario.faults[d.Fault].stationId;return id==WatchState.PipeStation?0:id==WatchState.GeneratorStation?1:id==WatchState.RadioStation?2:3;}
        void Update(){
            var s=watch.State;var d=s.Drowned;if(d==null)return;
            if(revision!=watch.Revision){revision=watch.Revision;visibility=0;}
            int station=Station();SafeAnchor=station>=0&&supported[station];
            if(station>=0&&supported[station]){figure.position=places[station];var inward=new Vector3(-figure.position.x,0,-figure.position.z);figure.rotation=Quaternion.LookRotation(inward.sqrMagnitude>.01f?inward:Vector3.forward);}
            if(Camera.main&&(Camera.main.transform.position-figure.position).sqrMagnitude<2.25f)SafeAnchor=false; // no face pop at arm's length
            bool active=Sightings&&SafeAnchor&&!watch.Sleeping&&!s.Failed&&(d.Phase==DrownedPhase.Manifest||d.Phase==DrownedPhase.Retreat);
            float wanted=active&&d.Phase==DrownedPhase.Manifest?1:0;visibility=Mathf.MoveTowards(visibility,wanted,Time.deltaTime*(wanted>0?1.4f:.5f));
            body.enabled=visibility>.005f;block.SetFloat("_Visibility",visibility);block.SetFloat("_Breath",(float)s.Elapsed);body.SetPropertyBlock(block);
        }
        public static Mesh BuildMesh(){
            var v=new List<Vector3>();var colours=new List<Color>();var t=new List<int>();
            Color cloth=new(.014f,.023f,.020f),seam=new(.025f,.035f,.027f),skin=new(.065f,.085f,.08f),black=new(.004f,.008f,.009f);
            void Tri(Vector3 a,Vector3 b,Vector3 c,Color col){int k=v.Count;v.AddRange(new[]{a,b,c});colours.AddRange(new[]{col,col,col});t.AddRange(new[]{k,k+1,k+2});}
            void RingBody(Vector3 offset,float[] ys,float[] xs,float[] zs,Color col,int n=16){for(int j=0;j<ys.Length-1;j++)for(int k=0;k<n;k++){
                Vector3 P(int row,int seg){float a=seg*Mathf.PI*2/n;float folds=1+Mathf.Sin(a*7+row*.4f)*.035f;return offset+new Vector3(Mathf.Sin(a)*xs[row]*folds,ys[row],Mathf.Cos(a)*zs[row]*folds);}
                var a=P(j,k);var b=P(j,k+1);var c=P(j+1,k+1);var d=P(j+1,k);Tri(a,b,c,col);Tri(a,c,d,col);
            }}
            RingBody(Vector3.zero,new[]{.24f,.32f,.65f,1f,1.25f,1.43f,1.50f},new[]{.31f,.32f,.28f,.23f,.29f,.34f,.16f},new[]{.20f,.21f,.18f,.16f,.19f,.20f,.12f},cloth);
            for(int side=-1;side<=1;side+=2){RingBody(new Vector3(side*.33f,0,.015f),new[]{.65f,.75f,1.03f,1.28f,1.42f},new[]{.045f,.07f,.075f,.095f,.06f},new[]{.055f,.075f,.085f,.105f,.06f},cloth,12);RingBody(new Vector3(side*.33f,0,.04f),new[]{.55f,.64f,.73f},new[]{.025f,.045f,.03f},new[]{.055f,.055f,.03f},skin,8);RingBody(new Vector3(side*.12f,0,.04f),new[]{.015f,.09f,.25f,.31f},new[]{.075f,.085f,.065f,.065f},new[]{.17f,.17f,.07f,.07f},black,12);}
            // Hood is open at the front. Its deep black face sits behind the rolled rim.
            for(int k=0;k<24;k++){float a=k*Mathf.PI*2/24,b=(k+1)*Mathf.PI*2/24;
                Vector3 P(float angle,float scale,float z)=>new(Mathf.Sin(angle)*.17f*scale,1.67f+Mathf.Cos(angle)*.23f*scale,z);
                Tri(P(a,1,.09f),P(b,1,.09f),P(b,.77f,.13f),cloth);Tri(P(a,1,.09f),P(b,.77f,.13f),P(a,.77f,.13f),cloth);
                Tri(P(a,1,.09f),new Vector3(0,1.72f,-.16f),P(b,1,.09f),cloth);Tri(new Vector3(0,1.66f,.035f),P(a,.77f,.035f),P(b,.77f,.035f),black);
            }
            // Front storm flap and buckles are mesh detail, not glowing eyes/extra lights.
            for(int j=0;j<4;j++){float y=.67f+j*.17f;Tri(new Vector3(-.025f,y,.193f),new Vector3(.025f,y,.193f),new Vector3(.025f,y+.025f,.193f),seam);}
            var mesh=new Mesh{name="Original Drowned oilskin silhouette"};mesh.SetVertices(v);mesh.SetColors(colours);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
    }
}
