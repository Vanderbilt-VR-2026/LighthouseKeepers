using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LighthouseKeepers.Core.Simulation;
using Mono.Cecil;
using NUnit.Framework;
using UnityEditor.Compilation;
using UnityEngine;

namespace LighthouseKeepers.Tests.Simulation
{
    public sealed class NoNetworkingLeakageTest
    {
        static bool NetworkingNamespace(string value) => new[]
        {
            "Fusion", "Photon", "Unity.Netcode", "Unity.Networking.Transport",
            "LighthouseKeepers.Networking", "LighthouseKeepers.StormWatchNetworking"
        }.Any(prefix => value == prefix || value.StartsWith(prefix + ".", StringComparison.Ordinal));

        // LK.Net intentionally wraps the retained Fusion lobby and historical Storm Watch
        // adapters. This explicit boundary matches the integrated runtime architecture;
        // new simulation/gameplay assemblies must still stay independent of networking.
        static readonly HashSet<string> NetworkAdapters = new HashSet<string>(StringComparer.Ordinal)
        { "LK.Net", "LK.Networking", "LK.StormWatch.Networking" };

        [Serializable] sealed class Definition
        {
            public string[] optionalUnityReferences;
        }
        static bool DeclaredTestAssembly(string name)
        {
            string path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(name);
            if (string.IsNullOrEmpty(path)) return false;
            var definition = JsonUtility.FromJson<Definition>(File.ReadAllText(path));
            return definition.optionalUnityReferences != null &&
                definition.optionalUnityReferences.Contains("TestAssemblies", StringComparer.Ordinal);
        }

        [TestCase("LK.Net", true)]
        [TestCase("LK.Networking", true)]
        [TestCase("LK.StormWatch.Networking", true)]
        [TestCase("LK.Net.Gameplay", false)]
        [TestCase("LK.Core.Simulation", false)]
        [TestCase("LighthouseKeepers.Runtime", false)]
        [TestCase("Assembly-CSharp", false)]
        public void OnlyExplicitTransportAdaptersAreApproved(string name, bool approved)
            => Assert.That(NetworkAdapters.Contains(name), Is.EqualTo(approved));

        static bool ProjectSource(string path)
        {
            string assets = Path.GetFullPath("Assets").Replace('\\', '/').TrimEnd('/') + "/";
            string full = Path.GetFullPath(path).Replace('\\', '/');
            if (!full.StartsWith(assets, StringComparison.Ordinal)) return false;
            string relative = full.Substring(assets.Length);
            return !relative.StartsWith("Photon/", StringComparison.Ordinal) &&
                !relative.StartsWith("Samples/", StringComparison.Ordinal);
        }

        [Test] public void SimulationCoreHasNoUnityOrNetworkingAssemblyReferences()
        {
            var forbidden = typeof(SimulationHost).Assembly.GetReferencedAssemblies()
                .Where(a => a.Name.StartsWith("Unity", StringComparison.Ordinal) || NetworkingNamespace(a.Name));
            Assert.That(forbidden, Is.Empty);
        }

        [Test] public void GameplayLayersKeepNetworkingInsideApprovedAdapters()
        {
            var violations = new List<string>();
            // Project player assemblies only: vendor packages and Editor/test tooling are
            // dependencies, not game layers. Inspect IL type references, including method bodies.
            foreach (var assembly in CompilationPipeline.GetAssemblies(AssembliesType.Player))
            {
                bool project = assembly.sourceFiles.Any(ProjectSource);
                if (!project || NetworkAdapters.Contains(assembly.name) || DeclaredTestAssembly(assembly.name)) continue;
                Assert.That(File.Exists(assembly.outputPath), Is.True, "Missing compiled assembly: " + assembly.name);
                using (var metadata = AssemblyDefinition.ReadAssembly(assembly.outputPath))
                    foreach (var reference in metadata.MainModule.GetTypeReferences())
                        if (NetworkingNamespace(reference.Namespace))
                            violations.Add(assembly.name + " -> " + reference.FullName);
            }
            Assert.That(violations, Is.Empty, "Networking must stay in LK.Net and its explicitly retained transport adapters:\n" +
                string.Join("\n", violations.Distinct()));
        }
    }
}
