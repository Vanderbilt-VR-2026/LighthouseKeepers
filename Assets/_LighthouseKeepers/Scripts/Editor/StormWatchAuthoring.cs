using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.StormWatch;
using LighthouseKeepers.StormWatchNetworking;
using static LighthouseKeepers.Editor.LighthouseScenes;
namespace LighthouseKeepers.Editor
{
    public static class StormWatchAuthoring
    {
        public const string Entry="Assets/_LighthouseKeepers/Scenes/Development/LK_StormWatch.unity";
        [MenuItem("Lighthouse Keepers/Storm Watch/Create entry")]
        public static void Create()
        {
            if(File.Exists(Entry))return;
            if(!AssetDatabase.CopyAsset(ScenePath("LK_Bootstrap"),Entry))throw new IOException("Cannot copy Bootstrap.");
            var scene=EditorSceneManager.OpenScene(Entry);
            foreach(var root in scene.GetRootGameObjects())if(root.name=="Networking"||root.name=="Prototype Network Start"||root.name=="Prototype Runner")root.SetActive(false);
            var authority=new GameObject("Storm Watch / shared host authority").AddComponent<StormWatchDirector>();
            var link=authority.gameObject.AddComponent<StormWatchFusionLink>();Set(authority,"link",link);
            Set(authority,"feedbackClip",AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/DemoAssets/Audio/Button Pop.wav"));
            var gates=new UnityEngine.Object[3];
            for(int i=0;i<3;i++)
            {
                var gate=Shape("Route "+(i+1)+" / mechanical stair gate",PrimitiveType.Cube,new(0,(i+1)*3.2f-.12f,0),new(5.4f,.12f,5.4f),"FirstWatch_Red",true);
                gates[i]=gate;Label(gate.transform,"REPAIR TO OPEN",new(0,.35f,-.12f),.085f);
            }
            SetArray(authority,"gates",gates);
            Control(authority,"BRACE / hold coupling",WatchAction.Brace,WatchStations.Brace,180);
            Control(authority,"PIPE / touch wrench and twist",WatchAction.Pipe,WatchStations.Pipe,180);
            Control(authority,"PUMP / grip and stroke",WatchAction.Pump,WatchStations.Pump,145);
            Control(authority,"GENERATOR / grip and turn",WatchAction.Generator,WatchStations.Generator,180);
            Control(authority,"RADIO / hold to transmit",WatchAction.Radio,WatchStations.Radio,180);
            for(int i=0;i<3;i++)Control(authority,StormWatchBoard.Symbol(i), (WatchAction)((int)WatchAction.BeaconA+i),WatchStations.Beacon+Vector3.right*(i-1)*.45f,0);
            Board(authority,0,new(-6.35f,1.75f,.8f),90);
            Board(authority,1,new(0,2.25f,-2.9f),180);
            Board(authority,2,new(1.4f,5.25f,-4.1f),180);
            Board(authority,3,new(.65f,8.35f,-4.15f),180);
            Board(authority,4,new(0,11.9f,4.35f),0);
            var wrench=Shape("Watch wrench / grip and bring to bolt",PrimitiveType.Cube,new(2.8f,1,-1.8f),new(.09f,.045f,.4f),"FirstWatch_Brass",true);
            wrench.AddComponent<Rigidbody>().mass=.3f;wrench.AddComponent<XRGrabInteractable>();Set(wrench.AddComponent<WatchWrench>(),"director",authority);
            Label(wrench.transform,"WRENCH",new(0,.1f,0),.08f);
            var buoy=Shape("Reusable yellow lifeline / aim and release",PrimitiveType.Sphere,new(-7.7f,1.1f,0),Vector3.one*.18f,"FirstWatch_Brass",true);
            buoy.AddComponent<Rigidbody>().useGravity=false;buoy.AddComponent<XRGrabInteractable>();Set(buoy.AddComponent<WatchLifeline>(),"director",authority);
            var catchRing=Control(authority,"CATCH LIFELINE",WatchAction.CatchLine,new(-8,1,0),0);
            var pull=Control(authority,"PULL LIFELINE",WatchAction.PullLine,new(-8,1,0),0);
            Set(authority,"rescueCatch",catchRing);Set(authority,"rescuePull",pull);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("STORM WATCH AUTHORED: two-player, three acts, four floors.");
        }
        static GameObject Control(StormWatchDirector director,string name,WatchAction action,Vector3 position,float yaw)
        {
            var go=new GameObject(name);go.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
            go.AddComponent<SphereCollider>().radius=.19f;go.AddComponent<XRSimpleInteractable>();
            var control=go.AddComponent<WatchControl>();Set(control,"director",director);Set(control,"action",(int)action);
            var wheel=Shape("Brass grip",PrimitiveType.Cube,position,new(.3f,.08f,.08f),"FirstWatch_Brass");wheel.transform.SetParent(go.transform,true);
            Set(control,"movingPart",wheel.transform);Set(control,"indicator",wheel.GetComponent<Renderer>());
            Label(go.transform,name,new(0,-.25f,0),.045f);return go;
        }
        static void Board(StormWatchDirector director,int floor,Vector3 position,float yaw)
        {var board=new GameObject("Watch instructions "+floor).AddComponent<StormWatchBoard>();board.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));Set(board,"director",director);Set(board,"floor",floor);}
        static GameObject Shape(string name,PrimitiveType type,Vector3 position,Vector3 scale,string material,bool collider=false)
        {var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_LighthouseKeepers/Art/Materials/"+material+".mat");if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
        static void Label(Transform parent,string text,Vector3 local,float size)
        {var t=new GameObject("Label").AddComponent<TextMeshPro>();t.transform.SetParent(parent,false);t.transform.localPosition=local;t.transform.localScale=Vector3.one*size;t.rectTransform.sizeDelta=new(18,3);t.text=text;t.font=TMP_Settings.defaultFontAsset;t.fontSize=5;t.alignment=TextAlignmentOptions.Center;t.textWrappingMode=TextWrappingModes.NoWrap;}
        [MenuItem("Lighthouse Keepers/Storm Watch/Play two-player watch")]
        public static void Play() => LighthousePlayLaunch.PlayGame();
        [MenuItem("Lighthouse Keepers/Storm Watch/Build Quest APK")]
        public static void BuildQuest()=>Build(BuildTarget.Android,"Builds/LighthouseKeepers-StormWatch.apk");
        [MenuItem("Lighthouse Keepers/Storm Watch/Build desktop companion")]
        public static void BuildDesktop()=>Build(BuildTarget.StandaloneOSX,"Builds/LighthouseKeepers-StormWatch.app");
        static void Build(BuildTarget target,string output)
        {
            Create();
            Directory.CreateDirectory("Builds");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=LighthouseNetworkLobby.GameScenes,locationPathName=output,target=target,options=BuildOptions.Development});
            File.WriteAllText("Builds/StormWatch-"+target+".txt",$"Result={report.summary.result}\nErrors={report.summary.totalErrors}\nWarnings={report.summary.totalWarnings}\n");
            if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors>0)throw new InvalidOperationException("Storm Watch build failed.");
        }
    }
}
