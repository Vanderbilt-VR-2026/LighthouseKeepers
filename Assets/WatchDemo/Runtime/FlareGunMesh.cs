using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Original signal-pistol silhouette in metres. No borrowed VRChat code, textures or meshes.
    public static class FlareGunMesh
    {
        public static readonly Vector3 GripCentre = new(0, -.045f, -.035f);
        public static readonly Vector3 Muzzle = new(0, .061f, .20f);
        public static Mesh[] Build()
        {
            var iron = new WorkshopMesh(); var paint = new WorkshopMesh(); var grip = new WorkshopMesh();
            // The receiver is one chamfered side profile; the wood panels follow its slanted grip.
            var frame = Profile(new[]{new Vector2(-.075f,.065f),new Vector2(.045f,.065f),new Vector2(.065f,.027f),new Vector2(.014f,.006f),new Vector2(-.002f,-.110f),new Vector2(-.064f,-.117f),new Vector2(-.047f,.006f),new Vector2(-.078f,.025f)},.031f,.003f);
            var stocks = Profile(new[]{new Vector2(-.042f,.002f),new Vector2(.002f,-.008f),new Vector2(-.010f,-.101f),new Vector2(-.057f,-.108f)},.039f,.004f);
            // Barrel, breech band, front lip and dark hollow bore are actual circular profiles.
            var barrel = Lathe(new[]{new Vector2(-.035f,.028f),new Vector2(-.031f,.031f),new Vector2(-.020f,.031f),new Vector2(-.016f,.025f),new Vector2(.185f,.025f),new Vector2(.192f,.028f),new Vector2(.200f,.028f),new Vector2(.202f,.022f),new Vector2(.202f,.0185f),new Vector2(.162f,.0185f),new Vector2(.155f,0)},.061f,32);
            iron.Pipe(.0036f,new Vector3(0,.014f,.006f),new Vector3(0,.008f,.042f),new Vector3(0,-.006f,.058f),new Vector3(0,-.044f,.054f),new Vector3(0,-.055f,.034f),new Vector3(0,-.046f,.004f));
            iron.Pipe(.0028f,new Vector3(0,.006f,.025f),new Vector3(0,-.014f,.020f),new Vector3(0,-.028f,.030f));
            iron.Pipe(.007f,new Vector3(0,.050f,-.072f),new Vector3(0,.085f,-.085f),new Vector3(0,.092f,-.068f));
            // Break-action pivot pin and countersunk grip screws, duplicated on both sides.
            foreach(int side in new[]{-1,1}) {
                iron.Tube(new Vector3(side*.017f,.032f,.035f),new Vector3(side*.021f,.032f,.035f),.009f,12);
                foreach(float y in new[]{-.022f,-.081f}) {
                    float z=Mathf.Lerp(-.021f,-.034f,(-y-.022f)/.059f);
                    iron.Tube(new Vector3(side*.0195f,y,z),new Vector3(side*.0215f,y,z),.0043f,12);
                }
                // Shallow raised grip ribs catch probe highlights without floating strips.
                for(int i=0;i<7;i++) {float y=-.025f-i*.009f;grip.Pipe(.0009f,new Vector3(side*.020f,y,-.043f),new Vector3(side*.020f,y,-.010f));}
            }
            paint.Case(new Vector3(0,.089f,.174f),new Vector3(.006f,.010f,.017f),.002f);
            paint.Case(new Vector3(0,.081f,-.025f),new Vector3(.022f,.008f,.012f),.002f);
            iron.Pipe(.0024f,new Vector3(-.008f,-.114f,-.037f),new Vector3(-.008f,-.129f,-.037f),new Vector3(.008f,-.129f,-.037f),new Vector3(.008f,-.114f,-.037f));
            return new[]{Combine("Signal pistol receiver and hardware",frame,iron.Finish("hardware")),Combine("Signal pistol enamel barrel",barrel,paint.Finish("sights")),Combine("Fitted signal pistol grip panels",stocks,grip.Finish("grip ribs"))};
        }
        static Mesh Combine(string name, params Mesh[] parts)
        {var list=new CombineInstance[parts.Length];for(int i=0;i<parts.Length;i++)list[i]=new CombineInstance{mesh=parts[i],transform=Matrix4x4.identity};var m=new Mesh{name=name};m.CombineMeshes(list);foreach(var p in parts)Object.DestroyImmediate(p);return m;}
        static Mesh Profile(Vector2[] polygon,float width,float bevel)
        {
            var v=new List<Vector3>();var ix=new List<int>();var uv=new List<Vector2>();
            void Tri(Vector3 a,Vector3 b,Vector3 c){int n=v.Count;v.AddRange(new[]{a,b,c});ix.AddRange(new[]{n,n+1,n+2});uv.AddRange(new[]{new Vector2(a.z*5,a.y*5),new Vector2(b.z*5,b.y*5),new Vector2(c.z*5,c.y*5)});}
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){Tri(a,b,c);Tri(a,c,d);}
            Vector2 centre=Vector2.zero;foreach(var p in polygon)centre+=p;centre/=polygon.Length;
            // Work with clockwise yz outline (its outward face points +x).
            float area=0;for(int i=0;i<polygon.Length;i++){var p=polygon[i];var q=polygon[(i+1)%polygon.Length];area+=p.x*q.y-q.x*p.y;}if(area>0)System.Array.Reverse(polygon);
            Vector3 P(Vector2 p,float x)=>new(x,p.y,p.x);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<polygon.Length;i++) {
                var a=polygon[i];var b=polygon[(i+1)%polygon.Length];var ai=Vector2.MoveTowards(a,centre,bevel);var bi=Vector2.MoveTowards(b,centre,bevel);
                var pa=P(a,side*(width*.5f-bevel));var pb=P(b,side*(width*.5f-bevel));var ia=P(ai,side*width*.5f);var ib=P(bi,side*width*.5f);var c=P(centre,side*width*.5f);
                if(side>0){Quad(pa,pb,ib,ia);Tri(c,ia,ib);}else {Quad(pb,pa,ia,ib);Tri(c,ib,ia);}
                if(side>0)Quad(P(b,-width*.5f+bevel),P(a,-width*.5f+bevel),pa,pb);
            }
            return Finish("Bevelled side casting",v,ix,uv);
        }
        static Mesh Lathe(Vector2[] profile,float y,int segments)
        {
            var v=new List<Vector3>();var ix=new List<int>();var uv=new List<Vector2>();
            for(int row=0;row<profile.Length;row++)for(int i=0;i<=segments;i++){float a=i*Mathf.PI*2/segments;v.Add(new Vector3(Mathf.Cos(a)*profile[row].y,y+Mathf.Sin(a)*profile[row].y,profile[row].x));uv.Add(new Vector2(i/(float)segments,profile[row].x*4));}
            for(int row=0;row<profile.Length-1;row++)for(int i=0;i<segments;i++){int a=row*(segments+1)+i,b=a+segments+1;ix.AddRange(new[]{a,a+1,b+1,a,b+1,b});}
            return Finish("Hollow tapered signal barrel",v,ix,uv);
        }
        static Mesh Finish(string name,List<Vector3> v,List<int> ix,List<Vector2> uv){var m=new Mesh{name=name};m.SetVertices(v);m.SetTriangles(ix,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();return m;}
    }
}
