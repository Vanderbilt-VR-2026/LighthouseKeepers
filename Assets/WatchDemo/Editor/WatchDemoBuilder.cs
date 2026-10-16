using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Editor.Environment;
namespace LighthouseKeepers.WatchDemo.Editor
{
    public static class WatchDemoBuilder
    {
        const string Root="Assets/WatchDemo/";
        static Material metal,wood,cloth,white,red;
        static Transform parent;
        static void Set(UnityEngine.Object o,string field,UnityEngine.Object value){var s=new SerializedObject(o);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        static Material Mat(string name,Color color){var p=Root+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=color;return m;}
        static GameObject Part(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material mat,bool collision=true)
        {var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=position;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());return o;}
        static TextMesh Text(string name,string value,Vector3 position,float size,Transform root=null,int fontSize=28,TextAnchor anchor=TextAnchor.UpperCenter,TextAlignment alignment=TextAlignment.Center,FontStyle style=FontStyle.Normal)
        {
            var t=new GameObject(name).AddComponent<TextMesh>();t.transform.SetParent(root?root:parent,false);t.transform.localPosition=position;
            t.text=value;t.fontSize=fontSize;t.characterSize=size;t.anchor=anchor;t.alignment=alignment;t.fontStyle=style;t.color=new Color(.90f,.90f,.84f);return t;
        }
        static GameObject HudBar(Transform root,string name,Vector3 position,Vector3 size,Material material)
        {
            var bar=GameObject.CreatePrimitive(PrimitiveType.Cube);bar.name=name;UnityEngine.Object.DestroyImmediate(bar.GetComponent<Collider>());
            bar.transform.SetParent(root,false);bar.transform.localPosition=position;bar.transform.localScale=size;bar.GetComponent<Renderer>().sharedMaterial=material;return bar;
        }
        static void StairwellLiner(string id,float floorY,float innerRadius,float startAngle,float endAngle)
        {
            // A solid backing directly behind the inner rails prevents the exposed open shaft from showing ocean/sky through every flight.
            const float segmentDegrees=5f;int count=Mathf.CeilToInt(Mathf.Abs(endAngle-startAngle)/segmentDegrees);
            float direction=Mathf.Sign(endAngle-startAngle);
            for(int i=0;i<count;i++)
            {
                float a=startAngle+direction*(i+.5f)*Mathf.Abs(endAngle-startAngle)/count;
                float span=Mathf.Abs(endAngle-startAngle)/count*Mathf.Deg2Rad;
                float length=2f*innerRadius*Mathf.Sin(span*.5f)+.02f;
                var panel=Part("Stairwell liner "+id+" "+i,PrimitiveType.Cube,LighthouseBuilder.Polar(a,innerRadius,floorY+1.63f),new Vector3(length,3.26f,.07f),wood,false);
                panel.transform.localRotation=Quaternion.Euler(0,a,0);
            }
        }
        static void SaveMesh(Mesh mesh,string path){var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);}else AssetDatabase.CreateAsset(mesh,path);}
        public static void Build()
        {
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
            var old=GameObject.Find("Watch demo");if(old)UnityEngine.Object.DestroyImmediate(old);
            parent=new GameObject("Watch demo").transform;
            metal=Mat("Metal",new Color(.23f,.28f,.31f));wood=Mat("Wood",new Color(.22f,.15f,.09f));cloth=Mat("Blanket",new Color(.14f,.23f,.18f));white=Mat("Pillow",new Color(.65f,.65f,.55f));red=Mat("LeakJoint",new Color(.33f,.13f,.08f));
            // Halve fixture glow in this preview without editing shared shell materials.
            foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var materials=r.sharedMaterials;bool changed=false;
                for(int i=0;i<materials.Length;i++)
                {
                    var source=materials[i];if(!source||(source.name!="M_Environment_Lamp"&&source.name!="M_Environment_Emergency"))continue;
                    string path=Root+source.name+"_Half.mat";var half=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(!half){half=new Material(source);AssetDatabase.CreateAsset(half,path);}
                    half.SetColor("_EmissionColor",source.GetColor("_EmissionColor")*.5f);materials[i]=half;changed=true;
                }
                if(changed)r.sharedMaterials=materials;
            }
            var controller=parent.gameObject.AddComponent<WatchDemoController>();
            var flood=new GameObject("Eli flood authority — local watch").AddComponent<FloodController>();flood.transform.SetParent(parent);
            string profilePath=Root+"WatchFlood.asset";var profile=AssetDatabase.LoadAssetAtPath<FloodProfile>(profilePath);if(!profile){profile=ScriptableObject.CreateInstance<FloodProfile>();AssetDatabase.CreateAsset(profile,profilePath);}
            var fp=new SerializedObject(profile);fp.FindProperty("minimumHeight").floatValue=-.03f;fp.FindProperty("maximumHeight").floatValue=.65f;fp.FindProperty("riseMetresPerSecond").floatValue=.0015f;fp.FindProperty("surgePerMetre").floatValue=.1f;fp.ApplyModifiedPropertiesWithoutUndo();Set(flood,"profile",profile);Set(controller,"flood",flood);
            var water=new GameObject("Visible first-floor flood");water.transform.SetParent(parent);water.transform.localPosition=new Vector3(0,.02f,0);var mesh=new Mesh{name="First floor water disc"};var v=new Vector3[65];var tri=new int[64*3];for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;v[i+1]=new Vector3(Mathf.Cos(a)*3.83f,0,Mathf.Sin(a)*3.83f);tri[i*3]=0;tri[i*3+1]=(i+1)%64+1;tri[i*3+2]=i+1;}mesh.vertices=v;mesh.triangles=tri;mesh.RecalculateNormals();SaveMesh(mesh,Root+"WaterDisc.asset");water.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"WaterDisc.asset");var waterMaterial=Mat("FloodWater",new Color(.14f,.28f,.30f));waterMaterial.shader=Shader.Find("Universal Render Pipeline/Unlit");water.AddComponent<MeshRenderer>().sharedMaterial=waterMaterial;water.AddComponent<FloodSurface>();
            // A wall-mounted whiteboard is readable from the house spawn; its desk sits directly in front without blocking the tower throat.
            var boardFramePosition=new Vector3(-2.20f,1.68f,-7.50f);
            Part("Wall-mounted checklist brown frame",PrimitiveType.Cube,boardFramePosition,new Vector3(1.95f,1.22f,.10f),wood);
            var boardWhite=Mat("TaskBoardWhite",new Color(.89f,.86f,.75f));
            Part("Wall-mounted checklist white face",PrimitiveType.Cube,boardFramePosition+new Vector3(0,0,-.065f),new Vector3(1.76f,1.02f,.024f),boardWhite,false);
            var board=Text("Watch checklist","KEEPER WATCH",new Vector3(-2.97f,2.10f,-7.58f),.017f,null,34,TextAnchor.UpperLeft,TextAlignment.Left,FontStyle.Italic);board.color=new Color(.12f,.17f,.14f);Set(controller,"board",board);
            Part("Wrench table top",PrimitiveType.Cube,new Vector3(-2.15f,.82f,-8.20f),new Vector3(1.62f,.12f,.70f),wood);
            foreach(float x in new[]{-2.82f,-1.48f})foreach(float z in new[]{-8.46f,-7.94f})Part("Wrench table leg",PrimitiveType.Cube,new Vector3(x,.38f,z),new Vector3(.09f,.76f,.09f),wood);
            var tool=new GameObject("Grab wrench — grip to hold");tool.transform.SetParent(parent);tool.transform.position=new Vector3(-2.15f,.98f,-8.20f);
            var visual=new GameObject("Wrench visual").transform;visual.SetParent(tool.transform,false);visual.localScale=Vector3.one*1.45f;var wrenchMetal=Mat("WrenchMetal",new Color(.50f,.54f,.52f));var saved=parent;parent=visual;
            Part("Steel handle",PrimitiveType.Cube,Vector3.zero,new Vector3(.035f,.025f,.24f),wrenchMetal,false);
            Part("Open jaw left",PrimitiveType.Cube,new Vector3(-.035f,0,.135f),new Vector3(.025f,.03f,.08f),wrenchMetal,false);Part("Open jaw right",PrimitiveType.Cube,new Vector3(.035f,0,.135f),new Vector3(.025f,.03f,.08f),wrenchMetal,false);Part("Jaw bridge",PrimitiveType.Cube,new Vector3(0,0,.105f),new Vector3(.095f,.03f,.025f),wrenchMetal,false);parent=saved;
            tool.AddComponent<BoxCollider>().size=new Vector3(.32f,.22f,.66f);var body=tool.AddComponent<Rigidbody>();body.mass=.45f;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            var grab=tool.AddComponent<XRGrabInteractable>();var attach=new GameObject("Grip attachment").transform;attach.SetParent(tool.transform,false);grab.attachTransform=attach;grab.useDynamicAttach=false;grab.throwOnDetach=true;tool.AddComponent<WrenchHaptics>();tool.AddComponent<WatchWrenchHandPose>();Set(controller,"wrench",grab);Set(controller,"wrenchVisual",visual);
            var pipe=Part("Single service pipe",PrimitiveType.Cylinder,new Vector3(.8f,1.05f,.15f),new Vector3(.18f,1.15f,.18f),metal);pipe.transform.rotation=Quaternion.Euler(0,0,90);
            foreach(float x in new[]{-.2f,1.8f}){Part("Pipe support",PrimitiveType.Cube,new Vector3(x,.5f,.15f),new Vector3(.12f,1,.16f),metal);Part("Support foot",PrimitiveType.Cube,new Vector3(x,.04f,.15f),new Vector3(.35f,.08f,.4f),metal);}
            var joint=Part("Leaking centre coupling",PrimitiveType.Cylinder,new Vector3(.8f,1.05f,.15f),new Vector3(.25f,.12f,.25f),red);joint.transform.rotation=Quaternion.Euler(0,0,90);
            var repair=new GameObject("Repair point").transform;repair.SetParent(parent);repair.position=new Vector3(.8f,1.05f,-.01f);Set(controller,"repairPoint",repair);
            var repairText=Text("Repair feedback","",new Vector3(.8f,1.55f,0f),.004f,null,34);repairText.color=new Color(.96f,.92f,.78f);Set(controller,"repairPrompt",repairText);
            var leak=new GameObject("Visible centre leak").AddComponent<ParticleSystem>();leak.transform.SetParent(parent);leak.transform.position=repair.position;leak.transform.rotation=Quaternion.Euler(110,0,0);leak.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var lm=leak.main;lm.startSpeed=1.4f;lm.startLifetime=.6f;lm.startSize=.025f;lm.maxParticles=100;lm.gravityModifier=.8f;lm.startColor=new Color(.45f,.7f,.8f,.65f);var le=leak.emission;le.rateOverTime=85;var ls=leak.shape;ls.angle=12;ls.radius=.015f;leak.GetComponent<ParticleSystemRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_LighthouseKeepers/Art/Materials/LK_Rain.mat");Set(controller,"leak",leak);
            var audio=leak.gameObject.AddComponent<AudioSource>();audio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_LighthouseKeepers/Audio/Ambience/Placeholder_Drip.wav");audio.loop=true;audio.spatialBlend=1;audio.volume=.35f;audio.minDistance=1;audio.maxDistance=15;Set(controller,"leakAudio",audio);
            var bed=Part("Third-floor bed frame",PrimitiveType.Cube,new Vector3(0f,6.9f,-.3f),new Vector3(1.05f,.45f,2.05f),wood);Part("Mattress and blanket",PrimitiveType.Cube,new Vector3(0f,7.18f,-.3f),new Vector3(1.0f,.18f,2),cloth);Part("Pillow",PrimitiveType.Cube,new Vector3(0f,7.32f,.4f),new Vector3(.75f,.12f,.4f),white);Set(controller,"bed",bed.transform);Set(controller,"bedPrompt",Text("Sleep prompt","",new Vector3(0f,8.15f,-.5f),.018f));
            StairwellLiner("F1 F2",0f,2.36f,220f,320f);StairwellLiner("F2 F3",3.3f,2.36f,220f,320f);StairwellLiner("F3 F4",6.6f,1.06f,140f,320f);
            var camera=Camera.main;Set(controller,"head",camera.transform);
            if(!camera.transform.root.GetComponent<WatchPlayerHeight>())camera.transform.root.gameObject.AddComponent<WatchPlayerHeight>();
            // The wrench owns its two discrete pulses; avoid duplicate default hover/grab vibration.
            foreach(var feedback in camera.transform.root.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Feedback.SimpleHapticFeedback>(true))feedback.enabled=false;
            var oldHud=camera.transform.Find("Watch camera HUD");if(oldHud)UnityEngine.Object.DestroyImmediate(oldHud.gameObject);
            var oldOverlay=camera.transform.Find("Camera recording overlay");if(oldOverlay)UnityEngine.Object.DestroyImmediate(oldOverlay.gameObject);
            var hud=new GameObject("Watch camera HUD").transform;hud.SetParent(camera.transform,false);
            var grain=GameObject.CreatePrimitive(PrimitiveType.Quad);grain.name="Subtle sensor grain";UnityEngine.Object.DestroyImmediate(grain.GetComponent<Collider>());grain.transform.SetParent(hud,false);grain.transform.localPosition=new Vector3(0,0,.58f);grain.transform.localScale=new Vector3(2,2,1);grain.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Environment/M_CameraGrain.mat");
            var hudLight=Mat("CameraHudLight",new Color(.87f,.87f,.82f));hudLight.shader=Shader.Find("Universal Render Pipeline/Unlit");var hudDark=Mat("CameraHudDark",new Color(.04f,.04f,.04f));hudDark.shader=Shader.Find("Universal Render Pipeline/Unlit");var hudRed=Mat("CameraHudRed",new Color(.78f,.05f,.05f));hudRed.shader=Shader.Find("Universal Render Pipeline/Unlit");
            var rec=Text("REC status","● REC",new Vector3(-.32f,.108f,.54f),.0032f,hud,38,TextAnchor.MiddleLeft,TextAlignment.Left);rec.color=new Color(.86f,.08f,.07f);
            Set(controller,"clock",Text("Elapsed watch time","00:00 / 07:00",new Vector3(-.075f,.108f,.54f),.00345f,hud,38,TextAnchor.MiddleLeft,TextAlignment.Left));
            var exposure=Text("Exposure indicator","W   [====]   T",new Vector3(-.075f,.035f,.54f),.00175f,hud,30);exposure.color=new Color(.82f,.82f,.78f);
            Text("Battery count","4 / 10",new Vector3(.205f,.108f,.54f),.0021f,hud,30,TextAnchor.MiddleLeft,TextAlignment.Left);
            HudBar(hud,"Battery body",new Vector3(.362f,.108f,.54f),new Vector3(.092f,.044f,.006f),hudLight);HudBar(hud,"Battery interior",new Vector3(.362f,.108f,.535f),new Vector3(.078f,.030f,.008f),hudDark);HudBar(hud,"Battery charge",new Vector3(.350f,.108f,.531f),new Vector3(.048f,.018f,.010f),hudLight);HudBar(hud,"Battery cap",new Vector3(.417f,.108f,.54f),new Vector3(.011f,.020f,.006f),hudLight);
            Set(controller,"dayTitle",Text("Day title","DAY 1",new Vector3(0,.028f,.82f),.0038f,hud,40));
            var fade=GameObject.CreatePrimitive(PrimitiveType.Quad);fade.name="Sleep fade only";UnityEngine.Object.DestroyImmediate(fade.GetComponent<Collider>());fade.transform.SetParent(hud,false);fade.transform.localPosition=new Vector3(0,0,.4f);fade.transform.localScale=new Vector3(2,2,1);var fadeMat=AssetDatabase.LoadAssetAtPath<Material>(Root+"SleepFade.mat");if(!fadeMat){fadeMat=new Material(Shader.Find("LighthouseKeepers/Watch Fade"));AssetDatabase.CreateAsset(fadeMat,Root+"SleepFade.mat");}fadeMat.color=new Color(0,0,0,0);fade.GetComponent<Renderer>().sharedMaterial=fadeMat;Set(controller,"fade",fade.GetComponent<Renderer>());
            MakeWhoosh();foreach(var a in camera.GetComponents<AudioSource>())if(a.clip&&a.clip.name=="DayWhoosh")UnityEngine.Object.DestroyImmediate(a);var whoosh=camera.gameObject.AddComponent<AudioSource>();whoosh.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"DayWhoosh.wav");whoosh.playOnAwake=false;whoosh.volume=.18f;Set(controller,"whoosh",whoosh);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("WATCH_DEMO_AUTHORED");
        }
        public static void CapturePreview()
        {
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);
            var lighting=UnityEngine.Object.FindFirstObjectByType<LighthouseKeepers.Presentation.Environment.BeaconAndMainsLightingGroup>();lighting.enabled=false;
            var baked=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.gameObject.isStatic).OrderBy(r=>EnvironmentLightingBake.PathOf(r.transform),StringComparer.Ordinal).ToArray();
            EnvironmentLightingBake.Apply(AssetDatabase.LoadAssetAtPath<LighthouseKeepers.Presentation.Environment.EnvironmentLightingPreset>("Assets/Settings/Lighting/LightingPreset_Powered_Half.asset"),baked);
            GameObject.Find("Watch checklist").GetComponent<TextMesh>().text="KEEPER WATCH — DAY 1\n\n[ ] FIX THE FLOOD\n\n1. TAKE WRENCH\n2. FIND LEAK\n3. REPAIR PIPE";
            GameObject.Find("Sleep prompt").GetComponent<TextMesh>().text="PRESS GRIP TO SLEEP";
            var rig=Camera.main.transform.root;foreach(var r in rig.GetComponentsInChildren<Renderer>(true))r.enabled=false;
            var camera=new GameObject("Preview capture camera").AddComponent<Camera>();camera.enabled=false;camera.fieldOfView=65;camera.nearClipPlane=.03f;camera.farClipPlane=300;camera.useOcclusionCulling=false;
            var target=new RenderTexture(1280,960,24);target.Create();Directory.CreateDirectory("artifacts/atmosphere/captures");
            var views=new[]{("board",new Vector3(-2.15f,1.65f,-9.20f),new Vector3(-2.20f,1.45f,-7.50f)),("pipe",new Vector3(.8f,1.65f,-1.5f),new Vector3(.8f,1,.15f)),("bed",new Vector3(1.6f,8.2f,-.3f),new Vector3(0,7.3f,-.3f))};
            foreach(var v in views)
            {
                camera.transform.position=v.Item2;camera.transform.LookAt(v.Item3);
                if(v.Item1=="bed")GameObject.Find("Sleep prompt").transform.rotation=Quaternion.LookRotation(GameObject.Find("Sleep prompt").transform.position-camera.transform.position);
                RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=target});
                var old=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(1280,960,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,960),0,0);texture.Apply();RenderTexture.active=old;
                File.WriteAllBytes("artifacts/atmosphere/captures/"+v.Item1+".png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            }
            target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(camera.gameObject);
            Debug.Log("WATCH_PREVIEW_CAPTURES_READY — editor stills only, not headset verification");
        }
        static void MakeWhoosh()
        {
            string path=Root+"DayWhoosh.wav";if(File.Exists(path))return;int rate=22050,count=(int)(rate*.7f);var random=new System.Random(61);
            using(var w=new BinaryWriter(File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);float filtered=0;for(int i=0;i<count;i++){float t=(float)i/count;filtered=Mathf.Lerp(filtered,(float)random.NextDouble()*2-1,.08f+.3f*t);float envelope=Mathf.Sin(t*Mathf.PI);w.Write((short)(filtered*envelope*envelope*16000));}}
            AssetDatabase.ImportAsset(path);
        }
    }
}
