using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace LighthouseKeepers.WatchDemo
{
 // Solo-playtest collision fixes for the restored Lab shell, applied once per Lab load.
 // Runtime-only: the generated shell FBX/prefabs and Lab scene are not regenerated or saved.
 // Visual z-fight nudges live in the shell prefabs instead, because shell renderers are static-batched
 // (moving their transforms at runtime would not move what is drawn). Colliders are not batched.
 public sealed class PlaytestWorldFixes:MonoBehaviour
 {
  public const string SceneName="LK_Lab_Environment",ObjectName="Playtest world fixes",GuardPrefix="Playtest edge guard ",WrenchName="Grab wrench — grip to hold";
  [Serializable] public struct EdgeGuard
  {
   public string label;
   [Tooltip("Walking surface Y of the floor that owns the open edge, metres.")] public float floorY;
   [Tooltip("Azimuth of the radial edge, degrees (0 = +Z, 90 = +X; shell.py polar()).")] public float azimuthDeg;
   [Min(0)] public float innerRadius,outerRadius;
   [Tooltip("-1: the drop lies at lower azimuth; +1: at higher azimuth. The guard sits entirely on the drop side.")] public int dropSide;
  }
  // F3/F4 floors are cut between the stair arrival/departure azimuths (shell.py room()/floor_mesh()).
  // Only the stair arrival edge needs to stay open; these edges fall 3.3 m to the floor below.
  [SerializeField] EdgeGuard[] guards={
   new EdgeGuard{label="F3 320° stairwell edge",floorY=6.6f,azimuthDeg=320,innerRadius=2.44f,outerRadius=3.66f,dropSide=-1},
   new EdgeGuard{label="F4 320° stairwell edge",floorY=9.9f,azimuthDeg=320,innerRadius=1.15f,outerRadius=2.75f,dropSide=-1},
   new EdgeGuard{label="F4 140° outer stair gap",floorY=9.9f,azimuthDeg=140,innerRadius=2.26f,outerRadius=2.75f,dropSide=1}};
  [SerializeField,Range(.9f,1.5f),Tooltip("Guard height above the floor, metres.")] float guardHeight=1.1f;
  [SerializeField,Range(.02f,.2f),Tooltip("Guard thickness, metres.")] float guardThickness=.08f;
  [SerializeField,Range(0,.02f),Tooltip("Exterior rock collider tops intruding through interior floors are lowered to this depth below Y0, metres.")] float rockClearance=.002f;
  [SerializeField,Range(0,.2f),Tooltip("Rock colliders whose top is at or below this height (shallow plates) are candidates, metres.")] float rockPlateMaxTop=.1f;
  [SerializeField,Tooltip("Keep the player capsule from stepping onto the (dropped or held) wrench.")] bool playerIgnoresWrench=true;
  public int GuardsBuilt{get;private set;}
  public int RocksLowered{get;private set;}
  public int WrenchPairsIgnored{get;private set;}
  public bool Applied{get;private set;}
  public float GuardHeight=>guardHeight;
  public EdgeGuard[] Guards=>guards;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Install()
  {SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
  static void Loaded(Scene scene,LoadSceneMode unused)
  {
   if(!scene.IsValid()||!scene.isLoaded||scene.name!=SceneName)return;
   foreach(var root in scene.GetRootGameObjects())if(root.name==ObjectName)return;
   var go=new GameObject(ObjectName);SceneManager.MoveGameObjectToScene(go,scene);go.AddComponent<PlaytestWorldFixes>();
  }
  IEnumerator Start(){yield return null;Apply(gameObject.scene);} // after every scene Start (wrench/rig setup)

  public void Apply(Scene scene)
  {
   if(Applied)return;Applied=true;
   foreach(var g in guards)BuildGuard(g);
   foreach(var root in scene.GetRootGameObjects())
   {
    if(root==gameObject)continue;
    foreach(var c in root.GetComponentsInChildren<Collider>(true))
     if(c.name.StartsWith("UCX_SM_Rock_",StringComparison.Ordinal)&&LowerIntrudingRock(c))RocksLowered++;
   }
   if(playerIgnoresWrench)
   {
    Transform wrench=null;CharacterController[] bodies=null;
    foreach(var root in scene.GetRootGameObjects())
    {
     if(!wrench)foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==WrenchName){wrench=t;break;}
     var found=root.GetComponentsInChildren<CharacterController>(true);if(found.Length>0)bodies=bodies==null?found:Concat(bodies,found);
    }
    if(wrench&&bodies!=null)foreach(var body in bodies)foreach(var c in wrench.GetComponentsInChildren<Collider>(true)){Physics.IgnoreCollision(body,c,true);WrenchPairsIgnored++;}
   }
   Physics.SyncTransforms();
  }
  void BuildGuard(EdgeGuard g)
  {
   float a=g.azimuthDeg*Mathf.Deg2Rad;var radial=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));var tangent=new Vector3(Mathf.Cos(a),0,-Mathf.Sin(a));
   var go=new GameObject(GuardPrefix+g.label);go.transform.SetParent(transform,false);
   go.transform.SetPositionAndRotation(radial*(g.innerRadius+g.outerRadius)*.5f+tangent*(Mathf.Sign(g.dropSide)*guardThickness*.5f)+Vector3.up*(g.floorY+guardHeight*.5f),Quaternion.LookRotation(radial,Vector3.up));
   // Collision only: the visible stair rails/guards remain the authored shell art.
   go.AddComponent<BoxCollider>().size=new Vector3(guardThickness,guardHeight,g.outerRadius-g.innerRadius);
   GuardsBuilt++;
  }
  // Exterior outcrop collision hulls are capped at Y0.05 and poke through the House/F1 floors,
  // which made the player capsule step up 5 cm (the head "pop" around the wrench bench).
  bool LowerIntrudingRock(Collider c)
  {
   var b=c.bounds;if(b.max.y<=0||b.max.y>rockPlateMaxTop||!UnderInterior(b))return false;
   c.transform.position+=Vector3.down*(b.max.y+rockClearance);return true;
  }
  static bool UnderInterior(Bounds b)
  {
   bool Rect(float x0,float x1,float z0,float z1)=>b.max.x>x0&&b.min.x<x1&&b.max.z>z0&&b.min.z<z1;
   if(Rect(-3.95f,3.95f,-10.3f,-4.4f)||Rect(-.85f,.85f,-5.55f,-3.85f))return true; // House, throat
   float cx=Mathf.Clamp(0,b.min.x,b.max.x),cz=Mathf.Clamp(0,b.min.z,b.max.z);return cx*cx+cz*cz<3.9f*3.9f; // F1 room
  }
  static CharacterController[] Concat(CharacterController[] a,CharacterController[] b){var r=new CharacterController[a.Length+b.Length];a.CopyTo(r,0);b.CopyTo(r,a.Length);return r;}
 }
}
