using LighthouseKeepers.Lobby;
using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class CoopCommandWindowTests
    {
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
