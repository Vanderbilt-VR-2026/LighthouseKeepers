using UnityEngine;

namespace LighthouseKeepers.Audio
{
    /// <summary>Distance-based cadence; pauses, falls and discontinuities start a fresh stride.</summary>
    public sealed class FootstepCadence
    {
        float distance;

        public void Reset() => distance = 0;

        public bool Advance(float travelled, float deltaTime, bool grounded, float strideLength)
        {
            // Reject teleports/recentering and long suspended frames rather than playing catch-up steps.
            if (!grounded || deltaTime <= 0 || deltaTime > .25f ||
                travelled < .08f * deltaTime || travelled > Mathf.Min(.5f, 4f * deltaTime))
            {
                Reset();
                return false;
            }

            distance += travelled;
            strideLength = Mathf.Max(.1f, strideLength);
            if (distance < strideLength) return false;
            distance %= strideLength;
            return true;
        }
    }
}
