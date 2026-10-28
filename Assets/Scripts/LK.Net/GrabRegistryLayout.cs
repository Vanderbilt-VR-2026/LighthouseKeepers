using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LighthouseKeepers.Net
{
    /// <summary>Every peer must agree on the object-to-ID mapping before any ownership request is accepted.</summary>
    public static class GrabRegistryLayout
    {
        public const int ProtocolVersion = 2;
        public static string Fingerprint(IEnumerable<string> paths)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));
            var ordered = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string path in paths)
                if (string.IsNullOrWhiteSpace(path) || !ordered.Add(path))
                    throw new ArgumentException("Network grabbables need unique, nonempty hierarchy paths.", nameof(paths));
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(
                ProtocolVersion + "\n" + string.Join("\n", ordered)))).Replace("-", "");
        }
        public static bool Matches(string local, string remote, int expectedCount, int receivedCount) =>
            !string.IsNullOrEmpty(local) && local.Length == 64 &&
            string.Equals(local, remote, StringComparison.Ordinal) && expectedCount == receivedCount;
    }
}
