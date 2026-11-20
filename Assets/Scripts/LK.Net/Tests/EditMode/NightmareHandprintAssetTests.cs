using NUnit.Framework;
using UnityEngine;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class NightmareHandprintAssetTests
    {
        [TestCase(3),TestCase(9),TestCase(15),TestCase(21),TestCase(27),TestCase(33)]
        public void BakedRailMarkIsPresentAndBounded(int segment)
        {
            var mesh=Resources.Load<Mesh>("NightmareHandprints/Segment_"+segment);
            Assert.NotNull(mesh,"The player build needs each baked rail mark in Resources.");
            Assert.Greater(mesh.vertexCount,0);
            Assert.Greater(mesh.triangles.Length,0);
            Assert.LessOrEqual(mesh.bounds.size.magnitude,.20f,"A mark must not stretch between floors.");
            Assert.That(mesh.bounds.center.y,Is.InRange(4f,8f),"The mark belongs on the F2–F3 railing.");
        }
    }
}
