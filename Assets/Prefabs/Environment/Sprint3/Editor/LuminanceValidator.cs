using System;
using System.IO;
using LighthouseKeepers.Editor.Environment;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LighthouseKeepers.Environment.Sprint3.Editor
{
    /// <summary>Reuse the existing linear floating-point camera sampler and 0.5m route grid.
    /// The original powered reference is immutable; never calibrate a fault state.</summary>
    public static class LuminanceValidator
    {
        const string CalibrationPath = "Assets/Settings/Lighting/LuminanceCalibration.json";
        const string MeasurementStamp = "Assets/Settings/Lighting/Sprint3/LuminanceFingerprint.txt";
        [Serializable] sealed class Calibration { public float referenceLinearLuminance; public float exposure; }
        static void RequireReference()
        {
            if (!File.Exists(CalibrationPath)) throw new BuildFailedException("Missing original powered reference; no automatic recalibration.");
            var calibration = JsonUtility.FromJson<Calibration>(File.ReadAllText(CalibrationPath));
            if (calibration == null || float.IsNaN(calibration.referenceLinearLuminance) || float.IsInfinity(calibration.referenceLinearLuminance)
                || calibration.referenceLinearLuminance < .005f || calibration.exposure != 1)
                throw new BuildFailedException("Invalid/near-black powered reference or changed exposure.");
        }
        [MenuItem("Lighthouse/Sprint 3/Measure both lighting states")]
        public static void Run()
        {
            RequireReference(); Sprint3LightingBake.RequireCurrentBake();
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                throw new BuildFailedException("Luminance measurement requires a graphics-enabled Editor; omit -nographics.");
            Directory.CreateDirectory("artifacts/batch-a");
            // Existing sampler disables post processing and occlusion, uses linear ARGBFloat,
            // Rec.709 coefficients, eye=1.6m and one frozen reference for BOTH states.
            // It samples rooms at 0.5m, every route at <=0.5m, passage and gallery as well.
            // Sprint3 presets deliberately cannot take the old Half-preview threshold branch.
            var setup = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
            try { LighthouseKeepers.Editor.Environment.LuminanceValidator.Run(); }
            finally { UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(setup); }
            File.WriteAllText(MeasurementStamp, Sprint3LightingBake.Fingerprint() + "\n"); AssetDatabase.ImportAsset(MeasurementStamp);
        }
        public static void RequireCurrentMeasurements()
        {
            RequireReference();
            if (!File.Exists(MeasurementStamp) || File.ReadAllText(MeasurementStamp).Trim() != Sprint3LightingBake.Fingerprint())
                throw new BuildFailedException("Both lighting states require current fixed-reference luminance measurements before shipping.");
        }
    }
}
