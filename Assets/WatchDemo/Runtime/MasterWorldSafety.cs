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
   // House slab and throat slab overlap at the same authored y=0. Offset only the visual.
   foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(renderer.name=="SM_Floor_Throat_LOD0"||renderer.name=="SM_Floor_Throat") {
    // Batched renderers share a combined mesh and ignore individual transform offsets.
    // Replace this one visible slab while retaining its original collision and shared material.
    var bounds=renderer.bounds;var slab=GameObject.CreatePrimitive(PrimitiveType.Cube);slab.name="Separated throat floor visual";slab.transform.SetParent(transform,true);slab.transform.position=bounds.center+Vector3.up*.012f;slab.transform.localScale=bounds.size;var collider=slab.GetComponent<Collider>();collider.enabled=false;Destroy(collider);slab.GetComponent<Renderer>().sharedMaterial=renderer.sharedMaterial;renderer.enabled=false;Changes.Add("Throat floor replacement visual +12 mm: "+renderer.name);
   }
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
  void OnDestroy(){if(railMesh)Destroy(railMesh);foreach(var m in pieces)if(m)Destroy(m);}
 }
}