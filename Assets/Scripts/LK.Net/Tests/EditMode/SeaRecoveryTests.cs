using LighthouseKeepers.Core;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests {
 public sealed class SeaRecoveryTests {
  [Test]public void InteriorAndGalleryAreSafeButSeaAndIslandEscapeAreNot(){foreach(var p in new[]{new Vector3(0,1.6f,-8),new Vector3(0,1.6f,0),new Vector3(4.2f,11.5f,0),new Vector3(0,1.6f,-14)})Assert.False(SeaRecoveryState.Outside(p));Assert.True(SeaRecoveryState.Outside(new Vector3(9,1,0)));Assert.True(SeaRecoveryState.Outside(new Vector3(0,-1,0)));}
  [Test]public void SoloSeaFallLosesAndFreshNightResets(){var s=new SeaRecoveryState();s.Tick(0,new Vector3(0,-1,0),0,default,.1f);Assert.True(s.lost);Assert.False(new SeaRecoveryState().lost);}
  [Test]public void CrewOnlyReturnsFallenKeeperAfterTenSeconds(){var s=new SeaRecoveryState();s.Tick(1,new Vector3(0,-1,0),2,new Vector3(0,2,0),.1f);Assert.False(s.lost);Assert.AreEqual(10,s.For(1).remaining);s.Tick(1,default,2,Vector3.up*2,9.9f);Assert.Zero(s.For(1).serial);s.Tick(1,default,2,Vector3.up*2,.11f);Assert.AreEqual(1,s.For(1).serial);Assert.Zero(s.For(2).serial);Assert.Zero(s.For(2).remaining);Assert.True(SeaRecoveryState.Valid(s));}
  [Test]public void HostLeavingPreservesGuestsExistingRecovery(){var s=new SeaRecoveryState();s.Tick(2,Vector3.up,3,Vector3.down,.1f);s.Tick(3,Vector3.down,0,default,10);Assert.AreEqual(1,s.For(3).serial);Assert.False(s.lost);}
  [Test]public void DisconnectDuringRecoveryStillReturnsSurvivor(){var s=new SeaRecoveryState();s.Tick(1,Vector3.down,2,Vector3.up,.1f);s.Tick(1,Vector3.down,0,default,10);Assert.AreEqual(1,s.For(1).serial);Assert.False(s.lost);s.first.remaining=float.NaN;Assert.False(SeaRecoveryState.Valid(s));}
 }
}
