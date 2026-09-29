using System;
using System.Collections.Generic;
namespace LighthouseKeepers.Lobby {
/// <summary>Seam between the lobby UI and whatever networking backs it.
/// LobbyManager and LobbyScreen only ever talk to this interface, so swapping
/// the local stub for a Netcode-for-GameObjects transport later means adding
/// one class, not rewriting the lobby.</summary>
public interface INetworkTransport {
 bool IsOnline {get;}
 bool IsHost {get;}
 string LocalPlayerId {get;}
 IReadOnlyList<LobbyPlayer> Players {get;}
 event Action PlayersChanged;
 event Action<bool> ConnectionChanged;
 bool HostGame(int maxPlayers);
 bool JoinGame(string address);
 void LeaveGame();
 void SetLocalReady(bool ready);
 void SetLocalName(string displayName);
 /// <summary>Removes a guest. Returns false when not host, offline, or targeting self.</summary>
 bool KickPlayer(string playerId);
}}
