using System;
namespace LighthouseKeepers.Lobby {
/// <summary>One roster entry. Plain data so any transport (local stub today,
/// Netcode/Relay tomorrow) can populate it without touching UI code.</summary>
[Serializable]
public sealed class LobbyPlayer {
 public string Id;
 public string DisplayName;
 public bool IsHost;
 public bool IsReady;
 public LobbyPlayer(string id,string displayName,bool isHost=false){Id=id;DisplayName=displayName;IsHost=isHost;}
 public override string ToString(){var role=IsHost?"HOST":"guest";var ready=IsReady?"READY":"not ready";return $"{DisplayName} [{role}, {ready}]";}
}}
