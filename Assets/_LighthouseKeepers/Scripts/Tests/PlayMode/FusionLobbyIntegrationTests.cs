using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fusion;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Assert = NUnit.Framework.Assert;

namespace LighthouseKeepers.Tests
{
    // These peers test real transport traffic without loading three copies of the
    // XR environment. The production/default scene manager is tested separately.
    public sealed class LobbyTrafficSceneManager : MonoBehaviour, INetworkSceneManager
    {
        Scene scene;
        public bool IsBusy => false;
        public Scene MainRunnerScene => scene;
        public void Initialize(NetworkRunner runner) => scene = SceneManager.CreateScene(
            "Lobby traffic " + Guid.NewGuid().ToString("N"),
            new CreateSceneParameters(LocalPhysicsMode.Physics2D | LocalPhysicsMode.Physics3D));
        public void Shutdown() { if (scene.IsValid() && scene.isLoaded) SceneManager.UnloadSceneAsync(scene); }
        public bool IsRunnerScene(Scene candidate) => candidate == scene;
        public bool TryGetPhysicsScene2D(out PhysicsScene2D physics) { physics = scene.GetPhysicsScene2D(); return physics.IsValid(); }
        public bool TryGetPhysicsScene3D(out PhysicsScene physics) { physics = scene.GetPhysicsScene(); return physics.IsValid(); }
        public void MakeDontDestroyOnLoad(GameObject target) => DontDestroyOnLoad(target);
        public bool MoveGameObjectToScene(GameObject target, SceneRef reference) { SceneManager.MoveGameObjectToScene(target, scene); return true; }
        public NetworkSceneAsyncOp LoadScene(SceneRef reference, NetworkLoadSceneParameters parameters) => NetworkSceneAsyncOp.FromCompleted(reference);
        public NetworkSceneAsyncOp UnloadScene(SceneRef reference) => NetworkSceneAsyncOp.FromCompleted(reference);
        public SceneRef GetSceneRef(GameObject target) => SceneRef.None;
        public SceneRef GetSceneRef(string name) => SceneRef.None;
        public bool OnSceneInfoChanged(NetworkSceneInfo info, NetworkSceneInfoChangeSource source) => true;
    }
    public sealed class FusionLobbyIntegrationTests
    {
        readonly List<FusionLobbyTransport> sessions = new();
        const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        static void Set(FusionLobbyTransport transport, string field, object value) =>
            typeof(FusionLobbyTransport).GetField(field, PrivateInstance).SetValue(transport, value);
        static IEnumerator Until(Func<bool> condition, string failure, float timeout = 40)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(condition(), failure);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var session in sessions.Where(s => s)) session.LeaveGame();
            yield return Until(() => sessions.Where(s => s).All(s => !s.IsBusy), "Session shutdown timed out.");
            foreach (var session in sessions.Where(s => s)) Object.Destroy(session.gameObject);
            sessions.Clear(); yield return null;
        }
        [UnityTest] public IEnumerator OfflineFusionSessionLoadsBootstrapAndSurvivesBoardUnload()
        {
            yield return SceneManager.LoadSceneAsync("LK_NetworkLobby");
            var transport = Object.FindAnyObjectByType<FusionLobbyTransport>(); sessions.Add(transport);
            var manager = Object.FindAnyObjectByType<LobbyManager>();
            Assert.AreSame(transport, manager.Transport);
            // Fusion Single exercises the real runner and scene manager without a Photon account.
            bool accepted = (bool)typeof(FusionLobbyTransport).GetMethod("BeginConnection", PrivateInstance)
                .Invoke(transport, new object[] { GameMode.Single, "offline-integration", 4 });
            Assert.IsTrue(accepted);
            yield return Until(() => !transport.IsBusy, "Offline Fusion connection timed out.");
            Assert.IsTrue(transport.IsOnline, transport.LastError);
            Assert.IsTrue(transport.IsHost); Assert.AreEqual(1, transport.Players.Count);
            manager.ToggleReady(); Assert.IsTrue(manager.CanStart);
            Assert.IsTrue(manager.StartGame(), manager.LastError);
            yield return Until(() => SceneManager.GetSceneByName("LK_Level04_Lantern").isLoaded,
                "Fusion did not launch Bootstrap and its content scenes.");
            Assert.IsFalse(SceneManager.GetSceneByName("LK_NetworkLobby").isLoaded);
            Assert.IsTrue(transport && transport.IsOnline, "Session was destroyed with the watch board.");
            Assert.AreEqual(1, Object.FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None).Length);
            transport.LeaveGame();
            yield return Until(() => SceneManager.GetSceneByName("LK_NetworkLobby").isLoaded,
                "Leaving the environment did not return to the crew lobby.");
            yield return null;
            var returned = Object.FindAnyObjectByType<FusionLobbyTransport>(); sessions.Add(returned);
            Assert.IsFalse(returned.IsOnline);
        }

        [UnityTest] public IEnumerator DefaultLobbyUsesPhotonInsteadOfLocalSimulation()
        {
            var board = new GameObject("Default lobby test");
            try
            {
                var manager = board.AddComponent<LobbyManager>();
                Assert.IsInstanceOf<FusionLobbyTransport>(manager.Transport);
                Assert.IsFalse(manager.IsLocalPreview);
                Assert.IsNull(board.GetComponent<LocalLobbyTransport>());
                sessions.Add((FusionLobbyTransport)manager.Transport);
                yield return null;
            }
            finally { Object.Destroy(board); }
        }

        async Task<FusionLobbyTransport> CloudPeer(GameMode mode, string room)
        {
            var session = new GameObject("Cloud test " + mode).AddComponent<FusionLobbyTransport>();
            sessions.Add(session);
            var runner = new GameObject("Cloud test runner " + mode).AddComponent<NetworkRunner>();
            Set(session, "runner", runner);
            runner.AddCallbacks(session);
            var config = JsonUtility.FromJson<NetworkProjectConfig>(JsonUtility.ToJson(NetworkProjectConfig.Global));
            config.PeerMode = NetworkProjectConfig.PeerModes.Multiple;
            var result = await runner.StartGame(new StartGameArgs
            {
                GameMode = mode, SessionName = room, PlayerCount = 4, Config = config,
                EnableClientSessionCreation = false, IsVisible = false,
                SceneManager = runner.gameObject.AddComponent<LobbyTrafficSceneManager>()
            });
            Assert.IsTrue(result.Ok, "Photon " + mode + " failed: " + result.ShutdownReason);
            Set(session, "online", true);
            return session;
        }
        [UnityTest] public IEnumerator PhotonPeersShareRosterReadyLateJoinKickAndDisconnect()
        {
            if (System.Environment.GetEnvironmentVariable("LK_FUSION_CLOUD_TESTS") != "1")
                Assert.Ignore("Set LK_FUSION_CLOUD_TESTS=1 to opt into temporary Photon Cloud rooms.");
            string room = "LK-Issue33-" + Guid.NewGuid().ToString("N");
            var hostTask = CloudPeer(GameMode.Host, room);
            yield return Until(() => hostTask.IsCompleted, "Photon host timed out.", 75);
            var host = hostTask.GetAwaiter().GetResult(); host.SetLocalName("Captain");
            var guestTask = CloudPeer(GameMode.Client, room);
            yield return Until(() => guestTask.IsCompleted, "Photon guest timed out.", 75);
            var guest = guestTask.GetAwaiter().GetResult(); guest.SetLocalName("Guest");
            yield return Until(() => host.Players.Count == 2 && guest.Players.Count == 2 &&
                guest.Players.Any(p => p.DisplayName == "Captain") && host.Players.Any(p => p.DisplayName == "Guest"),
                "Host and guest did not receive the shared roster.");
            host.SetLocalReady(true); guest.SetLocalReady(true);
            yield return Until(() => host.Players.All(p => p.IsReady) && guest.Players.All(p => p.IsReady),
                "Ready state did not replicate.");
            var lateTask = CloudPeer(GameMode.Client, room);
            yield return Until(() => lateTask.IsCompleted, "Late join timed out.", 75);
            var late = lateTask.GetAwaiter().GetResult(); late.SetLocalName("Late keeper");
            yield return Until(() => late.Players.Count == 3 && guest.Players.Count == 3 &&
                late.Players.Count(p => p.IsReady) == 2, "Late join did not receive current ready states.");
            Assert.IsFalse(guest.KickPlayer(host.LocalPlayerId));
            Assert.IsTrue(host.KickPlayer(late.LocalPlayerId));
            yield return Until(() => !late.IsOnline && host.Players.Count == 2 && guest.Players.Count == 2,
                "Host kick did not remove the guest on all peers.");
            host.LeaveGame();
            yield return Until(() => !host.IsOnline && !guest.IsOnline && !guest.IsBusy,
                "Host departure did not clean up the client session.");
            Assert.AreEqual(0, guest.Players.Count);
        }
    }
}
