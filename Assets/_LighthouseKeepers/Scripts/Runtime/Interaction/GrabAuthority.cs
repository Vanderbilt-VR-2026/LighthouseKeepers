using System;
using System.Collections.Generic;

namespace LighthouseKeepers.Interaction
{
    /// <summary>Host-side exclusive leases. Epochs reject delayed updates from a previous grab.</summary>
    public sealed class GrabAuthority
    {
        public const int MaximumPlayers = 4;
        public const double LeaseTimeout = 2.0;

        public sealed class Lease
        {
            public int Owner { get; internal set; }
            public int Epoch { get; internal set; }
            public double LastUpdate { get; internal set; }
        }

        readonly Dictionary<int, Lease> leases = new();
        public void Register(int objectId)
        {
            if (objectId <= 0) throw new ArgumentOutOfRangeException(nameof(objectId));
            if (!leases.ContainsKey(objectId)) leases.Add(objectId, new Lease());
        }
        public Lease Find(int objectId) => leases.TryGetValue(objectId, out var lease) ? lease : null;
        public void Clear() => leases.Clear();

        public bool Acquire(int objectId, int player, int expectedEpoch, double now)
        {
            var lease = Find(objectId);
            if (player <= 0 || lease == null || lease.Epoch != expectedEpoch || lease.Owner != 0) return false;
            lease.Owner = player;
            lease.Epoch++;
            lease.LastUpdate = now;
            return true;
        }
        public bool Touch(int objectId, int player, int epoch, double now)
        {
            var lease = Find(objectId);
            if (lease == null || player <= 0 || lease.Owner != player || lease.Epoch != epoch) return false;
            lease.LastUpdate = now;
            return true;
        }
        public bool Release(int objectId, int player, int epoch)
        {
            var lease = Find(objectId);
            if (lease == null || player <= 0 || lease.Owner != player || lease.Epoch != epoch) return false;
            lease.Owner = 0;
            lease.Epoch++;
            return true;
        }
        public List<int> ReleasePlayer(int player)
        {
            var released = new List<int>();
            foreach (var pair in leases)
                if (player > 0 && pair.Value.Owner == player && Release(pair.Key, player, pair.Value.Epoch)) released.Add(pair.Key);
            return released;
        }
        public List<int> Expire(double now)
        {
            var released = new List<int>();
            foreach (var pair in leases)
                if (pair.Value.Owner != 0 && now - pair.Value.LastUpdate > LeaseTimeout &&
                    Release(pair.Key, pair.Value.Owner, pair.Value.Epoch)) released.Add(pair.Key);
            return released;
        }
    }
}
