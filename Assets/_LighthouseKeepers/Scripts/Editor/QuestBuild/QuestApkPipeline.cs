using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LighthouseKeepers.Editor.QuestBuild
{
    public static class QuestApkPipeline
    {
        [Serializable]
        sealed class Evidence
        {
            public string apk, sourceCommit, sourceStatus, unityVersion, playerVersion, startedUtc, finishedUtc, result, buildOptions;
            public string[] scenes;
            public int versionCode, errors, warnings;
        }

        public static void Build(string[] scenes, BuildOptions options = BuildOptions.Development)
        {
            ShipSceneManifest.Validate(new BuildPlayerOptions { scenes = scenes, target = BuildTarget.Android });
            QuestAndroidBuildGuard.ValidateSettings();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new BuildFailedException("Switch the active build target to Android first (CLI: -buildTarget Android).");
            if (scenes.Any(scene => !File.Exists(scene)))
                throw new BuildFailedException("Both committed ship scenes must exist before building; run authoring separately and review the result.");
            var started = DateTime.UtcNow;
            var commit = Run("git", "rev-parse HEAD").Trim();
            var status = Run("git", "status --porcelain=v1 --untracked-files=all").Trim();
            var stamp = "Sprint3-" + commit.Substring(0, 8) + "-" + started.ToString("yyyyMMddTHHmmssZ");
            var output = Environment.GetEnvironmentVariable("LK_APK_OUTPUT");
            if (string.IsNullOrWhiteSpace(output)) output = Path.Combine("Builds", stamp, "LighthouseKeepers-" + stamp + ".apk");
            output = Path.GetFullPath(output);
            if (!output.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) || File.Exists(output))
                throw new BuildFailedException("Choose a fresh .apk output path (LK_APK_OUTPUT). Existing binaries are never reused: " + output);
            if (EditorUserBuildSettings.buildAppBundle || EditorUserBuildSettings.exportAsGoogleAndroidProject)
                throw new BuildFailedException("Disable App Bundle and Export Project; this command must produce an installable APK.");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var playerOptions = new BuildPlayerOptions
            {
                scenes = scenes, target = BuildTarget.Android, locationPathName = output,
                options = options | BuildOptions.CleanBuildCache
            };
            ShipSceneManifest.Validate(playerOptions);
            var report = BuildPipeline.BuildPlayer(playerOptions);
            var record = new Evidence
            {
                apk = output, sourceCommit = commit, sourceStatus = status, unityVersion = Application.unityVersion,
                playerVersion = PlayerSettings.bundleVersion, versionCode = PlayerSettings.Android.bundleVersionCode,
                scenes = scenes, buildOptions = playerOptions.options.ToString(),
                startedUtc = started.ToString("O"), finishedUtc = DateTime.UtcNow.ToString("O"),
                result = report.summary.result.ToString(), errors = (int)report.summary.totalErrors,
                warnings = (int)report.summary.totalWarnings
            };
            var evidencePath = Path.ChangeExtension(output, ".build.json");
            File.WriteAllText(evidencePath, JsonUtility.ToJson(record, true));
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0 || !File.Exists(output))
                throw new BuildFailedException("Quest APK build failed. See " + evidencePath);
            // Package settings are insufficient evidence. Check the actual binary manifest,
            // native libraries, ZIP CRC and signature before declaring this output deliverable.
            var buildToolsRoot = Path.Combine(AndroidExternalToolsSettings.sdkRootPath, "build-tools");
            var tools = Directory.GetDirectories(buildToolsRoot)
                .Where(path => Version.TryParse(Path.GetFileName(path), out _))
                .OrderByDescending(path => Version.Parse(Path.GetFileName(path))).FirstOrDefault();
            if (tools == null) throw new BuildFailedException("No Android SDK build-tools available for final APK verification.");
            var python = Environment.GetEnvironmentVariable("LK_PYTHON") ?? "python3";
            var verification = Run(python, Quote(Path.GetFullPath("Tools/verify_quest_apk.py")) + " " + Quote(output) +
                " --build-tools " + Quote(tools) + " --java-home " + Quote(AndroidExternalToolsSettings.jdkRootPath) +
                " --package " + Quote(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)) +
                " --source-record " + Quote(evidencePath));
            UnityEngine.Debug.Log(verification);
            UnityEngine.Debug.Log("QUEST APK PACKAGED AND VERIFIED: " + output + " (headset acceptance still required)");
        }

        static string Quote(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        static string Run(string executable, string arguments)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(executable, arguments)
                {
                    UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true,
                    WorkingDirectory = Directory.GetCurrentDirectory(), CreateNoWindow = true
                };
                process.Start();
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                var output = outputTask.GetAwaiter().GetResult();
                var error = errorTask.GetAwaiter().GetResult();
                if (process.ExitCode != 0) throw new BuildFailedException(executable + " failed: " + error + output);
                return output;
            }
        }
    }
}
