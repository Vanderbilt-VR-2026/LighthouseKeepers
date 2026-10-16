using System;
using System.Collections.Generic;
using LighthouseKeepers.Lobby;
using NUnit.Framework;
using UnityEngine;

namespace LighthouseKeepers.Tests
{
    public sealed class TestSessionTransport : MonoBehaviour, ISessionLobbyTransport
    {
        public bool IsOnline { get; private set; }
        public bool IsHost => IsOnline;
        public bool IsBusy { get; private set; }
        public bool IsStarting { get; private set; }
        public int MaxPlayers => 6;
        public string LastError { get; private set; } = "";
        public string SessionName { get; set; } = "test-room";
        public string LocalPlayerId => "1";
        readonly List<LobbyPlayer> players = new();
        public IReadOnlyList<LobbyPlayer> Players => players;
        public string StartedScene;
        public event Action PlayersChanged;
        public event Action<bool> ConnectionChanged;
        public event Action StatusChanged;
        public event Action GameStarting;
        public bool HostGame(int capacity) { IsBusy = true; StatusChanged?.Invoke(); return true; }
        public bool JoinGame(string room) => HostGame(4);
        public void CompleteConnection(bool success)
        {
            IsBusy = false; IsOnline = success;
            if (success) players.Add(new LobbyPlayer("1", "Keeper", true));
            else LastError = "Room not found.";
            ConnectionChanged?.Invoke(success); StatusChanged?.Invoke();
        }
        public void LeaveGame() { IsOnline = IsBusy = IsStarting = false; players.Clear(); ConnectionChanged?.Invoke(false); }
        public void SetLocalReady(bool ready) { players[0].IsReady = ready; PlayersChanged?.Invoke(); }
        public void SetLocalName(string name) { }
        public bool KickPlayer(string id) => false;
        public bool StartGame(string scene, int minimum, bool ready)
        {
            StartedScene = scene; IsStarting = true; GameStarting?.Invoke(); return true;
        }
        public void FailLaunch() { IsStarting = false; LastError = "Load failed."; StatusChanged?.Invoke(); }
    }
    public sealed class SessionLobbyManagerTests
    {
        GameObject root;
        LobbyManager manager;
        TestSessionTransport transport;
        [SetUp] public void Setup()
        {
            root = new GameObject("Session manager test");
            transport = root.AddComponent<TestSessionTransport>(); manager = root.AddComponent<LobbyManager>();
            TestLifecycle.Invoke(manager, "Awake");
        }
        [TearDown] public void Cleanup() => UnityEngine.Object.DestroyImmediate(root);
        [Test] public void PendingConnectionBlocksDuplicateConnectsAndStart()
        {
            Assert.IsTrue(manager.Host()); Assert.IsTrue(manager.IsBusy); Assert.IsFalse(manager.IsOnline);
            Assert.IsFalse(manager.Host()); Assert.IsFalse(manager.Join("another-room")); Assert.IsFalse(manager.CanStart);
            transport.CompleteConnection(true); Assert.IsFalse(manager.IsBusy); Assert.AreEqual(6, manager.MaxPlayers);
        }
        [Test] public void AsyncErrorsReachTheBoardAndCanBeCancelled()
        {
            manager.Host(); transport.CompleteConnection(false);
            Assert.AreEqual("Room not found.", manager.LastError);
            manager.Host(); manager.Leave(); Assert.IsFalse(manager.IsBusy); Assert.IsFalse(manager.IsOnline);
        }
        [Test] public void NetworkLaunchUsesTransportAndRecoversFromFailure()
        {
            manager.Host(); transport.CompleteConnection(true); manager.ToggleReady();
            int starts = 0; manager.OnGameStarting.AddListener(() => starts++);
            Assert.IsTrue(manager.StartGame()); Assert.AreEqual("LK_Bootstrap", transport.StartedScene);
            Assert.IsTrue(manager.IsStarting); Assert.AreEqual(1, starts); Assert.IsFalse(manager.CanStart);
            transport.FailLaunch(); Assert.IsFalse(manager.IsStarting); Assert.IsTrue(manager.CanStart);
            Assert.AreEqual("Load failed.", manager.LastError);
        }
    }
}
