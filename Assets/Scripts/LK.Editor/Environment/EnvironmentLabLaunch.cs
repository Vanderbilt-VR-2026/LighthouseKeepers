using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment
{
    [InitializeOnLoad]
    public static class EnvironmentLabLaunch
    {
        const string Key="LKEnvironmentLabPreviousStart";
        static EnvironmentLabLaunch(){EditorApplication.playModeStateChanged+=Changed;}
        [MenuItem("Lighthouse/Play Environment Lab (temporary)")]
        public static void Play()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            SessionState.SetString(Key,EditorSceneManager.playModeStartScene?AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene):"");
            SessionState.SetBool(Key+"Active",true);EditorSceneManager.playModeStartScene=null;
            EditorSceneManager.OpenScene(LighthouseBuilder.LabPath);EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredEditMode||!SessionState.GetBool(Key+"Active",false))return;
            string previous=SessionState.GetString(Key,"");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(previous)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);SessionState.SetBool(Key+"Active",false);
        }
    }
}
