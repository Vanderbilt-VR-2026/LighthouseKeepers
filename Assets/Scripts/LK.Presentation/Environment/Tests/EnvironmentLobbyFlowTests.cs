#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.XR.CoreUtils;
namespace LighthouseKeepers.Presentation.Environment.Tests
{
    [PrebuildSetup(typeof(EnvironmentLobbySceneSetup))]
    [PostBuildCleanup(typeof(EnvironmentLobbySceneSetup))]
    public sealed class EnvironmentLobbyFlowTests
    {
        [UnityTest] public IEnumerator OriginalLobbyStartLoadsHouseWithOneGroundedPlayer()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lobby_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var manager=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c=>c.GetType().FullName=="LighthouseKeepers.Lobby.LobbyManager");
            var type=manager.GetType();Assert.That((bool)type.GetMethod("Host").Invoke(manager,null),Is.True);
            type.GetMethod("ToggleReady").Invoke(manager,null);
            Assert.That((bool)type.GetMethod("StartGame").Invoke(manager,null),Is.True,(string)type.GetProperty("LastError").GetValue(manager));
            for(int i=0;i<180 && SceneManager.GetActiveScene().name!="LK_Lab_Environment";i++)yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("LK_Lab_Environment"));
            for(int i=0;i<72;i++)yield return null;
            var rigs=Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None);Assert.That(rigs.Length,Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            Assert.That(rigs[0].transform.position.y,Is.InRange(-.05f,.15f));
            Assert.That(rigs[0].transform.position.z,Is.EqualTo(-8.4f).Within(.15f));
            foreach(var c in rigs[0].GetComponentsInChildren<MonoBehaviour>(true))
                if(c && c.GetType().Name=="BodyPresence")Assert.That(c.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled),Is.False);
        }
    }
    public sealed class EnvironmentLobbySceneSetup : IPrebuildSetup, IPostBuildCleanup
    {
        [System.Serializable] sealed class SceneList {public string[] paths;public bool[] enabled;}
        const string Backup="LK.Environment.LobbyTest.BuildSettings";
        public void Setup()
        {
            if(SessionState.GetString(Backup,"").Length==0)SessionState.SetString(Backup,JsonUtility.ToJson(new SceneList{paths=EditorBuildSettings.scenes.Select(s=>s.path).ToArray(),enabled=EditorBuildSettings.scenes.Select(s=>s.enabled).ToArray()}));
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Lab/LK_Lobby_Environment.unity",true),new EditorBuildSettingsScene("Assets/Scenes/Lab/LK_Lab_Environment.unity",true)};
        }
        public void Cleanup()
        {
            var saved=SessionState.GetString(Backup,"");if(saved.Length==0)return;
            var data=JsonUtility.FromJson<SceneList>(saved);EditorBuildSettings.scenes=data.paths.Select((p,i)=>new EditorBuildSettingsScene(p,data.enabled[i])).ToArray();SessionState.EraseString(Backup);
        }

    }

}
#endif
