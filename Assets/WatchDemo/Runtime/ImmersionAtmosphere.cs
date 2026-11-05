using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class ImmersionAtmosphere : MonoBehaviour
    {
        WatchDemoController watch;Transform head;AudioSource wind,events,heartbeat;Light lightning,dawn;HandRig[] hands;
        AudioClip thunder,creak,horn,drip;float nextEvent,flashAge=100;int eventNumber,day;bool waking;bool failed;
        public void Initialize(WatchDemoController w,Transform h)
        {
            watch=w;head=h;hands=FindObjectsByType<HandRig>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            wind=Source("Storm through window",new Vector3(2.5f,5,-.5f));wind.clip=Resources.Load<AudioClip>("ImmersionAudio/wind");wind.loop=true;wind.volume=w.State.Immersion.atmosphereVolume*.5f;if(!(AudioPassSettings.Current&&AudioPassSettings.Current.layeredRain))wind.Play();
            events=Source("Tower creaks and distant weather",new Vector3(0,6,0));events.volume=w.State.Immersion.atmosphereVolume;
            thunder=Resources.Load<AudioClip>("ImmersionAudio/thunder");creak=Resources.Load<AudioClip>("ImmersionAudio/creak");horn=Resources.Load<AudioClip>("ImmersionAudio/foghorn");drip=Resources.Load<AudioClip>("ImmersionAudio/drip");
            var go=new GameObject("Gentle lightning through windows");go.transform.SetParent(transform,false);go.transform.position=new Vector3(8,7,-8);
            lightning=go.AddComponent<Light>();lightning.type=LightType.Directional;lightning.color=new Color(.58f,.68f,.8f);lightning.transform.rotation=Quaternion.Euler(35,-50,0);lightning.shadows=LightShadows.None;lightning.intensity=0;
            var morning=new GameObject("Brief dawn through windows");morning.transform.SetParent(transform,false);
            dawn=morning.AddComponent<Light>();dawn.type=LightType.Directional;dawn.color=new Color(1,.73f,.43f);dawn.transform.rotation=Quaternion.Euler(18,-65,0);dawn.shadows=LightShadows.None;dawn.intensity=0;dawn.enabled=false;
            heartbeat=Source("Keeper low heartbeat",h.position);heartbeat.transform.SetParent(h,false);heartbeat.transform.localPosition=Vector3.zero;heartbeat.volume=.16f;heartbeat.clip=Resources.Load<AudioClip>("ImmersionAudio/heartbeat");heartbeat.loop=true;
            nextEvent=Time.time+12;
        }
        AudioSource Source(string name,Vector3 position)
        {var t=M2StationParts.Root(name,transform,position);var s=t.gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=1;s.dopplerLevel=0;s.minDistance=3;s.maxDistance=25;s.rolloffMode=AudioRolloffMode.Linear;t.gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=1800;return s;}
        void Update()
        {
            if(watch.State.Failed!=failed){failed=watch.State.Failed;if(failed){heartbeat.Play();if(watch.State.HighWaterFailed){var roar=Resources.Load<AudioClip>("S4Audio/water");if(roar)events.PlayOneShot(roar,1);}}else heartbeat.Stop();}
            if(day!=watch.State.Day){waking=day>0&&watch.State.Day>day;day=watch.State.Day;flashAge=0;PlayThunder();}
            // Existing host day/time drives both views. A brief recovery wash, never permanent daylight.
            float dawnPhase=(float)watch.State.Elapsed/watch.State.Immersion.dawnWashSeconds;
            dawn.enabled=waking&&!failed&&dawnPhase<1;
            dawn.intensity=dawn.enabled?watch.State.Immersion.dawnWashIntensity*Mathf.Sin(Mathf.Clamp01(dawnPhase)*Mathf.PI):0;
            flashAge+=Time.deltaTime;
            // One broad 1.4-second envelope, never repeated rapidly. No hard flashes or camera shake.
            lightning.intensity=flashAge<1.4f?.35f*Mathf.Sin(flashAge/1.4f*Mathf.PI):0;
            if(Time.time<nextEvent)return;nextEvent=Time.time+14+(eventNumber%3)*4;
            var clip=eventNumber%4==0?creak:eventNumber%4==1?horn:eventNumber%4==2?drip:thunder;
            events.transform.position=head.position+new Vector3(eventNumber%2==0?3:-3,1.5f,2);
            if(clip){if(clip==thunder){events.clip=clip;events.PlayDelayed(1.2f);}else events.PlayOneShot(clip,.7f);}
            if(eventNumber%4==3){flashAge=0;foreach(var hand in hands)if(hand)hand.Pulse(.08f,.12f);}
            eventNumber++;
        }
        void PlayThunder(){if(thunder){events.clip=thunder;events.PlayDelayed(1.2f);}foreach(var hand in hands)if(hand)hand.Pulse(.08f,.12f);}
    }
}
