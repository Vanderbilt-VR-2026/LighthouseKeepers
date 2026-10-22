using System;
using System.Collections;
using System.Linq;
using LighthouseKeepers.Gameplay.Interaction;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
namespace LighthouseKeepers.Net
{
    public sealed class ShipWorldLoader : MonoBehaviour
    {
        public static ShipWorldLoader Active { get; private set; }
        public static bool Solo { get; private set; }
        public bool Ready { get; private set; }
        public string LastError { get; private set; }
        bool loading;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset(){Active=null;Solo=false;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Active)return;
            var root=new GameObject("Ship session / canonical scene set");
            Active=root.AddComponent<ShipWorldLoader>();DontDestroyOnLoad(root);
            Active.Arrived(SceneManager.GetActiveScene(),LoadSceneMode.Single);
        }
        public static void RequestSolo(){Solo=true;}
        public static void RequestCrew(){Solo=false;}
        void OnEnable()=>SceneManager.sceneLoaded+=Arrived;
        void OnDisable()=>SceneManager.sceneLoaded-=Arrived;
        void Arrived(Scene scene,LoadSceneMode mode)
        {
            // House is the same entry in both modes; only the authority policy differs.
            if(scene.name=="LK_Shell_House" && !loading)StartCoroutine(LoadWorld());
            else if(scene.name=="LK_Lobby_Environment" || scene.name=="LK_NetworkLobby")
            {
                Ready=false;
                foreach(var origin in FindObjectsByType<XROrigin>(FindObjectsSortMode.None))StripVignette(origin.gameObject);
            }
        }
        IEnumerator LoadWorld()
        {
            loading=true;Ready=false;LastError="";
            ShipRouteSettings settings;
            try{settings=ShipRouteSettings.Load();_ = settings.Plan;}
            catch(Exception e){Fail(e.Message);yield break;}
            foreach(string path in settings.Plan.additiveScenes)
            {
                if(SceneManager.GetSceneByPath(path).isLoaded)continue;
                AsyncOperation load=null;
                try{load=SceneManager.LoadSceneAsync(path,LoadSceneMode.Additive);}
                catch(Exception e){Fail("Cannot load "+path+": "+e.Message);}
                if(load==null)yield break;
                yield return load;
            }
            Physics.SyncTransforms();LightProbes.Tetrahedralize();
            var house=SceneManager.GetSceneByPath(settings.Plan.entryScene);SceneManager.SetActiveScene(house);
            var rig=FindAnyObjectByType<XROrigin>();
            if(!rig)
            {
                if(!settings.localRig){Fail("ShipRouteSettings has no authored local XR rig.");yield break;}
                var hold=new GameObject("Local keeper rig");hold.SetActive(false);SceneManager.MoveGameObjectToScene(hold,house);
                var instance=Instantiate(settings.localRig,hold.transform,false);
                instance.transform.SetPositionAndRotation(settings.houseSpawn,Quaternion.Euler(0,settings.houseSpawnYaw,0));
                hold.SetActive(true);rig=instance.GetComponent<XROrigin>();
            }
            if(!rig || !rig.Camera){Fail("Canonical House needs exactly one tracked XR Origin and camera.");yield break;}
            StripVignette(rig.gameObject);
            var identity=rig.GetComponent<ActorIdentity>() ?? rig.gameObject.AddComponent<ActorIdentity>();
            identity.Bind(Solo?0:FusionMessageChannel.Active?FusionMessageChannel.Active.LocalActorId:0);
            yield return null; // A's scene-loaded bootstrap installs its physical hands.
            ShipGameplaySession gameplay=null;
            try{gameplay=new GameObject("Ship gameplay / one state authority").AddComponent<ShipGameplaySession>();gameplay.Configure(settings,rig,Solo);}
            catch(Exception e){if(gameplay)Destroy(gameplay.gameObject);Fail(e.Message);yield break;}
            Ready=true;loading=false;
            Debug.Log("LK_SHIP_READY: Lobby -> Play Solo/crew -> "+string.Join(", ",new[]{settings.Plan.entryScene}.Concat(settings.Plan.additiveScenes)));
        }
        void Fail(string message){LastError=message;loading=false;Debug.LogError("SHIP LOAD FAILED: "+message,this);}
        public static void StripVignette(GameObject root)
        {
            foreach(var c in root.GetComponentsInChildren<Behaviour>(true))
                if(c && c.GetType().Name=="TunnelingVignetteController")
                {c.enabled=false;c.gameObject.SetActive(false);Debug.Log("Removed comfort vignette: "+c.transform.name+" under "+root.name);}
        }
    }
}
