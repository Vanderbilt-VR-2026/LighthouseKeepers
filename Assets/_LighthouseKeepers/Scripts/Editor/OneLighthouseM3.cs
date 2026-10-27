using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Unity.XR.CoreUtils;
using LighthouseKeepers.WatchDemo;
using LighthouseKeepers.WatchDemo.Editor;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Presentation.Environment;
using Object=UnityEngine.Object;
namespace LighthouseKeepers.Editor
{
    public static class OneLighthouseM3
    {
        public const string Output="artifacts/one/M3";
        const string Art="Assets/WatchDemo/Radio";
        static readonly Dictionary<string,Material> palette=new();
        static Mesh box,cylinder;
        static Transform cabinet;
        static WatchRadioConsole console;
        static readonly List<RadioControl> controls=new();
        static readonly List<Renderer> lamps=new();
        [MenuItem("Lighthouse/Watch/Apply M3 radio foundation")]
        public static void Apply()
        {
            Directory.CreateDirectory(Output);if(!AssetDatabase.IsValidFolder(Art))AssetDatabase.CreateFolder("Assets/WatchDemo","Radio");
            var program=AssetDatabase.LoadAssetAtPath<RadioProgram>("Assets/WatchDemo/Resources/StormRadio.asset");
            if(!program){program=ScriptableObject.CreateInstance<RadioProgram>();AssetDatabase.CreateAsset(program,"Assets/WatchDemo/Resources/StormRadio.asset");}
            var day=AssetDatabase.LoadAssetAtPath<DayDefinition>("Assets/Data/Days/Day03.asset");
            day.tasks=new[]{new WatchTaskStep(WatchState.GeneratorStation,"Restart the generator"),new WatchTaskStep(WatchState.PipeStation,"Repair the pipe"),new WatchTaskStep(WatchState.RadioStation,"Broadcast the storm warning.")};
            day.startingFaults=new[]{WatchState.GeneratorStation,WatchState.PipeStation,WatchState.RadioStation};EditorUtility.SetDirty(day);
            if(!AssetDatabase.LoadAssetAtPath<DayDefinition>("Assets/Data/Days/Day04.asset"))
            {var next=Object.Instantiate(day);next.day=4;next.name="Day04";AssetDatabase.CreateAsset(next,"Assets/Data/Days/Day04.asset");}
            WatchDayAuthoring.RefreshCatalog();
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");
            foreach(string name in new[]{"STN_RADIO_CONSOLE","CodeSource","EmergencyLighting"}){var old=GameObject.Find(name);if(old)Object.DestroyImmediate(old);}
            program=AssetDatabase.LoadAssetAtPath<RadioProgram>("Assets/WatchDemo/Resources/StormRadio.asset");
            PreparePalette();BuildConsole(program);
            var watch=Object.FindFirstObjectByType<WatchDemoController>();Set(watch,"radio",console);
            BuildLighting(watch);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();OneLighthouseM1.ApplyShipBuildScenes();
            File.WriteAllText(Output+"/authoring.txt",$"F4 console root={console.transform.position}, controls={controls.Count}, code source separate=true\nGeneratorRestart/HandRig and accepted generator scene subtree were not rebuilt.\n");
            EditorSceneManager.OpenScene(LighthouseNetworkLobby.ScenePath);
        }
        static void PreparePalette()
        {
            palette.Clear();
            Mat("Cabinet",new Color(.19f,.27f,.28f),.5f);Mat("Face",new Color(.055f,.09f,.11f),.3f);Mat("Brass",new Color(.55f,.39f,.17f),.65f);
            Mat("Steel",new Color(.45f,.51f,.53f),.7f);Mat("Rubber",new Color(.025f,.035f,.04f),0);Mat("Cream",new Color(.76f,.70f,.51f),0);
            Mat("Red",new Color(.8f,.055f,.025f),.1f,true);Mat("Amber",new Color(.9f,.48f,.06f),.1f,true);
            Mat("Blue",new Color(.05f,.28f,.85f),.1f,true);Mat("Green",new Color(.08f,.62f,.3f),.1f,true);
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);box=primitive.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(primitive);
            primitive=GameObject.CreatePrimitive(PrimitiveType.Cylinder);cylinder=primitive.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(primitive);
        }
        static void Mat(string name,Color color,float metal,bool emission=false)
        {
            string path=Art+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",.32f);
            m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*(emission?.3f:.22f));EditorUtility.SetDirty(m);palette[name]=m;
        }
        static Transform Root(string name,Transform parent,Vector3 p)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
        static GameObject Part(string name,Transform parent,Vector3 p,Vector3 size,string material,bool round=false)
        {
            var t=Root(name,parent,p);t.localScale=size;t.gameObject.AddComponent<MeshFilter>().sharedMesh=round?cylinder:box;
            var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=palette[material];r.lightProbeUsage=LightProbeUsage.Off;
            r.shadowCastingMode=ShadowCastingMode.Off;return t.gameObject;
        }
        static TextMesh Label(string name,string text,Transform parent,Vector3 p,float height,Color? color=null)
        {
            var t=Root(name,parent,p);t.localRotation=Quaternion.Euler(0,180,0);var label=t.gameObject.AddComponent<TextMesh>();
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=64;label.characterSize=height;
            label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.text=text;label.color=color??new Color(.84f,.80f,.64f);
            label.GetComponent<Renderer>().sharedMaterial=label.font.material;return label;
        }
        static RadioControl Control(string name,Vector3 p,RadioControl.Kind kind,string colour,int code=-1,bool main=false,bool frequency=false)
        {
            var t=Root(name,console.transform,p);var control=t.gameObject.AddComponent<RadioControl>();
            control.kind=kind;control.codeColour=code;control.mainPower=main;control.frequency=frequency;control.console=console;
            var collider=t.gameObject.AddComponent<BoxCollider>();collider.size=kind==RadioControl.Kind.Knob?new Vector3(.15f,.15f,.12f):new Vector3(.12f,.12f,.10f);
            var simple=t.GetComponent<XRSimpleInteractable>();simple.colliders.Add(collider);t.GetComponent<PhysicalGrip>().Configure(t,HandPose.Pinch);
            var visual=Root("Moving control",t,Vector3.zero);control.visual=visual;
            Part(name+" bezel",cabinet,p-new Vector3(0,0,.055f),new Vector3(.15f,.15f,.025f),"Brass");
            if(kind==RadioControl.Kind.Button)
            {var face=Part("Backlit push cap",visual,Vector3.zero,new Vector3(.10f,.075f,.065f),colour);lamps.Add(face.GetComponent<Renderer>());}
            else if(kind==RadioControl.Kind.Toggle)
            {
                Part("Toggle socket",visual,Vector3.zero,new Vector3(.055f,.03f,.055f),"Steel",true).transform.localRotation=Quaternion.Euler(90,0,0);
                Part("Metal lever",visual,new Vector3(0,0,.06f),new Vector3(.018f,.065f,.018f),"Steel",true).transform.localRotation=Quaternion.Euler(90,0,0);
                Part("Lever tip",visual,new Vector3(0,0,.12f),new Vector3(.035f,.035f,.035f),main?"Red":"Cream");
            }
            else
            {
                float width=frequency?.15f:.11f;
                Part("Knurled knob",visual,Vector3.zero,new Vector3(width,.045f,width),"Rubber",true).transform.localRotation=Quaternion.Euler(90,0,0);
                Part("Index mark",visual,new Vector3(0,width*.28f,.048f),new Vector3(.012f,width*.32f,.007f),"Cream");
            }
            controls.Add(control);return control;
        }
        static void BuildConsole(RadioProgram program,bool guided=false)
        {
            controls.Clear();lamps.Clear();var root=Root("STN_RADIO_CONSOLE",null,new Vector3(-.50f,10.15f,2.32f));root.rotation=Quaternion.Euler(0,180,0);root.localScale=Vector3.one*.7f;
            console=root.gameObject.AddComponent<WatchRadioConsole>();console.program=program;cabinet=Root("Console cabinet",root,Vector3.zero);
            Part("Steel cabinet",cabinet,new Vector3(0,1.42f,.10f),new Vector3(2.22f,1.5f,.24f),"Cabinet");
            Part("Inset face",cabinet,new Vector3(0,1.42f,.235f),new Vector3(2.10f,1.37f,.035f),"Face");
            for(int i=0;i<8;i++)Part("Vent slot",cabinet,new Vector3(-.89f+i*.075f,1.96f,.258f),new Vector3(.045f,.07f,.008f),"Rubber");
            for(int x=-1;x<=1;x+=2)for(int y=0;y<2;y++)Part("Wall mounting bracket",cabinet,new Vector3(x*.88f,.83f+y*1.18f,-.25f),new Vector3(.10f,.12f,.7f),"Steel");
            var collision=root.gameObject.AddComponent<BoxCollider>();collision.center=new Vector3(0,1.42f,.1f);collision.size=new Vector3(2.22f,1.50f,.24f);
            Label("Radio header","KEEPER COASTAL  /  STORM TRANSMITTER",root,new Vector3(0,2.08f,.27f),.006f);
            Part("Frequency dial rim",cabinet,new Vector3(-.48f,1.70f,.27f),new Vector3(1.02f,.36f,.035f),"Brass");
            Part("Frequency dial face",cabinet,new Vector3(-.48f,1.70f,.293f),new Vector3(.96f,.31f,.016f),"Cream");
            for(int i=0;i<=20;i++)
            {
                float x=-.92f+i*.044f;
                Part("Frequency graduation",cabinet,new Vector3(x,1.78f,.307f),new Vector3(.006f,i%5==0?.055f:.025f,.006f),"Rubber");
            }
            for(int i=0;i<5;i++)Label("Band scale "+i,(156+i*.5f).ToString("F1",System.Globalization.CultureInfo.InvariantCulture),root,new Vector3(-.04f-i*.22f,1.67f,.313f),.004f,Color.black);
            var needle=Root("Frequency needle",root,new Vector3(-.48f,1.54f,.32f));console.frequencyNeedle=needle;
            Part("Tuning needle",needle,new Vector3(0,.115f,0),new Vector3(.009f,.23f,.009f),"Red");
            console.meterNeedles=new Transform[2];
            for(int i=0;i<2;i++)
            {
                float x=.38f+i*.43f;Part("VU meter bezel",cabinet,new Vector3(x,1.81f,.267f),new Vector3(.37f,.31f,.04f),"Steel");
                Part("VU meter face",cabinet,new Vector3(x,1.81f,.293f),new Vector3(.33f,.26f,.014f),"Cream");
                Label("Meter scale "+i,"-20  -10  0  +3\n"+(i==0?"MODULATION":"OUTPUT"),root,new Vector3(x,1.84f,.306f),.0032f,Color.black);
                var pivot=Root("VU needle "+i,root,new Vector3(x,1.70f,.315f));console.meterNeedles[i]=pivot;
                Part("Meter needle",pivot,new Vector3(0,.075f,0),new Vector3(.005f,.15f,.006f),"Rubber");
            }
            Part("CRT surround",cabinet,new Vector3(.52f,1.48f,.27f),new Vector3(.98f,.30f,.04f),"Steel");
            Part("CRT dark screen",cabinet,new Vector3(.52f,1.48f,.296f),new Vector3(.91f,.25f,.015f),"Rubber");
            console.readout=Label("Radio readout","NO POWER",root,new Vector3(.52f,1.48f,.311f),.007f,new Color(.24f,1,.48f));
            Control("MAIN power",new Vector3(-.91f,1.27f,.32f),RadioControl.Kind.Toggle,"Red",main:true);
            Label("Main label","MAIN\nPOWER",root,new Vector3(-.91f,1.42f,.28f),.004f);
            Control("Frequency knob",new Vector3(-.50f,1.27f,.32f),RadioControl.Kind.Knob,"Rubber",frequency:true);
            Control("Fine trim",new Vector3(-.23f,1.27f,.32f),RadioControl.Kind.Knob,"Rubber");
            Control("Gain",new Vector3(.02f,1.27f,.32f),RadioControl.Kind.Knob,"Rubber");
            Label("Tune label","TUNE",root,new Vector3(-.50f,1.12f,.28f),.005f);
            Label("Trim label","TRIM",root,new Vector3(-.23f,1.12f,.28f),.005f);
            Label("Gain label","GAIN",root,new Vector3(.02f,1.12f,.28f),.005f);
            string[] colours={"Red","Amber","Blue","Green"};
            for(int i=0;i<4;i++){Control("Code "+i,new Vector3(.36f+i*.20f,1.15f,.32f),RadioControl.Kind.Button,colours[i],i);Label("Code label "+i,colours[i].ToUpperInvariant(),root,new Vector3(.36f+i*.20f,1.05f,.28f),.0045f);}
            for(int row=0;row<2;row++)for(int i=0;i<4;i++)
            {Control("Channel "+(row*4+i+1),new Vector3(-.9f+i*.23f,.83f+row*.15f,.32f),RadioControl.Kind.Button,colours[(i+row)%4]);}
            if(!guided)for(int i=0;i<6;i++)Control("Auxiliary switch "+i,new Vector3(.10f+i*.17f,.82f,.32f),RadioControl.Kind.Toggle,"Steel");
            Label("Utility labels","CHANNEL BANK                  MONITOR / AUXILIARY",root,new Vector3(0,.69f,.28f),.0037f);
            console.lockLamp=Part("Tune lock lamp",root,new Vector3(.29f,1.98f,.29f),new Vector3(.07f,.045f,.03f),"Green").GetComponent<Renderer>();
            console.transmitLamp=Part("Transmission lamp",root,new Vector3(.72f,1.98f,.29f),new Vector3(.07f,.045f,.03f),"Red").GetComponent<Renderer>();
            Label("Lock label","LOCK",root,new Vector3(.29f,2.015f,.28f),.004f);
            Label("TX label","TX",root,new Vector3(.72f,2.015f,.28f),.004f);
            console.controls=controls.ToArray();console.panelLamps=lamps.ToArray();
            var source=Root("CodeSource",null,root.TransformPoint(new Vector3(0,.43f,.35f)));source.rotation=root.rotation;source.localScale=root.localScale;
            Part("Call card backing",source,Vector3.zero,new Vector3(1.30f,.30f,.025f),"Brass");
            var clue=Label("Solo call card","",source,new Vector3(0,0,.02f),.006f);
            var code=source.gameObject.AddComponent<CodeSource>();Set(code,"program",program);Set(code,"readout",clue);code.Refresh();
            BuildMicrophone(root);Combine(cabinet,"ConsoleBody");
            console.effects=Audio(root,"Control audio",null,.55f,false);
            console.click=Clip("click",.07f,0);console.buzzer=Clip("buzzer",.22f,1);console.reply=Clip("reply",1.6f,2);
            console.hiss=Audio(root,"Tuning static",Clip("static",2,3),0,true);
            console.tone=Audio(root,"Tuning tone",Clip("tone",2,4),0,true);
            console.chatter=Audio(root,"Transmission chatter",Clip("chatter",2,5),.25f,true);console.chatter.playOnAwake=false;
        }
        public static WatchRadioConsole RebuildRadioForM4()
        {
            foreach(string name in new[]{"STN_RADIO_CONSOLE","CodeSource"}){var old=GameObject.Find(name);if(old)Object.DestroyImmediate(old);}
            PreparePalette();BuildConsole(AssetDatabase.LoadAssetAtPath<RadioProgram>("Assets/WatchDemo/Resources/StormRadio.asset"),true);return console;
        }
        static void BuildMicrophone(Transform root)
        {
            var socket=Root("Microphone cord socket",root,new Vector3(1.06f,1.03f,.25f));
            var mic=Root("GRAB_BroadcastMicrophone",root,new Vector3(1.22f,1.35f,.30f));
            var visual=Root("Microphone shell",mic,Vector3.zero);
            Part("Mic handle",visual,new Vector3(0,-.05f,0),new Vector3(.045f,.07f,.045f),"Rubber",true);
            Part("Mic capsule",visual,new Vector3(0,.045f,0),new Vector3(.085f,.11f,.055f),"Steel");
            for(int i=0;i<7;i++)Part("Mic grille",visual,new Vector3(0,.005f+i*.013f,.03f),new Vector3(.07f,.006f,.008f),"Rubber");
            Combine(visual,"Microphone");
            var body=mic.gameObject.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
            var collider=mic.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(.085f,.23f,.075f);
            var grab=mic.gameObject.AddComponent<XRGrabInteractable>();grab.colliders.Add(collider);grab.throwOnDetach=false;grab.movementType=XRBaseInteractable.MovementType.Kinematic;
            grab.attachTransform=Root("Microphone palm attach",mic,new Vector3(0,-.045f,0));grab.attachTransform.localRotation=Quaternion.Euler(90,0,0);grab.useDynamicAttach=false;
            var component=mic.gameObject.AddComponent<RadioMicrophone>();component.cordSocket=socket;console.microphone=component;
            var line=Root("Coiled microphone cord",root,Vector3.zero).gameObject.AddComponent<LineRenderer>();line.useWorldSpace=true;line.widthMultiplier=.008f;line.numCapVertices=2;line.sharedMaterial=palette["Rubber"];line.shadowCastingMode=ShadowCastingMode.Off;component.cord=line;
            Label("Mic label","MIC / HOLD TRIGGER",root,new Vector3(.78f,.57f,.28f),.0038f);
        }
        static void Combine(Transform parent,string name)
        {
            var filters=parent.GetComponentsInChildren<MeshFilter>();int group=0;
            foreach(var materialGroup in filters.GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial))
            {
                var mesh=new Mesh{name=name+group};mesh.CombineMeshes(materialGroup.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=parent.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());
                string path=Art+"/"+name+group+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
                var t=Root(name+" / "+materialGroup.Key.name,parent,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=materialGroup.Key;r.lightProbeUsage=LightProbeUsage.Off;r.shadowCastingMode=ShadowCastingMode.Off;group++;
            }
            foreach(var f in filters)Object.DestroyImmediate(f.gameObject);
        }
        static AudioSource Audio(Transform root,string name,AudioClip clip,float volume,bool loop)
        {
            var source=Root(name,root,new Vector3(0,1.5f,.35f)).gameObject.AddComponent<AudioSource>();source.clip=clip;source.volume=volume;source.loop=loop;
            source.playOnAwake=loop;source.spatialBlend=1;source.minDistance=.7f;source.maxDistance=10;return source;
        }
        static AudioClip Clip(string name,float duration,int kind)
        {
            string path=Art+"/"+name+".wav";if(!File.Exists(path))
            {
                const int rate=22050;int count=(int)(duration*rate);var random=new System.Random(731);
                using(var writer=new BinaryWriter(File.Create(path)))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
                    for(int i=0;i<count;i++)
                    {
                        double t=(double)i/rate,noise=random.NextDouble()*2-1;
                        double value=kind switch{0=>noise*Math.Exp(-t*90)*.4,1=>Math.Sin(2*Math.PI*170*t)*.22,2=>Math.Sin(2*Math.PI*(510+70*Math.Sin(t*9))*t)*(.15+.1*Math.Sin(t*22)),3=>noise*.25,4=>Math.Sin(2*Math.PI*660*t)*.18,5=>(Math.Sin(2*Math.PI*173*t)+.4*Math.Sin(2*Math.PI*520*t))*(.1+.08*Math.Sin(t*37)),_=>Math.Sin(2*Math.PI*145*t)*(.10+.025*Math.Sin(t*Math.PI*2/1.5))};
                        float envelope=loopKind(kind)?1:Mathf.Min(1,(float)t/.01f,(float)(duration-t)/.04f);writer.Write((short)(Math.Clamp(value*envelope,-1,1)*32767));
                    }
                }
                AssetDatabase.ImportAsset(path);
            }
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        static bool loopKind(int kind)=>kind==3||kind==4||kind==5||kind==6;
        static void BuildLighting(WatchDemoController watch)
        {
            var root=new GameObject("EmergencyLighting");var emergency=root.AddComponent<EmergencyLighting>();
            Set(emergency,"bakedLighting",Object.FindFirstObjectByType<BeaconAndMainsLightingGroup>());
            // Existing F1/F2 service lights and fixtures stay. Add bounded shadowless fill for the remaining occupied floors.
            foreach(var item in new[]{("House",new Vector3(0,2.2f,-7.5f)),("F3",new Vector3(0,8.9f,0)),("F4",new Vector3(0,12.2f,0))})
            {var l=Root("Central service light "+item.Item1,root.transform,item.Item2).gameObject.AddComponent<Light>();l.type=LightType.Point;l.range=5;l.intensity=1.2f;l.color=new Color(1,.78f,.52f);l.shadows=LightShadows.None;}
            var surfaces=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>Interior(r.bounds.center)&&r.bounds.size.x<12&&r.bounds.size.z<12 &&
                !r.GetComponent<TextMesh>()&&!r.GetComponent<TMPro.TMP_Text>()&&!r.GetComponentInParent<XROrigin>()&&!r.GetComponentInParent<XRGrabInteractable>()&&!r.GetComponentInParent<PhysicalGrip>()&&
                !r.GetComponentInParent<WatchRadioConsole>()&&!r.GetComponentInParent<CodeSource>()&&r.sharedMaterial&&r.sharedMaterial.name!="GeneratorLamp"&&r.sharedMaterial.name!="TaskBoardWhite").ToArray();
            Array(emergency,"interiorSurfaces",surfaces);
            Array(emergency,"interiorLights",Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.type!=LightType.Directional&&Interior(l.transform.position)).ToArray());
            var alarm=Audio(root.transform,"Low emergency alarm",Clip("alarm",3,6),.15f,true);alarm.playOnAwake=false;alarm.spatialBlend=0;
            Set(emergency,"alarm",alarm);Set(watch,"emergencyLighting",emergency);
        }
        public static bool Interior(Vector3 p)
        {if(p.y<-.5f||p.y>13.5f)return false;if(p.z<-4.3f)return p.z>-10.6f&&Mathf.Abs(p.x)<4.3f&&p.y<3.5f;float radius=p.y<3.3f?4.45f:p.y<6.6f?4.2f:p.y<9.9f?4.05f:3.15f;return new Vector2(p.x,p.z).magnitude<radius;}
        static void Set(Object target,string field,Object value){var s=new SerializedObject(target);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        static void Array<T>(Object target,string field,T[] values)where T:Object
        {var s=new SerializedObject(target);var a=s.FindProperty(field);a.arraySize=values.Length;for(int i=0;i<values.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=values[i];s.ApplyModifiedPropertiesWithoutUndo();}
        public static void Build()
        {
            Directory.CreateDirectory(Output);Directory.CreateDirectory("Builds");OneLighthouseM1.ApplyShipBuildScenes();
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=LighthouseNetworkLobby.GameScenes,target=BuildTarget.Android,locationPathName="Builds/LighthouseKeepers-M3.apk",options=BuildOptions.Development});
            File.WriteAllText(Output+"/build.txt",$"{report.summary.result}\nErrors={report.summary.totalErrors}\nWarnings={report.summary.totalWarnings}\nDuration={report.summary.totalTime}\n");
            if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new BuildFailedException("M3 build failed");
        }
    }
}
