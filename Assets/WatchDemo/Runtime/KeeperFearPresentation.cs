using UnityEngine;
using UnityEngine.XR;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
    public static class HeartbeatSettings
    {
        const string Key="LK.HeartbeatHaptics";
        public static bool Enabled {get=>PlayerPrefs.GetInt(Key,1)!=0;set{PlayerPrefs.SetInt(Key,value?1:0);PlayerPrefs.Save();}}
    }
    // Local feedback for the host-authored value assigned to THIS keeper. No camera/body transforms change.
    public sealed class KeeperFearPresentation:MonoBehaviour
    {
        WatchDemoController watch;AudioSource heart,breath,hunted;AudioClip beatClip,breathClip,huntedClip;
        float shown,nextBeat,secondAt=-1;
        public float Value=>shown;
        public static void Create(WatchDemoController w,Transform head)
        {if(w.State.Fear==null)return;var p=w.gameObject.AddComponent<KeeperFearPresentation>();p.watch=w;p.Initialize();}
        void Initialize()
        {
            beatClip=Clip("Original low heartbeat",.13f,0);breathClip=Clip("Original keeper breath",5,1);huntedClip=Clip("Original hunted undertone",4,2);
            heart=Source("Keeper felt heartbeat",beatClip,false,"SFX_Critical");breath=Source("Keeper close breathing",breathClip,true,"SFX_Critical");hunted=Source("Hunted music undertone",huntedClip,true,"Music");
            breath.Play();hunted.Play();nextBeat=Time.unscaledTime+.5f;
        }
        AudioSource Source(string name,AudioClip clip,bool loop,string bus)
        {var g=new GameObject(name);g.transform.SetParent(transform,false);var a=g.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=0;a.clip=clip;a.loop=loop;a.volume=0;a.priority=100;KeeperAudioMix.Route(a,bus);return a;}
        // Bounded deterministic synthesis; original sounds, no external sample or licence dependency.
        static AudioClip Clip(string name,float seconds,int kind)
        {
            const int rate=44100;var data=new float[Mathf.RoundToInt(seconds*rate)];var rng=new System.Random(713+kind);float low=0;
            for(int i=0;i<data.Length;i++){
                float t=(float)i/rate,phase=t/seconds;float noise=(float)(rng.NextDouble()*2-1);low+=.08f*(noise-low);
                if(kind==0)data[i]=.6f*Mathf.Sin(2*Mathf.PI*(58*t-25*t*t))*Mathf.Sin(Mathf.PI*phase)*Mathf.Exp(-14*t);
                else if(kind==1){float envelope=Mathf.Pow(Mathf.Sin(Mathf.PI*phase),2);data[i]=low*envelope*.65f;}
                else data[i]=.08f*(Mathf.Sin(2*Mathf.PI*43*t)+.35f*Mathf.Sin(2*Mathf.PI*61*t))*(.75f+.25f*Mathf.Sin(2*Mathf.PI*.5f*t));
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void Update()
        {
            var s=watch.State;int actor=(s.Coop as ICoopKeeperLocations)?.LocalActor??0;
            bool silent=watch.Sleeping||s.Failed||s.Fear==null;
            float target=silent?0:s.Fear.ForActor(actor);shown=Mathf.MoveTowards(shown,target,Time.unscaledDeltaTime*2);
            if(KeeperAudioMix.Instance)KeeperAudioMix.Instance.LocalFear=silent?0:shown;
            breath.volume=silent||s.Fear.HoldingBreath(actor)?0:Mathf.Lerp(.07f,.2f,shown);breath.pitch=Mathf.Lerp(1,2.2f,shown);
            hunted.volume=silent?0:Mathf.InverseLerp(.6f,1,shown)*.16f;
            if(silent){secondAt=-1;return;}
            float now=Time.unscaledTime;
            if(now>=nextBeat){Pulse(1);secondAt=now+.13f;nextBeat=now+60/KeeperFearState.BeatsPerMinute(shown);}
            if(secondAt>=0&&now>=secondAt){Pulse(.72f);secondAt=-1;}
        }
        void Pulse(float accent)
        {
            heart.volume=Mathf.Lerp(.045f,.16f,shown);heart.PlayOneShot(beatClip,accent);
            if(!HeartbeatSettings.Enabled||watch.ObjectivePresentationBlocked)return;
            float amplitude=KeeperFearState.HapticAmplitude(shown)*accent;
            Haptic(XRNode.LeftHand,amplitude);Haptic(XRNode.RightHand,amplitude);
        }
        static void Haptic(XRNode hand,float amplitude){var device=InputDevices.GetDeviceAtXRNode(hand);if(device.TryGetHapticCapabilities(out var cap)&&cap.supportsImpulse)device.SendHapticImpulse(0,amplitude,.04f);}
        void OnDestroy(){if(KeeperAudioMix.Instance)KeeperAudioMix.Instance.LocalFear=0;if(beatClip)Destroy(beatClip);if(breathClip)Destroy(breathClip);if(huntedClip)Destroy(huntedClip);}
    }
}
