using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace LighthouseKeepers.Editor
{
    public static class Sprint2Integration
    {
        // Diagnostic integration build only; no authoring/reset helpers run here.
        public static void BuildDiagnosticAndroid()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled && !s.path.Contains("/Development/"))
                .Select(s => s.path).ToArray();
            if (scenes.Length != 7 || !scenes[0].EndsWith("/LK_Bootstrap.unity"))
                throw new InvalidOperationException("Unexpected environment build scene list.");
            Directory.CreateDirectory("Builds");
            Directory.CreateDirectory("artifacts/verification");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/LighthouseKeepers-Sprint2-Integration.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });
            File.WriteAllText("artifacts/verification/integration-build.txt",
                $"Result={report.summary.result}\nErrors={report.summary.totalErrors}\nWarnings={report.summary.totalWarnings}\nBytes={report.summary.totalSize}\n");
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Integration APK failed. Inspect build log.");
        }

        [MenuItem("Lighthouse Keepers/Sprint 2/Preserve recovery scene")]
        public static void PreserveRecovery()
        {
            const string source = "Assets/_Recovery/0.unity";
            const string target = "Assets/_Recovery/LK_Recovery.unity";
            const string expectedGuid = "8790e0acdc4c38148b27e8188cfc3b3a";
            if (File.Exists(source))
            {
                if (File.Exists(target)) throw new InvalidOperationException("Recovery target already exists; preserve both scenes for review.");
                var error = AssetDatabase.MoveAsset(source, target);
                if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            }
            if (AssetDatabase.AssetPathToGUID(target) != expectedGuid)
                throw new InvalidOperationException("William's recovery scene GUID was not preserved.");
            // Do not load this scene with the environment: it has its own camera.
            // Do not regenerate Bootstrap or alter the build scene list.
            AssetDatabase.SaveAssets();
        }
    }
}
