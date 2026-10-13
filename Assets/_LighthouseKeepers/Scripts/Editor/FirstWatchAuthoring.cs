using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Progression;
using LighthouseKeepers.Flood;
using static LighthouseKeepers.Editor.LighthouseScenes;

namespace LighthouseKeepers.Editor
{
    public static class FirstWatchAuthoring
    {
        public const string Path = "Assets/_LighthouseKeepers/Scenes/Development/LK_FirstWatch.unity";
        const string Materials = "Assets/_LighthouseKeepers/Art/Materials/";

        [MenuItem("Lighthouse Keepers/First Watch/Play plumbing escape")]
        public static void Play()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create(); ValidateEntry(); EditorSceneManager.OpenScene(Path);
            // The entry is already open. Do not leave a start-scene override that redirects tests.
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("Lighthouse Keepers/First Watch/Create missing entry scene")]
        public static void Create()
        {
            if (File.Exists(Path)) return;
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save the open scene before creating First Watch.");
            if (!AssetDatabase.CopyAsset(ScenePath("LK_Bootstrap"), Path)) throw new IOException("Could not copy Bootstrap.");
            var scene = EditorSceneManager.OpenScene(Path);
            // These are disconnected Photon sample controls, not the First Watch session flow.
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "Networking" || root.name == "Prototype Network Start" || root.name == "Prototype Runner") root.SetActive(false);
            var director = new GameObject("First Watch / local gameplay authority").AddComponent<FirstWatchDirector>();
            Set(director,"feedbackClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/DemoAssets/Audio/Button Pop.wav"));
            var station = new GameObject("Emergency seal controls").transform;
            station.position = new(0,1.1f,-2.7f); station.rotation = Quaternion.Euler(0,180,0);
            Set(director,"station",station);
            var iron = Material("FirstWatch_Slate",new(.045f,.10f,.11f));
            var brass = Material("FirstWatch_Brass",new(.83f,.55f,.21f));
            var red = Material("FirstWatch_Red",new(.45f,.11f,.06f));
            Shape("Seal control plate",PrimitiveType.Cube,station,new(0,0,.1f),new(1.65f,.55f,.08f),iron);
            for(int i=0;i<3;i++)
            {
                var control = new GameObject("Seal valve "+FirstWatchDirector.Letter(i)).transform;
                control.SetParent(station,false); control.localPosition = new(-.4f+i*.4f,0,-.04f);
                var hit = control.gameObject.AddComponent<SphereCollider>(); hit.radius = .17f;
                var interaction = control.gameObject.AddComponent<XRSimpleInteractable>();
                var valve = control.gameObject.AddComponent<FirstWatchValve>();
                Set(valve,"director",director); Set(valve,"valveIndex",i);
                var wheel = new GameObject("Turnable wheel").transform; wheel.SetParent(control,false); Set(valve,"wheel",wheel);
                for(int spoke=0;spoke<4;spoke++)
                {
                    float a=spoke*Mathf.PI/2;
                    var grip=Shape("Wheel grip",PrimitiveType.Sphere,wheel,new(Mathf.Cos(a)*.12f,Mathf.Sin(a)*.12f,0),Vector3.one*.065f,red);
                    var bar=Shape("Wheel spoke",PrimitiveType.Cube,wheel,Vector3.zero,new(.27f,.025f,.025f),brass);
                    bar.transform.localRotation=Quaternion.Euler(0,0,spoke*90);
                }
                var hub=Shape("Seal indicator",PrimitiveType.Sphere,wheel,Vector3.zero,Vector3.one*.09f,brass);
                Set(valve,"indicator",hub.GetComponent<Renderer>());
                Label(control,FirstWatchDirector.Letter(i),new(0,-.24f,-.02f),.12f);
            }
            var braceRoot = new GameObject("Hold brass brace").transform; braceRoot.SetParent(station,false); braceRoot.localPosition = new(-.74f,0,-.1f);
            var braceHit = braceRoot.gameObject.AddComponent<BoxCollider>(); braceHit.size = new(.13f,.3f,.15f);
            braceRoot.gameObject.AddComponent<XRSimpleInteractable>();
            var brace = braceRoot.gameObject.AddComponent<FirstWatchValve>(); Set(brace,"director",director); Set(brace,"isBrace",true); Set(director,"brace",brace);
            var handle = Shape("Brass brace grip",PrimitiveType.Cube,braceRoot,Vector3.zero,new(.07f,.25f,.07f),brass);
            Set(brace,"indicator",handle.GetComponent<Renderer>());
            Label(braceRoot,"HOLD",new(0,-.24f,-.02f),.065f);
            Board("Watch briefing",director,WatchBoardRole.Briefing,new(-6.3f,1.65f,.9f),90);
            Board("Seal order and pressure",director,WatchBoardRole.Station,new(0,2.02f,-2.75f),180);
            Board("Landing refuge",director,WatchBoardRole.Landing,new(4.15f,4.65f,0),90);
            var monitor = director.gameObject.AddComponent<FloodDangerMonitor>();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("FIRST WATCH AUTHORED: " + Path);
        }

        static void Board(string name,FirstWatchDirector director,WatchBoardRole role,Vector3 position,float yaw)
        {
            var board = new GameObject(name).AddComponent<FirstWatchBoard>();
            board.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
            Set(board,"director",director); Set(board,"role",(int)role);
        }
        static void Label(Transform parent,string value,Vector3 position,float size)
        {
            var label=new GameObject(value).AddComponent<TextMeshPro>(); label.transform.SetParent(parent,false); label.transform.localPosition=position;
            label.text=value; label.font=TMP_Settings.defaultFontAsset; label.fontSize=10; label.alignment=TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta=new(6,3); label.textWrappingMode=TextWrappingModes.NoWrap;
            label.transform.localScale=Vector3.one*size; label.color=new(.94f,.88f,.73f);
        }
        static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 local,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }
        static Material Material(string name,Color color)
        {
            string path=Materials+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material)return material;
            material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};material.color=color;
            AssetDatabase.CreateAsset(material,path);return material;
        }

        [MenuItem("Lighthouse Keepers/First Watch/Validate entry references")]
        public static void ValidateEntry()
        {
            // Inspect the saved entry without replacing or saving the user's current scene.
            var scene = EditorSceneManager.OpenPreviewScene(Path);
            try
            {
                var roots = scene.GetRootGameObjects();
                T[] Components<T>() where T : Component => roots.SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
                var directors = Components<FirstWatchDirector>();
                if (directors.Length != 1) throw new InvalidOperationException("First Watch needs exactly one local authority.");
                var director = directors[0];
                if (!director.ValidateConfiguration(out var reason)) throw new InvalidOperationException(reason);
                var assignedBrace = new SerializedObject(director).FindProperty("brace").objectReferenceValue;
                var valves = Components<FirstWatchValve>();
                if (valves.Length != 4) throw new InvalidOperationException("Expected three seal valves and one brace.");
                var indices = new System.Collections.Generic.HashSet<int>();
                int braces = 0;
                foreach (var valve in valves)
                {
                    var data = new SerializedObject(valve);
                    if (data.FindProperty("director").objectReferenceValue != director ||
                        !data.FindProperty("indicator").objectReferenceValue || !valve.GetComponent<XRSimpleInteractable>() ||
                        !valve.GetComponent<Collider>())
                        throw new InvalidOperationException("Incomplete interaction references on " + valve.name);
                    float reach = data.FindProperty("handReach").floatValue;
                    if (float.IsNaN(reach) || float.IsInfinity(reach) || reach <= 0)
                        throw new InvalidOperationException("Invalid hand reach on " + valve.name);
                    if (data.FindProperty("isBrace").boolValue)
                    {
                        braces++;
                        if (assignedBrace != valve) throw new InvalidOperationException("Authority must reference the authored brace.");
                    }
                    else
                    {
                        if (!data.FindProperty("wheel").objectReferenceValue || !indices.Add(data.FindProperty("valveIndex").intValue))
                            throw new InvalidOperationException("Missing wheel or duplicate seal index on " + valve.name);
                    }
                }
                if (braces != 1 || !indices.SetEquals(new[] { 0, 1, 2 }))
                    throw new InvalidOperationException("Expected one brace and seal indices A, B, C.");
                var boards = Components<FirstWatchBoard>();
                var roles = new System.Collections.Generic.HashSet<int>();
                foreach (var board in boards)
                {
                    var data = new SerializedObject(board);
                    if (data.FindProperty("director").objectReferenceValue != director || !roles.Add(data.FindProperty("role").intValue))
                        throw new InvalidOperationException("Invalid board authority or duplicate role on " + board.name);
                }
                if (boards.Length != 3 || !roles.SetEquals(new[] { 0, 1, 2 }))
                    throw new InvalidOperationException("Expected briefing, station and landing boards.");
                Debug.Log("FIRST WATCH REFERENCES VALID: authority, tuning, three seals, brace and three boards.");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        [MenuItem("Lighthouse Keepers/First Watch/Build Quest APK")]
        public static void BuildAndroid()
        {
            Create(); ValidateEntry();
            var content=EditorBuildSettings.scenes.Where(s=>s.enabled&&!s.path.Contains("/Development/")&&!s.path.EndsWith("LK_Bootstrap.unity")).Select(s=>s.path).ToArray();
            if(content.Length!=6)throw new InvalidOperationException("Expected six authored environment scenes.");
            Directory.CreateDirectory("Builds");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Path}.Concat(content).ToArray(),locationPathName="Builds/LighthouseKeepers-FirstWatch.apk",target=BuildTarget.Android,options=BuildOptions.Development});
            File.WriteAllText("Builds/FirstWatch-Build.txt",$"Result={report.summary.result}\nErrors={report.summary.totalErrors}\nWarnings={report.summary.totalWarnings}\nBytes={report.summary.totalSize}\n");
            if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new InvalidOperationException("First Watch APK failed. Inspect build log.");
        }
    }
}
