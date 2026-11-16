using System;
using UnityEngine;

namespace LighthouseKeepers.Core.Progression
{
    [Serializable] public sealed class FlareShot
    {
        public bool fired, stopped, repelled;
        public double at;
        public float stopAge;
        public Vector3 origin, direction, stop;
        public FlareShot Copy() => (FlareShot)MemberwiseClone();
        public static bool Finite(Vector3 p) => float.IsFinite(p.x) && float.IsFinite(p.y) && float.IsFinite(p.z);
        public bool Valid(double now)
        {
            if (!double.IsFinite(at) || at < 0 || at > now + .001 || !float.IsFinite(stopAge) || stopAge < 0 || stopAge > FlareState.FlightSeconds) return false;
            if (!Finite(origin) || !Finite(direction) || !Finite(stop) || origin.sqrMagnitude > 10000 || stop.sqrMagnitude > 100000) return false;
            if (!fired) return !stopped && !repelled && at == 0 && stopAge == 0 && origin == Vector3.zero && direction == Vector3.zero && stop == Vector3.zero;
            return Mathf.Abs(direction.sqrMagnitude - 1) < .01f && (!repelled || stopped) && (!stopped || stopAge <= now-at+.05) && (stopped || stopAge == 0 && stop == Vector3.zero);
        }
    }
    [Serializable] public sealed class FlareFrame
    {
        public bool enabled;
        public FlareShot first = new(), second = new();
        public bool Valid(double now) => first != null && second != null && first.Valid(now) && second.Valid(now) && (enabled || !first.fired && !second.fired);
    }
    // Two physical guns share the night's limited supply. Only the host spends rounds and
    // resolves obstruction/repulsion. Clients reproduce the last flight from its timestamp.
    public sealed class FlareState
    {
        public const float Speed = 14, Gravity = 3.5f, FlightSeconds = 6, BurnSeconds = 3;
        public bool ReadOnly { get; set; }
        FlareFrame frame = new() { enabled = true };
        public FlareShot Shot(int slot) => slot == 0 ? frame.first.Copy() : slot == 1 ? frame.second.Copy() : null;
        public bool Loaded(int slot) => slot >= 0 && slot < 2 && !Shot(slot).fired;
        public bool Fire(int slot, Vector3 muzzle, Vector3 direction, double now)
        {
            if (ReadOnly || !Loaded(slot) || !FlareShot.Finite(direction) || direction.sqrMagnitude < .9f || !FlareShot.Finite(muzzle)) return false;
            var shot = new FlareShot { fired = true, at = now, origin = muzzle, direction = direction.normalized };
            if (!shot.Valid(now)) return false;
            if (slot == 0) frame.first = shot; else frame.second = shot;
            return true;
        }
        public bool Stop(int slot, Vector3 position, float age, bool repelled)
        {
            var shot = Shot(slot);
            if (ReadOnly || shot == null || !shot.fired || shot.stopped || !FlareShot.Finite(position) || position.sqrMagnitude > 100000 || !float.IsFinite(age) || age < 0 || age > FlightSeconds) return false;
            shot.stopped = true; shot.stop = position; shot.stopAge = age; shot.repelled = repelled;
            if (slot == 0) frame.first = shot; else frame.second = shot;
            return true;
        }
        public static Vector3 Position(FlareShot shot, float age) => shot.stopped && age >= shot.stopAge ? shot.stop : shot.origin + shot.direction * (Speed * Mathf.Clamp(age, 0, FlightSeconds)) + Vector3.down * (.5f * Gravity * Mathf.Pow(Mathf.Clamp(age, 0, FlightSeconds), 2));
        public static bool NearSegment(Vector3 a, Vector3 b, Vector3 point, float radius)
        {
            var d = b - a; float t = d.sqrMagnitude > .00001f ? Mathf.Clamp01(Vector3.Dot(point - a, d) / d.sqrMagnitude) : 0;
            return (a + d * t - point).sqrMagnitude <= radius * radius;
        }
        public FlareFrame Capture() => new() { enabled = true, first = frame.first.Copy(), second = frame.second.Copy() };
        public bool Apply(FlareFrame value, double now)
        {
            if (!ReadOnly || value == null || !value.enabled || !value.Valid(now)) return false;
            frame = new FlareFrame { enabled = true, first = value.first.Copy(), second = value.second.Copy() }; return true;
        }
    }
}
