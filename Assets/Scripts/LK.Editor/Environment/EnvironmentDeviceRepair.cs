using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment
{
    public static class EnvironmentDeviceRepair
    {
        public static void HideTorso(GameObject rig)
        {
            foreach(var component in rig.GetComponentsInChildren<MonoBehaviour>(true))
                if(component && component.GetType().FullName=="LighthouseKeepers.Player.BodyPresence")
                    foreach(var renderer in component.GetComponentsInChildren<Renderer>(true))
                    {renderer.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);}
        }
        public static void Apply()
        {
            foreach(var guid in AssetDatabase.FindAssets("t:Model",new[]{"Assets/Art/Generated"}))
            {var path=AssetDatabase.GUIDToAssetPath(guid);if(path.Contains("SM_Shell_"))AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);}
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())HideTorso(root);
            foreach(var collider in UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(collider.sharedMesh && AssetDatabase.GetAssetPath(collider.sharedMesh).StartsWith("Assets/Art/Generated/") && !collider.sharedMesh.isReadable)
                    throw new Exception("Device collision requires readable mesh: "+collider.name);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            EnvironmentValidation.RunGeometry();EnvironmentBatchAValidation.ValidateAtlasLayout();
            Debug.Log("DEVICE_COLLISION_IMPORT_PASS");
        }
    }
}
