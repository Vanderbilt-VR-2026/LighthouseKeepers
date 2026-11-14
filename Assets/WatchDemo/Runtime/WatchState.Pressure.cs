using System;using UnityEngine;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class WatchState {
  public float LatestWater {get;private set;}
  public float PeakDanger {get;private set;}
  public bool PreviousCloseCall {get;private set;}
  public bool PressureEnabled=>Master&&Master.pressureCues;
  public float WaterDanger=>Mathf.Clamp01(LatestWater/(Tuning?Tuning.failWaterHeightM:.55f));
  public float BeaconDanger=>Ships!=null?(BeaconShining?0:Ships.Danger):BeaconShining||BeaconDarkLimit<=0||BeaconGraceRemaining>0?0:Mathf.Clamp01((float)(BeaconDarkElapsed/BeaconDarkLimit));
  public float TimeDanger=>Tutorial||Complete?0:Mathf.Clamp01(1-(float)((Duration-Elapsed)/120));
  public float Danger=>Mathf.Max(WaterDanger,BeaconDanger,TimeDanger);
  public string DangerKey=>WaterDanger>=BeaconDanger&&WaterDanger>=TimeDanger?"danger.water":BeaconDanger>=TimeDanger?"danger.beacon":"danger.time";
  void ObservePressure(){if(!ReadOnly&&PressureEnabled)PeakDanger=Mathf.Max(PeakDanger,Danger);}
 }
}