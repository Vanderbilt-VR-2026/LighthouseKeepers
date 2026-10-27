using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace LighthouseKeepers.Environment.Sprint3.Editor
{
    public sealed class ShipBuildScenes : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;
        [MenuItem("LK/Ship/Apply Build Scenes")]
        public static void Apply()
        {
            var paths = ShipSceneCatalog.Paths;
            Validate(paths);
            EditorBuildSettings.scenes = paths.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
        }
        public static void Validate(string[] paths)
        {
            if (paths.Length != 14 || paths.Distinct().Count() != paths.Length ||
                !paths[0].EndsWith("/LK_Lobby_Environment.unity", StringComparison.Ordinal) || paths[1] != ShipSceneCatalog.ScenePath)
                throw new BuildFailedException("Ship requires lobby at 0, House at 1, all six Shell/Dress pairs and existing NetworkLobby.");
            foreach (var path in paths)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.Contains("StormWatch") || name.Contains("Lab") || name.Contains("DevGym") || name.Contains("Level"))
                    throw new BuildFailedException("Non-ship scene: " + path);
            }
            foreach (string floor in new[] { "House", "F1", "F2", "F3", "F4", "Exterior" })
                foreach (string kind in new[] { "Shell", "Dress" })
                    if (!paths.Contains($"Assets/Scenes/{kind}/LK_{kind}_{floor}.unity"))
                        throw new BuildFailedException("Missing ship floor: " + kind + "/" + floor);
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            var paths = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            Validate(paths);
            if (EditorBuildSettings.scenes.Any(s => !s.enabled) || !paths.SequenceEqual(ShipSceneCatalog.Paths))
                throw new BuildFailedException("Build Settings differ from ShipScenes.json. Run LK/Ship/Apply Build Scenes.");
        }
    }
}
