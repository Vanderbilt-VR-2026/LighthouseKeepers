using System;
using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.Lobby {
/// <summary>Offline loopback transport: hosting and joining work with no
/// packages, no accounts and no connection. Lets the lobby screen, roster
/// rules and ready flow be built and tested now; a real Netcode transport
/// implementing INetworkTransport replaces this class when the team
/// approves a networking package. Not for production multiplayer.</summary>
public sealed class LocalLobbyTransport:MonoBehaviour,INetworkTransport {
 [SerializeField] string pendingName="Keeper";
 readonly List<LobbyPlayer> roster=new();
 int capacity=4;
 bool online,host;
 string localId;
 public event Action PlayersChanged;
 public event Action<bool> ConnectionChanged;
 public bool IsOnline=>online;
 public bool IsHost=>online&&host;
 public string LocalPlayerId=>localId;
 public IReadOnlyList<LobbyPlayer> Players=>roster;
 void Awake(){localId=Guid.NewGuid().ToString("N");}
 public bool HostGame(int maxPlayers){if(online)return false;capacity=Mathf.Clamp(maxPlayers,2,8);roster.Clear();
  roster.Add(new LobbyPlayer(localId,DisplayOrDefault(pendingName),true));SetOnline(true,true);return true;}
 public bool JoinGame(string address){if(online)return false;capacity=4;roster.Clear();
  roster.Add(new LobbyPlayer("host-simulated","Host",true));
  roster.Add(new LobbyPlayer(localId,DisplayOrDefault(pendingName)));SetOnline(true,false);return true;}
 public void LeaveGame(){if(!online)return;roster.Clear();SetOnline(false,false);}
 public void SetLocalReady(bool ready){var local=FindLocal();if(local==null)return;local.IsReady=ready;PlayersChanged?.Invoke();}
 public void SetLocalName(string displayName){pendingName=displayName;var local=FindLocal();if(local!=null){local.DisplayName=DisplayOrDefault(pendingName);PlayersChanged?.Invoke();}}
 public bool KickPlayer(string playerId){if(!IsHost||playerId==localId)return false;int removed=roster.RemoveAll(p=>p.Id==playerId);if(removed>0)PlayersChanged?.Invoke();return removed>0;}
 /// <summary>Editor/test helper: fills roster rows to preview scaling. Returns false past capacity.</summary>
 public bool AddSimulatedPlayer(string displayName){if(!online||roster.Count>=capacity)return false;
  roster.Add(new LobbyPlayer(Guid.NewGuid().ToString("N"),displayName));PlayersChanged?.Invoke();return true;}
 static string DisplayOrDefault(string name)=>string.IsNullOrWhiteSpace(name)?"Keeper":name.Trim();
 LobbyPlayer FindLocal()=>roster.Find(p=>p.Id==localId);
 void SetOnline(bool value,bool asHost){online=value;host=asHost;ConnectionChanged?.Invoke(online);PlayersChanged?.Invoke();}
}}
