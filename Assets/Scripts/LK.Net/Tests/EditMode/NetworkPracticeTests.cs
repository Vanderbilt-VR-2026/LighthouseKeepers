using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using NUnit.Framework;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class NetworkPracticeTests
    {
        static FusionLobbyState ReadyCrew()
        {
            var crew=new FusionLobbyState();crew.Reset(4);
            for(int actor=1;actor<=4;actor++)
            {
                string id=actor.ToString();Assert.IsTrue(crew.Add(id,actor==1));
                Assert.IsTrue(crew.ApplyRequest(id,new LobbyPacket {revision=1,ready=true}));
            }
            return crew;
        }
        [Test] public void FourReadyKeepersCanEnterPracticeAndTheSnapshotCarriesTheMode()
        {
            var host=ReadyCrew();Assert.IsTrue(host.BeginRehearsal(1,true));
            var client=new FusionLobbyState();Assert.IsTrue(client.ApplySnapshot(host.Snapshot()));
            Assert.IsTrue(client.Rehearsal);Assert.IsTrue(client.Starting);Assert.AreEqual(4,client.Players.Count);
        }
        [Test] public void PracticeDoesNotBypassReadinessOrPermitAFifthKeeper()
        {
            var crew=ReadyCrew();Assert.IsFalse(crew.Add("5",false));
            crew.ApplyRequest("4",new LobbyPacket {revision=2,ready=false});
            Assert.IsFalse(crew.BeginRehearsal(1,true));Assert.IsFalse(crew.Rehearsal);
        }
        [Test] public void CancellingOrResettingClearsPracticeMode()
        {
            var crew=ReadyCrew();crew.BeginRehearsal(1,true);crew.CancelStart();
            Assert.IsFalse(crew.Starting);Assert.IsFalse(crew.Rehearsal);
            crew.BeginRehearsal(1,true);crew.Reset(4);Assert.IsFalse(crew.Rehearsal);
        }
        [Test] public void AReadyPacketCannotEnablePracticeMode()
        {
            var crew=ReadyCrew();crew.ApplyRequest("2",new LobbyPacket {revision=2,ready=true,rehearsal=true});
            Assert.IsFalse(crew.Rehearsal);
            var bad=crew.Snapshot();bad.rehearsal=true;
            Assert.IsFalse(new FusionLobbyState().ApplySnapshot(bad));
        }
        [Test] public void NormalCoopStillNeedsSharedObjectivesWhilePracticeMayOpenTheLab()
        {
            Assert.IsNotEmpty(CrewSessionPolicy.GameplayLaunchBlockReason(ShipWorld.GameplaySceneName,4));
            Assert.AreEqual(4,ShipWorld.MaximumCrewSize);
        }
    }
}
