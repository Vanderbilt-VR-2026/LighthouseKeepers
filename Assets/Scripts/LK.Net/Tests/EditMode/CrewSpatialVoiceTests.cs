using NUnit.Framework;using UnityEngine;using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class CrewSpatialVoiceTests
 {
  [Test] public void NearbySpeechIsClearWallsMuffleAndTwoFloorsAreFaint()
  {
   var t=ScriptableObject.CreateInstance<ConnectedTasksTuning>();try{
    Assert.AreEqual(1,CrewSpatialVoice.ProximityGain(2,0,false,t),.001);
    Assert.Less(CrewSpatialVoice.ProximityGain(2,0,true,t),.6f);
    Assert.Less(CrewSpatialVoice.ProximityGain(7,6.6f,true,t),.04f);
    Assert.Greater(CrewSpatialVoice.ProximityGain(7,6.6f,true,t),0);
    Assert.Greater(CrewSpatialVoice.ProximityGain(4,3.3f,false,t),CrewSpatialVoice.ProximityGain(7,6.6f,false,t));
   }finally{Object.DestroyImmediate(t);}
  }
  [Test] public void RadioRoutingRequiresFreshHostHoldsAndReleaseCannotSilenceOtherKeeper()
  {
   var state=new CrewRadioState(.55f);Assert.True(state.Set(0,1,true));Assert.True(state.Transmitting(1));Assert.False(state.Set(0,2,false));Assert.False(state.Set(0,2,true));state.Tick(.6f);Assert.False(state.Transmitting(1));
   state.Set(1,2,true);state.Remove(2);Assert.False(state.Transmitting(2));state.Set(0,1,true);state.Clear();Assert.False(state.Transmitting(1));
   var replica=new CrewRadioState(.55f){ReadOnly=true};Assert.False(replica.Set(0,2,true));Assert.True(replica.Apply(2,0));replica.Tick(20);Assert.True(replica.Transmitting(2));Assert.False(replica.Apply(-1,0));Assert.True(replica.Apply(0,0));Assert.False(replica.Transmitting(2));
  }
 }
}
