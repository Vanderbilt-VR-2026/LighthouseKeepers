using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo.Editor
{
    // Rebuildable circular subtraction; preserves source triangle attributes/submeshes.
    public static class PoleShaftAuthoring
    {
        const string Folder="Assets/WatchDemo/PoleShaftMeshes";
        static readonly Dictionary<string,string> Sources=new Dictionary<string,string>{
            {"SM_Floor_F2","37efadb7fc0a0f7d89a2666a2fe10011"},{"SM_Floor_F3","d3999fd2514909d141a95d98e632239c"},
            {"SM_Floor_F4","f3ec45edcd5ca3686f40035351d3ba40"},{"SM_Floor_Lantern_Threshold","8c0f2dcac7c24842513227c2a532ba16"}};
        static bool Floor(string name)=>name=="SM_Floor_F2"||name=="SM_Floor_F3"||name=="SM_Floor_F4"||name=="SM_Floor_Lantern_Threshold";
        public static void Apply(GameObject root)
        {
            foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))if(Floor(f.name))
            {var m=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/"+f.name+".asset");if(m)f.sharedMesh=m;}
            foreach(var c in root.GetComponentsInChildren<MeshCollider>(true))
            {var key=c.name.StartsWith("UCX_")?c.name.Substring(4):c.name; if(key.EndsWith("_01"))key=key.Substring(0,key.Length-3);if(!Floor(key))continue;
             var m=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/"+key+".asset");if(m){c.convex=false;c.sharedMesh=m;}}
        }
        [MenuItem("Lighthouse Keepers/Rebuild continuous pole openings")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            foreach(var root in scene.GetRootGameObjects())foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))if(Floor(f.name))
            {
                var path=Folder+"/"+f.name+".asset";
                // Always cut the original UV-corrected source, never a previous aperture.
                var source=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/VisualPass/UV/"+Sources[f.name]+".asset");
                var mesh=Cut(source,f.transform);mesh.name=f.name+" circular pole opening";
                var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);}else AssetDatabase.CreateAsset(mesh,path);
            }
            foreach(var root in scene.GetRootGameObjects())Apply(root);
            var bunk=GameObject.Find("Third-floor bed frame");if(bunk)BunkPlacement.Apply(bunk.transform);
            var beacon=GameObject.Find("STN_BEACON_CONTROL");if(beacon){beacon.transform.position=new Vector3(1f,9.9f,2.4f);beacon.transform.rotation=Quaternion.Euler(0,202.62f,0);}
            foreach(string part in new[]{"F2","F3","F4"}){
                string path="Assets/Prefabs/Environment/PF_Shell_"+part+".prefab";var prefab=PrefabUtility.LoadPrefabContents(path);
                try{Apply(prefab);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}}

            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        struct V
        {public Vector3 p,n;public Vector4 t;public Vector2 uv,uv2;public Color c;
         public static V Lerp(V a,V b,float f)=>new V{p=Vector3.Lerp(a.p,b.p,f),n=Vector3.Lerp(a.n,b.n,f),t=Vector4.Lerp(a.t,b.t,f),uv=Vector2.Lerp(a.uv,b.uv,f),uv2=Vector2.Lerp(a.uv2,b.uv2,f),c=Color.Lerp(a.c,b.c,f)};}
        static List<V> Clip(List<V> polygon,Vector3 normal,float distance,bool inside)
        {
            var output=new List<V>();if(polygon.Count==0)return output;
            V prev=polygon[polygon.Count-1];float dp=Vector3.Dot(prev.p,normal)-distance;bool ip=inside?dp<=0:dp>=0;
            foreach(var next in polygon){float dn=Vector3.Dot(next.p,normal)-distance;bool inn=inside?dn<=0:dn>=0;
             if(ip!=inn)output.Add(V.Lerp(prev,next,dp/(dp-dn)));if(inn)output.Add(next);prev=next;dp=dn;ip=inn;}return output;
        }
        public static Mesh Cut(Mesh source,Transform transform)
        {
            var p=source.vertices;var n=source.normals;var t=source.tangents;var u=source.uv;var u2=source.uv2;var c=source.colors;
            var all=new List<V>();var subs=new List<int[]>();
            V Read(int i)=>new V{p=transform.TransformPoint(p[i]),n=n.Length==p.Length?n[i]:Vector3.up,t=t.Length==p.Length?t[i]:new Vector4(1,0,0,1),uv=u.Length==p.Length?u[i]:Vector2.zero,uv2=u2.Length==p.Length?u2[i]:Vector2.zero,c=c.Length==p.Length?c[i]:Color.white};
            for(int sub=0;sub<source.subMeshCount;sub++){
                var indices=new List<int>();var tri=source.GetTriangles(sub);
                void Emit(List<V> polygon){for(int i=1;i+1<polygon.Count;i++){indices.Add(all.Count);all.Add(polygon[0]);indices.Add(all.Count);all.Add(polygon[i]);indices.Add(all.Count);all.Add(polygon[i+1]);}}
                for(int i=0;i<tri.Length;i+=3){var remainder=new List<V>{Read(tri[i]),Read(tri[i+1]),Read(tri[i+2])};
                 for(int side=0;side<32&&remainder.Count>0;side++){float angle=side*Mathf.PI*2/32;var normal=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));float distance=Vector3.Dot(PoleSlide.ShaftAxis,normal)+PoleSlide.OpeningRadius;
                  Emit(Clip(remainder,normal,distance,false));remainder=Clip(remainder,normal,distance,true);}}
                subs.Add(indices.ToArray());
            }
            var vertices=new Vector3[all.Count];var normals=new Vector3[all.Count];var tangents=new Vector4[all.Count];var uv=new Vector2[all.Count];var uv2=new Vector2[all.Count];var colors=new Color[all.Count];
            for(int i=0;i<all.Count;i++){vertices[i]=transform.InverseTransformPoint(all[i].p);normals[i]=all[i].n.normalized;tangents[i]=all[i].t;uv[i]=all[i].uv;uv2[i]=all[i].uv2;colors[i]=all[i].c;}
            var result=new Mesh{indexFormat=all.Count>65535?UnityEngine.Rendering.IndexFormat.UInt32:UnityEngine.Rendering.IndexFormat.UInt16};result.vertices=vertices;result.normals=normals;result.tangents=tangents;result.uv=uv;result.uv2=uv2;result.colors=colors;result.subMeshCount=subs.Count;for(int i=0;i<subs.Count;i++)result.SetTriangles(subs[i],i);result.RecalculateBounds();return result;
        }
    }
}
