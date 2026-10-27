using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.WatchDemo;
using LighthouseKeepers.WatchDemo.Editor;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Presentation.Environment;
using LighthouseKeepers.Environment.Sprint3;
using Object=UnityEngine.Object;
namespace LighthouseKeepers.Editor
{
    public static class OneLighthouseM2
    {
        public const string Output="artifacts/one/M2";
        const string Lab="Assets/Scenes/Lab/LK_Lab_Environment.unity";
        [MenuItem("Lighthouse/Watch/Apply M2 foundation")]
        public static void Apply()
        {
            Directory.CreateDirectory(Output);WatchDayAuthoring.Seed();
            var scene=EditorSceneManager.OpenScene(Lab);
            var old=GameObject.Find("STN_GENERATOR");
            var survey=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Renderer>(true))
                .Where(r=>r.bounds.center.y>3.3f&&r.bounds.center.y<6.3f)
                .Select(r=>$"{r.name} centre={r.bounds.center} size={r.bounds.size}");
            File.WriteAllLines(Output+"/f2-survey.txt",survey);
            var position=old.transform.position;var rotation=old.transform.rotation;var parent=old.transform.parent;
            // Copy only the generator fixture. No session C board, bed, environment or bake migration.
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Sprint3/PF_Sprint3Fixtures.prefab");
            var source=prefab.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="STN_GENERATOR");
            Object.DestroyImmediate(old);
            var gen=Object.Instantiate(source.gameObject,parent);gen.name="STN_GENERATOR";gen.transform.SetPositionAndRotation(position,rotation);
            // Keep the frozen service/legacy aliases when replacing the old empty station root.
            var contract=JsonUtility.FromJson<StationBoxExporter.Contract>(File.ReadAllText("Assets/Data/Layout/lighthouse-layout.json"));
            var station=contract.stations.Single(s=>s.id=="STN_GENERATOR");
            foreach(var anchor in station.interactiveAnchors)Anchor(gen.transform,anchor.name,anchor.localPosition);
            foreach(var alias in station.legacySockets)
            {
                var socket=Anchor(gen.transform,alias.socketName,alias.localPosition);
                Anchor(socket,alias.anchorName,new float[]{0,0,0});
            }
            foreach(var t in gen.GetComponentsInChildren<Transform>(true))GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
            // Noninteractive stubs retain their authored locations for future primer/choke work.
            foreach(string name in new[]{"GRIP_PrimerLever","GRIP_ChokeLever"})
            {var t=Find(gen,name);t.name="FUTURE_"+name;t.gameObject.SetActive(false);}
            Object.DestroyImmediate(Find(gen,"Starter legend").gameObject);
            foreach(var label in gen.GetComponentsInChildren<FixtureLabel>())label.Prepare();
            var grip=Find(gen,"GRIP_StarterHandle");
            var collider=grip.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(.14f,.08f,.08f);
            var interactable=grip.gameObject.AddComponent<XRSimpleInteractable>();interactable.colliders.Add(collider);
            var feedback=grip.gameObject.AddComponent<PhysicalGrip>();feedback.Configure(grip,HandPose.WrapGrip);
            var starter=grip.gameObject.AddComponent<GeneratorRestart>();starter.ConfigureFoundation(grip);
            // Engine-only shake; stationary plinth, exhaust and controller remain fixed.
            var engine=new GameObject("Engine feedback").transform;engine.SetParent(gen.transform,false);
            foreach(var t in gen.transform.Cast<Transform>().ToArray())
                if(t.name=="Diesel crankcase"||t.name=="Alternator cover"||t.name=="Radiator grille")t.SetParent(engine,true);
            var lamp=Find(gen,"Starter recovery lens").GetComponent<Renderer>();
            lamp.sharedMaterial=LampMaterial();
            var text=new GameObject("Starter instruction").AddComponent<TextMesh>();text.transform.SetParent(gen.transform,false);
            text.transform.localPosition=new Vector3(0,1.57f,.76f);text.transform.localRotation=Quaternion.Euler(0,180,0);
            text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.fontSize=64;text.characterSize=.006f;text.color=new Color(.8f,.76f,.57f);text.text="PULL STARTER 0/3";
            var audio=gen.AddComponent<AudioSource>();audio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_LighthouseKeepers/Audio/Ambience/Placeholder_Generator.wav");
            audio.playOnAwake=false;audio.loop=true;audio.spatialBlend=1;audio.volume=.18f;audio.minDistance=.6f;audio.maxDistance=12;
            var lights=new Light[2];
            for(int i=0;i<2;i++)
            {
                var name="Watch powered service light F"+(i+1);var previous=GameObject.Find(name);if(previous)Object.DestroyImmediate(previous);
                var light=new GameObject(name).AddComponent<Light>();light.transform.position=new Vector3(0,2.4f+i*3.3f,1.5f);
                light.type=LightType.Point;light.color=new Color(1,.78f,.52f);light.intensity=1.2f;light.range=4.2f;light.shadows=LightShadows.None;lights[i]=light;
            }
            var component=gen.AddComponent<WatchGenerator>();
            Set(component,"starter",starter);Set(component,"engine",engine);Set(component,"lamp",lamp);Set(component,"prompt",text);Set(component,"runningAudio",audio);
            Set(component,"lighting",Object.FindFirstObjectByType<BeaconAndMainsLightingGroup>());
            var serialized=new SerializedObject(component);var array=serialized.FindProperty("serviceLights");array.arraySize=2;
            for(int i=0;i<2;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=lights[i];serialized.ApplyModifiedPropertiesWithoutUndo();
            Set(Object.FindFirstObjectByType<WatchDemoController>(),"generator",component);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();OneLighthouseM1.ApplyShipBuildScenes();
            EditorSceneManager.OpenScene(LighthouseNetworkLobby.ScenePath);
        }
        static Transform Anchor(Transform parent,string name,float[] position)
        {
            var t=parent.Find(name);if(!t){t=new GameObject(name).transform;t.SetParent(parent,false);}
            t.localPosition=new Vector3(position[0],position[1],position[2]);return t;
        }
        static Material LampMaterial()
        {
            const string path="Assets/WatchDemo/GeneratorLamp.mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",Color.green);AssetDatabase.CreateAsset(m,path);}return m;
        }
        static Transform Find(GameObject root,string name)=>root.GetComponentsInChildren<Transform>(true).Single(t=>t.name==name);
        static void Set(Object target,string name,Object value){var s=new SerializedObject(target);s.FindProperty(name).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        public static void Build()
        {
            Directory.CreateDirectory(Output);Directory.CreateDirectory("Builds");OneLighthouseM1.ApplyShipBuildScenes();
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=LighthouseNetworkLobby.GameScenes,target=BuildTarget.Android,locationPathName="Builds/LighthouseKeepers-M2.apk",options=BuildOptions.Development});
            File.WriteAllText(Output+"/build.txt",$"{report.summary.result}\nErrors={report.summary.totalErrors}\nWarnings={report.summary.totalWarnings}\nBytes={report.summary.totalSize}\nDuration={report.summary.totalTime}\n");
            if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new BuildFailedException("M2 build failed");
        }
    }
}
