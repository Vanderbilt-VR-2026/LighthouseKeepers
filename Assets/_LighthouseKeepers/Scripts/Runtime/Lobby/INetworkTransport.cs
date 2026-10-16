using System;
using System.Collections.Generic;
namespace LighthouseKeepers.Lobby {
/// <summary>Seam between the lobby UI and whatever networking backs it.
/// LobbyManager and LobbyScreen use it for local practice and Photon Fusion.
/// ISessionLobbyTransport supplies asynchronous status and shared game launch.</summary>
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
