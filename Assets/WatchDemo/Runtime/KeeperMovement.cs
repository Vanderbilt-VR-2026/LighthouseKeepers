using UnityEngine;using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
namespace LighthouseKeepers.WatchDemo {
 public sealed class KeeperMovement:MonoBehaviour {
  SnapTurnProvider snap;ContinuousTurnProvider smooth;LighthouseKeepers.Player.PlayerComfort comfort;float multiplier;
  public bool Smooth=>smooth&&smooth.enabled;
  public static KeeperMovement Current {get;private set;}
  public static void Create(WatchDemoController watch){var c=watch.gameObject.AddComponent<KeeperMovement>();var rig=FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();c.multiplier=watch.State.Master.walkMultiplier;c.comfort=rig.GetComponentInChildren<LighthouseKeepers.Player.PlayerComfort>();var move=rig.GetComponentInChildren<ContinuousMoveProvider>();if(move&&!c.comfort)move.moveSpeed*=c.multiplier;c.snap=rig.GetComponentInChildren<SnapTurnProvider>();if(c.snap){c.smooth=rig.GetComponentInChildren<ContinuousTurnProvider>(true);if(!c.smooth)c.smooth=c.snap.gameObject.AddComponent<ContinuousTurnProvider>();c.smooth.enabled=false;c.smooth.turnSpeed=45;c.smooth.rightHandTurnInput=c.snap.rightHandTurnInput;c.smooth.leftHandTurnInput=c.snap.leftHandTurnInput;c.smooth.mediator=c.snap.mediator;c.SetSmooth(PlayerPrefs.GetInt("KeeperSmoothTurn",0)==1);}Current=c;PoleSlide.Create(watch,rig);}
  public void Toggle(){SetSmooth(!Smooth);PlayerPrefs.SetInt("KeeperSmoothTurn",Smooth?1:0);PlayerPrefs.Save();}
  void SetSmooth(bool value){if(comfort)comfort.SetWatchComfort(multiplier,value);if(!snap||!smooth)return;snap.enabled=!value;smooth.enabled=value;}
  void OnDestroy(){if(comfort)comfort.SetWatchComfort(1,null);if(Current==this)Current=null;}
 }
}