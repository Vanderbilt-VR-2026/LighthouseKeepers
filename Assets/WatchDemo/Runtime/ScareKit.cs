using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;

namespace LighthouseKeepers.WatchDemo
{
    // Reusable, bounded world presentation. No camera transform or project-material edits.
    public sealed class ScareKit:System.IDisposable
    {
        readonly GameObject root;readonly Transform body;readonly Mesh mesh;readonly Material material;
        readonly MeshRenderer renderer;readonly MaterialPropertyBlock properties=new();
        readonly AudioSource sound,breath,voice,rising,hiss;readonly AudioHighPassFilter radioHigh;readonly AudioLowPassFilter radioLow;readonly List<(Renderer renderer,int index)> lightmaps=new();
        ScareHandprints prints;Vector3 previousBody,smoothedBody;float gait;bool dark;ScareBeat beat;Vector3 turnBase,wall,ceiling;Quaternion wallRotation;bool turnReady,turnHasWall;
        int observedAttempt=-1,observedCaught;float lungeAt=-1;Vector3 lungeFrom,lungeTo;bool burstPlayed;
        ParticleSystem bulbSparks;AudioSource bulbPop;
        readonly RaycastHit[] lungeHits=new RaycastHit[32];
        readonly Collider[] lungeOverlaps=new Collider[32];
        readonly List<(AudioSource source,bool muted)> quiet=new();
        public ScareKit(Transform parent)
        {
            root=new GameObject("Scripted scare kit");root.transform.SetParent(parent,false);
            body=new GameObject("Nightmare child").transform;body.SetParent(root.transform,false);
            mesh=DrownedForms.Build(DrownedForm.Doll);var bounds=mesh.bounds;bounds.Expand(.6f);mesh.bounds=bounds;body.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            material=new Material(Resources.Load<Shader>("S5Drowned"));renderer=body.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.enabled=false;
            sound=Source("Scene spatial one-shot",false);breath=Source("Scene wet breathing",true);breath.clip=Resources.Load<AudioClip>("S5Audio/DrownedHorror/breathing");voice=Source("Scare dialogue placeholder",false);KeeperAudioMix.Route(voice,"Radio");radioHigh=voice.gameObject.AddComponent<AudioHighPassFilter>();radioHigh.cutoffFrequency=250;radioLow=voice.gameObject.AddComponent<AudioLowPassFilter>();radioLow.cutoffFrequency=4500;radioHigh.enabled=radioLow.enabled=false;rising=Source("Caught keeper wet cloth strain",true);rising.transform.SetParent(root.transform,false);rising.clip=Resources.Load<AudioClip>("S5Audio/possession-rise");hiss=Source("Nightmare recovery radio static",true);hiss.clip=Resources.Load<AudioClip>("ScareDialogue/radio_static");hiss.volume=.006f;KeeperAudioMix.Route(hiss,"Radio");
        }
        AudioSource Source(string name,bool loop)
        {var o=new GameObject(name);o.transform.SetParent(body,false);o.transform.localPosition=Vector3.up;var a=o.AddComponent<AudioSource>();a.playOnAwake=false;a.loop=loop;a.spatialBlend=1;a.minDistance=1;a.maxDistance=14;a.dopplerLevel=0;a.volume=.4f;KeeperAudioMix.Route(a,"SFX_Critical");return a;}
        public void Beat(ScareBeat value,float age)
        {
            beat=value;turnReady=false;sound.Stop();radioHigh.enabled=radioLow.enabled=value==ScareBeat.Relief;
            // A replica arriving after the event does not replay a stale burst.
            burstPlayed=value!=ScareBeat.Turn||age>=HorrorTuning.Current.nightmareBulbBurstAt;
            string line=value==ScareBeat.Follow?"unknown_n2_follow":value==ScareBeat.Turn?"unknown_n2_not_him":value==ScareBeat.Relief?"hm_n2_nightmare_return":null;
            if(value==ScareBeat.Failed||value==ScareBeat.Idle)voice.Stop();
            if(age>1)return;
            if(line!=null){voice.Stop();voice.clip=Resources.Load<AudioClip>("ScareDialogue/"+line);voice.volume=value==ScareBeat.Relief?.55f:.4f;if(voice.clip)voice.Play();}
            string clip=value==ScareBeat.Turn?"reveal_b":value==ScareBeat.Warning?"approach":value==ScareBeat.Relief?"retreat":null;
            if(clip!=null){sound.clip=Resources.Load<AudioClip>("S5Audio/DrownedHorror/"+clip);if(sound.clip)sound.Play();}
        }
        public void Present(ScareSequenceState scene,Vector3 eye)
        {
            if(scene.Active&&prints==null)prints=new ScareHandprints(root.transform);prints?.Present(scene);
            if(!burstPlayed&&scene.Beat==ScareBeat.Turn&&scene.Age>=HorrorTuning.Current.nightmareBulbBurstAt){
                burstPlayed=true;if(scene.Age<HorrorTuning.Current.nightmareBulbBurstAt+1)BurstBulb();
            }
            if(observedAttempt!=scene.Attempt){observedAttempt=scene.Attempt;observedCaught=scene.Caught;lungeAt=-1;}
            else{int caught=scene.Caught&~observedCaught;observedCaught=scene.Caught;if(caught!=0)StartLunge(scene,(caught&1)!=0?1:2);}
            float lungeAge=Time.unscaledTime-lungeAt;bool lunging=lungeAt>=0&&lungeAge<HorrorTuning.Current.nightmareLungeSeconds;
            bool enabled=(scene.Active&&scene.Beat<=ScareBeat.Escape||lunging)&&DrownedAppearance.Sightings;
            float speed=Vector3.Distance(previousBody,scene.Body)/Mathf.Max(.001f,Time.deltaTime);previousBody=scene.Body;
            gait=Mathf.MoveTowards(gait,speed>.03f?1:0,Time.deltaTime*6);
            smoothedBody=scene.ReadOnly&&(scene.Beat==ScareBeat.Follow||scene.Beat==ScareBeat.Escape)&&Vector3.Distance(smoothedBody,scene.Body)<1?Vector3.Lerp(smoothedBody,scene.Body,1-Mathf.Exp(-18*Time.deltaTime)):scene.Body;
            body.SetPositionAndRotation(smoothedBody,Quaternion.Euler(0,scene.Yaw,0));
            if(scene.Beat==ScareBeat.Turn){
                if(!turnReady){turnReady=true;turnHasWall=false;turnBase=scene.Body;wall=turnBase;ceiling=turnBase;wallRotation=Quaternion.Euler(0,scene.Yaw,0);float best=3;
                    for(int i=0;i<8;i++){var direction=Quaternion.Euler(0,i*45,0)*Vector3.forward;if(Physics.Raycast(turnBase+Vector3.up*1.6f,direction,out var h,best,~0,QueryTriggerInteraction.Ignore)&&!h.collider.attachedRigidbody&&Mathf.Abs(h.normal.y)<.1f){best=h.distance;turnHasWall=true;var contact=h.point+h.normal*.25f;wall=new Vector3(contact.x,turnBase.y,contact.z);wallRotation=Quaternion.LookRotation(-h.normal,Vector3.up);}}
                    if(turnHasWall){ceiling=wall+Vector3.up*.5f;if(Physics.Raycast(wall+Vector3.up*1.6f,Vector3.up,out var roof,2.8f,~0,QueryTriggerInteraction.Ignore))ceiling=roof.point+Vector3.down*.36f;}
                }
                float t=scene.Age/Mathf.Max(1,HorrorTuning.Current.nightmareTurnSeconds);
                body.position=t<.55f?Vector3.Lerp(turnBase,wall,Mathf.SmoothStep(0,1,t/.55f)):Vector3.Lerp(wall,ceiling,Mathf.SmoothStep(0,1,(t-.55f)/.45f));
                body.rotation=turnHasWall?Quaternion.Slerp(wallRotation,wallRotation*Quaternion.Euler(90,0,180),Mathf.SmoothStep(0,1,(t-.55f)/.45f)):wallRotation;
            }
            if(lunging){float t=Mathf.Clamp01(lungeAge/Mathf.Max(.1f,HorrorTuning.Current.nightmareLungeSeconds));body.position=Vector3.Lerp(lungeFrom,lungeTo,Mathf.SmoothStep(0,1,t));var facing=Vector3.ProjectOnPlane(lungeTo-lungeFrom,Vector3.up);if(facing.sqrMagnitude>.001f)body.rotation=Quaternion.LookRotation(facing);}
            // Brief in-place joint/head distortion, not a camera effect or a collision warp.
            properties.SetFloat("_Walk",scene.Beat==ScareBeat.Follow?.55f*gait:scene.Beat==ScareBeat.Escape?gait:0);properties.SetFloat("_HeadTurn",scene.Beat==ScareBeat.Turn?Mathf.SmoothStep(0,2.7f,scene.Age):0);properties.SetFloat("_Visibility",1);properties.SetFloat("_Height",1.28f);properties.SetFloat("_Breath",scene.Clock);properties.SetFloat("_Arrival",lunging?lungeAge:scene.Age);properties.SetFloat("_Style",scene.Beat==ScareBeat.Turn?2:0);renderer.SetPropertyBlock(properties);
            renderer.enabled=enabled&&Vector3.Distance(eye,renderer.bounds.ClosestPoint(eye))>=.5f;
            bool breathe=enabled&&(beat==ScareBeat.Warning||beat==ScareBeat.Escape);
            if(breathe&&breath.clip&&!breath.isPlaying)breath.Play();else if(!breathe&&breath.isPlaying)breath.Stop();
            bool possessed=scene.Caught!=0&&scene.Active;
            if(possessed){rising.transform.position=scene.CatchPoint((scene.Caught&1)!=0?1:2);rising.transform.position+=Vector3.up;}
            if(possessed&&rising.clip&&!rising.isPlaying)rising.Play();else if(!possessed&&rising.isPlaying)rising.Stop();
            if(KeeperAudioMix.Instance)voice.volume=(scene.Beat==ScareBeat.Relief?.55f:.4f)*(KeeperAudioMix.Instance.VoiceActive?.5f:1);
            if(scene.Beat==ScareBeat.Relief)voice.transform.position=eye+Vector3.forward*.8f;else voice.transform.localPosition=Vector3.up;
            hiss.transform.position=voice.transform.position;bool radio=scene.Beat==ScareBeat.Relief&&voice.isPlaying;if(radio&&hiss.clip&&!hiss.isPlaying)hiss.Play();else if(!radio&&hiss.isPlaying)hiss.Stop();
            Blackout(scene.Dark);
        }
        void BurstBulb()
        {
            if(!bulbSparks){
                var go=new GameObject("Nightmare generator-room bulb pop");go.transform.SetParent(root.transform,false);go.transform.position=LanternRoomLighting.RoomLights[2];
                bulbSparks=go.AddComponent<ParticleSystem>();bulbSparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=bulbSparks.main;main.playOnAwake=false;main.loop=false;main.maxParticles=24;main.startLifetime=.35f;main.startSpeed=.65f;main.startSize=.018f;main.gravityModifier=.6f;main.startColor=new Color(1,.58f,.18f);main.simulationSpace=ParticleSystemSimulationSpace.World;
                var emission=bulbSparks.emission;emission.enabled=false;var shape=bulbSparks.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.025f;
                var render=go.GetComponent<ParticleSystemRenderer>();render.sharedMaterial=Resources.Load<Material>("TowerWiring/Sparks");render.shadowCastingMode=ShadowCastingMode.Off;render.receiveShadows=false;
                bulbPop=go.AddComponent<AudioSource>();bulbPop.playOnAwake=false;bulbPop.spatialBlend=1;bulbPop.minDistance=1;bulbPop.maxDistance=9;bulbPop.volume=.3f;bulbPop.clip=Resources.Load<AudioClip>("S4Audio/breaker");KeeperAudioMix.Route(bulbPop,"SFX_Critical");
            }
            bulbSparks.Emit(Mathf.Clamp(HorrorTuning.Current.nightmareBulbSparks,0,24));if(bulbPop.clip)bulbPop.Play();
            foreach(var node in new[]{XRNode.LeftHand,XRNode.RightHand}){var d=InputDevices.GetDeviceAtXRNode(node);if(d.TryGetHapticCapabilities(out var cap)&&cap.supportsImpulse)d.SendHapticImpulse(0,.18f,.06f);}
        }
        void StartLunge(ScareSequenceState scene,int duty)
        {
            var target=scene.CatchPoint(duty);var away=Vector3.ProjectOnPlane(scene.Body-target,Vector3.up).normalized;if(away.sqrMagnitude<.9f)return;
            var t=HorrorTuning.Current;float near=Mathf.Max(1.2f,t.nightmareLungeNear),far=Mathf.Max(near+.1f,t.nightmareLungeFar);
            int count=Physics.SphereCastNonAlloc(target+Vector3.up*.8f,t.radius,away,lungeHits,far,~0,QueryTriggerInteraction.Ignore);
            if(count==lungeHits.Length)return;
            for(int i=0;i<count;i++){var hit=lungeHits[i];if(hit.collider.attachedRigidbody||hit.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())continue;far=Mathf.Min(far,hit.distance-t.radius);}
            if(far<near+.1f)return;
            lungeFrom=target+away*far;lungeTo=target+away*near;
            if(!ClearLunge(lungeFrom,t.radius)||!ClearLunge(lungeTo,t.radius))return;
            lungeAt=Time.unscaledTime;
            var clip=Resources.Load<AudioClip>("S5Audio/DrownedHorror/reveal_b");if(clip){sound.Stop();sound.clip=clip;sound.Play();}
        }
        bool ClearLunge(Vector3 point,float radius)
        {
            int count=Physics.OverlapCapsuleNonAlloc(point+Vector3.up*(radius+.05f),point+Vector3.up*1.15f,radius,lungeOverlaps,~0,QueryTriggerInteraction.Ignore);
            if(count==lungeOverlaps.Length)return false;
            for(int i=0;i<count;i++)if(!lungeOverlaps[i].attachedRigidbody&&!lungeOverlaps[i].GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())return false;
            return true;
        }
        void Blackout(bool value)
        {
            if(dark==value)return;dark=value;
            if(value){foreach(var a in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None)){var name=a.gameObject.name.ToLowerInvariant();if(a.transform.IsChildOf(root.transform)||!(name.Contains("engine")||name.Contains("generator")))continue;quiet.Add((a,a.mute));a.mute=true;}foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(r.lightmapIndex>=0&&r.lightmapIndex<65534){lightmaps.Add((r,r.lightmapIndex));r.lightmapIndex=-1;}}
            else{foreach(var p in quiet)if(p.source)p.source.mute=p.muted;quiet.Clear();foreach(var p in lightmaps)if(p.renderer)p.renderer.lightmapIndex=p.index;lightmaps.Clear();}
        }
        public void Dispose(){Blackout(false);prints?.Dispose();Object.Destroy(root);Object.Destroy(mesh);Object.Destroy(material);}
    }
}
