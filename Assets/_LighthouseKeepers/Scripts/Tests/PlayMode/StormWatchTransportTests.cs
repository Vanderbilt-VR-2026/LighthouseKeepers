#if UNITY_EDITOR
using System;
using System.Collections;
using Fusion;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using UnityEngine;
using UnityEngine.TestTools;
using LighthouseKeepers.StormWatch;
using LighthouseKeepers.StormWatchNetworking;

namespace LighthouseKeepers.Tests
{
    public sealed class StormWatchTransportTests
    {
        [UnityTest, Category("Online")]
        public IEnumerator TwoFusionPeersExchangeCommandsAndAuthoritativeState()
        {
            if (System.Environment.GetEnvironmentVariable("LK_RUN_ONLINE_TESTS") != "1")
                Assert.Ignore("Opt in with LK_RUN_ONLINE_TESTS=1; requires the configured Photon service.");
            var config = NetworkProjectConfig.Global;
            var previousMode = config.PeerMode;
            // Transport-only peers share the already loaded scene; no duplicated scene simulation.
            config.PeerMode = NetworkProjectConfig.PeerModes.Single;
            var host = new GameObject("Test host").AddComponent<StormWatchFusionLink>();
            var client = new GameObject("Test client").AddComponent<StormWatchFusionLink>();
            try
            {
                string room = "verify-" + Guid.NewGuid().ToString("N").Substring(0, 12);
                host.Connect(true, room);
                float deadline = Time.realtimeSinceStartup + 50;
                while (!host.Connected && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.IsTrue(host.Connected, host.Status);
                client.Connect(false, room);
                deadline = Time.realtimeSinceStartup + 50;
                while ((!client.PartnerConnected || !host.PartnerConnected) && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.IsTrue(client.PartnerConnected, client.Status);
                Assert.IsTrue(host.PartnerConnected, host.Status);
                Assert.AreEqual(0, host.LocalSlot); Assert.AreEqual(1, client.LocalSlot);
                var input = new KeeperInput {connected=true, head=new Vector3(0, 1.6f, -2.7f), command=WatchAction.Begin, sequence=7, action=WatchAction.Brace, effort=1};
                client.SendInput(input);
                input.command=WatchAction.None;
                client.SendInput(input);
                deadline=Time.realtimeSinceStartup+10;
                while(host.RemoteInput.sequence!=7 && Time.realtimeSinceStartup<deadline)yield return null;
                Assert.AreEqual(WatchAction.Begin,host.RemoteInput.command,"Later pose packets must preserve an unconsumed command.");
                Assert.AreEqual(WatchAction.Brace,host.RemoteInput.action);
                var rules=new StormWatchRules();
                var local=new KeeperInput {connected=true,head=new Vector3(0,1.6f,-2.7f)};
                rules.Tick(.05f,local,host.RemoteInput);
                host.Publish(rules.State);
                deadline=Time.realtimeSinceStartup+10;
                while(client.ReceivedState==null && Time.realtimeSinceStartup<deadline)yield return null;
                Assert.IsNotNull(client.ReceivedState);
                Assert.AreEqual(WatchAct.FirstWatch,client.ReceivedState.act);
                Assert.AreEqual(rules.State.water,client.ReceivedState.water);
                client.Leave();
                deadline=Time.realtimeSinceStartup+10;
                while(host.PartnerConnected && Time.realtimeSinceStartup<deadline)yield return null;
                Assert.IsFalse(host.PartnerConnected);
                float elapsed=rules.State.elapsed;
                rules.Tick(.05f,local,host.RemoteInput);
                Assert.AreEqual(elapsed,rules.State.elapsed,"A dropped peer pauses authority.");
            }
            finally
            {
                host.Leave(); client.Leave();
                UnityEngine.Object.Destroy(host.gameObject);UnityEngine.Object.Destroy(client.gameObject);
                config.PeerMode=previousMode;
            }
        }
    }
}
#endif
