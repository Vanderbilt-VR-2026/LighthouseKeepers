using System.Collections.Generic;
using UnityEngine;

namespace LighthouseKeepers.WatchDemo
{
    // Shared runtime authoring: applies to the restored scene and fresh WatchDemoBuilder builds.
    // East wall is opposite the west stair arc and leaves the House doorway/centre clear.
    public sealed class PipeWallLayout : MonoBehaviour
    {
        public const float BoltHeight = 1.02f;
        public const float WallInset = .13f;
        readonly List<Mesh> owned = new();
        public static void Apply(WatchPipeRepair pipe)
        {
            if (pipe.GetComponent<PipeWallLayout>()) return;
            var layout = pipe.gameObject.AddComponent<PipeWallLayout>();
            var parts = pipe.gameObject.AddComponent<M2StationParts>(); parts.Initialize();
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if (t.name == "Single service pipe" || t.name == "Pipe support" || t.name == "Support foot" ||
                    t.name == "Leaking centre coupling" || t.name.StartsWith("Pipe branch ")) t.gameObject.SetActive(false);
            Physics.SyncTransforms();
            var centres=new Vector3[5];float[] positions={-1.3f,-.88f,0,.88f,1.3f};
            for(int i=0;i<centres.Length;i++)
            {var wall=WallSurface(positions[i],out var normal);centres[i]=wall+normal*WallInset;centres[i].y=.90f;}
            for(int i=0;i<centres.Length-1;i++)
            {
                var delta=centres[i+1]-centres[i];
                var barrel=parts.Part("Wall-following service pipe "+i,pipe.transform,(centres[i]+centres[i+1])*.5f,new Vector3(.20f,delta.magnitude*.5f,.20f),parts.Metal,PrimitiveType.Cylinder);
                barrel.transform.rotation=Quaternion.FromToRotation(Vector3.up,delta.normalized);
                var collider=barrel.AddComponent<CapsuleCollider>();collider.direction=1;collider.height=2;collider.radius=.5f;
            }
            for (int i=0;i<3;i++)
            {
                var bolt = pipe.bolts[i];
                var wall=WallSurface((i-1)*.88f,out var normal);
                var centre=wall+normal*WallInset;centre.y=BoltHeight;
                bolt.SetPositionAndRotation(centre,Quaternion.LookRotation(Vector3.up,normal));
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
                var bracket=parts.Part("Pipe wall bracket "+i,pipe.transform,new Vector3((wall.x+centre.x)*.5f,.83f,(wall.z+centre.z)*.5f),new Vector3(.10f,.045f,WallInset),parts.Dark);
                bracket.transform.rotation=Quaternion.LookRotation(normal);
                var plate=parts.Part("Bracket wall plate "+i,pipe.transform,new Vector3(wall.x,.90f,wall.z)+normal*.013f,new Vector3(.18f,.32f,.025f),parts.Metal);
                plate.transform.rotation=Quaternion.LookRotation(normal);
                pipe.sprays[i].transform.SetPositionAndRotation(bolt.position+Vector3.left*.06f,Quaternion.LookRotation(new Vector3(-1,.4f,0)));
            }
            // A red valve wheel at either end ties the run into the room's plumbing.
            for(int end=-1;end<=1;end+=2)
            {
                var wall=WallSurface(end*1.3f,out var normal);
                var wheel=M2StationParts.Root("Red isolation valve",pipe.transform,wall+normal*(WallInset+.14f));
                wheel.rotation=Quaternion.LookRotation(normal);
                layout.MeshPart(wheel,layout.Lathe("Valve rim",16,new[]{new Vector2(.105f,-.015f),new Vector2(.13f,-.015f),new Vector2(.13f,.015f),new Vector2(.105f,.015f)}),parts.Red);
                for(int spoke=0;spoke<3;spoke++)
                {var o=parts.Part("Valve spoke",wheel,Vector3.zero,new Vector3(.018f,.23f,.018f),parts.Red);o.transform.localRotation=Quaternion.Euler(0,0,spoke*60);}
            }
            var anchor=GameObject.Find("Repair point");if(anchor)anchor.transform.position=pipe.bolts[1].position;
            pipe.leakAudio.transform.position=pipe.bolts[1].position;
        }
        public static Vector3 WallSurface(float z,out Vector3 normal)
        {
            normal=Vector3.left;float closest=float.PositiveInfinity;Vector3 point=Vector3.zero;
            foreach(var hit in Physics.RaycastAll(new Vector3(1.5f,.90f,z),Vector3.right,4,~0,QueryTriggerInteraction.Ignore))
                if(hit.collider.name.StartsWith("UCX_SM_Wall_F1_")&&hit.distance<closest)
                {closest=hit.distance;point=hit.point;normal=hit.normal;}
            if(float.IsPositiveInfinity(closest))
            {
                Debug.LogError("F1 shell wall collider missing at z="+z);
                point=new Vector3(Mathf.Sqrt(3.9f*3.9f-z*z),.90f,z);normal=-new Vector3(point.x,0,z).normalized;
            }
            return point;
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
