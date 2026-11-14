using System.Collections.Generic;
using UnityEngine;

namespace LighthouseKeepers.WatchDemo
{
    // Original low-poly oilskin keeper. No external character, collider or runtime mesh rebuild.
    public static class DrownedSculpt
    {
        sealed class Shape
        {
            readonly List<Vector3> vertices=new(); readonly List<Color> colours=new();
            readonly List<Vector2> uv=new(); readonly List<int> triangles=new();
            public int Vertex(Vector3 p,Color c,float kind=0,float head=0)
            {int i=vertices.Count;vertices.Add(p);colours.Add(c);uv.Add(new Vector2(kind,head));return i;}
            public void Tri(int a,int b,int c){triangles.Add(a);triangles.Add(b);triangles.Add(c);}
            public void Grid(Vector3[,] p,Color colour,float kind=0,float head=0)
            {
                int start=vertices.Count,rows=p.GetLength(0),cols=p.GetLength(1);
                for(int r=0;r<rows;r++)for(int c=0;c<cols;c++)Vertex(p[r,c],colour,kind,head);
                for(int r=0;r<rows-1;r++)for(int c=0;c<cols-1;c++)
                {int a=start+r*cols+c,b=a+1,d=a+cols,e=d+1;Tri(a,b,e);Tri(a,e,d);}
            }
            public void Tube(Vector3[] centres,float[] widths,float[] depths,Color c,int sides=10,float kind=0)
            {
                var p=new Vector3[centres.Length,sides+1];
                for(int r=0;r<centres.Length;r++)
                {
                    Vector3 along=(centres[Mathf.Min(r+1,centres.Length-1)]-centres[Mathf.Max(0,r-1)]).normalized;
                    Vector3 right=Vector3.Cross(along,Vector3.forward).normalized;
                    if(right.sqrMagnitude<.01f)right=Vector3.right;
                    var forward=Vector3.Cross(right,along).normalized;
                    for(int j=0;j<=sides;j++){float a=j*Mathf.PI*2/sides;p[r,j]=centres[r]+right*Mathf.Sin(a)*widths[r]+forward*Mathf.Cos(a)*depths[r];}
                }
                Grid(p,c,kind);
            }
            public void Ellipsoid(Vector3 centre,Vector3 radius,Color c,int sides=12,int rings=6,float kind=1,bool head=false)
            {
                var p=new Vector3[rings+1,sides+1];
                for(int r=0;r<=rings;r++)for(int j=0;j<=sides;j++)
                {float a=j*Mathf.PI*2/sides,b=r*Mathf.PI/rings;var v=centre+Vector3.Scale(radius,new Vector3(Mathf.Sin(a)*Mathf.Sin(b),Mathf.Cos(b),Mathf.Cos(a)*Mathf.Sin(b)));p[r,j]=head?Head(v):v;}
                Grid(p,c,kind,head?1:0);
            }
            public Mesh Finish(string name){var m=new Mesh{name=name};m.SetVertices(vertices);m.SetColors(colours);m.SetUVs(0,uv);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
        }
        static Vector3 Head(Vector3 p)=>new Vector3(.055f,1.78f,.055f)+Quaternion.Euler(10,0,-14)*p;
        public static Mesh Body()
        {
            var s=new Shape();Color cloth=new(.018f,.031f,.027f),worn=new(.032f,.041f,.033f),skin=new(.13f,.16f,.14f),voidColour=new(.0015f,.003f,.003f),boot=new(.009f,.014f,.013f);
            float[] ys={.24f,.38f,.67f,.95f,1.22f,1.45f,1.57f,1.61f};
            float[] xs={.32f,.33f,.28f,.24f,.28f,.34f,.29f,.12f},zs={.20f,.21f,.18f,.15f,.17f,.19f,.16f,.09f};
            var coat=new Vector3[ys.Length,21];
            for(int r=0;r<ys.Length;r++)for(int j=0;j<=20;j++)
            {
                float a=j*Mathf.PI/10,fold=1+.055f*Mathf.Cos(a*7+r*.25f);
                float hem=r==0?.04f*Mathf.Sin(a*7)+.045f*Mathf.Cos(a*3):0;
                float sag=(r>=5?-.055f*Mathf.Sin(a):0);
                coat[r,j]=new Vector3(Mathf.Sin(a)*xs[r]*fold+.025f*r/ys.Length,ys[r]+hem+sag,Mathf.Cos(a)*zs[r]*fold+.025f*r/ys.Length);
            }
            s.Grid(coat,cloth);
            // Uneven torn tails, storm flap and raised seams remain part of the single body mesh.
            for(int j=0;j<7;j++){
                float a=j*Mathf.PI*2/7;var p=new Vector3(Mathf.Sin(a)*.31f,.27f,Mathf.Cos(a)*.20f);
                int x=s.Vertex(p+Vector3.right*.025f,worn),y=s.Vertex(p-Vector3.right*.025f,cloth),z=s.Vertex(p+new Vector3(.02f,-.12f-j%3*.015f,.015f),cloth);s.Tri(x,y,z);
            }
            s.Tube(new[]{new Vector3(-.02f,.3f,.21f),new Vector3(-.01f,.8f,.18f),new Vector3(.01f,1.25f,.20f),new Vector3(.025f,1.55f,.19f)},new[]{.013f,.014f,.012f,.01f},new[]{.008f,.008f,.008f,.006f},worn,6);
            for(int side=-1;side<=1;side+=2)
            {
                float drop=side<0?.035f:0;
                s.Tube(new[]{new Vector3(side*.27f,1.49f-drop,0),new Vector3(side*.38f,1.30f-drop,.015f),new Vector3(side*.42f,1.04f-drop,.05f),new Vector3(side*.38f,.75f-drop,.115f)},new[]{.10f,.093f,.075f,.055f},new[]{.105f,.09f,.075f,.055f},cloth,12);
                var palm=new Vector3(side*.38f,.67f-drop,.125f);s.Ellipsoid(palm,new Vector3(.055f,.095f,.04f),skin,10,4);
                for(int finger=0;finger<4;finger++)
                {
                    float offset=(finger-1.5f)*.024f,len=.12f+.025f*(finger==1||finger==2?1:0);
                    var a=palm+new Vector3(offset,-.055f,.009f);
                    s.Tube(new[]{a,a+new Vector3(side*.007f,-len*.55f,.012f),a+new Vector3(side*.014f,-len,.035f)},new[]{.010f,.009f,.003f},new[]{.012f,.010f,.006f},skin,6,1);
                }
                var thumb=palm+new Vector3(-side*.045f,.01f,.015f);
                s.Tube(new[]{thumb,thumb+new Vector3(-side*.025f,-.05f,.025f),thumb+new Vector3(-side*.015f,-.09f,.045f)},new[]{.017f,.012f,.006f},new[]{.016f,.012f,.008f},skin,6,1);
                s.Ellipsoid(new Vector3(side*.13f,.10f,side<0?.075f:.045f),new Vector3(.085f,.105f,.19f),boot,10,4,0);
            }
            // A rounded collapsed hood; asymmetric skull/jaw barely emerge from its recess.
            var hood=new Vector3[6,25];
            for(int r=0;r<6;r++)for(int j=0;j<=24;j++)
            {float a=j*Mathf.PI/12,b=r*Mathf.PI/10;hood[r,j]=Head(new Vector3(Mathf.Sin(a)*.185f*Mathf.Cos(b),Mathf.Cos(a)*.25f*Mathf.Cos(b),.135f-.31f*Mathf.Sin(b)));}
            s.Grid(hood,cloth,0,1);
            var rim=new Vector3[3,25];
            for(int r=0;r<3;r++)for(int j=0;j<=24;j++)
            {float a=j*Mathf.PI/12,scale=1-r*.10f;rim[r,j]=Head(new Vector3(Mathf.Sin(a)*.18f*scale,Mathf.Cos(a)*.245f*scale,.14f+(r==1?.018f:0)));}
            s.Grid(rim,worn,0,1);
            s.Ellipsoid(new Vector3(0,-.005f,.065f),new Vector3(.145f,.205f,.035f),voidColour,16,6,2,true);
            // Gaunt face stays mostly inside the hood: a narrow jaw, hollow sockets,
            // pinched nose and a thin broken mouth. No circular mask/mouth silhouette.
            s.Ellipsoid(new Vector3(.004f,-.033f,.105f),new Vector3(.087f,.157f,.049f),skin*.38f,16,8,1,true);
            for(int side=-1;side<=1;side+=2)
            {
                s.Ellipsoid(new Vector3(side*.042f,.018f,.148f),new Vector3(.033f,.020f,.012f),voidColour,10,4,2,true);
                s.Ellipsoid(new Vector3(side*.041f,.022f,.16f),new Vector3(.003f,.002f,.002f),new Color(.052f,.067f,.057f),6,3,3,true);
            }
            int n0=s.Vertex(Head(new Vector3(-.016f,.008f,.151f)),skin*.31f,1,1);
            int n1=s.Vertex(Head(new Vector3(.012f,.008f,.151f)),skin*.31f,1,1);
            int n2=s.Vertex(Head(new Vector3(.004f,-.051f,.184f)),skin*.40f,1,1);
            int n3=s.Vertex(Head(new Vector3(-.018f,-.059f,.15f)),skin*.27f,1,1);
            int n4=s.Vertex(Head(new Vector3(.018f,-.056f,.15f)),skin*.27f,1,1);
            s.Tri(n0,n1,n2);s.Tri(n0,n2,n3);s.Tri(n1,n4,n2);s.Tri(n3,n2,n4);
            s.Ellipsoid(new Vector3(.002f,-.087f,.153f),new Vector3(.035f,.007f,.006f),voidColour,12,3,2,true);
            // Uneven dark water trails down the cheeks, with the forehead lost to shadow.
            for(int j=0;j<3;j++)
            {
                float x=(j-1)*.05f;
                var q=new Vector3[3,2];
                for(int r=0;r<3;r++)for(int c=0;c<2;c++)q[r,c]=Head(new Vector3(x+c*.005f+(r%2)*.004f,-.037f-r*.044f,.147f-r*.005f));
                s.Grid(q,cloth*.25f,2,1);
            }
            return s.Finish("Crooked drowned keeper — original sculpt");
        }
        public static Mesh Effects()
        {
            var v=new List<Vector3>();var uv=new List<Vector2>();var seed=new List<Vector2>();var t=new List<int>();
            // Eight low mist cards, 24 drips, two small breath cards. No ParticleSystem.
            for(int i=0;i<34;i++)
            {
                int k=v.Count;float kind=i<8?0:i<32?1:2;
                for(int j=0;j<4;j++){v.Add(Vector3.zero);uv.Add(new Vector2(j==0||j==3?-1:1,j<2?-1:1));seed.Add(new Vector2((i*.618034f)%1,kind));}
                t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});
            }
            var m=new Mesh{name="Bounded Drowned mist and dripping water"};m.SetVertices(v);m.SetUVs(0,uv);m.SetUVs(1,seed);m.SetTriangles(t,0);m.bounds=new Bounds(new Vector3(0,1,0),new Vector3(2,2.5f,2));return m;
        }
    }
}
