using System;using UnityEngine;
namespace LighthouseKeepers.WatchDemo {
 // One host-owned stage. Weather views consume it; they never roll their own danger state.
 public sealed class StormDirector {
  readonly WatchState state;int tasks;double recoveryUntil;
  public int Stage {get;private set;}=1;
  public StormDirector(WatchState state){this.state=state;}
  public static int Choose(double remaining,double duration,float danger)=>remaining<=120||danger>=.9f?4:danger>=.75f||remaining<=duration*.4?3:danger>=.5f||remaining<=duration*.7?2:1;
  public void Reset(){Stage=1;tasks=0;recoveryUntil=0;}
  public void Tick(){if(state.ReadOnly)return;if(state.Tension!=null){Stage=state.Tension.StormStage;return;}if(state.CompletedTasks>tasks)recoveryUntil=state.Elapsed+state.Master.stormRecoverySeconds;tasks=state.CompletedTasks;int wanted=Choose(state.Duration-state.Elapsed,state.Duration,state.Danger);if(state.Campaign){wanted=Mathf.Max(wanted,state.Scenario.stormBaseStage);if(state.Elapsed>=state.Duration*state.Scenario.peakStormAfterFraction)wanted=4;}Stage=state.Complete?1:Mathf.Max(1,wanted-(state.Elapsed<recoveryUntil?1:0));}
  public void Apply(int value){if(state.ReadOnly)Stage=value;}
 }
}