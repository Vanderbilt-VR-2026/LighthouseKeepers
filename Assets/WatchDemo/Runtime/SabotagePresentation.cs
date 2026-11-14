using System;
using LighthouseKeepers.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    // Receipts from host watch time drive both clients. No local random fault decisions.
    public sealed class SabotagePresentation:MonoBehaviour
    {
        WatchDemoController watch; Material material; Mesh[] meshes; MeshRenderer[] marks;
        AudioSource[] sound;AudioClip[] warnings;double[] heard;int revision=-1;
        MaterialPropertyBlock block;
        static readonly string[] jobs={WatchState.PipeStation,WatchState.GeneratorStation,WatchState.RadioStation,WatchState.BeaconStation};
        public static void Create(WatchDemoController watch){if(watch.State.Sabotage==null)return;var p=watch.gameObject.AddComponent<SabotagePresentation>();p.watch=watch;p.Build();}
        void Build()
        {
            block=new MaterialPropertyBlock();
            material=new Material(Resources.Load<Shader>("S5Evidence")){name="Wet sabotage traces"};
            marks=new MeshRenderer[4];meshes=new Mesh[4];sound=new AudioSource[4];warnings=new AudioClip[4];heard=new double[30];Array.Fill(heard,-1);
            var pipe=FindAnyObjectByType<WatchPipeRepair>();var gen=FindAnyObjectByType<WatchGenerator>();var radio=FindAnyObjectByType<WatchRadioConsole>();var beacon=FindAnyObjectByType<WatchBeacon>();
            var anchors=new[]{pipe.transform,gen.transform,radio.transform,beacon.transform};
            string[] clips={"tear","fuel","wire","smear"};
            for(int i=0;i<4;i++){
                var go=new GameObject("Sabotage evidence "+jobs[i]);go.transform.SetParent(transform,false);go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
                var art=new TraceMesh();
                if(i==0){
                    var from=pipe.ActivePosition;from.y=.025f;var door=new Vector3(0,.025f,-3.3f);var forward=(door-from).normalized;var right=Vector3.Cross(Vector3.up,forward);
                    for(int n=0;n<7;n++){var p=Vector3.Lerp(from,door,(n+.5f)/7)+right*(n%2==0?-.10f:.10f);art.Foot(p,Quaternion.LookRotation(forward),.85f);}
                    for(int n=0;n<3;n++)art.Streak(pipe.ActivePosition+pipe.transform.right*((n-1)*.035f)+Vector3.down*.10f,pipe.transform.rotation,.006f,.12f);
                }else if(i==1){
                    art.TankHand(gen.transform);
                    art.Oval(gen.transform.TransformPoint(new Vector3(.2f,.025f,1.35f)),Quaternion.Euler(90,0,0),.32f,.17f);
                }else if(i==2){
                    art.Hand(radio.transform.TransformPoint(new Vector3(.60f,.85f,.10f)),radio.transform.rotation,.65f);
                    for(int n=0;n<4;n++)art.Streak(radio.transform.TransformPoint(new Vector3(.54f+n*.035f,.68f,.105f)),radio.transform.rotation,.008f,.24f);
                }else art.Hand(beacon.transform.TransformPoint(new Vector3(0,1.35f,.455f)),beacon.transform.rotation,1.2f);
                meshes[i]=art.Finish("Wet marks "+jobs[i]);go.AddComponent<MeshFilter>().sharedMesh=meshes[i];marks[i]=go.AddComponent<MeshRenderer>();marks[i].sharedMaterial=material;marks[i].shadowCastingMode=ShadowCastingMode.Off;marks[i].receiveShadows=false;marks[i].enabled=false;
                var a=new GameObject("Sabotage presound "+jobs[i]);a.transform.SetParent(anchors[i],false);a.transform.localPosition=Vector3.up;
                sound[i]=a.AddComponent<AudioSource>();sound[i].playOnAwake=false;sound[i].spatialBlend=1;sound[i].minDistance=1;sound[i].maxDistance=16;sound[i].rolloffMode=AudioRolloffMode.Logarithmic;sound[i].volume=.35f;sound[i].priority=80;KeeperAudioMix.Route(sound[i],"SFX_Critical");warnings[i]=Resources.Load<AudioClip>("S5Audio/sabotage_"+clips[i]);
            }
        }
        void Update()
        {
            var s=watch.State;if(s.Sabotage==null)return;
            if(revision!=watch.Revision){revision=watch.Revision;Array.Fill(heard,-1);foreach(var a in sound)a.Stop();}
            for(int j=0;j<jobs.Length;j++){
                float alpha=0;
                for(int i=0;i<s.Sabotage.Count;i++)if(s.Scenario.faults[i].stationId==jobs[j]){
                    alpha=Mathf.Max(alpha,s.Sabotage.Evidence(i,s.Elapsed));double at=s.Sabotage.Warning(i);
                    if(at>=0&&at!=heard[i]){
                        heard[i]=at;
                        // Late join shows evidence but does not replay a past warning.
                        if(!s.Failed&&s.Elapsed-at<=1.5&&warnings[j]){sound[j].clip=warnings[j];sound[j].Play();}
                    }
                }
                marks[j].enabled=alpha>.005f;block.SetColor("_Tint",new Color(.025f,.05f,.055f,alpha*.82f));marks[j].SetPropertyBlock(block);
            }
        }
        void OnDestroy(){if(material)Destroy(material);if(meshes!=null)foreach(var m in meshes)if(m)Destroy(m);}
        sealed class TraceMesh
        {
            readonly List<Vector3> v=new();readonly List<int> t=new();
            public void Oval(Vector3 p,Quaternion rotation,float x,float y){int k=v.Count;v.Add(p);for(int n=0;n<12;n++){float a=n*Mathf.PI/6;v.Add(p+rotation*new Vector3(Mathf.Cos(a)*x,Mathf.Sin(a)*y,0));}for(int n=0;n<12;n++){t.Add(k);t.Add(k+1+n);t.Add(k+1+(n+1)%12);}}
            public void TankHand(Transform tank){int start=v.Count;Hand(new Vector3(0,1.50f,.4f),Quaternion.identity,.7f);for(int i=start;i<v.Count;i++){var p=v[i];float dy=p.y-1.53f;p.z=.20f+Mathf.Sqrt(Mathf.Max(.001f,.19f*.19f-dy*dy))+.004f;v[i]=tank.TransformPoint(p);}}
            public void Streak(Vector3 p,Quaternion q,float width,float length)=>Oval(p,q,width,length*.5f);
            public void Foot(Vector3 p,Quaternion q,float scale){q*=Quaternion.Euler(90,0,0);Oval(p,q,.055f*scale,.10f*scale);Oval(p+q*new Vector3(0,-.13f*scale,0),q,.040f*scale,.04f*scale);}
            public void Hand(Vector3 p,Quaternion q,float scale){Oval(p,q,.065f*scale,.08f*scale);for(int i=0;i<4;i++)Oval(p+q*new Vector3((i-1.5f)*.032f*scale,.11f*scale,0),q,.012f*scale,(i==0||i==3?.05f:.07f)*scale);Oval(p+q*new Vector3(-.08f*scale,.015f*scale,0),q*Quaternion.Euler(0,0,30),.022f*scale,.05f*scale);}
            public Mesh Finish(string name){var m=new Mesh{name=name};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
        }
    }
}
