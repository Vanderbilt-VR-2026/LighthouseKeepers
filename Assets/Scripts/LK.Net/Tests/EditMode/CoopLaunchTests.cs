using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class CoopLaunchTests
    {
        [Test] public void FlagGatesTwoKeeperLabAndStillRejectsThirdKeeper()
        {
            bool before=ShipWorld.CoopEnabled;
            try
            {
                ShipWorld.CoopEnabled=false;Assert.IsNotEmpty(CrewSessionPolicy.GameplayLaunchBlockReason(ShipWorld.GameplaySceneName,2));
                Assert.IsEmpty(CrewSessionPolicy.GameplayLaunchBlockReason(ShipWorld.GameplaySceneName,1));
                ShipWorld.CoopEnabled=true;Assert.IsNotEmpty(CrewSessionPolicy.GameplayLaunchBlockReason(ShipWorld.GameplaySceneName,2));
                Assert.IsEmpty(CrewSessionPolicy.GameplayLaunchBlockReason(ShipWorld.GameplaySceneName,2,true));
                Assert.IsNotEmpty(CrewSessionPolicy.GameplayLaunchBlockReason(ShipWorld.GameplaySceneName,3));
                var roster=new FusionLobbyState();roster.Reset(ShipWorld.MaximumCrewSize);
                Assert.IsTrue(roster.Add("1",true));Assert.IsTrue(roster.Add("2",false));Assert.IsFalse(roster.Add("3",false));
                roster.ApplyRequest("1",new LobbyPacket{revision=1,ready=true});Assert.IsFalse(roster.Begin(1,true));
                roster.ApplyRequest("2",new LobbyPacket{revision=1,ready=true});Assert.IsTrue(roster.Begin(1,true));
            }
            finally{ShipWorld.CoopEnabled=before;}
        }
    }
}
