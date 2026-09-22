using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
namespace LighthouseKeepers.Player {
public sealed class RecenterPlayer:MonoBehaviour {
 [ContextMenu("Recenter tracking")]
 public void Recenter(){var displays=new List<XRInputSubsystem>();SubsystemManager.GetSubsystems(displays);foreach(var input in displays)input.TryRecenter();}
}}
