using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment
{
    public static class EnvironmentBuild
    {
        internal static bool IsEnvironmentLabBuild;
        [MenuItem("Lighthouse/Build isolated Environment Lab APK")]
        public static void BuildAndroid()
        {
            Directory.CreateDirectory("artifacts/builds");IsEnvironmentLabBuild=true;
            try
            {
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{LighthouseBuilder.LabPath},locationPathName="artifacts/builds/LighthouseKeepers-Environment-BatchA.apk",target=BuildTarget.Android,options=BuildOptions.Development|BuildOptions.AllowDebugging});
                File.WriteAllText("artifacts/batch-a/android-build.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nBytes: "+report.summary.totalSize+"\n");
                if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new Exception("Environment Lab Android build failed");Debug.Log("ENVIRONMENT_APK_BUILD_PASS");
            }finally{IsEnvironmentLabBuild=false;}
        }
    }
    public sealed class EnvironmentPrebuild : IPreprocessBuildWithReport
    {
        public int callbackOrder=>-100;
        public void OnPreprocessBuild(BuildReport report)
        {
            // Environment tooling never changes or gates the team's legacy production build.
            if(!EnvironmentBuild.IsEnvironmentLabBuild)return;
            try { EnvironmentBatchAValidation.ValidateAtlasLayout();EnvironmentValidation.RunGeometry();LuminanceValidator.Run(); }
            catch(Exception error) { throw new BuildFailedException(error); }
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
            foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None))
                foreach(var m in r.sharedMaterials)if(!m||!m.shader||m.shader.name=="Hidden/InternalErrorShader")throw new BuildFailedException("Missing/invalid material "+r.name);
        }
    }
}
