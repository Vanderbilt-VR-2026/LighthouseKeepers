using System;
using System.Collections.Generic;
using System.Linq;
using LighthouseKeepers.Simulation.Editor;
using NUnit.Framework;

namespace LighthouseKeepers.Tests.Simulation
{
    public sealed class ShipSceneBuildPolicyTests
    {
        static string[] ShipScenes()
        {
            var paths = new List<string>
            {
                "Assets/Scenes/Lab/LK_Lobby_Environment.unity"
            };
            foreach (string area in new[] { "House", "F1", "F2", "F3", "F4", "Exterior" })
            {
                paths.Add("Assets/Scenes/Shell/LK_Shell_" + area + ".unity");
                paths.Add("Assets/Scenes/Dress/LK_Dress_" + area + ".unity");
            }
            paths.Add("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");
            return paths.ToArray();
        }

        [Test] public void CanonicalShipSetRetainsManifestOrderAndDoesNotAliasTheInput()
        {
            var input = ShipScenes(); var result = ShipSceneBuildPolicy.Validate(input, _ => true);
            Assert.That(result, Is.EqualTo(input)); Assert.That(ReferenceEquals(input, result), Is.False);
        }

        [Test] public void LegacyStormWatchAndLabCannotBeSmuggledIntoShipBuild()
        {
            foreach (string old in new[] { "LK_StormWatch", "LK_Lab_Environment", "LK_DevGym", "LK_Level01_Plumbing" })
            {
                var paths = ShipScenes().Concat(new[] { "Assets/Scenes/" + old + ".unity" }).ToArray();
                Assert.That(() => ShipSceneBuildPolicy.Validate(paths, _ => true),
                    Throws.ArgumentException.With.Message.Contains("Non-ship scene"));
            }
        }

        [Test] public void MissingFloorDressOrExteriorIsRejected()
        {
            foreach (string missing in new[] { "LK_Shell_F4", "LK_Dress_House", "LK_Dress_Exterior" })
            {
                var paths = ShipScenes().Where(p => !p.EndsWith(missing + ".unity", StringComparison.Ordinal)).ToArray();
                Assert.That(() => ShipSceneBuildPolicy.Validate(paths, _ => true),
                    Throws.ArgumentException.With.Message.Contains("Missing required ship scene"));
            }
        }

        [Test] public void MissingManifestFilesDuplicatesAndWrongEntryAreRejected()
        {
            var paths = ShipScenes();
            Assert.That(() => ShipSceneBuildPolicy.Validate(paths, _ => false), Throws.ArgumentException);
            Assert.That(() => ShipSceneBuildPolicy.Validate(paths.Concat(new[] { paths[0] }).ToArray(), _ => true), Throws.ArgumentException);
            string house = paths[1]; paths[1] = paths[paths.Length - 1]; paths[paths.Length - 1] = house;
            Assert.That(() => ShipSceneBuildPolicy.Validate(paths, _ => true), Throws.ArgumentException.With.Message.Contains("Scene 1"));
            Array.Reverse(paths);
            Assert.That(() => ShipSceneBuildPolicy.Validate(paths, _ => true),
                Throws.ArgumentException.With.Message.Contains("Entry scene"));
        }

        [Test] public void ShellAndDressNamesInLegacyFoldersAreRejected()
        {
            var paths = ShipScenes(); paths[1] = "Assets/Scenes/Lab/LK_Shell_House.unity";
            Assert.That(() => ShipSceneBuildPolicy.Validate(paths, _ => true),
                Throws.ArgumentException.With.Message.Contains("lead's Shell folder"));
        }
    }
}
