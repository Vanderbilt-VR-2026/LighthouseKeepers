using LighthouseKeepers.Networking;
using NUnit.Framework;

namespace LighthouseKeepers.Tests
{
    public sealed class CrewSessionPolicyTests
    {
        [TestCase("LK_StormWatch", false)]
        [TestCase("LK_Bootstrap", false)]
        [TestCase("LK_NetworkLobby", false)]
        [TestCase(null, false)]
        public void BootstrapPermitsOneKeeperForDebugging(string sceneName, bool endsSession)
        {
            Assert.AreEqual(endsSession, CrewSessionPolicy.RequiresWholeCrew(sceneName));
        }
    }
}
