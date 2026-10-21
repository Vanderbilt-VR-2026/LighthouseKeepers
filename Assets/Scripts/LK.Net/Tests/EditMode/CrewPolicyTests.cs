using NUnit.Framework;
using LighthouseKeepers.Interaction;
using LighthouseKeepers.Networking;
using UnityEngine;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class CrewPolicyTests
    {
        [TestCase(" abc234 ","ABC234")]
        [TestCase("XY9Z87","XY9Z87")]
        public void CodeNormalizes(string input,string expected)
        { Assert.IsTrue(CrewConnectionPolicy.TryCode(input,out var code));Assert.AreEqual(expected,code); }
        [TestCase(null)] [TestCase("")] [TestCase("ABC12")] [TestCase("ABC1234")] [TestCase("ABC 23")] [TestCase("AB0I23")]
        public void InvalidCodesAreRejected(string input) => Assert.IsFalse(CrewConnectionPolicy.TryCode(input,out _));
        [Test] public void GeneratedCodesCanBeEnteredOnTheOtherDevice()
        { for (int i=0;i<1000;i++) Assert.IsTrue(CrewConnectionPolicy.TryCode(CrewConnectionPolicy.NewCode(),out _)); }
        [Test] public void ReadinessMustIncludeBothPlayersAndStaleReadyCannotReappear()
        {
            var state=new FusionLobbyState();state.Reset(2);state.Add("1",true);state.Add("2",false);
            state.ApplyRequest("1",new LobbyPacket { revision=1,ready=true });
            Assert.IsFalse(state.Begin(2,true));
            state.ApplyRequest("2",new LobbyPacket { revision=2,ready=false });
            Assert.IsFalse(state.ApplyRequest("2",new LobbyPacket { revision=1,ready=true }));
            Assert.IsFalse(state.Begin(2,true));
            state.ApplyRequest("2",new LobbyPacket { revision=3,ready=true });Assert.IsTrue(state.Begin(2,true));
        }
        [Test] public void TwoControllersFromOnePlayerNeverPassTheRealGate()
        {
            var gate=new CoopStationGate();gate.Observe(1,10,true,0);gate.Observe(1,20,true,0);
            Assert.IsFalse(gate.Allows(10,20,.1));
        }
        [Test] public void DifferentActorsAtTheSameStationCannotPass()
        {
            var gate=new CoopStationGate();Assert.IsTrue(gate.Observe(1,10,true,0));
            Assert.IsFalse(gate.Observe(2,10,true,0));Assert.IsFalse(gate.Allows(10,10,0));
        }
        [Test] public void DistinctFreshHoldsPassAndReleaseOrTimeoutClosesGate()
        {
            var gate=new CoopStationGate();gate.Observe(1,10,true,0);gate.Observe(2,20,true,0);
            Assert.IsTrue(gate.Allows(10,20,.1));Assert.IsFalse(gate.Allows(10,20,.61));
            gate.Observe(1,10,true,1);gate.Observe(2,20,true,1);gate.Observe(1,10,false,1.1);
            Assert.IsFalse(gate.Allows(10,20,1.1));
        }
        [Test] public void DisconnectAndForgedReleaseCannotTransferAStation()
        {
            var gate=new CoopStationGate();gate.Observe(1,10,true,0);gate.Observe(2,20,true,0);
            gate.Observe(3,10,false,.1);Assert.IsTrue(gate.Allows(10,20,.1));
            gate.RemoveActor(1);Assert.IsFalse(gate.Allows(10,20,.1));
        }
        [Test] public void ExpiredLeaseCannotMoveANewOwnersProp()
        {
            var leases=new GrabAuthority();leases.Register(1);Assert.IsTrue(leases.Acquire(1,1,0,0));
            Assert.AreEqual(1,leases.Expire(2.1).Count);Assert.IsTrue(leases.Acquire(1,2,2,2.2));
            Assert.IsFalse(leases.Touch(1,1,1,2.3));Assert.IsFalse(leases.Release(1,1,1));
            Assert.AreEqual(2,leases.Find(1).Owner);
        }
        [Test] public void PosePacketHasOnlyThreeTransformsAndCompactHandStates()
        {
            var pose=ValidPose(1);pose.LeftPose=4;pose.RightPose=3;pose.LeftGrip=.75f;pose.RightTrigger=.5f;
            byte[] bytes=pose.Encode();Assert.AreEqual(110,bytes.Length);
            Assert.IsTrue(KeeperPoseFrame.TryDecode(bytes,out var read));
            Assert.AreEqual(pose.LeftPose,read.LeftPose);Assert.AreEqual(pose.RightTrigger,read.RightTrigger);
        }
        [Test] public void CorruptPosesAndStaleSequencesAreRejected()
        {
            var pose=ValidPose(1);var buffer=new KeeperPoseBuffer();Assert.IsTrue(buffer.Add(pose,0));
            Assert.IsFalse(buffer.Add(pose,.1));pose.Sequence=2;pose.LeftGrip=float.NaN;
            Assert.IsFalse(buffer.Add(pose,.2));Assert.IsFalse(KeeperPoseFrame.TryDecode(new byte[109],out _));
            pose=ValidPose(3);pose.Left=Vector3.one*100;Assert.IsFalse(pose.Valid);
        }
        [Test,Category("RequiresUnity")] public void PoseInterpolationHoldsBoundedHistoryAndHidesStalledPlayers()
        {
            var buffer=new KeeperPoseBuffer();var first=ValidPose(1);var next=ValidPose(2);next.Head=Vector3.right*.1f;
            Assert.IsTrue(buffer.Add(first,0));Assert.IsTrue(buffer.Add(next,.05));
            Assert.IsTrue(buffer.Sample(.125,out var frame));Assert.AreEqual(.05,frame.Head.x,.0001);
            Assert.IsFalse(buffer.Sample(.7,out _));
        }
        static KeeperPoseFrame ValidPose(int serial) => new KeeperPoseFrame { Actor=1,Sequence=serial,
            HeadRotation=Quaternion.identity,LeftRotation=Quaternion.identity,RightRotation=Quaternion.identity };
    }
}
