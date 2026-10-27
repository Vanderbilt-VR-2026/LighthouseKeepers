using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace LighthouseKeepers.Editor.Environment
{
    public static class EnvironmentOcclusion
    {
        public static void BakeLab()
        {
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
            // Keep all generated bake products inside the approved Settings/Lighting subtree.
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Settings/Lighting/LK_VisibilityBake.unity");
            StaticOcclusionCulling.smallestOccluder=.5f;StaticOcclusionCulling.smallestHole=.2f;StaticOcclusionCulling.backfaceThreshold=100;
            if(!StaticOcclusionCulling.Compute())throw new Exception("Occlusion bake failed");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),LighthouseBuilder.LabPath);
            File.WriteAllText("artifacts/batch-a/occlusion.txt","Baked combined Lab occlusion data. Per-floor renderer visibility and device cost still require profiler/device inspection; this bake alone is not proof that every other floor is culled.\n");Debug.Log("ENVIRONMENT_OCCLUSION_BAKE_PASS");
        }
    }
}
