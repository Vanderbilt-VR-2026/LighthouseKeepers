using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
namespace LighthouseKeepers.Lobby {
/// <summary>Owns lobby state and rules; never touches networking directly.
/// Bind any MonoBehaviour implementing INetworkTransport (defaults to the
/// local loopback). UI reads this class, so it is the only scaling point
/// for roster rules, start conditions and scene flow.</summary>
public sealed class LobbyManager:MonoBehaviour {
 const int FallbackMaxPlayers=4;
 const string FallbackScene="LK_Bootstrap";
 [SerializeField] LobbyConfig config;
 [SerializeField,Tooltip("Transport implementing INetworkTransport. Defaults to LocalLobbyTransport on this object.")] MonoBehaviour transportBehaviour;
 [SerializeField] string playerName="";
 [SerializeField] UnityEvent onRosterChanged=new();
 [SerializeField] UnityEvent<bool> onConnectionChanged=new();
 [SerializeField] UnityEvent onGameStarting=new();
 INetworkTransport transport;
 public INetworkTransport Transport=>transport;
 public UnityEvent OnRosterChanged=>onRosterChanged;
 public UnityEvent<bool> OnConnectionChanged=>onConnectionChanged;
 public UnityEvent OnGameStarting=>onGameStarting;
 public bool IsOnline=>transport!=null&&transport.IsOnline;
 public bool IsHost=>transport!=null&&transport.IsHost;
 public int MaxPlayers=>config?config.MaxPlayers:FallbackMaxPlayers;
 public int MinPlayersToStart=>config?config.MinPlayersToStart:1;
 public bool RequireAllReady=>config==null||config.RequireAllReady;
 public string StartSceneName=>config?config.StartSceneName:FallbackScene;
 public System.Collections.Generic.IReadOnlyList<LobbyPlayer> Players=>transport!=null?transport.Players:System.Array.Empty<LobbyPlayer>();
 public bool CanStart=>IsOnline&&IsHost&&Players.Count>=MinPlayersToStart&&(!RequireAllReady||Players.All(p=>p.IsReady));
 void Awake(){ResolveTransport();}
 public bool Host(){if(transport==null||IsOnline)return false;ApplyPendingName();return transport.HostGame(MaxPlayers);}
 public bool Join(string address){if(transport==null||IsOnline)return false;ApplyPendingName();return transport.JoinGame(address);}
 public void Leave(){transport?.LeaveGame();}
 public void ToggleReady(){var local=LocalPlayer();if(local!=null)transport.SetLocalReady(!local.IsReady);}
 public bool Kick(string playerId)=>transport!=null&&transport.KickPlayer(playerId);
 public LobbyPlayer LocalPlayer(){if(transport==null)return null;return Players.FirstOrDefault(p=>p.Id==transport.LocalPlayerId);}
 /// <summary>Returns false unless start conditions hold; never loads a scene from tests.</summary>
 public bool StartGame(){if(!CanStart)return false;onGameStarting.Invoke();SceneManager.LoadScene(StartSceneName,LoadSceneMode.Single);return true;}
 void ResolveTransport(){foreach(var mb in GetComponents<MonoBehaviour>()){if(mb is INetworkTransport t){transport=t;break;}}
  if(transport==null){if(transportBehaviour is INetworkTransport t)transport=t;else transport=gameObject.AddComponent<LocalLobbyTransport>();}
  transport.PlayersChanged+=()=>onRosterChanged.Invoke();
  transport.ConnectionChanged+=online=>onConnectionChanged.Invoke(online);}
 public string PlayerName{get=>string.IsNullOrWhiteSpace(playerName)?(config?config.DefaultPlayerName:"Keeper"):playerName;set{playerName=value;transport?.SetLocalName(PlayerName);}}
 void ApplyPendingName(){transport?.SetLocalName(PlayerName);}
}}
