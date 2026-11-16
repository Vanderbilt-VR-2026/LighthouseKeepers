using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
namespace LighthouseKeepers.Editor
{
    public static class LanternFixtureCleanup
    {
        public static bool Unsupported(string name)=>name.StartsWith("SM_Lamp_F4_")||name.StartsWith("SM_Fixture_F4_")||name.StartsWith("SM_EmergencyLamp_F4_")||name.StartsWith("SM_EmergencyFixture_F4_")||name.StartsWith("SM_Gallery_Lamp")||name.StartsWith("SM_Gallery_EmergencyLamp");
        public static void Apply(GameObject root){foreach(var t in root.GetComponentsInChildren<Transform>(true))if(Unsupported(t.name)||t.name.StartsWith("UCX_")&&Unsupported(t.name.Substring(4)))t.gameObject.SetActive(false);}
        [MenuItem("Lighthouse Keepers/Cleanup/Remove unsupported lantern fixtures")]
        public static void Build(){EditorUtility.audioMasterMute=true;var scene=EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");foreach(var r in scene.GetRootGameObjects())Apply(r);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            const string path="Assets/Prefabs/Environment/PF_Shell_F4.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{Apply(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
            var data=AssetDatabase.LoadAssetAtPath<LighthouseKeepers.WatchDemo.TowerWiringAsset>("Assets/WatchDemo/Resources/TowerWiring/Layout.asset");
            data.runs=data.runs.Where(r=>!Unsupported(r.name)).ToArray();data.endpoints=data.endpoints.Where(e=>!Unsupported(e.name)).ToArray();
            NormalizeLanternRoutes(data);
            EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();TowerWiringAuthoring.Bake();}
        // The glazed lantern has no wall cavity for these exposed runs. Retire the
        // upper meshes rather than span glass/doorways/guard rails; services enter below deck.
        public static void NormalizeLanternRoutes(LighthouseKeepers.WatchDemo.TowerWiringAsset data)
        {
            data.runs=data.runs.Where(r=>!Unsupported(r.name)&&r.floor!=4).ToArray();
            foreach(var r in data.runs)if(r.floor==3&&r.name.Contains("riser"))
                for(int i=0;i<r.points.Length;i++)if(r.points[i].y>9.7f){var p=Quaternion.Euler(0,315,0)*Vector3.forward*2.60f+Quaternion.Euler(0,315,0)*Vector3.right*((r.circuit-2)*.018f);r.points[i]=new Vector3(p.x,Mathf.Min(r.points[i].y,9.88f),p.z);}
        }
    }
}
