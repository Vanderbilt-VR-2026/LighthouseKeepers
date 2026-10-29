namespace LighthouseKeepers.Lobby
{
    // Both launch modes use this world; the Sprint 2 Lab shell owns its WatchDemo loop.
    public static class ShipWorld
    {
        public const string GameplaySceneName = "LK_Lab_Environment";
        // Experimental branch switch. Solo never depends on this flag.
        public static bool CoopEnabled { get; set; } = true;
        public const int MaximumCrewSize = 2;
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCoop() => CoopEnabled = true;
        public const string LobbyScenePath = "Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity";
        public const string GameplayScenePath = "Assets/Scenes/Lab/" + GameplaySceneName + ".unity";
    }
}
