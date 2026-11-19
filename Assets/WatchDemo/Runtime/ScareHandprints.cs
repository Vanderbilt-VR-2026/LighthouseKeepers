using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Core.Progression;

namespace LighthouseKeepers.WatchDemo
{
    // The meshes are projected in the Editor; playback never cooks tower-sized colliders.
    public sealed class ScareHandprints : System.IDisposable
    {
        public static readonly int[] Segments={3,9,15,21,27,33};
        public const string ResourceFolder="NightmareHandprints/Segment_";
        readonly List<(GameObject root,float floor)> prints = new();
        readonly Material material;
        public ScareHandprints(Transform parent)
        {
            material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor",new Color(.022f,.032f,.026f));
            material.SetFloat("_Smoothness",.85f);
            foreach(int segment in Segments)
            {
                var rail=GameObject.Find("SM_Rail_F2_F3_Inner_Top_"+segment);
                var mesh=Resources.Load<Mesh>(ResourceFolder+segment);
                if(!rail||!mesh||!rail.TryGetComponent<Renderer>(out var railRenderer))continue;
                var root=new GameObject("Wet child handprint "+segment);root.transform.SetParent(parent,true);
                root.AddComponent<MeshFilter>().sharedMesh=mesh;var r=root.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;
                root.SetActive(false);prints.Add((root,railRenderer.bounds.center.y-1.1f));
            }
        }
        public void Present(ScareSequenceState scene)
        {
            foreach(var print in prints)
                print.root.SetActive(scene.Dark&&scene.Beat>=ScareBeat.Follow&&scene.Body.y<print.floor+.25f);
        }
        public void Dispose(){foreach(var print in prints)Object.Destroy(print.root);Object.Destroy(material);}

#if UNITY_EDITOR
        // Editor-only projection used to rebuild the six assets after shell changes.
        // Several rail renderers share the whole tower mesh: constrain every ray
        // to this segment, then reject discontinuities from the local surface.
        public static Mesh ProjectRail(GameObject rail)
        {
            if(!rail||!rail.TryGetComponent<MeshFilter>(out var filter)||!filter.sharedMesh||!rail.TryGetComponent<Renderer>(out var renderer))return null;
            var surface=rail.AddComponent<MeshCollider>();surface.sharedMesh=filter.sharedMesh;
            try
            {
                var bounds=renderer.bounds;float top=bounds.max.y+.01f,depth=bounds.size.y+.02f;
                var allowed=bounds;allowed.Expand(.004f);
                bool OnSegment(Vector3 p,out RaycastHit hit)
                {return surface.Raycast(new Ray(new Vector3(p.x,top,p.z),Vector3.down),out hit,depth)&&allowed.Contains(hit.point);}
                if(!OnSegment(bounds.center,out var anchor)||anchor.normal.y<.25f)return null;
                var vertices=new List<Vector3>();var triangles=new List<int>();
                var along=Vector3.Cross(Vector3.up,new Vector3(bounds.center.x,0,bounds.center.z)).normalized;
                var across=Vector3.Cross(along,Vector3.up);
                void Oval(float x,float y,float rx,float ry)
                {
                    int start=vertices.Count;
                    for(int i=0;i<=10;i++)
                    {
                        float a=(i-1)*Mathf.PI*2/10;
                        var p=bounds.center+across*(x+(i==0?0:Mathf.Cos(a)*rx))+along*(y+(i==0?0:Mathf.Sin(a)*ry));
                        if(!OnSegment(p,out var hit)){vertices.RemoveRange(start,vertices.Count-start);return;}
                        var offset=hit.point-anchor.point;
                        float expected=anchor.point.y-(anchor.normal.x*offset.x+anchor.normal.z*offset.z)/anchor.normal.y;
                        if(Mathf.Abs(hit.point.y-expected)>.025f){vertices.RemoveRange(start,vertices.Count-start);return;}
                        vertices.Add(hit.point+hit.normal*.0015f);
                    }
                    for(int i=1;i<=10;i++)triangles.AddRange(new[]{start,start+i,start+(i==10?1:i+1)});
                }
                Oval(0,-.009f,.016f,.022f);
                for(int finger=0;finger<4;finger++)Oval((finger-1.5f)*.008f,.021f,.0033f,.018f);
                Oval(-.020f,-.007f,.005f,.012f);
                if(triangles.Count==0)return null;
                var mesh=new Mesh{name="Small wet rail handprint"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            }
            finally{Object.DestroyImmediate(surface);}
        }
#endif
    }
}
