using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using LighthouseKeepers.Player;
namespace LighthouseKeepers.Core {
[DefaultExecutionOrder(-500)]
public sealed class EnvironmentBootstrap : MonoBehaviour {
 [SerializeField] string[] scenes = {"LK_Exterior","LK_Core","LK_Level01_Plumbing","LK_Level02_Generator","LK_Level03_Communications","LK_Level04_Lantern"};
 public bool Ready {get;private set;}
 XROrigin rig;CharacterController character;Vector3 spawn;
 readonly List<(Behaviour component,bool enabled)> movement=new();
 void Awake(){
  rig=FindAnyObjectByType<XROrigin>();if(!rig)return;spawn=rig.transform.position;character=rig.GetComponent<CharacterController>();
  foreach(var component in rig.GetComponentsInChildren<Behaviour>(true))if(component is LocomotionProvider||component is DesktopPreview||component is PlayerComfort){movement.Add((component,component.enabled));component.enabled=false;}
  if(character)character.enabled=false;
 }
 IEnumerator Start() {
  foreach(var scene in scenes) if(!SceneManager.GetSceneByName(scene).isLoaded) yield return SceneManager.LoadSceneAsync(scene,LoadSceneMode.Additive);
  SceneManager.SetActiveScene(SceneManager.GetSceneByName("LK_Exterior"));
  Physics.SyncTransforms();if(rig)rig.transform.position=spawn;if(character)character.enabled=true;
  foreach(var item in movement)if(item.component)item.component.enabled=item.enabled;
  Ready=true;
  Debug.Log("LK_READY: six content scenes loaded; movement enabled on authored spawn. Position="+(rig?rig.transform.position.ToString():"missing rig"));
 }
}}
