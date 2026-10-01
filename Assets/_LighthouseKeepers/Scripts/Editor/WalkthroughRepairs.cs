using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LighthouseKeepers.Editor.LighthouseAssets;

namespace LighthouseKeepers.Editor
{
    public static class WalkthroughRepairs
    {
        const string Marker = "LK Walkthrough Repair September 29";
        static Vector3 Polar(float r,float degrees,float y) => new Vector3(r*Mathf.Cos(degrees*Mathf.Deg2Rad),y,r*Mathf.Sin(degrees*Mathf.Deg2Rad));
        // Preserve tread tops/materials while closing the underside of authored surfaces.
        static Mesh Thicken(Mesh source, float depth)
        {
            var vertices=new System.Collections.Generic.List<Vector3>();
            var triangles=new System.Collections.Generic.List<int>();
            var original=source.vertices;var indices=source.triangles;
            void Face(Vector3 a,Vector3 b,Vector3 c){int n=vertices.Count;vertices.AddRange(new[]{a,b,c});triangles.AddRange(new[]{n,n+1,n+2});}
            var down=Vector3.up*depth;
            for(int i=0;i<indices.Length;i+=3)
            {
                var a=original[indices[i]];var b=original[indices[i+1]];var c=original[indices[i+2]];
                Face(a,b,c);Face(a-down,c-down,b-down);
                foreach(var edge in new[]{(a,b),(b,c),(c,a)})
                {Face(edge.Item1,edge.Item1-down,edge.Item2-down);Face(edge.Item1,edge.Item2-down,edge.Item2);}
            }
            var mesh=new Mesh{name=source.name+"_Enclosed"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);
            mesh.uv=vertices.Select(v=>new Vector2(v.x,v.z)).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
            Unwrapping.GenerateSecondaryUVSet(mesh);return SaveMesh(mesh,mesh.name);
        }
        public static void RefreshClosedSurfaces()
        {
            foreach(var name in new[]{"LK_Core","LK_Level01_Plumbing"})
            {
                var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
                foreach(var f in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.sharedMesh&&f.sharedMesh.name.EndsWith("_Enclosed")))
                {
                    var path=AssetDatabase.GetAssetPath(f.sharedMesh).Replace("_Enclosed.asset",".asset");
                    f.sharedMesh=Thicken(AssetDatabase.LoadAssetAtPath<Mesh>(path),f.name=="Shutoff valve handwheel"?.035f:.18f);
                    if(f.TryGetComponent<MeshCollider>(out var c))c.sharedMesh=f.sharedMesh;
                }
                LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();BakeAndPreview();
        }
        public static void RefineAfterHeadset()
        {
            var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Level01_Plumbing"));
            if(!GameObject.Find("Compact player facing manifold"))
            {
                var parent=new GameObject("Compact player facing manifold").transform;
                parent.position=new Vector3(0,0,-3.45f);
                string[] names={"Corroded pressure main","Damaged split pipe left","Damaged split pipe right",
                    "Analog pressure dial","Gauge needle","Gauge neck","Gauge pressure takeoff","Shutoff valve handwheel",
                    "Valve spindle","Red valve wheel spoke","Valve manifold branch","Valve manifold return",
                    "Pressure main floor riser","Bolted floor pipe collar","Pressure main flange","Repair run floor connection",
                    "Repair run base flange","Repair pipe pedestal","Pedestal floor fixing","Active pipe leak"};
                foreach(var root in scene.GetRootGameObjects())
                {
                    if(!names.Contains(root.name)&&!root.name.StartsWith("Plumbing Repair -")&&root.name!="Pressure Sequence - Lower gauges")continue;
                    if(root.name=="Shutoff valve handwheel")root.GetComponent<MeshFilter>().sharedMesh=Thicken(root.GetComponent<MeshFilter>().sharedMesh,.035f);
                    root.transform.SetParent(parent,true);
                }
                // Keep the floor-fed assembly and its puzzle anchors together.
                // Rotate around the floor pivot: previously the gauges faced the wall and the mains hid the wheels.
                parent.rotation=Quaternion.Euler(0,180,0);
                parent.localScale=Vector3.one*.75f;
                LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
            }
            scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Core"));
            if(!GameObject.Find("LK Solid Floors And Seated Rails"))
            {
                foreach(var filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
                    if(filter.name.Contains("annular floor")||filter.name.StartsWith("Supported stair arrival landing"))
                    {
                        filter.sharedMesh=Thicken(filter.sharedMesh,.18f);
                        if(filter.TryGetComponent<MeshCollider>(out var c))c.sharedMesh=filter.sharedMesh;
                    }
                foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                    if(t.name=="Spiral railing post"||t.name=="Spiral handrail")t.gameObject.SetActive(false);
                for(int floor=0;floor<3;floor++)for(int step=0;step<32;step++)
                {
                    float a=step*11.25f,y=floor*3.2f+step*.1f+.012f;
                    Pipe("Continuous stair handrail",Polar(1.1f,a,y+1.05f),Polar(1.1f,a+11.25f,y+1.15f),.05f,"Iron");
                    if(step%2!=0)continue;
                    Pipe("Seated stair railing post",Polar(1.1f,a,y-.055f),Polar(1.1f,a,y+1.05f),.045f,"Iron");
                    Cylinder("Stair post mounting shoe",Polar(1.1f,a,y+.012f),new Vector3(.13f,.018f,.13f),"Iron");
                }
                new GameObject("LK Solid Floors And Seated Rails");
                foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(!c.isTrigger&&!c.GetComponentInParent<Rigidbody>())c.gameObject.layer=8;
                LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();BakeAndPreview();
        }
        public static void ExposeValveControls()
        {
            var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Level01_Plumbing"));
            var group=GameObject.Find("Compact player facing manifold").transform;
            if(!GameObject.Find("LK Aisle Valve Controls"))
            {
                foreach(var t in group.GetComponentsInChildren<Transform>())
                {
                    if(t.name=="Shutoff valve handwheel"||t.name=="Red valve wheel spoke")t.position+=Vector3.forward*.5f;
                    if(t.name=="Valve spindle"||t.name=="Valve manifold branch"||t.name=="Valve manifold return")t.gameObject.SetActive(false);
                }
                int row=0;
                foreach(var wheel in group.GetComponentsInChildren<Transform>().Where(t=>t.name=="Shutoff valve handwheel").OrderByDescending(t=>t.position.x))
                {
                    var main=group.TransformPoint(new Vector3(wheel.localPosition.x,1.3f+row*.3f,-.25f+row*.36f));
                    var join=new Vector3(wheel.position.x,wheel.position.y,main.z);
                    Pipe("Aisle valve connected stem "+row,wheel.position,join,.065f,"Iron");GameObject.Find("Aisle valve connected stem "+row).transform.SetParent(group,true);
                    Pipe("Aisle valve connected branch "+row,join,main,.065f,"Iron");GameObject.Find("Aisle valve connected branch "+row).transform.SetParent(group,true);row++;
                }
                new GameObject("LK Aisle Valve Controls");LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();BakeAndPreview();
        }
        public static void MountCeilingFixtures()
        {
            foreach(var name in new[]{"LK_Core","LK_Level01_Plumbing","LK_Level02_Generator","LK_Level03_Communications","LK_Level04_Lantern"})
            {
                var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
                if(GameObject.Find("LK Ceiling Fixture Mounts"))continue;
                foreach(var old in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                    if(old.name=="Suspended service lamp hanger")old.gameObject.SetActive(false);
                foreach(var fixture in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="LK_P_Lamp"||t.name=="Caged tungsten service lamp").ToArray())
                {
                    var renderers=fixture.GetComponentsInChildren<MeshRenderer>();if(renderers.Length==0)continue;
                    var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
                    float ceiling=fixture.position.x < -5 ? 2.975f : fixture.position.y>10 ? 12.82f : (Mathf.Floor(fixture.position.y/3.2f)+1)*3.2f-.18f;
                    float bottom=Mathf.Min(bounds.max.y-.025f,ceiling-.06f);
                    var start=new Vector3(bounds.center.x,bottom,bounds.center.z);
                    var end=new Vector3(start.x,ceiling+.025f,start.z);
                    Pipe("Fixture ceiling suspension",start,end,.045f,"Iron");
                    Cylinder("Fixture ceiling rose",new Vector3(start.x,ceiling-.012f,start.z),new Vector3(.22f,.025f,.22f),"Iron");
                }
                new GameObject("LK Ceiling Fixture Mounts");LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();BakeAndPreview();
        }
        public static void ConnectValveFeeds()
        {
            var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Level01_Plumbing"));
            if(!GameObject.Find("LK Connected Valve Feeds"))
            {
                foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                    if(t.name=="Valve pipe feed"||t.name=="Valve body connection")t.gameObject.SetActive(false);
                int row=0;
                foreach(var wheel in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Shutoff valve handwheel").OrderBy(t=>t.position.x))
                {
                    var start=wheel.position+Vector3.forward*.2f;
                    var junction=new Vector3(start.x,start.y,-3.7f+row*.36f);
                    var main=new Vector3(start.x,1.3f+row*.3f,junction.z);
                    Pipe("Valve manifold branch",start,junction,.075f,"Iron");
                    Pipe("Valve manifold return",junction,main,.075f,"Iron");row++;
                }
                new GameObject("LK Connected Valve Feeds");
                foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(!c.isTrigger&&!c.GetComponentInParent<Rigidbody>())c.gameObject.layer=8;
                LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            BakeAndPreview();
        }
        public static void AnchorFixtures()
        {
            var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Core"));
            if(!GameObject.Find("LK Fixture Attachment Revision"))
            {
                int coat=0,boots=0;
                foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                {
                    if(t.name=="Raincoat on hook"){t.position=new Vector3(-10.65f,1.5f,1.2f+.4f*coat++);t.rotation=Quaternion.Euler(0,90,0);}
                    if(t.name=="Boots below coat"){t.position=new Vector3(-10.45f,.18f,1.2f+.4f*boots++);t.rotation=Quaternion.Euler(0,90,0);}
                }
                Pipe("Wall mounted coat rack",new Vector3(-10.73f,2.02f,1.05f),new Vector3(-10.73f,2.02f,2.15f),.05f,"Iron");
                for(int i=0;i<3;i++)Pipe("Coat hook",new Vector3(-10.73f,2.02f,1.2f+i*.4f),new Vector3(-10.6f,1.96f,1.2f+i*.4f),.025f,"Iron");
                new GameObject("LK Fixture Attachment Revision");LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
            }
            foreach(var name in new[]{"LK_Level01_Plumbing","LK_Level03_Communications"})
            {
                scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
                if(GameObject.Find("LK Fixture Attachment Revision"))continue;
                float top=name.Contains("01")?3.1f:9.5f;
                var lamp=GameObject.Find("LK_P_Lamp");
                if(lamp)foreach(float x in new[]{-.35f,.35f})Pipe("Suspended service lamp hanger",lamp.transform.position+new Vector3(x,.04f,0),new Vector3(x,top,lamp.transform.position.z),.025f,"Iron");
                if(name.Contains("01"))
                {
                    var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/Materials/LK_ValveRed.mat");
                    if(!material){material=new Material(M("Rust"));material.name="LK_ValveRed";material.color=new Color(.38f,.035f,.018f);AssetDatabase.CreateAsset(material,Root+"/Art/Materials/LK_ValveRed.mat");}
                    Materials["ValveRed"]=material;
                    foreach(var wheel in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Shutoff valve handwheel").ToArray())
                    {
                        wheel.localScale=Vector3.one;wheel.GetComponent<MeshFilter>().sharedMesh=Annulus("Walkthrough_ValveWheel",.105f,.15f,0,0,0,360,24);
                        wheel.GetComponent<MeshRenderer>().sharedMaterial=material;
                        foreach(var c in wheel.GetComponents<Collider>())UnityEngine.Object.DestroyImmediate(c);
                        wheel.gameObject.AddComponent<BoxCollider>().size=new Vector3(.3f,.045f,.3f);
                        var p=wheel.position;
                        Pipe("Red valve wheel spoke",p-Vector3.right*.135f,p+Vector3.right*.135f,.024f,"ValveRed");
                        Pipe("Red valve wheel spoke",p-Vector3.up*.135f,p+Vector3.up*.135f,.024f,"ValveRed");
                        Pipe("Valve pipe feed",new Vector3(p.x,1,-3.2f),new Vector3(p.x,p.y,-3.2f),.1f,"Iron");
                    }
                }
                new GameObject("LK Fixture Attachment Revision");
                foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(!c.isTrigger&&!c.GetComponentInParent<Rigidbody>())c.gameObject.layer=8;
                LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
        public static void BakeAndPreview()
        {
            EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Bootstrap"));
            foreach(var name in LighthouseScenes.Names.Skip(1).Take(6)) EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name),OpenSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("LK_Exterior"));
            if(!Lightmapping.Bake()) throw new InvalidOperationException("Lighting bake failed; do not publish stale lighting.");
            EditorSceneManager.SaveOpenScenes();
            Directory.CreateDirectory("artifacts/verification/walkthrough-previews");
            var go=new GameObject("Walkthrough audit camera");var camera=go.AddComponent<Camera>();camera.fieldOfView=75;camera.nearClipPlane=.05f;camera.farClipPlane=100;
            var positions=new[]{new Vector3(-9.8f,1.6f,0),new Vector3(0,1.6f,-2.4f),new Vector3(3.5f,4.8f,0),new Vector3(0,8,-2.6f),new Vector3(3.5f,11.2f,.3f)};
            var targets=new[]{new Vector3(-5,1.4f,0),new Vector3(0,1.1f,-3.7f),new Vector3(0,3.5f,0),new Vector3(-4.1f,7,0),new Vector3(1.6f,9.6f,.1f)};
            var texture=new RenderTexture(1024,768,24);camera.targetTexture=texture;
            for(int i=0;i<positions.Length;i++)
            {
                camera.transform.position=positions[i];camera.transform.LookAt(targets[i]);camera.Render();RenderTexture.active=texture;
                var image=new Texture2D(1024,768,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1024,768),0,0);image.Apply();
                File.WriteAllBytes($"artifacts/verification/walkthrough-previews/{i}.png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
            }
            RenderTexture.active=null;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(go);
            File.WriteAllText("artifacts/verification/walkthrough-lighting.txt",$"Bake completed; lightmaps={LightmapSettings.lightmaps.Length}. Editor images do not establish headset acceptance.\n");
        }
        public static void FinishTreads()
        {
            var scene=EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_Core"));
            foreach(var filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Stair tread"))
            {
                if(filter.sharedMesh.name.EndsWith("_Solid")) continue;
                var source=filter.sharedMesh;var vertices=source.vertices.ToList();var triangles=source.triangles.ToList();
                int count=vertices.Count;
                for(int first=0;first<count;first+=4)
                {
                    int bottom=vertices.Count;
                    for(int j=0;j<4;j++)vertices.Add(vertices[first+j]-Vector3.up*.11f);
                    for(int j=0;j<4;j++)
                    {int a=first+j,b=first+(j+1)%4,c=bottom+j,d=bottom+(j+1)%4;triangles.AddRange(new[]{a,c,d,a,d,b});}
                    triangles.AddRange(new[]{bottom,bottom+2,bottom+1,bottom,bottom+3,bottom+2});
                }
                var mesh=new Mesh{name=source.name+"_Solid"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);
                mesh.uv=vertices.Select(v=>new Vector2(v.x,v.z)).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(mesh);
                filter.sharedMesh=SaveMesh(mesh,mesh.name);
            }
            LighthouseCirculationWeather.RebuildBatches();EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        public static void Apply()
        {
            if (Application.unityVersion != "6000.3.23f1") throw new InvalidOperationException("Use team editor.");
            foreach (var name in LighthouseScenes.Names.Take(7))
            {
                if(name=="LK_Level04_Lantern") continue;
                var scene = EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
                if (scene.GetRootGameObjects().Any(g=>g.name==Marker)) continue;
                if (name == "LK_Bootstrap")
                {
                    foreach(var body in UnityEngine.Object.FindObjectsByType<LighthouseKeepers.Player.BodyPresence>(FindObjectsSortMode.None))
                        foreach(var r in body.GetComponentsInChildren<Renderer>(true)) r.enabled=false;
                }
                if (name == "LK_Exterior")
                    foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                        if ((t.name=="Dead coastal branch" || t.name=="Wind bent branch") && t.position.x < -5 && Mathf.Abs(t.position.z)<2.7f)
                            t.gameObject.SetActive(false);
                if (name == "LK_Core") RepairCore();
                if (name == "LK_Level01_Plumbing") RepairPlumbing();
                if (name == "LK_Level02_Generator")
                {
                    // Preserve sockets, thresholds, lights and authored asset files. Clear room dressing, not team source assets.
                    foreach(var root in scene.GetRootGameObjects())
                    {
                        if (root.name=="Static render batches") continue;
                        if (root.GetComponentInChildren<Renderer>(true) && !root.GetComponentInChildren<Light>(true) && root.name!="Flood warning lamp")
                            root.SetActive(false);
                        if(root.name=="Room-specific placeholder Generator") root.SetActive(false);
                    }
                }
                if (name == "LK_Level03_Communications") RepairQuarters();
                new GameObject(Marker);
                if (name != "LK_Bootstrap") LighthouseCirculationWeather.RebuildBatches();
                foreach(var collider in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
                    if(!collider.isTrigger && !collider.GetComponentInParent<Rigidbody>() && !collider.GetComponent<CharacterController>()) collider.gameObject.layer=8;
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
        static void RepairCore()
        {
            foreach(var filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                bool ramp=filter.name=="Smooth spiral collision and stair bed", tread=filter.name=="Stair tread";
                bool floor=filter.name.Contains("annular floor") && filter.name!="Level 1 annular floor";
                if(!ramp&&!tread&&!floor)continue;
                var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);var vertices=mesh.vertices;
                for(int i=0;i<vertices.Length;i++)
                {
                    float radius=new Vector2(vertices[i].x,vertices[i].z).magnitude;
                    float target=floor?(radius<3?2.35f:radius):Mathf.Lerp(1,2.3f,Mathf.InverseLerp(1,2.6f,radius));
                    vertices[i].x*=target/radius;vertices[i].z*=target/radius;
                }
                mesh.vertices=vertices;mesh.RecalculateBounds();mesh.RecalculateNormals();
                var saved=SaveMesh(mesh,"Walkthrough_"+filter.sharedMesh.name);filter.sharedMesh=saved;
                if(filter.TryGetComponent<MeshCollider>(out var collision)){collision.sharedMesh=null;collision.sharedMesh=saved;}
                // The prior visible smooth ramp masked the treads. Keep only its collision surface.
                if(ramp && filter.TryGetComponent<MeshRenderer>(out var renderer))UnityEngine.Object.DestroyImmediate(renderer);
            }
            foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.name=="Landing shaft rail"||t.name=="Landing rail post"||t.name=="Shaft fall guard")
                {
                    var p=t.position;float radius=new Vector2(p.x,p.z).magnitude;
                    p.x*=2.38f/radius;p.z*=2.38f/radius;t.position=p;
                    // Keep the enlarged stair exit open, with rails bordering the remaining shaft.
                    float a=Mathf.Repeat(Mathf.Atan2(p.z,p.x)*Mathf.Rad2Deg,360);
                    if(a<40 || a>345)t.gameObject.SetActive(false);
                }
            for(int level=1;level<=3;level++)
            {
                float y=level*3.2f;
                MeshObject("Supported stair arrival landing "+level,Annulus("Walkthrough_Landing_"+level,1,3.5f,y,0,-3,43,16),"Wood");

            }
        }
        static void RepairPlumbing()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())
                if(root.name=="LK_Crane01_Motor")root.SetActive(false);
            // Complete the existing horizontal runs with floor-fed risers and load-bearing saddles.
            for(int row=0;row<3;row++)
            {
                float z=-3.7f+row*.36f,y=1.3f+row*.3f,d=.13f+row*.07f;
                foreach(float x in new[]{-2f,2f})
                {
                    Pipe("Pressure main floor riser",new Vector3(x,.05f,z),new Vector3(x,y,z),d,"Rust");
                    Cylinder("Bolted floor pipe collar",new Vector3(x,.05f,z),new Vector3(d*1.8f,.05f,d*1.8f),"Iron");
                    Pipe("Pressure main flange",new Vector3(x-.035f,y,z),new Vector3(x+.035f,y,z),d*1.6f,"Iron");
                }
            }
            foreach(float x in new[]{-2.3f,2.1f})
            {
                Pipe("Repair run floor connection",new Vector3(x,.04f,-3.2f),new Vector3(x,1,-3.2f),.23f,"Iron");
                Cylinder("Repair run base flange",new Vector3(x,.05f,-3.2f),new Vector3(.38f,.05f,.38f),"Iron");
            }
            foreach(float x in new[]{-1.6f,1.5f})
            {
                Pipe("Repair pipe pedestal",new Vector3(x,.04f,-3.2f),new Vector3(x,.87f,-3.2f),.07f,"Iron");
                Cylinder("Pedestal floor fixing",new Vector3(x,.03f,-3.2f),new Vector3(.24f,.03f,.24f),"Iron");
            }
            // Gauge bodies previously floated above the mains, and valve wheels lacked a connection.
            foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t.name=="Analog pressure dial")
                {var p=t.position;Pipe("Gauge pressure takeoff",p+Vector3.forward*.14f-Vector3.up*.5f,p+Vector3.forward*.14f,.045f,"Brass");Pipe("Gauge neck",p+Vector3.forward*.14f,p,.045f,"Brass");}
                if(t.name=="Shutoff valve handwheel")
                {var p=t.position;Pipe("Valve body connection",p+Vector3.forward*.25f,p+Vector3.forward*.1f,.12f,"Iron");}
            }
        }
        static void RepairQuarters()
        {
            foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t.name=="Keeper bunk frame"||t.name=="Folded wool bedding"||t.name=="Pillow")t.position+=Vector3.left*.65f;
                if(t.name=="Keeper bunk frame"||t.name=="Folded wool bedding") { var scale=t.localScale;scale.x*=.85f;t.localScale=scale; }
                if(t.name=="LK_ChairSchool"||t.name=="Keeper shelf"||t.name=="Personal photograph frame")t.gameObject.SetActive(false);
                if(t.name=="LK_TableOffice")t.position+=Vector3.back*.65f;
                if(t.name=="Radio receiver"||t.name=="Analog pressure dial"||t.name=="Gauge needle"||t.name=="Handheld radio")t.position+=Vector3.back*.65f;
                if(t.GetComponent<LighthouseKeepers.Puzzles.PuzzleSocket>() && t.name.StartsWith("Radio Relay"))t.position+=Vector3.back*.65f;
            }
            foreach(float x in new[]{-4.55f,-3.75f})foreach(float z in new[]{-.9f,.9f})
                Pipe("Keeper bunk supporting leg",new Vector3(x,6.4f,z),new Vector3(x,6.73f,z),.065f,"Iron");
        }
        public static void Audit()
        {
            var report = new StringBuilder();
            foreach (var name in LighthouseScenes.Names.Take(7))
            {
                var scene = EditorSceneManager.OpenScene(LighthouseScenes.ScenePath(name));
                report.AppendLine("SCENE " + name);
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == "Static render batches") continue;
                    report.AppendLine($"{root.name} active={root.activeSelf} position={root.transform.position:F2} scale={root.transform.localScale:F2}");
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    {
                        if (root.name.StartsWith("LK_") || root.name.Contains("stair") || root.name.Contains("floor"))
                            report.AppendLine($"  {renderer.name}: enabled={renderer.enabled} bounds={renderer.bounds}");
                    }
                }
            }
            Directory.CreateDirectory("artifacts/verification");
            File.WriteAllText("artifacts/verification/walkthrough-before.txt", report.ToString());
        }
    }
}
