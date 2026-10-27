using System.Linq;

namespace LighthouseKeepers.Editor
{
    public static class LighthouseNetworkLobby
    {
        public const string ScenePath = "Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity";
        public static string[] GameScenes => new[] { ScenePath, "Assets/Scenes/Lab/" + LighthouseKeepers.Lobby.ShipWorld.GameplaySceneName + ".unity" };
        public static string[] EditorScenes => GameScenes;

    }
}
