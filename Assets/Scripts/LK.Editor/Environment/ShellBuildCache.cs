using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment
{
    // Committed shells retain Unity's serialized IDs. A no-op regeneration never churns them.
    public static class ShellBuildCache
    {
        const string PathName="Assets/Data/Layout/shell-build-state.json";
        [Serializable] sealed class State {public string inputs;public Entry[] outputs;}
        [Serializable] sealed class Entry {public string path,hash;}
        static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","");}
        static string Inputs()
        {
            var files=new[]{LighthouseBuilder.LayoutPath,"Assets/Art/Generated/ArtManifest.json"}.Concat(Directory.GetFiles("Assets/Scripts/LK.Editor/Environment","*.cs",SearchOption.AllDirectories)).Concat(Directory.GetFiles("Assets/Scripts/LK.Presentation/Environment","*.cs",SearchOption.AllDirectories)).OrderBy(x=>x,StringComparer.Ordinal);
            return string.Join("\n",files.Select(p=>p+":"+Hash(p)));
        }
        public static bool IsCurrent()
        {
            if(!File.Exists(PathName))return false;var s=JsonUtility.FromJson<State>(File.ReadAllText(PathName));
            if(s.inputs!=Inputs())return false;
            foreach(var e in s.outputs)if(!File.Exists(e.path)||Hash(e.path)!=e.hash)throw new Exception("Generated asset differs from its saved build stamp: "+e.path+". Preserve/review the edit before regeneration.");
            return File.Exists(LighthouseBuilder.LabPath);
        }
        public static void Record()
        {
            var paths=Directory.GetFiles("Assets/Scenes/Shell","*.unity").Concat(Directory.GetFiles("Assets/Prefabs/Environment","PF_Shell_*.prefab")).OrderBy(p=>p,StringComparer.Ordinal);
            File.WriteAllText(PathName,JsonUtility.ToJson(new State{inputs=Inputs(),outputs=paths.Select(p=>new Entry{path=p,hash=Hash(p)}).ToArray()},true));
        }
        public static void VerifyTwice()
        {
            LighthouseBuilder.BuildAll();var paths=Directory.GetFiles("Assets/Scenes/Shell","*.unity").Append(LighthouseBuilder.LabPath).ToArray();var before=paths.Select(Hash).ToArray();
            LighthouseBuilder.BuildAll();for(int i=0;i<paths.Length;i++)if(before[i]!=Hash(paths[i]))throw new Exception("Scene churn: "+paths[i]);
            File.WriteAllText("artifacts/batch-a/determinism.txt","PASS: identical inputs rerun against committed/generated assets retain byte-identical shell and Lab scenes. Clean-room independent Unity file-ID regeneration is not asserted.\n");Debug.Log("SHELL_REBUILD_BYTE_IDENTITY_PASS");
        }
    }
}
