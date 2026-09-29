using UnityEngine;
namespace LighthouseKeepers.Lobby {
[CreateAssetMenu(menuName="Lighthouse Keepers/Lobby Config")]
public sealed class LobbyConfig:ScriptableObject {
 [SerializeField,Range(2,8)] int maxPlayers=4;
 [SerializeField,Min(1)] int minPlayersToStart=1;
 [SerializeField] string defaultPlayerName="Keeper";
 [SerializeField] string lobbyTitle="Lighthouse Keepers";
 [SerializeField,Tooltip("When true every roster member must be ready before the host can start.")] bool requireAllReady=true;
 [SerializeField,Tooltip("Scene loaded for everyone when the host starts the game.")] string startSceneName="LK_Bootstrap";
 public int MaxPlayers=>Mathf.Clamp(maxPlayers,2,8);
 public int MinPlayersToStart=>Mathf.Max(1,minPlayersToStart);
 public string DefaultPlayerName=>string.IsNullOrWhiteSpace(defaultPlayerName)?"Keeper":defaultPlayerName;
 public string LobbyTitle=>string.IsNullOrWhiteSpace(lobbyTitle)?"Lighthouse Keepers":lobbyTitle;
 public bool RequireAllReady=>requireAllReady;
 public string StartSceneName=>string.IsNullOrWhiteSpace(startSceneName)?"LK_Bootstrap":startSceneName;
}}
