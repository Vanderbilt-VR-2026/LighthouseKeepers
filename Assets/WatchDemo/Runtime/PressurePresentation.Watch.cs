using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class PressurePresentation {
  Transform watchMount;readonly System.Collections.Generic.List<Mesh> watchMeshes=new();TextMesh nightLabel;Material resin,lcd;int shownNight;
  public bool WatchBacklight {get;private set;}
  void BuildWatch(M2StationParts parts){
   var left=System.Array.Find(FindObjectsByType<HandRig>(FindObjectsInactive.Include,FindObjectsSortMode.None),h=>h.Node==XRNode.LeftHand);if(!left)return;
   var wrist=left.transform.Find("Skinned glove/wrist");if(!wrist)return;
   // Mount on the dorsal side of the visual wrist, including its accepted grip correction.
   // This changes the accessory only, never the tracked controller or the hand pose.
   watchMount=M2StationParts.Root("Keeper wristwatch",wrist,new Vector3(0,.025f,-.014f));watchMount.localRotation=Quaternion.Euler(-90,0,0);
   resin=new Material(parts.Dark){name="Watch black resin"};resin.SetTexture("_BaseMap",null);resin.SetColor("_BaseColor",new Color(.018f,.021f,.018f));resin.SetColor("_EmissionColor",Color.black);resin.SetFloat("_Metallic",0);resin.SetFloat("_Smoothness",.23f);
   lcd=new Material(Resources.Load<Material>("WatchLCD")){name="Watch grey-green LCD"};
   WatchCase("Watch resin case",watchMount,Vector3.zero,new Vector3(.049f,.055f,.011f),.009f,resin);
   WatchCase("Watch raised protective bezel",watchMount,new Vector3(0,0,.006f),new Vector3(.045f,.051f,.003f),.007f,resin);
   WatchCase("Watch LCD",watchMount,new Vector3(0,0,.008f),new Vector3(.039f,.043f,.001f),.005f,lcd);
   // A half turn around the dial normal corrects reading direction without touching
   // the watch mount, accepted hand rotation, bones or tracked controller.
   var dial=M2StationParts.Root("Watch readable dial",watchMount,Vector3.zero);dial.localRotation=Quaternion.AngleAxis(180,Vector3.forward);
   for(int i=0;i<16;i++){
    float a=i*Mathf.PI*2/16,b=(i+1)*Mathf.PI*2/16;var p=new Vector3(Mathf.Cos(a)*.030f,0,-.023f+Mathf.Sin(a)*.024f);var q=new Vector3(Mathf.Cos(b)*.030f,0,-.023f+Mathf.Sin(b)*.024f);
    var band=parts.Part("Resin wrist strap",watchMount,(p+q)*.5f,new Vector3(.003f,.020f,Vector3.Distance(p,q)+.001f),resin);band.transform.localRotation=Quaternion.LookRotation(q-p,Vector3.up);
   }
   for(int side=-1;side<=1;side+=2)for(int end=-1;end<=1;end+=2)parts.Part("Watch side button",watchMount,new Vector3(side*.026f,end*.016f,0),new Vector3(.005f,.007f,.006f),parts.Metal);
   nightLabel=parts.Text("Watch night","",dial,new Vector3(0,.015f,.009f),.00060f);nightLabel.color=new Color(.07f,.10f,.06f);
   clock=parts.Text("Time to dawn","",dial,new Vector3(0,.003f,.009f),.0011f);clock.color=nightLabel.color;
   var face=new GameObject("Wrist current objective",typeof(RectTransform),typeof(TextMeshPro));face.transform.SetParent(dial,false);face.transform.localPosition=new Vector3(0,-.013f,.009f);face.transform.localScale=Vector3.one*.0001f;face.transform.localRotation=Quaternion.Euler(0,180,0);
   objective=face.GetComponent<TextMeshPro>();objective.font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");objective.rectTransform.sizeDelta=new Vector2(380,140);objective.enableAutoSizing=true;objective.fontSize=200;objective.fontSizeMin=110;objective.fontSizeMax=200;objective.alignment=TextAlignmentOptions.Center;objective.textWrappingMode=TextWrappingModes.Normal;objective.overflowMode=TextOverflowModes.Ellipsis;objective.maxVisibleLines=2;objective.color=nightLabel.color;
  }
  void UpdateWatch(WatchState s){
   if(!watchMount)return;
   var instruction=KeeperStrings.Get("step."+watch.CurrentInstruction);if(objective.text!=instruction)objective.text=instruction;
   int left=Mathf.Max(0,(int)(s.Duration-s.Elapsed));if(left!=shownSecond||s.Day!=shownNight){shownSecond=left;shownNight=s.Day;clock.text=$"{left/60:00}:{left%60:00}";nightLabel.text=$"N{s.Day}";}
   var toEyes=head.position-watchMount.position;WatchBacklight=toEyes.sqrMagnitude<.8f*.8f&&Vector3.Dot(watchMount.forward,toEyes.normalized)>.65f;
   lcd.SetColor("_BaseColor",WatchBacklight?new Color(.67f,.76f,.51f):new Color(.55f,.62f,.48f));
  }
  void WatchCase(string name,Transform parent,Vector3 position,Vector3 size,float radius,Material material){var build=new WorkshopMesh();build.Case(Vector3.zero,size,radius);var mesh=build.Finish(name);watchMeshes.Add(mesh);var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
  void OnDestroy(){foreach(var mesh in watchMeshes)if(mesh)Destroy(mesh);if(resin)Destroy(resin);if(lcd)Destroy(lcd);}
 }
}
