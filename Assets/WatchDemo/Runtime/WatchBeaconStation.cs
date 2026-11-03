using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
 public sealed class WatchBeaconStation:MonoBehaviour
 {
  WatchDemoController watch;WatchBeacon beacon;M2StationParts parts;TextMesh readout;Transform marker;Renderer lamp;MaterialPropertyBlock block;AudioSource sound;AudioClip click;
  Vector3 previousRag;bool wasHeld;Mesh beamMesh;Material beamMaterial;
  public BeaconControl Ignition {get;private set;}
  public BeaconControl Wheel {get;private set;}
  public M2CarryProp Rag {get;private set;}
  public Vector3 LensPoint=>beacon.transform.TransformPoint(new Vector3(0,1.3f,.43f));
  public WatchState State=>watch.State;
  public static WatchBeaconStation Create(WatchDemoController watch,WatchBeacon beacon)
  {var anchor=GameObject.Find("STN_BEACON_CONTROL");if(!anchor)throw new System.InvalidOperationException("Missing F4 beacon control anchor");var root=new GameObject("S4 beacon station");root.SetActive(false);root.transform.SetParent(anchor.transform,false);var c=root.AddComponent<WatchBeaconStation>();c.watch=watch;c.beacon=beacon;c.Build();root.SetActive(true);return c;}
  void Build()
  {
   parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();block=new MaterialPropertyBlock();
   parts.Part("Beacon control cabinet",transform,new Vector3(0,1.3f,.10f),new Vector3(.8f,.85f,.13f),parts.Dark);
   parts.Text("Beacon heading","BEACON CONTROL",transform,new Vector3(0,1.65f,.18f),.0045f);
   readout=parts.Text("Beacon instructions","",transform,new Vector3(0,1.53f,.19f),.003f);
   Ignition=Control("SWITCH_BeaconIgnition",new Vector3(.25f,1.20f,.19f),false);
   parts.Text("Ignition label","1 LIGHT IT",transform,new Vector3(.25f,1.05f,.20f),.003f);
   Wheel=Control("GRIP_BeaconWheel",new Vector3(-.14f,1.21f,.23f),true);
   parts.Text("Wheel label","2 ALIGN",transform,new Vector3(-.14f,.91f,.21f),.0035f);
   marker=M2StationParts.Root("Alignment pointer",transform,new Vector3(-.14f,1.21f,.22f));
   parts.Part("Alignment needle",marker,new Vector3(0,.16f,0),new Vector3(.014f,.11f,.018f),parts.Red);
   parts.Part("Target alignment mark",transform,new Vector3(-.14f,1.44f,.25f),new Vector3(.045f,.025f,.02f),parts.Green);
   lamp=parts.Part("Alignment green lamp",transform,new Vector3(.25f,1.39f,.20f),new Vector3(.075f,.035f,.02f),parts.Green).GetComponent<Renderer>();
   Rag=M2CarryProp.Create(parts,transform,"BEACON_RAG",new Vector3(.28f,.82f,.25f),true);
   parts.Part("Rag shelf",transform,new Vector3(.28f,.775f,.22f),new Vector3(.22f,.035f,.22f),parts.Brass);
   parts.Text("Rag instruction","3 WIPE LENS\nWITH CLOTH",transform,new Vector3(.25f,.64f,.22f),.003f);
   var wipeLabel=parts.Text("Lens wipe label","WIPE PRISM RINGS",beacon.transform,new Vector3(0,.72f,.46f),.003f);
   sound=gameObject.AddComponent<AudioSource>();sound.spatialBlend=1;sound.minDistance=.4f;sound.maxDistance=10;sound.playOnAwake=false;click=Resources.Load<AudioClip>("S4Audio/breaker");
   BuildBeam();State.Changed+=Refresh;Refresh();
  }
  BeaconControl Control(string name,Vector3 position,bool wheel)
  {
   var t=M2StationParts.Root(name,transform,position);var visual=M2StationParts.Root("Moving fitting",t,Vector3.zero);
   if(wheel)
   {
    const int n=24;for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;var segment=parts.Part("Brass wheel rim "+i,visual,new Vector3(Mathf.Sin(a)*.20f,Mathf.Cos(a)*.20f,.035f),new Vector3(.058f,.027f,.027f),parts.Brass);segment.transform.localRotation=Quaternion.Euler(0,0,-a*Mathf.Rad2Deg);}
    for(int i=0;i<3;i++){var spoke=parts.Part("Wheel spoke "+i,visual,Vector3.zero,new Vector3(.025f,.38f,.025f),parts.Brass);spoke.transform.localRotation=Quaternion.Euler(0,0,i*60);}
    parts.Part("Wheel hub",visual,Vector3.zero,Vector3.one*.07f,parts.Metal,PrimitiveType.Sphere);
   }
   else{parts.Part("Ignition lever",visual,new Vector3(0,0,.055f),new Vector3(.02f,.02f,.11f),parts.Metal);parts.Part("Ignition grip",visual,new Vector3(0,0,.12f),new Vector3(.10f,.035f,.035f),parts.Brass);}
   var col=t.gameObject.AddComponent<BoxCollider>();col.isTrigger=true;col.center=new Vector3(0,0,.05f);col.size=wheel?new Vector3(.45f,.45f,.15f):new Vector3(.14f,.14f,.20f);
   var x=t.gameObject.AddComponent<XRSimpleInteractable>();x.colliders.Add(col);t.gameObject.AddComponent<PhysicalGrip>().Configure(visual,HandPose.WrapGrip);
   var c=t.gameObject.AddComponent<BeaconControl>();c.Initialize(this,wheel,visual);return c;
  }
  public void Ignite()
  {bool on=!State.Beacon.Ignited;if(State.RemoteReplica&&State.Coop!=null)State.Coop.Send(CoopAction.BeaconIgnition,0,on);else watch.ApplyCommand(CoopAction.BeaconIgnition,0,on);Click();}
  public void Align(float angle)
  {if(State.RemoteReplica&&State.Coop!=null)State.Coop.Send(CoopAction.BeaconAlign,angle);else watch.ApplyCommand(CoopAction.BeaconAlign,angle,false);}
  public void Click(){if(click)sound.PlayOneShot(click,.22f);}
  void Refresh()
  {
   var b=State.Beacon;marker.localRotation=Quaternion.Euler(0,0,-135+b.Alignment*270);bool powered=State.HasPower(LighthouseKeepers.Core.PowerCircuit.Beacon);
   readout.text=!powered?"NO BEACON POWER — BREAKER F2":!b.Ignited?"FLIP IGNITION SWITCH":!b.Aligned?"TURN WHEEL TO GREEN MARK":$"ALIGNED · SOOT {b.Soot*100:0}%";
   var colour=b.Aligned?new Color(.03f,.9f,.1f):new Color(.7f,.08f,.015f);block.Clear();block.SetColor("_BaseColor",colour);block.SetColor("_EmissionColor",colour*.5f);lamp.SetPropertyBlock(block);
  }
  void Update()
  {
   var point=Rag.Tip.position;bool held=Rag.Grab.isSelected||State.Coop?.RemoteHeld(Rag.Grab)==true;
   if(!State.ReadOnly&&!State.Failed&&held&&wasHeld&&Vector3.Distance(point,LensPoint)<.58f)
   {float distance=Vector3.Distance(point,previousRag);if(distance>=State.PowerTuning.wipeStrokeMetres){if(distance<.15f)State.WipeBeacon(distance);previousRag=point;}}
   else previousRag=point;wasHeld=held;
  }
  public void ResetProps(){if(!Rag.Grab.isSelected&&State.Coop?.RemoteHeld(Rag.Grab)!=true)Rag.ResetHome();wasHeld=false;}
  void BuildBeam()
  {
   var shader=Resources.Load<Shader>("S4BeaconBeam");if(!shader)throw new System.InvalidOperationException("Missing included beacon beam shader");
   beamMaterial=new Material(shader){name="S4 sweeping amber beam"};
   var go=new GameObject("Visible outside beacon sweep");go.transform.SetParent(beacon.beam.transform,false);
   const int n=24;var vertices=new Vector3[n*2];var colours=new Color[n*2];var indices=new int[n*6];
   for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;var radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);vertices[i*2]=radial*.12f;vertices[i*2+1]=radial*4+Vector3.forward*24;colours[i*2]=Color.white;colours[i*2+1]=new Color(1,1,1,.02f);int k=i*6,next=(i+1)%n*2;indices[k]=i*2;indices[k+1]=next;indices[k+2]=i*2+1;indices[k+3]=i*2+1;indices[k+4]=next;indices[k+5]=next+1;}
   beamMesh=new Mesh{name="Beacon cone 24 segments"};beamMesh.vertices=vertices;beamMesh.colors=colours;beamMesh.triangles=indices;beamMesh.RecalculateBounds();go.AddComponent<MeshFilter>().sharedMesh=beamMesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=beamMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;beacon.SetVisibleBeam(renderer);
  }
  void OnDestroy(){if(watch&&State!=null)State.Changed-=Refresh;if(beamMesh)Destroy(beamMesh);if(beamMaterial)Destroy(beamMaterial);}
 }
 [DefaultExecutionOrder(100)]
 public sealed class BeaconControl:MonoBehaviour
 {
  WatchBeaconStation owner;bool wheel;Transform visual,left,right;PhysicalGrip grip;XRSimpleInteractable interaction;Quaternion previous;float working,nextSend,detent;
  public void Initialize(WatchBeaconStation parent,bool isWheel,Transform model){owner=parent;wheel=isWheel;visual=model;}
  void Awake(){grip=GetComponent<PhysicalGrip>();interaction=GetComponent<XRSimpleInteractable>();left=Palm(-1);right=Palm(1);interaction.selectEntered.AddListener(Select);interaction.selectExited.AddListener(Release);}
  Transform Palm(int side){var p=M2StationParts.Root(side<0?"Left palm":"Right palm",visual,new Vector3(side*(wheel?.20f:.014f),.012f,wheel?.055f:.14f));p.localRotation=Quaternion.Euler(0,180,0);return p;}
  void Select(SelectEnterEventArgs e){previous=e.interactorObject.transform.rotation;working=owner.State.Beacon.Alignment;if(!wheel)owner.Ignite();else owner.Click();}
  void Release(SelectExitEventArgs e){if(wheel)owner.Align(working);}
  void Update()
  {
   if(wheel&&interaction.isSelected&&interaction.interactorsSelecting.Count>0)
   {
    var rotation=interaction.interactorsSelecting[0].transform.rotation;var delta=rotation*Quaternion.Inverse(previous);previous=rotation;delta.ToAngleAxis(out float angle,out var axis);if(angle>180)angle-=360;float degrees=angle*Vector3.Dot(axis,transform.forward);
    if(float.IsFinite(degrees))working=Mathf.Clamp01(working+Mathf.Clamp(degrees,-20,20)/270);
    if(Time.time>=nextSend){nextSend=Time.time+.1f;owner.Align(working);}
    if(Mathf.Abs(working-detent)>.04f){detent=working;owner.Click();if(grip.HoldingHand)grip.HoldingHand.Pulse(.25f,.035f);}
   }
   visual.localRotation=wheel?Quaternion.Euler(0,0,-135+(interaction.isSelected?working:owner.State.Beacon.Alignment)*270):Quaternion.Euler(owner.State.Beacon.Ignited?-30:30,0,0);
   var hand=grip.HoldingHand;if(hand)hand.SetConstrainedGrip(hand.Node==XRNode.LeftHand?left:right);
  }
  void OnDestroy(){if(interaction){interaction.selectEntered.RemoveListener(Select);interaction.selectExited.RemoveListener(Release);}}
 }
}
