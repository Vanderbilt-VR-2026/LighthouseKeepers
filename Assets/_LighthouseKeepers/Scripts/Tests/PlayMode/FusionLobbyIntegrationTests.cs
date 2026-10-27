using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fusion;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using LighthouseKeepers.StormWatch;
using LighthouseKeepers.StormWatchNetworking;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
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
        [UnityTest] public IEnumerator OfflineFusionSessionLoadsLabAndSurvivesBoardUnload()
        {
            yield return SceneManager.LoadSceneAsync("LK_NetworkLobby");
            var manager = Object.FindAnyObjectByType<LobbyManager>();
            var transport = ((Component)manager.Transport).GetComponent<FusionLobbyTransport>(); sessions.Add(transport);
            Assert.AreEqual(ShipWorld.GameplaySceneName, manager.StartSceneName);
            bool accepted = (bool)typeof(FusionLobbyTransport).GetMethod("BeginConnection", PrivateInstance)
                .Invoke(transport, new object[] { GameMode.Single, "offline-integration", 2 });
            Assert.IsTrue(accepted);
            yield return Until(() => !transport.IsBusy, "Offline Fusion connection timed out.");
            Assert.IsTrue(transport.IsOnline, transport.LastError);
            Assert.IsTrue(transport.IsHost); Assert.AreEqual(1, transport.Players.Count);
            Assert.IsFalse(manager.CanStart, "Unready host must not start.");
            manager.ToggleReady(); Assert.IsTrue(manager.CanStart);
            var originalRunner = transport.Runner;
            Assert.IsTrue(manager.StartGame(), manager.LastError);
            Assert.IsFalse(manager.StartGame(), "Repeated launch must not create another world.");
            yield return Until(() => SceneManager.GetSceneByName("LK_Lab_Environment").isLoaded && Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Any(c=>c.GetType().Name=="WatchDemoController"),
                "Fusion did not launch the Sprint 2 Lab world.", 80);
            Assert.IsFalse(SceneManager.GetSceneByName("LK_NetworkLobby").isLoaded);
            Assert.IsTrue(transport && transport.IsOnline);
            Assert.AreSame(originalRunner, transport.Runner);
            Assert.AreEqual(1, Object.FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(1, Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None).Length);
            var rig = Object.FindAnyObjectByType<XROrigin>();
            var hands = rig.GetComponentsInChildren<LighthouseKeepers.Gameplay.Interaction.HandRig>(true);
            Assert.AreEqual(2, hands.Length, "Hosted play uses the same Sprint 3 hands.");
            Assert.AreNotEqual(hands[0].Node, hands[1].Node);
            Assert.IsEmpty(rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.TunnelingVignetteController>(true));
            Assert.IsNull(Object.FindAnyObjectByType<MultiplayerPanel>(), "No floating development board.");
            Assert.IsNull(Object.FindAnyObjectByType<KeeperReplicationSession>(), "No duplicate legacy replication.");
            Assert.IsNull(Object.FindAnyObjectByType<StormWatchDirector>());
            transport.LeaveGame();
            yield return Until(() => SceneManager.GetSceneByName("LK_NetworkLobby").isLoaded,
                "Leaving the environment did not return to the crew lobby.");
            yield return null;
            var returned = Object.FindAnyObjectByType<FusionLobbyTransport>(); sessions.Add(returned);
            Assert.IsFalse(returned.IsOnline); Assert.IsNull(returned.Runner);
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

        async Task<FusionLobbyTransport> CloudPeer(GameMode mode, string room, int capacity = 4)
        {
            var session = new GameObject("Cloud test " + mode).AddComponent<FusionLobbyTransport>();
            sessions.Add(session);
            // Traffic-only peers do not navigate the shared test process on exit.
            // Actual return/rejoin scene loading is covered by the Single-mode test.
            Set(session, "lobbySceneName", "TrafficOnlyLobby");
            var runner = new GameObject("Cloud test runner " + mode).AddComponent<NetworkRunner>();
            Set(session, "runner", runner);
            ((FusionLobbyState)typeof(FusionLobbyTransport).GetField("state", PrivateInstance).GetValue(session)).Reset(capacity);
            runner.AddCallbacks(session);
            var config = JsonUtility.FromJson<NetworkProjectConfig>(JsonUtility.ToJson(NetworkProjectConfig.Global));
            config.PeerMode = NetworkProjectConfig.PeerModes.Multiple;
            var result = await runner.StartGame(new StartGameArgs
            {
                GameMode = mode, SessionName = room, PlayerCount = capacity, Config = config,
                EnableClientSessionCreation = false, IsVisible = false,
                SceneManager = runner.gameObject.AddComponent<LobbyTrafficSceneManager>()
            });
            Assert.IsTrue(result.Ok, "Photon " + mode + " failed: " + result.ShutdownReason);
            Set(session, "online", true);
            return session;
        }
        [UnityTest, Category("Online")] public IEnumerator ReadyCrewLaunchesWatchOnSameRunnersAndSharesGameplay()
        {
            if (System.Environment.GetEnvironmentVariable("LK_FUSION_CLOUD_TESTS") != "1")
                Assert.Ignore("Set LK_FUSION_CLOUD_TESTS=1 for two real Photon peers.");
            string room = "LK-ConnectedWatch-" + Guid.NewGuid().ToString("N");
            var hostTask = CloudPeer(GameMode.Host, room, 2);
            yield return Until(() => hostTask.IsCompleted, "Host timed out.", 75);
            var host = hostTask.GetAwaiter().GetResult(); host.SetLocalName("Captain");
            var guestTask = CloudPeer(GameMode.Client, room, 2);
            yield return Until(() => guestTask.IsCompleted, "Guest timed out.", 75);
            var guest = guestTask.GetAwaiter().GetResult(); guest.SetLocalName("Crewmate");
            yield return Until(() => host.Players.Count == 2 && guest.Players.Count == 2, "Crew roster did not replicate.");
            host.SetLocalReady(true);
            Assert.IsFalse(host.StartGame(ShipWorld.GameplaySceneName, 1, false), "Unready guest must block launch even with relaxed caller arguments.");
            guest.SetLocalReady(true);
            yield return Until(() => host.Players.All(p => p.IsReady), "Ready state did not reach host.");
            var hostRunner = host.Runner; var guestRunner = guest.Runner;
            Assert.IsTrue(host.StartGame(ShipWorld.GameplaySceneName, 2, true), host.LastError);
            yield return Until(() => guest.IsStarting, "Guest did not receive shared launch.");
            var hostLink = new GameObject("Watch on host crew").AddComponent<StormWatchFusionLink>();
            var guestLink = new GameObject("Watch on guest crew").AddComponent<StormWatchFusionLink>();
            try
            {
                hostLink.BindSession(host); guestLink.BindSession(guest);
                Assert.AreEqual(0, hostLink.LocalSlot); Assert.AreEqual(1, guestLink.LocalSlot);
                Assert.IsFalse(hostLink.RemoteInput.connected, "Loading a scene is not yet a gameplay-ready input.");
                var input = new KeeperInput { connected = true, head = WatchStations.Pipe + Vector3.up * .5f,
                    command = WatchAction.Begin, sequence = 7, action = WatchAction.Brace, effort = 1 };
                guestLink.SendInput(input); input.command = WatchAction.None; guestLink.SendInput(input);
                yield return Until(() => hostLink.RemoteInput.sequence == 7, "Gameplay input did not use the crew connection.");
                Assert.AreEqual(WatchAction.Begin, hostLink.RemoteInput.command);
                var rules = new StormWatchRules();
                rules.Tick(.05f, new KeeperInput { connected = true, head = input.head }, hostLink.RemoteInput);
                hostLink.Publish(rules.State);
                yield return Until(() => guestLink.ReceivedState != null, "Authoritative watch did not reach the guest.");
                Assert.AreEqual(WatchAct.FirstWatch, guestLink.ReceivedState.act);
                Assert.AreEqual(rules.State.water, guestLink.ReceivedState.water);
                Assert.AreEqual("Captain", guest.Players.Single(p => p.IsHost).DisplayName);
                Assert.AreSame(hostRunner, host.Runner); Assert.AreSame(guestRunner, guest.Runner);
                Object.Destroy(hostLink.gameObject); yield return null;
                Assert.IsTrue(host.IsOnline && host.Runner.IsRunning, "Unloading gameplay must not dispose the borrowed runner.");
                guestLink.Leave();
                yield return Until(() => !guest.IsOnline && !guest.IsBusy, "Gameplay Leave did not close the lobby-owned connection.");
                Assert.IsFalse(guestLink.Connected);
                yield return Until(() => !host.IsOnline && !host.IsBusy,
                    "A partner leaving after approval must end the host session even before scene callbacks.");
                StringAssert.Contains("partner left", host.LastError);
            }
            finally
            {
                if (hostLink) Object.Destroy(hostLink.gameObject);
                if (guestLink) Object.Destroy(guestLink.gameObject);
            }
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
