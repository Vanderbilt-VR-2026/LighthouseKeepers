using System;
using System.Security.Cryptography;

namespace LighthouseKeepers.Net
{
    public static class CrewConnectionPolicy
    {
        public const int Capacity = LighthouseKeepers.Lobby.ShipWorld.MaximumCrewSize;
        public const string VoiceNotice = "Day 3 requires voice chat — use a party.";
        public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public static string NewCode()
        {
            var bytes = new byte[6];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
            var chars = new char[6];
            // Thirty-two symbols divide the byte range exactly.
            for (int i = 0; i < chars.Length; i++) chars[i] = Alphabet[bytes[i] % Alphabet.Length];
            return new string(chars);
        }
        public static bool TryCode(string text, out string code)
        {
            code = (text ?? "").Trim().ToUpperInvariant();
            if (code.Length != 6) return false;
            foreach (char c in code) if (Alphabet.IndexOf(c) < 0) return false;
            return true;
        }
        public static string Explain(string error)
        {
            if (string.IsNullOrEmpty(error)) return "";
            if (error.Contains("GameNotFound")) return "No crew has that code. Check all six characters and the host's region.";
            if (error.Contains("GameIsFull") || error.Contains("ServerFull")) return "That crew is full. Up to four keepers can connect.";
            if (error.Contains("GameClosed")) return "That watch has started. Ask the host to return to the lobby, then reconnect.";
            if (error.Contains("GameIdAlreadyExists")) return "That code is in use. Press Host crew to get a new code.";
            if (error.Contains("InvalidAuthentication") || error.Contains("InvalidRegion")) return "Photon setup needs attention. Check the Fusion App ID and region in the setup guide.";
            if (error.Contains("MaxCcuReached")) return "The project's Photon player limit is reached. Try again after another crew leaves.";
            if (error.Contains("Timeout") || error.Contains("ConnectionFailed") || error.Contains("ConnectionRefused"))
                return "Connection lost or unavailable. Check Wi-Fi, then press Reconnect.";
            if (error.Contains("host left") || error.Contains("HostMigration") || error.Contains("Disconnected") || error.Contains("session ended"))
                return "The host left or the connection was lost. Ask the host to reconnect first, then press Reconnect.";
            if (error.Contains("partner left")) return "Your partner left. Reconnect to the same code and ready up for a new watch.";
            return error;
        }
    }
}
