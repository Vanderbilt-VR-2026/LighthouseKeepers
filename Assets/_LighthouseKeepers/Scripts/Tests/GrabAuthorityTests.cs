using System;
using LighthouseKeepers.Interaction;
using NUnit.Framework;

namespace LighthouseKeepers.Tests
{
    public sealed class GrabAuthorityTests
    {
        static GrabAuthority Table() { var table = new GrabAuthority(); table.Register(1); table.Register(2); return table; }
        [Test] public void TwoPlayersCannotOwnTheSameProp()
        {
            var table = Table();
            Assert.IsTrue(table.Acquire(1, 1, 0, 0));
            Assert.IsFalse(table.Acquire(1, 2, 0, 0));
            Assert.IsFalse(table.Acquire(1, 2, 1, 0));
            Assert.AreEqual(1, table.Find(1).Owner);
        }
        [Test] public void OwnerCanReleaseAndAnotherPlayerCanGrab()
        {
            var table = Table(); table.Acquire(1, 1, 0, 0);
            Assert.IsTrue(table.Release(1, 1, 1));
            Assert.IsTrue(table.Acquire(1, 2, 2, 1));
            Assert.AreEqual(2, table.Find(1).Owner);
            Assert.AreEqual(3, table.Find(1).Epoch);
        }
        [Test] public void OldPacketsCannotMoveOrReleaseANewGrab()
        {
            var table = Table(); table.Acquire(1, 1, 0, 0); table.Release(1, 1, 1); table.Acquire(1, 1, 2, 1);
            Assert.IsFalse(table.Touch(1, 1, 1, 2));
            Assert.IsFalse(table.Release(1, 1, 1));
            Assert.IsTrue(table.Touch(1, 1, 3, 2));
            Assert.AreEqual(1, table.Find(1).Owner);
        }
        [Test] public void AnotherPlayerCannotUpdateOrReleaseOwnedProp()
        {
            var table = Table(); table.Acquire(1, 1, 0, 0);
            Assert.IsFalse(table.Touch(1, 2, 1, 1));
            Assert.IsFalse(table.Release(1, 2, 1));
        }
        [Test] public void DisconnectReleasesEveryPropOwnedByThatPlayer()
        {
            var table = Table(); table.Register(3);
            table.Acquire(1, 1, 0, 0); table.Acquire(2, 1, 0, 0); table.Acquire(3, 2, 0, 0);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, table.ReleasePlayer(1));
            Assert.AreEqual(0, table.Find(1).Owner); Assert.AreEqual(0, table.Find(2).Owner);
            Assert.AreEqual(2, table.Find(3).Owner);
            Assert.IsTrue(table.Acquire(1, 2, 2, 1));
        }
        [Test] public void ActiveHeldUpdatesKeepLeaseAliveAndStalledOwnerExpires()
        {
            var table = Table(); table.Acquire(1, 1, 0, 0); table.Acquire(2, 2, 0, 0);
            Assert.IsTrue(table.Touch(1, 1, 1, 1.5));
            CollectionAssert.AreEqual(new[] { 2 }, table.Expire(2.1));
            Assert.AreEqual(1, table.Find(1).Owner); Assert.AreEqual(0, table.Find(2).Owner);
            CollectionAssert.AreEqual(new[] { 1 }, table.Expire(3.6));
        }
        [Test] public void UnknownPropsAndInvalidPlayersAreRejected()
        {
            var table = Table();
            Assert.IsFalse(table.Acquire(999, 1, 0, 0)); Assert.IsFalse(table.Acquire(1, 0, 0, 0));
            Assert.IsFalse(table.Acquire(1, -1, 0, 0)); Assert.IsFalse(table.Release(1, 0, 0));
            Assert.IsFalse(table.Touch(1, 0, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => table.Register(0));
        }
        [Test] public void RegisterPreservesALeaseAndResetStartsANewSession()
        {
            var table = Table(); table.Acquire(1, 1, 0, 0); table.Register(1);
            Assert.AreEqual(1, table.Find(1).Owner);
            table.Clear(); Assert.IsNull(table.Find(1));
            table.Register(1); Assert.IsTrue(table.Acquire(1, 2, 0, 0));
        }
    }
}
