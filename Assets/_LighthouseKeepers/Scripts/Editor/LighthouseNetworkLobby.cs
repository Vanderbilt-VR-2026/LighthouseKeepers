using System.Linq;

namespace LighthouseKeepers.Editor
{
    public static class LighthouseNetworkLobby
    {
        public const string ScenePath = "Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity";
        public static string[] GameScenes => new[] { ScenePath, StormWatchAuthoring.Entry }.Concat(LighthouseScenes.Names.Take(7).Select(LighthouseScenes.ScenePath)).ToArray();
        public static string[] EditorScenes => new[] { ScenePath, StormWatchAuthoring.Entry }.Concat(LighthouseScenes.Names.Select(LighthouseScenes.ScenePath)).ToArray();

    }
}
