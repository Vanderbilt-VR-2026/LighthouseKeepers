using UnityEngine.SceneManagement;

namespace LighthouseKeepers.StormWatch
{
    public sealed class SoloWatchLink : WatchLink
    {
        readonly KeeperInput absent = new();
        bool active = true;
        public override bool ReturnsToLobby => true;
        public override bool Connected => active;
        public override bool IsHost => true;
        public override int LocalSlot => 0;
        public override bool PartnerConnected => false;
        public override string Status => "SOLO PRACTICE / OFFLINE";
        public override KeeperInput RemoteInput => absent;
        public override WatchState ReceivedState => null;
        public override void SendInput(KeeperInput input) { }
        public override void Publish(WatchState state) { }
        public override void Leave()
        {
            active = false;
            SoloWatchSession.Cancel();
            SceneManager.LoadScene("LK_NetworkLobby");
        }
    }
}
