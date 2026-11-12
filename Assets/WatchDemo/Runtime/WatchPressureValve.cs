using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class WatchPressureValve : MonoBehaviour
    {
        WatchDemoController watch;XRSimpleInteractable grip;Transform bar;WatchPipeRepair pipe;Transform presentation;Collider[] colliders;bool shown=true;int placedBolt=-1;Renderer lamp;MaterialPropertyBlock block;TextMesh instruction;float nextSend;bool selected;
        public XRSimpleInteractable Grip=>grip;
        public Transform GuidanceRoot=>presentation;
        public static WatchPressureValve Create(WatchDemoController w)
        {
            if(w.State.Pressure==null)return null;
            var root=new GameObject("STN_PRESSURE_VALVE");root.SetActive(false);root.transform.SetParent(w.transform,false);
            var v=root.AddComponent<WatchPressureValve>();v.watch=w;v.Build();root.SetActive(true);return v;
        }
        void Build()
        {
            var p=gameObject.AddComponent<M2StationParts>();p.Initialize();block=new MaterialPropertyBlock();
            pipe=watch.GetComponentInChildren<WatchPipeRepair>();if(!pipe)pipe=FindAnyObjectByType<WatchPipeRepair>();
            PlaceBesideActiveBolt();
            p.Part("Pressure valve wall plate",transform,new Vector3(0,.08f,-.08f),new Vector3(.40f,.48f,.04f),p.Dark);
            var hub=p.Part("Pressure valve hub",transform,Vector3.zero,new Vector3(.13f,.07f,.13f),p.Metal,PrimitiveType.Cylinder);hub.transform.localRotation=Quaternion.Euler(90,0,0);
            bar=M2StationParts.Root("Spring loaded valve handle",transform,new Vector3(0,0,.08f));
            p.Part("Valve grip bar",bar,Vector3.zero,new Vector3(.20f,.035f,.035f),p.Red);
            p.Part("Valve stem",transform,new Vector3(0,0,.04f),new Vector3(.025f,.025f,.08f),p.Metal);
            var tube=p.Part("Bypass return pipe",transform,new Vector3(0,-.30f,-.015f),new Vector3(.055f,.25f,.055f),p.Metal,PrimitiveType.Cylinder);
            instruction=p.Text("Valve fixed instruction","",transform,new Vector3(0,.235f,-.052f),.0030f);
            lamp=p.Part("Pressure indicator",transform,new Vector3(0,.14f,-.04f),new Vector3(.04f,.04f,.025f),p.Brass).GetComponent<Renderer>();
            var col=gameObject.AddComponent<BoxCollider>();col.isTrigger=true;col.center=new Vector3(0,0,.07f);col.size=new Vector3(.26f,.14f,.15f);
            grip=gameObject.AddComponent<XRSimpleInteractable>();grip.colliders.Add(col);
            gameObject.AddComponent<PhysicalGrip>().Configure(bar,HandPose.WrapGrip);
            grip.selectEntered.AddListener(Grab);grip.selectExited.AddListener(Release);
            presentation=new GameObject("Valve presentation").transform;presentation.SetParent(transform,false);
            var children=new System.Collections.Generic.List<Transform>();foreach(Transform child in transform)if(child!=presentation)children.Add(child);
            foreach(var child in children)child.SetParent(presentation,true);
            colliders=new Collider[]{col};SetVisible(watch.State.Pressure.CrewRequired);
        }
        void Grab(SelectEnterEventArgs e)
        {selected=true;nextSend=0;Send(true);}
        void Release(SelectExitEventArgs e){selected=false;Send(false);}
        void Send(bool held)
        {
            var s=watch.State;if(s.Coop!=null)s.Coop.Send(CoopAction.PressureValve,0,held);
            else watch.PressureCommand(0,held);
        }
        void Update()
        {
            var s=watch.State;var v=s.Pressure;
            SetVisible(v.CrewRequired);if(!v.CrewRequired)return;
            // Never teleport a held control. Move beside the next bolt after the keeper lets go.
            if(!selected&&v.Holder==0&&placedBolt!=s.Pipe.ActiveBolt)PlaceBesideActiveBolt();
            if(selected&&Time.unscaledTime>=nextSend){nextSend=Time.unscaledTime+.1f;Send(true);}
            float angle=v.Reduced?55:0;bar.localRotation=Quaternion.Slerp(bar.localRotation,Quaternion.Euler(0,0,angle),1-Mathf.Exp(-12*Time.deltaTime));
            instruction.text=KeeperStrings.Get(s.Repaired?"connected.pressure":v.Reduced?"connected.pressureHeld":"step.HoldValve");
            var colour=v.Reduced?new Color(.15f,.6f,.25f):new Color(.75f,.43f,.1f);
            bool urgent=!s.Repaired&&(watch.CurrentInstruction==KeeperStep.HoldValve||watch.PartnerInstruction==KeeperStep.HoldValve);
            block.Clear();block.SetColor("_BaseColor",colour);block.SetColor("_EmissionColor",colour*(s.Repaired?0:v.Reduced?.2f:urgent?.35f+.15f*Mathf.Sin(Time.time*5.2f):0));lamp.SetPropertyBlock(block);
        }
        void SetVisible(bool visible)
        {
            if(shown==visible)return;shown=visible;
            if(!visible&&selected){Send(false);selected=false;}
            presentation.gameObject.SetActive(visible);
            foreach(var collider in colliders)collider.enabled=visible;
            grip.enabled=visible;
        }
        void PlaceBesideActiveBolt()
        {
            if(!pipe||pipe.bolts==null||pipe.bolts.Length<3)return;
            int index=Mathf.Max(0,watch.State.Pipe.ActiveBolt);placedBolt=watch.State.Pipe.ActiveBolt;
            var bolt=pipe.bolts[index];var normal=Vector3.ProjectOnPlane(bolt.up,Vector3.up).normalized;
            if(normal.sqrMagnitude<.1f)normal=Vector3.left;
            var side=Vector3.Cross(Vector3.up,normal);
            transform.SetPositionAndRotation(bolt.position+side*.38f+normal*.14f-Vector3.up*.10f,Quaternion.LookRotation(normal));
        }
        void OnDisable(){if(selected&&watch)Send(false);selected=false;}
    }
}
