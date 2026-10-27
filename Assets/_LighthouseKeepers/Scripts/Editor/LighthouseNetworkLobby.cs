namespace LighthouseKeepers.Editor
{
    public static class LighthouseNetworkLobby
    {
        public const string ScenePath = LighthouseKeepers.Lobby.ShipWorld.LobbyScenePath;
        public static string[] GameScenes => QuestBuild.ShipSceneManifest.Scenes;
        public static string[] EditorScenes => GameScenes;
    }
}
