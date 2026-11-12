using LighthouseKeepers.Core;
using TMPro;
using UnityEngine;using UnityEngine.XR;using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class PressurePresentation:MonoBehaviour {
  WatchDemoController watch;Transform head;TextMesh clock;TMP_Text objective;AudioSource horn;float nextHorn;int minute=-1,shownSecond=-1;bool shownTutorial;
  public static void Create(WatchDemoController w,Transform h){var p=w.gameObject.AddComponent<PressurePresentation>();p.watch=w;p.head=h;p.Build();}
  void Build(){var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();
   BuildWatch(parts);
   foreach(var mark in new[]{("mark.knee",.25f),("mark.generator",.42f)}) {
    var from=new Vector3(0,mark.Item2,1.3f);RaycastHit best=default;float distance=20;
    foreach(var hit in Physics.RaycastAll(from,Vector3.right,5,~0,QueryTriggerInteraction.Ignore))if(hit.collider.name.Contains("Wall")&&hit.distance<distance){best=hit;distance=hit.distance;}
    if(distance<20){var t=parts.Text(KeeperStrings.Get(mark.Item1),KeeperStrings.Get(mark.Item1),transform,Vector3.zero,.004f);t.transform.SetPositionAndRotation(best.point+best.normal*.012f,Quaternion.LookRotation(-best.normal));parts.Part("Painted waterline stripe",t.transform,new Vector3(0,-.035f,.004f),new Vector3(.45f,.012f,.003f),parts.Brass);}
   }
   horn=new GameObject("Approaching ship horn").AddComponent<AudioSource>();horn.transform.SetParent(transform,false);horn.transform.position=new Vector3(10,3,0);horn.playOnAwake=false;horn.spatialBlend=1;horn.minDistance=3;horn.maxDistance=30;horn.clip=Resources.Load<AudioClip>("ImmersionAudio/foghorn");KeeperAudioMix.Route(horn,"SFX_Critical");
  }
  void Update(){var s=watch.State;UpdateWatch(s);
   if(s.Failed||s.Complete)return;
   int m=(int)((s.Duration-s.Elapsed)/60);bool toll=m<2&&m!=minute;minute=m;
   if(toll||!s.BeaconShining&&Time.time>=nextHorn){nextHorn=Time.time+10;horn.transform.position=head.position+Vector3.right*Mathf.Lerp(22,5,s.BeaconDanger);horn.volume=Mathf.Lerp(.08f,.25f,s.BeaconDanger);if(horn.clip)horn.Play();}
  }
 }
}