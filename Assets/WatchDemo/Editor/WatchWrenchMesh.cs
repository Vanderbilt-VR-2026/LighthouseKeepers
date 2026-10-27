using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo.Editor
{
    public static class WatchWrenchMesh
    {
        // One closed, triangulated steel silhouette. No rendered primitive parts.
        public static Mesh Create()
        {
            var p=new List<Vector2>{new(-.011f,-.109f),new(-.011f,.055f),new(-.026f,.075f),new(-.036f,.090f),new(-.034f,.112f),new(-.022f,.130f),new(-.015f,.125f),new(-.019f,.100f),new(-.010f,.087f),new(.010f,.087f),new(.019f,.100f),new(.015f,.125f),new(.022f,.130f),new(.034f,.112f),new(.036f,.090f),new(.026f,.075f),new(.011f,.055f),new(.011f,-.109f)};
            for(int i=1;i<12;i++){float a=-Mathf.PI*i/12;p.Add(new Vector2(.011f*Mathf.Cos(a),-.109f+.011f*Mathf.Sin(a)));}
            float area=0;for(int i=0;i<p.Count;i++)area+=Cross(p[i],p[(i+1)%p.Count]);if(area<0)p.Reverse();
            var face=new List<int>();var remaining=new List<int>();for(int i=0;i<p.Count;i++)remaining.Add(i);
            for(int guard=0;remaining.Count>2 && guard<200;guard++)
            {
                bool found=false;
                for(int i=0;i<remaining.Count;i++)
                {
                    int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];
                    if(Cross(p[b]-p[a],p[c]-p[b])<=0)continue;
                    bool occupied=false;foreach(int k in remaining)if(k!=a&&k!=b&&k!=c&&Inside(p[k],p[a],p[b],p[c])){occupied=true;break;}
                    if(occupied)continue;face.Add(a);face.Add(b);face.Add(c);remaining.RemoveAt(i);found=true;break;
                }
                if(!found)throw new System.InvalidOperationException("Wrench outline cannot be triangulated");
            }
            var vertices=new List<Vector3>();var tris=new List<int>();var uv=new List<Vector2>();
            void Triangle(Vector3 a,Vector3 b,Vector3 c){int start=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);tris.Add(start);tris.Add(start+1);tris.Add(start+2);uv.Add(new Vector2(a.x*12,a.z*4));uv.Add(new Vector2(b.x*12,b.z*4));uv.Add(new Vector2(c.x*12,c.z*4));}
            var inset=new Vector2[p.Count];
            for(int i=0;i<p.Count;i++)
            {
                var before=(p[i]-p[(i+p.Count-1)%p.Count]).normalized;var after=(p[(i+1)%p.Count]-p[i]).normalized;
                var n1=new Vector2(-before.y,before.x);var n2=new Vector2(-after.y,after.x);var bisector=(n1+n2).normalized;
                inset[i]=p[i]+bisector*Mathf.Min(.002f,.0012f/Mathf.Max(.1f,Vector2.Dot(bisector,n1)));
            }
            Vector3 Point(int i,float y,bool cap=false)=>new Vector3(cap?inset[i].x:p[i].x,y,cap?inset[i].y:p[i].y);
            for(int i=0;i<face.Count;i+=3){int a=face[i],b=face[i+1],c=face[i+2];Triangle(Point(a,.005f,true),Point(c,.005f,true),Point(b,.005f,true));Triangle(Point(a,-.005f,true),Point(b,-.005f,true),Point(c,-.005f,true));}
            for(int i=0;i<p.Count;i++)
            {
                int j=(i+1)%p.Count;
                Triangle(Point(i,-.0038f),Point(j,.0038f),Point(i,.0038f));Triangle(Point(i,-.0038f),Point(j,-.0038f),Point(j,.0038f));
                Triangle(Point(i,.0038f),Point(j,.005f,true),Point(i,.005f,true));Triangle(Point(i,.0038f),Point(j,.0038f),Point(j,.005f,true));
                Triangle(Point(i,-.0038f),Point(i,-.005f,true),Point(j,-.005f,true));Triangle(Point(i,-.0038f),Point(j,-.005f,true),Point(j,-.0038f));
            }
            var mesh=new Mesh{name="25 cm forged open-end wrench"};mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static bool Inside(Vector2 p,Vector2 a,Vector2 b,Vector2 c)=>Cross(b-a,p-a)>=0&&Cross(c-b,p-b)>=0&&Cross(a-c,p-c)>=0;
    }
}
