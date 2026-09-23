using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LighthouseKeepers.Editor
{
    public static class LighthouseIntegrationAudit
    {
        public static void Inspect()
        {
            var text = new StringBuilder("Unity " + Application.unityVersion + "\n");
            foreach (var name in new[] { "LK_Core", "LK_Exterior", "LK_Level03_Communications", "LK_Level04_Lantern" })
            {
                EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
                Physics.SyncTransforms();
                text.AppendLine("SCENE " + name);
                foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                {
                    if (!(t.name.Contains("entry") || t.name.Contains("Entrance") || t.name.Contains("tower wall") || t.name.Contains("worktop") || t.name.Contains("leg") || t.name.Contains("logbook") || t.name.Contains("rain") || t.name.Contains("balcony") || t.name.Contains("Ocean"))) continue;
                    var c = t.GetComponent<Collider>();
                    text.AppendLine($"{t.name}: position={t.position:F3}; scale={t.localScale:F3}; rotation={t.eulerAngles:F1}; collider={(c ? c.bounds.ToString("F3") : "none")}");
                }
            }
            Directory.CreateDirectory("Docs/Verification");
            File.WriteAllText("Docs/Verification/IntegrationBefore.txt", text.ToString());
            EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));
        }
    }
}
