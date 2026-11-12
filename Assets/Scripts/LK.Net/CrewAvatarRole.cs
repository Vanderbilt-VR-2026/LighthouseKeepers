using System;
using System.Collections.Generic;
using LighthouseKeepers.Lobby;

namespace LighthouseKeepers.Net
{
    public enum CrewAvatarRole { Unknown, Captain, Fisherman }

    public static class CrewAvatarRoles
    {
        // Use the host-authored roster, not connection order, name or the viewing client's role.
        public static bool TryResolve(string playerId, IReadOnlyList<LobbyPlayer> roster, out CrewAvatarRole role)
        {
            role = CrewAvatarRole.Unknown;
            if (string.IsNullOrEmpty(playerId) || roster == null) return false;
            foreach (var player in roster)
            {
                if (player == null || !string.Equals(player.Id, playerId, StringComparison.Ordinal)) continue;
                role = player.IsHost ? CrewAvatarRole.Captain : CrewAvatarRole.Fisherman;
                return true;
            }
            return false;
        }
    }
}
