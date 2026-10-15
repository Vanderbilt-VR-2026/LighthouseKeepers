using System.Linq;
using System.Text;
using LighthouseKeepers.Networking;
using NUnit.Framework;

namespace LighthouseKeepers.Tests
{
    public sealed class FusionLobbyTests
    {
        static LobbyPacket Request(int revision, string name = "Guest", bool ready = true) =>
            new() { revision = revision, name = name, ready = ready };
        static FusionLobbyState Crew()
        {
            var state = new FusionLobbyState(); state.Reset(4);
            state.Add("1", true); state.Add("2", false); return state;
        }
        [Test] public void SenderIdentityControlsOnlyItsOwnRow()
        {
            var crew = Crew();
            var packet = Request(1); packet.players = new[] { new Lobby.LobbyPlayer("1", "Spoofed", true) };
            Assert.IsTrue(crew.ApplyRequest("2", packet));
            Assert.AreEqual("Keeper", crew.Players[0].DisplayName);
            Assert.IsFalse(crew.Players[0].IsReady);
            Assert.IsTrue(crew.Players[0].IsHost);
            Assert.IsTrue(crew.Players[1].IsReady);
            Assert.IsFalse(crew.Players[1].IsHost);
            Assert.IsFalse(crew.ApplyRequest("unconnected", Request(1)));
        }
        [Test] public void ReorderedReadyRequestsDoNotRestoreOldState()
        {
            var crew = Crew();
            Assert.IsTrue(crew.ApplyRequest("2", Request(3, ready: false)));
            Assert.IsFalse(crew.ApplyRequest("2", Request(2)));
            Assert.IsFalse(crew.Players[1].IsReady);
        }
        [Test] public void LateJoinSnapshotContainsCurrentNamesAndReadyStates()
        {
            var host = Crew(); host.ApplyRequest("1", Request(1, "Captain"));
            host.ApplyRequest("2", Request(1, "Guest")); host.Add("3", false);
            var guest = new FusionLobbyState();
            Assert.IsTrue(LobbyProtocol.TryDecode(LobbyProtocol.Encode(host.Snapshot()), out var packet));
            Assert.IsTrue(guest.ApplySnapshot(packet));
            Assert.AreEqual(3, guest.Players.Count);
            Assert.AreEqual("Captain", guest.Players[0].DisplayName);
            Assert.IsTrue(guest.Players[1].IsReady);
            Assert.IsFalse(guest.Players[2].IsReady);
        }
        [Test] public void OlderSnapshotsDoNotResurrectDisconnectedPlayers()
        {
            var host = Crew(); var old = host.Snapshot(); host.Remove("2");
            var guest = new FusionLobbyState();
            Assert.IsTrue(guest.ApplySnapshot(host.Snapshot()));
            Assert.IsFalse(guest.ApplySnapshot(old));
            Assert.AreEqual(1, guest.Players.Count);
        }
        [Test] public void StartRequiresMinimumCrewAndEveryReadyPlayer()
        {
            var crew = Crew();
            Assert.IsFalse(crew.Begin(3, false)); Assert.IsFalse(crew.Begin(2, true));
            crew.ApplyRequest("1", Request(1)); Assert.IsFalse(crew.Begin(2, true));
            crew.ApplyRequest("2", Request(1)); Assert.IsTrue(crew.Begin(2, true));
            Assert.IsFalse(crew.Add("3", false));
            Assert.IsFalse(crew.ApplyRequest("2", Request(2, ready: false)));
            Assert.IsFalse(crew.Begin(2, true));
            crew.CancelStart(); Assert.IsTrue(crew.Add("3", false));
        }
        [Test] public void CapacityAndLeaveRejoinAreEnforced()
        {
            var crew = Crew(); crew.Add("3", false); crew.Add("4", false);
            Assert.IsFalse(crew.Add("5", false)); Assert.IsFalse(crew.Add("2", false));
            crew.ApplyRequest("2", Request(10)); crew.Remove("2"); crew.Add("2", false);
            Assert.IsTrue(crew.ApplyRequest("2", Request(1)));
            Assert.IsTrue(crew.Players.Last().IsReady);
        }
        [Test] public void BadSnapshotsLeaveExistingRosterUntouched()
        {
            var host = Crew(); var guest = new FusionLobbyState(); guest.ApplySnapshot(host.Snapshot());
            var bad = host.Snapshot(); bad.revision++; bad.players[1].Id = "1";
            Assert.IsFalse(guest.ApplySnapshot(bad)); Assert.AreEqual("2", guest.Players[1].Id);
            bad = host.Snapshot(); bad.revision++; bad.players[1].IsHost = true;
            Assert.IsFalse(guest.ApplySnapshot(bad)); Assert.AreEqual(2, guest.Players.Count);
        }
        [Test] public void MalformedOversizedAndWrongVersionPacketsAreRejected()
        {
            Assert.IsFalse(LobbyProtocol.TryDecode(Encoding.UTF8.GetBytes("broken JSON"), out _));
            Assert.IsFalse(LobbyProtocol.TryDecode(new byte[LobbyProtocol.MaxBytes + 1], out _));
            Assert.IsFalse(LobbyProtocol.TryDecode(new byte[] { 255 }, out _));
            var request = Request(1); request.version = 2;
            Assert.IsFalse(LobbyProtocol.TryDecode(LobbyProtocol.Encode(request), out _));
        }
        [Test] public void NamesAndRoomNamesHaveBoundedInput()
        {
            Assert.AreEqual("Keeper", LobbyProtocol.DisplayName(" \n\t"));
            Assert.AreEqual("Alice", LobbyProtocol.DisplayName(" Ali\nce "));
            Assert.AreEqual(24, LobbyProtocol.DisplayName(new string('x', 100)).Length);
            Assert.IsTrue(LobbyProtocol.ValidRoom("class-crew-33"));
            Assert.IsFalse(LobbyProtocol.ValidRoom("\ncrew"));
            Assert.IsFalse(LobbyProtocol.ValidRoom(new string('x', 65)));
        }
    }
}
