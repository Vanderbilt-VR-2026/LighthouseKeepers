using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
    // Scene/input adapter. The watch owns all rescue rules; this component only observes hands,
    // publishes nearby grip, and presents a local fade without moving the tracked head itself.
    [DefaultExecutionOrder(1400)]
    public sealed class CrewRecovery : MonoBehaviour
    {
        WatchDemoController watch;XROrigin rig;HorrorTuning tune;HandRig[] hands;
        readonly XRSimpleInteractable[] targets=new XRSimpleInteractable[2];
        readonly LineRenderer[] rings=new LineRenderer[2];
        readonly bool[] selected=new bool[2];
        readonly List<Behaviour> disabled=new();
        AudioSource sound;readonly bool[] wasDown=new bool[2];float nextPulse;
        Material material;int seenSerial,revision=-1;bool blocked,moved;float fade,nextSend;
        readonly RaycastHit[] hits=new RaycastHit[24];
        public int LocalDuty=>(int)watch.CurrentDuty;
        public bool LocalDown=>watch.State.Recovery.Downed(LocalDuty);
        public static void Create(WatchDemoController w){var c=w.gameObject.AddComponent<CrewRecovery>();c.watch=w;c.rig=FindAnyObjectByType<XROrigin>();c.tune=HorrorTuning.Current;c.Build();}
        public Component Target(int duty)=>duty>=1&&duty<=2?targets[duty-1]:null;
        public bool HandPoint(int duty,out Vector3 point)
        {
            point=default;
            if(duty==LocalDuty){foreach(var hand in hands)if(hand&&hand.Node==XRNode.LeftHand){point=hand.transform.parent.position;return true;}return false;}
            if(watch.State.Coop is ICoopRecoveryHands views)return views.HandForDuty((CrewDuty)duty,out point);
            return false;
        }
        void Build()
        {
            hands=rig.GetComponentsInChildren<HandRig>(true);
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=1;sound.maxDistance=6;sound.volume=.12f;sound.clip=Resources.Load<AudioClip>("S4Audio/bolt-clunk");
            material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetColor("_BaseColor",new Color(.9f,.65f,.2f));
            for(int i=0;i<2;i++){
                int duty=i+1;
                var g=new GameObject("Rescue hand — "+(CrewDuty)duty);g.SetActive(false);g.transform.SetParent(transform,false);
                var col=g.AddComponent<SphereCollider>();col.radius=.12f;col.isTrigger=true;
                var body=g.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
                var grip=targets[i]=g.AddComponent<XRSimpleInteractable>();grip.colliders.Add(col);
                grip.selectEntered.AddListener(e=>{if(e.interactorObject is XRDirectInteractor&&!LocalDown){selected[duty-1]=true;Send(duty,true);}else e.manager.SelectExit(e.interactorObject,e.interactableObject);});
                grip.selectExited.AddListener(e=>{selected[duty-1]=false;Send(duty,false);});
                var ring=rings[i]=g.AddComponent<LineRenderer>();ring.sharedMaterial=material;ring.useWorldSpace=false;ring.widthMultiplier=.008f;ring.loop=false;ring.positionCount=33;ring.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        void Send(int duty,bool held){if(watch.State.Coop!=null)watch.State.Coop.Send(CoopAction.RecoveryGrip,duty,held);else watch.RecoveryCommand(0,duty,held,Vector3.zero,Vector3.zero);}
        public bool ClearReach(Vector3 hand,Vector3 target)
        {
            var delta=target-hand;if(delta.magnitude>tune.reviveHandReach)return false;
            int count=Physics.RaycastNonAlloc(hand,delta.normalized,hits,delta.magnitude,~0,QueryTriggerInteraction.Ignore);
            if(count==hits.Length)return false;
            for(int i=0;i<count;i++){var c=hits[i].collider;if(c.GetComponentInParent<XROrigin>()||c.GetComponentInParent<CrewRecovery>())continue;return false;}
            return true;
        }
        public bool Reach(Vector3 left,Vector3 right,int victim)=>HandPoint(victim,out var p)&&(ClearReach(left,p)||ClearReach(right,p));
        public bool LocalReach(int victim)
        {if(!HandPoint(victim,out var p))return false;foreach(var h in hands)if(h&&h.Direct&&ClearReach(h.transform.parent.position,p))return true;return false;}
        void Update()
        {
            if(!watch||watch.State?.Recovery==null||!rig)return;
            var s=watch.State;
            if(!s.ReadOnly&&!s.Failed)s.Recovery.SetHumans(s.HumanKeeperCount>1?3:LocalDuty);
            if(Time.unscaledTime>=nextSend){nextSend=Time.unscaledTime+.1f;for(int i=0;i<2;i++)if(selected[i])Send(i+1,true);}
        }
        void LateUpdate()
        {
            if(!watch||watch.State?.Recovery==null||!rig)return;
            var s=watch.State;var slot=s.Recovery.Slot(LocalDuty);
            if(revision!=watch.Revision){revision=watch.Revision;seenSerial=0;fade=0;moved=false;}
            bool down=slot?.down==true&&!s.Failed;
            if(down&&(!blocked||slot.serial!=seenSerial)){seenSerial=slot.serial;moved=false;fade=0;}
            SetBlocked(down);
            if(down&&!moved){fade=Mathf.MoveTowards(fade,1,Time.unscaledDeltaTime/tune.reviveFadeSeconds);if(fade>=1){Relocate();moved=true;}}
            else fade=Mathf.MoveTowards(fade,0,Time.unscaledDeltaTime/tune.reviveFadeSeconds);
            if(fade>0)watch.ShowRecoveryFade(fade);else watch.ClearRecoveryFade();
            for(int i=0;i<2;i++){
                var v=s.Recovery.Slot(i+1);
                if(wasDown[i]&&!v.down&&v.progress>=1){if(sound.clip)sound.Play();Pulse(.3f,.12f);}wasDown[i]=v.down;
                if(v.down&&v.progress>0&&Time.unscaledTime>=nextPulse&&(i+1==LocalDuty||selected[i])){nextPulse=Time.unscaledTime+.4f;Pulse(.10f,.035f);}
                bool visible=v.down&&!s.Failed&&HandPoint(i+1,out _);
                var g=targets[i].gameObject;if(g.activeSelf!=visible)g.SetActive(visible);if(!visible)continue;
                HandPoint(i+1,out var p);g.transform.SetPositionAndRotation(p,rig.Camera.transform.rotation);
                targets[i].enabled=i+1!=LocalDuty&&!LocalDown;
                var ring=rings[i];float arc=Mathf.Lerp(.12f,1,v.progress);
                for(int j=0;j<=32;j++){float a=j/32f*arc*Mathf.PI*2;ring.SetPosition(j,new Vector3(Mathf.Sin(a),Mathf.Cos(a),0)*.11f);}
            }
        }
        void SetBlocked(bool value)
        {
            if(blocked==value)return;blocked=value;
            if(!value){foreach(var b in disabled)if(b)b.enabled=true;disabled.Clear();return;}
            foreach(var h in hands)if(h&&h.Direct){var d=h.Direct;while(d.interactablesSelected.Count>0)d.interactionManager.SelectExit(d,d.interactablesSelected[0]);Disable(d);}
            foreach(var b in rig.GetComponentsInChildren<ContinuousMoveProvider>(true))Disable(b);
            foreach(var b in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>(true))Disable(b);
        }
        void Disable(Behaviour b){if(b.enabled){disabled.Add(b);b.enabled=false;}}
        void Relocate()
        {
            var c=rig.GetComponent<CharacterController>();bool enabled=c&&c.enabled;if(c)c.enabled=false;
            Vector3 h=rig.Camera.transform.position,anchor=tune.recoveryFloorPosition;
            rig.transform.position+=new Vector3(anchor.x-h.x,anchor.y-rig.transform.position.y,anchor.z-h.z);
            if(c)c.enabled=enabled;Physics.SyncTransforms();
        }
        void Pulse(float strength,float seconds){foreach(var h in hands)if(h)InputDevices.GetDeviceAtXRNode(h.Node).SendHapticImpulse(0,strength,seconds);}
        void OnDestroy(){SetBlocked(false);if(material)Destroy(material);}
    }
    public sealed partial class WatchDemoController:ICoopRecoveryView
    {
        CrewRecovery recoveryView;bool recoveryFading;
        public bool KeeperDown(int actor)=>state.Recovery.Downed(RecoveryDuty(actor));
        int RecoveryDuty(int actor)=>actor==LocalKeeper||coop==null?(int)CurrentDuty:(int)((coop as ICoopKeeperDuties)?.Duty(actor)??CrewDuty.None);
        public bool RecoveryCommand(int actor,int victim,bool held,Vector3 left,Vector3 right)
        {
            if(state.ReadOnly||sleeping||state.Failed||!recoveryView)return false;
            int rescuer=RecoveryDuty(actor);
            if(held&&(actor==LocalKeeper||coop==null?!recoveryView.LocalReach(victim):!recoveryView.Reach(left,right,victim)))return false;
            return state.Recovery.Hold(rescuer,victim,held);
        }
        public bool CompanionRescue(Vector3 actualHand)
        {
            var bot=GetComponent<CrewCompanion>();int victim=(int)CurrentDuty;
            if(!bot||!bot.Active||state.ReadOnly||state.HumanKeeperCount!=1||!recoveryView.HandPoint(victim,out var p)||!recoveryView.ClearReach(actualHand,p)||Vector3.Distance(bot.Position+Vector3.up,p)>1.2f)return false;
            return state.Recovery.Hold((int)bot.Duty,victim,true);
        }
        public bool RescueDrill(int actor)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(state.ReadOnly||sleeping||state.Failed||!recoveryView)return false;
            state.Recovery.SetHumans(state.HumanKeeperCount>1?3:(int)CurrentDuty);
            if(!state.Recovery.Down(RecoveryDuty(actor)))return false;
            state.Pressure?.Hold(actor,false);state.EngageWatch();return true;
#else
            return false;
#endif
        }
        public void RequestRescueDrill(){if(coop!=null)coop.Send(CoopAction.RecoveryDrill);else RescueDrill(0);}
        public void ShowRecoveryFade(float v){recoveryFading=true;fade.enabled=true;fadeMaterial.color=new Color(0,0,0,v);}
        public void ClearRecoveryFade(){if(!recoveryFading)return;recoveryFading=false;fade.enabled=currentFade>0;fadeMaterial.color=new Color(0,0,0,currentFade);}
    }
}
