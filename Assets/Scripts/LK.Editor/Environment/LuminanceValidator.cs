using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.Editor.Environment
{
    public static class LuminanceValidator
    {
        static string CalibrationPath=>LighthouseBuilder.Layout.lightingValidation.calibrationAsset;
        [Serializable] sealed class Calibration {public float referenceLinearLuminance;public int calibrationRevision=2;public float exposure=1;public string note;}
        static Camera camera;static RenderTexture target;static Texture2D readback;
        static void PausePresentation()
        {
            foreach(var g in UnityEngine.Object.FindObjectsByType<BeaconAndMainsLightingGroup>(FindObjectsSortMode.None))g.enabled=false;
            // A head-mounted overlay is not room illumination. Exclude it from the independent measurement camera.
            foreach(var view in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                foreach(var renderer in view.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
        }
        static void Begin()
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new Exception("Luminance validation requires a graphics device; omit -nographics.");
            camera=new GameObject("Editor measurement camera").AddComponent<Camera>();camera.enabled=false;camera.useOcclusionCulling=false;camera.fieldOfView=8;camera.nearClipPlane=.01f;camera.farClipPlane=40;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.allowHDR=true;
            target=new RenderTexture(64,64,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);target.Create();readback=new Texture2D(64,64,TextureFormat.RGBAFloat,false,true);
        }
        static float Measure(Vector3 eye,Vector3 point)
        {
            camera.transform.position=eye;camera.transform.rotation=Quaternion.LookRotation(point-eye,Vector3.forward);
            RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=target});
            var old=RenderTexture.active;RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,64,64),0,0);readback.Apply();RenderTexture.active=old;
            var pixels=readback.GetPixels();double sum=0;int count=0;
            for(int y=24;y<40;y++)for(int x=24;x<40;x++){var c=pixels[y*64+x];sum+=.2126*c.r+.7152*c.g+.0722*c.b;count++;}return (float)(sum/count);
        }
        static void End(){if(camera)UnityEngine.Object.DestroyImmediate(camera.gameObject);if(target){target.Release();UnityEngine.Object.DestroyImmediate(target);}if(readback)UnityEngine.Object.DestroyImmediate(readback);}
        static MeshRenderer[] Renderers()=>UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.gameObject.isStatic).OrderBy(r=>EnvironmentLightingBake.PathOf(r.transform),StringComparer.Ordinal).ToArray();
        public static void FinalizeMeasurements()
        {
            if(!File.Exists(CalibrationPath))CalibratePoweredReference();Run();CaptureViews();EnvironmentValidation.RunGeometry();EnvironmentOcclusion.BakeLab();ShellBuildCache.Record();
        }
        public static void CalibratePoweredReference()
        {
            if(File.Exists(CalibrationPath))throw new Exception("Calibration already frozen. Do not recalibrate to make a dark room pass.");
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);PausePresentation();
            var preset=AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>("Assets/Settings/Lighting/LightingPreset_Powered.asset");if(!preset)throw new Exception("Bake powered state first");
            EnvironmentLightingBake.Apply(preset,Renderers());Begin();
            try
            {
                var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Generated/SM_Shell_ScaleReference.fbx");var mesh=model.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="SM_ScaleReference_Cube").sharedMesh;
                var card=new GameObject("Measurement reference — 0.18 albedo");card.AddComponent<MeshFilter>().sharedMesh=mesh;var mr=card.AddComponent<MeshRenderer>();var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=new Color(.18f,.18f,.18f);mat.SetFloat("_Smoothness",0);mr.sharedMaterial=mat;
                var p=LighthouseBuilder.Polar(30,3.25f,.02f);card.transform.position=p;card.transform.localScale=new Vector3(.3f,.005f,.3f);
                float value=Measure(p+Vector3.up*1.6f,p+Vector3.up*.005f);
                if(value<.005f||float.IsNaN(value))throw new Exception("Powered reference is black/invalid: "+value);
                File.WriteAllText(CalibrationPath,JsonUtility.ToJson(new Calibration{referenceLinearLuminance=value,note="Single powered-state linear render capture with powered light probes explicitly applied at F1 azimuth30/r3.25, eye1.6m, exposure1. No post-processing. Headset coefficient tuning remains pending."},true));
                UnityEngine.Object.DestroyImmediate(card);UnityEngine.Object.DestroyImmediate(mat);Debug.Log("POWERED_REFERENCE_CAPTURE "+value);
            }finally{End();}
        }
        static EnvironmentLightingPreset ActivePreset(bool powered)
        {
            var group=UnityEngine.Object.FindFirstObjectByType<BeaconAndMainsLightingGroup>();
            if(!group)throw new Exception("Missing lighting group");
            return (EnvironmentLightingPreset)new SerializedObject(group).FindProperty(powered?"poweredPreset":"emergencyPreset").objectReferenceValue;
        }
        public static void Run()
        {
            if(!File.Exists(CalibrationPath))throw new Exception("No fixed powered-reference calibration. Run explicit calibration once after baking.");
            var cal=JsonUtility.FromJson<Calibration>(File.ReadAllText(CalibrationPath));if(cal.referenceLinearLuminance<=0||cal.exposure!=1)throw new Exception("Invalid calibration");
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);PausePresentation();var renderers=Renderers();var r=new CheckReport{validator="LuminanceValidator"};var samples=new List<Vector3>();
            // The explicitly requested half-bright preview has a separate target; never recalibrate the reference.
            bool halfPreview=AssetDatabase.GetAssetPath(ActivePreset(true)).EndsWith("LightingPreset_Powered_Half.asset",StringComparison.Ordinal)
                &&AssetDatabase.GetAssetPath(ActivePreset(false)).EndsWith("LightingPreset_Emergency_Half.asset",StringComparison.Ordinal);
            float targetScale=halfPreview?.5f:1f;
            File.WriteAllText("artifacts/batch-a/lighting-target.txt",halfPreview
                ?"Requested 50% preview: powered >= 0.06 and emergency >= 0.02 of the SAME original powered reference. This does NOT certify the old 0.12/0.04 targets. Headset readability pending."
                :"Original brightness targets: powered >= 0.12 and emergency >= 0.04 of the frozen powered reference.");
            foreach(var f in LighthouseBuilder.Layout.floors)for(float x=-f.interiorRadius+.5f;x<f.interiorRadius;x+=.5f)for(float z=-f.interiorRadius+.5f;z<f.interiorRadius;z+=.5f)
            {
                var p=new Vector3(x,f.floorY,z);if(new Vector2(x,z).magnitude>f.interiorRadius-.3f)continue;
                if(EnvironmentValidation.Floor(p,out var hit)&&Mathf.Abs(hit.point.y-f.floorY)<.025f&&EnvironmentValidation.Body(p).Length==0)samples.Add(p);
            }
            for(float x=-3;x<=3;x+=.5f)for(float z=-9.35f;z<=-5.35f;z+=.5f){var p=new Vector3(x,0,z);if(EnvironmentValidation.Body(p).Length==0)samples.Add(p);}
            // Include the passage, gallery and every stair run, not only room floor grids.
            foreach(var route in LighthouseBuilder.Layout.routes)
                for(int j=1;j<route.points.Length;j++)
                {
                    var a=route.points[j-1];var b=route.points[j];int count=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(a,b)/.5f));
                    for(int k=0;k<=count;k++){var p=Vector3.Lerp(a,b,(float)k/count);if(EnvironmentValidation.Floor(p,out var hit))samples.Add(hit.point);}
                }
            // Gallery's full navigable ring, rather than only the door-to-viewpoint route.
            for(float a=0;a<360;a+=8)for(float radius=3.4f;radius<=3.9f;radius+=.5f)samples.Add(LighthouseBuilder.Polar(a,radius,9.9f));
            Begin();var poweredValues=new List<float>();
            try
            {
                foreach(bool on in new[]{true,false})
                {
                    var preset=ActivePreset(on);if(!preset)throw new Exception("Missing baked preset");EnvironmentLightingBake.Apply(preset,renderers);
                    for(int i=0;i<samples.Count;i++)
                    {
                        float v=Measure(samples[i]+Vector3.up*1.6f,samples[i]);r.samples++;float threshold=targetScale*cal.referenceLinearLuminance*(on?LighthouseBuilder.Layout.lightingValidation.poweredFloorRatio:LighthouseBuilder.Layout.lightingValidation.emergencyFloorRatio);
                        if(v<threshold)r.failures.Add((on?"Powered":"Emergency")+" floor "+EnvironmentValidation.Pos(samples[i])+" luminance="+v+" required="+threshold);
                        if(on)poweredValues.Add(v);else if(v>poweredValues[i]+Mathf.Max(.0001f,poweredValues[i]*.02f))r.failures.Add("Emergency brighter than powered at "+EnvironmentValidation.Pos(samples[i])+" powered="+poweredValues[i]+" emergency="+v);
                    }
                }
            }finally{End();}
            r.passed=r.failures.Count==0;EnvironmentValidation.Save(r);if(!r.passed)throw new Exception("Luminance floor validation failed");
            Debug.Log("LUMINANCE_FLOORS_PASS — see lighting-target.txt for the active target; demo text readability requires headset verification");
        }
        public static void CaptureViews()
        {
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath,OpenSceneMode.Single);PausePresentation();var powered=ActivePreset(true);if(powered)EnvironmentLightingBake.Apply(powered,Renderers());Begin();
            try
            {
                target.Release();UnityEngine.Object.DestroyImmediate(target);target=new RenderTexture(1280,960,24,RenderTextureFormat.ARGB32);target.Create();camera.fieldOfView=70;
                var views=new[]{("house",new Vector3(0,1.65f,-9),new Vector3(0,1.2f,-4)),("f1",new Vector3(0,1.65f,-2.8f),new Vector3(-2.2f,1.5f,.4f)),("f3",new Vector3(2,8.25f,1),new Vector3(-1.5f,8.2f,0)),("gallery",new Vector3(4,11.55f,0),new Vector3(0,11.3f,0)),("exterior",new Vector3(15,9,-20),new Vector3(0,6,0))};
                foreach(var v in views){camera.transform.position=v.Item2;camera.transform.LookAt(v.Item3);RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=target});var old=RenderTexture.active;RenderTexture.active=target;var tex=new Texture2D(1280,960,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,960),0,0);tex.Apply();RenderTexture.active=old;File.WriteAllBytes("artifacts/batch-a/"+v.Item1+".png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);}
            }finally{End();}
        }
    }
}
