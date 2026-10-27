using System;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo.Editor
{
    /// <summary>Editor evidence: exact triangle/box SAT against the handle throughout its tightening roll.</summary>
    public static class WatchGripGeometry
    {
        public static void Verify(SkinnedMeshRenderer skin,Transform tool)
        {
            var mesh=new Mesh();skin.BakeMesh(mesh);var vertices=mesh.vertices;var triangles=mesh.triangles;
            for(int i=0;i<vertices.Length;i++)vertices[i]=tool.InverseTransformPoint(skin.transform.TransformPoint(vertices[i]));
            UnityEngine.Object.DestroyImmediate(mesh);
            var half=new Vector3(.011f,.005f,.094f)-Vector3.one*.00001f;
            var centre=new Vector3(0,0,-.026f);
            for(int degrees=-55;degrees<=55;degrees+=5)
            {
                var rotation=Quaternion.AngleAxis(-degrees,Vector3.forward);
                for(int i=0;i<triangles.Length;i+=3)
                {
                    var a=rotation*vertices[triangles[i]]-centre;var b=rotation*vertices[triangles[i+1]]-centre;var c=rotation*vertices[triangles[i+2]]-centre;
                    if(Overlap(a,b,c,half))throw new InvalidOperationException($"Hand triangle {i/3} penetrates handle at {degrees} degree tightening roll.");
                }
            }
        }
        static bool Overlap(Vector3 a,Vector3 b,Vector3 c,Vector3 h)
        {
            if(Separate(Vector3.right,a,b,c,h)||Separate(Vector3.up,a,b,c,h)||Separate(Vector3.forward,a,b,c,h))return false;
            var ab=b-a;var bc=c-b;var ca=a-c;
            if(Separate(Vector3.Cross(ab,bc),a,b,c,h))return false;
            return !EdgeSeparates(ab,a,b,c,h)&&!EdgeSeparates(bc,a,b,c,h)&&!EdgeSeparates(ca,a,b,c,h);
        }
        static bool EdgeSeparates(Vector3 e,Vector3 a,Vector3 b,Vector3 c,Vector3 h)=>Separate(Vector3.Cross(e,Vector3.right),a,b,c,h)||Separate(Vector3.Cross(e,Vector3.up),a,b,c,h)||Separate(Vector3.Cross(e,Vector3.forward),a,b,c,h);
        static bool Separate(Vector3 axis,Vector3 a,Vector3 b,Vector3 c,Vector3 h)
        {
            if(axis.sqrMagnitude<1e-18f)return false;
            float x=Vector3.Dot(axis,a),y=Vector3.Dot(axis,b),z=Vector3.Dot(axis,c);
            float radius=Mathf.Abs(axis.x)*h.x+Mathf.Abs(axis.y)*h.y+Mathf.Abs(axis.z)*h.z;
            return Mathf.Min(x,Mathf.Min(y,z))>=radius||Mathf.Max(x,Mathf.Max(y,z))<=-radius;
        }
    }
}
