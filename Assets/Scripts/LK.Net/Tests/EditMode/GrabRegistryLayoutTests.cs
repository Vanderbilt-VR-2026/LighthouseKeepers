using System;
using NUnit.Framework;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class GrabRegistryLayoutTests
    {
        [Test] public void ObjectDiscoveryOrderDoesNotChangeTheAgreedLayout()
        {
            var a=GrabRegistryLayout.Fingerprint(new[]{"Lab:Tools/Wrench","Lab:Radio/Mic"});
            var b=GrabRegistryLayout.Fingerprint(new[]{"Lab:Radio/Mic","Lab:Tools/Wrench"});
            Assert.AreEqual(a,b);Assert.AreEqual(64,a.Length);
            Assert.IsTrue(GrabRegistryLayout.Matches(a,b,2,2));
        }
        [Test] public void DifferentObjectSetsCannotAcquireEachOthersIds()
        {
            var a=GrabRegistryLayout.Fingerprint(new[]{"Lab:Wrench"});
            var b=GrabRegistryLayout.Fingerprint(new[]{"Lab:Mic"});
            Assert.IsFalse(GrabRegistryLayout.Matches(a,b,1,1));
            Assert.IsFalse(GrabRegistryLayout.Matches(a,a,1,2));
        }
        [Test] public void AmbiguousHierarchyPathsFailClosed()
        {
            Assert.Throws<ArgumentException>(()=>GrabRegistryLayout.Fingerprint(new[]{"Lab:Wrench","Lab:Wrench"}));
            Assert.Throws<ArgumentException>(()=>GrabRegistryLayout.Fingerprint(new[]{""}));
            Assert.Throws<ArgumentNullException>(()=>GrabRegistryLayout.Fingerprint(null));
        }
        [TestCase(null)] [TestCase("")] [TestCase("invalid")]
        public void MissingOrMalformedFingerprintsCannotConfirmARegistry(string hash)
        {Assert.IsFalse(GrabRegistryLayout.Matches(hash,hash,0,0));}
    }
}
