using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LighthouseKeepers.Audio;
using LighthouseKeepers.Player;

namespace LighthouseKeepers.Tests
{
    public sealed class PlayerFootstepAssetTests
    {
        const string Root = "Assets/_LighthouseKeepers/";

        [Test]
        public void SharedPlayerHasGroundDetectionAndPlayerMixerFootsteps()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Player/LK_QuestPlayer.prefab");
            CheckPlayer(prefab.GetComponent<PlayerFootsteps>());
        }

        [Test]
        public void DevGymHasTheSameGroundDetectionAndFootsteps()
        {
            EditorSceneManager.OpenScene(Root + "Scenes/Development/LK_DevGym.unity");
            CheckPlayer(Object.FindAnyObjectByType<PlayerFootsteps>());
        }

        static void CheckPlayer(PlayerFootsteps footsteps)
        {
            Assert.IsNotNull(footsteps);
            Assert.IsNotNull(footsteps.GetComponent<GroundedSlopeMovement>());
            var serialized = new SerializedObject(footsteps);
            Assert.IsNotNull(serialized.FindProperty("head").objectReferenceValue);
            var gravity = new SerializedObject(serialized.FindProperty("gravity").objectReferenceValue);
            Assert.AreEqual(LayerMask.GetMask("Default", "Environment"), gravity.FindProperty("m_SphereCastLayerMask").intValue);
            var source = (AudioSource)serialized.FindProperty("source").objectReferenceValue;
            Assert.AreEqual("Player", source.outputAudioMixerGroup.name);
            Assert.IsFalse(source.playOnAwake);
            Assert.IsFalse(source.loop);
            Assert.AreEqual(0, source.dopplerLevel);
            var clips = serialized.FindProperty("clips");
            Assert.AreEqual(4, clips.arraySize);
            for (int i = 0; i < clips.arraySize; i++)
            {
                var clip = (AudioClip)clips.GetArrayElementAtIndex(i).objectReferenceValue;
                Assert.IsNotNull(clip);
                Assert.AreEqual(1, clip.channels);
                Assert.That(clip.length, Is.InRange(.15f, .3f));
                Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, clip.loadType);
                clip.LoadAudioData();
                var data = new float[clip.samples];
                Assert.IsTrue(clip.GetData(data, 0));
                float peak = data.Max(x => Mathf.Abs(x));
                Assert.That(peak, Is.InRange(.1f, .95f), "Silent or clipped footstep.");
            }
        }
    }
}
