using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Environment.Sprint3.Editor
{
    public static class Round2Inventory
    {
        public static void Run()
        {
            Directory.CreateDirectory("artifacts/round2/C");
            using var writer = new StreamWriter("artifacts/round2/C/inventory.txt");
            foreach (var path in new[] { "Assets/Scenes/Lab/LK_Lab_Environment.unity", "Assets/Scenes/Shell/LK_Shell_House.unity", "Assets/Scenes/Shell/LK_Shell_F1.unity", "Assets/Scenes/Shell/LK_Shell_F4.unity", "Assets/_LighthouseKeepers/Scenes/Development/LK_StormWatch.unity" })
            {
                var scene = EditorSceneManager.OpenScene(path);
                writer.WriteLine(path);
                foreach (var t in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)))
                {
                    if (t.name.StartsWith("UCX")) continue;
                    writer.WriteLine(ShipSceneCatalog.PathOf(t) + " @ " + t.position.ToString("F2") + " | " + string.Join(",", t.GetComponents<Component>().Where(c => c).Select(c => c.GetType().Name)));
                }
            }
        }
    }
}
