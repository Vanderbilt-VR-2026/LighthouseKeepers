using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LighthouseKeepers.Lobby;
using UnityEngine;

namespace LighthouseKeepers.Networking
{
    /// <summary>The two-player watch ends on partner loss; the development environment supports a changing crew.</summary>
    public static class CrewSessionPolicy
    {
        public const string SharedGameStatePending = "Co-op gameplay is unavailable until shared game state is implemented. Use PLAY SOLO or host alone to debug.";
        public static string GameplayLaunchBlockReason(string sceneName, int keepers, bool connectedCrew=false) =>
            sceneName != ShipWorld.GameplaySceneName ? "" : keepers > ShipWorld.MaximumCrewSize ? "This test supports two keepers." :
            keepers > 1 && !(ShipWorld.CoopEnabled&&connectedCrew) ? SharedGameStatePending : "";

        // Historical environments retain their existing session/disconnect behavior.
        public static bool RequiresWholeCrew(string sceneName) => ShipWorld.CoopEnabled && sceneName == ShipWorld.GameplaySceneName;
    }

    // Lobby traffic is infrequent and has no simulation state. Reliable data keeps
    // this adapter independent of network prefabs and the game's additive scenes.
    [Serializable]
    public sealed class LobbyPacket
    {
        public int version = LobbyProtocol.Version;
        public CrewDuty requestedDuty;
        public bool snapshot;
        public int revision;
        public string name;
        public bool ready;
        public int capacity;
        public bool starting;
        public bool rehearsal;
        public LobbyPlayer[] players;
    }

