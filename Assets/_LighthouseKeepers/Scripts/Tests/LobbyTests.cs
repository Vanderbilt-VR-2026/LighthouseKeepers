using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.Tests {
public sealed class LobbyTests {
 static (GameObject,LobbyManager,LocalLobbyTransport) Setup(){var go=new GameObject();var transport=go.AddComponent<LocalLobbyTransport>();var manager=go.AddComponent<LobbyManager>();return (go,manager,transport);}
 [Test] public void HostCreatesSingleHostRoster(){var (go,manager,_) = Setup();try{Assert.IsTrue(manager.Host());Assert.IsTrue(manager.IsOnline);Assert.IsTrue(manager.IsHost);Assert.AreEqual(1,manager.Players.Count);Assert.IsTrue(manager.Players[0].IsHost);}finally{Object.DestroyImmediate(go);}}
 [Test] public void SecondHostOrJoinWhileOnlineFails(){var (go,manager,_) = Setup();try{manager.Host();Assert.IsFalse(manager.Host());Assert.IsFalse(manager.Join("anywhere"));}finally{Object.DestroyImmediate(go);}}
 [Test] public void JoinLeaveFiresConnectionEvents(){var (go,manager,_) = Setup();try{int connects=0;manager.OnConnectionChanged.AddListener(_=>connects++);manager.Join("local");Assert.IsTrue(manager.IsOnline);Assert.IsFalse(manager.IsHost);manager.Leave();Assert.IsFalse(manager.IsOnline);Assert.AreEqual(0,manager.Players.Count);Assert.AreEqual(2,connects);}finally{Object.DestroyImmediate(go);}}
 [Test] public void ReadyToggleMarksLocalReady(){var (go,manager,_) = Setup();try{manager.Host();manager.ToggleReady();Assert.IsTrue(manager.LocalPlayer().IsReady);manager.ToggleReady();Assert.IsFalse(manager.LocalPlayer().IsReady);}finally{Object.DestroyImmediate(go);}}
 [Test] public void SimulatedPlayersClampedToMax(){var (go,manager,transport) = Setup();try{manager.Host();int capacity=manager.MaxPlayers;int added=0;for(int i=0;i<capacity+2;i++)if(transport.AddSimulatedPlayer($"Extra {i}"))added++;
   Assert.AreEqual(capacity-1,added);Assert.AreEqual(capacity,manager.Players.Count);}finally{Object.DestroyImmediate(go);}}
 [Test] public void HostKicksGuestButNotSelf(){var (go,manager,transport) = Setup();try{manager.Host();transport.AddSimulatedPlayer("Guest");var guest=manager.Players[1];
   Assert.IsTrue(manager.Kick(guest.Id));Assert.AreEqual(1,manager.Players.Count);
   Assert.IsFalse(manager.Kick(manager.LocalPlayer().Id));Assert.AreEqual(1,manager.Players.Count);}finally{Object.DestroyImmediate(go);}}
 [Test] public void StartRequiresOnlineHostAndAllReady(){var (go,manager,transport) = Setup();try{
   // Config-less manager: all-ready required, single host can start once ready.
   manager.Host();Assert.IsFalse(manager.CanStart);manager.ToggleReady();Assert.IsTrue(manager.CanStart);
   transport.AddSimulatedPlayer("Guest");Assert.IsFalse(manager.CanStart);}finally{Object.DestroyImmediate(go);}}
 [Test] public void LobbyConfigDefaultsAreSane(){var config=ScriptableObject.CreateInstance<LobbyConfig>();
  Assert.That(config.MaxPlayers,Is.InRange(2,8));Assert.GreaterOrEqual(config.MinPlayersToStart,1);
  Assert.IsNotEmpty(config.StartSceneName);Assert.IsNotEmpty(config.DefaultPlayerName);Object.DestroyImmediate(config);}
}}
