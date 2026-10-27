using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
namespace LighthouseKeepers.Editor.Environment
{
    public static class LighthouseBuilder
    {
        public const string LayoutPath = "Assets/Data/Layout/lighthouse-layout.json";
        public const string LabPath = "Assets/Scenes/Lab/LK_Lab_Environment.unity";
        public static ShellLayout Layout => JsonUtility.FromJson<ShellLayout>(StationBoxExporter.ConvertRouteArrays(File.ReadAllText(LayoutPath)));
        static readonly string[] Parts = { "Exterior", "House", "F1", "F2", "F3", "F4" };
        static readonly Dictionary<string, string> Palette = new Dictionary<string, string> {
            {"Stone","#6B6054"},{"Plaster","#B5A48C"},{"Trim","#8A7F6E"},{"Timber","#4A3B2C"},{"Metal","#5A6066"},{"Valve","#B22A20"},
            {"Generator","#C8961E"},{"Bilge","#2C5F7A"},{"Door","#3E5C43"},{"Glass","#73939D"},{"Brass","#A27E48"},{"Slate","#343C45"},{"Rock","#222A30"},{"Lamp","#FFB85C"},{"Emergency","#9FC4D6"} };
        public static Vector3 V(float[] a) => new Vector3(a[0],a[1],a[2]);
        public static Vector3 Polar(float a,float r,float y) => new Vector3(Mathf.Sin(a*Mathf.Deg2Rad)*r,y,Mathf.Cos(a*Mathf.Deg2Rad)*r);
        static string Hash(string path) { using(var h=SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-",""); }
        [MenuItem("Lighthouse/Rebuild Shell Scenes")]
        public static void BuildAndValidate(){BuildAll();EnvironmentValidation.RunGeometry();}
        public static void BuildAll()
        {
            for(int i=0;i<SceneManager.sceneCount;i++) { var s=SceneManager.GetSceneAt(i); if(s.isDirty) throw new InvalidOperationException("Save/close unsaved scenes before rebuilding: "+s.path); }
            if(ShellBuildCache.IsCurrent()) {ValidateScale();Debug.Log("SHELL_UNCHANGED_INPUT_BYTE_IDENTICAL");return;}
            foreach(string d in new[]{"Assets/Prefabs/Environment","Assets/Art/Materials/Environment","Assets/Scenes/Shell","Assets/Scenes/Dress","Assets/Settings/Lighting","artifacts/batch-a"}) Directory.CreateDirectory(d);
            AssetDatabase.Refresh();
            var manifest=JsonUtility.FromJson<ShellManifest>(File.ReadAllText("Assets/Art/Generated/ArtManifest.json"));
            if(!string.Equals(manifest.sourceHash,Hash(LayoutPath),StringComparison.OrdinalIgnoreCase)) throw new Exception("Blender manifest does not match layout; regenerate meshes.");
            foreach(var entry in manifest.assets.OrderBy(a=>a.scene,StringComparer.Ordinal)) MakePrefab(entry);
            foreach(string part in Parts)
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var root=new GameObject("__GENERATED__ DO NOT HAND EDIT"); new GameObject("Source SHA256 "+manifest.sourceHash).transform.SetParent(root.transform,false);
                var geometry=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/PF_Shell_"+part+".prefab"));geometry.transform.SetParent(root.transform,false);
                AddAnchors(root.transform,part); AddLights(root.transform,part);
                RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.16f,.18f,.21f);RenderSettings.skybox=null;RenderSettings.fog=false;
                EditorSceneManager.SaveScene(scene,"Assets/Scenes/Shell/LK_Shell_"+part+".unity");
                string dress="Assets/Scenes/Dress/LK_Dress_"+part+".unity";
                if(!File.Exists(dress)) { var ds=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); new GameObject("Hand-dressed props — builder never replaces this scene");EditorSceneManager.SaveScene(ds,dress); }
            }
            BuildLab(); ValidateScale(); AssetDatabase.SaveAssets();
            File.WriteAllText("artifacts/batch-a/generation.txt","Generated six shell scenes and standalone Lab. Production build settings unchanged.\n");
            ShellBuildCache.Record();Debug.Log("SHELL_GENERATION_PASS");
        }
        static Material MaterialFor(string name)
        {
            string key=name.Replace("M_Environment_",""); if(!Palette.TryGetValue(key,out var hex))throw new Exception("Unknown authored material "+name);
            string path="Assets/Art/Materials/Environment/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m) {m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;AssetDatabase.CreateAsset(m,path);}
            ColorUtility.TryParseHtmlString(hex,out var c);m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",key=="Metal"?.38f:.18f);m.SetFloat("_Metallic",key=="Metal"||key=="Brass"?.65f:0);
            m.enableInstancing=true;
            if(key=="Lamp"||key=="Emergency") {m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*2);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
            if(key=="Glass") {m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",(int)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.SetColor("_BaseColor",new Color(c.r,c.g,c.b,.13f));m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;}
            EditorUtility.SetDirty(m);return m;
        }
        static void MakePrefab(ShellAsset entry)
        {
            if(!string.Equals(Hash(entry.path),entry.sha256,StringComparison.OrdinalIgnoreCase))throw new Exception("FBX does not match tracked manifest: "+entry.path);
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(entry.path);if(!model)throw new Exception("Model import failed "+entry.path);
            var root=UnityEngine.Object.Instantiate(model);root.name="PF_Shell_"+entry.scene;
            // Unity infers an FBX LODGroup from _LOD names; replace that imported group
            // before assigning our explicit island-only LODs, avoiding duplicate ownership.
            foreach(var importedLod in root.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(importedLod);
            if(Vector3.Distance(root.transform.localScale,Vector3.one)>.00001f||Quaternion.Angle(root.transform.localRotation,Quaternion.identity)>.001f)throw new Exception("FBX root scale/orientation "+entry.path);
            var metadata=entry.objects.ToDictionary(o=>o.name);
            foreach(var t in root.GetComponentsInChildren<Transform>(true).OrderBy(t=>t.name,StringComparer.Ordinal))
            {
                var mf=t.GetComponent<MeshFilter>();if(!mf)continue;
                if(!metadata.TryGetValue(t.name,out var spec))throw new Exception("Imported object absent from manifest: "+t.name);
                var renderer=t.GetComponent<MeshRenderer>();
                if(t.name.StartsWith("UCX_",StringComparison.Ordinal))
                {
                    if(renderer)UnityEngine.Object.DestroyImmediate(renderer);
                    if(spec.collisionEnabled)
                    {
                        var col=t.gameObject.AddComponent<MeshCollider>();col.sharedMesh=mf.sharedMesh;col.convex=!spec.staticCollision;col.cookingOptions=MeshColliderCookingOptions.CookForFasterSimulation|MeshColliderCookingOptions.EnableMeshCleaning|MeshColliderCookingOptions.WeldColocatedVertices;
                        // Lower LOD meshes never add a second overlapping island collider.
                        if(t.name.Contains("_LOD1")||t.name.Contains("_LOD2"))col.enabled=false;
                    }
                    UnityEngine.Object.DestroyImmediate(mf);continue;
                }
                if(spec.collisionOnly) {if(renderer)UnityEngine.Object.DestroyImmediate(renderer);UnityEngine.Object.DestroyImmediate(mf);continue;}
                renderer.sharedMaterials=spec.materials.Select(MaterialFor).ToArray();
                GameObjectUtility.SetStaticEditorFlags(t.gameObject,StaticEditorFlags.BatchingStatic|StaticEditorFlags.ContributeGI|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic|StaticEditorFlags.ReflectionProbeStatic);
                renderer.scaleInLightmap=1f;
                if(t.name.Contains("_LOD1")||t.name.Contains("_LOD2"))renderer.enabled=false;
            }
            var rocks=root.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("SM_Rock_Island_LOD")).OrderBy(r=>r.name).ToArray();
            if(rocks.Length==3){var g=new GameObject("Island LOD");g.transform.SetParent(root.transform,false);foreach(var r in rocks){r.enabled=true;r.transform.SetParent(g.transform,true);}var lod=g.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.15f,new Renderer[]{rocks[0]}),new LOD(.06f,new Renderer[]{rocks[1]}),new LOD(.01f,new Renderer[]{rocks[2]})});lod.RecalculateBounds();}
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Environment/PF_Shell_"+entry.scene+".prefab");UnityEngine.Object.DestroyImmediate(root);
        }
        static void AddAnchors(Transform root,string floor)
        {
            foreach(var s in Layout.stations.Where(s=>s.floor==floor).OrderBy(s=>s.id,StringComparer.Ordinal))
            {
                var p=new GameObject(s.id).transform;p.SetParent(root,false);p.position=V(s.anchor.position);p.rotation=Quaternion.Euler(V(s.anchor.rotationEuler));
                foreach(var a in s.interactiveAnchors){var t=new GameObject(a.name).transform;t.SetParent(p,false);t.localPosition=V(a.localPosition);}
                foreach(var a in s.legacySockets){var t=new GameObject(a.socketName).transform;t.SetParent(p,false);t.localPosition=V(a.localPosition);new GameObject(a.anchorName).transform.SetParent(t,false);}
            }
        }
        static void AddLights(Transform root,string floor)
        {
            if(floor=="Exterior")return;
            var f=Layout.floors.FirstOrDefault(x=>x.id==floor);float y=f==null?0:f.floorY,r=f==null?0:f.interiorRadius;
            Vector3[] positions=f==null?new[]{new Vector3(-2.4f,2.35f,-5.05f),new Vector3(2.4f,2.35f,-5.05f),new Vector3(0,2.32f,-7.35f)}:new[]{Polar(30,r-.23f,y+2.35f),Polar(150,r-.23f,y+2.35f),Polar(270,r-.23f,y+2.35f)};
            foreach(var pos in positions){var g=new GameObject(f==null&&pos.x<0?"Oil fixture light":"Mains fixture light");g.transform.SetParent(root,false);g.transform.position=pos;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.78f,.52f);l.intensity=f==null?5:4;l.range=8;l.shadows=LightShadows.Hard;l.lightmapBakeType=LightmapBakeType.Baked;}
            if(f!=null)foreach(float a in new[]{90f,270f}) {var g=new GameObject("Emergency fixture light");g.transform.SetParent(root,false);g.transform.position=Polar(a,r-.21f,y+2.45f);var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(.62f,.77f,.84f);l.intensity=f.id=="F2"?3.6f:(f.id=="F3"?1.2f:.5f);l.range=8;l.shadows=LightShadows.Hard;l.lightmapBakeType=LightmapBakeType.Baked;}
            if(floor=="F4")foreach(float a in new[]{45f,225f})
            {
                var g=new GameObject("Emergency gallery fixture light");g.transform.SetParent(root,false);g.transform.position=Polar(a,3.2f,y+2.59f);
                var light=g.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(.62f,.77f,.84f);light.intensity=1.2f;light.range=7;light.shadows=LightShadows.Hard;light.lightmapBakeType=LightmapBakeType.Baked;
            }
            var probes=new GameObject("Hand light probes").AddComponent<LightProbeGroup>();probes.transform.SetParent(root,false);var points=new List<Vector3>();
            for(float x=-2.5f;x<=2.5f;x+=1.25f)for(float z=-2.5f;z<=2.5f;z+=1.25f)if(f==null||new Vector2(x,z).magnitude<r-.25f)foreach(float h in new[]{.5f,1.5f,2.3f})points.Add(new Vector3(x,y+h,z+(f==null?-7.35f:0)));
            probes.probePositions=points.ToArray();
        }
        public static void BuildLab()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            foreach(string part in Parts)
            {
                var source=EditorSceneManager.OpenScene("Assets/Scenes/Shell/LK_Shell_"+part+".unity",OpenSceneMode.Additive);
                foreach(var g in source.GetRootGameObjects()){SceneManager.MoveGameObjectToScene(g,scene);g.name="Shell "+part;}
                EditorSceneManager.CloseScene(source,true);
            }
            SceneManager.SetActiveScene(scene);
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.22f,.24f,.27f);RenderSettings.skybox=null;RenderSettings.fog=false;
            string rigPath="Assets/_LighthouseKeepers/Prefabs/Player/LK_QuestPlayer.prefab";
            var rigAsset=AssetDatabase.LoadAssetAtPath<GameObject>(rigPath);if(!rigAsset)throw new Exception("Existing player prefab missing");
            var rig=(GameObject)PrefabUtility.InstantiatePrefab(rigAsset);rig.name="Existing player — unmodified source prefab";rig.transform.position=new Vector3(0,.04f,-8.4f);
            EnvironmentDeviceRepair.HideTorso(rig);
            var managerType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEngine.XR.Interaction.Toolkit.XRInteractionManager")).FirstOrDefault(t=>t!=null);
            if(managerType!=null)new GameObject("Existing XRI interaction manager").AddComponent(managerType);
            // Measurement props belong to ValidateScale, never the playable House.
            EditorSceneManager.SaveScene(scene,LabPath);
        }
        public static void ValidateScale()
        {
            var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Generated/SM_Shell_ScaleReference.fbx"));
            var cube=go.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="SM_ScaleReference_Cube");var marker=go.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="SM_ScaleReference_Height");
            if(Vector3.Distance(cube.bounds.size,Vector3.one)>.001f||Mathf.Abs(marker.bounds.size.y-1.7f)>.001f||Vector3.Distance(marker.bounds.center,new Vector3(1.3f,.85f,0))>.001f||Vector3.Distance(cube.bounds.center,new Vector3(0,.5f,0))>.001f)throw new Exception("Art scale/orientation test failed: "+cube.bounds+" marker "+marker.bounds);
            UnityEngine.Object.DestroyImmediate(go);Debug.Log("ART_SCALE_AND_ORIENTATION_PASS");
        }
        public static void ExportStationContract()=>StationBoxExporter.Build();
    }
}
