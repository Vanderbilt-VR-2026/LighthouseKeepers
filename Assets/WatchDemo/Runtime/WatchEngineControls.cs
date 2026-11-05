using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchEngineControls : MonoBehaviour
    {
        WatchDemoController watch;M2StationParts parts;MaterialPropertyBlock block;
        readonly Renderer[] lamps=new Renderer[4];
        Transform gaugeNeedle,starterHandle;LineRenderer starterCord;Vector3 starterCordMount;TextMesh gaugeText;AudioSource sound;AudioClip cough,clunk;
        int lastCough;float nextWarning;
        public EngineControl Primer {get;private set;}
        public EngineControl Choke {get;private set;}
        public M2CarryProp FuelCan {get;private set;}
        public Transform Filler {get;private set;}
        public WatchState State=>watch.State;
        public static WatchEngineControls Create(WatchDemoController watch,WatchGenerator generator)
        {var root=new GameObject("S4 generator sequence");root.SetActive(false);root.transform.SetParent(generator.transform,false);var c=root.AddComponent<WatchEngineControls>();c.watch=watch;c.Build();root.SetActive(true);return c;}
        public static string Instruction(WatchState state)
        {
            if(state.Director!=null)return state.Director.StationInstruction(WatchState.GeneratorStation);
            var e=state.Engine;if(state.Failed)return "WATCH ENDED";
            if(!e.IgnoreExtraPulls&&e.FloodRemaining>0)return $"ENGINE FLOODED — WAIT {Mathf.CeilToInt(e.FloodRemaining)} s";
            if(e.Running)return e.ChokeClosed?$"OPEN CHOKE NOW — {Mathf.CeilToInt(e.OpenRemaining)} s":e.LowFuel?"LOW FUEL — REFILL SOON":"ENGINE RUNNING";
            if(!e.EnoughFuel)return "1 FUEL — TILT RED CAN OVER FILLER";
            if(!e.Primed)return $"2 PRIME — GRIP BULB + SQUEEZE TRIGGER {e.Primer}/{state.PowerTuning.primerSqueezes}";
            if(!e.ChokeClosed)return "3 CHOKE — GRIP LEVER TO CLOSE";
            return $"4 START — PULL {e.Pulls}/{state.PowerTuning.starterPulls} · THEN OPEN CHOKE";
        }
        void Build()
        {
            foreach(var t in transform.parent.GetComponentsInChildren<Transform>(true))
                if(t.name=="READOUT_FuelGauge"||t.name=="Filler cap")t.gameObject.SetActive(false);
            parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();block=new MaterialPropertyBlock();
            parts.Part("Generator sequence panel",transform,new Vector3(0,1.36f,.86f),new Vector3(1.05f,.20f,.055f),parts.Dark);
            string[] names={"FUEL","PRIMED","CHOKE","RUNNING"};
            for(int i=0;i<4;i++){float x=.39f-i*.26f;parts.Text(names[i],names[i],transform,new Vector3(x,1.40f,.90f),.0032f);lamps[i]=parts.Part(names[i]+" lamp",transform,new Vector3(x,1.33f,.90f),new Vector3(.10f,.035f,.025f),parts.Green).GetComponent<Renderer>();}
            ExposeStarter();
            Primer=Control("GRIP_PrimerBulb",new Vector3(.40f,1.06f,.88f),false);
            Choke=Control("GRIP_ChokeControl",new Vector3(-.40f,1.06f,.88f),true);
            parts.Text("Primer label","PRIMER\nGRIP + TRIGGER",transform,new Vector3(.4f,.88f,.96f),.0028f);
            parts.Text("Choke label","CHOKE\nCLOSED / OPEN",transform,new Vector3(-.4f,.88f,.96f),.0028f);
            Filler=M2StationParts.Root("FUEL_FILLER",transform,new Vector3(-.38f,1.43f,.28f));
            parts.Part("Filler collar",Filler,Vector3.zero,new Vector3(.13f,.025f,.13f),parts.Brass,PrimitiveType.Cylinder);
            parts.Text("Filler label","FILL",transform,new Vector3(-.38f,1.49f,.40f),.003f);
            parts.Part("Fuel gauge face",transform,new Vector3(.69f,1.34f,.67f),new Vector3(.20f,.23f,.04f),parts.Dark);
            gaugeNeedle=M2StationParts.Root("Fuel gauge needle pivot",transform,new Vector3(.69f,1.30f,.697f));
            parts.Part("Fuel gauge needle",gaugeNeedle,new Vector3(0,.05f,0),new Vector3(.007f,.10f,.007f),parts.Brass);
            gaugeText=parts.Text("Fuel level","",transform,new Vector3(.69f,1.24f,.70f),.0028f);
            FuelCan=M2CarryProp.Create(parts,transform,"FUEL_CAN",new Vector3(1.02f,.24f,.20f),false);
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=.4f;sound.maxDistance=12;
            cough=Resources.Load<AudioClip>("S4Audio/cough");clunk=Resources.Load<AudioClip>("S4Audio/breaker");
            if(State.Immersion&&State.Immersion.physicalGuidance)BuildPhysicalGuidance();
            watch.State.Changed+=Refresh;Refresh();
        }
        void ExposeStarter()
        {
            var starter=GetComponentInParent<WatchGenerator>().Starter;
            starterHandle=starter.transform;
            // The new lamp panel occluded the original recessed handle from standing height.
            // Move the whole existing interaction forward; retain its rotation, grip anchors and 30 cm travel.
            starterHandle.position=transform.TransformPoint(new Vector3(.05f,1.15f,1.02f));
            starter.ConfigureFoundation(starterHandle);
            var bar=starterHandle.Find("Control grip");
            var size=bar.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            // This authored cylinder has baked dimensions and a 90-degree local rotation.
            bar.localPosition=Vector3.zero;bar.localScale=new Vector3(.035f/size.x,.14f/size.y,.035f/size.z);
            var renderer=bar.GetComponent<Renderer>();renderer.sharedMaterial=parts.Brass;
            var colour=new Color(.95f,.75f,.27f);block.Clear();block.SetColor("_BaseColor",colour);block.SetColor("_EmissionColor",colour*.24f);renderer.SetPropertyBlock(block);
            var oldStem=starterHandle.Find("Control stem");if(oldStem)oldStem.gameObject.SetActive(false);
            var housing=parts.Part("Starter recoil housing",transform,new Vector3(.05f,1.15f,.91f),new Vector3(.13f,.06f,.13f),parts.Dark,PrimitiveType.Cylinder);
            housing.transform.localRotation=Quaternion.Euler(90,0,0);
            parts.Text("Starter pull label","4 STARTER\nGRIP + PULL",transform,new Vector3(.05f,.98f,1.04f),.0032f);
            starterCordMount=new Vector3(.05f,1.15f,.97f);
            starterCord=new GameObject("Starter recoil cord").AddComponent<LineRenderer>();starterCord.transform.SetParent(transform,false);
            starterCord.positionCount=2;starterCord.useWorldSpace=true;starterCord.widthMultiplier=.009f;starterCord.sharedMaterial=parts.Metal;
            starterCord.generateLightingData=true;starterCord.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;starterCord.receiveShadows=false;
        }
        EngineControl Control(string name,Vector3 position,bool choke)
        {
            var t=M2StationParts.Root(name,transform,position);var visual=M2StationParts.Root("Moving grip",t,Vector3.zero);
            if(choke){parts.Part("Choke lever",visual,new Vector3(0,0,.07f),new Vector3(.025f,.025f,.14f),parts.Metal);parts.Part("Choke bar",visual,new Vector3(0,0,.14f),new Vector3(.12f,.035f,.035f),parts.Brass);}
            else parts.Part("Rubber primer bulb",visual,new Vector3(0,0,.07f),new Vector3(.065f,.09f,.065f),parts.Dark,PrimitiveType.Sphere);
            var col=t.gameObject.AddComponent<BoxCollider>();col.isTrigger=true;col.center=new Vector3(0,0,.08f);col.size=new Vector3(.14f,.14f,.18f);
            var x=t.gameObject.AddComponent<XRSimpleInteractable>();x.colliders.Add(col);t.gameObject.AddComponent<PhysicalGrip>().Configure(visual,HandPose.WrapGrip);
            var c=t.gameObject.AddComponent<EngineControl>();c.Initialize(this,choke,visual);return c;
        }
        public void Act(bool choke)
        {var action=choke?CoopAction.EngineChoke:CoopAction.EnginePrime;bool closed=choke&&!State.Engine.ChokeClosed;if(State.RemoteReplica&&State.Coop!=null)State.Coop.Send(action,0,closed);else watch.ApplyCommand(action,0,closed);if(clunk)sound.PlayOneShot(clunk,.25f);}
        void Refresh()
        {
            var e=State.Engine;
            for(int i=0;i<4;i++){bool on=i==0?e.EnoughFuel:i==1?e.Primed||physicalGuidance&&e.Settled:i==2?e.ChokeClosed||physicalGuidance&&e.Settled:e.Running;var col=on?new Color(.05f,.75f,.1f):new Color(.12f,.015f,.01f);if(i==0&&e.LowFuel)col=new Color(.9f,.24f,.015f);block.Clear();block.SetColor("_BaseColor",col);block.SetColor("_EmissionColor",on?col*.65f:Color.black);if(State.Director!=null)block.SetColor("_EmissionColor",Color.black);lamps[i].SetPropertyBlock(block);}
            RefreshPhysicalGuidance();
            gaugeNeedle.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(-60,60,e.Fuel));gaugeText.text=$"FUEL {e.Fuel*100:0}%";
            if(e.CoughSerial!=lastCough&&cough)sound.PlayOneShot(cough,.5f);lastCough=e.CoughSerial;
        }
        void Update()
        {
            AnimateCounters();
            starterCord.SetPosition(0,transform.TransformPoint(starterCordMount));starterCord.SetPosition(1,starterHandle.position-starterHandle.forward*.025f);
            if(State.Engine.LowFuel&&State.Engine.Running&&Time.time>=nextWarning){nextWarning=Time.time+8;if(cough)sound.PlayOneShot(cough,.2f);}
            bool held=FuelCan.Grab.isSelected||State.Coop?.RemoteHeld(FuelCan.Grab)==true;
            bool pouring=CanPour(held);
            ShowPour(pouring);
            if(!State.ReadOnly&&!State.Failed&&pouring)State.PourFuel(Time.deltaTime);
        }
        public void ResetProps(){if(!FuelCan.Grab.isSelected&&State.Coop?.RemoteHeld(FuelCan.Grab)!=true)FuelCan.ResetHome();lastCough=State.Engine.CoughSerial;}
        void OnDestroy(){if(watch&&watch.State!=null)watch.State.Changed-=Refresh;}
    }
    [DefaultExecutionOrder(100)]
    public sealed class EngineControl : MonoBehaviour
    {
        WatchEngineControls owner;bool choke;Transform visual,left,right;PhysicalGrip grip;XRSimpleInteractable interaction;float squeezed;
        public void Initialize(WatchEngineControls parent,bool lever,Transform model){owner=parent;choke=lever;visual=model;}
        void Awake()
        {grip=GetComponent<PhysicalGrip>();interaction=GetComponent<XRSimpleInteractable>();left=Palm(-.014f);right=Palm(.014f);interaction.selectEntered.AddListener(Select);interaction.activated.AddListener(Squeeze);}
        Transform Palm(float x){var t=M2StationParts.Root(x<0?"Left palm":"Right palm",visual,new Vector3(x,.012f,choke?.16f:.085f));t.localRotation=Quaternion.Euler(0,180,0);return t;}
        void Select(SelectEnterEventArgs e){if(choke)owner.Act(true);}
        void Squeeze(ActivateEventArgs e){if(!choke){owner.Act(false);squeezed=Time.time+.18f;if(grip.HoldingHand)grip.HoldingHand.Pulse(.35f,.06f);}}
        void Update()
        {if(choke)visual.localRotation=Quaternion.Slerp(visual.localRotation,Quaternion.Euler(owner.State.Engine.ChokeClosed?-30:30,0,0),1-Mathf.Exp(-20*Time.deltaTime));else visual.localScale=Vector3.one*(Time.time<squeezed?.75f:1);var hand=grip.HoldingHand;if(hand)hand.SetConstrainedGrip(hand.Node==XRNode.LeftHand?left:right);}
        void OnDestroy(){if(interaction){interaction.selectEntered.RemoveListener(Select);interaction.activated.RemoveListener(Squeeze);}}
    }
}
