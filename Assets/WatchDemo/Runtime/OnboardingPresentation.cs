using UnityEngine;
namespace LighthouseKeepers.WatchDemo {
 public sealed class OnboardingPresentation:MonoBehaviour {
  WatchDemoController watch;Transform head,bed;WatchPipeRepair pipe;TextMesh bunkSlate,pipeSlate;Transform gate;BoxCollider gateCollider;bool opened;int night;
  public static void Create(WatchDemoController w,Transform h,Transform b,WatchPipeRepair p){var c=w.gameObject.AddComponent<OnboardingPresentation>();c.watch=w;c.head=h;c.bed=b;c.pipe=p;c.Build();}
  void Build(){var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();bunkSlate=Slate(parts,"Bunk job slate",bed.position+new Vector3(0,.8f,1.11f),bed.rotation*Quaternion.Euler(0,180,0));pipeSlate=Slate(parts,"Pipe room job slate",pipe.bolts[1].position+new Vector3(.09f,.5f,.45f),Quaternion.LookRotation(Vector3.left));
   // Small latch door only spans the existing House-to-tower threshold. No forced player movement.
   gate=M2StationParts.Root("Tutorial threshold gate",transform,new Vector3(-.68f,0,-4.43f));var leaf=parts.Part("Timber tutorial door",gate,new Vector3(.68f,1.05f,0),new Vector3(1.36f,2.1f,.045f),parts.Dark);gateCollider=leaf.AddComponent<BoxCollider>();
  }
  TextMesh Slate(M2StationParts parts,string name,Vector3 pos,Quaternion rot){var t=M2StationParts.Root(name,transform,Vector3.zero);t.SetPositionAndRotation(pos,rot);parts.Part(name+" backing",t,Vector3.zero,new Vector3(.62f,.20f,.025f),parts.Dark);return parts.Text(name+" chalk","",t,new Vector3(0,0,.014f),.004f);}
  void Update(){var s=watch.State;if(night!=s.Day){night=s.Day;opened=!s.Tutorial;}if(watch.WrenchHeld||s.Elapsed>=s.Master.gateTimeoutSeconds)opened=true;
   gateCollider.enabled=!opened;gate.localRotation=Quaternion.Slerp(gate.localRotation,Quaternion.Euler(0,opened?-90:0,0),1-Mathf.Exp(-4*Time.deltaTime));
   string text=s.Director.Instruction(watch.CurrentInstruction);bunkSlate.text=text;pipeSlate.text=text;
  }
 }
}