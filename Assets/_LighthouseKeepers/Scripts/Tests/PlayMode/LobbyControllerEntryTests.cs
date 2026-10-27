using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Tests
{
    public sealed class ControllerEntryTestTransport : MonoBehaviour, ISessionLobbyTransport, ILobbyLaunchPolicy
    {
        public bool IsOnline { get; private set; }
        public bool IsHost => IsOnline;
        public bool IsBusy => false;
        public bool IsStarting { get; private set; }
        public int MaxPlayers => 2;
        public string LastError => "";
        public string SessionName { get; set; } = "ABC234";
        public string LocalPlayerId => "local";
        readonly List<LobbyPlayer> players = new();
        public IReadOnlyList<LobbyPlayer> Players => players;
        public event Action PlayersChanged;
        public event Action<bool> ConnectionChanged;
        public event Action StatusChanged { add { } remove { } }
        public event Action GameStarting;
        public bool HostGame(int capacity)
        { IsOnline = true; players.Add(new LobbyPlayer("local", "Keeper", true)); ConnectionChanged?.Invoke(true); return true; }
        public bool JoinGame(string room) => false;
        public void LeaveGame() { IsOnline = IsStarting = false; players.Clear(); ConnectionChanged?.Invoke(false); }
        public void SetLocalName(string name) { }
        public void SetLocalReady(bool ready) { players[0].IsReady = ready; PlayersChanged?.Invoke(); }
        public bool KickPlayer(string id) => false;
        public string StartBlockReason(string scene) => CrewSessionPolicy.GameplayLaunchBlockReason(scene, Players.Count);
        public bool StartGame(string scene, int minimum, bool allReady)
        {
            if (!string.IsNullOrEmpty(StartBlockReason(scene))) return false;
            IsStarting = true; GameStarting?.Invoke(); return true;
        }
        public void AddReadyGuest()
        { players.Add(new LobbyPlayer("guest", "Guest", false) { IsReady = true }); PlayersChanged?.Invoke(); }
    }
    public sealed class LobbyControllerEntryTests
    {
        GameObject root;
        LobbyConfig config;
        LobbyManager manager;
        ControllerEntryTestTransport transport;
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [UnitySetUp] public IEnumerator Setup()
        {
            root = new GameObject("Lobby launch policy fixture"); root.SetActive(false);
            transport = root.AddComponent<ControllerEntryTestTransport>(); manager = root.AddComponent<LobbyManager>();
            config = ScriptableObject.CreateInstance<LobbyConfig>();
            typeof(LobbyManager).GetField("config", Private).SetValue(manager, config);
            root.SetActive(true); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        { Object.Destroy(root); Object.Destroy(config); yield return null; }
        [UnityTest] public IEnumerator HostAloneKeepsReadyGateAndCanEnterLabForDebugging()
        {
            manager.Host(); Assert.IsFalse(manager.CanStart);
            manager.ToggleReady(); Assert.IsTrue(manager.CanStart);
            Assert.IsTrue(manager.StartGame()); Assert.IsTrue(manager.IsStarting);
            yield return null;
        }
        [UnityTest] public IEnumerator TwoReadyKeepersStayConnectedButCannotEnterUnreplicatedLab()
        {
            manager.Host(); manager.ToggleReady(); transport.AddReadyGuest();
            Assert.IsFalse(manager.CanStart); Assert.IsFalse(manager.StartGame());
            Assert.AreEqual(CrewSessionPolicy.SharedGameStatePending, manager.StartHint);
            Assert.IsTrue(manager.IsOnline); Assert.AreEqual(2, manager.Players.Count);
            Assert.IsTrue(manager.LocalPlayer().IsReady);
            yield return null;
        }
        [UnityTest] public IEnumerator HistoricalSceneRetainsItsReadinessAndLaunchRules()
        {
            typeof(LobbyConfig).GetField("startSceneName", Private).SetValue(config, "LK_Bootstrap");
            manager.Host(); manager.ToggleReady(); transport.AddReadyGuest();
            Assert.IsTrue(manager.CanStart); Assert.IsTrue(manager.StartGame());
            yield return null;
        }
    }
}
