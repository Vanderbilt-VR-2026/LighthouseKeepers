using System.Collections;
using Fusion;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using UnityEngine;
using UnityEngine.TestTools;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class NetworkRigTests
    {
        [UnityTest] public IEnumerator PlayerPrefabSpawnsOnRealOfflineRunnerWithoutAnotherCamera()
        {
            var root=new GameObject("Offline network rig test");var runner=root.AddComponent<NetworkRunner>();
            var start=runner.StartGame(new StartGameArgs { GameMode=GameMode.Single,SceneManager=root.AddComponent<NetworkSceneManagerDefault>() });
            while(!start.IsCompleted)yield return null;
            Assert.IsTrue(start.Result.Ok,start.Result.ShutdownReason.ToString());
            try
            {
                var prefab=Resources.Load<NetworkObject>("PF_PlayerRig");Assert.IsNotNull(prefab);
                runner.Config.PrefabTable.TryAddSource(new NetworkPrefabSourceStatic { Object=prefab,AssetGuid=NetworkPrefabRegistry.PlayerGuid },out _);
                var player=runner.Spawn(NetworkPrefabRegistry.PlayerGuid,inputAuthority:runner.LocalPlayer);yield return null;
                Assert.IsTrue(player.IsValid);Assert.AreEqual(runner.LocalPlayer,player.InputAuthority);
                Assert.IsEmpty(player.GetComponentsInChildren<Camera>(true));Assert.IsEmpty(player.GetComponentsInChildren<AudioListener>(true));
                Assert.IsNotNull(player.GetComponent<CrewAvatarView>());runner.Despawn(player);
            }
            finally { if(runner) _ = runner.Shutdown(); }
            while(runner && runner.IsRunning)yield return null;
            Object.Destroy(root);
        }
        [Test] public void TwoInteractorChildrenResolveOneActor()
        {
            var root=new GameObject("Rig");var identity=root.AddComponent<ActorIdentity>();identity.Bind(7);
            var left=new GameObject("Left");left.transform.SetParent(root.transform);
            var right=new GameObject("Right");right.transform.SetParent(root.transform);
            try { Assert.AreEqual(7,ActorIdentity.Of(left.transform));Assert.AreEqual(7,ActorIdentity.Of(right.transform)); }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
