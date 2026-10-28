namespace LighthouseKeepers.Lobby
{
    // Both launch modes use this world; the Sprint 2 Lab shell owns its WatchDemo loop.
    public static class ShipWorld
    {
        public const string GameplaySceneName = "LK_Lab_Environment";
        public const int MaximumCrewSize = 4;
        public const string LobbyScenePath = "Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity";
        public const string GameplayScenePath = "Assets/Scenes/Lab/" + GameplaySceneName + ".unity";
    }
}
