#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using LighthouseKeepers.Core.Progression;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class SoloLobbyFlowTests
    {
        [UnityTest] public IEnumerator LobbyPlaySoloLoadsHouseDayOneWhiteboardAndWrench()
        {
            var plan=ShipRouteSettings.Load().Plan;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(plan.lobbyScene,new LoadSceneParameters(LoadSceneMode.Single));
            float deadline=Time.realtimeSinceStartup+90;
            while(!GameObject.Find("PlaySolo") && Time.realtimeSinceStartup<deadline)yield return null;
            var button=GameObject.Find("PlaySolo");Assert.IsNotNull(button,"Use the actual visible lobby entry.");
            button.GetComponent<Button>().onClick.Invoke();
            while((!ShipWorldLoader.Active || !ShipWorldLoader.Active.Ready) && Time.realtimeSinceStartup<deadline)
            {
                Assert.IsTrue(!ShipWorldLoader.Active || string.IsNullOrEmpty(ShipWorldLoader.Active.LastError),ShipWorldLoader.Active?ShipWorldLoader.Active.LastError:"");
                yield return null;
            }
            Assert.IsTrue(ShipWorldLoader.Active && ShipWorldLoader.Active.Ready,"Ship loader timed out.");
            Assert.AreEqual(plan.entryScene,SceneManager.GetActiveScene().path);
            foreach(var path in new[]{plan.entryScene}.Concat(plan.additiveScenes))Assert.IsTrue(SceneManager.GetSceneByPath(path).isLoaded,path);
            Assert.IsFalse(SceneManager.GetSceneByName("LK_StormWatch").isLoaded);
            var board=GameObject.Find("STN_BRIEFING_BOARD");Assert.IsNotNull(board);
            Assert.IsTrue(board.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("DAY 1")));
            Assert.IsNotNull(GameObject.Find("STN_TOOL_TABLE"));
            Assert.IsTrue(Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Any(r=>r.name.ToLowerInvariant().Contains("wrench")),"Authored wrench must be present in the loaded ship world.");
            Assert.IsFalse(Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name.StartsWith("SOLO DAY")||b.name=="SOLO RELAY"));
            Assert.AreEqual(0,Object.FindObjectsByType<Fusion.NetworkRunner>(FindObjectsSortMode.None).Length);
            Assert.IsTrue(ShipWorldLoader.Solo);
        }
    }
}
#endif
