using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace LighthouseKeepers.Core.Progression.Editor
{
    public static class Sprint3ProgressionBuild
    {
        // Uses the approved scene list without rewriting any C/E-owned scene or project settings.
        [MenuItem("Lighthouse Keepers/Sprint 3/Build progression Quest APK")]
        public static void BuildQuest()
        {
            string output = "Builds/LighthouseKeepers-Sprint3-Progression.apk";
            foreach (var argument in System.Environment.GetCommandLineArgs())
                if (argument.StartsWith("-progressionApk=", StringComparison.Ordinal)) output = argument.Substring("-progressionApk=".Length);
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0) throw new BuildFailedException("No enabled scenes; C/E must integrate the Sprint 3 entry.");
            var dependencies = AssetDatabase.GetDependencies(scenes, true);
            if (!dependencies.Contains("Assets/Scripts/LK.Core/Progression/DayDirector.cs") ||
                !dependencies.Contains("Assets/Data/Days/DayCatalog.asset"))
                throw new BuildFailedException("C/E must wire DayDirector and DayCatalog into the approved scene list before publishing a progression APK. Refusing to label an unchanged legacy player as this feature.");
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? "Builds");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes, locationPathName = output, target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4
            });
            Directory.CreateDirectory("artifacts");
            File.WriteAllText("artifacts/progression-build.txt", "Result=" + report.summary.result + "\nErrors=" + report.summary.totalErrors +
                "\nWarnings=" + report.summary.totalWarnings + "\nAPK=" + output +
                "\nIntegration and headset acceptance must be verified separately.\n");
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0)
                throw new BuildFailedException("Progression APK build failed; see artifacts/progression-build.txt.");
        }
    }
}
