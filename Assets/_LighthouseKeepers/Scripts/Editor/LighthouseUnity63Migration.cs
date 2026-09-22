using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LighthouseKeepers.Editor
{
    /// <summary>Explicit migration entry point; never runs automatically when teammates open Unity.</summary>
    public static class LighthouseUnity63Migration
    {
        [MenuItem("Lighthouse Keepers/Compatibility/Validate Unity 6.3 and Rebake")]
        public static void ApplyAndVerify()
        {
            if (Application.unityVersion != "6000.3.23f1")
                throw new InvalidOperationException("Use Unity 6000.3.23f1 for the team compatibility migration.");

            // URP 17.5 serialized a path-tracing resource type absent from URP 17.3.
            // Clear only missing managed references, retaining the asset and its GUID.
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath("Assets/Settings/UniversalRenderPipelineGlobalSettings.asset"))
            {
                if (!asset || !UnityEditor.SerializationUtility.HasManagedReferencesWithMissingTypes(asset)) continue;
                UnityEditor.SerializationUtility.ClearAllManagedReferencesWithMissingTypes(asset);
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
            LighthouseProjectSetup.Configure();
            foreach (var sceneName in new[] { "LK_Bootstrap", "LK_DevGym" })
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(sceneName));
                foreach (var controller in UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None))
                    controller.minMoveDistance = 0; // Preserve low-amplitude stick motion at any frame rate.
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            }
            string playerPath = LighthouseAssets.Root + "/Prefabs/Player/LK_QuestPlayer.prefab";
            var player = PrefabUtility.LoadPrefabContents(playerPath);
            try
            {
                foreach (var controller in player.GetComponentsInChildren<CharacterController>(true)) controller.minMoveDistance = 0;
                PrefabUtility.SaveAsPrefabAsset(player, playerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            var paths = AssetDatabase.GetAllAssetPaths().Where(path =>
                path.StartsWith(LighthouseAssets.Root + "/") &&
                new[] { ".unity", ".prefab", ".mat" }.Contains(Path.GetExtension(path))).ToArray();
            AssetDatabase.ForceReserializeAssets(paths);
            AssetDatabase.SaveAssets();
            LighthouseValidation.Validate();
            // The target editor must produce its own lighting data; a version-file edit is insufficient.
            LighthouseLighting.BakeAndPreview();
        }
    }
}
