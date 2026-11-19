using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo {
    public sealed class KeeperRunInput:MonoBehaviour {
        WatchDemoController watch;ContinuousMoveProvider move;KeeperStamina stamina;float nextSend;bool priorBreath;
        AudioSource breath,gasp;AudioClip breathClip,gaspClip;
        public float Remaining=>stamina?.Remaining??1;public bool HoldingBreath {get;private set;}
        public static void Create(WatchDemoController w){var c=w.gameObject.AddComponent<KeeperRunInput>();c.watch=w;c.stamina=new KeeperStamina(HorrorTuning.Current);var rig=FindAnyObjectByType<XROrigin>();c.move=rig?rig.GetComponentInChildren<ContinuousMoveProvider>():null;}
        void Start(){breathClip=KeeperFearPresentation.Clip("Running keeper breathing",5,1);gaspClip=KeeperFearPresentation.Clip("Low stamina exhale",.65f,1);breath=Source(breathClip,true);gasp=Source(gaspClip,false);breath.Play();}
        AudioSource Source(AudioClip clip,bool loop){var g=new GameObject(clip.name);g.transform.SetParent(transform,false);var a=g.AddComponent<AudioSource>();a.playOnAwake=false;a.clip=clip;a.loop=loop;a.spatialBlend=0;a.volume=0;KeeperAudioMix.Route(a,"SFX_Critical");return a;}
        void Update(){
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton,out bool a);
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primaryButton,out bool x);
            bool allowed=watch&&watch.State!=null&&!watch.ObjectivePresentationBlocked&&!watch.State.Failed&&!watch.State.Recovery.Downed((int)watch.CurrentDuty);
            bool moving=move&&move.enabled&&move.leftHandMoveInput.ReadValue().sqrMagnitude>.09f;
            HoldingBreath=allowed&&x&&!a;float speed=stamina.Tick(Time.unscaledDeltaTime,a,moving,allowed);KeeperMovement.Current?.SetRun(speed);
            if(Time.unscaledTime>=nextSend||priorBreath!=HoldingBreath){nextSend=Time.unscaledTime+.15f;priorBreath=HoldingBreath;if(watch.State.Coop!=null)watch.State.Coop.Send(CoopAction.KeeperBreath,0,HoldingBreath);else watch.BreathCommand(0,HoldingBreath);}
            breath.volume=allowed&&!HoldingBreath?(1-Remaining)*.24f:0;breath.pitch=Mathf.Lerp(1,1.8f,1-Remaining);
            if(stamina.Warning){gasp.volume=.22f;gasp.Play();InputDevices.GetDeviceAtXRNode(XRNode.RightHand).SendHapticImpulse(0,.18f,.06f);}
        }
        void OnDisable(){KeeperMovement.Current?.SetRun(1);if(watch&&watch.State!=null&&!watch.State.ReadOnly)watch.BreathCommand(0,false);}
        void OnDestroy(){if(breathClip)Destroy(breathClip);if(gaspClip)Destroy(gaspClip);}
    }
    public sealed partial class WatchDemoController:ICoopBreathView {
        public bool BreathCommand(int actor,bool held)=>!state.ReadOnly&&!state.Failed&&state.SetBreath(RecoveryDuty(actor),held);
    }
}
