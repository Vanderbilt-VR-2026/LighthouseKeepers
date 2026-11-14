using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.Presentation.Environment
{
    // Original project geometry, metres. +Z is the bow. Baked once by SteamerAuthoring.
    public static class SteamerGeometry
    {
        public const int TriangleBudget=12000,SmokeCount=72,SprayCount=32;
        public const float Length=52,Beam=8.8f;
        static readonly Color iron=new(.075f,.095f,.105f,0),rust=new(.23f,.075f,.035f,0),cream=new(.65f,.59f,.44f,0),wood=new(.22f,.12f,.055f,0),brass=new(.48f,.31f,.12f,0),lamp=new(1,.63f,.24f,1);
        public static Mesh BuildHull()
        {
            var b=new Builder();
            // Shaped bow, curved stern and a broad midship, with distinct boot-top and bulwark.
            for(int side=-1;side<=1;side+=2)for(int i=0;i<40;i++)
            {
                float z=-26+i*1.3f,nz=z+1.3f,w=Width(z),nw=Width(nz);
                float[] y={-2.4f,-1.3f,0,.35f,2.5f,3.2f};float[] scale={.4f,.85f,1,1,1,.96f};
                for(int k=0;k<y.Length-1;k++)
                {
                    var a=new Vector3(side*w*scale[k],y[k],z);var c=new Vector3(side*nw*scale[k],y[k],nz);
                    var d=new Vector3(side*nw*scale[k+1],y[k+1],nz);var e=new Vector3(side*w*scale[k+1],y[k+1],z);
                    if(side>0)b.Quad(a,e,d,c,k<2?rust:k==2?cream:iron);else b.Quad(c,d,e,a,k<2?rust:k==2?cream:iron);
                }
                if(side==1)b.Quad(new(-w,2.48f,z),new(-nw,2.48f,nz),new(nw,2.48f,nz),new(w,2.48f,z),wood);
            }
            b.Box(new(0,3.45f,-2),new(6.6f,1.9f,23),cream);
            b.Box(new(0,4.5f,-2),new(7.2f,.22f,24),iron);
            b.Box(new(0,5.5f,7),new(5.4f,1.9f,5.8f),wood);
            b.Box(new(0,6.55f,7),new(6.1f,.22f,6.4f),cream);
            b.Box(new(0,2.9f,17.5f),new(4.5f,.8f,6),iron);
            b.Box(new(0,2.9f,-19),new(4.7f,.8f,5),wood);
            // Riveted funnel, cap and black company band.
            b.Tube(new(0,4.65f,-3),new(0,8.7f,-3.3f),1.05f,20,rust);
            b.Tube(new(0,7.4f,-3.2f),new(0,8.4f,-3.28f),1.08f,20,iron);
            b.Torus(new(0,8.72f,-3.3f),Vector3.up,1.04f,.11f,20,6,iron);
            for(int side=-1;side<=1;side+=2)
            {
                // Portholes with brass rims; bright cores use the same shared material.
                for(int i=0;i<22;i++)
                {
                    float z=-20+i*1.8f;float x=side*(Width(z)+.012f);var centre=new Vector3(x,1.65f,z);
                    b.Torus(centre,Vector3.right,.17f,.035f,10,4,brass);b.Disc(centre+Vector3.right*side*.02f,Vector3.right*side,.145f,10,lamp);
                }
                for(int i=0;i<10;i++)b.Box(new(side*3.314f,3.65f,-11+i*2),new(.025f,.52f,.62f),i%4==0?iron:lamp);
                for(int i=0;i<4;i++)b.Box(new(side*2.714f,5.62f,5+i*1.3f),new(.025f,.7f,.8f),lamp);
                // Continuous railing is rounded, sufficiently thick to survive mip/distance sampling.
                for(int i=0;i<25;i++)
                {
                    float z=-23+i*1.9f;float nz=z+1.9f,x=side*(Width(z)*.94f),nx=side*(Width(nz)*.94f);
                    b.Tube(new(x,3.2f,z),new(x,4.15f,z),.055f,6,iron);
                    foreach(float y in new[]{3.65f,4.15f})b.Tube(new(x,y,z),new(nx,y,nz),.045f,6,iron);
                }
                // Two suspended wooden lifeboats on either side, open inside with seats.
                foreach(float z in new[]{-9f,0f})
                {
                    for(int i=0;i<12;i++)
                    {
                        float z0=z-2.8f+i*.4666667f,z1=z0+.4666667f;
                        float w0=.8f*Mathf.Sin(i/12f*Mathf.PI),w1=.8f*Mathf.Sin((i+1)/12f*Mathf.PI);
                        float x=side*3.7f;
                        b.Quad(new(x-w0,4.7f,z0),new(x+w0,4.7f,z0),new(x+w1,4.7f,z1),new(x-w1,4.7f,z1),wood);
                        b.Quad(new(x,4.15f,z0),new(x-w0,4.7f,z0),new(x-w1,4.7f,z1),new(x,4.15f,z1),cream);
                        b.Quad(new(x,4.15f,z1),new(x+w1,4.7f,z1),new(x+w0,4.7f,z0),new(x,4.15f,z0),cream);
                    }
                    foreach(float dz in new[]{-1.5f,0,1.5f})b.Box(new(side*3.7f,4.78f,z+dz),new(1.1f,.12f,.25f),wood);
                    foreach(float dz in new[]{-2f,2f})b.Tube(new(side*2.8f,4.6f,z+dz),new(side*4.5f,5.7f,z+dz),.075f,8,iron);
                }
                b.Box(new(side*2.1f,6.78f,7.8f),new(.22f,.22f,.25f),side>0?new Color(.06f,.85f,.27f,1):new Color(1,.09f,.03f,1));
            }
            // Wheelhouse forward windows, masts, booms and stays.
            for(int i=0;i<5;i++)b.Box(new(-2+i,5.62f,9.914f),new(.72f,.74f,.025f),lamp);
            foreach(float z in new[]{16f,-18f})
            {
                b.Tube(new(0,2.8f,z),new(0,12.7f,z-1),.14f,12,wood);
                b.Tube(new(-3,9,z-.7f),new(3,9,z-.7f),.08f,8,wood);
                b.Box(new(0,12.75f,z-1),new(.27f,.32f,.27f),lamp);
                for(int side=-1;side<=1;side+=2)
                {b.Tube(new(0,11.8f,z-.9f),new(side*3.4f,3.1f,z+5),.028f,6,iron);b.Tube(new(0,11.8f,z-.9f),new(side*3.4f,3.1f,z-5),.028f,6,iron);}
            }
            // Low vents and bollards: shapes that read from the gallery, no individual colliders/lights.
            foreach(float z in new[]{-22f,-14f,13f,21f})for(int side=-1;side<=1;side+=2){b.Tube(new(side*2,2.6f,z),new(side*2,3.2f,z),.17f,10,iron);b.Tube(new(side*2-.3f,3.13f,z),new(side*2+.3f,3.13f,z),.1f,8,iron);}
            return b.Mesh("Original coastal steamer");
        }
        public static float Width(float z){float t=Mathf.Clamp01((z+26)/52);return 4.4f*Mathf.Pow(Mathf.Sin(Mathf.PI*t),.37f)+.04f;}
        public static Mesh BuildName(string name)
        {
            var b=new Builder();string text=name.ToUpperInvariant();float advance=.54f;
            for(int side=-1;side<=1;side+=2)for(int letter=0;letter<text.Length;letter++){
                if(!Glyphs.TryGetValue(text[letter],out string bits))continue;
                for(int y=0;y<7;y++)for(int x=0;x<5;x++)if(bits[y*5+x]=='1'){
                    float z0=17+side*(letter*advance+x*.085f-text.Length*advance*.5f),z1=z0+side*.08f,low=2.42f-y*.085f,high=low+.08f;
                    Vector3 V(float z,float h)=>new(side*(Width(z)+.04f),h,z);
                    b.Quad(V(z0,low),V(z0,high),V(z1,high),V(z1,low),cream);
                }
            }
            return b.Mesh("Painted ship name "+name);
        }
        static readonly Dictionary<char,string> Glyphs=new(){
            ['A']="01110100011000111111100011000110001",['B']="11110100011000111110100011000111110",
            ['C']="01111100001000010000100001000001111",['D']="11110100011000110001100011000111110",
            ['E']="11111100001000011110100001000011111",['F']="11111100001000011110100001000010000",
            ['G']="01111100001000010111100011000101111",['H']="10001100011000111111100011000110001",
            ['I']="11111001000010000100001000010011111",['J']="00111000100001000010000101001001100",
            ['K']="10001100101010011000101001001010001",['L']="10000100001000010000100001000011111",
            ['M']="10001110111010110101100011000110001",['N']="10001110011010110011100011000110001",
            ['O']="01110100011000110001100011000101110",['P']="11110100011000111110100001000010000",
            ['Q']="01110100011000110001101011001001101",['R']="11110100011000111110101001001010001",
            ['S']="01111100001000001110000010000111110",['T']="11111001000010000100001000010000100",
            ['U']="10001100011000110001100011000101110",['V']="10001100011000110001100010101000100",
            ['W']="10001100011000110101101011101110001",['X']="10001100010101000100010101000110001",
            ['Y']="10001100010101000100001000010000100",['Z']="11111000010001000100010001000011111"
        };
        public static Mesh BuildDistant()
        {
            var b=new Builder();b.Box(new(0,1.6f,0),new(7,2.5f,45),iron);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<7;i++)b.Box(new(side*3.6f,3.6f,-16+i*5),new(.25f,.5f,.8f),lamp);
            foreach(float z in new[]{-18f,16f})b.Box(new(0,12.7f,z),new(.6f,.6f,.6f),lamp);
            return b.Mesh("Steamer distant lights");
        }
        public static Mesh BuildBeam()
        {
            var b=new Builder();for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24,c=(i+1)*Mathf.PI*2/24;b.Triangle(Vector3.zero,new(Mathf.Cos(a)*3,Mathf.Sin(a)*3,38),new(Mathf.Cos(c)*3,Mathf.Sin(c)*3,38),new Color(.7f,.72f,.58f,0));}
            return b.Mesh("Steamer searchlight cone");
        }
        public static Mesh BuildParticles()
        {
            var b=new Builder();for(int i=0;i<SmokeCount+SprayCount;i++){
                bool smoke=i<SmokeCount;var p=smoke?new Vector3(0,8.8f,-3.3f):new Vector3((i%2==0?1:-1)*.9f,.15f,24.5f);
                int n=b.vertices.Count;for(int j=0;j<4;j++){b.vertices.Add(p);b.colours.Add(smoke?new Color(.085f,.09f,.095f,.5f):new Color(.46f,.61f,.65f,1));b.uv.Add(new Vector2(j==1||j==2?1:0,j>=2?1:0));b.phases.Add(new Vector2((i*.61803399f)%1,(i*.381966f)%1));}
                b.triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            var mesh=b.Mesh("Steamer GPU smoke and bow spray");mesh.bounds=new Bounds(new Vector3(0,9,0),new Vector3(40,30,80));return mesh;
        }
        public static Pose Route(float age,float approach,float window,float depart,float sea=-.5f)
            => LighthouseKeepers.Core.Progression.ShipNavigation.Route(age,approach,window,depart,sea);
        sealed class Builder
        {
            public readonly List<Vector3> vertices=new();public readonly List<Color> colours=new();public readonly List<Vector2> uv=new(),phases=new();public readonly List<int> triangles=new();
            public void Triangle(Vector3 a,Vector3 b,Vector3 c,Color colour){int n=vertices.Count;vertices.AddRange(new[]{a,b,c});for(int j=0;j<3;j++){colours.Add(colour);uv.Add(Vector2.zero);phases.Add(Vector2.zero);}triangles.AddRange(new[]{n,n+1,n+2});}
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color colour){int n=vertices.Count;vertices.AddRange(new[]{a,b,c,d});for(int j=0;j<4;j++){colours.Add(colour);uv.Add(new Vector2(j==1||j==2?1:0,j>=2?1:0));phases.Add(Vector2.zero);}triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
            public void Box(Vector3 p,Vector3 s,Color c){var e=s*.5f;Vector3 V(int x,int y,int z)=>p+Vector3.Scale(e,new Vector3(x,y,z));Quad(V(-1,-1,-1),V(-1,1,-1),V(1,1,-1),V(1,-1,-1),c);Quad(V(1,-1,1),V(1,1,1),V(-1,1,1),V(-1,-1,1),c);Quad(V(-1,-1,1),V(-1,1,1),V(-1,1,-1),V(-1,-1,-1),c);Quad(V(1,-1,-1),V(1,1,-1),V(1,1,1),V(1,-1,1),c);Quad(V(-1,1,-1),V(-1,1,1),V(1,1,1),V(1,1,-1),c);Quad(V(-1,-1,1),V(-1,-1,-1),V(1,-1,-1),V(1,-1,1),c);}
            static void Basis(Vector3 normal,out Vector3 x,out Vector3 y){x=Vector3.Cross(normal,Mathf.Abs(normal.y)>.9f?Vector3.forward:Vector3.up).normalized;y=Vector3.Cross(normal,x).normalized;}
            public void Tube(Vector3 a,Vector3 b,float r,int sides,Color c){var n=(b-a).normalized;Basis(n,out var x,out var y);for(int i=0;i<sides;i++){float f=i*Mathf.PI*2/sides,g=(i+1)*Mathf.PI*2/sides;var u=r*(x*Mathf.Cos(f)+y*Mathf.Sin(f));var v=r*(x*Mathf.Cos(g)+y*Mathf.Sin(g));Quad(a+u,a+v,b+v,b+u,c);}Disc(a,-n,r,sides,c);Disc(b,n,r,sides,c);}
            public void Disc(Vector3 p,Vector3 normal,float r,int sides,Color c){Basis(normal,out var x,out var y);for(int i=0;i<sides;i++){float f=i*Mathf.PI*2/sides,g=(i+1)*Mathf.PI*2/sides;Triangle(p,p+(x*Mathf.Cos(f)+y*Mathf.Sin(f))*r,p+(x*Mathf.Cos(g)+y*Mathf.Sin(g))*r,c);}}
            public void Torus(Vector3 p,Vector3 normal,float r,float tube,int rings,int sides,Color c){Basis(normal,out var x,out var y);Vector3 V(int i,int j){float a=i*Mathf.PI*2/rings,b=j*Mathf.PI*2/sides;var radial=x*Mathf.Cos(a)+y*Mathf.Sin(a);return p+radial*(r+Mathf.Cos(b)*tube)+normal*Mathf.Sin(b)*tube;}for(int i=0;i<rings;i++)for(int j=0;j<sides;j++)Quad(V(i,j),V(i+1,j),V(i+1,j+1),V(i,j+1),c);}
            public Mesh Mesh(string name){var m=new Mesh{name=name};m.SetVertices(vertices);m.SetColors(colours);m.SetUVs(0,uv);m.SetUVs(1,phases);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
        }
    }
}
