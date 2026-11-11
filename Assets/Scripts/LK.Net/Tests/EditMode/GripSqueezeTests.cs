using NUnit.Framework;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests {
 public sealed class GripSqueezeTests {
  [Test]public void SustainedGripAndSensorNoiseCannotCountTwice(){var g=new GripSqueezeCycle();Assert.False(g.Sample(.5f,true));Assert.True(g.Sample(1,true));foreach(float p in new[]{1f,.85f,.79f,.7f,1f})Assert.False(g.Sample(p,true));}
  [Test]public void ReleaseRefillsAndEitherFullReleaseOrLightGripAllowsNextSqueeze(){var g=new GripSqueezeCycle();Assert.True(g.Sample(.9f,true));Assert.False(g.Sample(.2f,true));Assert.True(g.Sample(.85f,true));Assert.False(g.Sample(0,false));Assert.True(g.Sample(1,true));}
  [Test]public void HoverAndInvalidInputDoNotPrime(){var g=new GripSqueezeCycle();Assert.False(g.Sample(1,false));Assert.False(g.Sample(float.NaN,true));Assert.False(g.Sample(float.PositiveInfinity,true));Assert.False(g.Sample(.6f,true));Assert.True(g.Sample(.8f,true));}
 }
}
