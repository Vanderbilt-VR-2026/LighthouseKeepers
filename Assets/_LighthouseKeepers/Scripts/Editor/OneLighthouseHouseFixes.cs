using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LighthouseKeepers.Editor.Environment;
namespace LighthouseKeepers.Editor
{
    public static class OneLighthouseHouseFixes
    {
        public const string Output="artifacts/one/M1.5/house-fixes";
        public static void Survey()
        {
            Directory.CreateDirectory(Output);
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath);
            Physics.SyncTransforms();var log=new StringBuilder();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(r.bounds.center.z> -4 || r.bounds.center.y>3)continue;
                log.AppendLine($"RENDER {r.name} pos={r.transform.position:F3} rot={r.transform.eulerAngles:F1} bounds={r.bounds} materials={string.Join(",",r.sharedMaterials.Select(m=>m?m.name+" texture="+(m.mainTexture?m.mainTexture.name:"none"):"NULL"))}");
            }
            foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(c.bounds.center.z< -4 && c.bounds.center.y<3)log.AppendLine($"COLLIDER {c.name} pos={c.transform.position:F3} bounds={c.bounds}");
            File.WriteAllText(Output+"/house-survey.txt",log.ToString());
        }
    }
}
