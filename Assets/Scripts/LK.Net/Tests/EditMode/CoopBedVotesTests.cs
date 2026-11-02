using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class CoopBedVotesTests
    {
        [Test] public void EveryConnectedKeeperMustVoteAndDuplicateCannotReplacePartner()
        {
            var bed=new CoopBedVotes();Assert.IsFalse(bed.AllReady(new int[0]));
            Assert.IsFalse(bed.Add(1,false));Assert.IsFalse(bed.Add(0,true));
            Assert.IsTrue(bed.Add(1,true));Assert.IsFalse(bed.Add(1,true));
            Assert.AreEqual(1,bed.Count);Assert.IsFalse(bed.AllReady(new[]{1,2}));
            Assert.IsTrue(bed.Add(2,true));Assert.IsTrue(bed.AllReady(new[]{1,2}));
            bed.Clear();Assert.IsFalse(bed.AllReady(new[]{1,2}));
        }
        [Test] public void DisconnectRemovesVoteAndRemainingKeeperCanContinueAlone()
        {
            var bed=new CoopBedVotes();bed.Add(1,true);bed.Add(2,true);bed.Remove(2);
            Assert.AreEqual(1,bed.Count);Assert.IsTrue(bed.AllReady(new[]{1}));
            Assert.IsFalse(bed.AllReady(new[]{1,3}));
        }
    }
}
