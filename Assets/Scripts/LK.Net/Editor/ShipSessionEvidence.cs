using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace LighthouseKeepers.Net.Editor
{
    [InitializeOnLoad] public static class ShipSessionEvidence
    {
        const string Key="LK.Round2.E.Capture";
        const string Folder="artifacts/round2/E/";
        static bool clicked;
        static double started,readyAt;
        static ShipSessionEvidence(){EditorApplication.playModeStateChanged+=Changed;}
        [MenuItem("LK/Ship/Capture Session E route")]
        public static void PrepareAndCapture()
        {
            try
            {
                SessionState.SetBool(Key+"Fail",false);ShipRouteAuthoring.Apply();ShipRouteAuthoring.ApplyBuildScenes();
                var author=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.Environment.Sprint3.Editor.Round2EnvironmentAuthoring")).FirstOrDefault(t=>t!=null);
                if(author==null)throw new InvalidOperationException("C's Round2 environment authoring is required for real ship scene evidence.");
                author.GetMethod("Apply").Invoke(null,null);
                var begin=EditorSceneManager.OpenScene(ShipRouteSettings.Load().Plan.lobbyScene,OpenSceneMode.Single);
                EditorSceneManager.playModeStartScene=null;SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();
            }
            catch(Exception e){Failure(e.ToString());}
        }
        static void Changed(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode){clicked=false;started=EditorApplication.timeSinceStartup;readyAt=-1;EditorApplication.update+=Tick;}
            if(state==PlayModeStateChange.EnteredEditMode)
            {EditorApplication.update-=Tick;SessionState.SetBool(Key,false);if(Application.isBatchMode)EditorApplication.Exit(SessionState.GetBool(Key+"Fail",false)?1:0);}
        }
        static void Tick()
        {
            try
            {
                if(!Application.isPlaying)return;
                if(EditorApplication.timeSinceStartup-started>90)throw new TimeoutException("Lobby -> Play Solo did not reach the ready ship world.");
                if(!clicked)
                {
                    var button=GameObject.Find("PlaySolo");if(!button)return;
                    var ui=button.GetComponent<Button>();if(!ui || !ui.interactable)return;
                    ui.onClick.Invoke();clicked=true;return;
                }
                if(ShipWorldLoader.Active && !string.IsNullOrEmpty(ShipWorldLoader.Active.LastError))throw new InvalidOperationException(ShipWorldLoader.Active.LastError);
                if(!ShipWorldLoader.Active || !ShipWorldLoader.Active.Ready)return;
                if(readyAt<0){readyAt=EditorApplication.timeSinceStartup;return;}
                if(EditorApplication.timeSinceStartup-readyAt<2)return;
                var plan=ShipRouteSettings.Load().Plan;
                var loaded=Enumerable.Range(0,SceneManager.sceneCount).Select(i=>SceneManager.GetSceneAt(i).path).Where(p=>!string.IsNullOrEmpty(p)).ToArray();
                if(!new[]{plan.entryScene}.Concat(plan.additiveScenes).All(loaded.Contains) || loaded.Any(p=>p.Contains("LK_StormWatch")))throw new InvalidOperationException("Evidence is not the canonical world.");
                var anchor=GameObject.Find("VIEW_BriefingBoard_2p5m");if(!anchor)throw new InvalidOperationException("Missing named screenshot anchor VIEW_BriefingBoard_2p5m.");
                var board=GameObject.Find("STN_BRIEFING_BOARD");
                if(!board || !board.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("DAY 1")))throw new InvalidOperationException("House board does not show live Day 1.");
                if(!UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Any(r=>r.name.IndexOf("wrench",StringComparison.OrdinalIgnoreCase)>=0))throw new InvalidOperationException("Wrench missing from loaded ship world.");
                Capture(anchor.transform);
                Directory.CreateDirectory(Folder);
                File.WriteAllText(Folder+"playmode.json",JsonUtility.ToJson(new Proof{editorPlayMode=true,lobbyPlaySoloInvoked=clicked,anchor=anchor.name,position=anchor.transform.position,loadedScenes=loaded,utc=DateTime.UtcNow.ToString("O")},true));
                EditorApplication.update-=Tick;EditorApplication.ExitPlaymode();
            }
            catch(Exception e){Failure(e.ToString());}
        }
        [Serializable] sealed class Proof{public bool editorPlayMode,lobbyPlaySoloInvoked;public string anchor,utc;public Vector3 position;public string[] loadedScenes;}
        static void Capture(Transform anchor)
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new InvalidOperationException("Screenshot needs graphics: omit -nographics.");
            var root=new GameObject("E Play Mode evidence camera");var camera=root.AddComponent<Camera>();camera.enabled=false;
            camera.transform.SetPositionAndRotation(anchor.position,anchor.rotation);camera.fieldOfView=65;camera.nearClipPlane=.03f;camera.farClipPlane=250;
            var target=new RenderTexture(1600,1000,24,RenderTextureFormat.ARGB32);target.Create();var prior=RenderTexture.active;
            var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();Directory.CreateDirectory(Folder);File.WriteAllBytes(Folder+"VIEW_BriefingBoard_2p5m.png",image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=prior;target.Release();UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(root);}
        }
        static void Failure(string error)
        {
            Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"capture-failure.txt",error);Debug.LogError(error);
            SessionState.SetBool(Key+"Fail",true);EditorApplication.update-=Tick;
            if(EditorApplication.isPlaying)EditorApplication.ExitPlaymode();else if(Application.isBatchMode)EditorApplication.Exit(1);
        }
    }
}
