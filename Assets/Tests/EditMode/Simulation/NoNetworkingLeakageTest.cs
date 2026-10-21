using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LighthouseKeepers.Core.Simulation;
using Mono.Cecil;
using NUnit.Framework;
using UnityEditor.Compilation;

namespace LighthouseKeepers.Tests.Simulation
{
    public sealed class NoNetworkingLeakageTest
    {
        static bool NetworkingNamespace(string value) => new[]
        {
            "Fusion", "Photon", "Unity.Netcode", "Unity.Networking.Transport",
            "LighthouseKeepers.Networking", "LighthouseKeepers.StormWatchNetworking"
        }.Any(prefix => value == prefix || value.StartsWith(prefix + ".", StringComparison.Ordinal));

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

        [Test] public void OnlyLKNetMayReferenceNetworkingNamespaces()
        {
            var violations = new List<string>();
            // Project player assemblies only: vendor packages and Editor/test tooling are
            // dependencies, not game layers. Inspect IL type references, including method bodies.
            foreach (var assembly in CompilationPipeline.GetAssemblies(AssembliesType.Player))
            {
                bool project = assembly.sourceFiles.Any(ProjectSource);
                if (!project || assembly.name == "LK.Net") continue;
                Assert.That(File.Exists(assembly.outputPath), Is.True, "Missing compiled assembly: " + assembly.name);
                using (var metadata = AssemblyDefinition.ReadAssembly(assembly.outputPath))
                    foreach (var reference in metadata.MainModule.GetTypeReferences())
                        if (NetworkingNamespace(reference.Namespace))
                            violations.Add(assembly.name + " -> " + reference.FullName);
            }
            Assert.That(violations, Is.Empty, "Networking belongs in LK.Net. Existing base violations must be migrated by E:\n" +
                string.Join("\n", violations.Distinct()));
        }
    }
}
