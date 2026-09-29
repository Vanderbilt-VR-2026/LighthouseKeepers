using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Lobby;
using static LighthouseKeepers.Editor.LighthouseAssets;

namespace LighthouseKeepers.Editor
{
    /// <summary>Authored, isolated keeper room. Does not regenerate shared environments or prefabs.</summary>
    public static class LighthouseLobbyPreview
    {
        public const string ScenePath = "Assets/_LighthouseKeepers/Scenes/Development/LK_LobbyPreview.unity";
        const string VariantRoot = "Assets/_LighthouseKeepers/ThirdPartyVariants/";

        [MenuItem("Lighthouse Keepers/Play lobby preview")]
        public static void Play()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateScene();
            EditorSceneManager.OpenScene(ScenePath);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorApplication.EnterPlaymode();
        }
        public static void CreateScene()
        {
            if (File.Exists(ScenePath)) return;
            BuildRoom();
        }
        // Explicit authoring command for this PR, never called implicitly on an existing scene.
        public static void RebuildWatchRoom()
        {
            BuildRoom();
            EditorApplication.Exit(0);
        }
        public static void RebuildAndVerify()
        {
            BuildRoom();
            LighthouseLobbyVerification.RunBatch();
        }
        static void BuildRoom()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = M("StormSky");
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new(.34f,.40f,.44f);
            RenderSettings.ambientEquatorColor = new(.23f,.26f,.25f);
            RenderSettings.ambientGroundColor = new(.12f,.13f,.12f);
            RenderSettings.fog = false;
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/Player/LK_QuestPlayer.prefab"));
            rig.transform.SetPositionAndRotation(new Vector3(0,0,-1.25f),Quaternion.identity);
            var camera = rig.GetComponentInChildren<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 60;
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 180;
            var room = new GameObject("Keeper watch room").transform;
            Box("Timber floor",new(0,-.12f,0),new(7,.24f,7),"Wood",room);
            // Planks and exposed studs echo the keeper house's authored construction.
            for (int i=-8;i<=8;i++) Box("Floor seam",new(i*.4f,.002f,0),new(.012f,.005f,7),"Iron",room,false);
            Box("Front wall",new(.95f,1.7f,2.55f),new(5.1f,3.4f,.2f),"Interior",room);
            Box("Seaward window sill",new(-2.55f,.49f,2.55f),new(1.9f,.98f,.2f),"Interior",room);
            Box("Seaward window header",new(-2.55f,3.07f,2.55f),new(1.9f,.66f,.2f),"Interior",room);
            Box("Seaward window crossbar",new(-2.55f,1.89f,2.5f),new(1.9f,.07f,.16f),"Wood",room,false);
            Box("Seaward window centre",new(-2.55f,1.86f,2.5f),new(.07f,1.76f,.16f),"Wood",room,false);
            // An invisible solid pane prevents walking through the open visual window.
            var pane = new GameObject("Window safety pane").AddComponent<BoxCollider>();
            pane.transform.position = new(-2.55f,1.86f,2.55f); pane.size = new(1.9f,1.76f,.1f);
            Box("Back wall",new(0,1.7f,-3.5f),new(7,3.4f,.2f),"Interior",room);
            Box("Ceiling",new(0,3.45f,-.45f),new(7,.14f,8),"Wood",room);
            Box("East wall",new(3.5f,1.7f,0),new(.2f,3.4f,7),"Interior",room);
            // Large west window: solid sill and header, actual open view to the sea.
            Box("Window sill wall",new(-3.5f,.5f,0),new(.2f,1,7),"Interior",room);
            Box("Window header",new(-3.5f,3.03f,0),new(.2f,.74f,7),"Interior",room);
            for (int i=-3;i<=3;i+=2) Box("Window mullion",new(-3.5f,1.83f,i),new(.18f,1.65f,.10f),"Wood",room);
            var sidePane = new GameObject("West window safety pane").AddComponent<BoxCollider>();
            sidePane.transform.position = new(-3.5f,1.83f,0); sidePane.size = new(.1f,1.7f,7);
            Box("Window ledge",new(-3.38f,1.01f,0),new(.44f,.10f,7),"Wood",room);
            for (int i=-3;i<=3;i++)
            {
                if (i >= -1) Box("Wall stud",new(i,1.7f,2.38f),new(.10f,3.4f,.16f),"Wood",room,false);
                Box("Ceiling beam",new(i,3.27f,-.4f),new(.15f,.24f,7),"Wood",room,false);
            }
            Box("Wall skirting",new(0,.12f,2.38f),new(7,.24f,.12f),"Wood",room,false);
            var board = new GameObject("Physical watch board").transform;
            board.position = new(0,1.73f,1.55f);
            Box("Slate face",new(0,0,.055f),new(2.62f,1.98f,.10f),"Slate",board,false);
            Box("Timber backing",new(0,0,.12f),new(2.85f,2.16f,.12f),"Wood",board,false);
            foreach (int side in new[]{-1,1})
            {
                Box("Brass side binding",new(side*1.34f,0,-.012f),new(.025f,2.06f,.026f),"Brass",board,false);
                Box("Brass cross binding",new(0,side*1.015f,-.012f),new(2.70f,.025f,.026f),"Brass",board,false);
                foreach (int end in new[]{-1,1})
                {
                    var screw = Primitive(PrimitiveType.Sphere,"Frame fastener",new(side*1.38f,end*.99f,-.005f),new(.035f,.035f,.015f),"Brass",board,false);
                }
            }
            var lobby = board.gameObject.AddComponent<LobbyManager>();
            var screen = board.gameObject.AddComponent<LobbyScreen>();
            LighthouseScenes.Set(screen,"manager",lobby);
            LighthouseScenes.Set(screen,"boardMount",board);
            LighthouseScenes.Set(screen,"paperTexture",AssetDatabase.LoadAssetAtPath<Texture>(Root+"/Art/Textures/LK_Paper_Wear.png"));
            LighthouseScenes.Set(screen,"selectSound",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/DemoAssets/Audio/Button Pop.wav"));
            // Retain a clear approach in front of the board for either controller and seated reach.
            var desk = Prop("LK_TableOffice",new(-2.35f,0,.5f),new(0,90,0),1.3f);
            Prop("LK_SmallMetalicCase",new(-2.35f,LighthouseVendorVariants.BoundsOf(desk).max.y,.55f),Vector3.zero,.32f);
            Prop("LK_MetalCabinet01_1",new(2.5f,0,1.85f),new(0,180,0),1.3f);
            Prop("LK_FireExtinguisher01_1",new(2.2f,0,1.9f),Vector3.zero,.62f);
            Prop("LK_Barrel01a",new(-2.5f,0,2),Vector3.zero,.8f);
            // Warm practical fittings, two local lights without realtime shadows.
            Sconce(new(-1.85f,2.12f,1.95f));
            Sconce(new(1.85f,2.12f,1.95f));
            var skyLight = new GameObject("Cold coastal daylight").AddComponent<Light>();
            skyLight.type = LightType.Directional; skyLight.intensity = .8f;
            skyLight.color = new(.58f,.73f,.85f); skyLight.shadows = LightShadows.None;
            skyLight.transform.rotation = Quaternion.Euler(35,-75,0);
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ocean.name = "Ocean beyond keeper window";
            ocean.transform.position = new(-35,-2.0f,0); ocean.transform.localScale = new(14,1,14);
            Object.DestroyImmediate(ocean.GetComponent<Collider>());
            ocean.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(VariantRoot+"Materials/LK_QuestOcean.mat");
            foreach (var pos in new[]{new Vector3(-8,-1,3),new Vector3(-11,-1,-4),new Vector3(-7,-1,-8)})
                Primitive(PrimitiveType.Sphere,"Shore basalt",pos,new(3,3,4),"Rock",null,false);
            // Low-level positional sea/rain loops, using existing original placeholder clips.
            Ambience("Rain on window",Clip("RainInterior"),new(-3.4f,1.8f,0),.12f);
            Ambience("Sea outside",Clip("Ocean"),new(-6,0,0),.22f);
            EditorSceneManager.SaveScene(scene,ScenePath);
            AssetDatabase.SaveAssets();
        }
        static void Sconce(Vector3 position)
        {
            Box("Sconce back plate",position+new Vector3(0,0,.12f),new(.23f,.5f,.10f),"Iron",null,false);
            Primitive(PrimitiveType.Cylinder,"Sconce glass",position,new(.13f,.18f,.13f),"Lamp",null,false);
            foreach (int side in new[]{-1,1})
                Box("Lamp guard",position+new Vector3(side*.09f,0,-.025f),new(.018f,.42f,.02f),"Brass",null,false);
            var light = new GameObject("Warm watch lamp").AddComponent<Light>();
            light.transform.position = position + new Vector3(0,0,-.4f);
            light.type = LightType.Point; light.color = new(1,.69f,.36f);
            light.intensity = 3.5f; light.range = 5; light.shadows = LightShadows.None;
        }
        static GameObject Prop(string name,Vector3 position,Vector3 euler,float longestSide)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VariantRoot+"Prefabs/"+name+".prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var bounds = LighthouseVendorVariants.BoundsOf(go);
            float scale = longestSide / Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
            go.transform.localScale = Vector3.one * scale;
            go.transform.SetPositionAndRotation(position,Quaternion.Euler(euler));
            return go;
        }
        static void Ambience(string name,AudioClip clip,Vector3 position,float volume)
        {
            var source = new GameObject(name).AddComponent<AudioSource>();
            source.transform.position = position; source.clip = clip; source.loop = true;
            source.playOnAwake = true; source.spatialBlend = 1; source.volume = volume;
            source.minDistance = 1; source.maxDistance = 14; source.rolloffMode = AudioRolloffMode.Linear;
        }
    }
}