    public static class LobbyProtocol
    {
        public const int Version=2;
        public const int Channel = 0x4C4B4C42;
        public const int MaxBytes = 8192;
        public static string DisplayName(string value)
        {
            string name = new string((value ?? "").Where(c => !char.IsControl(c)).ToArray()).Trim();
            return string.IsNullOrEmpty(name) ? "Keeper" : name.Substring(0, Math.Min(24, name.Length));
        }
        public static bool ValidRoom(string value) => !string.IsNullOrWhiteSpace(value) &&
            value.Trim().Length <= 64 && !value.Any(char.IsControl);
        public static byte[] Encode(LobbyPacket packet) => Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet));
        public static bool TryDecode(ReadOnlySpan<byte> bytes, out LobbyPacket packet)
        {
            packet = null;
            if (bytes.Length == 0 || bytes.Length > MaxBytes) return false;
            try
            {
                packet = JsonUtility.FromJson<LobbyPacket>(new UTF8Encoding(false, true).GetString(bytes.ToArray()));
                return packet != null && packet.version == Version && packet.revision > 0;
            }
            catch (Exception e) when (e is ArgumentException || e is DecoderFallbackException) { return false; }
        }
    }

    /// <summary>Host-owned roster; request identity always comes from Fusion's sender.</summary>
    public sealed class FusionLobbyState
    {
        readonly List<LobbyPlayer> players = new();
        readonly Dictionary<string, int> requestRevisions = new();
        public IReadOnlyList<LobbyPlayer> Players => players;
        public int Capacity { get; private set; } = 4;
        public int Revision { get; private set; }
        public bool Starting { get; private set; }
        public bool Rehearsal { get; private set; }

        public void Reset(int capacity)
        {
            players.Clear(); requestRevisions.Clear();
            Capacity = Math.Max(2,Math.Min(8,capacity)); Revision = 0; Starting = Rehearsal = false;
        }
        public bool Add(string id, bool host)
        {
            if (Starting) return false;
            var existing = players.Find(p => p.Id == id);
            if (existing != null)
            {
                if (!host || existing.IsHost) return false;
                existing.IsHost = true; Revision++; return true;
            }
            if (players.Count >= Capacity) return false;
            players.Add(new LobbyPlayer(id, "Keeper", host){Duty=VacantDuty()}); Revision++; return true;
        }
        // Explicit host admission after the Lab is loaded; ordinary lobby Add stays locked.
        public bool AddToRunningWatch(string id)
        {
            if (!Starting || Rehearsal || string.IsNullOrEmpty(id) || players.Count >= Capacity || players.Any(p => p.Id == id)) return false;
            players.Add(new LobbyPlayer(id, "Keeper", false) { IsReady = true, Duty=VacantDuty() });
            requestRevisions.Remove(id); ResolveDuties(); Revision++; return true;
        }
        public bool Remove(string id)
        {
            if (players.RemoveAll(p => p.Id == id) == 0) return false;
            requestRevisions.Remove(id); ResolveDuties(); Revision++; return true;
        }
        public bool ApplyRequest(string senderId, LobbyPacket request, bool runningWatch = false)
        {
            var player = players.Find(p => p.Id == senderId);
            if ((Starting && (!runningWatch || Rehearsal)) || player == null || request == null || request.snapshot || request.version != LobbyProtocol.Version ||
                !CrewDuties.Valid(request.requestedDuty) || Starting&&request.requestedDuty!=CrewDuty.None || request.revision <= 0 ||
                (requestRevisions.TryGetValue(senderId, out int previous) && request.revision <= previous)) return false;
            requestRevisions[senderId] = request.revision;
            player.DisplayName = LobbyProtocol.DisplayName(request.name);
            if (!Starting) {
                player.IsReady = request.ready;
                if(request.requestedDuty!=CrewDuty.None){player.RequestedDuty=request.requestedDuty==player.Duty?CrewDuty.None:request.requestedDuty;ResolveDuties();}
            }
            Revision++; return true;
        }
        CrewDuty VacantDuty()=>!players.Any(p=>p.Duty==CrewDuty.Lightkeeper)?CrewDuty.Lightkeeper:!players.Any(p=>p.Duty==CrewDuty.Machinist)?CrewDuty.Machinist:CrewDuty.None;
        void ResolveDuties()
        {
            // Occupied duties only swap when both connected keepers request the exchange.
            foreach(var p in players)
            {
                if(p.RequestedDuty==CrewDuty.None)continue;
                var other=players.Find(q=>q!=p&&q.Duty==p.RequestedDuty);
                if(other==null){p.Duty=p.RequestedDuty;p.RequestedDuty=CrewDuty.None;}
                else if(other.RequestedDuty==p.Duty){var old=p.Duty;p.Duty=other.Duty;other.Duty=old;p.RequestedDuty=other.RequestedDuty=CrewDuty.None;}
            }
        }
        public bool Begin(int minimumPlayers, bool allReady) => BeginCore(minimumPlayers,allReady,false);
        public bool BeginRehearsal(int minimumPlayers, bool allReady) => BeginCore(minimumPlayers,allReady,true);
        bool BeginCore(int minimumPlayers, bool allReady, bool rehearsal)
        {
            if (Starting || players.Count < Math.Max(1, minimumPlayers) ||
                (allReady && players.Any(p => !p.IsReady))) return false;
            Starting = true; Rehearsal = rehearsal; foreach(var p in players)p.RequestedDuty=CrewDuty.None; Revision++; return true;
        }
        public void CancelStart() { Starting = Rehearsal = false; Revision++; }
        public LobbyPacket Snapshot() => new()
        {
            snapshot = true, revision = Revision, capacity = Capacity, starting = Starting, rehearsal = Rehearsal,
            players = players.Select(p => new LobbyPlayer(p.Id, p.DisplayName, p.IsHost) { IsReady = p.IsReady, Duty=p.Duty, RequestedDuty=p.RequestedDuty }).ToArray()
        };
        public bool ApplySnapshot(LobbyPacket packet)
        {
            if (packet == null || !packet.snapshot || packet.version != LobbyProtocol.Version || packet.revision <= Revision ||
                packet.capacity < 2 || packet.capacity > 8 || packet.players == null ||
                packet.players.Length == 0 || packet.players.Length > packet.capacity ||
                packet.players.Any(p => p == null || string.IsNullOrEmpty(p.Id) || p.Id.Length > 16 || !CrewDuties.Valid(p.Duty) || !CrewDuties.Valid(p.RequestedDuty) || p.RequestedDuty!=CrewDuty.None&&p.RequestedDuty==p.Duty || packet.starting&&p.RequestedDuty!=CrewDuty.None) ||
                packet.players.Where(p=>p.Duty!=CrewDuty.None).Select(p=>p.Duty).Distinct().Count()!=packet.players.Count(p=>p.Duty!=CrewDuty.None) ||
                packet.capacity==2&&packet.players.Any(p=>p.Duty==CrewDuty.None) ||
                packet.players.Select(p => p.Id).Distinct().Count() != packet.players.Length ||
                packet.players.Count(p => p.IsHost) != 1 || packet.rehearsal && !packet.starting) return false;
            players.Clear();
            players.AddRange(packet.players.Select(p => new LobbyPlayer(p.Id,
                LobbyProtocol.DisplayName(p.DisplayName), p.IsHost) { IsReady = p.IsReady, Duty=p.Duty, RequestedDuty=p.RequestedDuty }));
            Capacity = packet.capacity; Starting = packet.starting; Rehearsal = packet.rehearsal; Revision = packet.revision;
            return true;
        }
    }
}
