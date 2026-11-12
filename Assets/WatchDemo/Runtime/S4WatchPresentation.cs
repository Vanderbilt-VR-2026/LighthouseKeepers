using System.Collections.Generic;
using LighthouseKeepers.Core;
using LighthouseKeepers.Audio;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.WatchDemo
{
    /// <summary>Optional presentation layered on existing station/day rules; no camera movement.</summary>
    public sealed class S4WatchPresentation:MonoBehaviour
    {
        WatchDemoController watch;
        S4Tuning tuning;
        Transform head,bed;
        WatchGenerator generator;
        WatchPipeRepair pipe;
        TextMesh nightText;
        Material nightMaterial;
        AudioSource pipeFx,generatorFx,bedFx,boardFx,water;
        readonly Dictionary<string,AudioClip> clips=new();
        PlayerFootsteps footsteps;
        XRGrabInteractable microphone;
        WatchRadioConsole radioConsole;
        int day,bolts,pulls,shownNight;
        bool failed,complete,night;
        PowerState power;
        float nextWater;

        public void Initialize(WatchDemoController controller,Transform view,Transform bedPoint,
            WatchGenerator gen,WatchPipeRepair repair,WatchRadioConsole radio,Renderer fade)
        {
            watch=controller;tuning=watch.State.Tuning;head=view;bed=bedPoint;generator=gen;pipe=repair;
            if(tuning.starterGrip&&!generator.Starter.GetComponent<S4StarterGrip>())generator.Starter.gameObject.AddComponent<S4StarterGrip>();
            if(tuning.nightSequence&&!(watch.State.Immersion&&watch.State.Immersion.logbook))
            {
                nightText=new GameObject("Night survived readout").AddComponent<TextMesh>();nightText.transform.SetParent(head,false);
                nightText.transform.localPosition=new Vector3(0,0,.32f);nightText.characterSize=.0015f;nightText.fontSize=64;
                nightText.anchor=TextAnchor.MiddleCenter;nightText.alignment=TextAlignment.Center;
                nightMaterial=new Material(nightText.GetComponent<Renderer>().sharedMaterial){renderQueue=4020};
                nightText.GetComponent<Renderer>().sharedMaterial=nightMaterial;nightText.gameObject.SetActive(false);
            }
            if(tuning.soundPass)
            {
                foreach(var clip in Resources.LoadAll<AudioClip>("S4Audio")){clip.LoadAudioData();clips[clip.name]=clip;}
                pipeFx=Source("S4 pipe clunks",pipe.transform.position);generatorFx=Source("S4 engine cough and breaker",gen.transform.position);
                bedFx=Source("S4 night storm",bed.position);boardFx=Source("S4 watch bell",watch.BoardTransform.position);
                water=Source("S4 rising water",Vector3.zero);water.clip=Clip("water");water.loop=true;water.volume=0;water.Play();
                if(Clip("hiss")){pipe.leakAudio.clip=Clip("hiss");pipe.leakAudio.Play();}
                foreach(var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                    if(source.clip&&source.clip.name=="Placeholder_RainInterior")source.volume=tuning.weatherVolume;
                ConfigureFootsteps();
                radioConsole=radio;microphone=radio.microphone.GetComponent<XRGrabInteractable>();
                microphone.activated.AddListener(MicDown);microphone.deactivated.AddListener(MicUp);
                // Existing original rain/thunder and radio retain their authored spatial emitters
                // and AcousticDirector's indoor low-pass/mixer treatment.
                radio.effects.spatialBlend=radio.hiss.spatialBlend=radio.tone.spatialBlend=radio.chatter.spatialBlend=1;
                foreach(var source in new[]{radio.effects,radio.hiss,radio.tone,radio.chatter}){source.minDistance=.8f;source.maxDistance=10;}
            }
            day=watch.State.Day;bolts=watch.State.Pipe.BoltsDone;pulls=watch.State.StarterPulls;power=watch.State.Power;
            watch.State.Changed+=Changed;
            if(tuning.soundPass)Play(boardFx,"bell",.7f);
        }
        AudioClip Clip(string name)=>clips.TryGetValue(name,out var clip)?clip:null;
        AudioSource Source(string label,Vector3 position)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);go.transform.position=position;
            var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=1;source.dopplerLevel=0;
            source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=1;source.maxDistance=12;source.volume=tuning.effectsVolume;return source;
        }
        void Play(AudioSource source,string clip,float volume=1){if(source&&Clip(clip))source.PlayOneShot(Clip(clip),volume);}
        void ConfigureFootsteps()
        {
            var rig=head.GetComponentInParent<XROrigin>();if(!rig)return;
            footsteps=rig.GetComponent<PlayerFootsteps>();
            if(!footsteps)
            {
                var source=Source("S4 keeper footsteps",rig.transform.position);source.transform.SetParent(rig.transform,false);
                footsteps=rig.gameObject.AddComponent<PlayerFootsteps>();
                footsteps.Configure(head,rig.GetComponent<GravityProvider>(),source,new[]{Clip("wood1"),Clip("wood2"),Clip("wood3"),Clip("wood4")});
            }
            footsteps.SurfaceClip=SurfaceStep;
        }
        AudioClip SurfaceStep(AudioClip fallback)
        {
            if(Physics.Raycast(head.position,Vector3.down,out var hit,2.5f,~0,QueryTriggerInteraction.Ignore))
            {
                var r=hit.collider.GetComponent<Renderer>();
                string name=(r&&r.sharedMaterial?r.sharedMaterial.name:hit.collider.name).ToLowerInvariant();
                if(name.Contains("metal")||name.Contains("steel")||name.Contains("grate"))return Clip("metal")??fallback;
            }
            return fallback;
        }
        void Changed()
        {
            var s=watch.State;
            if(tuning.soundPass)
            {
                if(s.Day!=day)Play(boardFx,"bell",.7f);
                if(s.Pipe.BoltsDone>bolts)Play(pipeFx,"bolt-clunk",.8f);
                if(s.StarterPulls>pulls&&s.Power==PowerState.Off)Play(generatorFx,"cough",.55f);
                if(s.Power!=power&&s.Power==PowerState.On)Play(generatorFx,"breaker",.8f);
                if(s.Failed&&!failed)Play(boardFx,"fail",.7f);
                if(s.Complete&&!complete)Play(boardFx,"success",.65f);
            }
            day=s.Day;bolts=s.Pipe.BoltsDone;pulls=s.StarterPulls;power=s.Power;failed=s.Failed;complete=s.Complete;
        }
        public void Night(bool sleeping,int survivedDay,float fade)
        {
            if(!tuning||!tuning.nightSequence)return;
            if(sleeping&&!night&&tuning.soundPass)Play(bedFx,"night",.8f);
            night=sleeping;
            if(nightText)
            {
                nightText.gameObject.SetActive(sleeping&&survivedDay>0&&fade>.55f);
                if(shownNight!=survivedDay){shownNight=survivedDay;nightText.text=LighthouseKeepers.Core.PlayerStrings.Get("legacy.S4WatchPresentation.1")+survivedDay+LighthouseKeepers.Core.PlayerStrings.Get("legacy.S4WatchPresentation.2");}
                nightText.color=new Color(.95f,.88f,.7f,Mathf.InverseLerp(.55f,.9f,fade));
            }
        }
        void Update()
        {
#if UNITY_EDITOR
            if(tuning&&tuning.desktopSleep&&!UnityEngine.XR.XRSettings.isDeviceActive&&Keyboard.current!=null&&Keyboard.current.fKey.wasPressedThisFrame)
                watch.TrySleep();
#endif
            if(!water||Time.unscaledTime<nextWater)return;nextWater=Time.unscaledTime+.1f;
            water.transform.position=new Vector3(0,watch.WaterHeight,0);
            water.volume=tuning.waterVolume*Mathf.InverseLerp(.02f,tuning.failWaterHeightM,watch.WaterHeight);
        }
        void MicDown(ActivateEventArgs e){if(radioConsole)radioConsole.Click(.2f);}
        void MicUp(DeactivateEventArgs e){if(radioConsole)radioConsole.Click(.15f);}
        void OnDestroy()
        {if(microphone){microphone.activated.RemoveListener(MicDown);microphone.deactivated.RemoveListener(MicUp);}if(watch&&watch.State!=null)watch.State.Changed-=Changed;if(footsteps)footsteps.SurfaceClip=null;if(nightMaterial)Destroy(nightMaterial);}
    }
}
