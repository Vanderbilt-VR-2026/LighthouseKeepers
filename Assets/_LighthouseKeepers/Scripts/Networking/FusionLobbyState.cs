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
        public static string GameplayLaunchBlockReason(string sceneName, int keepers) =>
            sceneName == ShipWorld.GameplaySceneName && keepers > 1 ? SharedGameStatePending : "";

        // Historical environments retain their existing session/disconnect behavior.
        public static bool RequiresWholeCrew(string sceneName) => false;
    }

    // Lobby traffic is infrequent and has no simulation state. Reliable data keeps
    // this adapter independent of network prefabs and the game's additive scenes.
    [Serializable]
    public sealed class LobbyPacket
    {
        public int version = 1;
        public bool snapshot;
        public int revision;
        public string name;
        public bool ready;
        public int capacity;
        public bool starting;
        public LobbyPlayer[] players;
    }

    public static class LobbyProtocol
    {
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
                return packet != null && packet.version == 1 && packet.revision > 0;
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

        public void Reset(int capacity)
        {
            players.Clear(); requestRevisions.Clear();
            Capacity = Mathf.Clamp(capacity, 2, 8); Revision = 0; Starting = false;
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
            players.Add(new LobbyPlayer(id, "Keeper", host)); Revision++; return true;
        }
        public bool Remove(string id)
        {
            if (players.RemoveAll(p => p.Id == id) == 0) return false;
            requestRevisions.Remove(id); Revision++; return true;
        }
        public bool ApplyRequest(string senderId, LobbyPacket request)
        {
            var player = players.Find(p => p.Id == senderId);
            if (Starting || player == null || request == null || request.snapshot || request.version != 1 ||
                request.revision <= 0 ||
                (requestRevisions.TryGetValue(senderId, out int previous) && request.revision <= previous)) return false;
            requestRevisions[senderId] = request.revision;
            player.DisplayName = LobbyProtocol.DisplayName(request.name);
            player.IsReady = request.ready;
            Revision++; return true;
        }
        public bool Begin(int minimumPlayers, bool allReady)
        {
            if (Starting || players.Count < Math.Max(1, minimumPlayers) ||
                (allReady && players.Any(p => !p.IsReady))) return false;
            Starting = true; Revision++; return true;
        }
        public void CancelStart() { Starting = false; Revision++; }
        public LobbyPacket Snapshot() => new()
        {
            snapshot = true, revision = Revision, capacity = Capacity, starting = Starting,
            players = players.Select(p => new LobbyPlayer(p.Id, p.DisplayName, p.IsHost) { IsReady = p.IsReady }).ToArray()
        };
        public bool ApplySnapshot(LobbyPacket packet)
        {
            if (packet == null || !packet.snapshot || packet.version != 1 || packet.revision <= Revision ||
                packet.capacity < 2 || packet.capacity > 8 || packet.players == null ||
                packet.players.Length == 0 || packet.players.Length > packet.capacity ||
                packet.players.Any(p => p == null || string.IsNullOrEmpty(p.Id) || p.Id.Length > 16) ||
                packet.players.Select(p => p.Id).Distinct().Count() != packet.players.Length ||
                packet.players.Count(p => p.IsHost) != 1) return false;
            players.Clear();
            players.AddRange(packet.players.Select(p => new LobbyPlayer(p.Id,
                LobbyProtocol.DisplayName(p.DisplayName), p.IsHost) { IsReady = p.IsReady }));
            Capacity = packet.capacity; Starting = packet.starting; Revision = packet.revision;
            return true;
        }
    }
}
