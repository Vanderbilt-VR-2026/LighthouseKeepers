using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.Editor.Environment
{
    public static class EnvironmentAtmosphere
    {
        const string Materials="Assets/Art/Materials/Environment/";
        public static void Apply()
        {
            HalfPreset("Powered");HalfPreset("Emergency");
            foreach(var path in new[]{LighthouseBuilder.LabPath,EnvironmentLobbyBuild.LobbyPath})
            {
                EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                foreach(var c in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                    if(c&&c.GetType().Name=="TunnelingVignetteController")c.gameObject.SetActive(false);
                var camera=Camera.main;if(!camera)throw new Exception("Missing player camera");
                Overlay(camera);
                if(path==LighthouseBuilder.LabPath)
                {
                    var group=UnityEngine.Object.FindFirstObjectByType<BeaconAndMainsLightingGroup>();
                    var so=new SerializedObject(group);so.FindProperty("poweredPreset").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>("Assets/Settings/Lighting/LightingPreset_Powered_Half.asset");so.FindProperty("emergencyPreset").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>("Assets/Settings/Lighting/LightingPreset_Emergency_Half.asset");so.ApplyModifiedPropertiesWithoutUndo();
                    // Intensity authoring is idempotent; baked energy is carried by the separate half presets.
                    foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                    {if(light.name.EndsWith(" [50%]"))continue;light.intensity*=.5f;light.name+=" [50%]";}
                    AddWeather(camera);
                }
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
            AssetDatabase.SaveAssets();Debug.Log("ATMOSPHERE_AUTHORED");
        }
        public static void RebuildHalfLighting(){HalfPreset("Powered");HalfPreset("Emergency");AssetDatabase.SaveAssets();}
        static void HalfPreset(string state)
        {
            string dir="Assets/Settings/Lighting/"+state+"Half";Directory.CreateDirectory(dir);AssetDatabase.Refresh();
            var source=AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>("Assets/Settings/Lighting/LightingPreset_"+state+".asset");
            string path="Assets/Settings/Lighting/LightingPreset_"+state+"_Half.asset";
            var target=AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>(path);if(!target){target=ScriptableObject.CreateInstance<EnvironmentLightingPreset>();AssetDatabase.CreateAsset(target,path);}
            EditorUtility.CopySerialized(source,target);target.colorMaps=new Texture2D[source.colorMaps.Length];
            for(int i=0;i<source.colorMaps.Length;i++)
            {
                // Read the source EXR as linear HDR, not the platform-encoded lightmap texture.
                // Sampling an imported RGBM lightmap with a plain blit loses its HDR decode multiplier.
                string scratch="Assets/Settings/Lighting/_HalfSource.exr";
                File.Copy(AssetDatabase.GetAssetPath(source.colorMaps[i]),scratch,true);AssetDatabase.ImportAsset(scratch);
                var rawImporter=(TextureImporter)AssetImporter.GetAtPath(scratch);
                rawImporter.textureType=TextureImporterType.Default;rawImporter.sRGBTexture=false;rawImporter.isReadable=true;
                rawImporter.textureCompression=TextureImporterCompression.Uncompressed;rawImporter.maxTextureSize=8192;
                rawImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=8192,format=TextureImporterFormat.RGBAHalf});rawImporter.SaveAndReimport();
                var raw=AssetDatabase.LoadAssetAtPath<Texture2D>(scratch);
                var copy=new Texture2D(raw.width,raw.height,TextureFormat.RGBAFloat,false,true);copy.SetPixels(raw.GetPixels());copy.Apply();
                AssetDatabase.DeleteAsset(scratch);
                var pixels=copy.GetPixels();for(int j=0;j<pixels.Length;j++){pixels[j].r*=.5f;pixels[j].g*=.5f;pixels[j].b*=.5f;}copy.SetPixels(pixels);copy.Apply();
                var dst=dir+"/"+i+"-Color.exr";File.WriteAllBytes(dst,copy.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat));UnityEngine.Object.DestroyImmediate(copy);AssetDatabase.ImportAsset(dst);
                var importer=(TextureImporter)AssetImporter.GetAtPath(dst);var sourceImporter=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(source.colorMaps[i]));var settings=new TextureImporterSettings();sourceImporter.ReadTextureSettings(settings);importer.SetTextureSettings(settings);importer.textureCompression=sourceImporter.textureCompression;importer.maxTextureSize=sourceImporter.maxTextureSize;importer.SaveAndReimport();target.colorMaps[i]=AssetDatabase.LoadAssetAtPath<Texture2D>(dst);
            }
            target.probeCoefficients=source.probeCoefficients.Select(v=>v*.5f).ToArray();target.ambient=source.ambient*.5f;EditorUtility.SetDirty(target);
        }
        static Material Mat(string name,string shader)
        {
            string path=Materials+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}return m;
        }
        static void Overlay(Camera camera)
        {
            var old=camera.transform.Find("Camera recording overlay");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root=new GameObject("Camera recording overlay");root.transform.SetParent(camera.transform,false);
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="Subtle sensor grain";UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());quad.transform.SetParent(root.transform,false);quad.transform.localPosition=new Vector3(0,0,.55f);quad.transform.localScale=new Vector3(2,2,1);
            var renderer=quad.GetComponent<MeshRenderer>();renderer.sharedMaterial=Mat("M_CameraGrain","LighthouseKeepers/Environment/Camera Grain");renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            // HUD text belongs to the Watch Demo camera overlay. This root owns only the neutral grain layer.
        }
        static AudioSource Audio(Transform parent,string name,string clip,Vector3 position,bool loop,float volume)
        {
            var a=new GameObject(name).AddComponent<AudioSource>();a.transform.SetParent(parent);a.transform.position=position;a.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_LighthouseKeepers/Audio/Ambience/Placeholder_"+clip+".wav");a.loop=loop;a.playOnAwake=loop;a.volume=volume;a.spatialBlend=1;a.minDistance=8;a.maxDistance=100;a.rolloffMode=AudioRolloffMode.Linear;return a;
        }
        static void AddWeather(Camera camera)
        {
            var old=GameObject.Find("Shell storm presentation");if(old)UnityEngine.Object.DestroyImmediate(old);
            var root=new GameObject("Shell storm presentation");var weather=root.AddComponent<ShellWeatherPresentation>();
            RenderSettings.skybox=AssetDatabase.LoadAssetAtPath<Material>("Assets/_LighthouseKeepers/Art/Materials/LK_StormSky.mat");camera.clearFlags=CameraClearFlags.Skybox;camera.farClipPlane=350;
            var water=GameObject.CreatePrimitive(PrimitiveType.Plane);water.name="Original storm ocean presentation";water.transform.SetParent(root.transform);water.transform.position=new Vector3(0,-1.2f,0);water.transform.localScale=new Vector3(60,1,60);UnityEngine.Object.DestroyImmediate(water.GetComponent<Collider>());water.GetComponent<Renderer>().sharedMaterial=Mat("M_ShellStormOcean","LighthouseKeepers/Storm Ocean");
            var rain=new GameObject("Localized exterior rain").AddComponent<ParticleSystem>();rain.transform.SetParent(root.transform);rain.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=rain.main;main.maxParticles=650;main.startLifetime=1.5f;main.startSpeed=0;main.startSize=.018f;main.startColor=new Color(.6f,.72f,.8f,.35f);main.simulationSpace=ParticleSystemSimulationSpace.World;var emission=rain.emission;emission.rateOverTime=380;var shape=rain.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(15,.1f,15);var velocity=rain.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.y=-8;velocity.x=-1.2f;var renderer=rain.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=5;renderer.velocityScale=.025f;renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_LighthouseKeepers/Art/Materials/LK_Rain.mat");rain.Play();
            var flash=new GameObject("Distant lightning flash").AddComponent<Light>();flash.transform.SetParent(root.transform);flash.type=LightType.Directional;flash.color=new Color(.7f,.82f,1);flash.intensity=0;flash.shadows=LightShadows.None;flash.transform.rotation=Quaternion.Euler(48,-25,0);
            var outside=Audio(root.transform,"Exterior rain","RainExterior",new Vector3(0,10,9),true,0);var inside=Audio(root.transform,"Sheltered rain","RainInterior",new Vector3(0,6,0),true,.12f);var ocean=Audio(root.transform,"Ocean swell","Ocean",new Vector3(0,-2,18),true,.045f);var thunder=Audio(root.transform,"Distant thunder","Thunder",new Vector3(-25,25,40),false,.45f);var filter=thunder.gameObject.AddComponent<AudioLowPassFilter>();
            var so=new SerializedObject(weather);foreach(var pair in new (string,UnityEngine.Object)[]{("head",camera.transform),("rain",rain),("lightning",flash),("exteriorRain",outside),("interiorRain",inside),("ocean",ocean),("thunder",thunder),("thunderFilter",filter)})so.FindProperty(pair.Item1).objectReferenceValue=pair.Item2;so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
