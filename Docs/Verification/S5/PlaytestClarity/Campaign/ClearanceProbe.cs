using UnityEngine;using LighthouseKeepers.WatchDemo;
public static class ClearanceProbe {
 static WatchDemoController W=>Object.FindAnyObjectByType<WatchDemoController>();
 public static object Facts()=>new {day=W.State.Day,elapsed=W.State.Elapsed,remaining=W.State.Duration-W.State.Elapsed,cleared=W.State.Ships.ClearanceGranted,wait=W.State.Ships.DepartureWait,passed=W.State.Ships.PassedMask,repaired=W.State.Repaired,radio=W.State.Radio.Completed,failed=W.State.Failed,complete=W.State.Complete,replica=W.State.ReadOnly,first=W.State.Director.First.ToString()};
 public static object Prepare(){var s=W.State;s.CompleteRepair();s.EngageWatch();s.Tick(180);return Facts();}
 public static object Transmit(){var s=W.State;var r=s.Radio;r.ToggleMain();r.Tune(r.Program.targetMHz);r.Tick(3,true);for(int i=0;i<r.CodeLength;i++)r.PressCode(r.CodeSymbol(i));r.SetMicrophone(true,true);r.Tick(r.Program.transmitSeconds,true);s.Tick(.1);return Facts();}
 public static object Pass(){var s=W.State;for(int i=0;i<1500&&!s.Ships.Finished;i++){s.AlignBeacon(s.ShipWheelTarget);s.Tick(.1);}return Facts();}
 public static object Retry(){W.State.Retry();return Facts();}
}
