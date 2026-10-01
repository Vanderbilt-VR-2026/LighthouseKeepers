using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionLauncher : MonoBehaviour
{
    private NetworkRunner _runner;

    public async void StartHost()
    {
        await StartSession(GameMode.Host);
    }

    public async void JoinSession()
    {
        await StartSession(GameMode.Client);
    }

    private async System.Threading.Tasks.Task StartSession(GameMode mode)
    {
        // A NetworkRunner should only be used for one session,
        // so create a fresh one each time.
        GameObject runnerObject = new GameObject("Fusion Network Runner");

        _runner = runnerObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        NetworkSceneManagerDefault sceneManager =
            runnerObject.AddComponent<NetworkSceneManagerDefault>();

        SceneRef sceneRef =
            SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();

        if (sceneRef.IsValid)
        {
            sceneInfo.AddSceneRef(
                sceneRef,
                LoadSceneMode.Single
            );
        }

        StartGameResult result = await _runner.StartGame(
            new StartGameArgs
            {
                GameMode = mode,
                SessionName = "LighthouseKeepersRoom",
                PlayerCount = 4,
                Scene = sceneInfo,
                SceneManager = sceneManager
            }
        );

        if (result.Ok)
        {
            Debug.Log(
                $"Fusion connected successfully as {mode}. " +
                $"Room: {_runner.SessionInfo.Name}"
            );
        }
        else
        {
            Debug.LogError(
                $"Fusion failed to start: {result.ShutdownReason}"
            );

            Destroy(runnerObject);
            _runner = null;
        }
    }
}