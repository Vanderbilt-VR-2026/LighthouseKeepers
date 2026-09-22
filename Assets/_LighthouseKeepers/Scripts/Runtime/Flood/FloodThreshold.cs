using UnityEngine;
using UnityEngine.Events;
namespace LighthouseKeepers.Flood {
public sealed class FloodThreshold:MonoBehaviour {
 [SerializeField] float threshold;
 [SerializeField] UnityEvent<bool> onSubmergedChanged=new();
 FloodController flood; bool submerged;
 public float Threshold=>threshold; public bool Submerged=>submerged;
 public UnityEvent<bool> OnSubmergedChanged=>onSubmergedChanged;
 void OnEnable(){flood=FindAnyObjectByType<FloodController>();if(flood){flood.HeightChanged+=Evaluate;Evaluate(flood.Height);}}
 void OnDisable(){if(flood)flood.HeightChanged-=Evaluate;}
 public void Evaluate(float height){bool next=height>=threshold;if(next==submerged)return;submerged=next;onSubmergedChanged.Invoke(next);}
}}
