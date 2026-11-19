using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo {
 // Local presentation only: never changes tasks, colliders, or the partner's instructions.
 [DefaultExecutionOrder(1100)]public sealed class StationObjectiveGuidance:MonoBehaviour {
  public enum Station {Pipe,Valve,Engine,Radio,Beacon,Panel,Kit}
  readonly Dictionary<TextMesh,(Station station,bool originallyVisible)> labels=new();
  readonly List<(Transform root,Station station)> roots=new();
  WatchDemoController watch;WatchPipeRepair pipe;bool raster;float nextScan;
  public static void Create(WatchDemoController w){w.gameObject.AddComponent<StationObjectiveGuidance>().watch=w;}
  void Start(){
   pipe=FindAnyObjectByType<WatchPipeRepair>();raster=GetComponent<WorldLabelRaster>();
   Add(pipe,Station.Pipe);Add(FindAnyObjectByType<WatchPressureValve>(),Station.Valve);
   Add(FindAnyObjectByType<WatchGenerator>(),Station.Engine);Add(FindAnyObjectByType<WatchEngineControls>(),Station.Engine);
   Add(FindAnyObjectByType<WatchRadioConsole>(),Station.Radio);Add(FindAnyObjectByType<WatchBeaconStation>(),Station.Beacon);Add(FindAnyObjectByType<WatchPowerPanel>(),Station.Panel);
   Scan();foreach(var t in watch.GetComponentsInChildren<TextMesh>(true))if(t.name.EndsWith("lantern hook instruction"))labels.TryAdd(t,(Station.Kit,t.GetComponent<Renderer>().enabled));if(pipe&&pipe.prompt&&!labels.ContainsKey(pipe.prompt))labels.Add(pipe.prompt,(Station.Pipe,pipe.prompt.GetComponent<Renderer>().enabled));
  }
  void Add(Component c,Station s){if(c)roots.Add((c.transform,s));}
  void Scan(){nextScan=Time.unscaledTime+3;foreach(var root in roots)foreach(var text in root.root.GetComponentsInChildren<TextMesh>(true))if(!labels.ContainsKey(text))labels.Add(text,(root.station,text.GetComponent<Renderer>().enabled));}
  public static bool RadioStep(KeeperStep step)=>step>=KeeperStep.RadioPower&&step<=KeeperStep.RadioTransmit;
  public static bool BeamStep(KeeperStep step)=>step==KeeperStep.BeaconLight||step==KeeperStep.BeaconAlign||step==KeeperStep.KeepLight;
  public static bool Allows(Station station,string name,KeeperStep step,int activeBolt){
   switch(station){
    case Station.Kit:return step==KeeperStep.GrabLantern||step==KeeperStep.LanternWick;
    case Station.Pipe:
     bool bolt=step==KeeperStep.TightenBolt||step>=KeeperStep.BoltA&&step<=KeeperStep.BoltC;
     if(!bolt)return false;
     if(name.StartsWith("Chalk coupling "))return name.EndsWith(((char)('A'+activeBolt)).ToString());
     return name=="Pipe instruction"||name=="Repair instruction"; // The scene prompt is explicitly bound below.
    case Station.Valve:return step==KeeperStep.HoldValve&&name=="Valve fixed instruction";
    case Station.Engine:
     if(step==KeeperStep.Fuel||step==KeeperStep.CheckFuel)return name=="Fuel level"||name=="Fuel filling plate"||name=="Filler label"||name=="Fuel can label";
     if(step==KeeperStep.Starter)return name=="Timed starter grip"||name=="Starter pull label"||name=="Starter timing instruction"||name=="Starter tally numbered wheel";
     if(step==KeeperStep.HoldStarterAssist)return name=="Start assist brass stencil";
     if(step==KeeperStep.Prime)return name=="Primer plate"||name=="Primer tally numbered wheel";
     return (step==KeeperStep.CloseChoke||step==KeeperStep.OpenChoke)&&name=="Choke plate";
    case Station.Radio:
     if(!RadioStep(step))return false;
     if(name=="Radio header"||name=="Radio readout")return true;
     if(step==KeeperStep.RadioPower)return name=="Main label"||name=="1 POWER";
     if(step==KeeperStep.RadioTune)return name=="Tune label"||name=="Lock label"||name=="2 TUNE"||name.StartsWith("Band scale ");
     if(step==KeeperStep.RadioCode)return name=="3 CODE"||name.StartsWith("Code label ");
     return name=="4 TRANSMIT"||name=="Mic label"||name=="TX label";
    case Station.Beacon:
     if(!BeamStep(step))return false;
     if(name=="Beacon heading"||name=="Beacon instructions")return true;
     return step==KeeperStep.BeaconLight?name=="Ignition label":name=="Wheel label"||name=="Wheel alignment engraving";
    case Station.Panel:
     if(step<KeeperStep.PumpOff||step>KeeperStep.ResetBreaker)return false;
     if(name=="Panel heading"||name=="Breaker instruction")return true;
     if(step==KeeperStep.ResetBreaker)return name=="Reset label";
     int circuit=step>=KeeperStep.PumpOn?(int)step-(int)KeeperStep.PumpOn:(int)step-(int)KeeperStep.PumpOff;
     return name=="Circuit label "+circuit;
   }return false;
  }
  void LateUpdate(){if(!watch||watch.State==null)return;if(Time.unscaledTime>=nextScan)Scan();var step=watch.CurrentInstruction;bool first=watch.State.Day==1;
   foreach(var item in labels){var text=item.Key;if(!text)continue;bool show=!first||Allows(item.Value.station,text.name,step,watch.State.Pipe.ActiveBolt);
    if(first&&pipe&&text==pipe.prompt)show=step==KeeperStep.TightenBolt||step>=KeeperStep.BoltA&&step<=KeeperStep.BoltC;
    WorldLabelRaster.SetObjectiveVisible(text,show);
    if(!raster)text.GetComponent<Renderer>().enabled=show&&item.Value.originallyVisible;
   }
  }
  void OnDestroy(){foreach(var item in labels){WorldLabelRaster.ClearObjectiveVisibility(item.Key);if(!item.Key)continue;if(!raster)item.Key.GetComponent<Renderer>().enabled=item.Value.originallyVisible;}}
 }
}
