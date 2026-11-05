using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
    [DefaultExecutionOrder(1200)]
    public sealed class DirectorPresentation:MonoBehaviour
    {
        WatchDemoController watch;Transform head;WatchPipeRepair pipe;WatchEngineControls engine;WatchRadioConsole radio;WatchBeaconStation beacon;WatchPowerPanel panel;
        readonly Dictionary<KeeperStep,Transform> targets=new();readonly Dictionary<Transform,Fitting> fittings=new();
        readonly List<(TextMesh text,string key)> labels=new();AudioSource sound;AudioClip click,positive;float untouched,nextTick;int stamp,lastDay;KeeperStep previous;Material material;MaterialPropertyBlock block;
        sealed class Fitting { public Transform cap;public Renderer renderer;public bool completed; }
        public static void Create(WatchDemoController w,Transform head)
        {if(w.State.Director==null)return;var v=w.gameObject.AddComponent<DirectorPresentation>();v.watch=w;v.head=head;}
        void Start()
        {
            pipe=FindAnyObjectByType<WatchPipeRepair>();engine=FindAnyObjectByType<WatchEngineControls>();radio=FindAnyObjectByType<WatchRadioConsole>();beacon=FindAnyObjectByType<WatchBeaconStation>();panel=FindAnyObjectByType<WatchPowerPanel>();
            material=new Material(pipe.indicators[0].sharedMaterial){name="Director control lamp"};material.EnableKeyword("_EMISSION");block=new MaterialPropertyBlock();
            Add(KeeperStep.GrabWrench,pipe.wrench.transform);
            for(int i=0;i<3;i++)Add((KeeperStep)((int)KeeperStep.BoltA+i),pipe.bolts[i]);
            Add(KeeperStep.Fuel,engine.FuelCan.transform);FittingFor(engine.Filler);Add(KeeperStep.Prime,engine.Primer.transform);Add(KeeperStep.CloseChoke,engine.Choke.transform);Add(KeeperStep.OpenChoke,engine.Choke.transform);Add(KeeperStep.Starter,FindAnyObjectByType<WatchGenerator>().Starter.transform);
            for(int i=0;i<4;i++){Add((KeeperStep)((int)KeeperStep.PumpOff+i),panel.Control(i).transform);Add((KeeperStep)((int)KeeperStep.PumpOn+i),panel.Control(i).transform);}
            Add(KeeperStep.ResetBreaker,panel.ResetControl.transform);
            foreach(var control in radio.controls){if(control.mainPower)Add(KeeperStep.RadioPower,control.transform);if(control.frequency)Add(KeeperStep.RadioTune,control.transform);}
            Add(KeeperStep.RadioTransmit,radio.microphone.transform);Add(KeeperStep.BeaconLight,beacon.Ignition.transform);Add(KeeperStep.BeaconAlign,beacon.Wheel.transform);Add(KeeperStep.BeaconWipe,beacon.Rag.transform);
            foreach(var control in radio.controls)if(control.codeColour>=0)FittingFor(control.transform);
            foreach(var text in FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {string key="label."+text.name;if(KeeperStrings.Has(key))labels.Add((text,key));}
            sound=new GameObject("Objective machine cues").AddComponent<AudioSource>();sound.transform.SetParent(transform,false);sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=.4f;sound.maxDistance=7;sound.volume=.18f;click=Resources.Load<AudioClip>("S4Audio/bolt-clunk");positive=Resources.Load<AudioClip>("AudioPass/sting_fixed");
        }
        void Add(KeeperStep step,Transform target){targets[step]=target;FittingFor(target);}
        Fitting FittingFor(Transform target)
        {
            if(fittings.TryGetValue(target,out var found))return found;
            // Tools and bolt flanges already have a physical surface: illuminate that, never a floating bead.
            if(target==pipe.wrench.transform||System.Array.IndexOf(pipe.bolts,target)>=0) {
                var renderer=target==pipe.wrench.transform?pipe.wrench.GetComponentInChildren<Renderer>():pipe.indicators[System.Array.IndexOf(pipe.bolts,target)];
                found=new Fitting{renderer=renderer};fittings.Add(target,found);return found;
            }
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name="Riveted control indicator";var c=go.GetComponent<Collider>();c.enabled=false;Destroy(c);go.transform.SetParent(target,false);go.transform.localPosition=new Vector3(0,.07f,.015f);var scale=target.lossyScale;go.transform.localScale=new Vector3(.028f/Mathf.Max(.001f,Mathf.Abs(scale.x)),.008f/Mathf.Max(.001f,Mathf.Abs(scale.y)),.028f/Mathf.Max(.001f,Mathf.Abs(scale.z)));go.transform.position=target.position+target.forward*.018f;
            var r=go.GetComponent<Renderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            found=new Fitting{cap=go.transform,renderer=r};fittings.Add(target,found);return found;
        }
        Transform Target(KeeperStep step)
        {
            if(step==KeeperStep.RadioCode){int colour=watch.State.Radio.Program.callCode[Mathf.Min(watch.State.Radio.CodeProgress,watch.State.Radio.Program.callCode.Length-1)];foreach(var c in radio.controls)if(c.codeColour==colour)return c.transform;}
            return targets.TryGetValue(step,out var target)?target:null;
        }
        int Progress()=>watch.State.Pipe.BoltsDone*1000000+watch.State.Pipe.TurnsDone*100000+(watch.State.Engine?.Primer??0)*10000+(watch.State.Engine?.Pulls??0)*1000+watch.State.Radio.Step*100+watch.State.Radio.CodeProgress*10+(watch.State.Engine?.ChokeClosed==true?1:0);
        void LateUpdate()
        {
            if(!pipe)return;var state=watch.State;var d=state.Director;
            foreach(var entry in labels){string value=KeeperStrings.Get(entry.key);if(entry.text&&entry.text.text!=value)entry.text.text=value;}
            var current=watch.CurrentInstruction;var partner=watch.PartnerInstruction;var target=Target(current);var other=Target(partner);
            int now=Progress();if(lastDay!=state.Day){lastDay=state.Day;foreach(var f in fittings.Values)f.completed=false;previous=current;stamp=now;untouched=0;}
            if(now!=stamp||previous!=current){untouched=0;stamp=now;}else if(!watch.WrenchHeld)untouched+=Time.deltaTime;
            if(previous!=current&&ObjectiveDirector.Job(previous)==ObjectiveDirector.Job(current)&&target&&Target(previous) is Transform old&&old!=target)
            {
                fittings[old].completed=true;sound.transform.position=old.position;if(click)sound.PlayOneShot(click,.5f);if(positive)sound.PlayOneShot(positive,.12f);
                foreach(var node in new[]{XRNode.LeftHand,XRNode.RightHand}){var device=InputDevices.GetDeviceAtXRNode(node);if(device.isValid)device.SendHapticImpulse(0,.18f,.05f);}
            }
            previous=current;
            if(state.Director==null)return;
            float pulse=.45f+.2f*Mathf.Sin(Time.time*Mathf.PI*2/state.Master.guidancePulseSeconds);
            foreach(var pair in fittings)
            {
                bool active=!state.Failed&&(pair.Key==target||pair.Key==other||pair.Key==engine.Filler&&(current==KeeperStep.Fuel||partner==KeeperStep.Fuel));var f=pair.Value;var colour=active?new Color(.95f,.63f,.22f):f.completed?new Color(.12f,.5f,.2f):new Color(.11f,.10f,.07f);
                block.Clear();block.SetColor("_BaseColor",colour);block.SetColor("_EmissionColor",active?colour*pulse:f.completed?colour*.12f:Color.black);f.renderer.SetPropertyBlock(block);
                if(f.cap)f.cap.localRotation=Quaternion.Slerp(f.cap.localRotation,Quaternion.Euler(0,0,f.completed?45:0),1-Mathf.Exp(-10*Time.deltaTime));
            }
            if(target&&untouched>=state.Master.idleTickSeconds&&Time.time>=nextTick&&!watch.ObjectivePresentationBlocked){nextTick=Time.time+4;sound.transform.position=target.position;if(click)sound.PlayOneShot(click,.14f);}
        }
        void OnDestroy(){if(material)Destroy(material);}
    }
}
