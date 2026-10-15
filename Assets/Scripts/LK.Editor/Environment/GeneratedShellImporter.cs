using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment
{
    // Only our generated models are affected. Vendor importers and production assets are untouched.
    public sealed class GeneratedShellImporter : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/Art/Generated/SM_Shell_", StringComparison.Ordinal)) return;
            string manifestPath = "Assets/Art/Generated/ArtManifest.json";
            if (!File.Exists(manifestPath)) throw new InvalidOperationException("Generated shell lacks its tracked manifest. Run Blender build_all.py first.");
            var manifest = JsonUtility.FromJson<ShellManifest>(File.ReadAllText(manifestPath));
            if (!manifest.assets.Any(a => a.path == assetPath)) throw new InvalidOperationException("Unlisted generated shell: " + assetPath);
            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1; importer.useFileScale = true;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.isReadable = false; importer.optimizeMeshPolygons = true; importer.optimizeMeshVertices = true;
            importer.addCollider = false; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
            importer.generateSecondaryUV = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
        }
    }
    [Serializable] public sealed class ShellManifest { public string sourceHash; public ShellAsset[] assets; }
    [Serializable] public sealed class ShellAsset { public string path, scene, sha256; public int triangles; public ShellObject[] objects; }
    [Serializable] public sealed class ShellObject { public string name; public string[] materials; public bool collisionOnly, collisionEnabled, staticCollision; }
    [Serializable] public sealed class ShellLayout
    {
        public string version; public StationBoxExporter.Floor[] floors;
        public StationBoxExporter.Station[] stations; public StationBoxExporter.Work[] operatorPositions;
        public LightingRules lightingValidation;
        public ShellRoute[] routes; public ShellStair[] stairs; public ShellLanding[] landings;
    }
    [Serializable] public sealed class LightingRules {public string calibrationAsset;public float poweredFloorRatio, emergencyFloorRatio, poweredReadoutRatio, textContrast, exposure;}
    [Serializable] public sealed class ShellRoute { public string id, floor, kind; public float widthM; public Vector3[] points; }
    [Serializable] public sealed class ShellStair { public string id, from, to; public float floorY, rise, departureAzimuth, arrivalAzimuth, arcDegrees, walkLineRadius, clearWidth; public int risers, goings; }
    [Serializable] public sealed class ShellLanding { public string id, floor; public float floorY, innerRadius, outerRadius, arcStartDeg, arcEndDeg, clearRadialWidth, minimumInnerArcLength, wallReveal; }
}
