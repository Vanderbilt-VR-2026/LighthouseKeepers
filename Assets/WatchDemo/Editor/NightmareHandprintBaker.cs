using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LighthouseKeepers.WatchDemo;

namespace LighthouseKeepers.Editor
{
    public static class NightmareHandprintBaker
    {
        const string Folder="Assets/WatchDemo/Resources/NightmareHandprints";
        [MenuItem("Lighthouse Keepers/Scares/Rebuild nightmare rail handprints")]
        public static void Bake()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter the Watch scene in Play Mode before rebuilding the rail handprints.");
            var meshes=new Dictionary<int,Mesh>();
            try
            {
                // Validate all six before modifying any saved asset.
                foreach(int segment in ScareHandprints.Segments)
                {
                    var rail=GameObject.Find("SM_Rail_F2_F3_Inner_Top_"+segment);
                    var mesh=ScareHandprints.ProjectRail(rail);
                    if(!mesh)throw new InvalidOperationException("Could not project a safe handprint onto rail "+segment+".");
                    mesh.name="Wet rail mark "+segment;meshes.Add(segment,mesh);
                }
                if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/WatchDemo/Resources","NightmareHandprints");
                foreach(var entry in meshes)
                {
                    string path=Folder+"/Segment_"+entry.Key+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if(existing){EditorUtility.CopySerialized(entry.Value,existing);EditorUtility.SetDirty(existing);}
                    else AssetDatabase.CreateAsset(entry.Value,path);
                }
                AssetDatabase.SaveAssets();Debug.Log("Rebuilt six nightmare handprints from the loaded tower rails.");
            }
            finally
            {
                foreach(var mesh in meshes.Values)if(mesh&&!AssetDatabase.Contains(mesh))UnityEngine.Object.DestroyImmediate(mesh);
            }
        }
    }
}
