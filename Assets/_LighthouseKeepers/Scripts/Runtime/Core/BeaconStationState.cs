using System;
using UnityEngine;
namespace LighthouseKeepers.Core
{
 public sealed class BeaconStationState
 {
  readonly S4M2Tuning tuning;float sootRate,notifyAfter;
  public event Action Changed;
  public bool ReadOnly {get;set;}
  public bool Ignited {get;private set;}
  public float Alignment {get;private set;}
  public float Target {get;private set;}
  public float Soot {get;private set;}
  public bool Aligned=>Mathf.Abs(Alignment-Target)<=tuning.alignmentTolerance;
  public bool Shining(bool powered)=>powered&&Ignited&&Aligned;
  public float Brightness=>Mathf.Lerp(1,tuning.dirtyBeamBrightness,Soot);
  public BeaconStationState(S4M2Tuning value){tuning=value;}
  public void Reset(bool lit,float angle,float target,float soot,float rate)
  {Ignited=lit;Alignment=Mathf.Clamp01(angle);Target=Mathf.Clamp01(target);Soot=Mathf.Clamp01(soot);sootRate=Mathf.Max(0,rate);notifyAfter=0;Changed?.Invoke();}
  public bool Ignite(bool lit,bool powered)
  {if(ReadOnly||Ignited==lit||lit&&!powered)return false;Ignited=lit;Changed?.Invoke();return true;}
  public bool Align(float angle)
  {if(ReadOnly||!float.IsFinite(angle)||angle<0||angle>1||Alignment==angle)return false;Alignment=angle;Changed?.Invoke();return true;}
  public void Wipe(float metres)
  {if(ReadOnly||!float.IsFinite(metres)||metres<=0)return;Soot=Mathf.Max(0,Soot-Mathf.Min(.1f,metres)*tuning.sootRemovedPerMetre);}
  public void Tick(float seconds,bool powered)
  {if(ReadOnly||!float.IsFinite(seconds)||seconds<=0)return;if(Shining(powered))Soot=Mathf.Min(1,Soot+seconds*sootRate);notifyAfter-=seconds;if(notifyAfter<=0){notifyAfter=.2f;Changed?.Invoke();}}
  public void ApplyReplica(bool lit,float angle,float target,float soot)
  {if(!ReadOnly)return;Ignited=lit;Alignment=angle;Target=target;Soot=soot;}
 }
}
