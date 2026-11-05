using LighthouseKeepers.Lobby;
using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class CoopCommandWindowTests
    {
        [Test] public void EveryDeclaredActionIncludingWrenchClutchAcceptsFreshInputAndRejectsReplay()
        {
            var window=new CoopCommandWindow();int sequence=0;
            foreach(CoopAction action in System.Enum.GetValues(typeof(CoopAction)))
            {
                ++sequence;
                Assert.True(window.Accept(1,sequence,2,2,action,0),"Rejected declared action "+action);
                Assert.False(window.Accept(1,sequence,2,2,action,0),"Accepted replay "+action);
            }
            Assert.False(window.Accept(1,sequence+1,1,2,CoopAction.WrenchClutch,0),"Stale-day clutch");
            Assert.True(window.Accept(1,sequence+1,2,2,CoopAction.WrenchClutch,0),"Rejected fresh clutch after stale input");
        }
        [Test] public void RejectsOldDayReplayInvalidActorAndNonfiniteInputWithoutConsumingValidSequence()
        {
            var window=new CoopCommandWindow();
            Assert.IsFalse(window.Accept(0,1,2,2,CoopAction.Starter,0));
            Assert.IsFalse(window.Accept(1,1,1,2,CoopAction.Starter,0));
            Assert.IsFalse(window.Accept(1,1,2,2,CoopAction.RadioTune,float.NaN));
            Assert.IsFalse(window.Accept(1,1,2,2,(CoopAction)99,0));
            Assert.IsTrue(window.Accept(1,1,2,2,CoopAction.Starter,0));
            Assert.IsFalse(window.Accept(1,1,2,2,CoopAction.Starter,0));
            Assert.IsTrue(window.Accept(2,1,2,2,CoopAction.Starter,0));
            Assert.IsFalse(window.Accept(1,2,2,3,CoopAction.Bed,0));
            Assert.IsTrue(window.Accept(1,2,3,3,CoopAction.Bed,0));
            window.Remove(1);Assert.IsTrue(window.Accept(1,1,3,3,CoopAction.Starter,0));
        }
    }
}
