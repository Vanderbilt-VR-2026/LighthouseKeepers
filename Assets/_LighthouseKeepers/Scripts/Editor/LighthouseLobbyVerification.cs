using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;

namespace LighthouseKeepers.Editor
{
    /// <summary>Repeatable rendered lobby evidence plus the actual lobby-to-environment scene transition.</summary>
    [InitializeOnLoad]
    public static class LighthouseLobbyVerification
    {
        const string ActiveKey = "LKLobbyVerification";
        const string Output = "Docs/Verification/Lobby";
        static double began;
        static int phase, frame, errors;
        static RenderTexture target;
        static Camera camera;
        static LobbyScreen screen;
        static LobbyManager manager;
        static string oldStartScene;

        static LighthouseLobbyVerification() => EditorApplication.playModeStateChanged += StateChanged;

        public static void RunBatch()
        {
            LighthouseLobbyPreview.CreateScene();
            EditorSceneManager.OpenScene(LighthouseLobbyPreview.ScenePath);
            oldStartScene = AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene);
            SessionState.SetString("LKLobbyOldStartScene", oldStartScene);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(LighthouseLobbyPreview.ScenePath);
            SessionState.SetBool(ActiveKey, true);
            // Let Unity's startup search-index callback finish before the Play Mode reload.
            EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }
        static void StateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                began = EditorApplication.timeSinceStartup;
                phase = frame = errors = 0;
                Application.logMessageReceived += Log;
                EditorApplication.update += Tick;
            }
        }
        static void Log(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
        }
        static void Click(string name)
        {
            var button = screen.GetComponentsInChildren<Button>(true).Single(b => b.name == name);
            if (!button.interactable) throw new Exception(name + " is unexpectedly disabled.");
            if (!button.gameObject.activeInHierarchy) throw new Exception(name + " is hidden.");
            Canvas.ForceUpdateCanvases();
            camera.Render(); // Newly revealed controls must have registered CanvasRenderer depths before raycasting.
            var pointer = new PointerEventData(EventSystem.current) { position = camera.WorldToScreenPoint(button.transform.position), button = PointerEventData.InputButton.Left };
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            if (hits.Count == 0 || hits[0].gameObject != button.gameObject) throw new Exception("UI raycast did not reach " + name + " as its first hit.");
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            if (button) ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
        }
        static void Tick()
        {
            if (EditorApplication.timeSinceStartup - began > 180) { Finish(false, "Timed out."); return; }
            if (Time.frameCount - frame < 30) return;
            frame = Time.frameCount;
            try
            {
                if (phase == 0)
                {
                    screen = UnityEngine.Object.FindAnyObjectByType<LobbyScreen>();
                    manager = UnityEngine.Object.FindAnyObjectByType<LobbyManager>();
                    camera = Camera.main;
                    if (!screen || !manager || !camera) throw new Exception("Missing lobby objects.");
                    if (!screen.GetComponentsInChildren<TMP_Text>().Single(t => t.name == "Mode").text.StartsWith("LOCAL PRACTICE"))
                        throw new Exception("Initial UI does not reflect the local transport.");
                    foreach (var button in screen.GetComponentsInChildren<Button>(true))
                        if (string.IsNullOrWhiteSpace(button.GetComponentInChildren<TMP_Text>().text))
                            throw new Exception("Missing label on " + button.name);
                    target = new RenderTexture(1600, 1000, 24);
                    camera.targetTexture = target;
                    var canvas = screen.GetComponentInChildren<Canvas>();
                    if (canvas.renderMode != RenderMode.WorldSpace) throw new Exception("Lobby must stay in world space, including desktop preview.");
                    if (canvas.worldCamera != camera) throw new Exception("Lobby camera is not bound.");
                    var position = canvas.transform.position;
                    var rotation = canvas.transform.rotation;
                    var headRotation = camera.transform.rotation;
                    camera.transform.Rotate(0, 20, 0, Space.World);
                    if (canvas.transform.position != position || canvas.transform.rotation != rotation) throw new Exception("Board followed the head.");
                    camera.transform.rotation = headRotation;
                    Canvas.ForceUpdateCanvases();
                    phase++;
                }
                else if (phase == 1)
                {
                    Capture("01-offline");
                    Click("OpenPractice");
                    if (manager.CanStart) throw new Exception("Unready crew can start.");
                    phase++;
                }
                else if (phase == 2)
                {
                    Capture("02-not-ready");
                    Click("Ready");
                    if (!manager.CanStart) throw new Exception("Ready solo practice cannot start.");
                    phase++;
                }
                else if (phase == 3)
                {
                    Capture("03-ready");
                    Click("Ready");
                    if (manager.CanStart) throw new Exception("Unready toggle did not revoke start.");
                    Click("Leave");
                    Click("CrewOptions");
                    Capture("07-crew-options");
                    Click("JoinCrew");
                    if (!manager.IsOnline || manager.IsHost || manager.CanStart) throw new Exception("Guest preview rules failed.");
                    phase++;
                }
                else if (phase == 4)
                {
                    Capture("04-guest-preview");
                    Click("Leave");
                    Click("OpenPractice");
                    Click("Ready");
                    phase++;
                }
                else if (phase == 5)
                {
                    var headPosition = camera.transform.position;
                    var boardPosition = screen.transform.position;
                    Click("BoardHeight");
                    if (!screen.IsLowered || Vector3.Distance(screen.transform.position, boardPosition + Vector3.down * .25f) > .001f)
                        throw new Exception("Seated height did not move the board by 25 cm.");
                    if (camera.transform.position != headPosition) throw new Exception("Board adjustment moved the head.");
                    camera.transform.position = new Vector3(headPosition.x, 1.2f, headPosition.z);
                    Capture("05-seated");
                    Click("BoardHeight");
                    if (screen.IsLowered || Vector3.Distance(screen.transform.position, boardPosition) > .001f)
                        throw new Exception("Board did not return to standing height.");
                    // Keep the capture in front of the body proxy while widening the room view.
                    camera.transform.position = new Vector3(.7f, 1.85f, -1.25f);
                    var oldRotation = camera.transform.rotation;
                    var oldFieldOfView = camera.fieldOfView;
                    camera.fieldOfView = 80;
                    camera.transform.LookAt(new Vector3(-.15f,1.5f,1.5f));
                    Capture("06-watch-room");
                    camera.fieldOfView = oldFieldOfView;
                    camera.transform.SetPositionAndRotation(headPosition,oldRotation);
                    Click("Leave");
                    var fullCrew = ScriptableObject.CreateInstance<LobbyConfig>();
                    var configData = new SerializedObject(fullCrew);
                    configData.FindProperty("maxPlayers").intValue = 8;
                    configData.ApplyModifiedPropertiesWithoutUndo();
                    var managerData = new SerializedObject(manager);
                    managerData.FindProperty("config").objectReferenceValue = fullCrew;
                    managerData.ApplyModifiedPropertiesWithoutUndo();
                    Click("OpenPractice");
                    var local = (LocalLobbyTransport)manager.Transport;
                    for (int i = 2; i <= 8; i++) local.AddSimulatedPlayer("Keeper " + i);
                    screen.Refresh();
                    phase++;
                }
                else if (phase == 6)
                {
                    Capture("08-full-crew");
                    if (screen.GetComponentsInChildren<TMP_Text>().Count(t => t.name.StartsWith("CrewSlot")) != 8)
                        throw new Exception("Full crew rows are missing.");
                    foreach (var label in screen.GetComponentsInChildren<TMP_Text>())
                        if (label.isActiveAndEnabled && label.isTextTruncated) throw new Exception("Clipped label: " + label.name);
                    Click("Leave");
                    Click("OpenPractice");
                    Click("Ready");
                    camera.targetTexture = null;
                    target.Release();
                    UnityEngine.Object.Destroy(target);
                    Click("Start");
                    phase++;
                }
                else
                {
                    var bootstrap = UnityEngine.Object.FindAnyObjectByType<EnvironmentBootstrap>();
                    if (!bootstrap || !bootstrap.Ready) return;
                    if (SceneManager.sceneCount != 7) throw new Exception("Expected seven environment scenes.");
                    if (UnityEngine.Object.FindAnyObjectByType<LobbyScreen>()) throw new Exception("Lobby leaked into environment.");
                    Finish(errors == 0, "Rendered eight spatial lobby views including full crew; world-space canvas and seated height checked; UI raycasts and pointer events exercised host, ready/unready, leave, guest preview, board height and start; environment ready with seven scenes. Runtime errors: " + errors);
                }
            }
            catch (Exception e) { Finish(false, e.ToString()); }
        }
        static void Capture(string name)
        {
            Directory.CreateDirectory(Output);
            // Render the actual room-anchored canvas through the player camera, with no overlay substitution.
            foreach (var text in screen.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(Output + "/" + name + ".png", image.EncodeToPNG());
            UnityEngine.Object.Destroy(image);
            RenderTexture.active = previous;
        }
        static void Finish(bool success, string message)
        {
            SessionState.SetBool(ActiveKey, false);
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= Log;
            var previous = SessionState.GetString("LKLobbyOldStartScene", "");
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "/PlayMode.txt", (success ? "PASS: " : "FAIL: ") + message + "\n");
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
