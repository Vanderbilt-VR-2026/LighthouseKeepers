using System.Collections.Generic;
using UnityEngine;

namespace LighthouseKeepers.WatchDemo
{
    public enum DrownedForm { SeaHag, Doll, Gaunt }

    // Original authored silhouettes, not meshes/textures taken from the reference pictures.
    // One combined, vertex-coloured mesh per form; only one is rendered in an encounter.
    public static class DrownedForms
    {
        public const int TriangleBudget = 12000;
        static readonly Color Skin = new(.29f,.34f,.30f), Bone = new(.36f,.40f,.34f);
        static readonly Color Hair = new(.018f,.024f,.020f), Hollow = new(.008f,.010f,.009f);
        static readonly Color Cloth = new(.16f,.19f,.17f), Damp = new(.062f,.089f,.074f);
        // The same night/fault receipt already replicated to every keeper determines the form.
        // No local random state: late joins and retries see the same creature.
        public static DrownedForm Select(int night,int fault) => (DrownedForm)((Mathf.Max(0,night-2)+Mathf.Max(0,fault-1))%3);
        public static float Height(DrownedForm form) => form==DrownedForm.Doll?1.28f:form==DrownedForm.Gaunt?2.30f:1.84f;
        public static float FaceHeight(DrownedForm form) => form==DrownedForm.Doll?1.10f:form==DrownedForm.Gaunt?2.04f:1.57f;
        public static Mesh Build(DrownedForm form)
        {
            var s=new Sculpt();
            if(form==DrownedForm.SeaHag)Hag(s);else if(form==DrownedForm.Doll)Doll(s);else Gaunt(s);
            return s.Finish("Drowned "+form+" — original single-material sculpt");
        }
        public static Mesh BuildWalter()
        {
            var s=new Sculpt();Gaunt(s);s.RemoveCoveredSkin();
            var coat=new Color(.035f,.045f,.035f);
            s.Tube(new[]{new Vector3(0,.65f,-.03f),new Vector3(0,1.03f,-.03f),new Vector3(0,1.4f,-.07f),new Vector3(0,1.68f,-.02f),new Vector3(0,1.79f,.04f)},new[]{.25f,.21f,.22f,.31f,.095f},new[]{.18f,.16f,.17f,.20f,.09f},coat,24,0,true);
            for(int side=-1;side<=1;side+=2){
                s.Tube(new[]{new Vector3(side*.255f,1.69f,0),new Vector3(side*.4f,1.13f,.045f),new Vector3(side*.43f,.62f,.16f)},new[]{.105f,.07f,.045f},new[]{.11f,.075f,.05f},coat,16,0,true);
                s.Tube(new[]{new Vector3(side*.13f,1.0f,0),new Vector3(side*.16f,.58f,.09f),new Vector3(side*.18f,.10f,.025f)},new[]{.092f,.064f,.042f},new[]{.1f,.069f,.044f},Damp*.6f,14,0,true);
            }
            // The photograph's pale square repair patch, sewn onto the left shoulder, not a glowing badge.
            s.PaintShoulderPatch();
            for(int i=0;i<5;i++)s.Ellipsoid(new Vector3(.025f,1.55f-i*.14f,.18f),new Vector3(.012f,.012f,.006f),Cloth*.5f,8,4);
            return s.Finish("Walter Brennick — patched soaked oilskin");
        }
        static void Hag(Sculpt s)
        {
            // Bent neck and rounded back under a ragged soaked shift. The head leans towards you.
            s.Tube(new[]{new Vector3(0,.28f,-.02f),new Vector3(0,.62f,-.04f),new Vector3(0,.99f,-.11f),new Vector3(0,1.30f,-.10f),new Vector3(-.02f,1.49f,.05f)},new[]{.29f,.28f,.22f,.33f,.12f},new[]{.20f,.19f,.17f,.27f,.10f},Damp,24,0,true);
            s.Ellipsoid(new Vector3(-.02f,1.45f,-.12f),new Vector3(.27f,.27f,.21f),Damp,20,10);
            for(int side=-1;side<=1;side+=2){
                var shoulder=new Vector3(side*.28f,1.35f,-.035f);var elbow=new Vector3(side*.39f,.99f,.09f);var wrist=new Vector3(side*.41f,.65f,.22f);
                s.Ellipsoid(shoulder,new Vector3(.104f,.11f,.10f),Skin,14,8,1);
                s.Tube(new[]{shoulder,Vector3.Lerp(shoulder,elbow,.5f),elbow,wrist},new[]{.104f,.076f,.053f,.035f},new[]{.10f,.072f,.053f,.033f},Skin,12,1);
                s.Ellipsoid(elbow,new Vector3(.057f,.062f,.052f),Skin,10,6,1);
                Hand(s,wrist+Vector3.down*.055f,side,.95f,Skin);
                s.Tube(new[]{new Vector3(side*.13f,.40f,-.03f),new Vector3(side*.14f,.11f,.05f)},new[]{.065f,.035f},new[]{.065f,.04f},Skin,12,1);
                Foot(s,new Vector3(side*.14f,.045f,.10f),.85f,Skin);
            }
            var centre=new Vector3(-.035f,1.58f,.19f);var rotation=Quaternion.Euler(13,0,-11);
            Face(s,centre,rotation,new Vector3(.135f,.205f,.126f),false);
            // Wet rope-like locks obscure half of the face but leave one eye and the jaw readable.
            HairCap(s,centre,rotation,new Vector3(.152f,.225f,.145f),.70f);
            for(int i=0;i<29;i++){
                float a=i*Mathf.PI*2/29;float side=Mathf.Sin(a),front=Mathf.Cos(a);
                if(front>.64f && side>.05f)continue;
                float length=.40f+.24f*(.5f+.5f*Mathf.Sin(i*2.7f));
                var top=centre+rotation*new Vector3(side*.142f,.10f,front*.127f);
                float veil=front>.25f?.075f:0;
                s.Tube(new[]{top,top+new Vector3(side*.025f+.018f*Mathf.Sin(i*2),-.17f,.025f),top+new Vector3(side*.05f-.020f*Mathf.Cos(i*3),-length*.65f,.05f+veil),top+new Vector3(side*.024f,-length,.09f+veil)},new[]{.005f,.006f,.003f,.0006f},new[]{.002f,.003f,.002f,.0005f},Hair,6,0);
            }
            // Damp hem streaks, loose torn ends and a collar seam, all part of the same mesh.
            for(int i=0;i<13;i++){
                float a=i*Mathf.PI*2/13;var p=new Vector3(Mathf.Sin(a)*.28f,.34f,Mathf.Cos(a)*.195f-.03f);
                s.Tube(new[]{p,p+new Vector3(.01f,-.13f-i%3*.026f,.012f)},new[]{.018f,.001f},new[]{.006f,.001f},Cloth*.62f,5);
            }
        }
        static void Doll(Sculpt s)
        {
            // Child-size, too-still proportions: a sewn nightdress, bare feet and a cracked face.
            s.Tube(new[]{new Vector3(0,.23f,0),new Vector3(0,.39f,0),new Vector3(0,.64f,-.012f),new Vector3(0,.85f,0),new Vector3(0,.92f,0)},new[]{.235f,.22f,.15f,.19f,.075f},new[]{.15f,.14f,.09f,.11f,.06f},Cloth,28,0,true);
            s.Tube(new[]{new Vector3(0,.90f,0),new Vector3(0,1.02f,.015f)},new[]{.05f,.048f},new[]{.05f,.046f},Bone,12,1);
            for(int side=-1;side<=1;side+=2){
                var shoulder=new Vector3(side*.16f,.85f,0);var elbow=new Vector3(side*.24f,.63f,.035f);var wrist=new Vector3(side*.245f,.44f,.055f);
                s.Ellipsoid(shoulder,new Vector3(.062f,.064f,.060f),Cloth,12,6);
                s.Tube(new[]{shoulder,elbow,wrist},new[]{.060f,.037f,.024f},new[]{.060f,.034f,.023f},Bone,12,1);Hand(s,wrist+Vector3.down*.03f,side,.57f,Bone);
                s.Tube(new[]{new Vector3(side*.085f,.33f,0),new Vector3(side*.087f,.08f,.005f)},new[]{.043f,.024f},new[]{.045f,.027f},Bone,12,1);Foot(s,new Vector3(side*.086f,.032f,.04f),.59f,Bone);
                // Short gathered sleeve, ragged rather than floating cuffs.
                s.Ellipsoid(shoulder,new Vector3(.083f,.097f,.089f),Skin,14,8,1);
                s.Tube(new[]{shoulder,Vector3.Lerp(shoulder,elbow,.45f)},new[]{.067f,.058f},new[]{.067f,.050f},Cloth,14,0,true);
            }
            var centre=new Vector3(.014f,1.095f,.028f);var rotation=Quaternion.Euler(-2,-4,9);
            Face(s,centre,rotation,new Vector3(.137f,.173f,.118f),true);
            HairCap(s,centre,rotation,new Vector3(.147f,.182f,.129f),.52f);
            // Uneven bob, individual clumped strands. Keep the eyes open to the viewer.
            for(int i=0;i<24;i++){
                float a=.76f+i*(Mathf.PI*2-1.52f)/23;var p=centre+rotation*new Vector3(Mathf.Sin(a)*.142f,.045f,Mathf.Cos(a)*.121f);
                s.Tube(new[]{p,p+new Vector3(Mathf.Sin(a)*.007f,-.12f,.012f),p+new Vector3(Mathf.Sin(a)*.012f,-.24f+.022f*Mathf.Sin(i*3),.025f)},new[]{.006f,.005f,.001f},new[]{.003f,.002f,.0005f},Hair,6);
            }
            // A few painted cracks; no emissive eyes or bright circular mask.
            for(int side=-1;side<=1;side+=2){
                Vector3 P(float x,float y,float z)=>centre+rotation*new Vector3(x,y,z);
                s.Tube(new[]{P(side*.058f,.055f,.112f),P(side*.072f,.021f,.121f),P(side*.060f,-.015f,.124f),P(side*.085f,-.056f,.107f)},new[]{.0025f,.002f,.0018f,.0005f},new[]{.002f,.002f,.002f,.001f},Hollow,4,2);
            }
            for(int i=0;i<3;i++)s.Ellipsoid(new Vector3(0,.73f-i*.085f,.105f),new Vector3(.010f,.012f,.005f),Damp,8,4);
            s.Tube(new[]{new Vector3(-.14f,.86f,.075f),new Vector3(-.065f,.82f,.116f),new Vector3(0,.80f,.124f),new Vector3(.065f,.82f,.116f),new Vector3(.14f,.86f,.075f)},new[]{.012f,.01f,.01f,.01f,.012f},new[]{.006f,.006f,.006f,.006f,.006f},Cloth*1.3f,6);
        }
        static void Gaunt(Sculpt s)
        {
            // An elongated salt-grey silhouette. No copied SCP model, insignia or character texture.
            s.Tube(new[]{new Vector3(0,.99f,-.015f),new Vector3(0,1.19f,-.06f),new Vector3(0,1.42f,-.06f),new Vector3(0,1.66f,0),new Vector3(0,1.84f,.045f)},new[]{.18f,.125f,.17f,.28f,.08f},new[]{.13f,.085f,.12f,.17f,.075f},Skin,24,1);
            for(int side=-1;side<=1;side+=2){
                var hip=new Vector3(side*.12f,1.05f,0);var knee=new Vector3(side*.16f,.58f,.09f);var ankle=new Vector3(side*.18f,.09f,.025f);
                s.Tube(new[]{hip,Vector3.Lerp(hip,knee,.45f),knee,Vector3.Lerp(knee,ankle,.45f),ankle},new[]{.087f,.066f,.050f,.052f,.028f},new[]{.095f,.073f,.054f,.060f,.034f},Skin,14,1);s.Ellipsoid(knee,new Vector3(.053f,.065f,.052f),Bone,10,6,1);Foot(s,new Vector3(side*.18f,.043f,.11f),1.05f,Skin);
                var shoulder=new Vector3(side*.255f,1.69f,0);var elbow=new Vector3(side*.40f,1.13f,.045f);var wrist=new Vector3(side*.43f,.54f,.18f);
                s.Ellipsoid(shoulder,new Vector3(.083f,.097f,.089f),Skin,14,8,1);
                s.Tube(new[]{shoulder,Vector3.Lerp(shoulder,elbow,.45f),elbow,Vector3.Lerp(elbow,wrist,.42f),wrist},new[]{.081f,.053f,.043f,.047f,.026f},new[]{.087f,.056f,.044f,.047f,.030f},Skin,14,1);s.Ellipsoid(elbow,new Vector3(.047f,.055f,.048f),Bone,10,6,1);Hand(s,wrist+Vector3.down*.055f,side,1.12f,Bone);
                // Ribs emerge subtly from the skin; tapered arcs, not stacked floating bars.
                for(int i=0;i<6;i++){
                    float y=1.61f-i*.055f,w=.22f-i*.013f;
                    s.Tube(new[]{new Vector3(side*.018f,y,.155f-i*.009f),new Vector3(side*w*.55f,y-.015f,.162f-i*.008f),new Vector3(side*w,y-.04f,.07f)},new[]{.008f,.012f,.004f},new[]{.006f,.008f,.004f},Bone*.87f,6,1);
                }
                s.Tube(new[]{new Vector3(side*.02f,1.77f,.12f),new Vector3(side*.13f,1.755f,.16f),new Vector3(side*.25f,1.70f,.07f)},new[]{.012f,.018f,.008f},new[]{.01f,.012f,.006f},Bone,8,1);
            }
            s.Tube(new[]{new Vector3(0,1.78f,.045f),new Vector3(0,1.98f,.10f)},new[]{.067f,.053f},new[]{.073f,.060f},Skin,14,1);
            var centre=new Vector3(.012f,2.085f,.14f);var rotation=Quaternion.Euler(7,0,-6);
            Face(s,centre,rotation,new Vector3(.118f,.20f,.119f),false,true);
            // Scant wet hair at the back; chin is long, eyes recessed and the mouth visibly open.
            for(int i=0;i<8;i++){
                float a=1.7f+i*.4f;var p=centre+new Vector3(Mathf.Sin(a)*.105f,.14f,Mathf.Cos(a)*.11f);
                s.Tube(new[]{p,p+new Vector3(.01f,-.12f,-.01f),p+new Vector3(.025f,-.30f,-.005f)},new[]{.004f,.003f,.0008f},new[]{.003f,.003f,.001f},Hair,5);
            }
            // Tattered waistband/short cloth protects the silhouette from looking like a mannequin.
            s.Tube(new[]{new Vector3(0,.91f,-.01f),new Vector3(0,1.09f,-.02f)},new[]{.185f,.175f},new[]{.14f,.13f},Damp,24,0,true);
        }
        static void Face(Sculpt s,Vector3 c,Quaternion q,Vector3 radius,bool doll,bool open=false)
        {
            Vector3 P(float x,float y,float z)=>c+q*new Vector3(x,y,z);
            var skin=doll?Bone:Skin;
            s.Head(c,radius,skin,q);
            // Jaw and cheek planes, sunken sockets, lids, non-glowing small eyes.
            for(int side=-1;side<=1;side+=2){
                float x=side*radius.x*.48f;float eye=radius.z*.88f;
                s.Ellipsoid(P(x,.025f,eye),new Vector3(doll?.028f:.027f,doll?.012f:.009f,.004f),skin*.25f,14,6,2,q,true);
                s.Ellipsoid(P(x,.022f,eye+.012f),new Vector3(.010f,.006f,.003f),Skin*.55f,12,6,1,q,true);
                s.Ellipsoid(P(x,.020f,eye+.017f),new Vector3(.004f,.004f,.001f),Hollow,8,4,2,q,true);
                s.Tube(new[]{P(side*.018f,.052f,eye),P(x,.061f,eye+.001f),P(side*radius.x*.78f,.047f,eye-.02f)},new[]{.008f,.012f,.004f},new[]{.006f,.008f,.003f},skin*.72f,8,1,trueHead:true);
                // Salt/dark runoff under each eye, flush against the cheek.
                s.Tube(new[]{P(x,-.008f,eye+.009f),P(x+side*.004f,-.05f,eye+.006f),P(x+side*.012f,-.081f,eye-.004f)},new[]{.004f,.003f,.0005f},new[]{.002f,.002f,.001f},Damp*.35f,5,2,trueHead:true);
            }
            s.Ellipsoid(P(.004f,-.018f,radius.z+.009f),new Vector3(.015f,.030f,.018f),skin*.88f,10,6,1,q,true);
            if(open){
                s.Ellipsoid(P(0,-.085f,radius.z*.86f),new Vector3(.046f,.074f,.017f),Hollow,18,10,2,q,true);
                s.Tube(new[]{P(-.045f,-.045f,radius.z*.90f),P(-.042f,-.125f,radius.z*.89f),P(0,-.165f,radius.z*.86f),P(.042f,-.125f,radius.z*.89f),P(.045f,-.045f,radius.z*.90f)},new[]{.006f,.007f,.009f,.007f,.006f},new[]{.004f,.004f,.006f,.004f,.004f},skin*.65f,8,1,trueHead:true);
            }else s.Ellipsoid(P(0,-.082f,radius.z*.92f),new Vector3(doll?.031f:.039f,doll?.012f:.009f,.006f),Hollow,14,5,2,q,true);
        }
        static void HairCap(Sculpt s,Vector3 c,Quaternion q,Vector3 size,float extent)
        {
            var grid=new Vector3[8,25];
            for(int r=0;r<8;r++)for(int j=0;j<=24;j++){
                float a=j*Mathf.PI/12,b=r/7f*Mathf.PI*extent;
                // The front hairline rises above the eyes; the back wraps behind the skull.
                float limit=Mathf.Cos(a)>.2f?.36f+.025f*Mathf.Sin(a*9):extent;b=Mathf.Min(b,Mathf.PI*limit);
                grid[r,j]=c+q*Vector3.Scale(size*(1+.035f*Mathf.Sin(a*13)),new Vector3(Mathf.Sin(a)*Mathf.Sin(b),Mathf.Cos(b),Mathf.Cos(a)*Mathf.Sin(b)));
            }
            s.Grid(grid,Hair,0,true,true);
        }
        static void Hand(Sculpt s,Vector3 palm,int side,float scale,Color colour)
        {
            s.Ellipsoid(palm,new Vector3(.044f,.068f,.027f)*scale,colour,12,6,1);
            for(int f=0;f<4;f++){
                float length=(.11f+(f==1||f==2?.025f:0))*scale;var p=palm+new Vector3((f-1.5f)*.021f,-.045f,.002f)*scale;
                s.Tube(new[]{p,p+new Vector3(side*.005f,-length*.52f,.010f),p+new Vector3(side*.009f,-length,.030f)},new[]{.009f*scale,.007f*scale,.002f*scale},new[]{.009f*scale,.007f*scale,.004f*scale},colour,7,1);
            }
            var thumb=palm+new Vector3(-side*.035f,.015f,.006f)*scale;
            s.Tube(new[]{thumb,thumb+new Vector3(-side*.031f,-.036f,.022f)*scale,thumb+new Vector3(-side*.026f,-.082f,.040f)*scale},new[]{.014f*scale,.011f*scale,.004f*scale},new[]{.013f*scale,.01f*scale,.004f*scale},colour,7,1);
        }
        static void Foot(Sculpt s,Vector3 p,float scale,Color colour){s.Ellipsoid(p,new Vector3(.052f,.044f,.13f)*scale,colour,12,6,1);for(int i=0;i<5;i++)s.Ellipsoid(p+new Vector3((i-2)*.020f,0,.103f)*scale,new Vector3(.012f,.021f,.040f)*scale,colour,7,4,1);}

