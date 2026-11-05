using LighthouseKeepers.Core;
using UnityEngine;using UnityEngine.XR;using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo {
 public sealed class PressurePresentation:MonoBehaviour {
  WatchDemoController watch;Transform head;TextMesh clock;AudioSource horn;float nextHorn;int minute=-1;
  public static void Create(WatchDemoController w,Transform h){var p=w.gameObject.AddComponent<PressurePresentation>();p.watch=w;p.head=h;p.Build();}
  void Build(){var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();
   foreach(var hand in FindObjectsByType<HandRig>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(hand.Node==XRNode.LeftHand){var mount=M2StationParts.Root("Keeper wristwatch",hand.transform,new Vector3(0,-.045f,-.055f));mount.localRotation=Quaternion.Euler(90,0,0);parts.Part("Watch brass case",mount,Vector3.zero,new Vector3(.072f,.062f,.012f),parts.Brass);parts.Part("Watch dark dial",mount,new Vector3(0,0,.008f),new Vector3(.063f,.051f,.007f),parts.Dark);clock=parts.Text("Time to dawn","",mount,new Vector3(0,0,.013f),.0019f);}
   foreach(var mark in new[]{("mark.knee",.25f),("mark.generator",.42f)}) {
    var from=new Vector3(0,mark.Item2,1.3f);RaycastHit best=default;float distance=20;
    foreach(var hit in Physics.RaycastAll(from,Vector3.right,5,~0,QueryTriggerInteraction.Ignore))if(hit.collider.name.Contains("Wall")&&hit.distance<distance){best=hit;distance=hit.distance;}
    if(distance<20){var t=parts.Text(KeeperStrings.Get(mark.Item1),KeeperStrings.Get(mark.Item1),transform,Vector3.zero,.004f);t.transform.SetPositionAndRotation(best.point+best.normal*.012f,Quaternion.LookRotation(-best.normal));}
   }
   horn=new GameObject("Approaching ship horn").AddComponent<AudioSource>();horn.transform.SetParent(transform,false);horn.transform.position=new Vector3(10,3,0);horn.playOnAwake=false;horn.spatialBlend=1;horn.minDistance=3;horn.maxDistance=30;horn.clip=Resources.Load<AudioClip>("ImmersionAudio/foghorn");KeeperAudioMix.Route(horn,"SFX_Critical");
  }
  void Update(){var s=watch.State;if(clock){int left=Mathf.Max(0,(int)(s.Duration-s.Elapsed));clock.text=$"{left/60:00}:{left%60:00}";}
   if(s.Failed||s.Complete)return;
   int m=(int)((s.Duration-s.Elapsed)/60);bool toll=m<2&&m!=minute;minute=m;
   if(toll||!s.BeaconShining&&Time.time>=nextHorn){nextHorn=Time.time+10;horn.transform.position=head.position+Vector3.right*Mathf.Lerp(22,5,s.BeaconDanger);horn.volume=Mathf.Lerp(.08f,.25f,s.BeaconDanger);if(horn.clip)horn.Play();}
  }
 }
}