using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace LighthouseKeepers.Simulation.Editor
{
    public static class SimulationBuild
    {
        /// <summary>Builds the enabled crew scenes without reauthoring or saving them.
        /// Session C/D/E must integrate the simulation host before device acceptance.</summary>
        public static void BuildAndroid()
        {
            const string output = "artifacts/builds/LighthouseKeepers-Simulation.apk";
            Directory.CreateDirectory("artifacts/builds");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes, locationPathName = output, target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4
            });
            File.WriteAllText("artifacts/builds/simulation-build.txt",
                $"Result={report.summary.result}\nErrors={report.summary.totalErrors}\nAPK={output}\n" +
                "Source inclusion does not establish scene integration or headset acceptance.\n");
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Simulation Android build failed; see artifacts/build.log.");
        }
    }
}
