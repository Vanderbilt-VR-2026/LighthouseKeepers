using System.Collections.Generic;
namespace LighthouseKeepers.WatchDemo
{
 // Local UI state only. No commands, authority changes or replicated fields.
 public sealed class ObjectiveRecallState
 {
  public bool Held {get;private set;}
  public float Until {get;private set;}=float.NegativeInfinity;
  float lastTap=float.NegativeInfinity;
  public int Input(bool held,float now)
  {
   int result=0;
   if(held&&!Held){result=now-lastTap<=.35f?2:1;lastTap=result==2?float.NegativeInfinity:now;}
   if(held||Held)Until=now+ObjectiveHudSettings.Duration;
   Held=held;return result;
  }
  public bool Visible(float now)=>Held||now<Until;
 }
 public sealed class HarborRecallHistory
 {
  readonly List<int> cues=new(3);
  public IReadOnlyList<int> Cues=>cues;
  public int Latest=>cues.Count>0?cues[0]:0;
  public void Remember(int cue){if(cue<=0)return;cues.Insert(0,cue);if(cues.Count>3)cues.RemoveAt(3);}
 }
}
