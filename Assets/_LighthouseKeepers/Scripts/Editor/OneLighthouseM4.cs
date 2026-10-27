using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Unity.XR.CoreUtils;
using LighthouseKeepers.WatchDemo;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Gameplay.Interaction;
using Object=UnityEngine.Object;
namespace LighthouseKeepers.Editor
{
    public static class OneLighthouseM4
    {
        public const string Output="artifacts/one/M4";
        const string Art="Assets/WatchDemo/Polish";
        static Material steel,brass,amber,dark,lens;
        static Mesh cube,cylinder;
        public static void Apply()
        {
            Directory.CreateDirectory(Output);
            if(!AssetDatabase.IsValidFolder(Art))AssetDatabase.CreateFolder("Assets/WatchDemo","Polish");
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");
            if(!File.Exists(Output+"/before-survey.txt"))File.WriteAllLines(Output+"/before-survey.txt",Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.bounds.center.y>9.85f&&r.bounds.center.y<13.3f).Select(r=>r.name+" @ "+r.bounds.center));
            foreach(string name in new[]{"STN_PIPE_REPAIR","Powered lighthouse beacon"}){var old=GameObject.Find(name);if(old)Object.DestroyImmediate(old);}
            var profile=AssetDatabase.LoadAssetAtPath<PipeRepairProfile>("Assets/WatchDemo/Resources/PipeRepair.asset");
            if(!profile){profile=ScriptableObject.CreateInstance<PipeRepairProfile>();AssetDatabase.CreateAsset(profile,"Assets/WatchDemo/Resources/PipeRepair.asset");}
            foreach(int day in new[]{2,3,4}){var d=AssetDatabase.LoadAssetAtPath<DayDefinition>($"Assets/Data/Days/Day{day:00}.asset");d.beaconDarkLimitSeconds=day==2?90:75;EditorUtility.SetDirty(d);}
            steel=Mat("Steel",new Color(.28f,.33f,.35f),.03f,.6f);brass=Mat("Brass",new Color(.55f,.4f,.18f),.06f,.6f);
            amber=Mat("Amber",new Color(1,.55f,.12f),.3f);dark=Mat("Panel",new Color(.06f,.1f,.11f),.2f);lens=Mat("FresnelAmber",new Color(.7f,.4f,.15f),0,.2f);
            var p=GameObject.CreatePrimitive(PrimitiveType.Cube);cube=p.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(p);
            p=GameObject.CreatePrimitive(PrimitiveType.Cylinder);cylinder=p.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(p);
            var watch=Object.FindFirstObjectByType<WatchDemoController>();
            BuildPipe(watch);BuildBeacon(watch);BuildRadio(watch);ConfigureLighting();
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();OneLighthouseM1.ApplyShipBuildScenes();
            File.WriteAllText(Output+"/authoring.txt","Three physical pipe couplings; 16 radio controls + four-step strip; warm emergency; central power-driven beacon; D2 90s/D3 75s dark deadlines.\n");
            EditorSceneManager.OpenScene(LighthouseNetworkLobby.ScenePath);
        }
        static Material Mat(string name,Color color,float emission,float metal=0)
        {string path=Art+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",.3f);m.EnableKeyword("_EMISSION");m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;m.SetColor("_EmissionColor",color*emission);EditorUtility.SetDirty(m);return m;}
        static Transform Root(string name,Transform parent,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
        static GameObject Part(string name,Transform parent,Vector3 p,Vector3 scale,Material material,bool round=false)
        {var t=Root(name,parent,p);t.localScale=scale;t.gameObject.AddComponent<MeshFilter>().sharedMesh=round?cylinder:cube;var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.lightProbeUsage=LightProbeUsage.Off;r.shadowCastingMode=ShadowCastingMode.Off;return t.gameObject;}
        static TextMesh Label(string name,string text,Transform parent,Vector3 p,float size)
        {var t=Root(name,parent,p);t.localRotation=Quaternion.Euler(0,180,0);var label=t.gameObject.AddComponent<TextMesh>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.text=text;label.color=new Color(.95f,.88f,.68f);label.GetComponent<Renderer>().sharedMaterial=label.font.material;return label;}
        static void Set(Object target,string name,Object value){var s=new SerializedObject(target);s.FindProperty(name).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        static T Get<T>(Object target,string name)where T:Object=>(T)new SerializedObject(target).FindProperty(name).objectReferenceValue;
        static void Array<T>(Object target,string name,T[] values)where T:Object{var s=new SerializedObject(target);var a=s.FindProperty(name);a.arraySize=values.Length;for(int i=0;i<values.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=values[i];s.ApplyModifiedPropertiesWithoutUndo();}
        static AudioClip Clip(string name,bool rising)
        {
            string path=Art+"/"+name+".wav";if(!File.Exists(path))
            {const int rate=22050;int count=rate;using var w=new BinaryWriter(File.Create(path));w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);double phase=0;for(int i=0;i<count;i++){double t=(double)i/rate;phase+=2*Math.PI*(rising?90+220*t:310-220*t)/rate;w.Write((short)(Math.Sin(phase)*Math.Sin(t*Math.PI)*5000));}}
            AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        static void BuildPipe(WatchDemoController watch)
        {
            var root=Root("STN_PIPE_REPAIR",null,Vector3.zero);var pipe=root.gameObject.AddComponent<WatchPipeRepair>();
            pipe.wrench=Get<XRGrabInteractable>(watch,"wrench");pipe.head=Get<Transform>(watch,"head");pipe.prompt=Get<TextMesh>(watch,"repairPrompt");pipe.prompt.characterSize=.012f;
            pipe.jaw=pipe.wrench.transform.Find("Wrench jaw contact");if(!pipe.jaw)pipe.jaw=Root("Wrench jaw contact",pipe.wrench.transform,new Vector3(0,0,.105f));
            pipe.bolts=new Transform[3];pipe.indicators=new Renderer[3];pipe.sprays=new ParticleSystem[3];
            var positions=new[]{new Vector3(-.12f,.82f,-.03f),new Vector3(.8f,1.05f,-.08f),new Vector3(1.65f,1.35f,.02f)};
            var axes=new[]{new Vector3(0,0,-1),new Vector3(0,.25f,-1),new Vector3(-.25f,.18f,-1)};
            var oldLeak=Get<ParticleSystem>(watch,"leak");oldLeak.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var oldJoint=GameObject.Find("Leaking centre coupling");if(oldJoint)oldJoint.GetComponent<Renderer>().enabled=false;
            for(int i=0;i<3;i++)
            {
                var t=Root("Coupling "+(char)('A'+i),root,positions[i]);t.rotation=Quaternion.LookRotation(axes[i]);pipe.bolts[i]=t;
                var collar=Part("Flanged coupling",t,new Vector3(0,0,-.09f),new Vector3(.24f,.10f,.24f),steel,true);collar.transform.localRotation=Quaternion.Euler(90,0,0);
                var ring=Part("Active bolt indicator",t,new Vector3(0,0,-.005f),new Vector3(.10f,.02f,.10f),amber,true);ring.transform.localRotation=Quaternion.Euler(90,0,0);pipe.indicators[i]=ring.GetComponent<Renderer>();
                var bolt=Part("Ratchet bolt",t,new Vector3(0,0,.025f),new Vector3(.037f,.037f,.045f),brass);bolt.transform.localRotation=Quaternion.Euler(0,0,45);
                Label("Coupling marker",((char)('A'+i)).ToString(),t,new Vector3(.12f,0,.02f),.012f);
                pipe.sprays[i]=Object.Instantiate(oldLeak,t);pipe.sprays[i].name="Coupling spray "+i;pipe.sprays[i].transform.localPosition=Vector3.zero;pipe.sprays[i].transform.localRotation=Quaternion.Euler(30,0,0);
                foreach(var a in pipe.sprays[i].GetComponents<AudioSource>())Object.DestroyImmediate(a);
                var main=pipe.sprays[i].main;main.playOnAwake=false;
                if(i!=1){float y=(positions[i].y+1.05f)*.5f;Part("Pipe branch "+i,root,new Vector3(positions[i].x,y,.15f),new Vector3(.16f,Mathf.Abs(positions[i].y-1.05f)*.5f+.09f,.16f),steel,true);}
            }
            pipe.audioSource=root.gameObject.AddComponent<AudioSource>();pipe.audioSource.spatialBlend=1;pipe.audioSource.playOnAwake=false;pipe.audioSource.minDistance=.5f;pipe.audioSource.maxDistance=8;pipe.audioSource.transform.position=Vector3.zero;
            pipe.ratchetClick=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/WatchDemo/Radio/click.wav");
            var sound=Root("Pipe leak sound",root,new Vector3(.8f,1,.15f)).gameObject.AddComponent<AudioSource>();sound.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_LighthouseKeepers/Audio/Ambience/Placeholder_Drip.wav");sound.loop=true;sound.playOnAwake=false;sound.spatialBlend=1;sound.volume=.25f;sound.minDistance=.5f;sound.maxDistance=10;pipe.leakAudio=sound;
            Set(watch,"pipeRepair",pipe);
        }
        static void BuildBeacon(WatchDemoController watch)
        {
            var root=Root("Powered lighthouse beacon",null,new Vector3(0,9.9f,0));var beacon=root.gameObject.AddComponent<WatchBeacon>();
            var body=Part("Beacon drive housing",root,new Vector3(0,.4f,0),new Vector3(.72f,.4f,.72f),steel,true);var col=body.AddComponent<BoxCollider>();col.size=Vector3.one;
            beacon.rotor=Root("Rotating Fresnel assembly",root,new Vector3(0,.85f,0));beacon.lenses=new Renderer[9];
            for(int i=0;i<9;i++){float radius=.80f-Mathf.Abs(i-4)*.065f;beacon.lenses[i]=Part("Fresnel prism ring",beacon.rotor,new Vector3(0,i*.11f,0),new Vector3(radius,.025f,radius),lens,true).GetComponent<Renderer>();}
            for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;Part("Lens brass frame",beacon.rotor,new Vector3(Mathf.Sin(a)*.45f,.44f,Mathf.Cos(a)*.45f),new Vector3(.035f,1,.035f),brass);}
            Part("Lantern cap",root,new Vector3(0,1.85f,0),new Vector3(1,.04f,1),brass,true);
            beacon.lantern=Root("Powered lantern light",beacon.rotor,new Vector3(0,.45f,0)).gameObject.AddComponent<Light>();beacon.lantern.type=LightType.Point;beacon.lantern.range=4;beacon.lantern.color=new Color(1,.68f,.3f);beacon.lantern.shadows=LightShadows.None;
            beacon.beam=Root("Powered rotating beacon beam",beacon.rotor,new Vector3(0,.45f,.4f)).gameObject.AddComponent<Light>();beacon.beam.type=LightType.Spot;beacon.beam.range=45;beacon.beam.spotAngle=27;beacon.beam.color=new Color(1,.75f,.4f);beacon.beam.shadows=LightShadows.None;
            beacon.audioSource=root.gameObject.AddComponent<AudioSource>();beacon.audioSource.spatialBlend=.6f;beacon.audioSource.minDistance=2;beacon.audioSource.maxDistance=30;beacon.audioSource.playOnAwake=false;
            beacon.powerDown=Clip("beacon-power-down",false);beacon.powerUp=Clip("beacon-spin-up",true);Set(watch,"beacon",beacon);
        }
        static void BuildRadio(WatchDemoController watch)
        {
            var radio=OneLighthouseM3.RebuildRadioForM4();Set(watch,"radio",radio);var root=radio.transform;
            Part("Step strip backing",root,new Vector3(0,2.235f,.23f),new Vector3(2.2f,.34f,.06f),steel);
            radio.stepLamps=new Renderer[4];radio.stepLabels=new TextMesh[4];string[] steps={"1 POWER","2 TUNE","3 CODE","4 TRANSMIT"};
            for(int i=0;i<4;i++){float x=.81f-i*.54f;radio.stepLamps[i]=Part("Step "+i,root,new Vector3(x,2.3f,.27f),new Vector3(.50f,.19f,.03f),amber).GetComponent<Renderer>();radio.stepLabels[i]=Label(steps[i],steps[i],root,new Vector3(x,2.3f,.292f),.0063f);}
            radio.controlHighlights=new Renderer[radio.controls.Length];
            for(int i=0;i<radio.controls.Length;i++)radio.controlHighlights[i]=Part("Step guide dot",radio.controls[i].transform,new Vector3(0,.085f,.025f),new Vector3(.025f,.025f,.012f),amber).GetComponent<Renderer>();
            radio.microphoneHighlight=Part("Microphone step guide",root,new Vector3(1.22f,1.52f,.30f),new Vector3(.08f,.035f,.025f),amber).GetComponent<Renderer>();
            Part("Auxiliary instrument panel",root,new Vector3(.51f,.83f,.285f),new Vector3(1.06f,.24f,.04f),steel);
            for(int i=0;i<3;i++){float x=.16f+i*.34f;Part("Passive service meter",root,new Vector3(x,.85f,.312f),new Vector3(.28f,.14f,.015f),dark);Label("Service meter "+i,new[]{"VOLTS\n220","SIGNAL\nREADY","COAST\nCH 16"}[i],root,new Vector3(x,.85f,.327f),.0039f);}
            var utility=root.Find("Utility labels");if(utility)Object.DestroyImmediate(utility.gameObject);
            var code=Object.FindFirstObjectByType<CodeSource>();var text=Get<TextMesh>(code,"readout");text.characterSize=.009f;code.Refresh();
        }
        static void ConfigureLighting()
        {
            var emergency=Object.FindFirstObjectByType<EmergencyLighting>();var s=new SerializedObject(emergency);s.FindProperty("cycleSeconds").floatValue=2;s.ApplyModifiedPropertiesWithoutUndo();
            var surfaces=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>OneLighthouseM3.Interior(r.bounds.center)&&r.bounds.size.x<12&&r.bounds.size.z<12&&!r.GetComponent<TextMesh>()&&!r.GetComponent<TMPro.TMP_Text>()&&!r.GetComponentInParent<XROrigin>()&&!r.GetComponentInParent<XRGrabInteractable>()&&!r.GetComponentInParent<PhysicalGrip>()&&!r.GetComponentInParent<WatchRadioConsole>()&&!r.GetComponentInParent<CodeSource>()&&!r.GetComponentInParent<WatchBeacon>()&&!r.GetComponentInParent<WatchPipeRepair>()&&r.sharedMaterial&&r.sharedMaterial.name!="GeneratorLamp"&&r.sharedMaterial.name!="TaskBoardWhite").ToArray();
            Array(emergency,"interiorSurfaces",surfaces);Array(emergency,"interiorLights",Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.type!=LightType.Directional&&OneLighthouseM3.Interior(l.transform.position)&&!l.GetComponentInParent<WatchBeacon>()).ToArray());
        }
        public static void Build()
        {
            Directory.CreateDirectory(Output);OneLighthouseM1.ApplyShipBuildScenes();
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=LighthouseNetworkLobby.GameScenes,target=BuildTarget.Android,locationPathName="Builds/LighthouseKeepers-M4.apk",options=BuildOptions.Development});
            File.WriteAllText(Output+"/build.txt",$"{report.summary.result}\nErrors={report.summary.totalErrors}\nWarnings={report.summary.totalWarnings}\nDuration={report.summary.totalTime}\n");
            if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new BuildFailedException("M4 build failed");
        }
    }
}
