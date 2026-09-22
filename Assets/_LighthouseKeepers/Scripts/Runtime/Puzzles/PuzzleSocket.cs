using UnityEngine;
using UnityEngine.Events;
namespace LighthouseKeepers.Puzzles {
public sealed class PuzzleSocket:MonoBehaviour {
 [SerializeField] string stationId, interactionRole;
 [SerializeField,TextArea] string futureIntent;
 [SerializeField] Transform interactionAnchor;
 [SerializeField] UnityEvent onInteraction=new();
 public string StationId=>stationId;public string Role=>interactionRole;public Transform Anchor=>interactionAnchor;
 public void SignalInteraction()=>onInteraction.Invoke();
 void OnDrawGizmos(){Gizmos.color=new Color(0.2f,0.9f,0.9f,0.7f);Gizmos.DrawWireSphere(interactionAnchor?interactionAnchor.position:transform.position,0.13f);
 #if UNITY_EDITOR
 UnityEditor.Handles.Label(transform.position+Vector3.up*0.18f,stationId+" / "+interactionRole);
 #endif
 }
}}
