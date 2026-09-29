using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Lobby;

namespace LighthouseKeepers.Tests
{
    public sealed class LobbyPolishTests
    {
        [Test]
        public void HeadAboveMaximumFloodNeverHasSubmersionCountdown()
        {
            var go = new GameObject();
            try
            {
                var flood = go.AddComponent<FloodController>();
                flood.RiseSpeed = 1;
                flood.ResumeFlood();
                Assert.IsTrue(float.IsPositiveInfinity(flood.TimeToHeight(flood.MaximumHeight + 0.1f)));
                flood.SetHeight(flood.MaximumHeight);
                Assert.IsTrue(float.IsPositiveInfinity(flood.TimeToHeight(flood.MaximumHeight + 0.1f)));
                Assert.AreEqual(0, flood.TimeToHeight(flood.MaximumHeight));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void MinimumCrewCannotExceedCapacity()
        {
            var config = ScriptableObject.CreateInstance<LobbyConfig>();
            try
            {
                var data = new SerializedObject(config);
                data.FindProperty("maxPlayers").intValue = 2;
                data.FindProperty("minPlayersToStart").intValue = 8;
                data.ApplyModifiedPropertiesWithoutUndo();
                Assert.AreEqual(2, config.MinPlayersToStart);
            }
            finally { Object.DestroyImmediate(config); }
        }

        [Test]
        public void ExplicitTransportTakesPrecedenceAndUnsubscribesOnDestroy()
        {
            var go = new GameObject();
            var external = new GameObject();
            go.SetActive(false);
            try
            {
                go.AddComponent<LocalLobbyTransport>();
                var preferred = external.AddComponent<LocalLobbyTransport>();
                var manager = go.AddComponent<LobbyManager>();
                var data = new SerializedObject(manager);
                data.FindProperty("transportBehaviour").objectReferenceValue = preferred;
                data.ApplyModifiedPropertiesWithoutUndo();
                go.SetActive(true);
                // EditMode does not run the normal player lifecycle.
                TestLifecycle.Invoke(manager, "Awake");
                Assert.AreSame(preferred, manager.Transport);
                int notifications = 0;
                manager.OnRosterChanged.AddListener(() => notifications++);
                manager.Host();
                Assert.Greater(notifications, 0);
                TestLifecycle.Invoke(manager, "OnDestroy");
                Object.DestroyImmediate(manager);
                int before = notifications;
                preferred.SetLocalReady(true);
                Assert.AreEqual(before, notifications);
            }
            finally { Object.DestroyImmediate(go); Object.DestroyImmediate(external); }
        }

        [Test]
        public void ReadyAndLeaveHintsFollowLocalPracticeState()
        {
            var go = new GameObject();
            try
            {
                var manager = go.AddComponent<LobbyManager>();
                TestLifecycle.Invoke(manager, "Awake");
                Assert.IsTrue(manager.IsLocalPreview);
                manager.Host();
                Assert.IsFalse(manager.CanStart);
                StringAssert.Contains("ready", manager.StartHint);
                manager.ToggleReady();
                Assert.IsTrue(manager.CanStart);
                manager.Leave();
                Assert.IsFalse(manager.CanStart);
                Assert.IsEmpty(manager.Players);
                manager.Join("local");
                StringAssert.Contains("Guest preview", manager.StartHint);
                Assert.IsFalse(manager.CanStart);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
