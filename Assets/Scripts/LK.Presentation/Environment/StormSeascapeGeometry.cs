using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.Presentation.Environment
{
    // Pure geometry for the exterior seascape: radial ocean grid and one combined distant coast.
    // Shore/calm attenuation mirrors StormOceanWaves.shader so tests can check what the GPU will do.
    public static class StormSeascapeGeometry
    {
        public const int OceanRings=84,OceanSegments=128;
        public const float OceanInnerRadius=2.5f,OceanOuterRadius=320;
        public const int OceanVertexBudget=15000,CoastVertexBudget=2000;
        // Island footprint at the waterline (SM_Rock_Island_LOD0 bounds, Unity metres, XZ).
        public static readonly Vector2 IslandCentre=new(-.2f,-3.2f),IslandRadii=new(7.8f,13.7f);
        public const float ShoreFalloff=1.6f,ShoreAmplitude=.06f,CalmStart=80,CalmEnd=240,CalmAmplitude=.4f;
        // Radial spacing is RingRatio*radius. The shader fades a wave out when it is under-sampled.
        public static float RingRatio=>Mathf.Pow(OceanOuterRadius/OceanInnerRadius,1f/(OceanRings-1))-1;
        // Coast spans these azimuths (degrees clockwise from +Z toward +X); the inlet is centred on InletAzimuth.
        public const float CoastFrom=18,CoastTo=128,InletAzimuth=76;

        public static float WaveAttenuation(Vector2 xz)
        {
            float e=((xz-IslandCentre)/IslandRadii).magnitude;
            float shore=Mathf.Lerp(ShoreAmplitude,1,Smooth(1,1+ShoreFalloff,e));
            float calm=Mathf.Lerp(1,CalmAmplitude,Smooth(CalmStart,CalmEnd,xz.magnitude));
            return shore*calm;
        }
        static float Smooth(float a,float b,float x){float t=Mathf.Clamp01((x-a)/(b-a));return t*t*(3-2*t);}

        public static Mesh BuildOcean(bool gpuOnly=true)
        {
            var v=new Vector3[1+OceanRings*OceanSegments];var t=new int[(OceanSegments+(OceanRings-1)*OceanSegments*2)*3];
            float q=1+RingRatio;int k=0;v[0]=Vector3.zero;
            for(int r=0;r<OceanRings;r++)
            {
                float radius=OceanInnerRadius*Mathf.Pow(q,r);float offset=(r&1)*.5f; // stagger alternate rings for even triangles
                for(int s=0;s<OceanSegments;s++){float a=(s+offset)*Mathf.PI*2/OceanSegments;v[1+r*OceanSegments+s]=new Vector3(Mathf.Sin(a)*radius,0,Mathf.Cos(a)*radius);}
            }
            for(int s=0;s<OceanSegments;s++){t[k++]=0;t[k++]=1+s;t[k++]=1+(s+1)%OceanSegments;}
            for(int r=0;r<OceanRings-1;r++)for(int s=0;s<OceanSegments;s++)
            {
                int a=1+r*OceanSegments+s,b=1+r*OceanSegments+(s+1)%OceanSegments,c=a+OceanSegments,d=b+OceanSegments;
                t[k++]=a;t[k++]=c;t[k++]=d;t[k++]=a;t[k++]=d;t[k++]=b;
            }
            var mesh=new Mesh{name="LK_StormOceanRadialGrid",vertices=v,triangles=t};
            var n=new Vector3[v.Length];for(int i=0;i<n.Length;i++)n[i]=Vector3.up;mesh.normals=n;
            // Vertical margin covers the largest storm crest; horizontal covers Gerstner drift.
            mesh.bounds=new Bounds(Vector3.zero,new Vector3(OceanOuterRadius*2+10,8,OceanOuterRadius*2+10));
            mesh.UploadMeshData(gpuOnly);return mesh;
        }

        static Vector3 Polar(float azimuth,float radius,float y){float a=azimuth*Mathf.Deg2Rad;return new Vector3(Mathf.Sin(a)*radius,y,Mathf.Cos(a)*radius);}
        // 0 at the inlet axis, 1 a little outside its mouth.
        public static float InletBlend(float azimuth){float b=Mathf.Clamp01(1-Mathf.Abs(azimuth-InletAzimuth)/11f);return b*b*(3-2*b);}
        public static float CoastRadius(float azimuth)
        {
            float t=(azimuth-CoastFrom)/(CoastTo-CoastFrom);
            return 190+22*Mathf.Sin(t*7.3f)+10*Mathf.Sin(t*19+1.3f)+InletBlend(azimuth)*88;
        }

        // Vertex colour: rgb is a shade multiplier for rock (a=0) or emitted colour for a distant light (a=1).
        // uv.x carries a per-light twinkle phase.
        public static Mesh BuildCoast(bool gpuOnly=true)
        {
            var v=new List<Vector3>();var c=new List<Color>();var uv=new List<Vector2>();var t=new List<int>();
            const int samples=72;const float sea=-6;
            // Near headlands: cliff foot, cliff lip, crest, back slope.
            int nearStart=v.Count;
            for(int i=0;i<=samples;i++)
            {
                float az=Mathf.Lerp(CoastFrom,CoastTo,i/(float)samples),u=i/(float)samples;
                float inlet=InletBlend(az),r=CoastRadius(az);
                float taper=Smooth(0,.1f,u)*Smooth(1,.9f,u);
                // Taller headlands flank the inlet mouth.
                float mouth=Mathf.Exp(-Mathf.Pow((Mathf.Abs(az-InletAzimuth)-14)/6,2));
                float h=(14+9*Mathf.Sin(u*11+.4f)+5*Mathf.Sin(u*29)+22*mouth)*(1-.85f*inlet)*taper+1.5f;
                float shade=.8f+.2f*Mathf.Sin(u*37);
                v.Add(Polar(az,r,sea));v.Add(Polar(az,r+2.5f+h*.08f,h*.8f));v.Add(Polar(az,r+9,h));v.Add(Polar(az,r+26,h*.55f));
                c.Add(new Color(.55f*shade,.55f*shade,.6f*shade,0));c.Add(new Color(.8f*shade,.8f*shade,.85f*shade,0));c.Add(new Color(shade,shade,shade,0));c.Add(new Color(.9f,.9f,.9f,0));
                for(int j=0;j<4;j++)uv.Add(Vector2.zero);
            }
            Strip(t,nearStart,samples,4);
            // Far hills behind, a hazier layer that shows over low ground and the inlet.
            int farStart=v.Count;
            for(int i=0;i<=samples;i++)
            {
                float az=Mathf.Lerp(CoastFrom-6,CoastTo+4,i/(float)samples),u=i/(float)samples;
                float h=(34+16*Mathf.Sin(u*6.1f+1)+8*Mathf.Sin(u*17+.2f))*Smooth(0,.2f,u)*Smooth(1,.75f,u);
                v.Add(Polar(az,300,sea));v.Add(Polar(az,302,h));c.Add(new Color(.7f,.7f,.75f,0));c.Add(new Color(.95f,.95f,1,0));uv.Add(new Vector2(0,1));uv.Add(new Vector2(0,1));
            }
            Strip(t,farStart,samples,2);
            // Faint harbour lights at the head of the inlet and a few scattered on the headlands.
            var rng=new System.Random(7);
            for(int i=0;i<16;i++)
            {
                bool harbour=i<11;float az=harbour?InletAzimuth+(float)(rng.NextDouble()*2-1)*7:(i%2==0?InletAzimuth-24:InletAzimuth+20)+(float)(rng.NextDouble()*2-1)*6;
                float r=harbour?CoastRadius(az)+4+(float)rng.NextDouble()*6:CoastRadius(az)+14;
                float y=harbour?1+(float)rng.NextDouble()*5:12+(float)rng.NextDouble()*8;
                float size=harbour?1.6f+(float)rng.NextDouble()*.8f:1.3f; // ~0.35-0.5 deg: a few pixels per eye on Quest
                var colour=i%5==3?new Color(.55f,.65f,.75f,1):new Color(1,.62f,.28f,1);
                Quad(v,c,uv,t,Polar(az,r,y),size,colour*(harbour?1:.7f),(float)rng.NextDouble()*6.28f);
            }
            var mesh=new Mesh{name="LK_DistantCoast"};mesh.SetVertices(v);mesh.SetColors(c);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);
            mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.UploadMeshData(gpuOnly);return mesh;
        }
        static void Strip(List<int> t,int start,int samples,int rows)
        {
            for(int i=0;i<samples;i++)for(int j=0;j<rows-1;j++)
            {int a=start+i*rows+j,b=a+rows;t.Add(a);t.Add(a+1);t.Add(b+1);t.Add(a);t.Add(b+1);t.Add(b);}
        }
        static void Quad(List<Vector3> v,List<Color> c,List<Vector2> uv,List<int> t,Vector3 p,float size,Color colour,float phase)
        {
            // Faces the lighthouse; the viewer is always within a few metres of the tower axis.
            var toward=new Vector3(-p.x,0,-p.z).normalized;var right=Vector3.Cross(Vector3.up,toward)*size*.5f;var up=Vector3.up*size*.5f;int s=v.Count;
            v.Add(p-right-up);v.Add(p-right+up);v.Add(p+right+up);v.Add(p+right-up);
            for(int i=0;i<4;i++){c.Add(colour);uv.Add(new Vector2(phase,0));}
            t.Add(s);t.Add(s+1);t.Add(s+2);t.Add(s);t.Add(s+2);t.Add(s+3);
        }
    }
}
