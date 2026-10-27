using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment
{
    public static class EnvironmentReport
    {
        [Serializable] sealed class FloorReport {public string scene;public int objects,renderers,colliders,authoredTriangles,materialCount;}
        [Serializable] sealed class Report
        {
            public string status="BUILT-UNVERIFIED";
            public string drawCallEstimate="Not measured: static batching, lightmap atlas and occlusion determine actual device submissions. Renderer/material counts below are not draw calls.";
            public string deviceAcceptance="Pending human house-to-gallery climb and profiler capture.";
            public FloorReport[] floors;public CheckReport[] validators;
        }
        public static void Write()
        {
            var manifest=JsonUtility.FromJson<ShellManifest>(File.ReadAllText("Assets/Art/Generated/ArtManifest.json"));
            var reports=manifest.assets.Where(a=>a.scene!="ScaleReference").Select(a=>
            {
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/Shell/LK_Shell_"+a.scene+".unity",OpenSceneMode.Single);
                var roots=scene.GetRootGameObjects();var renderers=roots.SelectMany(r=>r.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
                return new FloorReport{scene=scene.path,objects=roots.Sum(r=>r.GetComponentsInChildren<Transform>(true).Length),renderers=renderers.Length,colliders=roots.Sum(r=>r.GetComponentsInChildren<Collider>(true).Count(c=>c.enabled)),authoredTriangles=a.triangles,materialCount=renderers.SelectMany(r=>r.sharedMaterials).Distinct().Count()};
            }).ToArray();
            var validators=new[]{"NavigationClearanceValidator","WorkPositionValidator","LuminanceValidator"}.Select(n=>JsonUtility.FromJson<CheckReport>(File.ReadAllText("artifacts/batch-a/"+n+".json"))).ToArray();
            File.WriteAllText("artifacts/build-report.json",JsonUtility.ToJson(new Report{floors=reports,validators=validators},true));
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
        }
    }
}
