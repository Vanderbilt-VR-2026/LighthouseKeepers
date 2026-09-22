using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using LighthouseKeepers.Core;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Progression;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
namespace LighthouseKeepers.Editor {
[InitializeOnLoad]
public static class LighthousePlayVerification {
 static int movementStage;static double movementStart;static Vector3 savedPosition;static Quaternion savedRotation;static ContinuousMoveProvider testedMove;static SnapTurnProvider testedTurn;static XRInputValueReader<Vector2> savedMove,savedTurn;
 static double start;static bool running;static int shot=-1,shotFrame;static Camera preview;static RenderTexture targetTexture;static List<string> errors=new();
 static LighthousePlayVerification(){EditorApplication.playModeStateChanged+=State;}
 public static void Run(){SessionState.SetBool("LKVerification",true);EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));EditorApplication.EnterPlaymode();}
 static void State(PlayModeStateChange state){if(!SessionState.GetBool("LKVerification",false))return;if(state==PlayModeStateChange.EnteredPlayMode){EditorApplication.LockReloadAssemblies();running=true;start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;EditorApplication.update+=Tick;}}
 static void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+trace);}
 static readonly string[] shotNames={"Entrance","Plumbing","Generator","Lantern","Exterior"};
 static readonly Vector3[] positions={new(-9.5f,1.7f,0),new(1.8f,1.65f,-2.25f),new(1.6f,4.85f,-2.2f),new(3.5f,11.25f,1.5f),new(-19,10,-22)};
 static readonly Vector3[] targets={new(-3,1.6f,0),new(-.7f,1.1f,-3.7f),new(-.4f,4.1f,-3.6f),new(0,11.2f,0),new(-1,5,0)};
 static void Tick(){
 if(!running||EditorApplication.timeSinceStartup-start<15)return;
 if(movementStage<3){
 try{
 var rig=UnityEngine.Object.FindAnyObjectByType<XROrigin>();
 if(movementStage==0){
  testedMove=rig.GetComponentInChildren<ContinuousMoveProvider>();testedTurn=rig.GetComponentInChildren<SnapTurnProvider>();
  if(!testedMove.leftHandMoveInput.inputActionReference.action.enabled)throw new Exception("Left movement action is disabled");
  if(!testedTurn.rightHandTurnInput.inputActionReference.action.enabled)throw new Exception("Right snap action is disabled");
  savedPosition=rig.transform.position;savedRotation=rig.transform.rotation;savedMove=testedMove.leftHandMoveInput;savedTurn=testedTurn.rightHandTurnInput;
  testedMove.leftHandMoveInput=new XRInputValueReader<Vector2>("Verification move",XRInputValueReader.InputSourceMode.ManualValue){manualValue=Vector2.up};movementStart=EditorApplication.timeSinceStartup;movementStage=1;return;
 }
 if(EditorApplication.timeSinceStartup-movementStart<1)return;
 if(movementStage==1){
  float distance=Vector3.ProjectOnPlane(rig.transform.position-savedPosition,Vector3.up).magnitude;
  if(distance<.3f)throw new Exception("Continuous locomotion did not move at spawn: "+distance+"m");
  testedMove.leftHandMoveInput=savedMove;testedTurn.rightHandTurnInput=new XRInputValueReader<Vector2>("Verification snap",XRInputValueReader.InputSourceMode.ManualValue){manualValue=Vector2.right};movementStart=EditorApplication.timeSinceStartup;movementStage=2;return;
 }
 if(Quaternion.Angle(savedRotation,rig.transform.rotation)<30)throw new Exception("Snap turn did not rotate rig");
 testedTurn.rightHandTurnInput=savedTurn;var cc=rig.GetComponent<CharacterController>();cc.enabled=false;rig.transform.SetPositionAndRotation(savedPosition,savedRotation);cc.enabled=true;movementStage=3;
 }catch(Exception e){errors.Add(e.ToString());Finish();return;}
 }
 if(shot>=0){if(Time.frameCount-shotFrame<30)return;RenderTexture.active=targetTexture;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("Docs/Verification/Previews/"+shotNames[shot]+".png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);RenderTexture.active=null;shot++;
 if(shot<shotNames.Length){preview.transform.position=positions[shot];preview.transform.LookAt(targets[shot]);shotFrame=Time.frameCount;return;}
 UnityEngine.Object.DestroyImmediate(preview.gameObject);UnityEngine.Object.DestroyImmediate(targetTexture);Finish();return;}

 try{
  var bootstrap=UnityEngine.Object.FindAnyObjectByType<EnvironmentBootstrap>();if(!bootstrap||!bootstrap.Ready)throw new Exception("Additive bootstrap not ready");
  if(SceneManager.sceneCount!=7)throw new Exception("Expected seven loaded scenes, got "+SceneManager.sceneCount);
  if(UnityEngine.Object.FindObjectsByType<XROrigin>().Length!=1)throw new Exception("Duplicate XR Origins");
  var origin=UnityEngine.Object.FindAnyObjectByType<XROrigin>();if(origin.transform.position.y < -.1f || origin.transform.position.y > .5f)throw new Exception("Spawn left safe floor: "+origin.transform.position);
  var flood=UnityEngine.Object.FindAnyObjectByType<FloodController>();flood.SetHeight(7);var thresholds=UnityEngine.Object.FindObjectsByType<FloodThreshold>();if(thresholds.Count(t=>t.Submerged)!=3)throw new Exception("Flood threshold propagation failed");flood.ResetFlood();if(thresholds.Any(t=>t.Submerged))throw new Exception("Flood threshold reset failed");
  var days=UnityEngine.Object.FindAnyObjectByType<DayDirector>();days.ApplyDay(5);if(Mathf.Abs(flood.RiseSpeed-days.Current.FloodRiseRate)>.00001f)throw new Exception("Day flood application failed");days.ApplyDay(1);
  Directory.CreateDirectory("Docs/Verification/Previews");
  UnityEngine.Rendering.Universal.UniversalRenderPipelineDebugDisplaySettings.Instance.Reset();
  preview=new GameObject("Warmed verification camera").AddComponent<Camera>();preview.fieldOfView=75;preview.nearClipPlane=.05f;preview.farClipPlane=250;preview.clearFlags=CameraClearFlags.Skybox;targetTexture=new RenderTexture(1280,720,24);preview.targetTexture=targetTexture;shot=0;shotFrame=Time.frameCount;preview.transform.position=positions[0];preview.transform.LookAt(targets[0]);

 }catch(Exception e){errors.Add(e.ToString());Finish();}
 }
 static void Finish(){running=false;
 File.WriteAllText("Docs/Verification/PlayMode.txt",errors.Count==0?"PASS: seven additive scenes loaded; one XR Origin; enabled stick actions and injected-input continuous movement/snap turn; flood thresholds and reset; Day 5/Day 1 application; five rendered previews. No runtime errors observed during 15 seconds. Headset comfort and 72Hz require device checks.\n":string.Join("\n",errors));
 SessionState.SetBool("LKVerification",false);Application.logMessageReceived-=Log;EditorApplication.update-=Tick;EditorApplication.UnlockReloadAssemblies();EditorApplication.Exit(errors.Count==0?0:1);
 }
 static void Capture(string name,Vector3 position,Vector3 target){var go=new GameObject("Verification camera");var camera=go.AddComponent<Camera>();camera.enabled=false;camera.transform.position=position;camera.transform.LookAt(target);camera.fieldOfView=75;camera.nearClipPlane=.05f;camera.farClipPlane=250;camera.backgroundColor=RenderSettings.fogColor;camera.clearFlags=CameraClearFlags.Skybox;var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("Docs/Verification/Previews/"+name+".png",image.EncodeToPNG());RenderTexture.active=null;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);}
}}
