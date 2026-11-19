using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;

namespace LighthouseKeepers.WatchDemo
{
    // Reusable, bounded world presentation. No camera transform or project-material edits.
    public sealed class ScareKit:System.IDisposable
    {
        readonly GameObject root;readonly Transform body;readonly Mesh mesh;readonly Material material;
        readonly MeshRenderer renderer;readonly MaterialPropertyBlock properties=new();
        readonly AudioSource sound,breath;readonly List<(Renderer renderer,int index)> lightmaps=new();
        bool dark;ScareBeat beat;Vector3 turnBase,wall,ceiling;Quaternion wallRotation;bool turnReady;
        readonly List<(AudioSource source,bool muted)> quiet=new();
        public ScareKit(Transform parent)
        {
            root=new GameObject("Scripted scare kit");root.transform.SetParent(parent,false);
            body=new GameObject("Nightmare child").transform;body.SetParent(root.transform,false);
            mesh=DrownedForms.Build(DrownedForm.Doll);body.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            material=new Material(Resources.Load<Shader>("S5Drowned"));renderer=body.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.enabled=false;
            sound=Source("Scene spatial one-shot",false);breath=Source("Scene wet breathing",true);breath.clip=Resources.Load<AudioClip>("S5Audio/DrownedHorror/breathing");
        }
        AudioSource Source(string name,bool loop)
        {var o=new GameObject(name);o.transform.SetParent(body,false);o.transform.localPosition=Vector3.up;var a=o.AddComponent<AudioSource>();a.playOnAwake=false;a.loop=loop;a.spatialBlend=1;a.minDistance=1;a.maxDistance=14;a.dopplerLevel=0;a.volume=.4f;KeeperAudioMix.Route(a,"SFX_Critical");return a;}
        public void Beat(ScareBeat value,float age)
        {
            beat=value;turnReady=false;sound.Stop();if(age>1)return;
            string clip=value==ScareBeat.Turn?"reveal_b":value==ScareBeat.Warning?"approach":value==ScareBeat.Relief?"retreat":null;
            if(clip!=null){sound.clip=Resources.Load<AudioClip>("S5Audio/DrownedHorror/"+clip);if(sound.clip)sound.Play();}
        }
        public void Present(ScareSequenceState scene,Vector3 eye)
        {
            bool enabled=scene.Active&&scene.Beat<=ScareBeat.Escape&&DrownedAppearance.Sightings;
            body.SetPositionAndRotation(scene.Body,Quaternion.Euler(0,scene.Yaw,0));
            if(scene.Beat==ScareBeat.Turn){
                if(!turnReady){turnReady=true;turnBase=scene.Body;wall=turnBase;ceiling=turnBase;float best=3;
                    for(int i=0;i<8;i++){var direction=Quaternion.Euler(0,i*45,0)*Vector3.forward;if(Physics.Raycast(turnBase+Vector3.up*1.6f,direction,out var h,best,~0,QueryTriggerInteraction.Ignore)&&!h.collider.attachedRigidbody&&Mathf.Abs(h.normal.y)<.1f){best=h.distance;wall=h.point+h.normal*.25f-Vector3.up*.4f;wallRotation=Quaternion.LookRotation(-h.normal,Vector3.up);}}
                    ceiling=wall+Vector3.up*.5f;
                    if(Physics.Raycast(wall+Vector3.up,Vector3.up,out var roof,2,~0,QueryTriggerInteraction.Ignore))ceiling=roof.point+Vector3.down*.2f;
                }
                float t=scene.Age/Mathf.Max(1,HorrorTuning.Current.nightmareTurnSeconds);
                body.position=t<.55f?Vector3.Lerp(turnBase,wall,Mathf.SmoothStep(0,1,t/.55f)):Vector3.Lerp(wall,ceiling,Mathf.SmoothStep(0,1,(t-.55f)/.45f));
                body.rotation=Quaternion.Slerp(wallRotation,wallRotation*Quaternion.Euler(180,0,0),Mathf.SmoothStep(0,1,(t-.55f)/.45f));
            }
            // Brief in-place joint/head distortion, not a camera effect or a collision warp.
            properties.SetFloat("_Walk",scene.Beat==ScareBeat.Follow?.55f:scene.Beat==ScareBeat.Escape?1:0);properties.SetFloat("_HeadTurn",scene.Beat==ScareBeat.Turn?Mathf.SmoothStep(0,2.7f,scene.Age):0);properties.SetFloat("_Visibility",1);properties.SetFloat("_Height",1.28f);properties.SetFloat("_Breath",scene.Clock);properties.SetFloat("_Arrival",scene.Age);properties.SetFloat("_Style",scene.Beat==ScareBeat.Turn?2:0);renderer.SetPropertyBlock(properties);
            renderer.enabled=enabled&&Vector3.Distance(eye,body.position+Vector3.up*1.1f)>=.65f;
            bool breathe=enabled&&(beat==ScareBeat.Warning||beat==ScareBeat.Escape);
            if(breathe&&breath.clip&&!breath.isPlaying)breath.Play();else if(!breathe&&breath.isPlaying)breath.Stop();
            Blackout(scene.Dark);
        }
        void Blackout(bool value)
        {
            if(dark==value)return;dark=value;
            if(value){foreach(var a in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None)){var name=a.gameObject.name.ToLowerInvariant();if(a.transform.IsChildOf(root.transform)||!(name.Contains("engine")||name.Contains("generator")||name.Contains("rain")||name.Contains("storm")||name.Contains("thunder")))continue;quiet.Add((a,a.mute));a.mute=true;}foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(r.lightmapIndex>=0&&r.lightmapIndex<65534){lightmaps.Add((r,r.lightmapIndex));r.lightmapIndex=-1;}}
            else{foreach(var p in quiet)if(p.source)p.source.mute=p.muted;quiet.Clear();foreach(var p in lightmaps)if(p.renderer)p.renderer.lightmapIndex=p.index;lightmaps.Clear();}
        }
        public void Dispose(){Blackout(false);Object.Destroy(root);Object.Destroy(mesh);Object.Destroy(material);}
    }
}
