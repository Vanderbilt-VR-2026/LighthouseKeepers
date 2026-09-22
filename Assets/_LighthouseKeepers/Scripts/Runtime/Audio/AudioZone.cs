using UnityEngine;
namespace LighthouseKeepers.Audio {
[RequireComponent(typeof(BoxCollider))]
public sealed class AudioZone:MonoBehaviour {
 [SerializeField] string snapshot="Interior Normal";
 [SerializeField] bool exterior;
 [SerializeField] int priority;
 BoxCollider volume;
 public int Priority=>priority;public string Snapshot=>snapshot;public bool Exterior=>exterior;
 void Awake(){volume=GetComponent<BoxCollider>();volume.isTrigger=true;}
 public bool Contains(Vector3 point)=>volume&&volume.bounds.Contains(point);
 void OnDrawGizmosSelected(){var box=GetComponent<BoxCollider>();Gizmos.color=new Color(.2f,.7f,1,.2f);Gizmos.matrix=transform.localToWorldMatrix;Gizmos.DrawCube(box.center,box.size);}
}}
