using System.Collections.Generic;using UnityEngine;using Unity.XR.CoreUtils;
namespace LighthouseKeepers.WatchDemo {
 public sealed class MasterWorldSafety:MonoBehaviour {
  public readonly List<string> Changes=new();Mesh railMesh;readonly List<Mesh> pieces=new();
  public static void Create(WatchDemoController watch){var s=watch.gameObject.AddComponent<MasterWorldSafety>();s.Apply();}
  void Apply(){
   var rig=FindAnyObjectByType<XROrigin>();var character=rig.GetComponent<CharacterController>();if(character)character.stepOffset=.10f;
   foreach(var collider in FindObjectsByType<Collider>(FindObjectsSortMode.None)) {
    var b=collider.bounds;
    if(collider.name.StartsWith("UCX_SM_Rock_Outcrop")&&b.max.y>0&&b.max.y<.2f&&b.center.z< -5&&Mathf.Abs(b.center.x)<3.5f){collider.enabled=false;Changes.Add("Nonblocking House rock: "+collider.name);}
   }
   // Coplanar shell offsets are authored by LighthouseBuilder, before static batching.
   var seen=new Dictionary<string,MeshRenderer>();
   foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)) {
    if(!renderer.enabled||renderer.isPartOfStaticBatch||renderer.GetComponentInParent<LODGroup>()||!renderer.name.StartsWith("SM_"))continue;var filter=renderer.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)continue;
    string key=filter.sharedMesh.GetInstanceID()+"/"+renderer.transform.localToWorldMatrix.ToString("F4");if(seen.ContainsKey(key)){renderer.enabled=false;Changes.Add("Exact duplicate renderer disabled: "+renderer.name);}else seen[key]=renderer;
   }
   GalleryGuard();Physics.SyncTransforms();
  }
  void GalleryGuard(){var root=new GameObject("Continuous gallery perimeter guard");root.transform.SetParent(transform,false);var parts=root.AddComponent<M2StationParts>();parts.Initialize();var combine=new List<CombineInstance>();
   for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;var centre=new Vector3(Mathf.Sin(a)*4.17f,10.45f,Mathf.Cos(a)*4.17f);var segment=new GameObject("Gallery edge collision "+i);segment.transform.SetParent(root.transform,false);segment.transform.SetPositionAndRotation(centre,Quaternion.Euler(0,a*Mathf.Rad2Deg,0));var box=segment.AddComponent<BoxCollider>();box.size=new Vector3(.43f,1.12f,.07f);
    foreach(float y in new[]{-.38f,.48f}) {var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var filter=primitive.GetComponent<MeshFilter>();var matrix=Matrix4x4.TRS(centre+Vector3.up*y,segment.transform.rotation,new Vector3(.43f,.045f,.045f));combine.Add(new CombineInstance{mesh=filter.sharedMesh,transform=matrix});primitive.SetActive(false);Destroy(primitive);}
   }
   railMesh=new Mesh{name="Single draw gallery safety rails"};railMesh.CombineMeshes(combine.ToArray(),true,true);var go=new GameObject("Gallery brass safety rails");go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=railMesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=parts.Metal;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;Changes.Add("64 overlapping perimeter collision segments; one combined visible railing mesh");
  }
  void Start(){
   // Surveyed with a 0.25 m floor grid and full-height outward capsule sweeps.
   // Keep the stair route itself open; guard only the unsupported sides of its arrival cut-outs.
   var samples=new[]{(new Vector3(-2.05f,6.6f,2.45f),Vector3.back),(new Vector3(-2.05f,6.6f,2.7f),Vector3.back),(new Vector3(-1.55f,9.9f,1.95f),Vector3.back),(new Vector3(-1.3f,9.9f,1.7f),Vector3.left),(new Vector3(-1.3f,9.9f,1.7f),Vector3.back)};
   var parts=GetComponent<M2StationParts>()??gameObject.AddComponent<M2StationParts>();parts.Initialize();
   foreach(var sample in samples){var t=M2StationParts.Root("Surveyed stair opening guard",transform,Vector3.zero);t.position=sample.Item1+sample.Item2*.44f+Vector3.up*.55f;t.rotation=Quaternion.LookRotation(sample.Item2);var c=t.gameObject.AddComponent<BoxCollider>();c.size=new Vector3(.52f,1.1f,.04f);
    foreach(float y in new[]{-.35f,.45f})parts.Part("Fixed brass guard rail",t,new Vector3(0,y,0),new Vector3(.52f,.035f,.035f),parts.Metal);
    parts.Part("Fixed guard upright",t,new Vector3(.23f,0,0),new Vector3(.035f,1.1f,.035f),parts.Metal);
   }Physics.SyncTransforms();
  }
  void OnDestroy(){if(railMesh)Destroy(railMesh);foreach(var m in pieces)if(m)Destroy(m);}
 }
}