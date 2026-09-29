using System;
using UnityEngine;
using UnityEngine.Events;
namespace LighthouseKeepers.Flood {
public enum FloodDangerStage { Safe, Watch, Warning, Critical, Submerged }
public sealed class FloodDangerMonitor:MonoBehaviour {
 [SerializeField,Min(0),Tooltip("Head clearance below which the flood is watching you.")] float watchDistance=2f;
 [SerializeField,Min(0),Tooltip("Head clearance below which the flood is an active warning.")] float warningDistance=1f;
 [SerializeField,Min(0),Tooltip("Head clearance below which the flood is critical.")] float criticalDistance=0.4f;
 [SerializeField] UnityEvent<FloodDangerStage> onStageChanged=new();
 public event Action<FloodDangerStage,FloodDangerStage> StageChanged;
 public FloodDangerStage Stage {get;private set;}=FloodDangerStage.Safe;
 public float HeadClearance {get;private set;}=float.PositiveInfinity;
 public float HeadHeight {get;private set;}=1.6f;
 public float TimeToSubmersion=>flood?flood.TimeToHeight(HeadHeight):float.PositiveInfinity;
 public UnityEvent<FloodDangerStage> OnStageChanged=>onStageChanged;
 FloodController flood;
 void OnEnable(){flood=FindAnyObjectByType<FloodController>();if(flood)flood.HeightChanged+=OnHeight;RefreshNow();}
 void OnDisable(){if(flood)flood.HeightChanged-=OnHeight;}
 void OnHeight(float _)=>EvaluateHead(CurrentHeadY());
 void Update()=>EvaluateHead(CurrentHeadY());
 public void RefreshNow()=>EvaluateHead(CurrentHeadY());
 float CurrentHeadY(){var cam=Camera.main;if(cam)HeadHeight=cam.transform.position.y;return HeadHeight;}
 public void EvaluateHead(float headY){HeadHeight=headY;float water=flood?flood.Height:float.NegativeInfinity;HeadClearance=float.IsNegativeInfinity(water)?float.PositiveInfinity:headY-water;SetStage(StageFor(HeadClearance));}
 public FloodDangerStage StageFor(float clearance){if(clearance<=0)return FloodDangerStage.Submerged;if(clearance<criticalDistance)return FloodDangerStage.Critical;if(clearance<warningDistance)return FloodDangerStage.Warning;if(clearance<watchDistance)return FloodDangerStage.Watch;return FloodDangerStage.Safe;}
 void SetStage(FloodDangerStage next){if(next==Stage)return;var previous=Stage;Stage=next;StageChanged?.Invoke(previous,next);onStageChanged.Invoke(next);}
}}