        sealed class Sculpt
        {
            readonly List<Vector3> vertices=new();readonly List<Color> colours=new();readonly List<Vector2> uv=new();readonly List<int> triangles=new();
            public void Grid(Vector3[,] p,Color colour,float kind=0,bool head=false,bool reverse=false){int start=vertices.Count,rows=p.GetLength(0),cols=p.GetLength(1);for(int r=0;r<rows;r++)for(int j=0;j<cols;j++){vertices.Add(p[r,j]);colours.Add(colour);uv.Add(new Vector2(kind,head?1:0));}for(int r=0;r<rows-1;r++)for(int j=0;j<cols-1;j++){int a=start+r*cols+j,b=a+1,d=a+cols,e=d+1;if(reverse){triangles.AddRange(new[]{a,e,b,a,d,e});}else triangles.AddRange(new[]{a,b,e,a,e,d});}}
            public void Tube(Vector3[] c,float[] widths,float[] depths,Color colour,int sides=12,float kind=0,bool folds=false,bool trueHead=false){var p=new Vector3[c.Length,sides+1];for(int r=0;r<c.Length;r++){var along=(c[Mathf.Min(r+1,c.Length-1)]-c[Mathf.Max(0,r-1)]).normalized;var right=Vector3.Cross(along,Vector3.forward).normalized;if(right.sqrMagnitude<.01f)right=Vector3.right;var forward=Vector3.Cross(right,along).normalized;for(int j=0;j<=sides;j++){float a=j*Mathf.PI*2/sides,f=folds?1+.055f*Mathf.Cos(a*9+r*.25f):1;p[r,j]=c[r]+right*(Mathf.Sin(a)*widths[r]*f)+forward*(Mathf.Cos(a)*depths[r]*f);}}Grid(p,colour,kind,trueHead,false);}
            public void Ellipsoid(Vector3 c,Vector3 radius,Color colour,int sides=16,int rings=8,float kind=0,Quaternion q=default,bool head=false){if(q==default)q=Quaternion.identity;var p=new Vector3[rings+1,sides+1];for(int r=0;r<=rings;r++)for(int j=0;j<=sides;j++){float a=j*Mathf.PI*2/sides,b=r*Mathf.PI/rings;p[r,j]=c+q*Vector3.Scale(radius,new Vector3(Mathf.Sin(a)*Mathf.Sin(b),Mathf.Cos(b),Mathf.Cos(a)*Mathf.Sin(b)));}Grid(p,colour,kind,head,true);}
            public void Head(Vector3 c,Vector3 radius,Color skin,Quaternion q)
            {
                const int rows=22,cols=36;var p=new Vector3[rows+1,cols+1];
                for(int r=0;r<=rows;r++)for(int j=0;j<=cols;j++){
                    float a=j*Mathf.PI*2/cols,b=r*Mathf.PI/rows;float y=Mathf.Cos(b),x=Mathf.Sin(a)*Mathf.Sin(b),z=Mathf.Cos(a)*Mathf.Sin(b);
                    float jaw=y<-.25f?Mathf.Lerp(.68f,1,Mathf.InverseLerp(-1,-.25f,y)):1;
                    x*=jaw;
                    if(z>0){float sockets=Mathf.Exp(-Mathf.Pow((Mathf.Abs(x)-.46f)*6,2)-Mathf.Pow((y-.13f)*8,2));float cheeks=Mathf.Exp(-Mathf.Pow((Mathf.Abs(x)-.62f)*6,2)-Mathf.Pow((y+.22f)*7,2));float brow=Mathf.Exp(-Mathf.Pow((y-.30f)*12,2));z+=-.14f*sockets+.09f*cheeks+.06f*brow;}
                    p[r,j]=c+q*Vector3.Scale(radius,new Vector3(x,y,z));
                }Grid(p,skin,1,true,true);
            }
            public void PaintShoulderPatch()
            {
                for(int i=0;i<vertices.Count;i++){var p=vertices[i];if(p.x<-.14f&&p.x>-.30f&&p.y>1.50f&&p.y<1.78f&&p.z>.045f)colours[i]=new Color(.22f,.20f,.14f);}
            }
            public void RemoveCoveredSkin()
            {
                for(int i=triangles.Count-3;i>=0;i-=3){var c=(vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3;
                    if(c.y>.10f&&c.y<1.80f&&!(Mathf.Abs(c.x)>.32f&&c.y<.72f))triangles.RemoveRange(i,3);
                }
            }
            public Mesh Finish(string name){var m=new Mesh{name=name};m.SetVertices(vertices);m.SetColors(colours);m.SetUVs(0,uv);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
        }
    }
}
