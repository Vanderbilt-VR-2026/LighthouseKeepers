using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Small original hard-surface mesh tool. Metres, UVs, shared faces with explicit bevels.
    // No colliders, primitive GameObjects, lights or per-frame geometry.
    public sealed class WorkshopMesh
    {
        readonly List<Vector3> vertices=new();readonly List<int> indices=new();readonly List<Vector2> uv=new();
        void Triangle(Vector3 a,Vector3 b,Vector3 c){int n=vertices.Count;vertices.AddRange(new[]{a,b,c});uv.AddRange(new[]{new Vector2(a.x+a.z,a.y+a.z),new Vector2(b.x+b.z,b.y+b.z),new Vector2(c.x+c.z,c.y+c.z)});indices.AddRange(new[]{n,n+1,n+2});}
        void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){Triangle(a,b,c);Triangle(a,c,d);}
        // Rounded XY extrusion: tangent bevel catches baked probe highlights, no paper-thin edges.
        public void Case(Vector3 centre,Vector3 size,float radius)
        {
            float x=size.x*.5f,y=size.y*.5f,z=size.z*.5f,r=Mathf.Min(radius,Mathf.Min(x,y)*.8f),edge=Mathf.Min(r*.25f,z*.4f);
            const int n=24;var ring=new Vector3[n];
            for(int i=0;i<n;i++){int quadrant=i/6;float a=(quadrant*90+(i%6)*90f/5)*Mathf.Deg2Rad;ring[i]=new Vector3((quadrant==0||quadrant==3?x-r:-x+r)+Mathf.Cos(a)*r,(quadrant<2?y-r:-y+r)+Mathf.Sin(a)*r,0);}
            for(int i=0;i<n;i++){var a=ring[i];var b=ring[(i+1)%n];var insetA=new Vector3(a.x*(1-edge/x),a.y*(1-edge/y),0);var insetB=new Vector3(b.x*(1-edge/x),b.y*(1-edge/y),0);
                Quad(centre+a-Vector3.forward*(z-edge),centre+b-Vector3.forward*(z-edge),centre+b+Vector3.forward*(z-edge),centre+a+Vector3.forward*(z-edge));
                Quad(centre+a+Vector3.forward*(z-edge),centre+b+Vector3.forward*(z-edge),centre+insetB+Vector3.forward*z,centre+insetA+Vector3.forward*z);
                Triangle(centre+Vector3.forward*z,centre+insetA+Vector3.forward*z,centre+insetB+Vector3.forward*z);
                Quad(centre+b-Vector3.forward*(z-edge),centre+a-Vector3.forward*(z-edge),centre+insetA-Vector3.forward*z,centre+insetB-Vector3.forward*z);
                Triangle(centre-Vector3.forward*z,centre+insetB-Vector3.forward*z,centre+insetA-Vector3.forward*z);
            }
        }
        public void Tube(Vector3 a,Vector3 b,float radius,int sides=12)
        {
            var axis=(b-a).normalized;var right=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.forward:Vector3.up).normalized;var up=Vector3.Cross(axis,right);
            for(int i=0;i<sides;i++){float f=i*Mathf.PI*2/sides,g=(i+1)*Mathf.PI*2/sides;var p=(right*Mathf.Cos(f)+up*Mathf.Sin(f))*radius;var q=(right*Mathf.Cos(g)+up*Mathf.Sin(g))*radius;Quad(a+p,a+q,b+q,b+p);Triangle(a,a+q,a+p);Triangle(b,b+p,b+q);}
        }
        public void Pipe(float radius,params Vector3[] points){for(int i=1;i<points.Length;i++)Tube(points[i-1],points[i],radius,8);}
        public Mesh Finish(string name){var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;}
    }
}
