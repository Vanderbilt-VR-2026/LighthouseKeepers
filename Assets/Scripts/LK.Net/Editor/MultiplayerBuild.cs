using System;
using System.IO;
using System.Linq;
using Fusion;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace LighthouseKeepers.Net.Editor
{
    public static class MultiplayerBuild
    {
        public const string Apk="Builds/LighthouseKeepers-Sprint3-Networking.apk";
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Validate network prefabs")]
        public static void ValidatePrefabs()
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
                    new NetworkObjectBaker().Bake(root);
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Build Quest APK")]
        public static void BuildQuest() => Build(BuildTarget.Android,Apk,BuildOptions.Development|BuildOptions.CompressWithLz4);
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Build release Quest APK")]
        public static void BuildReleaseQuest() => Build(BuildTarget.Android,"Builds/LighthouseKeepers-Sprint3-Networking-Release.apk",BuildOptions.CompressWithLz4);
        static void Build(BuildTarget target,string output,BuildOptions options)
        {
            ValidatePrefabs();Directory.CreateDirectory("Builds");
            ShipRouteAuthoring.Validate();
            var scenes=ShipRouteSettings.Load().Plan.scenes;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,target=target,locationPathName=output,options=options});
            File.WriteAllText(output+".txt",$"result={report.summary.result}\nerrors={report.summary.totalErrors}\nsize={report.summary.totalSize}\n");
            if(report.summary.result!=BuildResult.Succeeded || report.summary.totalErrors!=0)throw new BuildFailedException("Networking APK build failed; see the build log.");
            Debug.Log("[BUILT-UNTESTED] "+output+"\nINSTALL: adb install -r "+output);
        }
    }
}
