using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Environment;

namespace LighthouseKeepers.Tests
{
    public sealed class WeatherCoverageTests
    {
        [Test]
        public void BalconyCoverageIncludesEveryDirection()
        {
            for (int angle = 0; angle < 360; angle += 15)
            {
                float a = angle * Mathf.Deg2Rad;
                foreach (float radius in new[] { 5.46f, 6f, 6.45f })
                {
                    var head = new Vector3(Mathf.Cos(a) * radius, 11.25f, Mathf.Sin(a) * radius);
                    Assert.IsTrue(WeatherVolume.IsOnExposedBalcony(head), "Uncovered balcony angle " + angle);
                    Assert.IsFalse(WeatherVolume.IsSheltered(head));
                }
            }
        }

        [Test]
        public void TowerHouseAndOverhangRejectExteriorRain()
        {
            foreach (var p in new[] { new Vector3(0, 11, 0), new Vector3(5.2f, 11, 0), new Vector3(-8, 2, 0), new Vector3(-8, 3, 2.8f) })
                Assert.IsTrue(WeatherVolume.IsSheltered(p), "Rain entered protected volume " + p);
            Assert.IsFalse(WeatherVolume.IsOnExposedBalcony(new Vector3(4.8f, 11, 0)));
            Assert.IsFalse(WeatherVolume.IsOnExposedBalcony(new Vector3(-8, 1.65f, 0)));
            Assert.IsFalse(WeatherVolume.IsSheltered(new Vector3(-8, 3, 3.5f)));
        }
    }
}
