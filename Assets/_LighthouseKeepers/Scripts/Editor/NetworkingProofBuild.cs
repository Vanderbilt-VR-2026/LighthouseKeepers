using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Editor
{
    public static class NetworkingProofBuild
    {
        public const string Scene = "Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkingProof.unity";
        [MenuItem("Lighthouse Keepers/Development/Build two-player networking proof")]
        public static void Build()
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            foreach (var loaded in setup)
                if (SceneManager.GetSceneByPath(loaded.path).isDirty) throw new InvalidOperationException("Save open scenes before building the proof.");
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var proof = new GameObject("Two-process networking proof").AddComponent<LighthouseKeepers.NetworkingProof.NetworkingProof>();
                var serialized = new SerializedObject(proof);
                serialized.FindProperty("proofShader").objectReferenceValue = Shader.Find("Universal Render Pipeline/Lit");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.SaveScene(scene, Scene);
                AssetDatabase.SaveAssets();
                Directory.CreateDirectory("Builds/NetworkingProof");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { Scene }, locationPathName = "Builds/NetworkingProof/LighthouseNetworkProof.app",
                    target = BuildTarget.StandaloneOSX, options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded) throw new Exception("Proof build failed: " + report.summary.result);
                Debug.Log("PROOF_BUILD_SUCCEEDED " + report.summary.totalSize + " bytes");
            }
            finally
            {
                if (Array.Exists(setup, loaded => loaded.isLoaded && loaded.isActive))
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
    }
}
