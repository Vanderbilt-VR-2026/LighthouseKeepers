using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using LighthouseKeepers.Core;
using LighthouseKeepers.Player;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Progression;
using LighthouseKeepers.Environment;
using LighthouseKeepers.Audio;
using static LighthouseKeepers.Editor.LighthouseAssets;
namespace LighthouseKeepers.Editor {
public static class LighthouseScenes {
 public static readonly string[] Names={"LK_Bootstrap","LK_Exterior","LK_Core","LK_Level01_Plumbing","LK_Level02_Generator","LK_Level03_Communications","LK_Level04_Lantern","LK_DevGym"};
 public static string ScenePath(string name)=>Root+"/Scenes/"+(name=="LK_Bootstrap"?"Bootstrap":name=="LK_DevGym"?"Development":name.StartsWith("LK_Level")?"Levels":"Environment")+"/"+name+".unity";
 public static void Set(UnityEngine.Object target,string name,object value){var so=new SerializedObject(target);var p=so.FindProperty(name);if(p==null)throw new Exception(target.GetType()+" missing serialized field "+name);switch(value){case UnityEngine.Object o:p.objectReferenceValue=o;break;case float f:p.floatValue=f;break;case int i:p.intValue=i;break;case bool b:p.boolValue=b;break;case string s:p.stringValue=s;break;}so.ApplyModifiedPropertiesWithoutUndo();}
 public static void SetArray(UnityEngine.Object target,string name,UnityEngine.Object[] values){var so=new SerializedObject(target);var p=so.FindProperty(name);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
 static T Asset<T>(string name)where T:ScriptableObject {string path=Root+"/ScriptableObjects/"+name+".asset";var a=AssetDatabase.LoadAssetAtPath<T>(path);if(!a){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a;}
 [MenuItem("Lighthouse Keepers/02 Create Missing Sprint Scenes")]
 public static void Generate(){
  LighthouseAssets.Create();LighthouseAudioAssets.Create();
  for(int i=0;i<5;i++){if(File.Exists(Root+"/ScriptableObjects/Day"+(i+1)+".asset"))continue;var day=Asset<DayProfile>("Day"+(i+1));Set(day,"day",i+1);Set(day,"stormIntensity",.3f+i*.15f);Set(day,"lightningInterval",50f-i*8);Set(day,"thunderProximity",.2f+i*.15f);Set(day,"floodRiseRate",.003f+i*.003f);Set(day,"leakCount",2+i*2);Set(day,"lightFailureFrequency",.05f+i*.1f);Set(day,"machineryInstability",.1f+i*.15f);Set(day,"radioInterference",.1f+i*.15f);Set(day,"puzzleTimePressure",1f+i*.2f);Set(day,"environmentalAudioIntensity",.7f+i*.1f);}
  Asset<FloodProfile>("FloodProfile");Asset<ComfortSettings>("ComfortSettings");
  Action[] build={Bootstrap,LighthouseArchitecture.Exterior,LighthouseArchitecture.Core,LighthouseRooms.Plumbing,LighthouseRooms.Generator,LighthouseRooms.Communications,LighthouseRooms.Lantern,DevGym};
  for(int i=0;i<Names.Length;i++){string path=ScenePath(Names[i]);if(File.Exists(path))continue;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);build[i]();EditorSceneManager.SaveScene(scene,path);}
  EditorBuildSettings.scenes=Names.Select(n=>new EditorBuildSettingsScene(ScenePath(n),true)).ToArray();
  var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tags.FindProperty("layers");layers.GetArrayElementAtIndex(8).stringValue="Environment";layers.GetArrayElementAtIndex(9).stringValue="Interactable";layers.GetArrayElementAtIndex(10).stringValue="PlayerBody";tags.ApplyModifiedPropertiesWithoutUndo();
  AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(ScenePath("LK_Bootstrap"));Debug.Log("LIGHTHOUSE SCENES GENERATED");
 }
 static void Bootstrap(){Rig(new Vector3(-8,.05f,0));var systems=new GameObject("Lighthouse persistent systems");systems.AddComponent<EnvironmentBootstrap>();var flood=systems.AddComponent<FloodController>();Set(flood,"profile",Asset<FloodProfile>("FloodProfile"));var days=systems.AddComponent<DayDirector>();SetArray(days,"profiles",Enumerable.Range(1,5).Select(i=>(UnityEngine.Object)Asset<DayProfile>("Day"+i)).ToArray());systems.AddComponent<DevelopmentDiagnostics>();
  var thunder=LighthouseAudioAssets.Emitter("Storm thunder bus",Vector3.zero,"Thunder","Thunder",.6f,false);thunder.spatialBlend=0;var filter=thunder.gameObject.AddComponent<AudioLowPassFilter>();filter.cutoffFrequency=1600;
  var storm=systems.AddComponent<StormController>();Set(storm,"thunder",thunder);SetArray(storm,"thunderClips",new UnityEngine.Object[]{Clip("Thunder")});
  var flash=new GameObject("Lightning flash").AddComponent<Light>();flash.type=LightType.Directional;flash.intensity=0;flash.shadows=LightShadows.None;flash.color=new Color(.7f,.85f,1);Set(storm,"stormLight",flash);
  var acoustics=systems.AddComponent<AcousticDirector>();Set(acoustics,"mixer",LighthouseAudioAssets.Create());Set(acoustics,"thunderFilter",filter);
 }
 public static GameObject Rig(Vector3 spawn){
  const string sample="Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/";
  var rig=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(sample+"Prefabs/XR Origin (XR Rig).prefab"));PrefabUtility.UnpackPrefabInstance(rig,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);rig.name="LK Quest Player";
  rig.transform.position=spawn;rig.transform.rotation=Quaternion.Euler(0,90,0);var origin=rig.GetComponent<XROrigin>();origin.RequestedTrackingOriginMode=XROrigin.TrackingOriginMode.Floor;
  var camera=origin.Camera;camera.nearClipPlane=.05f;camera.farClipPlane=250;camera.backgroundColor=new Color(.28f,.36f,.41f);camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.localPosition=Vector3.up*1.65f;
  foreach(var t in rig.GetComponentsInChildren<Transform>(true))if(new[]{"Teleportation","Climb","Grab Move","Jump","Climb Teleport"}.Contains(t.name))t.gameObject.SetActive(false);
  var character=rig.GetComponent<CharacterController>();character.radius=.22f;character.height=1.65f;character.center=new Vector3(0,.875f,0);character.stepOffset=.18f;character.slopeLimit=45;character.skinWidth=.03f;
  var move=rig.GetComponentInChildren<ContinuousMoveProvider>(true);move.moveSpeed=1.6f;move.forwardSource=camera.transform;
  if(move is DynamicMoveProvider dynamic){dynamic.leftHandMovementDirection=DynamicMoveProvider.MovementDirection.HeadRelative;dynamic.rightHandMovementDirection=DynamicMoveProvider.MovementDirection.HeadRelative;}
  var snap=rig.GetComponentInChildren<SnapTurnProvider>(true);snap.turnAmount=45;var smooth=rig.GetComponentInChildren<ContinuousTurnProvider>(true);smooth.enabled=false;
  var managers=rig.GetComponentsInChildren<ControllerInputActionManager>(true);foreach(var manager in managers){manager.smoothMotionEnabled=manager.gameObject.name.Contains("Left");manager.smoothTurnEnabled=false;}
  var vignette=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(sample+"TunnelingVignette/TunnelingVignette.prefab"));vignette.transform.SetParent(camera.transform,false);var tunneling=vignette.GetComponent<TunnelingVignetteController>();tunneling.locomotionVignetteProviders.Clear();
  var comfort=rig.AddComponent<PlayerComfort>();Set(comfort,"settings",Asset<ComfortSettings>("ComfortSettings"));Set(comfort,"vignette",tunneling);Set(comfort,"move",move);Set(comfort,"snap",snap);Set(comfort,"smooth",smooth);SetArray(comfort,"inputManagers",managers.Cast<UnityEngine.Object>().ToArray());
  var torso=Box("Stabilized chest proxy",Vector3.zero,new Vector3(.34f,.42f,.18f),"Cloth",rig.transform,false);var body=torso.AddComponent<BodyPresence>();Set(body,"head",camera.transform);
  LighthousePlayerRepair.CreateGloves(rig);
  var preview=rig.AddComponent<DesktopPreview>();Set(preview,"character",character);Set(preview,"head",camera.transform);
  if(!UnityEngine.Object.FindAnyObjectByType<XRInteractionManager>())new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
  string path=Root+"/Prefabs/Player/LK_QuestPlayer.prefab";if(!File.Exists(path))PrefabUtility.SaveAsPrefabAssetAndConnect(rig,path,InteractionMode.AutomatedAction);return rig;
 }
 static void DevGym(){Rig(new Vector3(0,.05f,-4));Box("Locomotion lane",new Vector3(0,-.1f,0),new Vector3(6,.2f,14),"Interior");LighthouseRooms.Table(new Vector3(2,0,0));LighthouseRooms.Grab("Grab test",new Vector3(2,1,0),Vector3.one*.15f,"Brass");foreach(int side in new[]{-1,1})Box("Doorway width 1.1 metres",new Vector3(side*.65f,1.2f,2),new Vector3(.2f,2.4f,.2f),"Wood");var ramp=Box("Stair collision test ramp",new Vector3(0,.3f,4),new Vector3(1.6f,.2f,2),"Slate");ramp.transform.rotation=Quaternion.Euler(-18,0,0);
  var light=new GameObject("Gym light").AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(45,30,0);RenderSettings.ambientLight=Color.gray;
  var debug=new GameObject("Gym flood controls");var flood=debug.AddComponent<FloodController>();Set(flood,"profile",Asset<FloodProfile>("FloodProfile"));var water=Box("Gym flood preview",new Vector3(0,-.25f,0),new Vector3(5,.02f,6),"Water",null,false);water.AddComponent<FloodSurface>();var diag=debug.AddComponent<DevelopmentDiagnostics>();Set(diag,"showOverlay",true);LighthouseAudioAssets.Emitter("Gym enclosed rain",new Vector3(2,1,2),"RainInterior","Weather Interior");
 }
}}
