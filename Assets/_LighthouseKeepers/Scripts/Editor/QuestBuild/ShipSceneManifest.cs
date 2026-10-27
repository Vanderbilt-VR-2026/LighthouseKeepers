using System;
using System.Linq;
using LighthouseKeepers.Lobby;
using UnityEditor;
using UnityEditor.Build;

namespace LighthouseKeepers.Editor.QuestBuild
{
    public static class ShipSceneManifest
    {
        // Return a fresh array so a caller cannot edit the shared build policy in place.
        public static string[] Scenes => new[] { ShipWorld.LobbyScenePath, ShipWorld.GameplayScenePath };

        public static void Validate(BuildPlayerOptions options)
        {
            if (options.scenes == null || !options.scenes.SequenceEqual(Scenes, StringComparer.Ordinal))
                throw new BuildFailedException("The ship APK must include exactly LK_NetworkLobby followed by LK_Lab_Environment. " +
                    "Validate the actual BuildPlayerOptions.scenes; removing /Lab/ creates a lobby-only APK.");
        }
    }

    // BuildReport preprocessors cannot inspect the caller's scene array. This Unity 6
    // callback receives the actual options, including custom BuildPipeline callers.
    public sealed class ShipSceneBuildProcessor : BuildPlayerProcessor
    {
        public override void PrepareForBuild(BuildPlayerContext context)
        {
            if (context.BuildPlayerOptions.target == BuildTarget.Android)
                ShipSceneManifest.Validate(context.BuildPlayerOptions);
        }
    }
}
