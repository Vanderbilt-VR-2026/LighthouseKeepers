using System;
using System.IO;
using LighthouseKeepers.Core.Simulation;
using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class ShipNetworkContractTests
    {
        [Test] public void ManifestNamesExactlyTheTwelveShipScenesAndTwoLobbies()
        {
            var p=ShipManifest.Parse(File.ReadAllText("Assets/Scenes/Sprint3/ShipScenes.json"));
            Assert.AreEqual("LK_Shell_House",p.EntryName);Assert.AreEqual(11,p.additiveScenes.Length);Assert.AreEqual(14,p.scenes.Length);
            Assert.AreEqual("Assets/Scenes/Lab/LK_Lobby_Environment.unity",p.scenes[0]);
            foreach(var path in p.scenes){Assert.IsFalse(path.Contains("LK_StormWatch"));Assert.IsFalse(path.Contains("LK_Lab_Environment"));}
        }
        [Test] public void SnapshotRoundTripPreservesFaultPauseFuelAndRejectionMetadata()
        {
            var source=new SimSnapshot(42,12.3,.25f,.5f,.8f,true,false,0,false,RunPhase.Running,"Needs fuel",true,true,13,2,float.PositiveInfinity,true,2,true);
            Assert.IsTrue(SimulationSnapshotWire.TryRead(SimulationSnapshotWire.Write(7,source),out int sequence,out var result));
            Assert.AreEqual(7,sequence);Assert.AreEqual(source.WaterVolumeM3,result.WaterVolumeM3);
            Assert.AreEqual(source.LastRejectReason,result.LastRejectReason);Assert.AreEqual(source.FuelRemainingSeconds,result.FuelRemainingSeconds);
            Assert.IsTrue(result.Paused);Assert.IsTrue(result.GeneratorFaultActive);Assert.AreEqual(2,result.DayIndex);
        }
        [Test] public void CorruptSnapshotCannotPoisonTheReplica()
        {
            var source=new SimSnapshot(1,double.NaN,0,0,0,false,false,0,false,RunPhase.Running);
            Assert.IsFalse(SimulationSnapshotWire.TryRead(SimulationSnapshotWire.Write(1,source),out _,out _));
            Assert.IsFalse(SimulationSnapshotWire.TryRead(new byte[60],out _,out _));
        }
        [Test] public void NetworkFacadeImplementsBothIncidentEntryPoints()
        {
            Assert.IsTrue(typeof(ISimulationHost).IsAssignableFrom(typeof(FusionSimulationAdapter)));
            Assert.IsNotNull(typeof(FusionSimulationAdapter).GetMethod("ForceGeneratorFault",Type.EmptyTypes));
            Assert.IsNotNull(typeof(FusionSimulationAdapter).GetMethod("ActivateDayLeak",new[]{typeof(int)}));
        }
        [Test] public void TorsoYawTravelsWithBothHandStates()
        {
            var p=new KeeperPoseFrame{Actor=1,Sequence=1,HeadRotation=UnityEngine.Quaternion.identity,LeftRotation=UnityEngine.Quaternion.identity,RightRotation=UnityEngine.Quaternion.identity,TorsoYaw=359,LeftPose=2,RightPose=4,LeftGrip=.8f,RightTrigger=.7f};
            Assert.AreEqual(114,p.Encode().Length);Assert.IsTrue(KeeperPoseFrame.TryDecode(p.Encode(),out var read));
            Assert.AreEqual(359,read.TorsoYaw);Assert.AreEqual(2,read.LeftPose);Assert.AreEqual(4,read.RightPose);
            p.TorsoYaw=float.NaN;Assert.IsFalse(KeeperPoseFrame.TryDecode(p.Encode(),out _));
        }
    }
}
