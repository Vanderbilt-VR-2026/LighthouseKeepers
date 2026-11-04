using System.Collections.Generic;
using UnityEngine;

namespace LighthouseKeepers.WatchDemo
{
    // Shared runtime authoring: applies to the restored scene and fresh WatchDemoBuilder builds.
    // East wall is opposite the west stair arc and leaves the House doorway/centre clear.
    public sealed class PipeWallLayout : MonoBehaviour
    {
        public const float WallX = 3.05f, BoltHeight = 1.02f;
        readonly List<Mesh> owned = new();
        public static void Apply(WatchPipeRepair pipe)
        {
            if (pipe.GetComponent<PipeWallLayout>()) return;
            var layout = pipe.gameObject.AddComponent<PipeWallLayout>();
            var parts = pipe.gameObject.AddComponent<M2StationParts>(); parts.Initialize();
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if (t.name == "Single service pipe" || t.name == "Pipe support" || t.name == "Support foot" ||
                    t.name == "Leaking centre coupling" || t.name.StartsWith("Pipe branch ")) t.gameObject.SetActive(false);
            var barrel = parts.Part("East wall service pipe",pipe.transform,new Vector3(WallX,.90f,0),new Vector3(.20f,1.45f,.20f),parts.Metal,PrimitiveType.Cylinder);
            barrel.transform.rotation = Quaternion.Euler(90,0,0);
            var collider = barrel.AddComponent<CapsuleCollider>(); collider.direction = 1; collider.height = 2; collider.radius = .5f;
            for (int i=0;i<3;i++)
            {
                var bolt = pipe.bolts[i];
                bolt.SetPositionAndRotation(new Vector3(WallX,BoltHeight,(i-1)*.88f),Quaternion.LookRotation(Vector3.up,Vector3.left));
                var head = bolt.Find("Ratchet bolt");
                head.localScale=Vector3.one;head.localRotation=Quaternion.identity;head.localPosition=new Vector3(0,0,.025f);
                head.GetComponent<MeshFilter>().sharedMesh=layout.Lathe("Hex nut",6,new[]{new Vector2(.014f,-.015f),new Vector2(.029f,-.015f),new Vector2(.034f,-.010f),new Vector2(.034f,.010f),new Vector2(.029f,.015f),new Vector2(.014f,.015f)});
                head.GetComponent<Renderer>().sharedMaterial=parts.Metal;
                var washer=M2StationParts.Root("Rust-edged washer",bolt,new Vector3(0,0,.004f));
                layout.MeshPart(washer,layout.Lathe("Washer",8,new[]{new Vector2(.014f,-.003f),new Vector2(.046f,-.003f),new Vector2(.046f,.003f),new Vector2(.014f,.003f)}),parts.Brass);
                var stud=M2StationParts.Root("Threaded stud",bolt,Vector3.zero);
                var profile=new List<Vector2>{new Vector2(0,-.02f),new Vector2(.011f,-.02f)};
                for(int j=0;j<4;j++){profile.Add(new Vector2(.014f,.012f+j*.008f));profile.Add(new Vector2(.010f,.016f+j*.008f));}
                profile.Add(new Vector2(.010f,.048f));profile.Add(new Vector2(0,.048f));
                layout.MeshPart(stud,layout.Lathe("Stud threads",6,profile.ToArray()),parts.Metal);
                var bracket=parts.Part("Pipe wall bracket "+i,pipe.transform,new Vector3(3.35f,.83f,(i-1)*.88f),new Vector3(.6f,.045f,.10f),parts.Dark);
                parts.Part("Bracket wall plate "+i,pipe.transform,new Vector3(3.62f,.90f,(i-1)*.88f),new Vector3(.035f,.32f,.18f),parts.Metal);
                pipe.sprays[i].transform.SetPositionAndRotation(bolt.position+Vector3.left*.06f,Quaternion.LookRotation(new Vector3(-1,.4f,0)));
            }
            // A red valve wheel at either end ties the run into the room's plumbing.
            for(int end=-1;end<=1;end+=2)
            {
                var wheel=M2StationParts.Root("Red isolation valve",pipe.transform,new Vector3(WallX-.14f,.9f,end*1.3f));
                wheel.rotation=Quaternion.LookRotation(Vector3.left);
                layout.MeshPart(wheel,layout.Lathe("Valve rim",16,new[]{new Vector2(.105f,-.015f),new Vector2(.13f,-.015f),new Vector2(.13f,.015f),new Vector2(.105f,.015f)}),parts.Red);
                for(int spoke=0;spoke<3;spoke++)
                {var o=parts.Part("Valve spoke",wheel,Vector3.zero,new Vector3(.018f,.23f,.018f),parts.Red);o.transform.localRotation=Quaternion.Euler(0,0,spoke*60);}
            }
            var anchor=GameObject.Find("Repair point");if(anchor)anchor.transform.position=pipe.bolts[1].position;
            pipe.leakAudio.transform.position=pipe.bolts[1].position;
        }
        void MeshPart(Transform t,Mesh mesh,Material material)
        {t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;}
        Mesh Lathe(string name,int sides,Vector2[] profile)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int p=0;p<profile.Length;p++)
            {
                int next=(p+1)%profile.Length;
                for(int i=0;i<sides;i++)
                {
                    float a=2*Mathf.PI*i/sides,b=2*Mathf.PI*(i+1)/sides;int n=vertices.Count;
                    vertices.Add(new Vector3(Mathf.Cos(a)*profile[p].x,Mathf.Sin(a)*profile[p].x,profile[p].y));
                    vertices.Add(new Vector3(Mathf.Cos(b)*profile[p].x,Mathf.Sin(b)*profile[p].x,profile[p].y));
                    vertices.Add(new Vector3(Mathf.Cos(b)*profile[next].x,Mathf.Sin(b)*profile[next].x,profile[next].y));
                    vertices.Add(new Vector3(Mathf.Cos(a)*profile[next].x,Mathf.Sin(a)*profile[next].x,profile[next].y));
                    triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
                }
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();owned.Add(mesh);return mesh;
        }
        void OnDestroy(){foreach(var mesh in owned)if(mesh)Destroy(mesh);}
    }
}
