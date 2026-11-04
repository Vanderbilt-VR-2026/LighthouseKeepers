using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace LighthouseKeepers.WatchDemo
{
    // Sixteen fixed quads per emitter. Lifetime, drift and soft edges are GPU-driven.
    // Water rendering is untouched. Depth is requested solely for soft intersections.
    public sealed class FaultVapour : MonoBehaviour
    {
        WatchDemoController watch;WatchPipeRepair pipe;Mesh mesh;
        MeshRenderer steam,smoke;Material steamMaterial,smokeMaterial;
        UniversalAdditionalCameraData cameraData;CameraOverrideOption oldDepth;
        public static void Create(WatchDemoController watch,WatchPipeRepair pipe,WatchGenerator generator)
        {
            var s=watch.gameObject.AddComponent<FaultVapour>();s.watch=watch;s.pipe=pipe;
            s.mesh=s.Quads();var shader=Resources.Load<Shader>("ImmersionVapour");
            if(!shader){Debug.LogError("Missing service vapour shader");s.enabled=false;return;}
            s.steamMaterial=new Material(shader);s.steamMaterial.SetColor("_Tint",new Color(.72f,.78f,.8f,.14f));
            s.steamMaterial.SetFloat("_Rise",.45f);s.steamMaterial.SetFloat("_Size",.13f);s.steamMaterial.SetFloat("_Rate",.85f);
            s.smokeMaterial=new Material(shader);s.smokeMaterial.SetColor("_Tint",new Color(.24f,.25f,.26f,.10f));
            s.smokeMaterial.SetFloat("_Rise",.65f);s.smokeMaterial.SetFloat("_Size",.10f);s.smokeMaterial.SetFloat("_Rate",.5f);
            s.steam=s.Emitter("GPU leak steam",pipe.transform,s.steamMaterial);
            Transform exhaust=null;foreach(var t in generator.GetComponentsInChildren<Transform>())if(t.name.ToLowerInvariant().Contains("exhaust")){exhaust=t;break;}
            s.smoke=s.Emitter("GPU broken-engine smoke",watch.transform,s.smokeMaterial);
            var exhaustRenderer=exhaust?exhaust.GetComponent<Renderer>():null;
            s.smoke.transform.position=exhaustRenderer?new Vector3(exhaustRenderer.bounds.center.x,exhaustRenderer.bounds.max.y+.02f,exhaustRenderer.bounds.center.z):generator.transform.TransformPoint(new Vector3(.35f,1.5f,0));
            if(Camera.main){s.cameraData=Camera.main.GetUniversalAdditionalCameraData();if(s.cameraData){s.oldDepth=s.cameraData.requiresDepthOption;s.cameraData.requiresDepthOption=CameraOverrideOption.On;}}
        }
        MeshRenderer Emitter(string name,Transform parent,Material material)
        {
            var t=M2StationParts.Root(name,parent,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;return r;
        }
        Mesh Quads()
        {
            var v=new Vector3[64];var uv=new System.Collections.Generic.List<Vector4>();var tris=new int[96];
            for(int i=0;i<16;i++)
            {for(int k=0;k<4;k++)uv.Add(new Vector4(k==0||k==3?-1:1,k<2?-1:1,(i+.5f)/16,0));int a=i*4,b=i*6;tris[b]=a;tris[b+1]=a+1;tris[b+2]=a+2;tris[b+3]=a;tris[b+4]=a+2;tris[b+5]=a+3;}
            var m=new Mesh{name="16 GPU vapour quads"};m.vertices=v;m.SetUVs(0,uv);m.triangles=tris;m.bounds=new Bounds(Vector3.up*.4f,Vector3.one*2);return m;
        }
        void Update()
        {
            var s=watch.State;bool leaking=!s.Repaired&&!s.Failed,broken=s.Engine!=null?!s.Engine.Running:s.Power==PowerState.Off;
            steam.enabled=leaking;smoke.enabled=broken&&!s.Failed;
            if(leaking){steam.transform.position=pipe.ActivePosition+Vector3.left*.065f;steamMaterial.SetFloat("_Strength",s.Pipe.InflowFraction);}
        }
        void OnDestroy(){if(cameraData)cameraData.requiresDepthOption=oldDepth;if(mesh)Destroy(mesh);if(steamMaterial)Destroy(steamMaterial);if(smokeMaterial)Destroy(smokeMaterial);}
    }
}
