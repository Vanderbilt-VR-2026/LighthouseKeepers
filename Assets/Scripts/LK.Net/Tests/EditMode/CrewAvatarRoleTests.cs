using System.Collections.Generic;
using LighthouseKeepers.Lobby;
using NUnit.Framework;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class CrewAvatarRoleTests
    {
        [Test] public void CaptainComesFromHostFlagRatherThanPlayerNumberOrRosterOrder()
        {
            var players = new[] { new LobbyPlayer("1","Guest"), new LobbyPlayer("7","Host",true) };
            Assert.IsTrue(CrewAvatarRoles.TryResolve("7",players,out var captain));
            Assert.AreEqual(CrewAvatarRole.Captain,captain);
            Assert.IsTrue(CrewAvatarRoles.TryResolve("1",players,out var fisherman));
            Assert.AreEqual(CrewAvatarRole.Fisherman,fisherman);
        }
        [Test] public void RejoiningGuestRetainsFishermanRoleWithANewActorAndArbitraryName()
        {
            var players = new List<LobbyPlayer> { new LobbyPlayer("4","Host",true), new LobbyPlayer("8","Guest") };
            players.RemoveAt(1);players.Add(new LobbyPlayer("19","Captain"));
            Assert.IsTrue(CrewAvatarRoles.TryResolve("19",players,out var role));
            Assert.AreEqual(CrewAvatarRole.Fisherman,role);
            Assert.IsFalse(CrewAvatarRoles.TryResolve("8",players,out var departed));
            Assert.AreEqual(CrewAvatarRole.Unknown,departed);
        }
        [Test] public void MissingRosterEntryNeverGuessesACaptain()
        {
            Assert.IsFalse(CrewAvatarRoles.TryResolve("3",new[] { new LobbyPlayer("2","Host",true) },out var role));
            Assert.AreEqual(CrewAvatarRole.Unknown,role);
            Assert.IsFalse(CrewAvatarRoles.TryResolve("3",null,out role));
            Assert.AreEqual(CrewAvatarRole.Unknown,role);
        }
        [TestCase(null)] [TestCase("")]
        public void MissingPlayerIdDoesNotMatchAnUninitializedEntry(string id)
        {
            Assert.IsFalse(CrewAvatarRoles.TryResolve(id,new[] { new LobbyPlayer(id,"Host",true) },out var role));
            Assert.AreEqual(CrewAvatarRole.Unknown,role);
        }
    }
}
