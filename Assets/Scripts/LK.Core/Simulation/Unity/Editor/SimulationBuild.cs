using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace LighthouseKeepers.Simulation.Editor
{
    public static class SimulationBuild
    {
        /// <summary>Builds only C's canonical ship manifest. Never reauthors scenes or silently
        /// falls back to the enabled legacy scene list when composition is missing.</summary>
        public static void BuildAndroid()
        {
            const string output = "artifacts/round2/B/LighthouseKeepers-Ship.apk";
            var scenes = ShipSceneBuildPolicy.ReadManifest();
            ShipSceneBuildPolicy.ValidateBuildSettings(scenes);
            ShipSceneBuildPolicy.ValidateReachability(scenes);
            Directory.CreateDirectory("artifacts/round2/B");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes, locationPathName = output, target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4
            });
            File.WriteAllText("artifacts/round2/B/simulation-build.txt",
                $"Result={report.summary.result}\nErrors={report.summary.totalErrors}\nAPK={output}\n" +
                "Manifest inclusion does not establish Lobby -> Play Solo reachability or headset acceptance.\n");
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Simulation Android build failed; see artifacts/build.log.");
        }
    }
}
