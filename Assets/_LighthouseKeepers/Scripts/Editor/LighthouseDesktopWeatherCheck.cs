using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Unity.XR.CoreUtils;
namespace LighthouseKeepers.Editor
{
 public static class LighthouseDesktopWeatherCheck
 {
  static int frame;static int stage;static double started;static Vector3 position;static Quaternion rotation;static Keyboard keyboard;static bool added;static XROrigin rig;static int count;
  public static bool Tick()
  {
   if(stage==5)return true;
   if(stage==0){
    rig=UnityEngine.Object.FindAnyObjectByType<XROrigin>();position=rig.transform.position;rotation=rig.transform.rotation;
    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;keyboard=InputSystem.AddDevice<Keyboard>();added=true;keyboard.MakeCurrent();
    frame=Time.frameCount;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));started=EditorApplication.timeSinceStartup;stage=1;return false;
   }
   if(EditorApplication.timeSinceStartup-started<1)return false;
   if(stage==1){
    if(Vector3.ProjectOnPlane(rig.transform.position-position,Vector3.up).magnitude<.3f)throw new Exception("Desktop W did not walk: keyboard="+keyboard.enabled+", pressed="+keyboard.wKey.isPressed+", position="+rig.transform.position+", start="+position+", preview="+rig.GetComponent<LighthouseKeepers.Player.DesktopPreview>()+", XR="+UnityEngine.XR.XRSettings.isDeviceActive+", currentKeyboard="+Keyboard.current?.deviceId+", injectedKeyboard="+keyboard.deviceId+", frames="+(Time.frameCount-frame)+", enabled="+rig.GetComponent<LighthouseKeepers.Player.DesktopPreview>().enabled+", read="+rig.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.ContinuousMoveProvider>().leftHandMoveInput.ReadValue());
    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));stage=2;started=EditorApplication.timeSinceStartup;return false;
   }
   if(stage==2){
    if(Quaternion.Angle(rotation,rig.transform.rotation)<30)throw new Exception("Desktop E did not turn");
    InputSystem.QueueStateEvent(keyboard,new KeyboardState());
    Move(new Vector3(6,9.6f,0),rotation);stage=3;started=EditorApplication.timeSinceStartup;return false;
   }
   var rain=GameObject.Find("Recycled balcony rain").GetComponent<ParticleSystem>();
   if(stage==3){
    count=rain.particleCount;if(!rain.emission.enabled||count==0||count>220)throw new Exception("Balcony rain did not activate within budget");
    var particles=new ParticleSystem.Particle[220];int n=rain.GetParticles(particles);
    for(int i=0;i<n;i++)if(particles[i].remainingLifetime>0&&LighthouseKeepers.Environment.WeatherVolume.IsSheltered(particles[i].position))throw new Exception("Balcony rain entered shelter");
    Move(position,rotation);stage=4;started=EditorApplication.timeSinceStartup;return false;
   }
   if(rain.emission.enabled||rain.particleCount!=0)throw new Exception("Balcony rain did not clear indoors");
   if(added)InputSystem.RemoveDevice(keyboard);
   File.WriteAllText("Docs/Verification/DesktopWeatherPlayMode.txt","PASS: synthetic keyboard W walks and E snap-turns through DesktopPreview; balcony rain activates at x=6,y=9.6 with "+count+" particles, no live particles in shelter, stops and clears on returning indoors. This is automated input, not a human headset/keyboard audition.\n");
   stage=5;return true;
  }
  static void Move(Vector3 p,Quaternion q){var cc=rig.GetComponent<CharacterController>();cc.enabled=false;rig.transform.SetPositionAndRotation(p,q);cc.enabled=true;}
 }
}
