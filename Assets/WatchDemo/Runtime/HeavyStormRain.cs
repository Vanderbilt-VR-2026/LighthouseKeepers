using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.WatchDemo
{
    // Fixed geometry animated by the vertex shader. No simulated drops, collisions, depth copies,
    // reflection cameras or realtime lights. Existing head-zone audio remains authoritative.
    public sealed class HeavyStormRain:MonoBehaviour
    {
        public const int StreakCount=1600,MaximumPuddles=26;
        WatchDemoController watch;Transform head,field;Mesh rainMesh,puddleMesh;Material rainMaterial,puddleMaterial;
        readonly List<ParticleSystem> replaced=new();MaterialPropertyBlock block;float strength,quiet=1;
        public int PuddleCount{get;private set;}
        public int GalleryPuddleCount{get;private set;}
        public int TriangleCount=>(rainMesh?rainMesh.triangles.Length/3:0)+(puddleMesh?puddleMesh.triangles.Length/3:0);
        public static void Create(WatchDemoController w){if(!w.State.Sprint5||!w.State.Sprint5.heavyRain)return;var r=w.gameObject.AddComponent<HeavyStormRain>();r.watch=w;}
        void Start(){
            head=Camera.main.transform;block=new MaterialPropertyBlock();
            foreach(var p in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))if(p.name.ToLowerInvariant().Contains("rain")){replaced.Add(p);p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);p.GetComponent<ParticleSystemRenderer>().enabled=false;}
            var shell=FindAnyObjectByType<ShellWeatherPresentation>();if(shell)shell.ExternalRainVisuals=true;
            rainMesh=BuildStreaks();rainMaterial=new Material(Resources.Load<Shader>("S5Rain")){name="Heavy storm rain — vertex animated"};
            field=Render("Bounded exterior rain",rainMesh,rainMaterial).transform;
            puddleMesh=BuildPuddles();puddleMaterial=new Material(Resources.Load<Shader>("S5Puddles")){name="Rain-filled rock hollows"};Render("Outdoor puddles — grounded ripples",puddleMesh,puddleMaterial);
        }
        GameObject Render(string name,Mesh mesh,Material material){var go=new GameObject(name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;return go;}
        public static Mesh BuildStreaks(){var v=new Vector3[StreakCount*4];var uv=new Vector2[v.Length];var seed=new Vector2[v.Length];var t=new int[StreakCount*6];var random=new System.Random(7519);
            for(int i=0;i<StreakCount;i++){var p=new Vector3((float)random.NextDouble()*24,(float)random.NextDouble()*18,(float)random.NextDouble()*24);float r=(float)random.NextDouble();for(int j=0;j<4;j++){v[i*4+j]=p;uv[i*4+j]=new Vector2(j==0||j==3?-1:1,j<2?-1:1);seed[i*4+j]=new Vector2(r,(float)i/StreakCount);}int k=i*4,q=i*6;t[q]=k;t[q+1]=k+1;t[q+2]=k+2;t[q+3]=k;t[q+4]=k+2;t[q+5]=k+3;}
            var mesh=new Mesh{name="1600 bounded GPU rain streaks"};mesh.vertices=v;mesh.uv=uv;mesh.uv2=seed;mesh.triangles=t;mesh.bounds=new Bounds(Vector3.zero,Vector3.one*40);return mesh;
        }
        static bool Ground(Vector3 p,out Vector3 hit){hit=default;float highest=p.y-1;bool found=false;
            foreach(var h in Physics.RaycastAll(p+Vector3.up*2,Vector3.down,6,~0,QueryTriggerInteraction.Ignore)){
                if(h.normal.y<.93f||h.point.y>p.y+.15f||h.point.y<p.y-.3f||h.collider.GetComponentInParent<Rigidbody>())continue;
                if(h.point.y>highest){highest=h.point.y;hit=h.point;found=true;}
            }return found;
        }
        Mesh BuildPuddles(){var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
            var candidates=new List<Vector3>{new Vector3(0,0,-11),new Vector3(-1.6f,0,-11.5f),new Vector3(1.7f,0,-11.2f),new Vector3(0,0,-13.2f),new Vector3(-4.6f,0,-8),new Vector3(4.7f,0,-8.5f),new Vector3(4.8f,0,-5.5f),new Vector3(-4.7f,0,-5),new Vector3(5.1f,0,-2),new Vector3(-5.1f,0,-1.4f),new Vector3(4.9f,0,2),new Vector3(-4.9f,0,2),new Vector3(2,0,5.2f),new Vector3(-1.5f,0,5.6f),new Vector3(0,0,6.6f),new Vector3(5.5f,0,-9.3f),new Vector3(-4.8f,0,-10),new Vector3(2.8f,0,-12)};
            for(int n=0;n<8;n++){float angle=(n*45+20)*Mathf.Deg2Rad;candidates.Add(new Vector3(Mathf.Sin(angle)*3.65f,9.9f,Mathf.Cos(angle)*3.65f));}
            for(int n=0;n<candidates.Count;n++){
                if(!Ground(candidates[n],out var centre)||ShellWeatherPresentation.Sheltered(centre+Vector3.up))continue;
                var ring=new Vector3[24];bool valid=true;float size=n>=18?.34f:n<4?.72f:.48f;
                for(int j=0;j<24;j++){float a=j*Mathf.PI/12;float radius=size*(1+.12f*Mathf.Sin(a*3+n)+.06f*Mathf.Cos(a*7));var p=centre+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius*.65f);
                    if(!Ground(p,out ring[j])||Mathf.Abs(ring[j].y-centre.y)>.12f){valid=false;break;}ring[j].y+=centre.y>9?.025f:.009f;
                }
                if(!valid)continue;int k=v.Count;v.Add(centre+Vector3.up*(centre.y>9?.025f:.009f));uv.Add(Vector2.zero);for(int j=0;j<24;j++){v.Add(ring[j]);float a=j*Mathf.PI/12;uv.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a)));}
                for(int j=0;j<24;j++){t.Add(k);t.Add(k+1+(j+1)%24);t.Add(k+1+j);}PuddleCount++;if(centre.y>9)GalleryPuddleCount++;
            }
            var mesh=new Mesh{name="Combined supported exterior puddles"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void LateUpdate(){if(!head||!field)return;field.position=head.position;
            var s=watch.State;float desired=(s.Storm.Stage-1)/3f;strength=Mathf.MoveTowards(strength,desired,Time.deltaTime*.10f);
            rainMaterial.SetVector("_RainHead",head.position);rainMaterial.SetFloat("_Storm",strength);quiet=Mathf.MoveTowards(quiet,s.TensionQuiet?.55f:1,Time.deltaTime*.4f);rainMaterial.SetFloat("_Quiet",quiet);
            puddleMaterial.SetFloat("_Storm",strength);
        }
        void OnDestroy(){var shell=FindAnyObjectByType<ShellWeatherPresentation>();if(shell)shell.ExternalRainVisuals=false;foreach(var p in replaced)if(p){p.GetComponent<ParticleSystemRenderer>().enabled=true;p.Play();}foreach(var o in new Object[]{rainMesh,puddleMesh,rainMaterial,puddleMaterial})if(o)Destroy(o);}
    }
}
