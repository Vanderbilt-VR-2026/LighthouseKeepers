using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using LighthouseKeepers.Audio;
using LighthouseKeepers.Player;

namespace LighthouseKeepers.Editor
{
    /// <summary>Targeted prefab authoring. Does not regenerate team scenes or geometry.</summary>
    public static class LighthousePlayerMovementAudio
    {
        const string Root = "Assets/_LighthouseKeepers/";
        public const string PlayerPath = Root + "Prefabs/Player/LK_QuestPlayer.prefab";

        [MenuItem("Lighthouse Keepers/Player/Configure ground detection and footsteps")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.Refresh();
            var clips = Enumerable.Range(1, 4).Select(i =>
            {
                string path = Root + $"Audio/Footsteps/BootStep_{i:00}.wav";
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                if (!importer) throw new InvalidOperationException("Missing footstep: " + path);
                importer.forceToMono = true;
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }).ToArray();
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(Root + "Audio/Mixer/LK_Atmosphere.mixer");
            var group = mixer.FindMatchingGroups("Player").Single(g => g.name == "Player");
            var player = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                Configure(player, clips, group);
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }

            // DevGym has an unpacked player rather than an instance of the shared prefab.
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var gym = EditorSceneManager.OpenScene(LighthouseScenes.ScenePath("LK_DevGym"));
                Configure(UnityEngine.Object.FindAnyObjectByType<XROrigin>().gameObject, clips, group);
                EditorSceneManager.SaveScene(gym);
            }
            finally
            {
                if (setup.Any(s => s.isLoaded) && setup.Any(s => s.isActive))
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("LK_PLAYER_AUDIO: Environment grounding and four footstep clips configured on shared player prefab and DevGym.");
        }

        static void Configure(GameObject player, AudioClip[] clips, AudioMixerGroup group)
        {
            var gravity = player.GetComponentInChildren<GravityProvider>(true);
            if (!gravity) throw new InvalidOperationException("Player must have the XRI Gravity Provider.");
            // The Starter Assets mask (51) omits Environment, so fall force never resets
            // on the tower's floors and ramps and movement keeps using airborne smoothing.
            gravity.sphereCastLayerMask = LayerMask.GetMask("Default", "Environment");
            gravity.sphereCastTriggerInteraction = QueryTriggerInteraction.Ignore;
            if (!player.GetComponent<GroundedSlopeMovement>()) player.AddComponent<GroundedSlopeMovement>();
            var footsteps = player.GetComponent<PlayerFootsteps>();
            if (!footsteps) footsteps = player.AddComponent<PlayerFootsteps>();
            var child = player.transform.Find("Keeper footsteps");
            if (!child)
            {
                child = new GameObject("Keeper footsteps").transform;
                child.SetParent(player.transform, false);
                child.localPosition = new Vector3(0, .08f, 0);
            }
            var source = child.GetComponent<AudioSource>();
            if (!source) source = child.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0; // Local movement feedback stays centered for either ear.
            source.volume = 1;
            source.dopplerLevel = 0;
            source.outputAudioMixerGroup = group;
            var serialized = new SerializedObject(footsteps);
            serialized.FindProperty("head").objectReferenceValue = player.GetComponent<XROrigin>().Camera.transform;
            serialized.FindProperty("gravity").objectReferenceValue = gravity;
            serialized.FindProperty("source").objectReferenceValue = source;
            var array = serialized.FindProperty("clips");
            array.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
