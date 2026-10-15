using System;
using System.Linq;
using UnityEditor;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.Editor.Environment
{
    public static class EnvironmentBatchAValidation
    {
        public static void ValidateAtlasLayout()
        {
            var p=AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>("Assets/Settings/Lighting/LightingPreset_Powered.asset");
            var e=AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>("Assets/Settings/Lighting/LightingPreset_Emergency.asset");
            if(!p||!e||!p.rendererIndices.SequenceEqual(e.rendererIndices)||!p.rendererScaleOffsets.SequenceEqual(e.rendererScaleOffsets))
                throw new Exception("Lighting presets must share atlas layout for static-batched renderers.");
        }
        public static void Prepare()
        {
            ValidateAtlasLayout();StationBoxExporter.Build();LuminanceValidator.FinalizeMeasurements();EnvironmentReport.Write();
        }
    }
}
