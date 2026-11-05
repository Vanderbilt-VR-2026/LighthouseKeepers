using System.Collections.Generic;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.WatchDemo
{
    // Built at the reserved Sprint 2 F2 panel anchor. No scene, stair or rig migration.
    public sealed class WatchPowerPanel : MonoBehaviour
    {
        WatchDemoController watch;
        readonly List<Material> ownedMaterials=new();
        readonly PowerPanelControl[] controls=new PowerPanelControl[4];
        readonly Renderer[] lamps=new Renderer[4];
        PowerPanelControl reset;
        Transform needle;
        TextMesh readout;
        Renderer tripLamp;
        Material steel,face,cream,brass,red,green;
        MaterialPropertyBlock block;
        AudioSource sound;
        AudioClip clunk;
        bool wasTripped;
        public PowerGrid Grid=>watch.State.Grid;
        public Component ResetControl=>reset;
        public PowerPanelControl Control(int index)=>index>=0&&index<4?controls[index]:null;

        public static WatchPowerPanel Create(WatchDemoController watch)
        {
            var anchor=GameObject.Find("STN_GENERATOR_PANEL");
            if(!anchor)throw new System.InvalidOperationException("Lab is missing its F2 generator panel anchor");
            var root=new GameObject("S4 breaker panel");root.SetActive(false);
            root.transform.SetParent(anchor.transform,false);
            var panel=root.AddComponent<WatchPowerPanel>();panel.watch=watch;panel.Build();root.SetActive(true);return panel;
        }
        Material Material(string name,Color colour,float metallic=0,float emission=0)
        {
            // Copy a serialized URP material already included by the accepted Lab scene.
            var template=Object.FindFirstObjectByType<WatchGenerator>().GetComponentInChildren<Renderer>().sharedMaterial;
            var m=new Material(template){name="S4 M2 "+name};
            m.SetColor("_BaseColor",colour);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.4f);
            m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",colour*emission);ownedMaterials.Add(m);return m;
        }
        Transform Root(string name,Transform parent,Vector3 position)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
        GameObject Part(string name,Transform parent,Vector3 position,Vector3 scale,Material material,PrimitiveType shape=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(shape);go.name=name;go.transform.SetParent(parent,false);
            go.transform.localPosition=position;go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            var renderer=go.GetComponent<Renderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return go;
        }
        TextMesh Label(string name,string text,Vector3 position,float size)
        {
            var t=Root(name,transform,position);t.localRotation=Quaternion.Euler(0,180,0);
            var label=t.gameObject.AddComponent<TextMesh>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;
            label.color=new Color(.93f,.87f,.65f);label.text=text;label.GetComponent<Renderer>().sharedMaterial=label.font.material;return label;
        }
        void Build()
        {
            block=new MaterialPropertyBlock();steel=Material("enamel cabinet",new Color(.065f,.12f,.12f),.5f,.12f);
            face=Material("dark instrument face",new Color(.025f,.035f,.035f),.2f,.1f);
            cream=Material("ivory labels",new Color(.72f,.67f,.48f),.1f,.2f);
            brass=Material("brass fittings",new Color(.55f,.32f,.09f),.75f,.1f);
            red=Material("trip red",new Color(.65f,.04f,.02f),.1f,.4f);
            green=Material("circuit green",new Color(.04f,.55f,.12f),.1f,.5f);
            Part("Enamel breaker cabinet",transform,new Vector3(0,1.28f,.09f),new Vector3(.79f,1.06f,.16f),steel);
            Part("Recessed control face",transform,new Vector3(0,1.28f,.18f),new Vector3(.72f,.97f,.025f),face);
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)
                Part("Cabinet screw",transform,new Vector3(x*.345f,1.28f+y*.45f,.202f),Vector3.one*.025f,brass,PrimitiveType.Sphere);
            Label("Panel heading","POWER DISTRIBUTION",new Vector3(0,1.72f,.21f),.0045f);
            Part("Load meter surround",transform,new Vector3(0,1.54f,.205f),new Vector3(.59f,.23f,.035f),brass);
            Part("Load meter glass face",transform,new Vector3(0,1.54f,.227f),new Vector3(.55f,.19f,.012f),cream);
            Part("Overload red zone",transform,new Vector3(-.145f,1.525f,.237f),new Vector3(.08f,.07f,.006f),red);
            needle=Root("Load needle pivot",transform,new Vector3(0,1.465f,.247f));
            Part("Load needle",needle,new Vector3(0,.07f,0),new Vector3(.008f,.14f,.008f),face);
            Label("Meter legend","LOAD         OVER",new Vector3(0,1.59f,.25f),.0035f);
            for(int i=0;i<4;i++)
            {
                float x=.27f-i*.18f;
                Label("Circuit label "+i,((PowerCircuit)i).ToString().ToUpperInvariant(),new Vector3(x,1.37f,.22f),.0034f);
                lamps[i]=Part("Circuit lamp "+i,transform,new Vector3(x,1.30f,.225f),new Vector3(.07f,.04f,.025f),green).GetComponent<Renderer>();
                controls[i]=MakeControl("SWITCH_"+((PowerCircuit)i).ToString().ToUpperInvariant(),new Vector3(x,1.18f,.22f),i,false);
            }
            reset=MakeControl("RESET breaker lever",new Vector3(.23f,.92f,.22f),-1,true);
            Label("Reset label","RESET",new Vector3(.23f,.81f,.24f),.004f);
            tripLamp=Part("Tripped lamp",transform,new Vector3(-.25f,.93f,.23f),new Vector3(.07f,.05f,.025f),red).GetComponent<Renderer>();
            readout=Label("Breaker instruction","",new Vector3(-.075f,.98f,.24f),.0031f);
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.minDistance=.4f;sound.maxDistance=10;
            clunk=Resources.Load<AudioClip>("S4Audio/breaker");if(!clunk)clunk=Resources.Load<AudioClip>("Hands/clunk");
            watch.State.Changed+=Refresh;Refresh();
        }
        PowerPanelControl MakeControl(string name,Vector3 position,int circuit,bool isReset)
        {
            var t=Root(name,transform,position);
            var visual=Root("Moving lever",t,Vector3.zero);
            Part("Lever shaft",visual,new Vector3(0,0,.05f),new Vector3(.025f,.025f,.10f),brass);
            Part("Chunky grip",visual,new Vector3(0,0,.11f),new Vector3(isReset?.14f:.105f,.04f,.04f),isReset?red:cream);
            var collider=t.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,0,.08f);collider.size=new Vector3(.15f,.13f,.16f);collider.isTrigger=true;
            var simple=t.gameObject.AddComponent<XRSimpleInteractable>();simple.colliders.Add(collider);
            var grip=t.gameObject.AddComponent<PhysicalGrip>();grip.Configure(visual,HandPose.WrapGrip);
            var c=t.gameObject.AddComponent<PowerPanelControl>();c.Initialize(this,circuit,isReset,visual);return c;
        }
        public void Activate(int circuit,bool isReset)
        {
            if(watch.State.Failed)return;
            var action=isReset?CoopAction.GridReset:CoopAction.GridCircuit;
            bool next=!isReset&&!Grid.Selected((PowerCircuit)circuit);
            if(watch.State.RemoteReplica&&watch.State.Coop!=null)watch.State.Coop.Send(action,circuit,next);
            else watch.ApplyCommand(action,circuit,next);
            if(clunk)sound.PlayOneShot(clunk,.4f);
        }
        void Lamp(Renderer renderer,Color colour,bool on)
        {
            block.Clear();block.SetColor("_BaseColor",on?colour:colour*.16f);block.SetColor("_EmissionColor",on?colour*.65f:Color.black);renderer.SetPropertyBlock(block);
        }
        void Refresh()
        {
            if(Grid==null)return;
            for(int i=0;i<4;i++){controls[i].SetSelected(Grid.Selected((PowerCircuit)i));Lamp(lamps[i],new Color(.06f,.8f,.15f),Grid.Powered((PowerCircuit)i));}
            Lamp(tripLamp,new Color(1,.035f,.015f),Grid.Tripped);
            needle.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(-65,65,Mathf.Clamp01(Grid.Demand/(Grid.Capacity*1.25f))));
            readout.text=watch.State.Director!=null?"":Grid.Tripped?"TRIPPED\nSWITCH LOADS OFF\nTHEN RESET":!Grid.EngineRunning?"GENERATOR OFF\nSTART ENGINE":$"{Grid.Demand:0.##} / {Grid.Capacity:0.##}\nKEEP BELOW LIMIT";
            if(Grid.Tripped&&!wasTripped&&clunk)sound.PlayOneShot(clunk,.65f);wasTripped=Grid.Tripped;
        }
        void OnDestroy(){if(watch&&watch.State!=null)watch.State.Changed-=Refresh;foreach(var m in ownedMaterials)if(m)Destroy(m);}
    }

    [RequireComponent(typeof(XRSimpleInteractable),typeof(PhysicalGrip))]
    public sealed class PowerPanelControl : MonoBehaviour
    {
        WatchPowerPanel panel;int circuit;bool reset,selected;Transform visual,left,right;
        XRSimpleInteractable interaction;PhysicalGrip grip;float pressedUntil;
        public void Initialize(WatchPowerPanel owner,int index,bool momentary,Transform moving)
        {panel=owner;circuit=index;reset=momentary;visual=moving;}
        void Awake()
        {
            interaction=GetComponent<XRSimpleInteractable>();grip=GetComponent<PhysicalGrip>();
            left=Anchor("Left palm",-.014f);right=Anchor("Right palm",.014f);
            interaction.selectEntered.AddListener(Grab);
        }
        Transform Anchor(string name,float x)
        {var t=new GameObject(name).transform;t.SetParent(visual,false);t.localPosition=new Vector3(x,.012f,.13f);t.localRotation=Quaternion.Euler(0,180,0);return t;}
        void Grab(SelectEnterEventArgs e)
        {panel.Activate(circuit,reset);pressedUntil=Time.unscaledTime+.3f;if(grip.HoldingHand)grip.HoldingHand.Pulse(.45f,.06f);}
        public void SetSelected(bool value)=>selected=value;
        void Update()
        {
            bool on=reset?Time.unscaledTime<pressedUntil:selected;
            visual.localRotation=Quaternion.Slerp(visual.localRotation,Quaternion.Euler(on?-30:30,0,0),1-Mathf.Exp(-22*Time.deltaTime));
            var hand=grip.HoldingHand;if(hand)hand.SetConstrainedGrip(hand.Node==XRNode.LeftHand?left:right);
        }
        void OnDestroy(){if(interaction)interaction.selectEntered.RemoveListener(Grab);}
    }
}
