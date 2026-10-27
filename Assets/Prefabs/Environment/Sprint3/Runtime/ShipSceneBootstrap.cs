using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Environment.Sprint3
{
    [DefaultExecutionOrder(-1000)]
    public sealed class ShipSceneBootstrap : MonoBehaviour
    {
        [SerializeField] TextAsset manifest;
        [SerializeField] GameObject player;
        [SerializeField] GameObject systems;
        [System.Serializable] sealed class Manifest { public string[] scenes; }
        public bool Ready { get; private set; }
        IEnumerator Start()
        {
            var paths = JsonUtility.FromJson<Manifest>(manifest.text).scenes;
            foreach (var path in paths)
            {
                if (!path.Contains("/Shell/") && !path.Contains("/Dress/")) continue;
                if (!SceneManager.GetSceneByPath(path).isLoaded)
                    yield return SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
            }
            Physics.SyncTransforms();
            player.SetActive(true);
            systems.SetActive(true);
            Ready = true;
            Debug.Log("SHIP_READY: House and all Shell/Dress floors loaded from ShipScenes.json");
        }
    }
}
