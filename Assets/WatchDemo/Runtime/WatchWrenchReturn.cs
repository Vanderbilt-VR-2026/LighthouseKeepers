using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    [DefaultExecutionOrder(1100)]
    [RequireComponent(typeof(XRGrabInteractable),typeof(Rigidbody))]
    public sealed class WatchWrenchReturn : MonoBehaviour,INetworkPropRelease,INetworkReleasedBody
    {
        public const float ReturnSeconds=8f; // Legacy switch-off behavior.
        XRGrabInteractable grab; Rigidbody body;
        Vector3 home;Quaternion rotation;float returnAt=-1,awaySeconds;
        INetworkPropStatus network;INetworkKeeperProximity keepers;
        WatchDemoController watch;Transform head;AudioSource sound;AudioClip clank,splash;
        ParticleSystem entrySplash;
        int previousOwner;bool released,wet;float nextClank;
        ImmersionTuning tuning;
        bool Physical => tuning&&tuning.physicalWrenchDrops;
        public string LastReturnReason {get;private set;}
        public bool ReleasedBodyIsKinematic => !Physical||!released;
        void Awake()
        {
            grab=GetComponent<XRGrabInteractable>();body=GetComponent<Rigidbody>();home=transform.position;rotation=transform.rotation;
            tuning=ImmersionTuning.Current;
            if(Physical)
            {
                body.useGravity=true;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
                sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.dopplerLevel=0;sound.minDistance=.4f;sound.maxDistance=7;
                clank=Resources.Load<AudioClip>("S4Audio/bolt-clunk");splash=Resources.Load<AudioClip>("ImmersionAudio/wrench-splash");
            }
        }
        void OnEnable(){grab.selectEntered.AddListener(Grab);grab.selectExited.AddListener(Drop);}
        void OnDisable(){grab.selectEntered.RemoveListener(Grab);grab.selectExited.RemoveListener(Drop);}
        void Grab(SelectEnterEventArgs args){released=false;awaySeconds=0;returnAt=-1;}
        void Drop(SelectExitEventArgs args){released=true;awaySeconds=0;returnAt=Time.time+ReturnSeconds;}
        public void OnNetworkReleased(){released=true;awaySeconds=0;returnAt=Time.time+ReturnSeconds;previousOwner=0;}
        void LateUpdate()
        {
            if(network==null)network=GetComponent<INetworkPropStatus>();
            if(!watch){watch=FindAnyObjectByType<WatchDemoController>();if(watch)keepers=watch.GetComponent<INetworkKeeperProximity>();}
            if(!head&&Camera.main)head=Camera.main.transform;
            if(network!=null)
            {
                if(network.Owner!=previousOwner)
                {released=network.Owner==0;awaySeconds=0;returnAt=released?Time.time+ReturnSeconds:-1;previousOwner=network.Owner;}
            }
            // Clients hear the same water entry from replicated host position; only the host simulates it.
            if(Physical&&watch)
            {
                bool underwater=transform.position.y<watch.WaterHeight;
                if(underwater&&!wet&&released){if(sound&&splash)sound.PlayOneShot(splash,.4f);Splash();}
                wet=underwater;
            }
            if(network!=null&&!network.CanSimulateReleased)return;
            if(grab.isSelected||!released)return;
            if(!Physical){if(returnAt>=0&&Time.time>=returnAt)ReturnHome();return;}
            // XRI restores the pre-grab kinematic flag at detach; override only after detach.
            body.isKinematic=false;body.useGravity=true;
            body.linearDamping=wet?2.2f:.05f;body.angularDamping=wet?2f:.1f;
            bool near=keepers?.NearKeeper(transform.position,tuning.droppedWrenchKeeperRadius)==true;
            if(head){var d=head.position-transform.position;near|=Mathf.Abs(d.y)<2.5f&&new Vector2(d.x,d.z).sqrMagnitude<tuning.droppedWrenchKeeperRadius*tuning.droppedWrenchKeeperRadius;}
            awaySeconds=near?0:awaySeconds+Time.deltaTime;
            var p=transform.position;
            bool unreachable=!InsidePlayableRooms(p);
            if(unreachable||awaySeconds>=tuning.droppedWrenchReturnSeconds)
            {
                LastReturnReason=unreachable?"out of playable rooms":"20 seconds away from keepers";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"WRENCH_RETURN reason={LastReturnReason} position={p} awaySeconds={awaySeconds:F2}");
#endif
                ReturnHome();
            }
        }
        public static bool InsidePlayableRooms(Vector3 p)
        {
            if(!float.IsFinite(p.x)||!float.IsFinite(p.y)||!float.IsFinite(p.z)||p.y<-.35f||p.y>13.5f)return false;
            // Small collision tolerance around the authored shell; include the House throat.
            if(p.y<3f&&Mathf.Abs(p.x)<3.7f&&p.z> -10.05f&&p.z< -4.65f)return true;
            if(p.y<3f&&Mathf.Abs(p.x)<.75f&&p.z>=-5.55f&&p.z<=-3.75f)return true;
            float radius=p.y>=9.9f?2.85f:p.y>=6.6f?3.8f:p.y>=3.3f?3.85f:4.05f;
            return p.x*p.x+p.z*p.z<radius*radius;
        }
        void Splash()
        {
            if(!entrySplash)
            {
                var pipe=FindAnyObjectByType<WatchPipeRepair>();if(!pipe)return;
                entrySplash=Instantiate(pipe.sprays[0]);entrySplash.name="Wrench entry droplets";entrySplash.transform.SetParent(watch.transform,false);
                entrySplash.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=entrySplash.main;main.loop=false;main.playOnAwake=false;main.startLifetime=.35f;main.startSpeed=.65f;main.gravityModifier=1.2f;main.maxParticles=12;
                var emission=entrySplash.emission;emission.enabled=false;
            }
            entrySplash.transform.SetPositionAndRotation(new Vector3(transform.position.x,watch.WaterHeight+.01f,transform.position.z),Quaternion.LookRotation(Vector3.up));
            entrySplash.Play();entrySplash.Emit(10);
        }
        void OnCollisionEnter(Collision collision)
        {
            if(!Physical||!released||!sound||Time.time<nextClank||collision.relativeVelocity.sqrMagnitude<.09f)return;
            nextClank=Time.time+.2f;if(clank)sound.PlayOneShot(clank,Mathf.Clamp(collision.relativeVelocity.magnitude*.12f,.06f,.45f));
        }
        public void ReturnHome()
        {
            if(grab.isSelected || network!=null && !network.CanSimulateReleased)return;
            if(!body.isKinematic){body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;}
            released=false;awaySeconds=0;wet=false;
            body.isKinematic=true;body.position=home;body.rotation=rotation;transform.SetPositionAndRotation(home,rotation);returnAt=-1;
        }
    }
}
