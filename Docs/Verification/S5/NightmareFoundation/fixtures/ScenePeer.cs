using System;using System.Linq;using UnityEngine;using UnityEditor;using UnityEngine.XR;using UnityEngine.XR.Interaction.Toolkit;using UnityEngine.XR.Interaction.Toolkit.Interactables;using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Gameplay.Interaction;
public static class ScenePeer{
 static WatchDemoController W=>UnityEngine.Object.FindAnyObjectByType<WatchDemoController>();
 static HandRig Hand=>UnityEngine.Object.FindObjectsByType<HandRig>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(h=>h.Node==XRNode.RightHand);
 public static object Facts()=>new{day=W.State.Day,beat=(int)W.State.Scare.Beat,door=W.State.Scare.Door(0),duty=(int)W.CurrentDuty,down=W.State.Recovery.Downed((int)W.CurrentDuty),progress=Mathf.Max(W.State.Recovery.Slot(1).progress,W.State.Recovery.Slot(2).progress),head=Camera.main.transform.position.ToString("F3"),valid=W.State.Capture(W.WaterHeight).Valid()};
 public static object Prepare(string kind){EditorUtility.audioMasterMute=true;var rig=UnityEngine.Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();foreach(var b in rig.GetComponentsInChildren<Behaviour>(true))if(b.GetType().Name.Contains("TrackedPose")||b.GetType().Name.Contains("InputModality")||b.GetType().Name=="GravityProvider"||b.GetType().Name=="GroundedSlopeMovement")b.enabled=false;W.GetComponent<IslandSafety>().enabled=false;
 var target=kind=="door"?W.GetComponent<NightmareScene>().DoorTarget(0):W.GetComponent<CrewRecovery>().Target((int)LighthouseKeepers.Lobby.CrewDuties.Other(W.CurrentDuty));
 rig.MoveCameraToWorldLocation(target.transform.position+Vector3.up*.55f-Vector3.forward*.4f);
 Hand.transform.parent.gameObject.SetActive(true);Hand.enabled=false;Hand.transform.parent.position=target.transform.position;Hand.Direct.enabled=true;Hand.Direct.selectInput=new XRInputButtonReader("Scene grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);Hand.Direct.selectInput.QueueManualState(false,0);Physics.SyncTransforms();return true;}
 public static object Press(){Hand.Direct.selectInput.QueueManualState(true,1);return true;}
 public static object Held()=>Hand.Direct.hasSelection;
 public static object Pull(){Hand.transform.parent.position+=Vector3.back*.10f;Physics.SyncTransforms();return true;}
 public static object Release(){Hand.Direct.selectInput.QueueManualState(false,0);return true;}
 public static object Escape(){var c=W.State.Scare;for(int n=0;n<200&&c.Beat!=ScareBeat.Escape;n++)c.Tick(.1f,true,true,0,0);return Facts();}
 public static object CatchGuest(){int victim=(int)LighthouseKeepers.Lobby.CrewDuties.Other(W.CurrentDuty);W.State.Recovery.Down(victim);W.State.Scare.Catch(victim,new Vector3(1,3.3f,0));return Facts();}
 public static object Outside(){W.State.Scare.Tick(.1f,false,false,(int)W.CurrentDuty,(int)LighthouseKeepers.Lobby.CrewDuties.Other(W.CurrentDuty));return Facts();}
}
