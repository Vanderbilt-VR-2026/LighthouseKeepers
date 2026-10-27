using Fusion;
using LighthouseKeepers.Editor.QuestBuild;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace LighthouseKeepers.Net.Editor
{
    public static class MultiplayerBuild
    {
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Validate network prefabs")]
        public static void ValidatePrefabs() => ValidatePrefabs(false);
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Bake network prefabs")]
        public static void BakePrefabs() => ValidatePrefabs(true);
        static void ValidatePrefabs(bool bake)
        {
            foreach(var name in new[]{"PF_PlayerRig","PF_GrabAuthority"})
            {
                string path="Assets/Prefabs/Network/Resources/"+name+".prefab";
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if(!root.GetComponent<NetworkObject>())throw new BuildFailedException(name+" needs a NetworkObject at the root.");
                    if(root.GetComponentsInChildren<Camera>(true).Length>0 || root.GetComponentsInChildren<AudioListener>(true).Length>0)
                        throw new BuildFailedException(name+" must not contain another tracked camera or audio listener.");
                    if (bake)
                    {
                        new NetworkObjectBaker().Bake(root);
                        PrefabUtility.SaveAsPrefabAsset(root,path);
                    }
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            if (bake) AssetDatabase.SaveAssets();
        }
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Build Quest APK")]
        public static void BuildQuest() => Build(BuildOptions.Development | BuildOptions.CompressWithLz4);
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Build release Quest APK")]
        public static void BuildReleaseQuest() => Build(BuildOptions.CompressWithLz4);
        static void Build(BuildOptions options)
        {
            ValidatePrefabs();
            QuestApkPipeline.Build(ShipSceneManifest.Scenes, options);
        }
    }
}
